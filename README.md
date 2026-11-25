# <img src="assets/icon.svg" alt="" width="32"> EzClassify .NET SDK

[![CI](https://github.com/ezclassify/sdk-dotnet/actions/workflows/ci.yml/badge.svg)](https://github.com/ezclassify/sdk-dotnet/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/EzClassify.Rest.Client?label=EzClassify.Rest.Client)](https://www.nuget.org/packages/EzClassify.Rest.Client)
[![license: Apache-2.0](https://img.shields.io/badge/license-Apache--2.0-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-netstandard2.0%20%7C%20net8.0-512BD4.svg)](EzClassify.sln)

This is the .NET client for the EzClassify REST API. Use it to parse and validate security markings and sensitivity labels, roll portion marks up into an overall marking, and check need-to-know access, from any .NET application. Markings follow a scheme: IC Classification and Control Markings (CAPCO), Controlled Unclassified Information (CUI), Traffic Light Protocol (TLP) 2.0, Common Vulnerability Scoring System (CVSS), or rules you write yourself. The client needs a running EzClassify runtime; it does not contain the engine or any scheme content. The free developer runtime includes TLP 2.0, CVSS and demo subsets of CAPCO and CUI. The full CAPCO and CUI schemes are licensed content.

The [ezclassify/contracts](https://github.com/ezclassify/contracts) repository owns
document and wire types; EzClassify Runtime owns
marking and access evaluation. This client owns HTTP transport, response decoding, caching, and
language-native conveniences. Evaluation operations address a published Context;
health and Context discovery use the deployment endpoint.

## Install

```sh
dotnet add package EzClassify.Rest.Client
```

The package targets `netstandard2.0` and also ships a `net8.0` build. CI builds and tests on .NET 10; the `netstandard2.0` output is what a .NET Framework 4.7.2+ host would load, but no job exercises it there.

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

### Swap to your runtime

Four values change: the base URL, the Context, your application id and the caller token. The calls stay the same. Your runtime administrator issues the credentials: `EZC_ACCESS_TOKEN` (caller identity) and `EZC_CONTEXT_TOKEN` (signed Context credential). Use the Context id your runtime publishes, and omit the version to let the server choose: the token's default, or the newest published.

```csharp
using EzClassify;

// Supplied by the runtime administrator; keep tokens out of source control.
var credentials = new RestClientCredentials(
    Environment.GetEnvironmentVariable("EZC_ACCESS_TOKEN")
        ?? throw new InvalidOperationException("Set EZC_ACCESS_TOKEN."),
    Environment.GetEnvironmentVariable("EZC_CONTEXT_TOKEN")
        ?? throw new InvalidOperationException("Set EZC_CONTEXT_TOKEN."),
    applicationId: "your-application-id");
var baseUrl = Environment.GetEnvironmentVariable("EZC_API_URL")
    ?? throw new InvalidOperationException("Set EZC_API_URL.");

// Root client: health and Context discovery.
using var root = new EzClassifyClient(baseUrl, credentials);
Console.WriteLine($"server: {(await root.PingAsync()).EngineVersion}");
var contexts = await root.ListContextsAsync();

// Bound client: the Context composes scheme, guides, access, and crosswalks.
using var ctx = await EzClassifyClient.ConnectAsync(
    baseUrl, "your-context-id", version: null, credentials);
var result = (await ctx.Scheme!.ProcessAsync("your marking")).Result;
Console.WriteLine($"{result.Rendered} {result.Valid}");
Console.WriteLine($"resolved: {ctx.ResolvedVersion}");
```

The `applicationId` above names your application in the runtime's usage counts; it is the id the application is registered under, passed as the third argument:

```csharp
var credentials = new RestClientCredentials(accessToken, contextToken, applicationId: "case-intake-portal");
```

An id is 1 to 128 printable ASCII characters (0x21 to 0x7E) with no spaces, and the constructor throws `ArgumentException` for any other value. The SDK sends it as the `x-ezclassify-application-id` header on Context requests only, never on health. The id is not a credential and authorizes nothing: a request without it, or with an id the deployment has not registered, is served the same way. Keep one client and its caches per caller; your application owns user sign-in and token refresh.

Pass a concrete version to pin one, or omit the version to let the server choose: the token's default, or the newest published. An omitted version binds the same way as `latest`, and the concrete immutable version the server resolved is on `ResolvedVersion`. The bound groups are `Scheme`, `Guide`, `Access`, `Crosswalk`, and `Document`; they are null on the root discovery client, hence the `!`.

The bound client caches repeated scheme `Process`, `Validate`, `Explain`, `Suggest`, `SuggestForCategories`, `Aggregate`, `AggregateAssertions`, `Scan`, `Render`, `DescribeConcept`, `Diff`, and `ApplyPreset` results, plus `Access.Rules` and `Crosswalk.DescribeAsync`. Everything else reaches the server on every call: the guide operations, `CheckCoverage`, `Notices`, `ViewAsync` and the scheme reads (`SummaryAsync`, `ConceptsAsync`, and the rest of the scheme resources), `DescribeRequirements`, crosswalk translation, and document audit and assessment.

Every field of a request is part of its cache key, so two `ProcessAsync` calls that name different `outputs` or `suggestFor` are two entries. The operation is part of the key too: `ExplainAsync` and `ProcessAsync` never share an entry, and neither do `Access.CheckAsync` and `Access.CheckByMarkingAsync`. Identical concurrent calls share one request. Canceling one caller's `CancellationToken` settles only that caller; the request itself is canceled when its last caller has gone, and a cancellation after the answer arrived evicts nothing. A failed call is not cached, and neither is a 200 that is missing a member the contract requires: the next call retries. Results are read-only, so changing the collections in one caller's result cannot change what another caller, or a later cached read, receives.

Each client holds up to 500 results. A concrete Context version keeps results until eviction. An omitted version or `latest` bounds their lifetime instead: five minutes generally, two minutes for scan, and one hour for concept descriptions, access rules and the crosswalk inventory. A 401, the 404 with exactly `{"error":"not_found"}` that a deployment running authentication sends an unauthenticated caller, or a new resolved version on a `latest` binding clears the cache. A 404 that names a missing resource leaves it alone. Call `ctx.ClearCache()` for a fresh read, or pass `new ContextCacheOptions { Enabled = false }` when you connect to disable caching for that client.

`await ctx.DescribeContextAsync()` returns what the Context is made of: the composed scheme, every scheme it holds with its selector and roles, the guides, the access policy, and the crosswalks, each with the version the Context declares and the concrete version that answered. Record these when you need to say which artifacts a decision was made against. The description is read on first use and held for the life of the client, so `ClearCache()` does not drop it; on a `latest` binding it is read again after the composition moves.

Access and document decisions reach the server every time, so each decision has its audit record. Document audit and assessment, access requirements, and crosswalk translation are never cached, and neither are the access checks.

Cache options are fixed when the client connects. Scope each client to one caller identity, including when an HTTP handler supplies credentials from request context.

## API surface

| Operation | API |
|---|---|
| health / Context discovery | root `PingAsync`, `ListContextsAsync`, `ListContextVersionsAsync` |
| validate / process / explain / suggest / aggregate / scan | `Scheme.*` |
| render / diff / coverage / concept describe | `Scheme.*` |
| scheme reads: summary, concepts, categories, formats, scopes, label presets | `Scheme.SummaryAsync`, `ConceptsAsync`, `CategoriesAsync`, `FormatsAsync`, `ScopesAsync`, `LabelPresetsAsync` |
| the same operations and reads on another scheme the Context holds | `Schemes.Get(key).*` |
| access checks / requirements / rule list, and the policy's own marking | `Access.*`, `Access.MarkingAsync` |
| guide scan / evaluate / view | `Guide.*` |
| crosswalk translation, and the crosswalks the Context holds | `Crosswalk.TranslateAsync`, `Crosswalk.DescribeAsync` |
| Context composition and component versions | `DescribeContextAsync` |
| document audit and access assessment | `Document.*` |

Results that describe a marking carry everything cheap to compute alongside it. `ValidateResult`, `ProcessResult` and `AggregateResult` each expose a `Notices` list, always present and empty when the marking carries none, so reading the notices for a marking does not take a second round trip. `Scheme.NoticesAsync` remains for the case where you hold assertions without a result.

Scheme operations that read a marking take their format choices as named options, under the same names the wire uses. `inputFormat` says what the input is read as: one of the scheme's formats, or for a scheme that declares surfaces, a view, a `surface/view` address, or one of a view's forms. Leave it out and recognition works it out, then reports what it read the text as in `Recognition.InputFormat` and, for a scheme that declares surfaces, whether the text is spelled as a whole or a part in `Recognition.Role`. `outputFormat` picks the output the result is written in. A scheme without surfaces defaults to its default format. A scheme that declares surfaces names its outputs `surface/view`, such as `sensitivity/short`, and leaving `outputFormat` out writes each surface in its default for the marking's role, so a part and a whole can come back in different views, unless one default writes every other surface's scope beside its own, which is then written alone. An output you name is always written alone. `surface` names which of the scheme's scopes the input is, for a fragment such as a portion; leave it out and the input is treated as a whole marking. `RenderAsync` and `ApplyPresetAsync` only produce output, so they take `outputFormat` alone.

The defaults are in the scheme's metadata, so a picker does not have to guess them. For a scheme that declares surfaces, `SummaryAsync()` reports `DefaultPartRenderFormat` and `DefaultWholeRenderFormat`, and each entry `FormatsAsync()` returns lists in `DefaultFor` the roles it is the default for and in `Role` the role it is a view of. A scheme without surfaces has one default, `DefaultRenderFormat`, and its formats carry neither.

The rest of each request's optional fields are named options too, under their wire names: `assertions`, `outputs`, and `suggestFor` on `ProcessAsync`; `assertions` on `ExplainAsync`; and `includeGated` on both `SuggestAsync` overloads. Fields that carry a JSON object take it as a JSON string, the way `userAttributesJson` does, so `contextJson` on validate, process, explain, aggregate, and the access checks is forwarded as written. `ValidateAsync`, `ProcessAsync`, and `ExplainAsync` also take `parts`, the part markings the whole must cover: a part the whole does not cover is an error, and every proposed fix and rank floor is worked out against the whole and those parts together. An option you leave null stays off the wire and the server applies its default; `false` is sent.

The scheme reads answer with the view types a local engine's scheme view carries: `ConceptsAsync` returns `ConceptView`, `CategoriesAsync` returns `CategoryView`, `FormatsAsync` returns `FormatView`, `LabelPresetsAsync` returns `LabelPresetView`, and `ScopesAsync` returns `SchemeScope` entries in declaration order, empty when the scheme declares none. They read the scheme of the view they are called on.

Requests are closed. The server answers 400 with code `INVALID_INPUT` to a query parameter or body field the operation does not declare, and `Detail` names it, as in `query: scheme`. The client sends only declared ones.

Errors surface as `EzClassifyClientException` with the HTTP status code when one was received. A 200 whose body the contract does not allow, such as one missing a required member at any depth, throws `JsonException` instead, and its `Path` names the member, such as `$.contexts` or `$.result.assessment`. That holds for every call, health and discovery included. Any operation can answer 429 when the rate limit is spent, and the wait the server asks for is on `RetryAfter`, a `TimeSpan?` read from the `Retry-After` header and, when that is absent, the body's `retryAfter`. It is null when the server named no wait. The client never waits or retries by itself, so how long to back off, and whether to retry at all, is your decision.

```csharp
try
{
    var result = (await ctx.Scheme!.ProcessAsync("INTERNAL")).Result;
}
catch (EzClassifyClientException error) when (error.StatusCode == 429)
{
    await Task.Delay(error.RetryAfter ?? TimeSpan.FromSeconds(1));
    // retry within your own limit
}
```

When you pass a base URL, the client creates and owns its `HttpClient`. On `net8.0` that client recycles pooled connections every two minutes so a long-lived process picks up a DNS change when the rest-api address moves. Pass your own `HttpClient`, or one from `IHttpClientFactory`, and that setting is yours.

Every call finishes within the request timeout, which is the `HttpClient.Timeout` of the transport: 100 seconds on the client it creates for a base URL, and whatever your own `HttpClient` is set to when you hand it over. The client reads it once, at construction. If your `HttpClient` has no timeout (`Timeout.InfiniteTimeSpan`), as in a common `IHttpClientFactory` setup where Polly or another resilience handler owns the per-try timeouts, the whole request still gets the same 100 seconds. The deadline covers the whole request: sending it, waiting for the response, and reading the full body, so a server that sends its headers and then stalls fails on time, and so does a handler that ignores cancellation. A call that runs past it throws `EzClassifyClientException` with a `TimeoutException` as its `InnerException`. Your own `CancellationToken` still wins when it fires first, and that surfaces as `OperationCanceledException`. A handler that gives up sooner wins the same way. When a resilience pipeline stops trying before the deadline, the call ends then, with the handler's own exception (Polly's `TimeoutRejectedException`, for example) or, if the handler lets its cancellation through, an `EzClassifyClientException` that says the transport canceled the request.

### Marking in another scheme the Context holds

A Context with crosswalks holds more than one scheme. `Scheme` works in the primary one. `Schemes.Get(key)` returns the same operations and reads bound to another, by the key `DescribeContextAsync` lists for it as its selector:

```csharp
var description = await ctx.DescribeContextAsync();
var other = description.Schemes[1].Selector;

var result = await ctx.Schemes!.Get(other).ProcessAsync(markingText);
```

Every call on the view sends the key as `?scheme=`, including the label presets and `ApplyPresetAsync`, so a marking is read and written in that scheme. Its cached results are kept apart from every other scheme's, and the same key returns the same view. A key the Context does not hold is not replaced with the primary: each call fails with the server's 404, code `scheme_not_found`.

## Reading a marking and writing one

Two operations answer about one marking, and they are split by who is asking. The first two examples below run against the developer runtime from the [quick start](#quick-start), and their output is what it answers. The third shows a composer's request.

`ValidateAsync` is the reader's call. Code that displays a marking, gates on it, stores it, or hands its facts to an access check calls it, with the text or with the facts a document states:

```csharp
using var ctx = await EzClassifyClient.ConnectAsync("http://localhost:3000", "cedar-valley", "1.0.0");

var read = (await ctx.Scheme!.ValidateAsync("INTERNAL")).Result;
Console.WriteLine($"{read.Rendered} valid={read.Valid} asWritten={read.InputWasValid}");
foreach (var change in read.AppliedFixes)
    Console.WriteLine($"  {change.Message}");
```

```
INTERNAL valid=True asWritten=True
  Rule "nonpublic-is-internal-by-default": added "CEDAR_VALLEY_DISTRICT" to category "allow_sharing_with"
```

`Valid` says the marking stands once the engine has settled it; `InputWasValid` says it was right as written; `AppliedFixes` lists every change settling made, here the sharing default the policy adds. The client reports both exactly as the server judged them. `ValidateAsync` also takes an `IEnumerable<Assertion>` for a marking given as facts. Pass facts only as a document or an author stated them: the `Assertions` of an earlier result are already settled, and validating those can call a marking valid whose text was not.

`ProcessAsync` is the author's call. It answers everything `ValidateAsync` does, settled the same way, plus what someone writing the marking needs: the corrections to the text in `Recognition.Corrections`, the fix paths the server has proven, a rendering for each output you name, and picker options for the categories you name.

```csharp
var draft = (await ctx.Scheme.ProcessAsync("INTERNAL//FINANCIAL INFORMATION")).Result;
Console.WriteLine($"{draft.Rendered} valid={draft.Valid}");
foreach (var error in draft.Errors)
    Console.WriteLine($"  {error.Message}");
foreach (var fix in draft.ProposedFixes)
    Console.WriteLine($"  fix: {fix.Rendered}");
```

```
INTERNAL//FINANCIAL INFORMATION valid=False
  Financial information is confidential at minimum.
  fix: CONFIDENTIAL//FINANCIAL INFORMATION
  fix: INTERNAL
```

A composer sends what was typed together with the facts already picked, names the outputs it previews, and asks for the options of the pickers it shows, all in one request:

```csharp
var typed = "FINANCIAL INFORMATION";
var pickedSoFar = new[]
{
    new Assertion("level", "CONFIDENTIAL", Confidence.Certain, "user"),
};

var keystroke = (await ctx.Scheme.ProcessAsync(
    typed,
    assertions: pickedSoFar,
    outputs: new[] { "long" },
    suggestFor: new[] { "level" })).Result;

var preview = keystroke.Renders["long"].Rendered;
var options = keystroke.SuggestionsFor!["level"].Suggestions;
```

`Renders` holds only the outputs you name, and `SuggestionsFor` is null unless you name categories. Outputs are spelled the way the scheme declares them: a format id here, a `surface/view` address for a scheme that declares surfaces. `SuggestForCategoriesAsync` answers the same map on its own, for a picker that needs options without processing the marking.

Inputs and results are sealed records. The members a record requires go through its constructor, as `Assertion` is built above, and a reference-typed one refuses null. The optional members are settable properties that start null; set them in an object initializer. A member a later release adds is optional, so code that builds these records keeps compiling. Building and reading them works from C# 7.3, the default for a .NET Framework project.

An error about text the engine could not read may carry `DidYouMean`, the ids of the concepts the text was likely meant as, for a program to act on. The message says the same in words. It is a hint, not a fix: only `ProposedFixes` are proven.

`ExplainAsync` takes what `ProcessAsync` takes, apart from `outputs` and `suggestFor`, and returns the trace instead: what was read, repaired, inferred and found wrong, and what was written. It is for the moment someone asks why.

`SuggestAsync` takes either text or an `IEnumerable<Assertion>`, as separate overloads. An empty string and an empty sequence are valid supplied inputs; null is refused before transport. Only the text overload accepts `inputFormat`. The facts overload sends `assertions` without synthesizing `input` or `inputFormat`. Both forward `outputFormat`, `filter`, `surface`, and `includeGated` unchanged.

`Assertion.Source` is an open string. Labels such as `stated` and private host labels are preserved alongside the values and provenance supplied by the caller. The engine assigns `infer`, `default`, `context`, and `repair` when appropriate; a `sourceRule` records rule derivation. The client forwards these fields without applying local classification policy.

A failed Context binding and a missing concept both preserve the server's HTTP status and full `ResponseBody` in `EzClassifyClientException`. `Code` and `Detail` retain the operation-specific refusal, so a `conceptId: NOPE` detail is not treated as a missing Context. A resource refusal retains cached answers; a 401 or a denial-shaped 404 clears them.

## Versioning

SDK `major.minor` tracks the rest-api `major.minor`; patch versions may drift. `client.EngineSpecVersion` is the spec version this SDK was built against, `await client.PingAsync()` returns the live server version, and the first mismatch writes a one-time notice to `System.Diagnostics.Trace`, which any trace listener can route into your own logging. The shared conformance corpus checks request serialization and response decoding across clients. Acceptance against a live deployment is a separate lane.

## Demo

[`examples/server-tour`](examples/server-tour/README.md) walks the main client calls and runs against the developer runtime from the [quick start](#quick-start) once `EZC_API_URL` is set to `http://localhost:3000`. It requires the .NET 10 SDK. Its README shows the expected output and how to point it at your runtime.

## Integration testing

The [quick start](#quick-start) runtime also evaluates what your application needs to test connectivity, response handling, and access-decision logging.

A denied access decision is a successful response with `granted: false`; handle transport or authentication failures separately. The access example below logs the outcome and rule explanations without copying subject attributes or credentials. In your backend, derive attributes from trusted identity data and add your application's correlation ID and resource reference. Apply your log retention and redaction policy to returned explanations too.

```csharp
using EzClassify;

try
{
    using var ctx = await EzClassifyClient.ConnectAsync(
        "http://localhost:3000", "cedar-valley", "1.0.0");
    var decision = (await ctx.Access!.CheckByMarkingAsync("CONFIDENTIAL", "{}")).Result;
    Console.WriteLine($"granted={decision.Granted}");
    foreach (var rule in decision.Rules)
        Console.WriteLine($"{rule.Rule.RuleId} {rule.Granted} {rule.Reason}");
}
catch (EzClassifyClientException error)
{
    Console.Error.WriteLine($"Integration request failed: {error.StatusCode} {error.Message}");
}
```

To rehearse transport failures such as a 429, a dropped connection or a malformed body, run the Apache-2.0 [conformance server](https://github.com/ezclassify/contracts/tree/main/packages/conformance-server#testing-an-application-integration) (Node.js 22 or 24). It replays recorded failures on demand, which the developer runtime does not produce. Clear the client response cache or create a new client between runs so an earlier answer does not hide the scenario.

## Reporting issues

Issues are tracked centrally for the whole EzClassify family. File bugs and feature requests, or ask a question, at [github.com/ezclassify/ezclassify](https://github.com/ezclassify/ezclassify) (Issues and Discussions). For vulnerabilities, follow the [security policy](https://github.com/ezclassify/ezclassify/blob/main/SECURITY.md) instead.

## Links

The [API reference overview](docs/index.md) describes the two shipped libraries
and generated wire models. Their XML documentation also supports IntelliSense.

Website and runtime access: [ezclassify.com](https://ezclassify.com) · Contributing: [CONTRIBUTING.md](CONTRIBUTING.md) · Security: [policy](https://github.com/ezclassify/ezclassify/blob/main/SECURITY.md) · Engineering standards: [STANDARDS.md](./STANDARDS.md)

## License

[Apache License 2.0](LICENSE) © EzClassify, LLC
