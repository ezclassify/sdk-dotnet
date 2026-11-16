# Agent Instructions

## Standards

Read [`STANDARDS.md`](./STANDARDS.md) before any feature, refactor, architectural decision, or task > 30 minutes. These engineering standards apply to you as they do to any contributor. Skip for: quick bug fixes, typos, documentation-only changes.

Read [`ARCHITECTURE.md`](./ARCHITECTURE.md) before changing project boundaries,
dependency direction, generated-code ownership, the vendored contract, or the
public wire surface. It is the repository's executable ownership contract.

## Repository Layout

| Path | Purpose |
|------|---------|
| [`EzClassify.Rest.Client/`](EzClassify.Rest.Client/) | HTTP client (`EzClassifyClient`) for the REST API. `Wire/` holds the wire models, generated from the local [`vendor/spec/`](vendor/spec/) contract mirror, and the response parsers |
| [`EzClassify.Rest.Tests/`](EzClassify.Rest.Tests/) | xUnit test suite; run via `make test` |
| [`examples/server-tour/`](examples/server-tour/) | Runnable demo showing `EzClassifyClient` against a live REST API |
| [`EzClassify.Rest.CSharp73/`](EzClassify.Rest.CSharp73/) | C# 7.3 build of every request record; never packed |

## Build and Test

```sh
make build   # dotnet build EzClassify.sln --configuration Release
make test    # dotnet test  EzClassify.sln --configuration Release --filter "Category!=Live"
make check-architecture # declaration ownership and central package versions
make check-generated    # committed generated-source freshness
```

The [`examples/server-tour/`](examples/server-tour/) project is included in the solution; build failures there break `make build` and `make test`.

## Running the Demo

```sh
# Start the free developer runtime, then run the tour against it.
docker run --rm -p 3000:8080 ghcr.io/ezclassify/developer-runtime
# Context "cedar-valley", version 1.0.0, no credentials.
# EZC_CONTEXT_ID and EZC_CONTEXT_VERSION point the tour at another Context.
cd examples/server-tour && EZC_API_URL=http://localhost:3000 dotnet run
```
