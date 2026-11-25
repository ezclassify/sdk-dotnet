# EzClassify.Rest.Client

`EzClassify.Rest.Client` is the .NET client for the EzClassify REST API. Use it to parse and validate security markings and sensitivity labels, roll portion marks up into an overall marking, and check need-to-know access, from any .NET application. Markings follow a scheme: IC Classification and Control Markings (CAPCO), Controlled Unclassified Information (CUI), Traffic Light Protocol (TLP) 2.0, Common Vulnerability Scoring System (CVSS), or rules you write yourself. The client needs a running EzClassify runtime; it does not contain the engine or any scheme content. The free developer runtime includes TLP 2.0, CVSS and demo subsets of CAPCO and CUI. The full CAPCO and CUI schemes are licensed content. The one package holds the client, the wire models and the response parsers, and uses no third-party HTTP stack.

## Install

```
dotnet add package EzClassify.Rest.Client
```

## Quick start

Start the free developer runtime. It is one container that serves the built-in Contexts, such as `capco-demo` and `cedar-valley`, over real HTTP, with no license, credentials or account.

```sh
docker run --rm -p 3000:8080 ghcr.io/ezclassify/developer-runtime
```

Then connect with the normal client. The Context is `cedar-valley`, version `1.0.0`, and the runtime asks for no credentials.

```csharp
using EzClassify;

using var ctx = await EzClassifyClient.ConnectAsync(
    "http://localhost:3000", "cedar-valley", "1.0.0");

var result = (await ctx.Scheme!.ProcessAsync("INTERNAL")).Result;
Console.WriteLine($"{result.Rendered} valid={result.Valid}");

var decision = (await ctx.Access!.CheckByMarkingAsync("CONFIDENTIAL", "{}")).Result;
Console.WriteLine($"granted={decision.Granted}");
```

```text
INTERNAL valid=True
granted=False
```

`cedar-valley` is a fictional school district policy for teaching, so use synthetic markings with it. `GET /v1/ctx` on the runtime lists every built-in Context.

### Validate, roll up and check access

The `capco-demo` Context on the developer runtime is a demo subset of CAPCO. It validates a marking, rolls several markings up into one overall marking, and checks access for a user. The guides and access rules in it are notional demo content.

```csharp
using EzClassify;

using var ctx = await EzClassifyClient.ConnectAsync(
    "http://localhost:3000", "capco-demo", "1.0.0");

var read = (await ctx.Scheme!.ValidateAsync("SECRET//NOFORN")).Result;
Console.WriteLine($"{read.Rendered} valid={read.Valid}");

var overall = (await ctx.Scheme.AggregateAsync(new[]
{
    "SECRET//REL TO USA, GBR, CAN",
    "CONFIDENTIAL//REL TO USA, GBR",
    "UNCLASSIFIED",
})).Result;
Console.WriteLine(overall.Rendered);

var decision = (await ctx.Access!.CheckByMarkingAsync(
    "SECRET//NOFORN", """{"clearance":"SECRET","nationality":"GBR"}""")).Result;
Console.WriteLine($"granted={decision.Granted}");
foreach (var rule in decision.Rules)
    if (!rule.Skipped) Console.WriteLine($"  {rule.Name}: {rule.Granted}");
```

```text
SECRET//NOFORN valid=True
SECRET//REL TO USA, GBR
granted=False
  Clearance level: True
  NOFORN: USA nationality claim required (demo): False
```

A SECRET-cleared user with a GBR nationality claim is denied a NOFORN document.

## Swap to your runtime

Change the base URL and supply the tokens your runtime administrator issued. The calls stay the same.

```csharp
using EzClassify;

// Supplied by the runtime administrator; keep tokens out of source control.
var credentials = new RestClientCredentials(
    Environment.GetEnvironmentVariable("EZC_ACCESS_TOKEN")
        ?? throw new InvalidOperationException("Set EZC_ACCESS_TOKEN."),
    Environment.GetEnvironmentVariable("EZC_CONTEXT_TOKEN")
        ?? throw new InvalidOperationException("Set EZC_CONTEXT_TOKEN."));
var baseUrl = Environment.GetEnvironmentVariable("EZC_API_URL")
    ?? throw new InvalidOperationException("Set EZC_API_URL.");

// Root client: health and Context discovery.
using var root = new EzClassifyClient(baseUrl, credentials);
var ping = await root.PingAsync();

// Bound client: operations run against one immutable Context version.
using var ctx = await EzClassifyClient.ConnectAsync(
    baseUrl, "your-context-id", "1.0.0", credentials);
var result = (await ctx.Scheme!.ProcessAsync("your marking")).Result;
Console.WriteLine($"{result.Rendered} {result.Valid}");
```

To name your application in the runtime's usage counts, pass the id it is registered under as the third argument:

```csharp
var credentials = new RestClientCredentials(accessToken, contextToken, applicationId: "case-intake-portal");
```

An id is 1 to 128 printable ASCII characters (0x21 to 0x7E) with no spaces, and the constructor throws `ArgumentException` for any other value. The SDK sends it as the `x-ezclassify-application-id` header on Context requests only, never on health. The id is not a credential and authorizes nothing: a request without it, or with an id the deployment has not registered, is served the same way.

Failures surface as `EzClassifyClientException` with the HTTP status. When the server rate limits a request (429, which any operation can answer), `RetryAfter` holds the wait it asks for, read from the `Retry-After` header or the body's `retryAfter`. The client never retries by itself.

Every call finishes within the request timeout, the `HttpClient.Timeout` of the transport (100 seconds unless you supply an `HttpClient` set otherwise), including the full response body. An `HttpClient` with an infinite timeout, a common setup when Polly owns per-try timeouts, gets the 100 seconds too, and a resilience pipeline that gives up sooner still ends the call then. A call past the deadline throws `EzClassifyClientException` with a `TimeoutException` inside; canceling your own token throws `OperationCanceledException`.

Repeated scheme reads and access-rule lists share a bounded response cache on the bound client. Identical concurrent calls share one request, and canceling one caller's token leaves the others running. Failed calls and malformed 200 responses are never cached. A 200 that lacks a member the contract requires, at any depth, throws `JsonException` whose `Path` names the member (such as `$.result.assessment`), and the next call asks the server again. A concrete Context version keeps answers until eviction; an omitted version or `latest` bounds their lifetime to at most an hour and clears them when the resolved version changes. A 401 or the denial-shaped 404 `{"error":"not_found"}` clears them too. Use `ctx.ClearCache()` for a fresh read or pass `new ContextCacheOptions { Enabled = false }` when you connect to disable caching for that client. Access checks, crosswalk translation and document decisions call the server every time, so each decision reaches the audit record. Cache options are fixed when the client connects. Use one client instance for one caller identity.

`await ctx.DescribeContextAsync()` returns the Context's composition: the scheme, every scheme it holds, the guides, the access policy and the crosswalks, each with its declared and resolved version. It is read once and held, and `ClearCache()` does not drop it.

## License

Apache-2.0
