#nullable enable

using System.Collections.Generic;
using System.Text.Json;

namespace EzClassify;

/// <summary>A reference to one assertion in a rendered state.</summary>
public record AssertionRef
{
    /// <summary>The concept asserted.</summary>
    public required string ConceptId { get; init; }

    /// <summary>The category it was asserted in.</summary>
    public required string CategoryId { get; init; }

    /// <summary>Its value, for a concept that carries free text.</summary>
    public string? Value { get; init; }
}

/// <summary>Two assertions that claim the same render field.</summary>
public record RenderFieldConflict
{
    /// <summary>The field, by <c>name</c> or category id.</summary>
    public required string Field { get; init; }

    /// <summary>The assertions in conflict.</summary>
    public required IReadOnlyList<AssertionRef> Assertions { get; init; }
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
    public abstract string? Detail { get; init; }
}

/// <summary>A fact the selected format has no way to write.</summary>
public sealed record UnrepresentableFact : UnrepresentableValue
{
    /// <summary>The concept that cannot be written.</summary>
    public required string ConceptId { get; init; }

    /// <summary>Why the format cannot write it.</summary>
    public required UnrepresentableFactReason Reason { get; init; }

    /// <summary>Human-readable detail, for a message rather than a branch.</summary>
    public override string? Detail { get; init; }
}

/// <summary>
/// An output refused whole, before any of its facts was written. Only a scheme
/// that declares surfaces refuses one, because only its views choose a format
/// by condition.
/// </summary>
public sealed record UnrepresentableOutput : UnrepresentableValue
{
    /// <summary>The output refused, by its <c>surface/view</c> address.</summary>
    public required string Output { get; init; }

    /// <summary>Why it was refused.</summary>
    public required UnrepresentableOutputReason Reason { get; init; }

    /// <summary>Human-readable detail, for a message rather than a branch.</summary>
    public override string? Detail { get; init; }
}

/// <summary>Complete preservation accounting for one render.</summary>
public record RenderAssessment
{
    /// <summary>Facts this render wrote.</summary>
    public required IReadOnlyList<AssertionRef> Emitted { get; init; }

    /// <summary>Facts this render did not write.</summary>
    public required IReadOnlyList<AssertionRef> Omitted { get; init; }

    /// <summary>Groups of facts that collapsed into one written equivalent.</summary>
    public required IReadOnlyList<IReadOnlyList<AssertionRef>> Collapsed { get; init; }

    /// <summary>Fields that two assertions both claim.</summary>
    public required IReadOnlyList<RenderFieldConflict> Conflicts { get; init; }

    /// <summary>Facts that cannot be written in this format at all, and outputs refused whole.</summary>
    public required IReadOnlyList<UnrepresentableValue> Unrepresentable { get; init; }
}

/// <summary>A named render and its preservation assessment.</summary>
public record RenderResult
{
    /// <summary>The rendered marking.</summary>
    public required string Rendered { get; init; }

    /// <summary>What this render did to the state it was given.</summary>
    public required RenderAssessment Assessment { get; init; }
}

/// <summary>Rendered marking payload and its preservation assessment.</summary>
public record RenderResponse
{
    /// <summary>The rendered marking string.</summary>
    public required string Result { get; init; }

    /// <summary>What this render did to the state it was given: what was written, what was left out, what collapsed, what this format cannot write. Bound to this render, not to the format in the abstract.</summary>
    public required RenderAssessment Assessment { get; init; }
}

/// <summary>Resolved Context envelope returned by scheme processing, the author's call.</summary>
public record ContextSchemeProcessResult
{
    /// <summary>Single resolved composition used throughout the operation.</summary>
    public required ContextResolution Resolution { get; init; }

    /// <summary>Typed operation result.</summary>
    public required ProcessResult Result { get; init; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; init; }
}

/// <summary>Resolved Context envelope returned by scheme validation, the reader's call, and by label preset expansion.</summary>
public record ContextSchemeValidateResult
{
    /// <summary>Single resolved composition used throughout the operation.</summary>
    public required ContextResolution Resolution { get; init; }

    /// <summary>Typed operation result.</summary>
    public required ValidateResult Result { get; init; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; init; }
}

/// <summary>Resolved Context envelope returned by a scheme explanation: the trace of how the engine settled one marking.</summary>
public record ContextSchemeExplainResult
{
    /// <summary>Single resolved composition used throughout the operation.</summary>
    public required ContextResolution Resolution { get; init; }

    /// <summary>Typed operation result.</summary>
    public required ExplanationTrace Result { get; init; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; init; }
}

/// <summary>Resolved Context envelope returned by scheme suggestions.</summary>
public record ContextSchemeSuggestResult
{
    /// <summary>Single resolved composition used throughout the operation.</summary>
    public required ContextResolution Resolution { get; init; }

    /// <summary>Typed operation result.</summary>
    public required SuggestResult Result { get; init; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; init; }
}

/// <summary>
/// Resolved Context envelope returned by per-category scheme suggestions: one
/// <see cref="SuggestResult"/> per requested category id, the same map a process
/// call returns in <see cref="ProcessResult.SuggestionsFor"/>.
/// </summary>
public record ContextSchemeSuggestForCategoriesResult
{
    /// <summary>Single resolved composition used throughout the operation.</summary>
    public required ContextResolution Resolution { get; init; }

    /// <summary>Typed operation result.</summary>
    public required IReadOnlyDictionary<string, SuggestResult> Result { get; init; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; init; }
}

/// <summary>Resolved Context envelope returned by scheme aggregation.</summary>
public record ContextSchemeAggregateResult
{
    /// <summary>Single resolved composition used throughout the operation.</summary>
    public required ContextResolution Resolution { get; init; }

    /// <summary>Typed operation result.</summary>
    public required AggregateResult Result { get; init; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; init; }
}

/// <summary>What a scan answers: the markings it found, the wrapped marks it could
/// not read, and how far it got. An empty list is what a clean document and an
/// abandoned scan both look like, so the limit report travels with the results
/// rather than being inferred.</summary>
public record ScanResponseBody
{
    /// <summary>The markings the scan found, in document order.</summary>
    public required IReadOnlyList<ScanMatch> Results { get; init; }

    /// <summary>The wrapped marks the scan found but could not read, in document order.</summary>
    public required IReadOnlyList<UnreadMark> Unread { get; init; }

    /// <summary>How far the scan got, and what stopped it.</summary>
    public required ScanLimits Limits { get; init; }
}

/// <summary>Resolved Context envelope returned by scheme scans.</summary>
public record ContextSchemeScanResult
{
    /// <summary>Single resolved composition used throughout the operation.</summary>
    public required ContextResolution Resolution { get; init; }

    /// <summary>Typed operation result.</summary>
    public required ScanResponseBody Result { get; init; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; init; }
}

/// <summary>Resolved Context envelope returned by scheme rendering.</summary>
public record ContextSchemeRenderResult
{
    /// <summary>Single resolved composition used throughout the operation.</summary>
    public required ContextResolution Resolution { get; init; }

    /// <summary>Typed operation result.</summary>
    public required RenderResponse Result { get; init; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; init; }
}

/// <summary>Resolved Context envelope returned by scheme coverage checks.</summary>
public record ContextSchemeCoverageResult
{
    /// <summary>Single resolved composition used throughout the operation.</summary>
    public required ContextResolution Resolution { get; init; }

    /// <summary>Typed operation result.</summary>
    public required CoverageReport Result { get; init; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; init; }
}

/// <summary>Resolved Context envelope returned by concept descriptions.</summary>
public record ContextSchemeConceptDescriptionResult
{
    /// <summary>Single resolved composition used throughout the operation.</summary>
    public required ContextResolution Resolution { get; init; }

    /// <summary>Typed operation result.</summary>
    public required ConceptDescription Result { get; init; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; init; }
}

/// <summary>Resolved Context envelope returned by scheme diffs.</summary>
public record ContextSchemeDiffResult
{
    /// <summary>Single resolved composition used throughout the operation.</summary>
    public required ContextResolution Resolution { get; init; }

    /// <summary>Typed operation result.</summary>
    public required DiffResult Result { get; init; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; init; }
}

/// <summary>Resolved Context envelope returned by scheme notice collection.</summary>
public record ContextSchemeNoticesResult
{
    /// <summary>Single resolved composition used throughout the operation.</summary>
    public required ContextResolution Resolution { get; init; }

    /// <summary>Typed operation result.</summary>
    public required IReadOnlyList<Notice> Result { get; init; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; init; }
}

/// <summary>Resolved Context envelope returned by scheme structural views.</summary>
public record ContextSchemeViewResult
{
    /// <summary>Single resolved composition used throughout the operation.</summary>
    public required ContextResolution Resolution { get; init; }

    /// <summary>Typed operation result.</summary>
    public required SchemeView Result { get; init; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; init; }
}

/// <summary>Resolved Context envelope returned by guide scans.</summary>
public record ContextGuideScanResult
{
    /// <summary>Single resolved composition used throughout the operation.</summary>
    public required ContextResolution Resolution { get; init; }

    /// <summary>Typed operation result.</summary>
    public required IReadOnlyList<GuideScanResult> Result { get; init; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; init; }
}

/// <summary>Resolved Context envelope returned by guide evaluation.</summary>
public record ContextGuideEvaluationResult
{
    /// <summary>Single resolved composition used throughout the operation.</summary>
    public required ContextResolution Resolution { get; init; }

    /// <summary>Typed operation result.</summary>
    public required EvaluationResult Result { get; init; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; init; }
}

/// <summary>Resolved Context envelope returned by guide structural views.</summary>
public record ContextGuideViewResult
{
    /// <summary>Single resolved composition used throughout the operation.</summary>
    public required ContextResolution Resolution { get; init; }

    /// <summary>Typed operation result.</summary>
    public required GuideView Result { get; init; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; init; }
}

/// <summary>Resolved Context envelope returned by trusted-target access checks.</summary>
public record ContextAccessDecisionResult
{
    /// <summary>Single resolved composition used throughout the operation.</summary>
    public required ContextResolution Resolution { get; init; }

    /// <summary>Typed operation result.</summary>
    public required AccessDecision Result { get; init; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; init; }
}

/// <summary>Resolved Context envelope returned by access requirement descriptions.</summary>
public record ContextAccessRequirementsResult
{
    /// <summary>Single resolved composition used throughout the operation.</summary>
    public required ContextResolution Resolution { get; init; }

    /// <summary>Typed operation result.</summary>
    public required AccessRequirementsDescription Result { get; init; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; init; }
}

/// <summary>
/// Resolved Context envelope returned by directional crosswalk translation.
/// The translation result is preserved as contract-owned JSON because its
/// completed and forbidden branches are a discriminated union.
/// </summary>
public record ContextCrosswalkTranslationResult
{
    /// <summary>Single resolved composition used throughout the operation.</summary>
    public required ContextResolution Resolution { get; init; }

    /// <summary>Typed operation result.</summary>
    public required JsonElement Result { get; init; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection? ResponseProjection { get; init; }
}

/// <summary>
/// One artifact taking part in a resolved Context: the id and version the
/// Context declared, plus the concrete version those resolved to.
/// </summary>
public record ContextComponentRef
{
    /// <summary>Component id from the Context declaration.</summary>
    public required string Id { get; init; }

    /// <summary>Version literal the Context declares: a concrete semver or <c>latest</c>.</summary>
    public required string Version { get; init; }

    /// <summary>Concrete version after <c>latest</c> resolution.</summary>
    public required string ResolvedVersion { get; init; }

    /// <summary>Layers stacked beneath this component, in composition order. Empty when it has none.</summary>
    public required IReadOnlyList<ContextComponentRef> Layers { get; init; }
}

/// <summary>
/// One scheme a Context holds, and the selector that addresses it. Read it as a
/// sentence: this Context holds a scheme you select with <c>Selector</c>, it is
/// <c>Id</c> at <c>Version</c>, and it plays these <c>Roles</c>.
/// </summary>
public record ContextSchemeDescriptor
{
    /// <summary>Context-local key that selects this scheme. It is what <c>?scheme=</c> takes on the scheme resources, and it need not equal <c>Id</c>.</summary>
    public required string Selector { get; init; }

    /// <summary>Scheme id, from the scheme document itself.</summary>
    public required string Id { get; init; }

    /// <summary>Scheme version, from the scheme document itself.</summary>
    public required string Version { get; init; }

    /// <summary>What this scheme is to the Context. Never empty.</summary>
    public required IReadOnlyList<ContextSchemeRole> Roles { get; init; }
}

/// <summary>
/// What a Context is made of, as the server resolved it: the composed scheme,
/// every scheme held, the guides, the access policy and the crosswalks, each
/// with the version it declares and the concrete version that answered. An
/// integrator records these component versions to say which artifacts a
/// decision was made against.
/// </summary>
public record ContextDescription
{
    /// <summary>Context id.</summary>
    public required string Id { get; init; }

    /// <summary>Concrete Context version after <c>latest</c> resolution.</summary>
    public required string Version { get; init; }

    /// <summary>The composed scheme, the base of the layer stack. Its <c>Layers</c> are the Context's layers in composition order.</summary>
    public required ContextComponentRef Scheme { get; init; }

    /// <summary>Every scheme the Context holds, primary first, each with the selector that addresses it.</summary>
    public required IReadOnlyList<ContextSchemeDescriptor> Schemes { get; init; }

    /// <summary>Guide components in precedence order. Empty when the Context declares none.</summary>
    public required IReadOnlyList<ContextComponentRef> Guides { get; init; }

    /// <summary>Crosswalk components in declaration order, each with the layers it stacks and the concrete version that answered. Empty when the Context declares none.</summary>
    public required IReadOnlyList<ContextComponentRef> Crosswalks { get; init; }

    /// <summary>Access policy component, or null when the Context declares none.</summary>
    public ContextComponentRef? Access { get; init; }

    /// <summary>Human-readable description copied from the Context declaration, or null.</summary>
    public string? Description { get; init; }
}
