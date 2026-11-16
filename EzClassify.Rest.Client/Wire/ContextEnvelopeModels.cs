#nullable enable

using System.Collections.Generic;
using System.Text.Json;

namespace EzClassify;

/// <summary>A reference to one assertion in a rendered state.</summary>
public sealed record AssertionRef
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="conceptId">The concept asserted.</param>
    /// <param name="categoryId">The category it was asserted in.</param>
    public AssertionRef(string conceptId, string categoryId)
    {
        ConceptId = conceptId ?? throw new ArgumentNullException(nameof(conceptId));
        CategoryId = categoryId ?? throw new ArgumentNullException(nameof(categoryId));
    }

    /// <summary>The concept asserted.</summary>
    public string ConceptId { get; }

    /// <summary>The category it was asserted in.</summary>
    public string CategoryId { get; }

    /// <summary>Its value, for a concept that carries free text.</summary>
    public string? Value { get; set; }
}

/// <summary>Two assertions that claim the same render field.</summary>
public sealed record RenderFieldConflict
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="field">The field, by <c>name</c> or category id.</param>
    /// <param name="assertions">The assertions in conflict.</param>
    public RenderFieldConflict(string field, IReadOnlyList<AssertionRef> assertions)
    {
        Field = field ?? throw new ArgumentNullException(nameof(field));
        Assertions = assertions ?? throw new ArgumentNullException(nameof(assertions));
    }

    /// <summary>The field, by <c>name</c> or category id.</summary>
    public string Field { get; }

    /// <summary>The assertions in conflict.</summary>
    public IReadOnlyList<AssertionRef> Assertions { get; }
}

/// <summary>
/// Something a render could not write: one fact, as an
/// <see cref="UnrepresentableFact"/>, or a whole output, as an
/// <see cref="UnrepresentableOutput"/>. Match on the type to tell them apart;
/// only a fact names a concept.
/// </summary>
public abstract record UnrepresentableValue
{
    // The contract has exactly these two kinds, so the hierarchy stays closed
    // to this assembly: a caller that handles both has handled every kind.
    private protected UnrepresentableValue() { }

    /// <summary>Human-readable detail, for a message rather than a branch.</summary>
    public abstract string? Detail { get; set; }
}

/// <summary>A fact the selected format has no way to write.</summary>
public sealed record UnrepresentableFact : UnrepresentableValue
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="conceptId">The concept that cannot be written.</param>
    /// <param name="reason">Why the format cannot write it.</param>
    public UnrepresentableFact(string conceptId, UnrepresentableFactReason reason)
    {
        ConceptId = conceptId ?? throw new ArgumentNullException(nameof(conceptId));
        Reason = reason;
    }

    /// <summary>The concept that cannot be written.</summary>
    public string ConceptId { get; }

    /// <summary>Why the format cannot write it.</summary>
    public UnrepresentableFactReason Reason { get; }

    /// <summary>Human-readable detail, for a message rather than a branch.</summary>
    public override string? Detail { get; set; }
}

/// <summary>
/// An output refused whole, before any of its facts was written. Only a scheme
/// that declares surfaces refuses one, because only its views choose a format
/// by condition.
/// </summary>
public sealed record UnrepresentableOutput : UnrepresentableValue
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="output">The output refused, by its <c>surface/view</c> address.</param>
    /// <param name="reason">Why it was refused.</param>
    public UnrepresentableOutput(string output, UnrepresentableOutputReason reason)
    {
        Output = output ?? throw new ArgumentNullException(nameof(output));
        Reason = reason;
    }

    /// <summary>The output refused, by its <c>surface/view</c> address.</summary>
    public string Output { get; }

    /// <summary>Why it was refused.</summary>
    public UnrepresentableOutputReason Reason { get; }

    /// <summary>Human-readable detail, for a message rather than a branch.</summary>
    public override string? Detail { get; set; }
}

/// <summary>Complete preservation accounting for one render.</summary>
public sealed record RenderAssessment
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="emitted">Facts this render wrote.</param>
    /// <param name="omitted">Facts this render did not write.</param>
    /// <param name="collapsed">Groups of facts that collapsed into one written equivalent.</param>
    /// <param name="conflicts">Fields that two assertions both claim.</param>
    /// <param name="unrepresentable">Facts that cannot be written in this format at all, and outputs refused whole.</param>
    public RenderAssessment(IReadOnlyList<AssertionRef> emitted, IReadOnlyList<AssertionRef> omitted, IReadOnlyList<IReadOnlyList<AssertionRef>> collapsed, IReadOnlyList<RenderFieldConflict> conflicts, IReadOnlyList<UnrepresentableValue> unrepresentable)
    {
        Emitted = emitted ?? throw new ArgumentNullException(nameof(emitted));
        Omitted = omitted ?? throw new ArgumentNullException(nameof(omitted));
        Collapsed = collapsed ?? throw new ArgumentNullException(nameof(collapsed));
        Conflicts = conflicts ?? throw new ArgumentNullException(nameof(conflicts));
        Unrepresentable = unrepresentable ?? throw new ArgumentNullException(nameof(unrepresentable));
    }

    /// <summary>Facts this render wrote.</summary>
    public IReadOnlyList<AssertionRef> Emitted { get; }

    /// <summary>Facts this render did not write.</summary>
    public IReadOnlyList<AssertionRef> Omitted { get; }

    /// <summary>Groups of facts that collapsed into one written equivalent.</summary>
    public IReadOnlyList<IReadOnlyList<AssertionRef>> Collapsed { get; }

    /// <summary>Fields that two assertions both claim.</summary>
    public IReadOnlyList<RenderFieldConflict> Conflicts { get; }

    /// <summary>Facts that cannot be written in this format at all, and outputs refused whole.</summary>
    public IReadOnlyList<UnrepresentableValue> Unrepresentable { get; }
}

/// <summary>A named render and its preservation assessment.</summary>
public sealed record RenderResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="rendered">The rendered marking.</param>
    /// <param name="assessment">What this render did to the state it was given.</param>
    public RenderResult(string rendered, RenderAssessment assessment)
    {
        Rendered = rendered ?? throw new ArgumentNullException(nameof(rendered));
        Assessment = assessment ?? throw new ArgumentNullException(nameof(assessment));
    }

    /// <summary>The rendered marking.</summary>
    public string Rendered { get; }

    /// <summary>What this render did to the state it was given.</summary>
    public RenderAssessment Assessment { get; }
}

/// <summary>Rendered marking payload and its preservation assessment.</summary>
public sealed record RenderResponse
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="result">The rendered marking string.</param>
    /// <param name="assessment">What this render did to the state it was given: what was written, what was left out, what collapsed, what this format cannot write. Bound to this render, not to the format in the abstract.</param>
    public RenderResponse(string result, RenderAssessment assessment)
    {
        Result = result ?? throw new ArgumentNullException(nameof(result));
        Assessment = assessment ?? throw new ArgumentNullException(nameof(assessment));
    }

    /// <summary>The rendered marking string.</summary>
    public string Result { get; }

    /// <summary>What this render did to the state it was given: what was written, what was left out, what collapsed, what this format cannot write. Bound to this render, not to the format in the abstract.</summary>
    public RenderAssessment Assessment { get; }
}

/// <summary>Resolved Context envelope returned by scheme processing, the author's call.</summary>
public sealed record ContextSchemeProcessResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="resolution">Single resolved composition used throughout the operation.</param>
    /// <param name="result">Typed operation result.</param>
    public ContextSchemeProcessResult(ContextResolution resolution, ProcessResult result)
    {
        Resolution = resolution ?? throw new ArgumentNullException(nameof(resolution));
        Result = result ?? throw new ArgumentNullException(nameof(result));
    }

    /// <summary>Single resolved composition used throughout the operation.</summary>
    public ContextResolution Resolution { get; }

    /// <summary>Typed operation result.</summary>
    public ProcessResult Result { get; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; set; }
}

/// <summary>Resolved Context envelope returned by scheme validation, the reader's call, and by label preset expansion.</summary>
public sealed record ContextSchemeValidateResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="resolution">Single resolved composition used throughout the operation.</param>
    /// <param name="result">Typed operation result.</param>
    public ContextSchemeValidateResult(ContextResolution resolution, ValidateResult result)
    {
        Resolution = resolution ?? throw new ArgumentNullException(nameof(resolution));
        Result = result ?? throw new ArgumentNullException(nameof(result));
    }

    /// <summary>Single resolved composition used throughout the operation.</summary>
    public ContextResolution Resolution { get; }

    /// <summary>Typed operation result.</summary>
    public ValidateResult Result { get; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; set; }
}

/// <summary>Resolved Context envelope returned by a scheme explanation: the trace of how the engine settled one marking.</summary>
public sealed record ContextSchemeExplainResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="resolution">Single resolved composition used throughout the operation.</param>
    /// <param name="result">Typed operation result.</param>
    public ContextSchemeExplainResult(ContextResolution resolution, ExplanationTrace result)
    {
        Resolution = resolution ?? throw new ArgumentNullException(nameof(resolution));
        Result = result ?? throw new ArgumentNullException(nameof(result));
    }

    /// <summary>Single resolved composition used throughout the operation.</summary>
    public ContextResolution Resolution { get; }

    /// <summary>Typed operation result.</summary>
    public ExplanationTrace Result { get; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; set; }
}

/// <summary>Resolved Context envelope returned by scheme suggestions.</summary>
public sealed record ContextSchemeSuggestResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="resolution">Single resolved composition used throughout the operation.</param>
    /// <param name="result">Typed operation result.</param>
    public ContextSchemeSuggestResult(ContextResolution resolution, SuggestResult result)
    {
        Resolution = resolution ?? throw new ArgumentNullException(nameof(resolution));
        Result = result ?? throw new ArgumentNullException(nameof(result));
    }

    /// <summary>Single resolved composition used throughout the operation.</summary>
    public ContextResolution Resolution { get; }

    /// <summary>Typed operation result.</summary>
    public SuggestResult Result { get; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; set; }
}

/// <summary>
/// Resolved Context envelope returned by per-category scheme suggestions: one
/// <see cref="SuggestResult"/> per requested category id, the same map a process
/// call returns in <see cref="ProcessResult.SuggestionsFor"/>.
/// </summary>
public sealed record ContextSchemeSuggestForCategoriesResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="resolution">Single resolved composition used throughout the operation.</param>
    /// <param name="result">Typed operation result.</param>
    public ContextSchemeSuggestForCategoriesResult(ContextResolution resolution, IReadOnlyDictionary<string, SuggestResult> result)
    {
        Resolution = resolution ?? throw new ArgumentNullException(nameof(resolution));
        Result = result ?? throw new ArgumentNullException(nameof(result));
    }

    /// <summary>Single resolved composition used throughout the operation.</summary>
    public ContextResolution Resolution { get; }

    /// <summary>Typed operation result.</summary>
    public IReadOnlyDictionary<string, SuggestResult> Result { get; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; set; }
}

/// <summary>Resolved Context envelope returned by scheme aggregation.</summary>
public sealed record ContextSchemeAggregateResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="resolution">Single resolved composition used throughout the operation.</param>
    /// <param name="result">Typed operation result.</param>
    public ContextSchemeAggregateResult(ContextResolution resolution, AggregateResult result)
    {
        Resolution = resolution ?? throw new ArgumentNullException(nameof(resolution));
        Result = result ?? throw new ArgumentNullException(nameof(result));
    }

    /// <summary>Single resolved composition used throughout the operation.</summary>
    public ContextResolution Resolution { get; }

    /// <summary>Typed operation result.</summary>
    public AggregateResult Result { get; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; set; }
}

/// <summary>What a scan answers: the markings it found, the wrapped marks it could
/// not read, and how far it got. An empty list is what a clean document and an
/// abandoned scan both look like, so the limit report travels with the results
/// rather than being inferred.</summary>
public sealed record ScanResponseBody
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="results">The markings the scan found, in document order.</param>
    /// <param name="unread">The wrapped marks the scan found but could not read, in document order.</param>
    /// <param name="limits">How far the scan got, and what stopped it.</param>
    public ScanResponseBody(IReadOnlyList<ScanMatch> results, IReadOnlyList<UnreadMark> unread, ScanLimits limits)
    {
        Results = results ?? throw new ArgumentNullException(nameof(results));
        Unread = unread ?? throw new ArgumentNullException(nameof(unread));
        Limits = limits ?? throw new ArgumentNullException(nameof(limits));
    }

    /// <summary>The markings the scan found, in document order.</summary>
    public IReadOnlyList<ScanMatch> Results { get; }

    /// <summary>The wrapped marks the scan found but could not read, in document order.</summary>
    public IReadOnlyList<UnreadMark> Unread { get; }

    /// <summary>How far the scan got, and what stopped it.</summary>
    public ScanLimits Limits { get; }
}

/// <summary>Resolved Context envelope returned by scheme scans.</summary>
public sealed record ContextSchemeScanResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="resolution">Single resolved composition used throughout the operation.</param>
    /// <param name="result">Typed operation result.</param>
    public ContextSchemeScanResult(ContextResolution resolution, ScanResponseBody result)
    {
        Resolution = resolution ?? throw new ArgumentNullException(nameof(resolution));
        Result = result ?? throw new ArgumentNullException(nameof(result));
    }

    /// <summary>Single resolved composition used throughout the operation.</summary>
    public ContextResolution Resolution { get; }

    /// <summary>Typed operation result.</summary>
    public ScanResponseBody Result { get; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; set; }
}

/// <summary>Resolved Context envelope returned by scheme rendering.</summary>
public sealed record ContextSchemeRenderResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="resolution">Single resolved composition used throughout the operation.</param>
    /// <param name="result">Typed operation result.</param>
    public ContextSchemeRenderResult(ContextResolution resolution, RenderResponse result)
    {
        Resolution = resolution ?? throw new ArgumentNullException(nameof(resolution));
        Result = result ?? throw new ArgumentNullException(nameof(result));
    }

    /// <summary>Single resolved composition used throughout the operation.</summary>
    public ContextResolution Resolution { get; }

    /// <summary>Typed operation result.</summary>
    public RenderResponse Result { get; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; set; }
}

/// <summary>Resolved Context envelope returned by scheme coverage checks.</summary>
public sealed record ContextSchemeCoverageResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="resolution">Single resolved composition used throughout the operation.</param>
    /// <param name="result">Typed operation result.</param>
    public ContextSchemeCoverageResult(ContextResolution resolution, CoverageReport result)
    {
        Resolution = resolution ?? throw new ArgumentNullException(nameof(resolution));
        Result = result ?? throw new ArgumentNullException(nameof(result));
    }

    /// <summary>Single resolved composition used throughout the operation.</summary>
    public ContextResolution Resolution { get; }

    /// <summary>Typed operation result.</summary>
    public CoverageReport Result { get; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; set; }
}

/// <summary>Resolved Context envelope returned by concept descriptions.</summary>
public sealed record ContextSchemeConceptDescriptionResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="resolution">Single resolved composition used throughout the operation.</param>
    /// <param name="result">Typed operation result.</param>
    public ContextSchemeConceptDescriptionResult(ContextResolution resolution, ConceptDescription result)
    {
        Resolution = resolution ?? throw new ArgumentNullException(nameof(resolution));
        Result = result ?? throw new ArgumentNullException(nameof(result));
    }

    /// <summary>Single resolved composition used throughout the operation.</summary>
    public ContextResolution Resolution { get; }

    /// <summary>Typed operation result.</summary>
    public ConceptDescription Result { get; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; set; }
}

/// <summary>Resolved Context envelope returned by scheme diffs.</summary>
public sealed record ContextSchemeDiffResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="resolution">Single resolved composition used throughout the operation.</param>
    /// <param name="result">Typed operation result.</param>
    public ContextSchemeDiffResult(ContextResolution resolution, DiffResult result)
    {
        Resolution = resolution ?? throw new ArgumentNullException(nameof(resolution));
        Result = result ?? throw new ArgumentNullException(nameof(result));
    }

    /// <summary>Single resolved composition used throughout the operation.</summary>
    public ContextResolution Resolution { get; }

    /// <summary>Typed operation result.</summary>
    public DiffResult Result { get; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; set; }
}

/// <summary>Resolved Context envelope returned by scheme notice collection.</summary>
public sealed record ContextSchemeNoticesResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="resolution">Single resolved composition used throughout the operation.</param>
    /// <param name="result">Typed operation result.</param>
    public ContextSchemeNoticesResult(ContextResolution resolution, IReadOnlyList<Notice> result)
    {
        Resolution = resolution ?? throw new ArgumentNullException(nameof(resolution));
        Result = result ?? throw new ArgumentNullException(nameof(result));
    }

    /// <summary>Single resolved composition used throughout the operation.</summary>
    public ContextResolution Resolution { get; }

    /// <summary>Typed operation result.</summary>
    public IReadOnlyList<Notice> Result { get; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; set; }
}

/// <summary>Resolved Context envelope returned by scheme structural views.</summary>
public sealed record ContextSchemeViewResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="resolution">Single resolved composition used throughout the operation.</param>
    /// <param name="result">Typed operation result.</param>
    public ContextSchemeViewResult(ContextResolution resolution, SchemeView result)
    {
        Resolution = resolution ?? throw new ArgumentNullException(nameof(resolution));
        Result = result ?? throw new ArgumentNullException(nameof(result));
    }

    /// <summary>Single resolved composition used throughout the operation.</summary>
    public ContextResolution Resolution { get; }

    /// <summary>Typed operation result.</summary>
    public SchemeView Result { get; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; set; }
}

/// <summary>Resolved Context envelope returned by guide scans.</summary>
public sealed record ContextGuideScanResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="resolution">Single resolved composition used throughout the operation.</param>
    /// <param name="result">Typed operation result.</param>
    public ContextGuideScanResult(ContextResolution resolution, IReadOnlyList<GuideScanResult> result)
    {
        Resolution = resolution ?? throw new ArgumentNullException(nameof(resolution));
        Result = result ?? throw new ArgumentNullException(nameof(result));
    }

    /// <summary>Single resolved composition used throughout the operation.</summary>
    public ContextResolution Resolution { get; }

    /// <summary>Typed operation result.</summary>
    public IReadOnlyList<GuideScanResult> Result { get; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; set; }
}

/// <summary>Resolved Context envelope returned by guide evaluation.</summary>
public sealed record ContextGuideEvaluationResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="resolution">Single resolved composition used throughout the operation.</param>
    /// <param name="result">Typed operation result.</param>
    public ContextGuideEvaluationResult(ContextResolution resolution, EvaluationResult result)
    {
        Resolution = resolution ?? throw new ArgumentNullException(nameof(resolution));
        Result = result ?? throw new ArgumentNullException(nameof(result));
    }

    /// <summary>Single resolved composition used throughout the operation.</summary>
    public ContextResolution Resolution { get; }

    /// <summary>Typed operation result.</summary>
    public EvaluationResult Result { get; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; set; }
}

/// <summary>Resolved Context envelope returned by guide structural views.</summary>
public sealed record ContextGuideViewResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="resolution">Single resolved composition used throughout the operation.</param>
    /// <param name="result">Typed operation result.</param>
    public ContextGuideViewResult(ContextResolution resolution, GuideView result)
    {
        Resolution = resolution ?? throw new ArgumentNullException(nameof(resolution));
        Result = result ?? throw new ArgumentNullException(nameof(result));
    }

    /// <summary>Single resolved composition used throughout the operation.</summary>
    public ContextResolution Resolution { get; }

    /// <summary>Typed operation result.</summary>
    public GuideView Result { get; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; set; }
}

/// <summary>Resolved Context envelope returned by trusted-target access checks.</summary>
public sealed record ContextAccessDecisionResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="resolution">Single resolved composition used throughout the operation.</param>
    /// <param name="result">Typed operation result.</param>
    public ContextAccessDecisionResult(ContextResolution resolution, AccessDecision result)
    {
        Resolution = resolution ?? throw new ArgumentNullException(nameof(resolution));
        Result = result ?? throw new ArgumentNullException(nameof(result));
    }

    /// <summary>Single resolved composition used throughout the operation.</summary>
    public ContextResolution Resolution { get; }

    /// <summary>Typed operation result.</summary>
    public AccessDecision Result { get; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; set; }
}

/// <summary>Resolved Context envelope returned by access requirement descriptions.</summary>
public sealed record ContextAccessRequirementsResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="resolution">Single resolved composition used throughout the operation.</param>
    /// <param name="result">Typed operation result.</param>
    public ContextAccessRequirementsResult(ContextResolution resolution, AccessRequirementsDescription result)
    {
        Resolution = resolution ?? throw new ArgumentNullException(nameof(resolution));
        Result = result ?? throw new ArgumentNullException(nameof(result));
    }

    /// <summary>Single resolved composition used throughout the operation.</summary>
    public ContextResolution Resolution { get; }

    /// <summary>Typed operation result.</summary>
    public AccessRequirementsDescription Result { get; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; set; }
}

/// <summary>
/// Resolved Context envelope returned by directional crosswalk translation.
/// The translation result is preserved as contract-owned JSON because its
/// completed and forbidden branches are a discriminated union.
/// </summary>
public sealed record ContextCrosswalkTranslationResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="resolution">Single resolved composition used throughout the operation.</param>
    /// <param name="result">Typed operation result.</param>
    public ContextCrosswalkTranslationResult(ContextResolution resolution, JsonElement result)
    {
        Resolution = resolution ?? throw new ArgumentNullException(nameof(resolution));
        Result = result;
    }

    /// <summary>Single resolved composition used throughout the operation.</summary>
    public ContextResolution Resolution { get; }

    /// <summary>Typed operation result.</summary>
    public JsonElement Result { get; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; set; }
}

/// <summary>
/// One artifact taking part in a resolved Context: the id and version the
/// Context declared, plus the concrete version those resolved to.
/// </summary>
public sealed record ContextComponentRef
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="id">Component id from the Context declaration.</param>
    /// <param name="version">Version literal the Context declares: a concrete semver or <c>latest</c>.</param>
    /// <param name="resolvedVersion">Concrete version after <c>latest</c> resolution.</param>
    /// <param name="layers">Layers stacked beneath this component, in composition order. Empty when it has none.</param>
    public ContextComponentRef(string id, string version, string resolvedVersion, IReadOnlyList<ContextComponentRef> layers)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Version = version ?? throw new ArgumentNullException(nameof(version));
        ResolvedVersion = resolvedVersion ?? throw new ArgumentNullException(nameof(resolvedVersion));
        Layers = layers ?? throw new ArgumentNullException(nameof(layers));
    }

    /// <summary>Component id from the Context declaration.</summary>
    public string Id { get; }

    /// <summary>Version literal the Context declares: a concrete semver or <c>latest</c>.</summary>
    public string Version { get; }

    /// <summary>Concrete version after <c>latest</c> resolution.</summary>
    public string ResolvedVersion { get; }

    /// <summary>Layers stacked beneath this component, in composition order. Empty when it has none.</summary>
    public IReadOnlyList<ContextComponentRef> Layers { get; }
}

/// <summary>
/// One scheme a Context holds, and the selector that addresses it. Read it as a
/// sentence: this Context holds a scheme you select with <c>Selector</c>, it is
/// <c>Id</c> at <c>Version</c>, and it plays these <c>Roles</c>.
/// </summary>
public sealed record ContextSchemeDescriptor
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="selector">Context-local key that selects this scheme. It is what <c>?scheme=</c> takes on the scheme resources, and it need not equal <c>Id</c>.</param>
    /// <param name="id">Scheme id, from the scheme document itself.</param>
    /// <param name="version">Scheme version, from the scheme document itself.</param>
    /// <param name="roles">What this scheme is to the Context. Never empty.</param>
    public ContextSchemeDescriptor(string selector, string id, string version, IReadOnlyList<ContextSchemeRole> roles)
    {
        Selector = selector ?? throw new ArgumentNullException(nameof(selector));
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Version = version ?? throw new ArgumentNullException(nameof(version));
        Roles = roles ?? throw new ArgumentNullException(nameof(roles));
    }

    /// <summary>Context-local key that selects this scheme. It is what <c>?scheme=</c> takes on the scheme resources, and it need not equal <c>Id</c>.</summary>
    public string Selector { get; }

    /// <summary>Scheme id, from the scheme document itself.</summary>
    public string Id { get; }

    /// <summary>Scheme version, from the scheme document itself.</summary>
    public string Version { get; }

    /// <summary>What this scheme is to the Context. Never empty.</summary>
    public IReadOnlyList<ContextSchemeRole> Roles { get; }
}

/// <summary>
/// What a Context is made of, as the server resolved it: the composed scheme,
/// every scheme held, the guides, the access policy and the crosswalks, each
/// with the version it declares and the concrete version that answered. An
/// integrator records these component versions to say which artifacts a
/// decision was made against.
/// </summary>
public sealed record ContextDescription
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="id">Context id.</param>
    /// <param name="version">Concrete Context version after <c>latest</c> resolution.</param>
    /// <param name="scheme">The composed scheme, the base of the layer stack. Its <c>Layers</c> are the Context's layers in composition order.</param>
    /// <param name="schemes">Every scheme the Context holds, primary first, each with the selector that addresses it.</param>
    /// <param name="guides">Guide components in precedence order. Empty when the Context declares none.</param>
    /// <param name="crosswalks">Crosswalk components in declaration order, each with the layers it stacks and the concrete version that answered. Empty when the Context declares none.</param>
    public ContextDescription(string id, string version, ContextComponentRef scheme, IReadOnlyList<ContextSchemeDescriptor> schemes, IReadOnlyList<ContextComponentRef> guides, IReadOnlyList<ContextComponentRef> crosswalks)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Version = version ?? throw new ArgumentNullException(nameof(version));
        Scheme = scheme ?? throw new ArgumentNullException(nameof(scheme));
        Schemes = schemes ?? throw new ArgumentNullException(nameof(schemes));
        Guides = guides ?? throw new ArgumentNullException(nameof(guides));
        Crosswalks = crosswalks ?? throw new ArgumentNullException(nameof(crosswalks));
    }

    /// <summary>Context id.</summary>
    public string Id { get; }

    /// <summary>Concrete Context version after <c>latest</c> resolution.</summary>
    public string Version { get; }

    /// <summary>The composed scheme, the base of the layer stack. Its <c>Layers</c> are the Context's layers in composition order.</summary>
    public ContextComponentRef Scheme { get; }

    /// <summary>Every scheme the Context holds, primary first, each with the selector that addresses it.</summary>
    public IReadOnlyList<ContextSchemeDescriptor> Schemes { get; }

    /// <summary>Guide components in precedence order. Empty when the Context declares none.</summary>
    public IReadOnlyList<ContextComponentRef> Guides { get; }

    /// <summary>Crosswalk components in declaration order, each with the layers it stacks and the concrete version that answered. Empty when the Context declares none.</summary>
    public IReadOnlyList<ContextComponentRef> Crosswalks { get; }

    /// <summary>Access policy component, or null when the Context declares none.</summary>
    public ContextComponentRef? Access { get; set; }

    /// <summary>Human-readable description copied from the Context declaration, or null.</summary>
    public string? Description { get; set; }
}
