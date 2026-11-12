#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace EzClassify;

/// <summary>
/// JSON parsing helpers for the context-bound operation result types. Reuses the
/// shared element parsers on <see cref="ResultParser"/> (assertions, validation
/// errors, warnings, guide rules) where the shapes overlap.
/// </summary>
/// <remarks>
/// Every parser throws <see cref="JsonException"/> for a body that lacks a member
/// the contract requires, at any depth, and names the member in
/// <see cref="JsonException.Path"/>, such as <c>$.result.assessment</c>. A member
/// the contract marks optional may be absent or null.
/// </remarks>
internal static class ContextResultParser
{
    /// <summary>Parse a coverage report from a JSON string.</summary>
    public static CoverageReport ParseCoverageReport(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return ParseCoverageReportElement(doc.RootElement)
            ?? throw new JsonException("Expected a coverage report object");
    }

    /// <summary>Parse a coverage report from a JSON element (null-safe for nested optional reports).</summary>
    public static CoverageReport? ParseCoverageReportElement(JsonElement el)
    {
        if (el.ValueKind == JsonValueKind.Object) ResponseCheck.Require("CheckCoverageResult", el);
        return ReadCoverageReport(el);
    }

    internal static CoverageReport? ReadCoverageReport(JsonElement el)
    {
        if (el.ValueKind != JsonValueKind.Object) return null;

        return new CoverageReport
        {
            Covered = JsonRead.RequiredBool(el, "covered"),
            Missing = JsonRead.RequiredList(el, "missing", ResultParser.ReadAssertion),
            MissingLabels = JsonRead.StrictStringList(el, "missingLabels"),
            Excess = JsonRead.RequiredList(el, "excess", ResultParser.ReadAssertion),
            ExcessLabels = JsonRead.StrictStringList(el, "excessLabels"),
            Rendered = JsonRead.RequiredString(el, "rendered"),
            // Sent only when a rollup refused; absent stays null.
            Refusals = JsonRead.TolerantOptionalList(el, "refusals", ResultParser.ReadValidationError),
        };
    }

    /// <summary>Parse a concept description from a JSON string.</summary>
    public static ConceptDescription ParseConceptDescription(string json)
    {
        using var doc = JsonDocument.Parse(json);
        ResponseCheck.Require("ConceptDescription", doc.RootElement);
        return ParseConceptDescriptionElement(doc.RootElement);
    }

    // Element core: a nested occurrence is read from the element already
    // in hand instead of being re-serialized and re-parsed.
    internal static ConceptDescription ParseConceptDescriptionElement(JsonElement root)
    {
        return new ConceptDescription
        {
            ConceptId = JsonRead.RequiredString(root, "conceptId"),
            CategoryId = JsonRead.RequiredString(root, "categoryId"),
            Name = JsonRead.RequiredString(root, "name"),
            Lines = JsonRead.StrictStringList(root, "lines"),
            Definition = JsonRead.OptionalString(root, "definition"),
            Guidance = JsonRead.OptionalString(root, "guidance"),
            // An absent or null "rules" is null, not an empty list: the concept
            // is referenced by no rule and the contract says so by omission.
            Rules = JsonRead.TolerantOptionalStringList(root, "rules"),
        };
    }

    // Element core: a nested occurrence is read from the element already
    // in hand instead of being re-serialized and re-parsed.
    /// <summary>Parse the list of concept notices from a JSON string.</summary>
    public static IReadOnlyList<Notice> ParseNotices(string json)
    {
        using var doc = JsonDocument.Parse(json);
        ResponseCheck.RequireEach("Notice", doc.RootElement);
        return ParseNoticesElement(doc.RootElement);
    }

    // Element core: a nested occurrence is read from the element already
    // in hand instead of being re-serialized and re-parsed.
    internal static IReadOnlyList<Notice> ParseNoticesElement(JsonElement root)
    {
        JsonRead.RequireArray(root, "notice");
        return JsonRead.ElementList(root, ParseNoticeElement);
    }

    // Single-notice core. The notices operation returns a bare array of these,
    // and every result that describes a marking carries the same objects under
    // its own "notices" property, so both paths decode through one reader.
    internal static Notice ParseNoticeElement(JsonElement el) => new()
    {
        ConceptId = JsonRead.RequiredString(el, "conceptId"),
        ConceptName = JsonRead.RequiredString(el, "conceptName"),
        Type = JsonRead.RequiredString(el, "type"),
        Text = JsonRead.RequiredString(el, "text"),
        Required = JsonRead.RequiredBool(el, "required"),
        Views = JsonRead.TolerantOptionalStringMap(el, "views"),
    };

    /// <summary>Parse a suggest result from a JSON string.</summary>
    public static SuggestResult ParseSuggestResult(string json)
    {
        using var doc = JsonDocument.Parse(json);
        ResponseCheck.Require("SuggestResult", doc.RootElement);
        return ParseSuggestResultElement(doc.RootElement);
    }

    // Element core: a nested occurrence is read from the element already
    // in hand instead of being re-serialized and re-parsed.
    internal static SuggestResult ParseSuggestResultElement(JsonElement root)
    {
        JsonRead.RequireObject(root, "suggest result");
        return new()
        {
            Suggestions = JsonRead.RequiredList(root, "suggestions", ResultParser.ReadSuggestion),
            Warnings = JsonRead.RequiredList(root, "warnings", ResultParser.ReadWarning),
        };
    }

    /// <summary>
    /// Parse suggestions keyed by category id from a JSON string: what
    /// suggest-for-categories answers, and what a process result carries in
    /// <c>suggestionsFor</c>.
    /// </summary>
    public static IReadOnlyDictionary<string, SuggestResult> ParseSuggestResultsByCategory(string json)
    {
        using var doc = JsonDocument.Parse(json);
        ResponseCheck.Require("SuggestResultsByCategory", doc.RootElement);
        return ParseSuggestResultsByCategoryElement(doc.RootElement);
    }

    // An empty map is a real answer, to a request that named no category. Each
    // value must be a suggest result: a null or a non-object under a key would
    // hand the caller a category with nothing behind it.
    internal static IReadOnlyDictionary<string, SuggestResult> ParseSuggestResultsByCategoryElement(JsonElement root)
    {
        JsonRead.RequireObject(root, "suggestions by category");
        var map = new Dictionary<string, SuggestResult>(StringComparer.Ordinal);
        foreach (var entry in root.EnumerateObject())
        {
            if (entry.Value.ValueKind != JsonValueKind.Object)
                throw new JsonException($"Suggestions for category '{entry.Name}' must be an object");
            map[entry.Name] = ParseSuggestResultElement(entry.Value);
        }
        return new ReadOnlyDictionary<string, SuggestResult>(map);
    }

    // Element core: a nested occurrence is read from the element already
    // in hand instead of being re-serialized and re-parsed.
    /// <summary>Parse an access rule description from a JSON string.</summary>
    public static AccessRuleDescription ParseAccessRuleDescription(string json)
    {
        using var doc = JsonDocument.Parse(json);
        ResponseCheck.Require("AccessRuleDescription", doc.RootElement);
        return ReadAccessRuleDescription(doc.RootElement);
    }

    /// <summary>Parse an access rule description from a JSON element.</summary>
    public static AccessRuleDescription ParseAccessRuleDescriptionElement(JsonElement el)
    {
        ResponseCheck.Require("AccessRuleDescription", el);
        return ReadAccessRuleDescription(el);
    }

    internal static AccessRuleDescription ReadAccessRuleDescription(JsonElement el) => new()
    {
        Rule = ReadRuleReference(JsonRead.RequiredProperty(el, "rule")),
        Access = ReadArtifactReference(JsonRead.RequiredProperty(el, "access")),
        Scheme = ReadArtifactReference(JsonRead.RequiredProperty(el, "scheme")),
        Name = JsonRead.RequiredString(el, "name"),
        RequirementText = JsonRead.RequiredString(el, "requirementText"),
        AttributesNeeded = JsonRead.StrictStringList(el, "attributesNeeded"),
        DependsOn = JsonRead.RequiredList(el, "dependsOn", ReadRuleReference),
        Overrides = JsonRead.RequiredList(el, "overrides", ReadRuleReference),
        Description = JsonRead.OptionalString(el, "description"),
        WhenText = JsonRead.OptionalString(el, "whenText"),
    };

    /// <summary>Parse an aggregated access-requirements description from a JSON string.</summary>
    public static AccessRequirementsDescription ParseAccessRequirementsDescription(string json)
    {
        using var doc = JsonDocument.Parse(json);
        ResponseCheck.Require("AccessRequirementsDescription", doc.RootElement);
        return ParseAccessRequirementsDescriptionElement(doc.RootElement);
    }

    // Element core: a nested occurrence is read from the element already
    // in hand instead of being re-serialized and re-parsed.
    internal static AccessRequirementsDescription ParseAccessRequirementsDescriptionElement(JsonElement root)
    {
        return new AccessRequirementsDescription
        {
            Assertions = JsonRead.RequiredList(root, "assertions", ResultParser.ReadAssertion),
            AssertionDisplays = JsonRead.StrictStringList(root, "assertionDisplays"),
            Access = ReadArtifactReference(JsonRead.RequiredProperty(root, "access")),
            Scheme = ReadArtifactReference(JsonRead.RequiredProperty(root, "scheme")),
            AppliedRules = JsonRead.RequiredList(root, "appliedRules", ReadRuleReference),
            Rules = JsonRead.RequiredList(root, "rules", ReadAccessRuleDescription),
            CombinedRequirementText = JsonRead.RequiredString(root, "combinedRequirementText"),
            CombinedAttributesNeeded = JsonRead.StrictStringList(root, "combinedAttributesNeeded"),
            Marking = JsonRead.OptionalString(root, "marking"),
        };
    }

    /// <summary>Parse a render-ready scheme view from a JSON string.</summary>
    /// <exception cref="JsonException">A required member is missing or has the wrong type.</exception>
    public static SchemeView ParseSchemeView(string json)
    {
        using var doc = JsonDocument.Parse(json);
        ResponseCheck.Require("SchemeViewResponse", doc.RootElement);
        return ReadSchemeView(doc.RootElement);
    }

    /// <summary>Parse a render-ready guide view from a JSON string.</summary>
    /// <exception cref="JsonException">A required member is missing or has the wrong type.</exception>
    public static GuideView ParseGuideView(string json)
    {
        using var doc = JsonDocument.Parse(json);
        ResponseCheck.Require("GuideViewResponse", doc.RootElement);
        return ReadGuideView(doc.RootElement);
    }

    // Element cores: a nested occurrence is read from the element already
    // in hand instead of being re-serialized and re-parsed.
    internal static SchemeView ReadSchemeView(JsonElement root) => new()
    {
        Id = JsonRead.RequiredString(root, "id"),
        Name = JsonRead.RequiredString(root, "name"),
        Description = JsonRead.RequiredString(root, "description"),
        Categories = JsonRead.RequiredList(root, "categories", category => new SchemeViewCategory
        {
            Id = JsonRead.RequiredString(category, "id"),
            Label = JsonRead.RequiredString(category, "label"),
            Items = JsonRead.RequiredList(category, "items", ReadConceptView),
        }),
        Uncategorized = JsonRead.RequiredList(root, "uncategorized", ReadConceptView),
        Marking = JsonRead.OptionalString(root, "marking"),
        Vocabulary = OptionalVocabulary(root),
    };

    internal static GuideView ReadGuideView(JsonElement root) => new()
    {
        Id = JsonRead.RequiredString(root, "id"),
        Name = JsonRead.RequiredString(root, "name"),
        Description = JsonRead.RequiredString(root, "description"),
        Categories = JsonRead.RequiredList(root, "categories", category => new GuideViewCategory
        {
            Id = JsonRead.RequiredString(category, "id"),
            Label = JsonRead.RequiredString(category, "label"),
            Items = JsonRead.RequiredList(category, "items", ReadGuideRuleView),
        }),
        Uncategorized = JsonRead.RequiredList(root, "uncategorized", ReadGuideRuleView),
        Marking = JsonRead.OptionalString(root, "marking"),
        Vocabulary = OptionalVocabulary(root),
    };

    private static IReadOnlyDictionary<string, Term>? OptionalVocabulary(JsonElement root) =>
        root.TryGetProperty("vocabulary", out var vocabulary) && vocabulary.ValueKind == JsonValueKind.Object
            ? ParseVocabularyElement(vocabulary) : null;

    private static GuideRuleView ReadGuideRuleView(JsonElement el)
    {
        JsonRead.RequireObject(el, "guide rule view");
        return new GuideRuleView
        {
            Id = JsonRead.RequiredString(el, "id"),
            Name = JsonRead.RequiredString(el, "name"),
            OutcomeText = JsonRead.RequiredString(el, "outcomeText"),
            IsComputed = JsonRead.RequiredBool(el, "isComputed"),
            Keywords = JsonRead.StrictStringList(el, "keywords"),
            MatcherLabels = JsonRead.StrictStringList(el, "matcherLabels"),
            DependsOn = JsonRead.StrictStringList(el, "dependsOn"),
            DependsOnNames = JsonRead.StrictStringList(el, "dependsOnNames"),
            Overrides = JsonRead.StrictStringList(el, "overrides"),
            OverrideNames = JsonRead.StrictStringList(el, "overrideNames"),
            Description = JsonRead.OptionalString(el, "description"),
            WhenText = JsonRead.OptionalString(el, "whenText"),
            Rationale = JsonRead.OptionalString(el, "rationale"),
        };
    }

    /// <summary>Parse a composed document audit report from a JSON string.</summary>
    public static DocumentAuditReport ParseDocumentAuditReport(string json)
    {
        using var doc = JsonDocument.Parse(json);
        ResponseCheck.Require("DocumentAuditReport", doc.RootElement);
        return ReadDocumentAuditReport(doc.RootElement);
    }

    /// <summary>Parse a composed document audit report from a nested element.</summary>
    public static DocumentAuditReport ParseDocumentAuditReportElement(JsonElement root)
    {
        ResponseCheck.Require("DocumentAuditReport", root);
        return ReadDocumentAuditReport(root);
    }

    internal static DocumentAuditReport ReadDocumentAuditReport(JsonElement root)
    {
        JsonRead.RequireObject(root, "document audit report");

        var wholes = new List<AuditWhole>();
        foreach (var w in RequiredArray(root, "wholes"))
            wholes.Add(new AuditWhole
            {
                Marking = JsonRead.RequiredString(w, "marking"),
                Start = JsonRead.RequiredInt(w, "start"),
                End = JsonRead.RequiredInt(w, "end"),
                OffsetEncoding = JsonRead.RequiredString(w, "offsetEncoding"),
            });

        var parts = new List<AuditPart>();
        foreach (var p in RequiredArray(root, "parts"))
            parts.Add(new AuditPart
            {
                Marking = JsonRead.RequiredString(p, "marking"),
                Text = p.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String ? text.GetString() : null,
                Start = JsonRead.RequiredInt(p, "start"),
                End = JsonRead.RequiredInt(p, "end"),
                OffsetEncoding = JsonRead.RequiredString(p, "offsetEncoding"),
                ScopeId = JsonRead.RequiredString(p, "scopeId"),
                Validation = ParseMarkingValidationElement(JsonRead.RequiredProperty(p, "validation"))
                    ?? throw new JsonException("validation must be an object"),
            });

        var unmarked = new List<UnmarkedSpan>();
        foreach (var u in RequiredArray(root, "unmarkedSpans"))
            unmarked.Add(new UnmarkedSpan
            {
                Start = JsonRead.RequiredInt(u, "start"),
                End = JsonRead.RequiredInt(u, "end"),
                OffsetEncoding = JsonRead.RequiredString(u, "offsetEncoding"),
                Text = u.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String ? text.GetString() : null,
            });

        return new DocumentAuditReport
        {
            MarkingRequirements = ReadDocumentMarkingRequirements(JsonRead.RequiredProperty(root, "markingRequirements")),
            Whole = root.TryGetProperty("whole", out var wh) && wh.ValueKind == JsonValueKind.String ? wh.GetString() : null,
            Wholes = wholes.AsReadOnly(),
            WholeValidation = root.TryGetProperty("wholeValidation", out var wv) ? ParseMarkingValidationElement(wv) : null,
            Parts = parts.AsReadOnly(),
            WholeCoversParts = root.TryGetProperty("wholeCoversParts", out var wcp) ? ReadCoverageReport(wcp) : null,
            UnmarkedSpans = unmarked.AsReadOnly(),
            ProceduralFlags = JsonRead.StrictStringList(root, "proceduralFlags"),
            OverallVerdict = new AuditVerdict(JsonRead.RequiredString(root, "overallVerdict")),
            DocumentDigest = JsonRead.RequiredString(root, "documentDigest"),
            Limitations = JsonRead.RequiredList(root, "limitations", ParseAssessmentLimitationElement),
            Vocabulary = root.TryGetProperty("vocabulary", out var voc) && voc.ValueKind == JsonValueKind.Object ? ParseVocabularyElement(voc) : null,
            GuideAssessment = root.TryGetProperty("guideAssessment", out var guide) && guide.ValueKind == JsonValueKind.Object
                ? ParseDocumentGuideAssessmentElement(guide) : null,
        };
    }

    /// <summary>Parse the resolved Context envelope for document audit.</summary>
    public static ContextDocumentAuditResult ParseContextDocumentAuditResult(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        JsonRead.RequireObject(root, "document audit envelope");
        ResponseCheck.Require("ContextDocumentAuditResult", root);
        return new ContextDocumentAuditResult
        {
            Resolution = ParseContextResolutionElement(JsonRead.RequiredProperty(root, "resolution")),
            Result = ReadDocumentAuditReport(JsonRead.RequiredProperty(root, "result")),
            ResponseProjection = ParseDocumentResponseProjectionElement(JsonRead.RequiredProperty(root, "responseProjection")),
        };
    }

    /// <summary>Parse the resolved Context envelope for scheme processing.</summary>
    public static ContextSchemeProcessResult ParseContextSchemeProcessResult(string json)
    {
        var envelope = ParseContextEnvelope(json, "scheme process", "ContextSchemeProcessResult", ResultParser.ParseProcessResultElement);
        return new() { Resolution = envelope.Resolution, Result = envelope.Result };
    }

    /// <summary>Parse the resolved Context envelope for scheme validation or label preset expansion.</summary>
    public static ContextSchemeValidateResult ParseContextSchemeValidateResult(string json)
    {
        var envelope = ParseContextEnvelope(json, "scheme validation", "ContextSchemeValidateResult", ResultParser.ParseValidateResultElement);
        return new() { Resolution = envelope.Resolution, Result = envelope.Result };
    }

    /// <summary>Parse the resolved Context envelope for a scheme explanation.</summary>
    public static ContextSchemeExplainResult ParseContextSchemeExplainResult(string json)
    {
        var envelope = ParseContextEnvelope(json, "scheme explanation", "ContextSchemeExplainResult", el =>
        {
            JsonRead.RequireObject(el, "explanation trace");
            return ResultParser.ReadExplanationTrace(el);
        });
        return new() { Resolution = envelope.Resolution, Result = envelope.Result };
    }

    /// <summary>Parse the resolved Context envelope for scheme suggestions.</summary>
    public static ContextSchemeSuggestResult ParseContextSchemeSuggestResult(string json)
    {
        var envelope = ParseContextEnvelope(json, "scheme suggestions", "ContextSchemeSuggestResult", ParseSuggestResultElement);
        return new() { Resolution = envelope.Resolution, Result = envelope.Result };
    }

    /// <summary>Parse the resolved Context envelope for per-category scheme suggestions.</summary>
    public static ContextSchemeSuggestForCategoriesResult ParseContextSchemeSuggestForCategoriesResult(string json)
    {
        var envelope = ParseContextEnvelope(json, "per-category scheme suggestions", "ContextSchemeSuggestForCategoriesResult", ParseSuggestResultsByCategoryElement);
        return new() { Resolution = envelope.Resolution, Result = envelope.Result };
    }

    /// <summary>Parse the resolved Context envelope for scheme aggregation.</summary>
    public static ContextSchemeAggregateResult ParseContextSchemeAggregateResult(string json)
    {
        var envelope = ParseContextEnvelope(json, "scheme aggregation", "ContextSchemeAggregateResult", ResultParser.ParseAggregateResultElement);
        return new() { Resolution = envelope.Resolution, Result = envelope.Result };
    }

    /// <summary>Parse the resolved Context envelope for scheme scanning.</summary>
    public static ContextSchemeScanResult ParseContextSchemeScanResult(string json)
    {
        var envelope = ParseContextEnvelope(json, "scheme scan", "ContextSchemeScanResult", ResultParser.ParseScanResponseElement);
        return new() { Resolution = envelope.Resolution, Result = envelope.Result };
    }

    /// <summary>Parse the resolved Context envelope for scheme rendering.</summary>
    public static ContextSchemeRenderResult ParseContextSchemeRenderResult(string json)
    {
        var envelope = ParseContextEnvelope(json, "scheme render", "ContextSchemeRenderResult", el => new RenderResponse
        {
            Result = ParseRenderResultElement(el),
            Assessment = ResultParser.ParseRenderAssessmentElement(JsonRead.RequiredProperty(el, "assessment")),
        });
        return new() { Resolution = envelope.Resolution, Result = envelope.Result };
    }

    /// <summary>Parse the resolved Context envelope for scheme coverage checks.</summary>
    public static ContextSchemeCoverageResult ParseContextSchemeCoverageResult(string json)
    {
        var envelope = ParseContextEnvelope(json, "scheme coverage", "ContextSchemeCoverageResult", el => ReadCoverageReport(el) ?? throw new JsonException("Expected a coverage report object"));
        return new() { Resolution = envelope.Resolution, Result = envelope.Result };
    }

    /// <summary>Parse the resolved Context envelope for concept descriptions.</summary>
    public static ContextSchemeConceptDescriptionResult ParseContextSchemeConceptDescriptionResult(string json)
    {
        var envelope = ParseContextEnvelope(json, "concept description", "ContextSchemeConceptDescriptionResult", ParseConceptDescriptionElement);
        return new() { Resolution = envelope.Resolution, Result = envelope.Result };
    }

    /// <summary>Parse the resolved Context envelope for scheme diffs.</summary>
    public static ContextSchemeDiffResult ParseContextSchemeDiffResult(string json)
    {
        var envelope = ParseContextEnvelope(json, "scheme diff", "ContextSchemeDiffResult", ResultParser.ParseDiffResultElement);
        return new() { Resolution = envelope.Resolution, Result = envelope.Result };
    }

    /// <summary>Parse the resolved Context envelope for scheme notices.</summary>
    public static ContextSchemeNoticesResult ParseContextSchemeNoticesResult(string json)
    {
        var envelope = ParseContextEnvelope(json, "scheme notices", "ContextSchemeNoticesResult", ParseNoticesElement);
        return new() { Resolution = envelope.Resolution, Result = envelope.Result };
    }

    /// <summary>Parse the resolved Context envelope for a scheme structural view.</summary>
    public static ContextSchemeViewResult ParseContextSchemeViewResult(string json)
    {
        var envelope = ParseContextEnvelope(json, "scheme view", "ContextSchemeViewResult", ReadSchemeView);
        return new() { Resolution = envelope.Resolution, Result = envelope.Result };
    }

    /// <summary>Parse the resolved Context envelope for guide scanning.</summary>
    public static ContextGuideScanResult ParseContextGuideScanResult(string json)
    {
        var envelope = ParseContextEnvelope(json, "guide scan", "ContextGuideScanResult", ResultParser.ParseGuideScanResultsElement);
        return new() { Resolution = envelope.Resolution, Result = envelope.Result };
    }

    /// <summary>Parse the resolved Context envelope for guide evaluation.</summary>
    public static ContextGuideEvaluationResult ParseContextGuideEvaluationResult(string json)
    {
        var envelope = ParseContextEnvelope(json, "guide evaluation", "ContextGuideEvaluationResult", ResultParser.ReadEvaluationResult);
        return new() { Resolution = envelope.Resolution, Result = envelope.Result };
    }

    /// <summary>Parse the resolved Context envelope for a guide structural view.</summary>
    public static ContextGuideViewResult ParseContextGuideViewResult(string json)
    {
        var envelope = ParseContextEnvelope(json, "guide view", "ContextGuideViewResult", ReadGuideView);
        return new() { Resolution = envelope.Resolution, Result = envelope.Result };
    }

    /// <summary>Parse the resolved Context envelope for an access decision.</summary>
    public static ContextAccessDecisionResult ParseContextAccessDecisionResult(string json)
    {
        var envelope = ParseContextEnvelope(json, "access decision", "ContextAccessDecisionResult", ResultParser.ReadAccessDecision);
        return new() { Resolution = envelope.Resolution, Result = envelope.Result };
    }

    /// <summary>Parse the resolved Context envelope for access requirements.</summary>
    public static ContextAccessRequirementsResult ParseContextAccessRequirementsResult(string json)
    {
        var envelope = ParseContextEnvelope(json, "access requirements", "ContextAccessRequirementsResult", ParseAccessRequirementsDescriptionElement);
        return new() { Resolution = envelope.Resolution, Result = envelope.Result };
    }

    /// <summary>Parse every access-rule description from the Context resource response.</summary>
    public static IReadOnlyList<AccessRuleDescription> ParseContextAccessRules(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        JsonRead.RequireObject(root, "access rules response");
        ResponseCheck.Require("CtxAccessRulesResponseBody", root);
        return JsonRead.RequiredList(root, "rules", ReadAccessRuleDescription);
    }

    /// <summary>
    /// Parse the access policy's own overall marking from the same response
    /// <see cref="ParseContextAccessRules"/> reads, or null when the policy declares none.
    /// </summary>
    /// <exception cref="JsonException">The body lacks a member the contract requires.</exception>
    public static string? ParseContextAccessMarking(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        JsonRead.RequireObject(root, "access rules response");
        ResponseCheck.Require("CtxAccessRulesResponseBody", root);
        return JsonRead.OptionalString(root, "marking");
    }

    /// <summary>
    /// Parse every crosswalk from the <c>crosswalk/describe</c> response. Each
    /// entry stays JSON, as a translation result does: its mappings and
    /// selections carry the same reviewed decisions.
    /// </summary>
    /// <exception cref="JsonException">The body lacks a member the contract requires.</exception>
    public static IReadOnlyList<JsonElement> ParseContextCrosswalks(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        JsonRead.RequireObject(root, "crosswalk inventory response");
        ResponseCheck.Require("CtxCrosswalkDescribeResponseBody", root);
        return JsonRead.RequiredList(root, "crosswalks", element => element.Clone());
    }

    /// <summary>
    /// Parse the body of <c>GET /v1/ctx/{ctxId}/{version}</c>. The wire omits
    /// <c>guides</c>, <c>crosswalks</c>, <c>access</c> and a scheme's
    /// <c>layers</c> rather than sending empty collections, and "the Context
    /// declares none" is a real answer, so an omitted collection reads as empty.
    /// The held-scheme inventory is the part a caller reaches back into the
    /// server with, so a descriptor that is not whole makes the response
    /// malformed instead of being dropped.
    /// </summary>
    /// <exception cref="JsonException">A required member is missing or has the wrong type.</exception>
    public static ContextDescription ParseContextDescription(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        JsonRead.RequireObject(root, "Context description");
        ResponseCheck.Require("CtxDescribeResponseBody", root);
        var scheme = ParseContextComponentElement(JsonRead.RequiredProperty(root, "scheme"));
        return new ContextDescription
        {
            Id = JsonRead.RequiredString(root, "id"),
            Version = JsonRead.RequiredString(root, "version"),
            Scheme = scheme,
            Schemes = JsonRead.RequiredList(root, "schemes", ParseContextSchemeDescriptorElement),
            Guides = ParseOptionalComponentList(root, "guides"),
            Crosswalks = ParseOptionalComponentList(root, "crosswalks"),
            Access = Present(root, "access") ? ParseContextComponentElement(JsonRead.RequiredProperty(root, "access")) : null,
            Description = JsonRead.OptionalString(root, "description"),
        };
    }

    private static ContextComponentRef ParseContextComponentElement(JsonElement el)
    {
        JsonRead.RequireObject(el, "Context component");
        return new ContextComponentRef
        {
            Id = JsonRead.RequiredString(el, "id"),
            Version = JsonRead.RequiredString(el, "version"),
            ResolvedVersion = JsonRead.RequiredString(el, "resolvedVersion"),
            Layers = ParseOptionalComponentList(el, "layers"),
        };
    }

    private static IReadOnlyList<ContextComponentRef> ParseOptionalComponentList(JsonElement parent, string key) =>
        Present(parent, key)
            ? JsonRead.ElementList(JsonRead.RequiredProperty(parent, key), ParseContextComponentElement)
            : Array.Empty<ContextComponentRef>();

    private static ContextSchemeDescriptor ParseContextSchemeDescriptorElement(JsonElement el)
    {
        JsonRead.RequireObject(el, "Context scheme descriptor");
        var roles = JsonRead.RequiredList(el, "roles", role =>
            role.ValueKind == JsonValueKind.String
                ? new ContextSchemeRole(role.GetString()!)
                : throw new JsonException("roles must contain strings"));
        // "Never empty" is the contract: a scheme held for no stated reason is
        // `additional`, not a scheme with no roles.
        if (roles.Count == 0) throw new JsonException("roles must not be empty");
        return new ContextSchemeDescriptor
        {
            Selector = JsonRead.RequiredString(el, "selector"),
            Id = JsonRead.RequiredString(el, "id"),
            Version = JsonRead.RequiredString(el, "version"),
            Roles = roles,
        };
    }

    /// <summary>Parse a directional crosswalk result while preserving its discriminated union.</summary>
    public static ContextCrosswalkTranslationResult ParseContextCrosswalkTranslationResult(string json)
    {
        var envelope = ParseContextEnvelope(json, "crosswalk translation", "ContextCrosswalkTranslationResult", el => el.Clone());
        return new() { Resolution = envelope.Resolution, Result = envelope.Result };
    }

    // --- scheme resource projections ----------------------------------------
    //
    // The scheme sub-resources answer flat, outside the Context envelope: they
    // are reads of the composed scheme itself, not operations over it, so there
    // is no resolution block or operation-limitation list to carry. The one
    // exception is apply-preset, which runs the marking through the engine and
    // therefore answers in the standard context operation envelope like every
    // other context operation.
    //
    // Required fields are decoded strictly; an absent one is a contract
    // violation, not a projection the caller asked for. Opaque payloads are
    // cloned out of the JsonDocument before it is disposed.

    /// <summary>Parse a scheme summary from a JSON string.</summary>
    public static SchemeSummary ParseSchemeSummary(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        JsonRead.RequireObject(root, "scheme summary");
        ResponseCheck.Require("SchemeSummary", root);
        return new SchemeSummary
        {
            Id = JsonRead.RequiredString(root, "id"),
            Version = JsonRead.RequiredString(root, "version"),
            Name = JsonRead.RequiredString(root, "name"),
            Description = JsonRead.RequiredString(root, "description"),
            RoleAware = OptionalBool(root, "roleAware"),
            ReferenceSurfaceLabel = ParseReferenceSurfaceLabelElement(JsonRead.RequiredProperty(root, "referenceSurfaceLabel")),
            EmbeddedMarking = OptionalElement(root, "embeddedMarking"),
            DefaultRenderFormat = JsonRead.OptionalString(root, "defaultRenderFormat"),
            Definition = JsonRead.OptionalString(root, "definition"),
            Guidance = JsonRead.OptionalString(root, "guidance"),
            Marking = JsonRead.OptionalString(root, "marking"),
            ColorFrom = JsonRead.OptionalString(root, "colorFrom"),
            ColorRules = root.TryGetProperty("colorRules", out var rules) && rules.ValueKind == JsonValueKind.Array
                ? JsonRead.ElementList(rules, ParseMarkingColorRuleElement) : null,
            Vocabulary = root.TryGetProperty("vocabulary", out var vocabulary) && vocabulary.ValueKind == JsonValueKind.Object
                ? ParseVocabularyElement(vocabulary) : null,
            ContextFormats = root.TryGetProperty("contextFormats", out var formats) && formats.ValueKind == JsonValueKind.Object
                ? new SchemeContextFormats { Whole = JsonRead.StrictStringList(formats, "whole"), Part = JsonRead.StrictStringList(formats, "part") }
                : null,
            DefaultScope = JsonRead.OptionalString(root, "defaultScope"),
            ScopeLabel = JsonRead.OptionalString(root, "scopeLabel"),
            DefaultPartRenderFormat = JsonRead.OptionalString(root, "defaultPartRenderFormat"),
            DefaultWholeRenderFormat = JsonRead.OptionalString(root, "defaultWholeRenderFormat"),
        };
    }

    /// <summary>Parse a scheme's concept views from a JSON string.</summary>
    public static IReadOnlyList<ConceptView> ParseSchemeConcepts(string json)
    {
        using var doc = JsonDocument.Parse(json);
        ResponseCheck.RequireEach("ConceptView", doc.RootElement);
        return ParseRootArray(doc.RootElement, "scheme concepts", ReadConceptView);
    }

    private static ConceptView ReadConceptView(JsonElement el)
    {
        JsonRead.RequireObject(el, "concept view");
        return new ConceptView
        {
            Id = JsonRead.RequiredString(el, "id"),
            Name = JsonRead.RequiredString(el, "name"),
            Views = RequiredStringMap(el, "views"),
            Aliases = JsonRead.StrictStringList(el, "aliases"),
            Categories = JsonRead.StrictStringList(el, "categories"),
            Parents = JsonRead.StrictStringList(el, "parents"),
            Position = OptionalDouble(el, "position") ?? throw new JsonException("Missing required property: position"),
            Color = el.TryGetProperty("color", out var color) && color.ValueKind == JsonValueKind.Object
                ? ParseConceptColorElement(color) : null,
            Rank = OptionalDouble(el, "rank"),
            Tier = JsonRead.OptionalString(el, "tier"),
            Members = JsonRead.StrictOptionalStringList(el, "members"),
            Scopes = JsonRead.StrictOptionalStringList(el, "scopes"),
            Deprecated = OptionalBool(el, "deprecated"),
            DeprecatedMessage = JsonRead.OptionalString(el, "deprecatedMessage"),
            DeprecatedReplacement = JsonRead.OptionalString(el, "deprecatedReplacement"),
            Definition = JsonRead.OptionalString(el, "definition"),
            Citation = JsonRead.OptionalString(el, "citation"),
            Guidance = JsonRead.OptionalString(el, "guidance"),
            Notices = Present(el, "notices") ? JsonRead.RequiredList(el, "notices", ParseConceptNoticeElement) : null,
            RulesReferencing = JsonRead.StrictOptionalStringList(el, "rulesReferencing"),
            TextInput = OptionalElement(el, "textInput"),
        };
    }

    private static ConceptNotice ParseConceptNoticeElement(JsonElement el)
    {
        JsonRead.RequireObject(el, "concept notice");
        var required = JsonRead.RequiredProperty(el, "required");
        if (required.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
            throw new JsonException("required must be a boolean");
        return new ConceptNotice
        {
            Type = JsonRead.RequiredString(el, "type"),
            Required = required.GetBoolean(),
            Text = JsonRead.RequiredString(el, "text"),
            Condition = JsonRead.OptionalString(el, "condition"),
            Views = OptionalStringMap(el, "views"),
        };
    }

    /// <summary>Parse a scheme's category views from a JSON string.</summary>
    public static IReadOnlyList<CategoryView> ParseSchemeCategories(string json)
    {
        using var doc = JsonDocument.Parse(json);
        ResponseCheck.RequireEach("CategoryView", doc.RootElement);
        return ParseRootArray(doc.RootElement, "scheme categories", el => new CategoryView
        {
            Id = JsonRead.RequiredString(el, "id"),
            Label = JsonRead.RequiredString(el, "label"),
            Rollup = JsonRead.RequiredString(el, "rollup"),
            Required = OptionalBool(el, "required"),
            Single = OptionalBool(el, "single"),
            Sort = OptionalElement(el, "sort"),
            PositionOverrides = OptionalObjectMap(el, "positionOverrides", value =>
                value.ValueKind == JsonValueKind.Number ? value.GetDouble()
                    : throw new JsonException("positionOverrides must contain numbers")),
            Scopes = JsonRead.StrictOptionalStringList(el, "scopes"),
            Position = OptionalDouble(el, "position"),
            HierarchyRoot = OptionalBool(el, "hierarchyRoot"),
            DefaultConcept = OptionalElement(el, "defaultConcept"),
        });
    }

    /// <summary>Parse a scheme's render format list from a JSON string.</summary>
    public static IReadOnlyList<FormatView> ParseSchemeFormats(string json)
    {
        using var doc = JsonDocument.Parse(json);
        ResponseCheck.RequireEach("FormatView", doc.RootElement);
        return ParseRootArray(doc.RootElement, "scheme formats", ParseSchemeFormatElement);
    }

    private static FormatView ParseSchemeFormatElement(JsonElement el) => new()
    {
        Id = JsonRead.RequiredString(el, "id"),
        Surface = JsonRead.RequiredString(el, "surface"),
        Mode = new RenderFormatMode(JsonRead.RequiredString(el, "mode")),
        CategoryOrder = JsonRead.StrictStringList(el, "categoryOrder"),
        View = JsonRead.OptionalString(el, "view"),
        Prefix = JsonRead.OptionalString(el, "prefix"),
        Suffix = JsonRead.OptionalString(el, "suffix"),
        Separator = JsonRead.OptionalString(el, "separator"),
        CategorySeparators = OptionalStringMap(el, "categorySeparators"),
        SeparatorBefore = OptionalStringMap(el, "separatorBefore"),
        CategoryViews = OptionalStringMap(el, "categoryViews"),
        CategoryTiers = OptionalObjectMap(el, "categoryTiers", tiers => JsonRead.ElementList(tiers, ParseTierCompositionElement)),
        CategoryEach = OptionalObjectMap(el, "categoryEach", ParseEachAffixElement),
        Headers = Present(el, "headers") ? JsonRead.RequiredList(el, "headers", ParseFormatHeaderAssociationElement) : null,
        Syntax = Present(el, "syntax") ? ParseFormatSyntaxElement(JsonRead.RequiredProperty(el, "syntax")) : null,
        OutputView = JsonRead.OptionalString(el, "outputView"),
        // Sent only for a scheme that declares surfaces, and then on every view:
        // an empty list (a default for no role) is a different answer from an
        // absent one (a scheme with no surfaces), so absent stays null.
        DefaultFor = Present(el, "defaultFor")
            ? JsonRead.RequiredList(el, "defaultFor", ParseRoleElement)
            : null,
        Role = Present(el, "role") ? ParseRoleElement(JsonRead.RequiredProperty(el, "role")) : null,
    };

    private static StructuralRole ParseRoleElement(JsonElement el)
    {
        if (el.ValueKind != JsonValueKind.String) throw new JsonException("A role must be a string token");
        return new StructuralRole(el.GetString()!);
    }

    private static TierComposition ParseTierCompositionElement(JsonElement el)
    {
        JsonRead.RequireObject(el, "tier composition");
        return new TierComposition
        {
            Tier = JsonRead.RequiredString(el, "tier"),
            AttachedBy = JsonRead.RequiredString(el, "attachedBy"),
            View = JsonRead.OptionalString(el, "view"),
            HeadView = JsonRead.OptionalString(el, "headView"),
        };
    }

    private static EachAffix ParseEachAffixElement(JsonElement el)
    {
        JsonRead.RequireObject(el, "each affix");
        return new EachAffix { Prefix = JsonRead.OptionalString(el, "prefix"), Suffix = JsonRead.OptionalString(el, "suffix") };
    }

    private static FormatHeaderAssociation ParseFormatHeaderAssociationElement(JsonElement el)
    {
        JsonRead.RequireObject(el, "format header");
        return new FormatHeaderAssociation
        {
            Concept = JsonRead.RequiredString(el, "concept"),
            HeaderCategory = JsonRead.RequiredString(el, "headerCategory"),
            Category = JsonRead.RequiredString(el, "category"),
            Position = new FormatHeaderPosition(JsonRead.RequiredString(el, "position")),
        };
    }

    private static FormatSyntax ParseFormatSyntaxElement(JsonElement el)
    {
        JsonRead.RequireObject(el, "format syntax");
        return new FormatSyntax { Type = new FormatSyntaxType(JsonRead.RequiredString(el, "type")) };
    }

    /// <summary>
    /// Parse a scheme's scope definitions from a JSON string. The contract
    /// answers an array rather than a map, because the order is the order a
    /// selector offers the scopes in and a JSON object does not carry order. A
    /// scheme that declares no scopes answers an empty array.
    /// </summary>
    public static IReadOnlyList<SchemeScope> ParseSchemeScopes(string json)
    {
        using var doc = JsonDocument.Parse(json);
        ResponseCheck.Require("SchemeScopesResponse", doc.RootElement);
        return ParseRootArray(doc.RootElement, "scheme scopes", el =>
        {
            JsonRead.RequireObject(el, "scheme scope");
            return new SchemeScope { Id = JsonRead.RequiredString(el, "id"), Label = JsonRead.RequiredString(el, "label") };
        });
    }

    /// <summary>Parse a scheme's label preset views from a JSON string.</summary>
    public static IReadOnlyList<LabelPresetView> ParseSchemeLabelPresets(string json)
    {
        using var doc = JsonDocument.Parse(json);
        ResponseCheck.RequireEach("LabelPresetView", doc.RootElement);
        return ParseRootArray(doc.RootElement, "scheme label presets", el => new LabelPresetView
        {
            Id = JsonRead.RequiredString(el, "id"),
            Label = JsonRead.RequiredString(el, "label"),
            Description = JsonRead.OptionalString(el, "description"),
            Scope = JsonRead.OptionalString(el, "scope"),
        });
    }

    private static ReferenceSurfaceLabel ParseReferenceSurfaceLabelElement(JsonElement el)
    {
        JsonRead.RequireObject(el, "reference surface label");
        return new ReferenceSurfaceLabel { Singular = JsonRead.RequiredString(el, "singular"), Plural = JsonRead.RequiredString(el, "plural") };
    }

    private static MarkingColorRule ParseMarkingColorRuleElement(JsonElement el)
    {
        JsonRead.RequireObject(el, "marking color rule");
        return new MarkingColorRule
        {
            Color = ParseConceptColorElement(JsonRead.RequiredProperty(el, "color")),
            When = OptionalElement(el, "when"),
        };
    }

    internal static ConceptColor ParseConceptColorElement(JsonElement el)
    {
        JsonRead.RequireObject(el, "concept color");
        return new ConceptColor { Background = JsonRead.RequiredString(el, "background"), Text = JsonRead.RequiredString(el, "text") };
    }

    // The resource routes answer with a bare array rather than a wrapper object,
    // so the root itself is the collection to walk.
    private static IReadOnlyList<T> ParseRootArray<T>(JsonElement root, string name, Func<JsonElement, T> parse)
    {
        if (root.ValueKind != JsonValueKind.Array) throw new JsonException($"Expected {name} array");
        return JsonRead.ElementList(root, parse);
    }

    // Opaque payloads must outlive the JsonDocument they were read from, so the
    // element is cloned rather than handed back as a view into a disposed buffer.
    private static JsonElement? OptionalElement(JsonElement parent, string key) =>
        parent.TryGetProperty(key, out var value) && value.ValueKind != JsonValueKind.Null
            ? value.Clone()
            : null;

    /// <summary>Parse the <c>{ result }</c> envelope returned by a render op.</summary>
    public static string ParseRenderResult(string json)
    {
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.ValueKind == JsonValueKind.Object) ResponseCheck.Require("RenderResponse", doc.RootElement);
        return ParseRenderResultElement(doc.RootElement);
    }

    // Element core: a nested occurrence is read from the element already
    // in hand instead of being re-serialized and re-parsed.
    internal static string ParseRenderResultElement(JsonElement root)
    {
        JsonRead.RequireObject(root, "result envelope");
        return JsonRead.RequiredString(root, "result");
    }

    // Element core: a nested occurrence is read from the element already
    // in hand instead of being re-serialized and re-parsed.
    private static (ContextResolution Resolution, T Result)
        ParseContextEnvelope<T>(string json, string operation, string component, Func<JsonElement, T> parseResult)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        JsonRead.RequireObject(root, $"{operation} envelope");
        ResponseCheck.Require(component, root);
        return (
            ParseContextResolutionElement(JsonRead.RequiredProperty(root, "resolution")),
            parseResult(JsonRead.RequiredProperty(root, "result")));
    }

    /// <summary>Parse an immutable artifact reference.</summary>
    public static ArtifactReference ParseArtifactReferenceElement(JsonElement el)
    {
        ResponseCheck.Require("ArtifactReference", el);
        return ReadArtifactReference(el);
    }

    internal static ArtifactReference ReadArtifactReference(JsonElement el)
    {
        JsonRead.RequireObject(el, "artifact reference");
        return new ArtifactReference
        {
            Kind = new ArtifactKind(JsonRead.RequiredString(el, "kind")),
            Id = JsonRead.RequiredString(el, "id"),
            Version = JsonRead.RequiredString(el, "version"),
            Digest = JsonRead.OptionalString(el, "digest"),
        };
    }

    /// <summary>Parse a qualified rule reference.</summary>
    public static RuleReference ParseRuleReferenceElement(JsonElement el)
    {
        ResponseCheck.Require("RuleReference", el);
        return ReadRuleReference(el);
    }

    internal static RuleReference ReadRuleReference(JsonElement el)
    {
        JsonRead.RequireObject(el, "rule reference");
        return new RuleReference
        {
            Artifact = ReadArtifactReference(JsonRead.RequiredProperty(el, "artifact")),
            RuleId = JsonRead.RequiredString(el, "ruleId"),
            RuleDigest = JsonRead.RequiredString(el, "ruleDigest"),
        };
    }

    /// <summary>Parse access-rule activation evidence.</summary>
    public static AccessRuleActivation ParseAccessRuleActivationElement(JsonElement el)
    {
        ResponseCheck.Require("AccessRuleActivation", el);
        return ReadAccessRuleActivation(el);
    }

    internal static AccessRuleActivation ReadAccessRuleActivation(JsonElement el)
    {
        JsonRead.RequireObject(el, "access rule activation");
        return new AccessRuleActivation
        {
            Matched = JsonRead.RequiredBool(el, "matched"),
            Assertions = JsonRead.RequiredList(el, "assertions", ParseAssertionReferenceElement),
            ContextPaths = JsonRead.StrictStringList(el, "contextPaths"),
        };
    }

    /// <summary>Parse access comparison results.</summary>
    public static IReadOnlyList<AccessComparisonResult> ParseAccessComparisonResults(JsonElement el)
    {
        ResponseCheck.RequireEach("AccessComparisonResult", el);
        return ReadAccessComparisonResults(el);
    }

    internal static IReadOnlyList<AccessComparisonResult> ReadAccessComparisonResults(JsonElement el)
    {
        if (el.ValueKind != JsonValueKind.Array) throw new JsonException("Expected access comparisons array");
        var result = new List<AccessComparisonResult>();
        foreach (var item in el.EnumerateArray())
            result.Add(new AccessComparisonResult
            {
                Left = ParseAccessOperandElement(JsonRead.RequiredProperty(item, "left")),
                Operator = JsonRead.RequiredString(item, "operator"),
                Right = ParseAccessOperandElement(JsonRead.RequiredProperty(item, "right")),
                Matched = JsonRead.RequiredBool(item, "matched"),
                Missing = item.TryGetProperty("missing", out var missing) && missing.ValueKind == JsonValueKind.Array
                    ? CloneElementList(missing) : null,
            });
        return result.AsReadOnly();
    }

    private static AccessOperand ParseAccessOperandElement(JsonElement el)
    {
        JsonRead.RequireObject(el, "access operand");
        return new AccessOperand
        {
            Source = new AccessOperandSource(JsonRead.RequiredString(el, "source")),
            Path = JsonRead.OptionalString(el, "path"),
            CategoryId = JsonRead.OptionalString(el, "categoryId"),
            Assertions = el.TryGetProperty("assertions", out var assertions) && assertions.ValueKind == JsonValueKind.Array
                ? JsonRead.ElementList(assertions, ParseAssertionReferenceElement) : null,
            Value = el.TryGetProperty("value", out var value) ? value.Clone() : null,
        };
    }

    private static DocumentGuideAssessment ParseDocumentGuideAssessmentElement(JsonElement el) => new()
    {
        Status = new GuideCompilationStatus(JsonRead.RequiredString(el, "status")),
        NominationCount = JsonRead.RequiredInt(el, "nominationCount"),
        UnresolvedCount = JsonRead.RequiredInt(el, "unresolvedCount"),
        AcceptedCount = JsonRead.RequiredInt(el, "acceptedCount"),
        RejectedCount = JsonRead.RequiredInt(el, "rejectedCount"),
        EvidenceSpanCount = JsonRead.RequiredInt(el, "evidenceSpanCount"),
        UnitCoverage = JsonRead.RequiredList(el, "unitCoverage", ParseGuideScopeCoverageElement),
        DeterminedRequiredWhole = el.TryGetProperty("determinedRequiredWhole", out var determined) && determined.ValueKind == JsonValueKind.Object ? ParseGuideCompiledResultElement(determined) : null,
        ProjectedRequiredWhole = el.TryGetProperty("projectedRequiredWhole", out var projected) && projected.ValueKind == JsonValueKind.Object ? ParseGuideCompiledResultElement(projected) : null,
        DeterminedWholeCoverage = el.TryGetProperty("determinedWholeCoverage", out var dc) ? ReadCoverageReport(dc) : null,
        ProjectedWholeCoverage = el.TryGetProperty("projectedWholeCoverage", out var pc) ? ReadCoverageReport(pc) : null,
        Evaluation = el.TryGetProperty("evaluation", out var evaluation) && evaluation.ValueKind == JsonValueKind.Object ? ParseGuideContentEvaluationElement(evaluation) : null,
    };

    private static GuideScopeCoverage ParseGuideScopeCoverageElement(JsonElement el) => new()
    {
        ScopeId = JsonRead.RequiredString(el, "scopeId"),
        DeterminedCoverage = el.TryGetProperty("determinedCoverage", out var d) ? ReadCoverageReport(d) : null,
        ProjectedCoverage = el.TryGetProperty("projectedCoverage", out var p) ? ReadCoverageReport(p) : null,
    };

    private static GuideContentEvaluation ParseGuideContentEvaluationElement(JsonElement el) => new()
    {
        DocumentDigest = JsonRead.RequiredString(el, "documentDigest"),
        Nominations = JsonRead.RequiredList(el, "nominations", ParseGuideNominationElement),
        Determinations = JsonRead.RequiredList(el, "determinations", ParseGuideFindingDeterminationElement),
        ManualSelections = JsonRead.RequiredList(el, "manualSelections", ParseManualGuideSelectionElement),
        UnresolvedFindingIds = JsonRead.StrictStringList(el, "unresolvedFindingIds"),
        AcceptedFindingIds = JsonRead.StrictStringList(el, "acceptedFindingIds"),
        RejectedFindingIds = JsonRead.StrictStringList(el, "rejectedFindingIds"),
        SupportCatalog = JsonRead.RequiredList(el, "supportCatalog", ParseAssertionSupportElement),
        Units = JsonRead.RequiredList(el, "units", ParseGuideUnitEvaluationElement),
        Limitations = JsonRead.RequiredList(el, "limitations", ParseAssessmentLimitationElement),
    };

    private static GuideNomination ParseGuideNominationElement(JsonElement el) => new()
    {
        FindingId = JsonRead.RequiredString(el, "findingId"),
        DocumentDigest = JsonRead.RequiredString(el, "documentDigest"),
        Rule = ReadRuleReference(JsonRead.RequiredProperty(el, "rule")),
        Evidence = JsonRead.RequiredList(el, "evidence", ParseContentEvidenceSpanElement),
        ProposedAssertions = JsonRead.RequiredList(el, "proposedAssertions", ParseAssertionReferenceElement),
        Origin = JsonRead.RequiredString(el, "origin"),
        Rationale = JsonRead.OptionalString(el, "rationale"),
    };

    private static GuideFindingDetermination ParseGuideFindingDeterminationElement(JsonElement el) => new()
    {
        FindingId = JsonRead.RequiredString(el, "findingId"),
        DocumentDigest = JsonRead.RequiredString(el, "documentDigest"),
        Decision = new GuideFindingDecision(JsonRead.RequiredString(el, "decision")),
        Reason = JsonRead.RequiredString(el, "reason"),
        ProposalOrigin = el.TryGetProperty("proposalOrigin", out var origin) && origin.ValueKind == JsonValueKind.String
            ? new DecisionProposalOrigin(origin.GetString() ?? "") : null,
    };

    private static ManualGuideSelection ParseManualGuideSelectionElement(JsonElement el) => new()
    {
        DocumentDigest = JsonRead.RequiredString(el, "documentDigest"),
        Rule = ReadRuleReference(JsonRead.RequiredProperty(el, "rule")),
        Evidence = JsonRead.RequiredList(el, "evidence", ParseContentEvidenceSpanElement),
        Reason = JsonRead.RequiredString(el, "reason"),
        SelectionId = JsonRead.RequiredString(el, "selectionId"),
        ProposalOrigin = el.TryGetProperty("proposalOrigin", out var origin) && origin.ValueKind == JsonValueKind.String
            ? new DecisionProposalOrigin(origin.GetString() ?? "") : null,
    };

    private static AssertionSupport ParseAssertionSupportElement(JsonElement el) => new()
    {
        SupportId = JsonRead.RequiredString(el, "supportId"),
        Assertion = ParseAssertionReferenceElement(JsonRead.RequiredProperty(el, "assertion")),
        Rules = JsonRead.RequiredList(el, "rules", ReadRuleReference),
        Evidence = JsonRead.RequiredList(el, "evidence", ParseContentEvidenceSpanElement),
        FactReferences = JsonRead.RequiredList(el, "factReferences", f => new GuideFactReference
        {
            Kind = new GuideFactKind(JsonRead.RequiredString(f, "kind")),
            FactId = JsonRead.RequiredString(f, "factId"),
        }),
    };

    private static GuideUnitEvaluation ParseGuideUnitEvaluationElement(JsonElement el) => new()
    {
        ScopeId = JsonRead.RequiredString(el, "scopeId"),
        Start = JsonRead.RequiredInt(el, "start"),
        End = JsonRead.RequiredInt(el, "end"),
        OffsetEncoding = JsonRead.RequiredString(el, "offsetEncoding"),
        FindingIds = JsonRead.StrictStringList(el, "findingIds"),
        Determined = el.TryGetProperty("determined", out var d) && d.ValueKind == JsonValueKind.Object ? ParseGuideCompiledResultElement(d) : null,
        Projected = el.TryGetProperty("projected", out var p) && p.ValueKind == JsonValueKind.Object ? ParseGuideCompiledResultElement(p) : null,
    };

    private static GuideCompiledResult ParseGuideCompiledResultElement(JsonElement el) => new()
    {
        Assertions = JsonRead.RequiredList(el, "assertions", ResultParser.ReadAssertion),
        Rendered = JsonRead.RequiredString(el, "rendered"),
        Valid = JsonRead.RequiredBool(el, "valid"),
        Errors = JsonRead.RequiredList(el, "errors", ResultParser.ReadValidationError),
        Warnings = JsonRead.RequiredList(el, "warnings", ResultParser.ReadWarning),
        DirectRules = JsonRead.RequiredList(el, "directRules", ReadRuleReference),
        ComputedRules = JsonRead.RequiredList(el, "computedRules", ReadRuleReference),
        OverriddenRules = JsonRead.RequiredList(el, "overriddenRules", ReadRuleReference),
        SupportIds = JsonRead.StrictStringList(el, "supportIds"),
        Detail = el.TryGetProperty("detail", out var detail) && detail.ValueKind != JsonValueKind.Null
            ? new GuideCompiledDetail
            {
                Evaluation = ResultParser.ReadEvaluationResult(JsonRead.RequiredProperty(detail, "evaluation")),
                Process = ResultParser.ParseValidateResultElement(JsonRead.RequiredProperty(detail, "process")),
            }
            : null,
    };

    private static DocumentMarkingRequirements ReadDocumentMarkingRequirements(JsonElement el)
    {
        JsonRead.RequireObject(el, "document marking requirements");
        return new DocumentMarkingRequirements { Whole = JsonRead.RequiredBool(el, "whole"), Part = JsonRead.RequiredBool(el, "part") };
    }

    private static ContentEvidenceSpan ParseContentEvidenceSpanElement(JsonElement el) => new()
    {
        Start = JsonRead.RequiredInt(el, "start"),
        End = JsonRead.RequiredInt(el, "end"),
        OffsetEncoding = JsonRead.RequiredString(el, "offsetEncoding"),
        Rule = ReadRuleReference(JsonRead.RequiredProperty(el, "rule")),
        Matched = JsonRead.OptionalString(el, "matched"),
    };

    private static AssertionReference ParseAssertionReferenceElement(JsonElement el) => new()
    {
        CategoryId = JsonRead.RequiredString(el, "categoryId"),
        ConceptId = JsonRead.RequiredString(el, "conceptId"),
        Value = JsonRead.OptionalString(el, "value"),
    };

    private static ContextResolution ParseContextResolutionElement(JsonElement el) => new()
    {
        ContextId = JsonRead.RequiredString(el, "contextId"),
        ResolvedVersion = JsonRead.RequiredString(el, "resolvedVersion"),
        Artifacts = JsonRead.RequiredList(el, "artifacts", ReadArtifactReference),
        RequestedVersion = JsonRead.OptionalString(el, "requestedVersion"),
    };

    private static DocumentResponseProjection ParseDocumentResponseProjectionElement(JsonElement el) => new()
    {
        Profile = new DocumentResponseProfile(JsonRead.RequiredString(el, "profile")),
        DetailsComplete = JsonRead.RequiredBool(el, "detailsComplete"),
        OmittedFields = JsonRead.StrictStringList(el, "omittedFields"),
        Counts = ParseDocumentResponseCountsElement(JsonRead.RequiredProperty(el, "counts")),
    };

    private static DocumentResponseCounts ParseDocumentResponseCountsElement(JsonElement el) => new()
    {
        Nominations = JsonRead.RequiredInt(el, "nominations"),
        EvidenceSpans = JsonRead.RequiredInt(el, "evidenceSpans"),
        GuideUnits = JsonRead.RequiredInt(el, "guideUnits"),
    };

    private static AssessmentLimitation ParseAssessmentLimitationElement(JsonElement el) => new()
    {
        Code = JsonRead.RequiredString(el, "code"),
        Reason = JsonRead.RequiredString(el, "reason"),
        Scope = JsonRead.OptionalString(el, "scope"),
    };

    // Optional wire fields: absent or explicitly null reads as null. A present
    // value is still decoded strictly, so a wrong type is a contract violation
    // rather than something quietly dropped.
    private static bool? OptionalBool(JsonElement parent, string key)
    {
        if (!Present(parent, key)) return null;
        var value = JsonRead.RequiredProperty(parent, key);
        if (value.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
            throw new JsonException($"{key} must be a boolean");
        return value.GetBoolean();
    }

    private static double? OptionalDouble(JsonElement parent, string key)
    {
        if (!Present(parent, key)) return null;
        var value = JsonRead.RequiredProperty(parent, key);
        if (value.ValueKind != JsonValueKind.Number) throw new JsonException($"{key} must be a number");
        return value.GetDouble();
    }

    internal static IReadOnlyDictionary<string, string> RequiredStringMap(JsonElement parent, string key) =>
        OptionalStringMap(parent, key) ?? throw new JsonException($"Missing required property: {key}");

    internal static IReadOnlyDictionary<string, string>? OptionalStringMap(JsonElement parent, string key)
    {
        if (!Present(parent, key)) return null;
        var value = JsonRead.RequiredProperty(parent, key);
        if (value.ValueKind != JsonValueKind.Object) throw new JsonException($"{key} must be an object");
        var entries = new Dictionary<string, string>();
        foreach (var property in value.EnumerateObject())
        {
            if (property.Value.ValueKind != JsonValueKind.String)
                throw new JsonException($"{key} must contain strings");
            entries[property.Name] = property.Value.GetString()!;
        }
        return new ReadOnlyDictionary<string, string>(entries);
    }

    // An optional object whose values share one shape, keyed by caller-chosen ids.
    // Absent or null reads as null; a present value is decoded strictly.
    private static IReadOnlyDictionary<string, T>? OptionalObjectMap<T>(JsonElement parent, string key, Func<JsonElement, T> parse)
    {
        if (!Present(parent, key)) return null;
        var value = JsonRead.RequiredProperty(parent, key);
        if (value.ValueKind != JsonValueKind.Object) throw new JsonException($"{key} must be an object");
        var entries = new Dictionary<string, T>();
        foreach (var property in value.EnumerateObject())
            entries[property.Name] = parse(property.Value);
        return new ReadOnlyDictionary<string, T>(entries);
    }

    private static bool Present(JsonElement parent, string key) =>
        parent.TryGetProperty(key, out var value) && value.ValueKind != JsonValueKind.Null;

    private static IReadOnlyList<JsonElement?> CloneElementList(JsonElement array)
    {
        var values = new List<JsonElement?>();
        foreach (var value in array.EnumerateArray()) values.Add(value.Clone());
        return values.AsReadOnly();
    }

    private static JsonElement.ArrayEnumerator RequiredArray(JsonElement parent, string key)
    {
        var value = JsonRead.RequiredProperty(parent, key);
        if (value.ValueKind != JsonValueKind.Array) throw new JsonException($"{key} must be an array");
        return value.EnumerateArray();
    }

    private static IReadOnlyDictionary<string, Term>? ParseVocabularyElement(JsonElement el)
    {
        if (el.ValueKind != JsonValueKind.Object) return null;
        var vocabulary = new Dictionary<string, Term>();
        foreach (var property in el.EnumerateObject())
            vocabulary[property.Name] = ParseTermElement(property.Value);
        return new ReadOnlyDictionary<string, Term>(vocabulary);
    }

    private static Term ParseTermElement(JsonElement el) => new()
    {
        Singular = el.TryGetProperty("singular", out var s) ? s.GetString() ?? "" : "",
        Plural = el.TryGetProperty("plural", out var p) ? p.GetString() ?? "" : "",
    };

    // An audit's judgment of one marking. The facts and the recognition account
    // are what a caller rolls up or checks access from, so both are required;
    // the lists read an absent value as empty, as every result here does.
    private static MarkingValidation? ParseMarkingValidationElement(JsonElement el)
    {
        if (el.ValueKind != JsonValueKind.Object) return null;
        return new MarkingValidation
        {
            Assertions = JsonRead.RequiredList(el, "assertions", ResultParser.ReadAssertion),
            Recognition = ResultParser.ParseRecognitionResult(JsonRead.RequiredProperty(el, "recognition")),
            Valid = JsonRead.RequiredBool(el, "valid"),
            InputWasValid = JsonRead.RequiredBool(el, "inputWasValid"),
            Errors = JsonRead.RequiredList(el, "errors", ResultParser.ReadValidationError),
            ProposedFixes = JsonRead.RequiredList(el, "proposedFixes", ResultParser.ReadFix),
            AppliedFixes = JsonRead.RequiredList(el, "appliedFixes", ResultParser.ReadChange),
            Rendered = JsonRead.RequiredString(el, "rendered"),
        };
    }

}
