using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace EzClassify;

/// <summary>
/// Carries caller authorization (a bearer token and a signed Context token) and
/// the id of the calling application.
/// </summary>
public sealed class RestClientCredentials
{
    private const string ApplicationIdHeader = "x-ezclassify-application-id";
    private const int ApplicationIdMaxLength = 128;
    private const string ApplicationIdRule =
        "An application id is 1 to 128 printable ASCII characters (0x21 to 0x7E), with no spaces";

    /// <summary>Credentials that add no headers.</summary>
    public static RestClientCredentials None { get; } = new RestClientCredentials(null, null);

    /// <summary>Bearer token identifying the caller, or <c>null</c> when no separate caller token is required.</summary>
    public string? AccessToken { get; }

    /// <summary>Signed token authorizing Context operations, or <c>null</c> when not required.</summary>
    public string? ContextToken { get; }

    /// <summary>
    /// The id the calling application is registered under, or <c>null</c>. The runtime counts
    /// requests per application id and reports the usage. It is not a credential and authorizes
    /// nothing: a request without it, or with an id the deployment has not registered, is served
    /// the same way. It is sent only to Context routes.
    /// </summary>
    public string? ApplicationId { get; }

    /// <summary>Create caller authorization without an application id.</summary>
    /// <param name="accessToken">Bearer token identifying the caller, or <c>null</c>.</param>
    /// <param name="contextToken">Signed Context authorization token, or <c>null</c>.</param>
    /// <exception cref="ArgumentException">A token is blank.</exception>
    public RestClientCredentials(string? accessToken, string? contextToken)
        : this(accessToken, contextToken, null)
    {
    }

    /// <summary>Create caller authorization and the application id.</summary>
    /// <param name="accessToken">Bearer token identifying the caller, or <c>null</c>.</param>
    /// <param name="contextToken">Signed Context authorization token, or <c>null</c>.</param>
    /// <param name="applicationId">The id the application is registered under, or <c>null</c>: 1 to 128 printable ASCII characters (0x21 to 0x7E), with no spaces.</param>
    /// <exception cref="ArgumentException">A token is blank, or the application id is outside its grammar.</exception>
    public RestClientCredentials(string? accessToken, string? contextToken, string? applicationId)
    {
        if (applicationId is not null && !IsApplicationId(applicationId))
            throw new ArgumentException(ApplicationIdRule, nameof(applicationId));
        if (accessToken is not null && string.IsNullOrWhiteSpace(accessToken))
            throw new ArgumentException("accessToken must be null or non-blank", nameof(accessToken));
        if (contextToken is not null && string.IsNullOrWhiteSpace(contextToken))
            throw new ArgumentException("contextToken must be null or non-blank", nameof(contextToken));
        AccessToken = accessToken;
        ContextToken = contextToken;
        ApplicationId = applicationId;
    }

    // The grammar the contract's header pattern states. A server counts a
    // value outside it as unidentified, so the client refuses it up front.
    private static bool IsApplicationId(string value)
    {
        if (value.Length == 0 || value.Length > ApplicationIdMaxLength) return false;
        foreach (var character in value)
            if (character < 0x21 || character > 0x7e) return false;
        return true;
    }

    internal void ValidateTransport(HttpClient http)
    {
        if (ApplicationId is null) return;
        if (http.DefaultRequestHeaders.Contains(ApplicationIdHeader))
            throw new ArgumentException("The application id cannot be combined with a default " + ApplicationIdHeader + " header on the transport", "httpClient");
    }

    internal void Apply(HttpRequestMessage request, bool contextPath)
    {
        if (AccessToken is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
        if (contextPath && ContextToken is not null)
            request.Headers.TryAddWithoutValidation("x-ezclassify-context-token", ContextToken);
        if (contextPath && ApplicationId is not null)
            request.Headers.TryAddWithoutValidation(ApplicationIdHeader, ApplicationId);
    }
}
