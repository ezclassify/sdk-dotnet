# Architecture Ownership

## Sources of truth

The [ezclassify/contracts](https://github.com/ezclassify/contracts) repository owns
canonical wire types, schemas, OpenAPI, and conformance fixtures.
EzClassify Runtime owns engine behavior and Context composition. This
repository consumes the bot-managed mirror under [`vendor/spec/`](vendor/spec/);
contributors do not edit that mirror by hand. [`vendor/spec/MANIFEST.json`](vendor/spec/MANIFEST.json)
records the contracts commit, contract hashes, generator inputs, and committed
generated outputs.

The repository follows the conventional .NET layout: the root
[`EzClassify.sln`](EzClassify.sln) contains the one published library,
the xUnit test project, and the [`examples/server-tour/`](examples/server-tour/)
application. [`Directory.Build.props`](Directory.Build.props) owns common compiler and
package metadata. [`Directory.Packages.props`](Directory.Packages.props) is the only place NuGet dependency
versions are declared. The project file keeps only project-specific settings,
references, and packaging content.

## Generated code

[`scripts/gen.mjs`](scripts/gen.mjs) consumes
[`vendor/spec/openapi.yaml`](vendor/spec/openapi.yaml) and [`vendor/spec/json-schemas/`](vendor/spec/json-schemas/).
It owns the two committed outputs named by [`vendor/spec/MANIFEST.json`](vendor/spec/MANIFEST.json):

- [`EzClassify.Rest.Client/Wire/Models.g.cs`](EzClassify.Rest.Client/Wire/Models.g.cs): the schema-driven wire records
- [`EzClassify.Rest.Client/Wire/ResponseShapes.g.cs`](EzClassify.Rest.Client/Wire/ResponseShapes.g.cs): the required-member table the parsers check

Every output carries a generated ownership header. Do not edit an output
directly; change the canonical contract or the generator and
run `make regen`. `make check-generated` is the non-mutating freshness gate.

## Handwritten seams

The SDK owns language-native HTTP transport and connection lifecycle in the
[`EzClassify.Rest.Client/`](EzClassify.Rest.Client/) project directory, and thin Context operation groups over canonical
endpoints. In its [`Wire/`](EzClassify.Rest.Client/Wire/) directory, handwritten code is the Context and document
records, the extensible wire enums, defensive response parsers, and UTF-8 offset helpers.
The parsers are internal; the records, the enums and the offset helpers are the public wire surface.

Parsers refuse a response that lacks any member the contract requires, at any
depth, and tolerate optional members. What is required is not decided by hand:
`gen.mjs` writes a `ResponseShapes` table from the OpenAPI `required` lists into
`ResponseShapes.g.cs`, `ResponseCheck` walks it before a parser reads a
body, and a refusal is a `JsonException` whose `Path` names the missing member.
The client's own reads of the health, discovery and binding-probe bodies run the
same check.
Parsers also preserve opaque engine payloads as `JsonElement`. The contract is open-response and closed-request. Each wire enum
is an extensible struct over its raw token (the Azure SDK extensible-enum
pattern), so parsers keep a token this SDK does not know instead of failing, and
a request sends it back byte for byte. Only the server rejects a request token;
the client refuses nothing but `default`, which carries no token. The strict
`Parse`/`TryParse` on each struct are for tokens a caller types in, and
`WireEnumSpecTests` pins every struct's declared tokens to the vendored
contract. These are adapter responsibilities; they must not become a second
policy implementation.

## Forbidden ownership

Do not implement scheme, guide, access-policy, document-assessment, or engine
evaluation semantics in this repository. Do not recompute Context resolution,
compose Context results locally.
The client sends canonical inputs and decodes canonical outputs.

Do not add handwritten or shadow DTOs for generated wire types, declare a
generated type a second time, hand-edit [`vendor/spec/`](vendor/spec/), or put a
NuGet version in a project file. Wire models and parsers never reach back into
the transport.

## Required gates

- `make check-architecture` rejects shadow wire declarations, multiple compiled
  declaration owners, generated-path drift, and inline package versions.
- `make check-generated` verifies both committed outputs against the pinned
  generator, OpenAPI, and JSON Schemas.
- `dotnet format EzClassify.sln --verify-no-changes --no-restore`
- `dotnet build EzClassify.sln --no-restore --configuration Release`
- `dotnet test EzClassify.sln --no-build --configuration Release --filter "Category!=Live"`
- `dotnet test EzClassify.sln --no-build --configuration Release --filter "FullyQualifiedName~PublicApiSurfaceTests"`
- `dotnet test EzClassify.sln --no-build --configuration Release --filter "FullyQualifiedName~DocumentSemanticConformanceTests"`
- `python3 scripts/gen-notices.py notices --check`

Before compilation CI verifies the OpenAPI and generator hashes the manifest
records; `make check-generated` reruns the pinned generator and fails on any
difference from the committed outputs. A public surface change requires a deliberate
`make api-surface` update; a contract mirror change comes only from
the spec bot.
