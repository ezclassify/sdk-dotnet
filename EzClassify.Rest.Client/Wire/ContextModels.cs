#nullable enable

using System.Collections.Generic;
using System.Text.Json;

namespace EzClassify;

// Result shapes returned by the context-bound engine operations
// (/v1/ctx/{ctxId}/{version}/*), written by hand beside the generated records
// in Models.g.cs and held to the vendored OpenAPI document by
// ContractFidelityTests. Like the generated records, each is built from
// init-only properties, with the members the contract requires marked
// `required`.
//
// The fix and change buckets (Fix, Change, ConceptRef), the explanation trace
// (ExplanationTrace and its TraceEntry, TraceRepair, TraceInference and
// TraceValidationIssue records) and the per-rule guide breakdown
// (GuideBreakdownEntry) are here too.

/// <summary>
/// A reference to a concept in a specific category. Both ids are required on the
/// wire. Carried by <see cref="Fix.Keeps"/>, <see cref="Fix.Adds"/>, and
/// <see cref="Fix.Removes"/>, and by <see cref="Change.Add"/> and
/// <see cref="Change.Remove"/>.
/// </summary>
public record ConceptRef
{
    /// <summary>Concept identifier.</summary>
    public required string ConceptId { get; init; }

    /// <summary>Owning category identifier.</summary>
    public required string CategoryId { get; init; }
}

/// <summary>
/// One way to make the marking valid. Every change in it answers an error.
/// Lives only in <c>proposedFixes</c>, including a document audit's
/// <c>MarkingValidation.proposedFixes</c>. <see cref="Rendered"/> is the valid
/// marking this fix produces; <see cref="Keeps"/> names concepts of the
/// author's marking it keeps, <see cref="Adds"/> concepts it adds because an
/// error requires them, and <see cref="Removes"/> concepts of the author's
/// marking it removes because an error names them.
/// </summary>
public record Fix
{
    /// <summary>What this fix does, in words.</summary>
    public required string Message { get; init; }

    /// <summary>The valid marking this fix produces, in the result's output format.</summary>
    public required string Rendered { get; init; }

    /// <summary>Concepts of the author's marking this fix keeps.</summary>
    public required IReadOnlyList<ConceptRef> Keeps { get; init; }

    /// <summary>Concepts this fix adds because an error requires them.</summary>
    public required IReadOnlyList<ConceptRef> Adds { get; init; }

    /// <summary>Concepts of the author's marking this fix removes because an error names them.</summary>
    public required IReadOnlyList<ConceptRef> Removes { get; init; }
}

/// <summary>
/// A change the engine made while processing. Lives in <c>appliedFixes</c>.
/// <see cref="Action"/> says which of <see cref="Add"/> and <see cref="Remove"/>
/// are set: <c>add</c> only <see cref="Add"/>, <c>remove</c> only
/// <see cref="Remove"/>, <c>replace</c> both, and <c>reorder</c> neither (the
/// input parsed via a tolerant rule; the canonical form is in
/// <c>ProcessResult.Rendered</c>).
/// </summary>
public record Change
{
    /// <summary>Rule that made this change: a scheme rule's id, or an id the engine keeps for a change no scheme rule names.</summary>
    public required string RuleId { get; init; }

    /// <summary>'add' (only <c>add</c> set), 'remove' (only <c>remove</c> set), 'replace' (both), 'reorder' (neither: the input parsed via a tolerant rule; the canonical form is in the result's <c>rendered</c>).</summary>
    public required FixAction Action { get; init; }

    /// <summary>What the change did, in words.</summary>
    public required string Message { get; init; }

    /// <summary>Concepts the change added. Set for <c>add</c> and <c>replace</c>.</summary>
    public IReadOnlyList<ConceptRef>? Add { get; init; }

    /// <summary>Concepts the change removed. Set for <c>remove</c> and <c>replace</c>.</summary>
    public IReadOnlyList<ConceptRef>? Remove { get; init; }

    /// <summary>Concept IDs whose presence triggered the rule that made this change.</summary>
    public IReadOnlyList<string>? TriggerConceptIds { get; init; }
}

/// <summary>
/// One step in the engine's explanation trace: which stage acted, what it did,
/// and which concept/rule was involved. <see cref="Action"/> is a plain string
/// here (unlike the typed <see cref="FixAction"/>): the trace narrates every
/// pipeline stage and is not a contract value set.
/// </summary>
public record TraceEntry
{
    /// <summary>The pipeline stage that acted, such as <c>parse</c>, <c>normalize</c>, <c>repair</c>, <c>infer</c>, <c>validate</c>, <c>render</c>, <c>aggregate</c> or <c>context</c>. An open string.</summary>
    public required string Stage { get; init; }

    /// <summary>What the stage did, as a short machine-readable token, such as <c>asserted</c>, <c>inferred</c>, <c>removed</c> or <c>unmatched</c>. An open string.</summary>
    public required string Action { get; init; }

    /// <summary>A sentence describing the step for a person. It is prose and must not be parsed; the structured fields carry the ids.</summary>
    public required string Detail { get; init; }

    /// <summary>The concept the step acted on, when it acted on one.</summary>
    public string? ConceptId { get; init; }

    /// <summary>The scheme rule that drove the step, when one did.</summary>
    public string? RuleId { get; init; }

    /// <summary>Concept IDs that triggered the rule producing this entry.</summary>
    public IReadOnlyList<string>? TriggerConceptIds { get; init; }

    /// <summary>Resolved operational display for <c>conceptId</c> (the concept's rendered marking view), so a "why" breakdown UI can show the marking rather than the raw id. <c>conceptId</c> stays as the machine key.</summary>
    public string? ConceptName { get; init; }

    /// <summary>Category the action landed in, when the acting rule declared one. This is the structured channel for the category: <c>detail</c> is human-facing prose and must never be parsed to recover it.</summary>
    public string? CategoryId { get; init; }
}

/// <summary>
/// One repair the engine applied while normalizing malformed input, with the
/// before/after strings.
/// </summary>
public record TraceRepair
{
    /// <summary>The id of the repair rule that fired.</summary>
    public required string RuleId { get; init; }

    /// <summary>What the repair does, as the scheme describes it.</summary>
    public required string Description { get; init; }

    /// <summary>The text before the repair.</summary>
    public required string Before { get; init; }

    /// <summary>The text after the repair.</summary>
    public required string After { get; init; }

    /// <summary>True when the repair changed only formatting and left the assertions as they were.</summary>
    public bool? Cosmetic { get; init; }
}

/// <summary>
/// One inference the engine drew: the rule that fired, the concept it added, and
/// the human-readable reason. Mirrors <c>ExplanationTrace.inferences</c> items.
/// </summary>
public record TraceInference
{
    /// <summary>The id of the inference rule that fired.</summary>
    public required string RuleId { get; init; }

    /// <summary>The concept the rule added.</summary>
    public required string ConceptId { get; init; }

    /// <summary>Why the rule added the concept, in words.</summary>
    public required string Explanation { get; init; }

    /// <summary>Resolved operational display for <c>conceptId</c> (the concept's rendered marking view), so a "why" breakdown UI can show the inferred marking rather than the raw id. <c>conceptId</c> stays as the machine key.</summary>
    public string? ConceptName { get; init; }
}

/// <summary>
/// A validation problem recorded in an explanation trace. The wire deliberately
/// trims this to <c>constraintId</c> + <c>message</c> here; it is NOT the full
/// <see cref="ValidationError"/> shape, which carries severity, category, and
/// affected concepts.
/// </summary>
public record TraceValidationIssue
{
    /// <summary>The id of the constraint that failed.</summary>
    public required string ConstraintId { get; init; }

    /// <summary>What is wrong, in words.</summary>
    public required string Message { get; init; }
}

/// <summary>
/// What explain answers: the step-by-step trace of how the engine settled one
/// marking. It carries the input and scheme, each stage entry, the repairs and
/// inferences that fired, the trimmed validation issues, the final assertions,
/// and the rendered marking. Explain takes what process takes, and no other
/// result carries the trace.
/// </summary>
public record ExplanationTrace
{
    /// <summary>The marking text the explanation was asked for, as given.</summary>
    public required string Input { get; init; }

    /// <summary>The id of the scheme the marking was read against.</summary>
    public required string SchemeId { get; init; }

    /// <summary>The settled marking, written in the output format.</summary>
    public required string Rendered { get; init; }

    /// <summary>Every step the pipeline took, in the order it took them.</summary>
    public required IReadOnlyList<TraceEntry> Entries { get; init; }

    /// <summary>Each repair rule that rewrote the input, in the order applied.</summary>
    public required IReadOnlyList<TraceRepair> Repairs { get; init; }

    /// <summary>Each concept an inference rule added, in the order added.</summary>
    public required IReadOnlyList<TraceInference> Inferences { get; init; }

    /// <summary>The constraints the settled marking violates.</summary>
    public required IReadOnlyList<TraceValidationIssue> ValidationErrors { get; init; }

    /// <summary>The assertions the pipeline settled on, listed in print order as a validate result's <c>assertions</c> are.</summary>
    public required IReadOnlyList<Assertion> FinalAssertions { get; init; }
}

/// <summary>
/// Per-rule breakdown in a guide evaluation result: the qualified rule, the
/// outcome it contributed, whether it was a computed (auto-activated) rule, and
/// whether another rule overrode it.
///
/// <see cref="Outcome"/> stays opaque JSON, as it does on <see cref="GuideRule"/>:
/// it is the rendered marking or concept references the guide declares, which
/// the server has already applied.
/// </summary>
public record GuideBreakdownEntry
{
    /// <summary>Qualified rule whose outcome was evaluated.</summary>
    public required RuleReference Rule { get; init; }

    /// <summary>Rule outcome evaluated by the guide engine.</summary>
    public required JsonElement Outcome { get; init; }

    /// <summary>Whether the rule was activated from other guide facts.</summary>
    public required bool IsComputed { get; init; }

    /// <summary>Whether an active rule replaced this rule's outcome.</summary>
    public required bool WasOverridden { get; init; }

    /// <summary>Qualified overriding rule when this outcome was replaced.</summary>
    public RuleReference? OverriddenBy { get; init; }
}

/// <summary>
/// One selected rule, named by the guide that holds it. Guides in a Context are
/// independent, so two of them may use the same rule id; a selection names the
/// guide as well as the rule, and the two pairs are two rules.
/// </summary>
public record GuideRuleSelection
{
    /// <summary>Id of the guide that holds the rule.</summary>
    public required string GuideId { get; init; }

    /// <summary>Id of the rule within that guide.</summary>
    public required string RuleId { get; init; }
}

/// <summary>
/// Whether a parent marking covers its children, checked category by category.
/// A parent can fail in two directions, and each has its own list:
/// <c>Missing</c> is what the children need that the parent does not carry, and
/// <c>Excess</c> is what the parent carries that the children do not account for.
/// Which list a concept is in says whether to add it or remove it.
/// <c>Refusals</c> is null unless a rollup refused to combine the children.
/// </summary>
public record CoverageReport
{
    /// <summary>True when nothing is missing, nothing is in excess, and no rollup refused.</summary>
    public required bool Covered { get; init; }

    /// <summary>Concepts the children need that the parent does not carry.</summary>
    public required IReadOnlyList<Assertion> Missing { get; init; }

    /// <summary>Display labels for <c>Missing</c>, deduplicated and not aligned with it by position.</summary>
    public required IReadOnlyList<string> MissingLabels { get; init; }

    /// <summary>Concepts the parent carries that the children do not account for.</summary>
    public required IReadOnlyList<Assertion> Excess { get; init; }

    /// <summary>Display labels for <c>Excess</c>, deduplicated and not aligned with it by position.</summary>
    public required IReadOnlyList<string> ExcessLabels { get; init; }

    /// <summary>The children's aggregate, rendered in the requested format.</summary>
    public required string Rendered { get; init; }

    /// <summary>The errors of each rollup that refused to combine the children, or null when none did.</summary>
    public IReadOnlyList<ValidationError>? Refusals { get; init; }
}

/// <summary>A concept's usage: definition, guidance, applicable rules, and pre-formatted tooltip lines.</summary>
public record ConceptDescription
{
    /// <summary>The id of the concept described.</summary>
    public required string ConceptId { get; init; }

    /// <summary>The category the concept belongs to.</summary>
    public required string CategoryId { get; init; }

    /// <summary>The concept's display name.</summary>
    public required string Name { get; init; }

    /// <summary>Pre-formatted lines for tooltip display (header, description, guidance, rules).</summary>
    public required IReadOnlyList<string> Lines { get; init; }

    /// <summary>What the concept means, as the scheme defines it.</summary>
    public string? Definition { get; init; }

    /// <summary>How to apply the concept, as the scheme advises.</summary>
    public string? Guidance { get; init; }

    /// <summary>The scheme's rules that involve the concept, each as a sentence.</summary>
    public IReadOnlyList<string>? Rules { get; init; }
}

/// <summary>A regulatory or handling notice attached to a concept, collected for a set of assertions.</summary>
public record Notice
{
    /// <summary>The concept the notice is attached to.</summary>
    public required string ConceptId { get; init; }

    /// <summary>The concept's display name, for showing beside the notice.</summary>
    public required string ConceptName { get; init; }

    /// <summary>What kind of notice this is.</summary>
    public required string Type { get; init; }

    /// <summary>The notice text the marking requires.</summary>
    public required string Text { get; init; }

    /// <summary>Whether the notice must be shown. A host surfaces a required notice before the marking is accepted.</summary>
    public required bool Required { get; init; }

    /// <summary>The notice's own <c>ConceptNotice.views</c>, carried through unchanged.</summary>
    public IReadOnlyDictionary<string, string>? Views { get; init; }
}

/// <summary>Server-side description of a single access rule.</summary>
public record AccessRuleDescription
{
    /// <summary>Qualified rule being described.</summary>
    public required RuleReference Rule { get; init; }

    /// <summary>Access artifact that owns the rule.</summary>
    public required ArtifactReference Access { get; init; }

    /// <summary>Scheme artifact whose assertions the rule evaluates.</summary>
    public required ArtifactReference Scheme { get; init; }

    /// <summary>Human-readable rule name.</summary>
    public required string Name { get; init; }

    /// <summary>Human-readable requirement expression.</summary>
    public required string RequirementText { get; init; }

    /// <summary>Subject or context paths required to evaluate the rule.</summary>
    public required IReadOnlyList<string> AttributesNeeded { get; init; }

    /// <summary>Qualified rules this rule depends on.</summary>
    public required IReadOnlyList<RuleReference> DependsOn { get; init; }

    /// <summary>Qualified rules this rule overrides.</summary>
    public required IReadOnlyList<RuleReference> Overrides { get; init; }

    /// <summary>Optional policy-authored description.</summary>
    public string? Description { get; init; }

    /// <summary>Human-readable activation expression.</summary>
    public string? WhenText { get; init; }
}

/// <summary>
/// Aggregated access requirements for a marking: every rule that would fire
/// plus the flat union of subject attributes the caller has to provide.
/// </summary>
public record AccessRequirementsDescription
{
    /// <summary>Normalized effective target assertions, listed in print order: the order <c>marking</c> places each concept; a concept it leaves out appears where it would print.</summary>
    public required IReadOnlyList<Assertion> Assertions { get; init; }

    /// <summary>Resolved operational display (rendered marking view) for each entry in <c>assertions</c>, index-aligned. A consumer that instantiates <c>requirementText</c> against the target assertions substitutes this rather than the raw concept id, so the requirement renders the concept's marking token instead of its internal id. <c>assertions</c> keeps the machine <c>conceptId</c> keys.</summary>
    public required IReadOnlyList<string> AssertionDisplays { get; init; }

    /// <summary>Access artifact whose policy was described.</summary>
    public required ArtifactReference Access { get; init; }

    /// <summary>Scheme artifact used to normalize the target.</summary>
    public required ArtifactReference Scheme { get; init; }

    /// <summary>Qualified rules activated by the trusted target.</summary>
    public required IReadOnlyList<RuleReference> AppliedRules { get; init; }

    /// <summary>Per-rule immutable-policy descriptions.</summary>
    public required IReadOnlyList<AccessRuleDescription> Rules { get; init; }

    /// <summary>Combined human-readable requirement expression.</summary>
    public required string CombinedRequirementText { get; init; }

    /// <summary>Union of subject and context paths needed to evaluate the target.</summary>
    public required IReadOnlyList<string> CombinedAttributesNeeded { get; init; }

    /// <summary>Normalized marking when the caller supplied or derived one.</summary>
    public string? Marking { get; init; }
}

/// <summary>One category of a scheme view, with its concepts in declared order.</summary>
public record SchemeViewCategory
{
    /// <summary>The category id.</summary>
    public required string Id { get; init; }

    /// <summary>The category's display label.</summary>
    public required string Label { get; init; }

    /// <summary>The category's concepts, in declared order.</summary>
    public required IReadOnlyList<ConceptView> Items { get; init; }
}

/// <summary>
/// Render-ready tree of a scheme: its categories of concepts, in declared
/// order, plus the concepts that declare no category.
/// </summary>
public record SchemeView
{
    /// <summary>The scheme id.</summary>
    public required string Id { get; init; }

    /// <summary>The scheme's display name.</summary>
    public required string Name { get; init; }

    /// <summary>The scheme's description. Empty when the scheme declares none.</summary>
    public required string Description { get; init; }

    /// <summary>The scheme's categories in declared order, each with the concepts that belong to it. A category with no concepts is left out.</summary>
    public required IReadOnlyList<SchemeViewCategory> Categories { get; init; }

    /// <summary>Concepts that belong to none of the scheme's loaded categories. Usually empty.</summary>
    public required IReadOnlyList<ConceptView> Uncategorized { get; init; }

    /// <summary>The scheme document's own overall marking, exactly as its <c>scheme.marking</c> writes it. Display only: never interpreted. Absent when the document declares none.</summary>
    public string? Marking { get; init; }

    /// <summary>Display-term overrides the scheme declares, unresolved.</summary>
    public IReadOnlyDictionary<string, Term>? Vocabulary { get; init; }
}

/// <summary>One category of a guide view, with its rules in declared order.</summary>
public record GuideViewCategory
{
    /// <summary>The category id, or <c>_compound</c> for the compound rules.</summary>
    public required string Id { get; init; }

    /// <summary>The category's display label.</summary>
    public required string Label { get; init; }

    /// <summary>The category's rules, in declared order.</summary>
    public required IReadOnlyList<GuideRuleView> Items { get; init; }
}

/// <summary>
/// Render-ready Guidebook tree of a guide: its categories of rules, in declared
/// order, plus the rules that declare no category.
/// </summary>
public record GuideView
{
    /// <summary>The guide id.</summary>
    public required string Id { get; init; }

    /// <summary>The guide's display name.</summary>
    public required string Name { get; init; }

    /// <summary>The guide's description. Empty when the guide declares none.</summary>
    public required string Description { get; init; }

    /// <summary>The guide's categories in declared order, each with its checkable rules, followed by the <c>_compound</c> category that holds every rule with a <c>when</c> condition. A category with no rules is left out.</summary>
    public required IReadOnlyList<GuideViewCategory> Categories { get; init; }

    /// <summary>Checkable rules that name no category, or a category the guide does not declare.</summary>
    public required IReadOnlyList<GuideRuleView> Uncategorized { get; init; }

    /// <summary>The guide document's own overall marking, exactly as its <c>guide.marking</c> writes it. Display only: never interpreted. Absent when the document declares none.</summary>
    public string? Marking { get; init; }

    /// <summary>Display-term overrides the guide declares, unresolved.</summary>
    public IReadOnlyDictionary<string, Term>? Vocabulary { get; init; }
}

/// <summary>
/// One rule of a guide as the Guidebook shows it: its name and outcome in
/// words, what a text scan matches for it, and the rules it depends on and
/// overrides, each with the display names index-aligned to the ids.
/// </summary>
public record GuideRuleView
{
    /// <summary>The rule id.</summary>
    public required string Id { get; init; }

    /// <summary>The rule's display name.</summary>
    public required string Name { get; init; }

    /// <summary>The rule's outcome for display: its rendered marking, or its concept ids listed in order when it has none. Empty when the outcome carries neither.</summary>
    public required string OutcomeText { get; init; }

    /// <summary>True when the rule has a <c>when</c> condition and is activated by it.</summary>
    public required bool IsComputed { get; init; }

    /// <summary>The keywords a text scan matches for this rule.</summary>
    public required IReadOnlyList<string> Keywords { get; init; }

    /// <summary>Labels like "IPv4 address", "CIDR block", already resolved via getMatcher.</summary>
    public required IReadOnlyList<string> MatcherLabels { get; init; }

    /// <summary>Rule ids this compound rule references.</summary>
    public required IReadOnlyList<string> DependsOn { get; init; }

    /// <summary>Resolved display names for each id in <c>dependsOn</c>, index-aligned, so a "depends on" line reads the rule's name rather than its raw id. <c>dependsOn</c> stays as the machine keys.</summary>
    public required IReadOnlyList<string> DependsOnNames { get; init; }

    /// <summary>Rule ids this compound rule overrides when it fires.</summary>
    public required IReadOnlyList<string> Overrides { get; init; }

    /// <summary>Resolved display names for each id in <c>overrides</c>, index-aligned, so the Guidebook "Overrides:" line reads rule names rather than raw ids, matching the sibling "When:" line which already resolves display. <c>overrides</c> stays as the machine keys.</summary>
    public required IReadOnlyList<string> OverrideNames { get; init; }

    /// <summary>The rule's description, when it declares one.</summary>
    public string? Description { get; init; }

    /// <summary>The rule's <c>when</c> condition in words. Set only on a computed rule.</summary>
    public string? WhenText { get; init; }

    /// <summary>Optional rationale text from the rule definition.</summary>
    public string? Rationale { get; init; }
}

/// <summary>
/// How an audit judged one authored marking, whole or part, separately from
/// coverage. It carries the marking's facts and how far recognition got with
/// it, so a caller that rolls markings up, checks access, or offers a
/// correction works from the audit instead of reading each marking again.
/// </summary>
public record MarkingValidation
{
    /// <summary>The facts the marking states, settled as a validate result's are, in the role the marking has in the document.</summary>
    public required IReadOnlyList<Assertion> Assertions { get; init; }

    /// <summary>What recognition made of the marking, as on <see cref="ValidateResult.Recognition"/>.</summary>
    public required RecognitionResult Recognition { get; init; }

    /// <summary>Whether the marking stands once settled, as <see cref="ValidateResult.Valid"/>.</summary>
    public required bool Valid { get; init; }

    /// <summary>Whether the marking was right as the author wrote it, as <see cref="ValidateResult.InputWasValid"/>. This is the question an audit asks.</summary>
    public required bool InputWasValid { get; init; }

    /// <summary>What the scheme's rules find wrong with the settled marking.</summary>
    public required IReadOnlyList<ValidationError> Errors { get; init; }

    /// <summary>The ways to make the marking valid, each a whole marking the engine has proven.</summary>
    public required IReadOnlyList<Fix> ProposedFixes { get; init; }

    /// <summary>Every change the engine made while settling the marking; empty when it made none, so a false <see cref="InputWasValid"/> is always explained here or in <see cref="Errors"/>.</summary>
    public required IReadOnlyList<Change> AppliedFixes { get; init; }

    /// <summary>The settled marking, written in its surfaces' defaults for the role the marking has in the document.</summary>
    public required string Rendered { get; init; }
}

/// <summary>A run of document text that falls outside every detected marking.</summary>
public record UnmarkedSpan
{
    /// <summary>Zero-based inclusive offset in the accepted document.</summary>
    public required int Start { get; init; }

    /// <summary>Zero-based exclusive offset in the accepted document.</summary>
    public required int End { get; init; }

    /// <summary>Frozen byte-offset encoding.</summary>
    public required string OffsetEncoding { get; init; }

    /// <summary>Authored text, omitted outside the full response profile.</summary>
    public string? Text { get; init; }
}

/// <summary>A document-wide marking detected during an audit, with its byte span.</summary>
public record AuditWhole
{
    /// <summary>The whole marking as the document writes it.</summary>
    public required string Marking { get; init; }

    /// <summary>Zero-based inclusive UTF-8 byte offset.</summary>
    public required int Start { get; init; }

    /// <summary>Zero-based exclusive UTF-8 byte offset.</summary>
    public required int End { get; init; }

    /// <summary>Frozen byte-offset encoding.</summary>
    public required string OffsetEncoding { get; init; }
}

/// <summary>Per-part audit result with optional profile-projected text and coverage/validity verdicts.</summary>
public record AuditPart
{
    /// <summary>The part marking as the document writes it.</summary>
    public required string Marking { get; init; }

    /// <summary>The text the part marking governs. Omitted outside the full response profile.</summary>
    public string? Text { get; init; }

    /// <summary>Zero-based inclusive UTF-8 byte offset.</summary>
    public required int Start { get; init; }

    /// <summary>Zero-based exclusive UTF-8 byte offset.</summary>
    public required int End { get; init; }

    /// <summary>Frozen byte-offset encoding.</summary>
    public required string OffsetEncoding { get; init; }

    /// <summary>The scope id the part is evaluated under, <c>part-</c> followed by its one-based position in the document. Guide results for the part carry the same id.</summary>
    public required string ScopeId { get; init; }

    /// <summary>How the part marking was judged, with the facts it states.</summary>
    public required MarkingValidation Validation { get; init; }
}

/// <summary>Whether the loaded scheme requires this document to carry a whole marking and part markings.</summary>
public record DocumentMarkingRequirements
{
    /// <summary>Whether an authored document-wide marking is required.</summary>
    public required bool Whole { get; init; }

    /// <summary>Whether authored part markings are required for document text.</summary>
    public required bool Part { get; init; }
}

/// <summary>A singular/plural display-word pair.</summary>
public record Term
{
    /// <summary>The word for one, such as <c>rule</c>.</summary>
    public required string Singular { get; init; }

    /// <summary>The word for more than one, such as <c>rules</c>.</summary>
    public required string Plural { get; init; }
}

/// <summary>
/// Composed document audit: whole-marking adequacy, per-part coverage, unmarked
/// content, and an aggregate verdict. Returned by the context document-audit op.
/// </summary>
public record DocumentAuditReport
{
    /// <summary>Evaluated canonical marking requirements, independent of discovered evidence.</summary>
    public required DocumentMarkingRequirements MarkingRequirements { get; init; }

    /// <summary>First authored document-wide marking, or null when absent.</summary>
    public required string? Whole { get; init; }

    /// <summary>Every authored document-wide marking occurrence.</summary>
    public required IReadOnlyList<AuditWhole> Wholes { get; init; }

    /// <summary>How the first authored whole was judged, as a whole that must cover the authored parts that read as markings, with the facts it states. Null when the document has no authored whole.</summary>
    public required MarkingValidation? WholeValidation { get; init; }

    /// <summary>Authored part markings and their governed regions.</summary>
    public required IReadOnlyList<AuditPart> Parts { get; init; }

    /// <summary>Authored whole coverage of authored parts.</summary>
    public required CoverageReport? WholeCoversParts { get; init; }

    /// <summary>Authored ranges not governed by a marking.</summary>
    public required IReadOnlyList<UnmarkedSpan> UnmarkedSpans { get; init; }

    /// <summary>Stable authored-procedure findings.</summary>
    public required IReadOnlyList<string> ProceduralFlags { get; init; }

    /// <summary>Verdict derived only from authored structural dimensions.</summary>
    public required AuditVerdict OverallVerdict { get; init; }

    /// <summary>SHA-256 digest of the exact accepted UTF-8 document bytes.</summary>
    public required string DocumentDigest { get; init; }

    /// <summary>Document-level assessment limitations.</summary>
    public required IReadOnlyList<AssessmentLimitation> Limitations { get; init; }

    /// <summary>Scheme-provided display vocabulary.</summary>
    public required IReadOnlyDictionary<string, Term>? Vocabulary { get; init; }

    /// <summary>Guide compilation dimension, or null when no guide applies.</summary>
    public required DocumentGuideAssessment? GuideAssessment { get; init; }
}

// ---------------------------------------------------------------------------
// Scheme resource models.
//
// Read-only projections of a composed scheme: summary metadata, the concept and
// category lists, the render formats, the parse grammar, and the label presets.
// They are what a remote adapter needs to render and explain markings without
// holding the scheme document itself.
//
// These records serve the Context-bound scheme resources under
// /v1/ctx/{ctxId}/{version}/scheme/*. The Context selects the composed scheme;
// all but the label presets take an optional scheme query that identifies a
// source or target scheme within a crosswalk-bearing Context.

/// <summary>
/// Resolved display label for an artifact's own reference surface: the noun a
/// scheme's Handbook wears in a UI. Always fully populated on the wire, falling
/// back to the engine's generic noun when the scheme declares no override.
/// </summary>
public record ReferenceSurfaceLabel
{
    /// <summary>The noun for one, such as <c>Handbook</c>.</summary>
    public required string Singular { get; init; }

    /// <summary>The noun for more than one, such as <c>Handbooks</c>.</summary>
    public required string Plural { get; init; }
}

/// <summary>
/// Render format ids grouped by the structural role they mark. For a scheme
/// whose formats are its outputs, these are the lists the scanner reads a
/// marking's role from. For a scheme that declares surfaces, each role lists
/// the forms of the views that state that role, for a caller that picks a
/// format per role.
/// </summary>
public record SchemeContextFormats
{
    /// <summary>Format ids a whole marking is read in.</summary>
    public required IReadOnlyList<string> Whole { get; init; }

    /// <summary>Format ids a part marking is read in.</summary>
    public required IReadOnlyList<string> Part { get; init; }
}

/// <summary>
/// One entry in a scheme's ordered display-color rule list. The last rule may
/// omit <see cref="When"/> to act as the unconditional fallback.
///
/// <see cref="When"/> stays opaque JSON for the same reason
/// <see cref="GuideRule.When"/> does: it carries the recursive
/// assertion-match DSL, which this SDK passes through rather than over-models.
/// </summary>
public record MarkingColorRule
{
    /// <summary>Complete background/text pair to use when the condition matches.</summary>
    public required ConceptColor Color { get; init; }

    /// <summary>Assertion-set condition. Omit only on the final rule to create the standard unconditional/anything-else fallback.</summary>
    public JsonElement? When { get; init; }
}

/// <summary>
/// High-level metadata for a composed scheme: identity, presentation
/// configuration, and the render defaults a remote caller cannot derive from
/// the other sub-resources.
///
/// <see cref="RoleAware"/> is null when the scheme leaves it unset.
/// <see cref="EmbeddedMarking"/> is the list of ways a marking may appear
/// embedded in free text, carried as opaque JSON; a null one is how a client
/// tells that inline marking is off.
///
/// <see cref="DefaultRenderFormat"/> is the output a call that names none is
/// written in. A scheme that declares surfaces names its outputs
/// <c>surface/view</c> and writes a whole and a part in their own defaults:
/// <see cref="DefaultWholeRenderFormat"/> and
/// <see cref="DefaultPartRenderFormat"/> give those addresses, and are null
/// for a scheme whose formats are its outputs, which has the one default.
/// </summary>
public record SchemeSummary
{
    /// <summary>Scheme identifier from the scheme metadata.</summary>
    public required string Id { get; init; }

    /// <summary>Scheme semantic version.</summary>
    public required string Version { get; init; }

    /// <summary>Display name from the scheme metadata.</summary>
    public required string Name { get; init; }

    /// <summary>Free-text description from the scheme metadata.</summary>
    public required string Description { get; init; }

    /// <summary>Whether the scheme uses role-aware processing. Omitted when unset.</summary>
    public bool? RoleAware { get; init; }

    /// <summary>Display label for this scheme's own reference component, the Handbook: the scheme's override or the generic "Handbook" / "Handbooks" when it declares none. Always populated.</summary>
    public required ReferenceSurfaceLabel ReferenceSurfaceLabel { get; init; }

    /// <summary>Ways a marking may appear embedded in free text. Omitted when the scheme declares none, which is how a client tells that inline marking is off.</summary>
    public JsonElement? EmbeddedMarking { get; init; }

    /// <summary>Render format applied when a call names none. A remote adapter has no way to derive this from the formats sub-resource: format declaration order is not default order, so a client that guesses the first entry renders in the wrong format whenever the two differ.</summary>
    public string? DefaultRenderFormat { get; init; }

    /// <summary>Human-readable definition of the scheme, for tooltips and about panels.</summary>
    public string? Definition { get; init; }

    /// <summary>Usage guidance for the scheme, shown in reference views.</summary>
    public string? Guidance { get; init; }

    /// <summary>The scheme document's own overall marking, exactly as its <c>scheme.marking</c> writes it. Display only: never interpreted. Omitted when the document declares none.</summary>
    public string? Marking { get; init; }

    /// <summary>Single-category scheme-default color source, when category mode is used.</summary>
    public string? ColorFrom { get; init; }

    /// <summary>Ordered rule-driven color outcomes, when rule mode is used.</summary>
    public IReadOnlyList<MarkingColorRule>? ColorRules { get; init; }

    /// <summary>Scheme-authored display vocabulary.</summary>
    public IReadOnlyDictionary<string, Term>? Vocabulary { get; init; }

    /// <summary>Render format IDs grouped by role. For a scheme without <c>render.surfaces</c>, the lists <c>role_detection</c> gives, which the scanner reads a marking's role from. For a scheme with them, the scanner takes the role from the view a marking was read as, and these lists are derived from the views for a caller that picks a format per role: each role lists the forms of the views that state it, <c>format</c>, case formats and <c>also_read_in</c> in that order, surfaces and views in declared order and each format once.</summary>
    public SchemeContextFormats? ContextFormats { get; init; }

    /// <summary>Default scope id applied when a marking asserts no scope explicitly. Present exactly when the scheme declares scopes. Pairs with the per-category <c>scopes</c> lists on the categories sub-resource so a remote adapter can reconstruct which categories a scope-only assertion set targets: the metadata the assertion-set aggregate path needs to render scoped categories that the whole-marking format would otherwise drop.</summary>
    public string? DefaultScope { get; init; }

    /// <summary>Display label for the scope selector shown in UIs. Omitted when the scheme declares none. Carried here so <c>getSchemeMeta</c> answers the same for a remote adapter as it does for a local one; without it the selector is labeled from the scheme document in one mode and left unlabeled in the other.</summary>
    public string? ScopeLabel { get; init; }

    /// <summary>Present only for a scheme that declares <c>render.surfaces</c>: the output address a part marking is written in by default. It is the <c>surface/view</c> of the default scope's surface's default for a part, from that surface's <c>default</c>, and does not depend on <c>contextFormats</c>, which lists the formats each role is read in.</summary>
    public string? DefaultPartRenderFormat { get; init; }

    /// <summary>Present only for a scheme that declares <c>render.surfaces</c>: the output address a whole marking is written in by default, the default scope's surface's default for a whole, resolved the same way <c>defaultPartRenderFormat</c> is for a part.</summary>
    public string? DefaultWholeRenderFormat { get; init; }
}

/// <summary>
/// A regulatory or handling notice a concept carries, as the concept view
/// lists it: what has to be said, what kind of notice it is, and whether
/// showing it is mandatory. Where the text appears is the host's to decide.
/// </summary>
public record ConceptNotice
{
    /// <summary>What kind of notice this is.</summary>
    public required string Type { get; init; }

    /// <summary>Whether this notice MUST be displayed (regulatory requirement)</summary>
    public required bool Required { get; init; }

    /// <summary>The notice text the marking requires.</summary>
    public required string Text { get; init; }

    /// <summary>Only show when this concept is also present</summary>
    public string? Condition { get; init; }

    /// <summary>The value this notice is registered under, keyed by view.</summary>
    public IReadOnlyDictionary<string, string>? Views { get; init; }
}

/// <summary>
/// One concept of a scheme, resolved for display: identity, hierarchy,
/// position, the spellings it has in each view, and the documentation a
/// reference surface shows beside it. The same shape a local engine's scheme
/// view carries.
///
/// <see cref="Views"/> maps a view name to the spelling the concept has in
/// it; the map always holds at least <c>default</c>. <see cref="TextInput"/>
/// stays opaque JSON: it configures a free-text input, and a remote adapter
/// does not need it decoded.
///
/// Everything past <see cref="Position"/> is optional on the wire. A concept
/// that declares no rank, no members, no scopes, and no deprecation leaves
/// those null rather than sending empty values.
/// </summary>
public record ConceptView
{
    /// <summary>Unique concept identifier.</summary>
    public required string Id { get; init; }

    /// <summary>Human-readable display name.</summary>
    public required string Name { get; init; }

    /// <summary>Display representations keyed by view name; includes at least <c>'default'</c>.</summary>
    public required IReadOnlyDictionary<string, string> Views { get; init; }

    /// <summary>Alternative strings that parse to this concept.</summary>
    public required IReadOnlyList<string> Aliases { get; init; }

    /// <summary>Category IDs this concept belongs to.</summary>
    public required IReadOnlyList<string> Categories { get; init; }

    /// <summary>Broader concept IDs forming the hierarchy chain.</summary>
    public required IReadOnlyList<string> Parents { get; init; }

    /// <summary>Resolved rendering position within a category (lower = earlier).</summary>
    public required double Position { get; init; }

    /// <summary>Portable scheme-default display color pair, present only on concepts in the scheme's category-based <c>color_from</c> mode. Absent otherwise.</summary>
    public ConceptColor? Color { get; init; }

    /// <summary>Resolved severity rank for max/min rollup aggregation (higher = more severe).</summary>
    public double? Rank { get; init; }

    /// <summary>Tier label: marks this concept as a partial marking.</summary>
    public string? Tier { get; init; }

    /// <summary>For group concepts: the member concept IDs.</summary>
    public IReadOnlyList<string>? Members { get; init; }

    /// <summary>Scope IDs this concept is visible in.</summary>
    public IReadOnlyList<string>? Scopes { get; init; }

    /// <summary>Whether this concept is deprecated.</summary>
    public bool? Deprecated { get; init; }

    /// <summary>Human-readable deprecation message.</summary>
    public string? DeprecatedMessage { get; init; }

    /// <summary>Concept ID that should be used instead of this deprecated concept.</summary>
    public string? DeprecatedReplacement { get; init; }

    /// <summary>Definition text from the concept's documentation block.</summary>
    public string? Definition { get; init; }

    /// <summary>Where this concept came from: the provenance text the scheme records against it.</summary>
    public string? Citation { get; init; }

    /// <summary>Usage guidance from the concept's documentation block.</summary>
    public string? Guidance { get; init; }

    /// <summary>Regulatory or policy notices shown with this concept.</summary>
    public IReadOnlyList<ConceptNotice>? Notices { get; init; }

    /// <summary>Human-readable summary of rules that reference this concept. Set by <c>buildSchemeView</c>; absent from <c>getConceptsInCategory</c>.</summary>
    public IReadOnlyList<string>? RulesReferencing { get; init; }

    /// <summary>Text-input configuration for free-text concepts.</summary>
    public JsonElement? TextInput { get; init; }
}

/// <summary>
/// One category of a scheme, resolved for display and selection: its label,
/// the scopes that offer it, and how its concepts roll up and sort. The same
/// shape a local engine's scheme view carries.
///
/// <see cref="Sort"/> and <see cref="DefaultConcept"/> stay opaque JSON. Sort
/// is a single key or an ordered list of keys; the default is either an
/// unconditional concept identifier or a conditional object holding its
/// concept and when expression. <see cref="Rollup"/> is the merge operator
/// applied across aggregated inputs, kept as the token the server sent.
/// Position overrides apply only to this category when position sorting is
/// selected.
/// </summary>
public record CategoryView
{
    /// <summary>Unique category identifier.</summary>
    public required string Id { get; init; }

    /// <summary>Resolved human-readable label (always a plain string).</summary>
    public required string Label { get; init; }

    /// <summary>Rollup operator used when aggregating multiple markings.</summary>
    public required string Rollup { get; init; }

    /// <summary>Whether at least one concept is required for a valid marking.</summary>
    public bool? Required { get; init; }

    /// <summary>Whether only one concept can be selected in this category.</summary>
    public bool? Single { get; init; }

    /// <summary>Sort order for concepts within this category.</summary>
    public JsonElement? Sort { get; init; }

    /// <summary>Concept positions used only by this category's position sort.</summary>
    public IReadOnlyDictionary<string, double>? PositionOverrides { get; init; }

    /// <summary>Scope IDs this category is visible in.</summary>
    public IReadOnlyList<string>? Scopes { get; init; }

    /// <summary>Declared rendering position (lower = earlier). Not resolved from array order.</summary>
    public double? Position { get; init; }

    /// <summary>When true, the category grades its concepts by severity <c>rank</c>, and a coverage comparison counts a whole-marking concept here as covering a part's concept of equal or lower rank. It does not change rendering.</summary>
    public bool? HierarchyRoot { get; init; }

    /// <summary>Concept ID to assert automatically when no selection exists in this category. With <c>single: true</c>, this provides a default level. Mirrors the scheme's <c>default_concept</c> for the category.</summary>
    public JsonElement? DefaultConcept { get; init; }
}

/// <summary>
/// One output's identity and layout: the part of a marking it writes, how it
/// lays the fields out, and the text it puts between and around categories.
/// The authored field tree and recognition rules stay on the server.
///
/// For a scheme whose formats are its outputs, each entry is a format. For a
/// scheme that declares surfaces, each entry is a view, addressed
/// <c>surface/view</c>, laid out as that view's base format writes it, and it
/// says which roles it is the default for and which role it is a view of.
/// </summary>
public record FormatView
{
    /// <summary>The output this entry describes, named by callers choosing an output or input format: a format id, or a <c>surface/view</c> address for a scheme that declares surfaces.</summary>
    public required string Id { get; init; }

    /// <summary>The part of a marking this format writes: a scheme scope id.</summary>
    public required string Surface { get; init; }

    /// <summary>Whether the fields run together or each goes on its own line.</summary>
    public required RenderFormatMode Mode { get; init; }

    /// <summary>The categories this format writes, in the order it writes them.</summary>
    public required IReadOnlyList<string> CategoryOrder { get; init; }

    /// <summary>Default concept view for fields that do not name one.</summary>
    public string? View { get; init; }

    /// <summary>Written once before the nonempty whole, when it is written unconditionally.</summary>
    public string? Prefix { get; init; }

    /// <summary>Written once after the nonempty whole, when it is written unconditionally.</summary>
    public string? Suffix { get; init; }

    /// <summary>What the format writes between two adjacent categories by default. Null for a format with no global separator, such as an XML-attribute encoding.</summary>
    public string? Separator { get; init; }

    /// <summary>What is written between two values of one category, keyed by category id.</summary>
    public IReadOnlyDictionary<string, string>? CategorySeparators { get; init; }

    /// <summary>What is written immediately before a category, keyed by category id.</summary>
    public IReadOnlyDictionary<string, string>? SeparatorBefore { get; init; }

    /// <summary>The concept view a category is written in, where it differs from <c>View</c>.</summary>
    public IReadOnlyDictionary<string, string>? CategoryViews { get; init; }

    /// <summary>How a category's tiered concepts compose into one token, outermost tier first.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<TierComposition>>? CategoryTiers { get; init; }

    /// <summary>What is written around every single value of a category.</summary>
    public IReadOnlyDictionary<string, EachAffix>? CategoryEach { get; init; }

    /// <summary>Headings this format writes, paired with the lists they head.</summary>
    public IReadOnlyList<FormatHeaderAssociation>? Headers { get; init; }

    /// <summary>The encoding, or null when the format is plain text.</summary>
    public FormatSyntax? Syntax { get; init; }

    /// <summary>The view part of <see cref="Id"/>, unescaped, for a scheme that declares surfaces. This names an output, not a concept spelling; that is <see cref="View"/>. Null for a scheme whose formats are its outputs.</summary>
    public string? OutputView { get; init; }

    /// <summary>For a scheme that declares surfaces, the roles this view is its surface's default for: a marking of each listed role is written in this view when a request names no output. Empty on the surface's other views. Null for a scheme whose formats are its outputs, which has the one default its summary reports.</summary>
    public IReadOnlyList<StructuralRole>? DefaultFor { get; init; }

    /// <summary>For a scheme that declares surfaces, the role of the markings this view is a view of. Null for a view with no role, and for a scheme whose formats are its outputs.</summary>
    public StructuralRole? Role { get; init; }
}

/// <summary>How one tier of a category's concepts attaches to the tier above it.</summary>
public record TierComposition
{
    /// <summary>The tier this step writes.</summary>
    public required string Tier { get; init; }

    /// <summary>The text that joins a concept of this tier to the one it belongs to.</summary>
    public required string AttachedBy { get; init; }

    /// <summary>The concept view this tier is written in, when it differs.</summary>
    public string? View { get; init; }

    /// <summary>The view the owning concept is written in when this tier attaches to it.</summary>
    public string? HeadView { get; init; }
}

/// <summary>What is written around each single value of a category.</summary>
public record EachAffix
{
    /// <summary>Written before each value.</summary>
    public string? Prefix { get; init; }

    /// <summary>Written after each value.</summary>
    public string? Suffix { get; init; }
}

/// <summary>A heading concept a format writes, paired with the list it heads.</summary>
public record FormatHeaderAssociation
{
    /// <summary>The heading concept.</summary>
    public required string Concept { get; init; }

    /// <summary>The category the heading concept belongs to.</summary>
    public required string HeaderCategory { get; init; }

    /// <summary>The category whose values the heading introduces.</summary>
    public required string Category { get; init; }

    /// <summary>Whether the heading comes before or after the list.</summary>
    public required FormatHeaderPosition Position { get; init; }
}

/// <summary>A format's encoding.</summary>
public record FormatSyntax
{
    /// <summary>The framing this format writes its text in.</summary>
    public required FormatSyntaxType Type { get; init; }
}

/// <summary>One scope a scheme declares, as the scopes resource lists it.</summary>
public record SchemeScope
{
    /// <summary>Scope identifier, referenced by <c>scopes</c> on concepts and categories.</summary>
    public required string Id { get; init; }

    /// <summary>Display label for the scope selector.</summary>
    public required string Label { get; init; }
}

/// <summary>
/// One entry in a scheme's public label preset list. Name and scope only: the
/// preset's marking string and tag list stay server-side, so a preset is
/// expanded by calling the apply-preset operation rather than by reading it.
/// </summary>
public record LabelPresetView
{
    /// <summary>Preset id, which <c>applyPreset</c> takes.</summary>
    public required string Id { get; init; }

    /// <summary>Display label for the preset.</summary>
    public required string Label { get; init; }

    /// <summary>Longer explanation of when to use the preset.</summary>
    public string? Description { get; init; }

    /// <summary>Id of the scope the preset belongs to. A preset without one belongs to the scheme's <c>defaultScope</c>.</summary>
    public string? Scope { get; init; }
}
