using System.Net;
using Xunit;

namespace EzClassify.Tests;

public class ApplicationIdTests
{
    private const string Id = "case-intake-portal";
    private const string Header = "x-ezclassify-application-id";
    private static RestClientCredentials Application => new("caller", null, Id);

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(" leading")]
    [InlineData("trailing ")]
    [InlineData("tab\tvalue")]
    [InlineData("line\r\nvalue")]
    [InlineData("zero\0value")]
    [InlineData("delete\u007fvalue")]
    [InlineData("non-asciié")]
    public void MalformedApplicationId_IsRefusedWithoutTransport(string value)
    {
        var error = Assert.Throws<ArgumentException>(() => new RestClientCredentials(null, null, value));
        Assert.Equal("applicationId", error.ParamName);
    }

    [Fact]
    public async Task ApplicationId_ReachesDiscoveryVersionsBindingAndOperations_ButNotHealth()
    {
        using var handler = new RecordingHandler();
        using var http = Transport(handler);
        using var root = new EzClassifyClient(http, Application);
        await root.PingAsync();
        await root.ListContextsAsync();
        await root.ListContextVersionsAsync("sample");
        using var bound = await EzClassifyClient.ConnectAsync(http, "sample", "1.0.0", Application);
        await bound.DescribeContextAsync();

        Assert.Equal(5, handler.Requests.Count);
        Assert.Null(handler.Requests[0].ApplicationId);
        Assert.All(handler.Requests.Skip(1), request => Assert.Equal(Id, request.ApplicationId));
        Assert.All(handler.Requests, request => Assert.Equal("Bearer caller", request.Authorization));
        Assert.Equal(Id, Application.ApplicationId);
    }

    [Fact]
    public void TheHeaderNameIsTheOneTheContractDeclares()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (!File.Exists(Path.Combine(root.FullName, "EzClassify.Rest.sln"))) root = root.Parent!;
        var spec = File.ReadAllText(Path.Combine(root.FullName, "vendor", "spec", "openapi.yaml"));
        Assert.Contains("    ApplicationId:\n      name: " + Header + "\n      in: header", spec);
    }

    [Fact]
    public async Task WithoutAnApplicationId_NoHeaderIsSent()
    {
        using var handler = new RecordingHandler();
        using var http = Transport(handler);
        using var bound = await EzClassifyClient.ConnectAsync(http, "sample", "1.0.0", RestClientCredentials.None);
        await bound.DescribeContextAsync();

        Assert.NotEmpty(handler.Requests);
        Assert.All(handler.Requests, request => Assert.Null(request.ApplicationId));
    }

    [Fact]
    public async Task OwnedTransport_SendsTheIdToTheConfiguredServer()
    {
        using var handler = new RecordingHandler();
        await using var server = WireLoopbackServer.Start(handler);
        using var bound = await EzClassifyClient.ConnectAsync(server.Url, "sample", "1.0.0", Application);

        Assert.Equal(Id, handler.Requests.Single(request => request.Path.StartsWith("/v1/ctx/sample/1.0.0", StringComparison.Ordinal)).ApplicationId);
    }

    [Fact]
    public async Task ADefaultHeaderOnTheTransport_ConflictsWithTheId()
    {
        using var handler = new RecordingHandler();
        using var http = Transport(handler);
        http.DefaultRequestHeaders.TryAddWithoutValidation(Header, "other");
        var rootError = Assert.Throws<ArgumentException>(() => new EzClassifyClient(http, Application));
        var connectError = await Assert.ThrowsAsync<ArgumentException>(() => EzClassifyClient.ConnectAsync(http, "sample", "1.0.0", Application));
        Assert.Equal("httpClient", rootError.ParamName);
        Assert.Equal("httpClient", connectError.ParamName);
        Assert.Empty(handler.Requests);
    }

    [Theory]
    [InlineData(".")]
    [InlineData("..")]
    public async Task DotSegments_CannotNormalizeAContextRoute(string segment)
    {
        using var handler = new RecordingHandler();
        using var http = Transport(handler);
        using var root = new EzClassifyClient(http, Application);
        await Assert.ThrowsAsync<ArgumentException>(() => root.ListContextVersionsAsync(segment));
        await Assert.ThrowsAsync<ArgumentException>(() => EzClassifyClient.ConnectAsync(http, segment, "1.0.0", Application));
        await Assert.ThrowsAsync<ArgumentException>(() => EzClassifyClient.ConnectAsync(http, "sample", segment, Application));
        Assert.Empty(handler.Requests);
    }

    private static HttpClient Transport(HttpMessageHandler handler) => new(handler, disposeHandler: false)
    {
        BaseAddress = new Uri("http://localhost:8080/"),
    };

    private sealed record Observed(string Path, string? ApplicationId, string? Authorization);

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public List<Observed> Requests { get; } = new();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            static string? Value(HttpRequestMessage message, string name) =>
                message.Headers.TryGetValues(name, out var values) ? string.Join(",", values) : null;
            var path = request.RequestUri?.AbsolutePath ?? "";
            Requests.Add(new Observed(path, Value(request, Header), Value(request, "Authorization")));
            var body = path switch
            {
                "/v1/health" => """{"status":"ok","engineVersion":"1.0.0","engineSha":"abc","gitTag":"v1.0.0","openapiVersion":"1.0.0","abiVersion":"v1"}""",
                "/v1/ctx" => """{"contexts":["sample"]}""",
                "/v1/ctx/sample" => """{"id":"sample","versions":["1.0.0"]}""",
                _ => ContextProbe.Body,
            };
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) });
        }
    }
}
