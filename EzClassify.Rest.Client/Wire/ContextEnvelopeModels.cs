#nullable enable

using System.Collections.Generic;
using System.Text.Json;

namespace EzClassify;

/// <summary>A reference to one assertion in a rendered state.</summary>
/// <param name="ConceptId">The concept asserted.</param>
/// <param name="CategoryId">The category it was asserted in.</param>
/// <param name="Value">Its value, for a concept that carries free text.</param>
public record AssertionRef(string ConceptId, string CategoryId, string? Value = null);

/// <summary>Two assertions that claim the same render field.</summary>
/// <param name="Field">The field, by <c>name</c> or category id.</param>
/// <param name="Assertions">The assertions in conflict.</param>
public record RenderFieldConflict(string Field, IReadOnlyList<AssertionRef> Assertions);

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
/// <param name="ConceptId">The concept that cannot be written.</param>
/// <param name="Reason">Why the format cannot write it.</param>
/// <param name="Detail">Human-readable detail, for a message rather than a branch.</param>
public sealed record UnrepresentableFact(
    string ConceptId,
    UnrepresentableFactReason Reason,
    string? Detail = null) : UnrepresentableValue;

/// <summary>
/// An output refused whole, before any of its facts was written. Only a scheme
/// that declares surfaces refuses one, because only its views choose a format
/// by condition.
/// </summary>
/// <param name="Output">The output refused, by its <c>surface/view</c> address.</param>
/// <param name="Reason">Why it was refused.</param>
/// <param name="Detail">Human-readable detail, for a message rather than a branch.</param>
public sealed record UnrepresentableOutput(
    string Output,
    UnrepresentableOutputReason Reason,
    string? Detail = null) : UnrepresentableValue;

/// <summary>Complete preservation accounting for one render.</summary>
/// <param name="Emitted">Facts this render wrote.</param>
/// <param name="Omitted">Facts this render did not write.</param>
/// <param name="Collapsed">Groups of facts that collapsed into one written equivalent.</param>
/// <param name="Conflicts">Fields that two assertions both claim.</param>
/// <param name="Unrepresentable">Facts that cannot be written in this format at all, and outputs refused whole.</param>
public record RenderAssessment(
    IReadOnlyList<AssertionRef> Emitted,
    IReadOnlyList<AssertionRef> Omitted,
    IReadOnlyList<IReadOnlyList<AssertionRef>> Collapsed,
    IReadOnlyList<RenderFieldConflict> Conflicts,
    IReadOnlyList<UnrepresentableValue> Unrepresentable);

/// <summary>A named render and its preservation assessment.</summary>
/// <param name="Rendered">The rendered marking.</param>
/// <param name="Assessment">What this render did to the state it was given.</param>
public record RenderResult(string Rendered, RenderAssessment Assessment);

/// <summary>Rendered marking payload and its preservation assessment.</summary>
/// <param name="Result">The rendered marking string.</param>
/// <param name="Assessment">What this render did to the state it was given: what was written, what was left out, what collapsed, what this format cannot write. Bound to this render, not to the format in the abstract.</param>
public record RenderResponse(string Result, RenderAssessment Assessment);

/// <summary>Resolved Context envelope returned by scheme processing, the author's call.</summary>
/// <param name="Resolution">Single resolved composition used throughout the operation.</param>
/// <param name="Result">Typed operation result.</param>
/// <param name="ResponseProjection">Projection metadata, present only for document operations.</param>
public record ContextSchemeProcessResult(ContextResolution Resolution, ProcessResult Result, DocumentResponseProjection? ResponseProjection = null);

/// <summary>Resolved Context envelope returned by scheme validation, the reader's call, and by label preset expansion.</summary>
/// <param name="Resolution">Single resolved composition used throughout the operation.</param>
/// <param name="Result">Typed operation result.</param>
/// <param name="ResponseProjection">Projection metadata, present only for document operations.</param>
public record ContextSchemeValidateResult(ContextResolution Resolution, ValidateResult Result, DocumentResponseProjection? ResponseProjection = null);

/// <summary>Resolved Context envelope returned by a scheme explanation: the trace of how the engine settled one marking.</summary>
/// <param name="Resolution">Single resolved composition used throughout the operation.</param>
/// <param name="Result">Typed operation result.</param>
/// <param name="ResponseProjection">Projection metadata, present only for document operations.</param>
public record ContextSchemeExplainResult(ContextResolution Resolution, ExplanationTrace Result, DocumentResponseProjection? ResponseProjection = null);

/// <summary>Resolved Context envelope returned by scheme suggestions.</summary>
/// <param name="Resolution">Single resolved composition used throughout the operation.</param>
/// <param name="Result">Typed operation result.</param>
/// <param name="ResponseProjection">Projection metadata, present only for document operations.</param>
public record ContextSchemeSuggestResult(ContextResolution Resolution, SuggestResult Result, DocumentResponseProjection? ResponseProjection = null);

/// <summary>
/// Resolved Context envelope returned by per-category scheme suggestions: one
/// <see cref="SuggestResult"/> per requested category id, the same map a process
/// call returns in <see cref="ProcessResult.SuggestionsFor"/>.
/// </summary>
/// <param name="Resolution">Single resolved composition used throughout the operation.</param>
/// <param name="Result">Typed operation result.</param>
/// <param name="ResponseProjection">Projection metadata, present only for document operations.</param>
public record ContextSchemeSuggestForCategoriesResult(ContextResolution Resolution, IReadOnlyDictionary<string, SuggestResult> Result, DocumentResponseProjection? ResponseProjection = null);

/// <summary>Resolved Context envelope returned by scheme aggregation.</summary>
/// <param name="Resolution">Single resolved composition used throughout the operation.</param>
/// <param name="Result">Typed operation result.</param>
/// <param name="ResponseProjection">Projection metadata, present only for document operations.</param>
public record ContextSchemeAggregateResult(ContextResolution Resolution, AggregateResult Result, DocumentResponseProjection? ResponseProjection = null);

/// <summary>What a scan answers: the markings it found, the wrapped marks it could
/// not read, and how far it got. An empty list is what a clean document and an
/// abandoned scan both look like, so the limit report travels with the results
/// rather than being inferred.</summary>
/// <param name="Results">The markings the scan found, in document order.</param>
/// <param name="Unread">The wrapped marks the scan found but could not read, in document order.</param>
/// <param name="Limits">How far the scan got, and what stopped it.</param>
public record ScanResponseBody(IReadOnlyList<ScanMatch> Results, IReadOnlyList<UnreadMark> Unread, ScanLimits Limits);

/// <summary>Resolved Context envelope returned by scheme scans.</summary>
/// <param name="Resolution">Single resolved composition used throughout the operation.</param>
/// <param name="Result">Typed operation result.</param>
/// <param name="ResponseProjection">Projection metadata, present only for document operations.</param>
public record ContextSchemeScanResult(ContextResolution Resolution, ScanResponseBody Result, DocumentResponseProjection? ResponseProjection = null);

/// <summary>Resolved Context envelope returned by scheme rendering.</summary>
/// <param name="Resolution">Single resolved composition used throughout the operation.</param>
/// <param name="Result">Typed operation result.</param>
/// <param name="ResponseProjection">Projection metadata, present only for document operations.</param>
public record ContextSchemeRenderResult(ContextResolution Resolution, RenderResponse Result, DocumentResponseProjection? ResponseProjection = null);

/// <summary>Resolved Context envelope returned by scheme coverage checks.</summary>
/// <param name="Resolution">Single resolved composition used throughout the operation.</param>
/// <param name="Result">Typed operation result.</param>
/// <param name="ResponseProjection">Projection metadata, present only for document operations.</param>
public record ContextSchemeCoverageResult(ContextResolution Resolution, CoverageReport Result, DocumentResponseProjection? ResponseProjection = null);

/// <summary>Resolved Context envelope returned by concept descriptions.</summary>
/// <param name="Resolution">Single resolved composition used throughout the operation.</param>
/// <param name="Result">Typed operation result.</param>
/// <param name="ResponseProjection">Projection metadata, present only for document operations.</param>
public record ContextSchemeConceptDescriptionResult(ContextResolution Resolution, ConceptDescription Result, DocumentResponseProjection? ResponseProjection = null);

/// <summary>Resolved Context envelope returned by scheme diffs.</summary>
/// <param name="Resolution">Single resolved composition used throughout the operation.</param>
/// <param name="Result">Typed operation result.</param>
/// <param name="ResponseProjection">Projection metadata, present only for document operations.</param>
public record ContextSchemeDiffResult(ContextResolution Resolution, DiffResult Result, DocumentResponseProjection? ResponseProjection = null);

/// <summary>Resolved Context envelope returned by scheme notice collection.</summary>
/// <param name="Resolution">Single resolved composition used throughout the operation.</param>
/// <param name="Result">Typed operation result.</param>
/// <param name="ResponseProjection">Projection metadata, present only for document operations.</param>
public record ContextSchemeNoticesResult(ContextResolution Resolution, IReadOnlyList<Notice> Result, DocumentResponseProjection? ResponseProjection = null);

/// <summary>Resolved Context envelope returned by scheme structural views.</summary>
/// <param name="Resolution">Single resolved composition used throughout the operation.</param>
/// <param name="Result">Typed operation result.</param>
/// <param name="ResponseProjection">Projection metadata, present only for document operations.</param>
public record ContextSchemeViewResult(ContextResolution Resolution, SchemeView Result, DocumentResponseProjection? ResponseProjection = null);

/// <summary>Resolved Context envelope returned by guide scans.</summary>
/// <param name="Resolution">Single resolved composition used throughout the operation.</param>
/// <param name="Result">Typed operation result.</param>
/// <param name="ResponseProjection">Projection metadata, present only for document operations.</param>
public record ContextGuideScanResult(ContextResolution Resolution, IReadOnlyList<GuideScanResult> Result, DocumentResponseProjection? ResponseProjection = null);

/// <summary>Resolved Context envelope returned by guide evaluation.</summary>
/// <param name="Resolution">Single resolved composition used throughout the operation.</param>
/// <param name="Result">Typed operation result.</param>
/// <param name="ResponseProjection">Projection metadata, present only for document operations.</param>
public record ContextGuideEvaluationResult(ContextResolution Resolution, EvaluationResult Result, DocumentResponseProjection? ResponseProjection = null);

/// <summary>Resolved Context envelope returned by guide structural views.</summary>
/// <param name="Resolution">Single resolved composition used throughout the operation.</param>
/// <param name="Result">Typed operation result.</param>
/// <param name="ResponseProjection">Projection metadata, present only for document operations.</param>
public record ContextGuideViewResult(ContextResolution Resolution, GuideView Result, DocumentResponseProjection? ResponseProjection = null);

/// <summary>Resolved Context envelope returned by trusted-target access checks.</summary>
/// <param name="Resolution">Single resolved composition used throughout the operation.</param>
/// <param name="Result">Typed operation result.</param>
/// <param name="ResponseProjection">Projection metadata, present only for document operations.</param>
public record ContextAccessDecisionResult(ContextResolution Resolution, AccessDecision Result, DocumentResponseProjection? ResponseProjection = null);

/// <summary>Resolved Context envelope returned by access requirement descriptions.</summary>
/// <param name="Resolution">Single resolved composition used throughout the operation.</param>
/// <param name="Result">Typed operation result.</param>
/// <param name="ResponseProjection">Projection metadata, present only for document operations.</param>
public record ContextAccessRequirementsResult(ContextResolution Resolution, AccessRequirementsDescription Result, DocumentResponseProjection? ResponseProjection = null);

/// <summary>
/// Resolved Context envelope returned by directional crosswalk translation.
/// The translation result is preserved as contract-owned JSON because its
/// completed and forbidden branches are a discriminated union.
/// </summary>
/// <param name="Resolution">Single resolved composition used throughout the operation.</param>
/// <param name="Result">Typed operation result.</param>
/// <param name="ResponseProjection">Projection metadata, present only for document operations.</param>
public record ContextCrosswalkTranslationResult(ContextResolution Resolution, JsonElement Result, DocumentResponseProjection? ResponseProjection = null);

/// <summary>
/// One artifact taking part in a resolved Context: the id and version the
/// Context declared, plus the concrete version those resolved to.
/// </summary>
/// <param name="Id">Component id from the Context declaration.</param>
/// <param name="Version">Version literal the Context declares: a concrete semver or <c>latest</c>.</param>
/// <param name="ResolvedVersion">Concrete version after <c>latest</c> resolution.</param>
/// <param name="Layers">Layers stacked beneath this component, in composition order. Empty when it has none.</param>
public record ContextComponentRef(
    string Id,
    string Version,
    string ResolvedVersion,
    IReadOnlyList<ContextComponentRef> Layers);

/// <summary>
/// One scheme a Context holds, and the selector that addresses it. Read it as a
/// sentence: this Context holds a scheme you select with <c>Selector</c>, it is
/// <c>Id</c> at <c>Version</c>, and it plays these <c>Roles</c>.
/// </summary>
/// <param name="Selector">Context-local key that selects this scheme. It is what <c>?scheme=</c> takes on the scheme resources, and it need not equal <c>Id</c>.</param>
/// <param name="Id">Scheme id, from the scheme document itself.</param>
/// <param name="Version">Scheme version, from the scheme document itself.</param>
/// <param name="Roles">What this scheme is to the Context. Never empty.</param>
public record ContextSchemeDescriptor(
    string Selector,
    string Id,
    string Version,
    IReadOnlyList<ContextSchemeRole> Roles);

/// <summary>
/// What a Context is made of, as the server resolved it: the composed scheme,
/// every scheme held, the guides, the access policy and the crosswalks, each
/// with the version it declares and the concrete version that answered. An
/// integrator records these component versions to say which artifacts a
/// decision was made against.
/// </summary>
/// <param name="Id">Context id.</param>
/// <param name="Version">Concrete Context version after <c>latest</c> resolution.</param>
/// <param name="Scheme">The composed scheme, the base of the layer stack. Its <c>Layers</c> are the Context's layers in composition order.</param>
/// <param name="Schemes">Every scheme the Context holds, primary first, each with the selector that addresses it.</param>
/// <param name="Guides">Guide components in precedence order. Empty when the Context declares none.</param>
/// <param name="Crosswalks">Crosswalk components in declaration order, each with the layers it stacks and the concrete version that answered. Empty when the Context declares none.</param>
/// <param name="Access">Access policy component, or null when the Context declares none.</param>
/// <param name="Description">Human-readable description copied from the Context declaration, or null.</param>
public record ContextDescription(
    string Id,
    string Version,
    ContextComponentRef Scheme,
    IReadOnlyList<ContextSchemeDescriptor> Schemes,
    IReadOnlyList<ContextComponentRef> Guides,
    IReadOnlyList<ContextComponentRef> Crosswalks,
    ContextComponentRef? Access = null,
    string? Description = null);
