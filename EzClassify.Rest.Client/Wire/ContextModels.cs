#nullable enable

using System.Collections.Generic;
using System.Text.Json;

namespace EzClassify;

// Result shapes returned by the context-bound engine operations
// (/v1/ctx/{ctxId}/{version}/*), written by hand beside the generated records
// in Models.g.cs and held to the vendored OpenAPI document by
// ContractFidelityTests. Like the generated records, each is sealed, takes
// the members the contract requires through its constructor, and leaves the
// optional ones as settable properties.
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
public sealed record ConceptRef
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="conceptId">Concept identifier.</param>
    /// <param name="categoryId">Owning category identifier.</param>
    public ConceptRef(string conceptId, string categoryId)
    {
        ConceptId = conceptId ?? throw new ArgumentNullException(nameof(conceptId));
        CategoryId = categoryId ?? throw new ArgumentNullException(nameof(categoryId));
    }

    /// <summary>Concept identifier.</summary>
    public string ConceptId { get; }

    /// <summary>Owning category identifier.</summary>
    public string CategoryId { get; }
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
public sealed record Fix
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="message">What this fix does, in words.</param>
    /// <param name="rendered">The valid marking this fix produces, in the result's output format.</param>
    /// <param name="keeps">Concepts of the author's marking this fix keeps.</param>
    /// <param name="adds">Concepts this fix adds because an error requires them.</param>
    /// <param name="removes">Concepts of the author's marking this fix removes because an error names them.</param>
    public Fix(string message, string rendered, IReadOnlyList<ConceptRef> keeps, IReadOnlyList<ConceptRef> adds, IReadOnlyList<ConceptRef> removes)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Rendered = rendered ?? throw new ArgumentNullException(nameof(rendered));
        Keeps = keeps ?? throw new ArgumentNullException(nameof(keeps));
        Adds = adds ?? throw new ArgumentNullException(nameof(adds));
        Removes = removes ?? throw new ArgumentNullException(nameof(removes));
    }

    /// <summary>What this fix does, in words.</summary>
    public string Message { get; }

    /// <summary>The valid marking this fix produces, in the result's output format.</summary>
    public string Rendered { get; }

    /// <summary>Concepts of the author's marking this fix keeps.</summary>
    public IReadOnlyList<ConceptRef> Keeps { get; }

    /// <summary>Concepts this fix adds because an error requires them.</summary>
    public IReadOnlyList<ConceptRef> Adds { get; }

    /// <summary>Concepts of the author's marking this fix removes because an error names them.</summary>
    public IReadOnlyList<ConceptRef> Removes { get; }
}

/// <summary>
/// A change the engine made while processing. Lives in <c>appliedFixes</c>.
/// <see cref="Action"/> says which of <see cref="Add"/> and <see cref="Remove"/>
/// are set: <c>add</c> only <see cref="Add"/>, <c>remove</c> only
/// <see cref="Remove"/>, <c>replace</c> both, and <c>reorder</c> neither (the
/// input parsed via a tolerant rule; the canonical form is in
/// <c>ProcessResult.Rendered</c>).
/// </summary>
public sealed record Change
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="ruleId">Rule that made this change: a scheme rule's id, or an id the engine keeps for a change no scheme rule names.</param>
    /// <param name="action">'add' (only <c>add</c> set), 'remove' (only <c>remove</c> set), 'replace' (both), 'reorder' (neither: the input parsed via a tolerant rule; the canonical form is in the result's <c>rendered</c>).</param>
    /// <param name="message">What the change did, in words.</param>
    public Change(string ruleId, FixAction action, string message)
    {
        RuleId = ruleId ?? throw new ArgumentNullException(nameof(ruleId));
        Action = action;
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    /// <summary>Rule that made this change: a scheme rule's id, or an id the engine keeps for a change no scheme rule names.</summary>
    public string RuleId { get; }

    /// <summary>'add' (only <c>add</c> set), 'remove' (only <c>remove</c> set), 'replace' (both), 'reorder' (neither: the input parsed via a tolerant rule; the canonical form is in the result's <c>rendered</c>).</summary>
    public FixAction Action { get; }

    /// <summary>What the change did, in words.</summary>
    public string Message { get; }

    /// <summary>Concepts the change added. Set for <c>add</c> and <c>replace</c>.</summary>
    public IReadOnlyList<ConceptRef>? Add { get; set; }

    /// <summary>Concepts the change removed. Set for <c>remove</c> and <c>replace</c>.</summary>
    public IReadOnlyList<ConceptRef>? Remove { get; set; }

    /// <summary>Concept IDs whose presence triggered the rule that made this change.</summary>
    public IReadOnlyList<string>? TriggerConceptIds { get; set; }
}

/// <summary>
/// One step in the engine's explanation trace: which stage acted, what it did,
/// and which concept/rule was involved. <see cref="Action"/> is a plain string
/// here (unlike the typed <see cref="FixAction"/>): the trace narrates every
/// pipeline stage and is not a contract value set.
/// </summary>
public sealed record TraceEntry
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="stage">The pipeline stage that acted, such as <c>parse</c>, <c>normalize</c>, <c>repair</c>, <c>infer</c>, <c>validate</c>, <c>render</c>, <c>aggregate</c> or <c>context</c>. An open string.</param>
    /// <param name="action">What the stage did, as a short machine-readable token, such as <c>asserted</c>, <c>inferred</c>, <c>removed</c> or <c>unmatched</c>. An open string.</param>
    /// <param name="detail">A sentence describing the step for a person. It is prose and must not be parsed; the structured fields carry the ids.</param>
    public TraceEntry(string stage, string action, string detail)
    {
        Stage = stage ?? throw new ArgumentNullException(nameof(stage));
        Action = action ?? throw new ArgumentNullException(nameof(action));
        Detail = detail ?? throw new ArgumentNullException(nameof(detail));
    }

    /// <summary>The pipeline stage that acted, such as <c>parse</c>, <c>normalize</c>, <c>repair</c>, <c>infer</c>, <c>validate</c>, <c>render</c>, <c>aggregate</c> or <c>context</c>. An open string.</summary>
    public string Stage { get; }

    /// <summary>What the stage did, as a short machine-readable token, such as <c>asserted</c>, <c>inferred</c>, <c>removed</c> or <c>unmatched</c>. An open string.</summary>
    public string Action { get; }

    /// <summary>A sentence describing the step for a person. It is prose and must not be parsed; the structured fields carry the ids.</summary>
    public string Detail { get; }

    /// <summary>The concept the step acted on, when it acted on one.</summary>
    public string? ConceptId { get; set; }

    /// <summary>The scheme rule that drove the step, when one did.</summary>
    public string? RuleId { get; set; }

    /// <summary>Concept IDs that triggered the rule producing this entry.</summary>
    public IReadOnlyList<string>? TriggerConceptIds { get; set; }

    /// <summary>Resolved operational display for <c>conceptId</c> (the concept's rendered marking view), so a "why" breakdown UI can show the marking rather than the raw id. <c>conceptId</c> stays as the machine key.</summary>
    public string? ConceptName { get; set; }

    /// <summary>Category the action landed in, when the acting rule declared one. This is the structured channel for the category: <c>detail</c> is human-facing prose and must never be parsed to recover it.</summary>
    public string? CategoryId { get; set; }
}

/// <summary>
/// One repair the engine applied while normalizing malformed input, with the
/// before/after strings.
/// </summary>
public sealed record TraceRepair
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="ruleId">The id of the repair rule that fired.</param>
    /// <param name="description">What the repair does, as the scheme describes it.</param>
    /// <param name="before">The text before the repair.</param>
    /// <param name="after">The text after the repair.</param>
    public TraceRepair(string ruleId, string description, string before, string after)
    {
        RuleId = ruleId ?? throw new ArgumentNullException(nameof(ruleId));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Before = before ?? throw new ArgumentNullException(nameof(before));
        After = after ?? throw new ArgumentNullException(nameof(after));
    }

    /// <summary>The id of the repair rule that fired.</summary>
    public string RuleId { get; }

    /// <summary>What the repair does, as the scheme describes it.</summary>
    public string Description { get; }

    /// <summary>The text before the repair.</summary>
    public string Before { get; }

    /// <summary>The text after the repair.</summary>
    public string After { get; }

    /// <summary>True when the repair changed only formatting and left the assertions as they were.</summary>
    public bool? Cosmetic { get; set; }
}

/// <summary>
/// One inference the engine drew: the rule that fired, the concept it added, and
/// the human-readable reason. Mirrors <c>ExplanationTrace.inferences</c> items.
/// </summary>
public sealed record TraceInference
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="ruleId">The id of the inference rule that fired.</param>
    /// <param name="conceptId">The concept the rule added.</param>
    /// <param name="explanation">Why the rule added the concept, in words.</param>
    public TraceInference(string ruleId, string conceptId, string explanation)
    {
        RuleId = ruleId ?? throw new ArgumentNullException(nameof(ruleId));
        ConceptId = conceptId ?? throw new ArgumentNullException(nameof(conceptId));
        Explanation = explanation ?? throw new ArgumentNullException(nameof(explanation));
    }

    /// <summary>The id of the inference rule that fired.</summary>
    public string RuleId { get; }

    /// <summary>The concept the rule added.</summary>
    public string ConceptId { get; }

    /// <summary>Why the rule added the concept, in words.</summary>
    public string Explanation { get; }

    /// <summary>Resolved operational display for <c>conceptId</c> (the concept's rendered marking view), so a "why" breakdown UI can show the inferred marking rather than the raw id. <c>conceptId</c> stays as the machine key.</summary>
    public string? ConceptName { get; set; }
}

/// <summary>
/// A validation problem recorded in an explanation trace. The wire deliberately
/// trims this to <c>constraintId</c> + <c>message</c> here; it is NOT the full
/// <see cref="ValidationError"/> shape, which carries severity, category, and
/// affected concepts.
/// </summary>
public sealed record TraceValidationIssue
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="constraintId">The id of the constraint that failed.</param>
    /// <param name="message">What is wrong, in words.</param>
    public TraceValidationIssue(string constraintId, string message)
    {
        ConstraintId = constraintId ?? throw new ArgumentNullException(nameof(constraintId));
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    /// <summary>The id of the constraint that failed.</summary>
    public string ConstraintId { get; }

    /// <summary>What is wrong, in words.</summary>
    public string Message { get; }
}

/// <summary>
/// What explain answers: the step-by-step trace of how the engine settled one
/// marking. It carries the input and scheme, each stage entry, the repairs and
/// inferences that fired, the trimmed validation issues, the final assertions,
/// and the rendered marking. Explain takes what process takes, and no other
/// result carries the trace.
/// </summary>
public sealed record ExplanationTrace
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="input">The marking text the explanation was asked for, as given.</param>
    /// <param name="schemeId">The id of the scheme the marking was read against.</param>
    /// <param name="rendered">The settled marking, written in the output format.</param>
    /// <param name="entries">Every step the pipeline took, in the order it took them.</param>
    /// <param name="repairs">Each repair rule that rewrote the input, in the order applied.</param>
    /// <param name="inferences">Each concept an inference rule added, in the order added.</param>
    /// <param name="validationErrors">The constraints the settled marking violates.</param>
    /// <param name="finalAssertions">The assertions the pipeline settled on, listed in print order as a validate result's <c>assertions</c> are.</param>
    public ExplanationTrace(string input, string schemeId, string rendered, IReadOnlyList<TraceEntry> entries, IReadOnlyList<TraceRepair> repairs, IReadOnlyList<TraceInference> inferences, IReadOnlyList<TraceValidationIssue> validationErrors, IReadOnlyList<Assertion> finalAssertions)
    {
        Input = input ?? throw new ArgumentNullException(nameof(input));
        SchemeId = schemeId ?? throw new ArgumentNullException(nameof(schemeId));
        Rendered = rendered ?? throw new ArgumentNullException(nameof(rendered));
        Entries = entries ?? throw new ArgumentNullException(nameof(entries));
        Repairs = repairs ?? throw new ArgumentNullException(nameof(repairs));
        Inferences = inferences ?? throw new ArgumentNullException(nameof(inferences));
        ValidationErrors = validationErrors ?? throw new ArgumentNullException(nameof(validationErrors));
        FinalAssertions = finalAssertions ?? throw new ArgumentNullException(nameof(finalAssertions));
    }

    /// <summary>The marking text the explanation was asked for, as given.</summary>
    public string Input { get; }

    /// <summary>The id of the scheme the marking was read against.</summary>
    public string SchemeId { get; }

    /// <summary>The settled marking, written in the output format.</summary>
    public string Rendered { get; }

    /// <summary>Every step the pipeline took, in the order it took them.</summary>
    public IReadOnlyList<TraceEntry> Entries { get; }

    /// <summary>Each repair rule that rewrote the input, in the order applied.</summary>
    public IReadOnlyList<TraceRepair> Repairs { get; }

    /// <summary>Each concept an inference rule added, in the order added.</summary>
    public IReadOnlyList<TraceInference> Inferences { get; }

    /// <summary>The constraints the settled marking violates.</summary>
    public IReadOnlyList<TraceValidationIssue> ValidationErrors { get; }

    /// <summary>The assertions the pipeline settled on, listed in print order as a validate result's <c>assertions</c> are.</summary>
    public IReadOnlyList<Assertion> FinalAssertions { get; }
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
public sealed record GuideBreakdownEntry
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="rule">Qualified rule whose outcome was evaluated.</param>
    /// <param name="outcome">Rule outcome evaluated by the guide engine.</param>
    /// <param name="isComputed">Whether the rule was activated from other guide facts.</param>
    /// <param name="wasOverridden">Whether an active rule replaced this rule's outcome.</param>
    public GuideBreakdownEntry(RuleReference rule, JsonElement outcome, bool isComputed, bool wasOverridden)
    {
        Rule = rule ?? throw new ArgumentNullException(nameof(rule));
        Outcome = outcome;
        IsComputed = isComputed;
        WasOverridden = wasOverridden;
    }

    /// <summary>Qualified rule whose outcome was evaluated.</summary>
    public RuleReference Rule { get; }

    /// <summary>Rule outcome evaluated by the guide engine.</summary>
    public JsonElement Outcome { get; }

    /// <summary>Whether the rule was activated from other guide facts.</summary>
    public bool IsComputed { get; }

    /// <summary>Whether an active rule replaced this rule's outcome.</summary>
    public bool WasOverridden { get; }

    /// <summary>Qualified overriding rule when this outcome was replaced.</summary>
    public RuleReference? OverriddenBy { get; set; }
}

/// <summary>
/// One selected rule, named by the guide that holds it. Guides in a Context are
/// independent, so two of them may use the same rule id; a selection names the
/// guide as well as the rule, and the two pairs are two rules.
/// </summary>
public sealed record GuideRuleSelection
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="guideId">Id of the guide that holds the rule.</param>
    /// <param name="ruleId">Id of the rule within that guide.</param>
    public GuideRuleSelection(string guideId, string ruleId)
    {
        GuideId = guideId ?? throw new ArgumentNullException(nameof(guideId));
        RuleId = ruleId ?? throw new ArgumentNullException(nameof(ruleId));
    }

    /// <summary>Id of the guide that holds the rule.</summary>
    public string GuideId { get; }

    /// <summary>Id of the rule within that guide.</summary>
    public string RuleId { get; }
}

/// <summary>
/// Whether a parent marking covers its children, checked category by category.
/// A parent can fail in two directions, and each has its own list:
/// <c>Missing</c> is what the children need that the parent does not carry, and
/// <c>Excess</c> is what the parent carries that the children do not account for.
/// Which list a concept is in says whether to add it or remove it.
/// <c>Refusals</c> is null unless a rollup refused to combine the children.
/// </summary>
public sealed record CoverageReport
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="covered">True when nothing is missing, nothing is in excess, and no rollup refused.</param>
    /// <param name="missing">Concepts the children need that the parent does not carry.</param>
    /// <param name="missingLabels">Display labels for <c>Missing</c>, deduplicated and not aligned with it by position.</param>
    /// <param name="excess">Concepts the parent carries that the children do not account for.</param>
    /// <param name="excessLabels">Display labels for <c>Excess</c>, deduplicated and not aligned with it by position.</param>
    /// <param name="rendered">The children's aggregate, rendered in the requested format.</param>
    public CoverageReport(bool covered, IReadOnlyList<Assertion> missing, IReadOnlyList<string> missingLabels, IReadOnlyList<Assertion> excess, IReadOnlyList<string> excessLabels, string rendered)
    {
        Covered = covered;
        Missing = missing ?? throw new ArgumentNullException(nameof(missing));
        MissingLabels = missingLabels ?? throw new ArgumentNullException(nameof(missingLabels));
        Excess = excess ?? throw new ArgumentNullException(nameof(excess));
        ExcessLabels = excessLabels ?? throw new ArgumentNullException(nameof(excessLabels));
        Rendered = rendered ?? throw new ArgumentNullException(nameof(rendered));
    }

    /// <summary>True when nothing is missing, nothing is in excess, and no rollup refused.</summary>
    public bool Covered { get; }

    /// <summary>Concepts the children need that the parent does not carry.</summary>
    public IReadOnlyList<Assertion> Missing { get; }

    /// <summary>Display labels for <c>Missing</c>, deduplicated and not aligned with it by position.</summary>
    public IReadOnlyList<string> MissingLabels { get; }

    /// <summary>Concepts the parent carries that the children do not account for.</summary>
    public IReadOnlyList<Assertion> Excess { get; }

    /// <summary>Display labels for <c>Excess</c>, deduplicated and not aligned with it by position.</summary>
    public IReadOnlyList<string> ExcessLabels { get; }

    /// <summary>The children's aggregate, rendered in the requested format.</summary>
    public string Rendered { get; }

    /// <summary>The errors of each rollup that refused to combine the children, or null when none did.</summary>
    public IReadOnlyList<ValidationError>? Refusals { get; set; }
}

/// <summary>A concept's usage: definition, guidance, applicable rules, and pre-formatted tooltip lines.</summary>
public sealed record ConceptDescription
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="conceptId">The id of the concept described.</param>
    /// <param name="categoryId">The category the concept belongs to.</param>
    /// <param name="name">The concept's display name.</param>
    /// <param name="lines">Pre-formatted lines for tooltip display (header, description, guidance, rules).</param>
    public ConceptDescription(string conceptId, string categoryId, string name, IReadOnlyList<string> lines)
    {
        ConceptId = conceptId ?? throw new ArgumentNullException(nameof(conceptId));
        CategoryId = categoryId ?? throw new ArgumentNullException(nameof(categoryId));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Lines = lines ?? throw new ArgumentNullException(nameof(lines));
    }

    /// <summary>The id of the concept described.</summary>
    public string ConceptId { get; }

    /// <summary>The category the concept belongs to.</summary>
    public string CategoryId { get; }

    /// <summary>The concept's display name.</summary>
    public string Name { get; }

    /// <summary>Pre-formatted lines for tooltip display (header, description, guidance, rules).</summary>
    public IReadOnlyList<string> Lines { get; }

    /// <summary>What the concept means, as the scheme defines it.</summary>
    public string? Definition { get; set; }

    /// <summary>How to apply the concept, as the scheme advises.</summary>
    public string? Guidance { get; set; }

    /// <summary>The scheme's rules that involve the concept, each as a sentence.</summary>
    public IReadOnlyList<string>? Rules { get; set; }
}

/// <summary>A regulatory or handling notice attached to a concept, collected for a set of assertions.</summary>
public sealed record Notice
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="conceptId">The concept the notice is attached to.</param>
    /// <param name="conceptName">The concept's display name, for showing beside the notice.</param>
    /// <param name="type">What kind of notice this is.</param>
    /// <param name="text">The notice text the marking requires.</param>
    /// <param name="required">Whether the notice must be shown. A host surfaces a required notice before the marking is accepted.</param>
    public Notice(string conceptId, string conceptName, string type, string text, bool required)
    {
        ConceptId = conceptId ?? throw new ArgumentNullException(nameof(conceptId));
        ConceptName = conceptName ?? throw new ArgumentNullException(nameof(conceptName));
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Text = text ?? throw new ArgumentNullException(nameof(text));
        Required = required;
    }

    /// <summary>The concept the notice is attached to.</summary>
    public string ConceptId { get; }

    /// <summary>The concept's display name, for showing beside the notice.</summary>
    public string ConceptName { get; }

    /// <summary>What kind of notice this is.</summary>
    public string Type { get; }

    /// <summary>The notice text the marking requires.</summary>
    public string Text { get; }

    /// <summary>Whether the notice must be shown. A host surfaces a required notice before the marking is accepted.</summary>
    public bool Required { get; }

    /// <summary>The notice's own <c>ConceptNotice.views</c>, carried through unchanged.</summary>
    public IReadOnlyDictionary<string, string>? Views { get; set; }
}

/// <summary>Server-side description of a single access rule.</summary>
public sealed record AccessRuleDescription
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="rule">Qualified rule being described.</param>
    /// <param name="access">Access artifact that owns the rule.</param>
    /// <param name="scheme">Scheme artifact whose assertions the rule evaluates.</param>
    /// <param name="name">Human-readable rule name.</param>
    /// <param name="requirementText">Human-readable requirement expression.</param>
    /// <param name="attributesNeeded">Subject or context paths required to evaluate the rule.</param>
    /// <param name="dependsOn">Qualified rules this rule depends on.</param>
    /// <param name="overrides">Qualified rules this rule overrides.</param>
    public AccessRuleDescription(RuleReference rule, ArtifactReference access, ArtifactReference scheme, string name, string requirementText, IReadOnlyList<string> attributesNeeded, IReadOnlyList<RuleReference> dependsOn, IReadOnlyList<RuleReference> overrides)
    {
        Rule = rule ?? throw new ArgumentNullException(nameof(rule));
        Access = access ?? throw new ArgumentNullException(nameof(access));
        Scheme = scheme ?? throw new ArgumentNullException(nameof(scheme));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        RequirementText = requirementText ?? throw new ArgumentNullException(nameof(requirementText));
        AttributesNeeded = attributesNeeded ?? throw new ArgumentNullException(nameof(attributesNeeded));
        DependsOn = dependsOn ?? throw new ArgumentNullException(nameof(dependsOn));
        Overrides = overrides ?? throw new ArgumentNullException(nameof(overrides));
    }

    /// <summary>Qualified rule being described.</summary>
    public RuleReference Rule { get; }

    /// <summary>Access artifact that owns the rule.</summary>
    public ArtifactReference Access { get; }

    /// <summary>Scheme artifact whose assertions the rule evaluates.</summary>
    public ArtifactReference Scheme { get; }

    /// <summary>Human-readable rule name.</summary>
    public string Name { get; }

    /// <summary>Human-readable requirement expression.</summary>
    public string RequirementText { get; }

    /// <summary>Subject or context paths required to evaluate the rule.</summary>
    public IReadOnlyList<string> AttributesNeeded { get; }

    /// <summary>Qualified rules this rule depends on.</summary>
    public IReadOnlyList<RuleReference> DependsOn { get; }

    /// <summary>Qualified rules this rule overrides.</summary>
    public IReadOnlyList<RuleReference> Overrides { get; }

    /// <summary>Optional policy-authored description.</summary>
    public string? Description { get; set; }

    /// <summary>Human-readable activation expression.</summary>
    public string? WhenText { get; set; }
}

/// <summary>
/// Aggregated access requirements for a marking: every rule that would fire
/// plus the flat union of subject attributes the caller has to provide.
/// </summary>
public sealed record AccessRequirementsDescription
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="assertions">Normalized effective target assertions, listed in print order: the order <c>marking</c> places each concept; a concept it leaves out appears where it would print.</param>
    /// <param name="assertionDisplays">Resolved operational display (rendered marking view) for each entry in <c>assertions</c>, index-aligned. A consumer that instantiates <c>requirementText</c> against the target assertions substitutes this rather than the raw concept id, so the requirement renders the concept's marking token instead of its internal id. <c>assertions</c> keeps the machine <c>conceptId</c> keys.</param>
    /// <param name="access">Access artifact whose policy was described.</param>
    /// <param name="scheme">Scheme artifact used to normalize the target.</param>
    /// <param name="appliedRules">Qualified rules activated by the trusted target.</param>
    /// <param name="rules">Per-rule immutable-policy descriptions.</param>
    /// <param name="combinedRequirementText">Combined human-readable requirement expression.</param>
    /// <param name="combinedAttributesNeeded">Union of subject and context paths needed to evaluate the target.</param>
    public AccessRequirementsDescription(IReadOnlyList<Assertion> assertions, IReadOnlyList<string> assertionDisplays, ArtifactReference access, ArtifactReference scheme, IReadOnlyList<RuleReference> appliedRules, IReadOnlyList<AccessRuleDescription> rules, string combinedRequirementText, IReadOnlyList<string> combinedAttributesNeeded)
    {
        Assertions = assertions ?? throw new ArgumentNullException(nameof(assertions));
        AssertionDisplays = assertionDisplays ?? throw new ArgumentNullException(nameof(assertionDisplays));
        Access = access ?? throw new ArgumentNullException(nameof(access));
        Scheme = scheme ?? throw new ArgumentNullException(nameof(scheme));
        AppliedRules = appliedRules ?? throw new ArgumentNullException(nameof(appliedRules));
        Rules = rules ?? throw new ArgumentNullException(nameof(rules));
        CombinedRequirementText = combinedRequirementText ?? throw new ArgumentNullException(nameof(combinedRequirementText));
        CombinedAttributesNeeded = combinedAttributesNeeded ?? throw new ArgumentNullException(nameof(combinedAttributesNeeded));
    }

    /// <summary>Normalized effective target assertions, listed in print order: the order <c>marking</c> places each concept; a concept it leaves out appears where it would print.</summary>
    public IReadOnlyList<Assertion> Assertions { get; }

    /// <summary>Resolved operational display (rendered marking view) for each entry in <c>assertions</c>, index-aligned. A consumer that instantiates <c>requirementText</c> against the target assertions substitutes this rather than the raw concept id, so the requirement renders the concept's marking token instead of its internal id. <c>assertions</c> keeps the machine <c>conceptId</c> keys.</summary>
    public IReadOnlyList<string> AssertionDisplays { get; }

    /// <summary>Access artifact whose policy was described.</summary>
    public ArtifactReference Access { get; }

    /// <summary>Scheme artifact used to normalize the target.</summary>
    public ArtifactReference Scheme { get; }

    /// <summary>Qualified rules activated by the trusted target.</summary>
    public IReadOnlyList<RuleReference> AppliedRules { get; }

    /// <summary>Per-rule immutable-policy descriptions.</summary>
    public IReadOnlyList<AccessRuleDescription> Rules { get; }

    /// <summary>Combined human-readable requirement expression.</summary>
    public string CombinedRequirementText { get; }

    /// <summary>Union of subject and context paths needed to evaluate the target.</summary>
    public IReadOnlyList<string> CombinedAttributesNeeded { get; }

    /// <summary>Normalized marking when the caller supplied or derived one.</summary>
    public string? Marking { get; set; }
}

/// <summary>One category of a scheme view, with its concepts in declared order.</summary>
public sealed record SchemeViewCategory
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="id">The category id.</param>
    /// <param name="label">The category's display label.</param>
    /// <param name="items">The category's concepts, in declared order.</param>
    public SchemeViewCategory(string id, string label, IReadOnlyList<ConceptView> items)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Label = label ?? throw new ArgumentNullException(nameof(label));
        Items = items ?? throw new ArgumentNullException(nameof(items));
    }

    /// <summary>The category id.</summary>
    public string Id { get; }

    /// <summary>The category's display label.</summary>
    public string Label { get; }

    /// <summary>The category's concepts, in declared order.</summary>
    public IReadOnlyList<ConceptView> Items { get; }
}

/// <summary>
/// Render-ready tree of a scheme: its categories of concepts, in declared
/// order, plus the concepts that declare no category.
/// </summary>
public sealed record SchemeView
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="id">The scheme id.</param>
    /// <param name="name">The scheme's display name.</param>
    /// <param name="description">The scheme's description. Empty when the scheme declares none.</param>
    /// <param name="categories">The scheme's categories in declared order, each with the concepts that belong to it. A category with no concepts is left out.</param>
    /// <param name="uncategorized">Concepts that belong to none of the scheme's loaded categories. Usually empty.</param>
    public SchemeView(string id, string name, string description, IReadOnlyList<SchemeViewCategory> categories, IReadOnlyList<ConceptView> uncategorized)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Categories = categories ?? throw new ArgumentNullException(nameof(categories));
        Uncategorized = uncategorized ?? throw new ArgumentNullException(nameof(uncategorized));
    }

    /// <summary>The scheme id.</summary>
    public string Id { get; }

    /// <summary>The scheme's display name.</summary>
    public string Name { get; }

    /// <summary>The scheme's description. Empty when the scheme declares none.</summary>
    public string Description { get; }

    /// <summary>The scheme's categories in declared order, each with the concepts that belong to it. A category with no concepts is left out.</summary>
    public IReadOnlyList<SchemeViewCategory> Categories { get; }

    /// <summary>Concepts that belong to none of the scheme's loaded categories. Usually empty.</summary>
    public IReadOnlyList<ConceptView> Uncategorized { get; }

    /// <summary>The scheme document's own overall marking, exactly as its <c>scheme.marking</c> writes it. Display only: never interpreted. Absent when the document declares none.</summary>
    public string? Marking { get; set; }

    /// <summary>Display-term overrides the scheme declares, unresolved.</summary>
    public IReadOnlyDictionary<string, Term>? Vocabulary { get; set; }
}

/// <summary>One category of a guide view, with its rules in declared order.</summary>
public sealed record GuideViewCategory
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="id">The category id, or <c>_compound</c> for the compound rules.</param>
    /// <param name="label">The category's display label.</param>
    /// <param name="items">The category's rules, in declared order.</param>
    public GuideViewCategory(string id, string label, IReadOnlyList<GuideRuleView> items)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Label = label ?? throw new ArgumentNullException(nameof(label));
        Items = items ?? throw new ArgumentNullException(nameof(items));
    }

    /// <summary>The category id, or <c>_compound</c> for the compound rules.</summary>
    public string Id { get; }

    /// <summary>The category's display label.</summary>
    public string Label { get; }

    /// <summary>The category's rules, in declared order.</summary>
    public IReadOnlyList<GuideRuleView> Items { get; }
}

/// <summary>
/// Render-ready Guidebook tree of a guide: its categories of rules, in declared
/// order, plus the rules that declare no category.
/// </summary>
public sealed record GuideView
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="id">The guide id.</param>
    /// <param name="name">The guide's display name.</param>
    /// <param name="description">The guide's description. Empty when the guide declares none.</param>
    /// <param name="categories">The guide's categories in declared order, each with its checkable rules, followed by the <c>_compound</c> category that holds every rule with a <c>when</c> condition. A category with no rules is left out.</param>
    /// <param name="uncategorized">Checkable rules that name no category, or a category the guide does not declare.</param>
    public GuideView(string id, string name, string description, IReadOnlyList<GuideViewCategory> categories, IReadOnlyList<GuideRuleView> uncategorized)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Categories = categories ?? throw new ArgumentNullException(nameof(categories));
        Uncategorized = uncategorized ?? throw new ArgumentNullException(nameof(uncategorized));
    }

    /// <summary>The guide id.</summary>
    public string Id { get; }

    /// <summary>The guide's display name.</summary>
    public string Name { get; }

    /// <summary>The guide's description. Empty when the guide declares none.</summary>
    public string Description { get; }

    /// <summary>The guide's categories in declared order, each with its checkable rules, followed by the <c>_compound</c> category that holds every rule with a <c>when</c> condition. A category with no rules is left out.</summary>
    public IReadOnlyList<GuideViewCategory> Categories { get; }

    /// <summary>Checkable rules that name no category, or a category the guide does not declare.</summary>
    public IReadOnlyList<GuideRuleView> Uncategorized { get; }

    /// <summary>The guide document's own overall marking, exactly as its <c>guide.marking</c> writes it. Display only: never interpreted. Absent when the document declares none.</summary>
    public string? Marking { get; set; }

    /// <summary>Display-term overrides the guide declares, unresolved.</summary>
    public IReadOnlyDictionary<string, Term>? Vocabulary { get; set; }
}

/// <summary>
/// One rule of a guide as the Guidebook shows it: its name and outcome in
/// words, what a text scan matches for it, and the rules it depends on and
/// overrides, each with the display names index-aligned to the ids.
/// </summary>
public sealed record GuideRuleView
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="id">The rule id.</param>
    /// <param name="name">The rule's display name.</param>
    /// <param name="outcomeText">The rule's outcome for display: its rendered marking, or its concept ids listed in order when it has none. Empty when the outcome carries neither.</param>
    /// <param name="isComputed">True when the rule has a <c>when</c> condition and is activated by it.</param>
    /// <param name="keywords">The keywords a text scan matches for this rule.</param>
    /// <param name="matcherLabels">Labels like "IPv4 address", "CIDR block", already resolved via getMatcher.</param>
    /// <param name="dependsOn">Rule ids this compound rule references.</param>
    /// <param name="dependsOnNames">Resolved display names for each id in <c>dependsOn</c>, index-aligned, so a "depends on" line reads the rule's name rather than its raw id. <c>dependsOn</c> stays as the machine keys.</param>
    /// <param name="overrides">Rule ids this compound rule overrides when it fires.</param>
    /// <param name="overrideNames">Resolved display names for each id in <c>overrides</c>, index-aligned, so the Guidebook "Overrides:" line reads rule names rather than raw ids, matching the sibling "When:" line which already resolves display. <c>overrides</c> stays as the machine keys.</param>
    public GuideRuleView(string id, string name, string outcomeText, bool isComputed, IReadOnlyList<string> keywords, IReadOnlyList<string> matcherLabels, IReadOnlyList<string> dependsOn, IReadOnlyList<string> dependsOnNames, IReadOnlyList<string> overrides, IReadOnlyList<string> overrideNames)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        OutcomeText = outcomeText ?? throw new ArgumentNullException(nameof(outcomeText));
        IsComputed = isComputed;
        Keywords = keywords ?? throw new ArgumentNullException(nameof(keywords));
        MatcherLabels = matcherLabels ?? throw new ArgumentNullException(nameof(matcherLabels));
        DependsOn = dependsOn ?? throw new ArgumentNullException(nameof(dependsOn));
        DependsOnNames = dependsOnNames ?? throw new ArgumentNullException(nameof(dependsOnNames));
        Overrides = overrides ?? throw new ArgumentNullException(nameof(overrides));
        OverrideNames = overrideNames ?? throw new ArgumentNullException(nameof(overrideNames));
    }

    /// <summary>The rule id.</summary>
    public string Id { get; }

    /// <summary>The rule's display name.</summary>
    public string Name { get; }

    /// <summary>The rule's outcome for display: its rendered marking, or its concept ids listed in order when it has none. Empty when the outcome carries neither.</summary>
    public string OutcomeText { get; }

    /// <summary>True when the rule has a <c>when</c> condition and is activated by it.</summary>
    public bool IsComputed { get; }

    /// <summary>The keywords a text scan matches for this rule.</summary>
    public IReadOnlyList<string> Keywords { get; }

    /// <summary>Labels like "IPv4 address", "CIDR block", already resolved via getMatcher.</summary>
    public IReadOnlyList<string> MatcherLabels { get; }

    /// <summary>Rule ids this compound rule references.</summary>
    public IReadOnlyList<string> DependsOn { get; }

    /// <summary>Resolved display names for each id in <c>dependsOn</c>, index-aligned, so a "depends on" line reads the rule's name rather than its raw id. <c>dependsOn</c> stays as the machine keys.</summary>
    public IReadOnlyList<string> DependsOnNames { get; }

    /// <summary>Rule ids this compound rule overrides when it fires.</summary>
    public IReadOnlyList<string> Overrides { get; }

    /// <summary>Resolved display names for each id in <c>overrides</c>, index-aligned, so the Guidebook "Overrides:" line reads rule names rather than raw ids, matching the sibling "When:" line which already resolves display. <c>overrides</c> stays as the machine keys.</summary>
    public IReadOnlyList<string> OverrideNames { get; }

    /// <summary>The rule's description, when it declares one.</summary>
    public string? Description { get; set; }

    /// <summary>The rule's <c>when</c> condition in words. Set only on a computed rule.</summary>
    public string? WhenText { get; set; }

    /// <summary>Optional rationale text from the rule definition.</summary>
    public string? Rationale { get; set; }
}

/// <summary>
/// How an audit judged one authored marking, whole or part, separately from
/// coverage. It carries the marking's facts and how far recognition got with
/// it, so a caller that rolls markings up, checks access, or offers a
/// correction works from the audit instead of reading each marking again.
/// </summary>
public sealed record MarkingValidation
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="assertions">The facts the marking states, settled as a validate result's are, in the role the marking has in the document.</param>
    /// <param name="recognition">What recognition made of the marking, as on <see cref="ValidateResult.Recognition"/>.</param>
    /// <param name="valid">Whether the marking stands once settled, as <see cref="ValidateResult.Valid"/>.</param>
    /// <param name="inputWasValid">Whether the marking was right as the author wrote it, as <see cref="ValidateResult.InputWasValid"/>. This is the question an audit asks.</param>
    /// <param name="errors">What the scheme's rules find wrong with the settled marking.</param>
    /// <param name="proposedFixes">The ways to make the marking valid, each a whole marking the engine has proven.</param>
    /// <param name="appliedFixes">Every change the engine made while settling the marking; empty when it made none, so a false <see cref="InputWasValid"/> is always explained here or in <see cref="Errors"/>.</param>
    /// <param name="rendered">The settled marking, written in its surfaces' defaults for the role the marking has in the document.</param>
    public MarkingValidation(IReadOnlyList<Assertion> assertions, RecognitionResult recognition, bool valid, bool inputWasValid, IReadOnlyList<ValidationError> errors, IReadOnlyList<Fix> proposedFixes, IReadOnlyList<Change> appliedFixes, string rendered)
    {
        Assertions = assertions ?? throw new ArgumentNullException(nameof(assertions));
        Recognition = recognition ?? throw new ArgumentNullException(nameof(recognition));
        Valid = valid;
        InputWasValid = inputWasValid;
        Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        ProposedFixes = proposedFixes ?? throw new ArgumentNullException(nameof(proposedFixes));
        AppliedFixes = appliedFixes ?? throw new ArgumentNullException(nameof(appliedFixes));
        Rendered = rendered ?? throw new ArgumentNullException(nameof(rendered));
    }

    /// <summary>The facts the marking states, settled as a validate result's are, in the role the marking has in the document.</summary>
    public IReadOnlyList<Assertion> Assertions { get; }

    /// <summary>What recognition made of the marking, as on <see cref="ValidateResult.Recognition"/>.</summary>
    public RecognitionResult Recognition { get; }

    /// <summary>Whether the marking stands once settled, as <see cref="ValidateResult.Valid"/>.</summary>
    public bool Valid { get; }

    /// <summary>Whether the marking was right as the author wrote it, as <see cref="ValidateResult.InputWasValid"/>. This is the question an audit asks.</summary>
    public bool InputWasValid { get; }

    /// <summary>What the scheme's rules find wrong with the settled marking.</summary>
    public IReadOnlyList<ValidationError> Errors { get; }

    /// <summary>The ways to make the marking valid, each a whole marking the engine has proven.</summary>
    public IReadOnlyList<Fix> ProposedFixes { get; }

    /// <summary>Every change the engine made while settling the marking; empty when it made none, so a false <see cref="InputWasValid"/> is always explained here or in <see cref="Errors"/>.</summary>
    public IReadOnlyList<Change> AppliedFixes { get; }

    /// <summary>The settled marking, written in its surfaces' defaults for the role the marking has in the document.</summary>
    public string Rendered { get; }
}

/// <summary>A run of document text that falls outside every detected marking.</summary>
public sealed record UnmarkedSpan
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="start">Zero-based inclusive offset in the accepted document.</param>
    /// <param name="end">Zero-based exclusive offset in the accepted document.</param>
    /// <param name="offsetEncoding">Frozen byte-offset encoding.</param>
    public UnmarkedSpan(int start, int end, string offsetEncoding)
    {
        Start = start;
        End = end;
        OffsetEncoding = offsetEncoding ?? throw new ArgumentNullException(nameof(offsetEncoding));
    }

    /// <summary>Zero-based inclusive offset in the accepted document.</summary>
    public int Start { get; }

    /// <summary>Zero-based exclusive offset in the accepted document.</summary>
    public int End { get; }

    /// <summary>Frozen byte-offset encoding.</summary>
    public string OffsetEncoding { get; }

    /// <summary>Authored text, omitted outside the full response profile.</summary>
    public string? Text { get; set; }
}

/// <summary>A document-wide marking detected during an audit, with its byte span.</summary>
public sealed record AuditWhole
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="marking">The whole marking as the document writes it.</param>
    /// <param name="start">Zero-based inclusive UTF-8 byte offset.</param>
    /// <param name="end">Zero-based exclusive UTF-8 byte offset.</param>
    /// <param name="offsetEncoding">Frozen byte-offset encoding.</param>
    public AuditWhole(string marking, int start, int end, string offsetEncoding)
    {
        Marking = marking ?? throw new ArgumentNullException(nameof(marking));
        Start = start;
        End = end;
        OffsetEncoding = offsetEncoding ?? throw new ArgumentNullException(nameof(offsetEncoding));
    }

    /// <summary>The whole marking as the document writes it.</summary>
    public string Marking { get; }

    /// <summary>Zero-based inclusive UTF-8 byte offset.</summary>
    public int Start { get; }

    /// <summary>Zero-based exclusive UTF-8 byte offset.</summary>
    public int End { get; }

    /// <summary>Frozen byte-offset encoding.</summary>
    public string OffsetEncoding { get; }
}

/// <summary>Per-part audit result with optional profile-projected text and coverage/validity verdicts.</summary>
public sealed record AuditPart
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="marking">The part marking as the document writes it.</param>
    /// <param name="start">Zero-based inclusive UTF-8 byte offset.</param>
    /// <param name="end">Zero-based exclusive UTF-8 byte offset.</param>
    /// <param name="offsetEncoding">Frozen byte-offset encoding.</param>
    /// <param name="scopeId">The scope id the part is evaluated under, <c>part-</c> followed by its one-based position in the document. Guide results for the part carry the same id.</param>
    /// <param name="validation">How the part marking was judged, with the facts it states.</param>
    public AuditPart(string marking, int start, int end, string offsetEncoding, string scopeId, MarkingValidation validation)
    {
        Marking = marking ?? throw new ArgumentNullException(nameof(marking));
        Start = start;
        End = end;
        OffsetEncoding = offsetEncoding ?? throw new ArgumentNullException(nameof(offsetEncoding));
        ScopeId = scopeId ?? throw new ArgumentNullException(nameof(scopeId));
        Validation = validation ?? throw new ArgumentNullException(nameof(validation));
    }

    /// <summary>The part marking as the document writes it.</summary>
    public string Marking { get; }

    /// <summary>The text the part marking governs. Omitted outside the full response profile.</summary>
    public string? Text { get; set; }

    /// <summary>Zero-based inclusive UTF-8 byte offset.</summary>
    public int Start { get; }

    /// <summary>Zero-based exclusive UTF-8 byte offset.</summary>
    public int End { get; }

    /// <summary>Frozen byte-offset encoding.</summary>
    public string OffsetEncoding { get; }

    /// <summary>The scope id the part is evaluated under, <c>part-</c> followed by its one-based position in the document. Guide results for the part carry the same id.</summary>
    public string ScopeId { get; }

    /// <summary>How the part marking was judged, with the facts it states.</summary>
    public MarkingValidation Validation { get; }
}

/// <summary>Whether the loaded scheme requires this document to carry a whole marking and part markings.</summary>
public sealed record DocumentMarkingRequirements
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="whole">Whether an authored document-wide marking is required.</param>
    /// <param name="part">Whether authored part markings are required for document text.</param>
    public DocumentMarkingRequirements(bool whole, bool part)
    {
        Whole = whole;
        Part = part;
    }

    /// <summary>Whether an authored document-wide marking is required.</summary>
    public bool Whole { get; }

    /// <summary>Whether authored part markings are required for document text.</summary>
    public bool Part { get; }
}

/// <summary>A singular/plural display-word pair.</summary>
public sealed record Term
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="singular">The word for one, such as <c>rule</c>.</param>
    /// <param name="plural">The word for more than one, such as <c>rules</c>.</param>
    public Term(string singular, string plural)
    {
        Singular = singular ?? throw new ArgumentNullException(nameof(singular));
        Plural = plural ?? throw new ArgumentNullException(nameof(plural));
    }

    /// <summary>The word for one, such as <c>rule</c>.</summary>
    public string Singular { get; }

    /// <summary>The word for more than one, such as <c>rules</c>.</summary>
    public string Plural { get; }
}

/// <summary>
/// Composed document audit: whole-marking adequacy, per-part coverage, unmarked
/// content, and an aggregate verdict. Returned by the context document-audit op.
/// </summary>
public sealed record DocumentAuditReport
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="markingRequirements">Evaluated canonical marking requirements, independent of discovered evidence.</param>
    /// <param name="whole">First authored document-wide marking, or null when absent.</param>
    /// <param name="wholes">Every authored document-wide marking occurrence.</param>
    /// <param name="wholeValidation">How the first authored whole was judged, as a whole that must cover the authored parts that read as markings, with the facts it states. Null when the document has no authored whole.</param>
    /// <param name="parts">Authored part markings and their governed regions.</param>
    /// <param name="wholeCoversParts">Authored whole coverage of authored parts.</param>
    /// <param name="unmarkedSpans">Authored ranges not governed by a marking.</param>
    /// <param name="proceduralFlags">Stable authored-procedure findings.</param>
    /// <param name="overallVerdict">Verdict derived only from authored structural dimensions.</param>
    /// <param name="documentDigest">SHA-256 digest of the exact accepted UTF-8 document bytes.</param>
    /// <param name="limitations">Document-level assessment limitations.</param>
    /// <param name="vocabulary">Scheme-provided display vocabulary.</param>
    /// <param name="guideAssessment">Guide compilation dimension, or null when no guide applies.</param>
    public DocumentAuditReport(DocumentMarkingRequirements markingRequirements, string? whole, IReadOnlyList<AuditWhole> wholes, MarkingValidation? wholeValidation, IReadOnlyList<AuditPart> parts, CoverageReport? wholeCoversParts, IReadOnlyList<UnmarkedSpan> unmarkedSpans, IReadOnlyList<string> proceduralFlags, AuditVerdict overallVerdict, string documentDigest, IReadOnlyList<AssessmentLimitation> limitations, IReadOnlyDictionary<string, Term>? vocabulary, DocumentGuideAssessment? guideAssessment)
    {
        MarkingRequirements = markingRequirements ?? throw new ArgumentNullException(nameof(markingRequirements));
        Whole = whole;
        Wholes = wholes ?? throw new ArgumentNullException(nameof(wholes));
        WholeValidation = wholeValidation;
        Parts = parts ?? throw new ArgumentNullException(nameof(parts));
        WholeCoversParts = wholeCoversParts;
        UnmarkedSpans = unmarkedSpans ?? throw new ArgumentNullException(nameof(unmarkedSpans));
        ProceduralFlags = proceduralFlags ?? throw new ArgumentNullException(nameof(proceduralFlags));
        OverallVerdict = overallVerdict;
        DocumentDigest = documentDigest ?? throw new ArgumentNullException(nameof(documentDigest));
        Limitations = limitations ?? throw new ArgumentNullException(nameof(limitations));
        Vocabulary = vocabulary;
        GuideAssessment = guideAssessment;
    }

    /// <summary>Evaluated canonical marking requirements, independent of discovered evidence.</summary>
    public DocumentMarkingRequirements MarkingRequirements { get; }

    /// <summary>First authored document-wide marking, or null when absent.</summary>
    public string? Whole { get; }

    /// <summary>Every authored document-wide marking occurrence.</summary>
    public IReadOnlyList<AuditWhole> Wholes { get; }

    /// <summary>How the first authored whole was judged, as a whole that must cover the authored parts that read as markings, with the facts it states. Null when the document has no authored whole.</summary>
    public MarkingValidation? WholeValidation { get; }

    /// <summary>Authored part markings and their governed regions.</summary>
    public IReadOnlyList<AuditPart> Parts { get; }

    /// <summary>Authored whole coverage of authored parts.</summary>
    public CoverageReport? WholeCoversParts { get; }

    /// <summary>Authored ranges not governed by a marking.</summary>
    public IReadOnlyList<UnmarkedSpan> UnmarkedSpans { get; }

    /// <summary>Stable authored-procedure findings.</summary>
    public IReadOnlyList<string> ProceduralFlags { get; }

    /// <summary>Verdict derived only from authored structural dimensions.</summary>
    public AuditVerdict OverallVerdict { get; }

    /// <summary>SHA-256 digest of the exact accepted UTF-8 document bytes.</summary>
    public string DocumentDigest { get; }

    /// <summary>Document-level assessment limitations.</summary>
    public IReadOnlyList<AssessmentLimitation> Limitations { get; }

    /// <summary>Scheme-provided display vocabulary.</summary>
    public IReadOnlyDictionary<string, Term>? Vocabulary { get; }

    /// <summary>Guide compilation dimension, or null when no guide applies.</summary>
    public DocumentGuideAssessment? GuideAssessment { get; }
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
public sealed record ReferenceSurfaceLabel
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="singular">The noun for one, such as <c>Handbook</c>.</param>
    /// <param name="plural">The noun for more than one, such as <c>Handbooks</c>.</param>
    public ReferenceSurfaceLabel(string singular, string plural)
    {
        Singular = singular ?? throw new ArgumentNullException(nameof(singular));
        Plural = plural ?? throw new ArgumentNullException(nameof(plural));
    }

    /// <summary>The noun for one, such as <c>Handbook</c>.</summary>
    public string Singular { get; }

    /// <summary>The noun for more than one, such as <c>Handbooks</c>.</summary>
    public string Plural { get; }
}

/// <summary>
/// Render format ids grouped by the structural role they mark. For a scheme
/// whose formats are its outputs, these are the lists the scanner reads a
/// marking's role from. For a scheme that declares surfaces, each role lists
/// the forms of the views that state that role, for a caller that picks a
/// format per role.
/// </summary>
public sealed record SchemeContextFormats
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="whole">Format ids a whole marking is read in.</param>
    /// <param name="part">Format ids a part marking is read in.</param>
    public SchemeContextFormats(IReadOnlyList<string> whole, IReadOnlyList<string> part)
    {
        Whole = whole ?? throw new ArgumentNullException(nameof(whole));
        Part = part ?? throw new ArgumentNullException(nameof(part));
    }

    /// <summary>Format ids a whole marking is read in.</summary>
    public IReadOnlyList<string> Whole { get; }

    /// <summary>Format ids a part marking is read in.</summary>
    public IReadOnlyList<string> Part { get; }
}

/// <summary>
/// One entry in a scheme's ordered display-color rule list. The last rule may
/// omit <see cref="When"/> to act as the unconditional fallback.
///
/// <see cref="When"/> stays opaque JSON for the same reason
/// <see cref="GuideRule.When"/> does: it carries the recursive
/// assertion-match DSL, which this SDK passes through rather than over-models.
/// </summary>
public sealed record MarkingColorRule
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="color">Complete background/text pair to use when the condition matches.</param>
    public MarkingColorRule(ConceptColor color)
    {
        Color = color ?? throw new ArgumentNullException(nameof(color));
    }

    /// <summary>Complete background/text pair to use when the condition matches.</summary>
    public ConceptColor Color { get; }

    /// <summary>Assertion-set condition. Omit only on the final rule to create the standard unconditional/anything-else fallback.</summary>
    public JsonElement? When { get; set; }
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
public sealed record SchemeSummary
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="id">Scheme identifier from the scheme metadata.</param>
    /// <param name="version">Scheme semantic version.</param>
    /// <param name="name">Display name from the scheme metadata.</param>
    /// <param name="referenceSurfaceLabel">Display label for this scheme's own reference component, the Handbook: the scheme's override or the generic "Handbook" / "Handbooks" when it declares none. Always populated.</param>
    public SchemeSummary(string id, string version, string name, ReferenceSurfaceLabel referenceSurfaceLabel)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Version = version ?? throw new ArgumentNullException(nameof(version));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        ReferenceSurfaceLabel = referenceSurfaceLabel ?? throw new ArgumentNullException(nameof(referenceSurfaceLabel));
    }

    /// <summary>Scheme identifier from the scheme metadata.</summary>
    public string Id { get; }

    /// <summary>Scheme semantic version.</summary>
    public string Version { get; }

    /// <summary>Display name from the scheme metadata.</summary>
    public string Name { get; }

    /// <summary>Free-text description from the scheme metadata. Null when the scheme has none.</summary>
    public string? Description { get; set; }

    /// <summary>Whether the scheme uses role-aware processing. Omitted when unset.</summary>
    public bool? RoleAware { get; set; }

    /// <summary>Display label for this scheme's own reference component, the Handbook: the scheme's override or the generic "Handbook" / "Handbooks" when it declares none. Always populated.</summary>
    public ReferenceSurfaceLabel ReferenceSurfaceLabel { get; }

    /// <summary>Ways a marking may appear embedded in free text. Omitted when the scheme declares none, which is how a client tells that inline marking is off.</summary>
    public JsonElement? EmbeddedMarking { get; set; }

    /// <summary>Render format applied when a call names none. A remote adapter has no way to derive this from the formats sub-resource: format declaration order is not default order, so a client that guesses the first entry renders in the wrong format whenever the two differ.</summary>
    public string? DefaultRenderFormat { get; set; }

    /// <summary>Human-readable definition of the scheme, for tooltips and about panels.</summary>
    public string? Definition { get; set; }

    /// <summary>Usage guidance for the scheme, shown in reference views.</summary>
    public string? Guidance { get; set; }

    /// <summary>The scheme document's own overall marking, exactly as its <c>scheme.marking</c> writes it. Display only: never interpreted. Omitted when the document declares none.</summary>
    public string? Marking { get; set; }

    /// <summary>Single-category scheme-default color source, when category mode is used.</summary>
    public string? ColorFrom { get; set; }

    /// <summary>Ordered rule-driven color outcomes, when rule mode is used.</summary>
    public IReadOnlyList<MarkingColorRule>? ColorRules { get; set; }

    /// <summary>Scheme-authored display vocabulary.</summary>
    public IReadOnlyDictionary<string, Term>? Vocabulary { get; set; }

    /// <summary>Render format IDs grouped by role. For a scheme without <c>render.surfaces</c>, the lists <c>role_detection</c> gives, which the scanner reads a marking's role from. For a scheme with them, the scanner takes the role from the view a marking was read as, and these lists are derived from the views for a caller that picks a format per role: each role lists the forms of the views that state it, <c>format</c>, case formats and <c>also_read_in</c> in that order, surfaces and views in declared order and each format once.</summary>
    public SchemeContextFormats? ContextFormats { get; set; }

    /// <summary>Default scope id applied when a marking asserts no scope explicitly. Present exactly when the scheme declares scopes. Pairs with the per-category <c>scopes</c> lists on the categories sub-resource so a remote adapter can reconstruct which categories a scope-only assertion set targets: the metadata the assertion-set aggregate path needs to render scoped categories that the whole-marking format would otherwise drop.</summary>
    public string? DefaultScope { get; set; }

    /// <summary>Display label for the scope selector shown in UIs. Omitted when the scheme declares none. Carried here so <c>getSchemeMeta</c> answers the same for a remote adapter as it does for a local one; without it the selector is labeled from the scheme document in one mode and left unlabeled in the other.</summary>
    public string? ScopeLabel { get; set; }

    /// <summary>Present only for a scheme that declares <c>render.surfaces</c>: the output address a part marking is written in by default. It is the <c>surface/view</c> of the default scope's surface's default for a part, from that surface's <c>default</c>, and does not depend on <c>contextFormats</c>, which lists the formats each role is read in.</summary>
    public string? DefaultPartRenderFormat { get; set; }

    /// <summary>Present only for a scheme that declares <c>render.surfaces</c>: the output address a whole marking is written in by default, the default scope's surface's default for a whole, resolved the same way <c>defaultPartRenderFormat</c> is for a part.</summary>
    public string? DefaultWholeRenderFormat { get; set; }
}

/// <summary>
/// A regulatory or handling notice a concept carries, as the concept view
/// lists it: what has to be said, what kind of notice it is, and whether
/// showing it is mandatory. Where the text appears is the host's to decide.
/// </summary>
public sealed record ConceptNotice
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="type">What kind of notice this is.</param>
    /// <param name="required">Whether this notice MUST be displayed (regulatory requirement)</param>
    /// <param name="text">The notice text the marking requires.</param>
    public ConceptNotice(string type, bool required, string text)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Required = required;
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }

    /// <summary>What kind of notice this is.</summary>
    public string Type { get; }

    /// <summary>Whether this notice MUST be displayed (regulatory requirement)</summary>
    public bool Required { get; }

    /// <summary>The notice text the marking requires.</summary>
    public string Text { get; }

    /// <summary>Only show when this concept is also present</summary>
    public string? Condition { get; set; }

    /// <summary>The value this notice is registered under, keyed by view.</summary>
    public IReadOnlyDictionary<string, string>? Views { get; set; }
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
public sealed record ConceptView
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="id">Unique concept identifier.</param>
    /// <param name="name">Human-readable display name.</param>
    /// <param name="views">Display representations keyed by view name; includes at least <c>'default'</c>.</param>
    /// <param name="aliases">Alternative strings that parse to this concept.</param>
    /// <param name="categories">Category IDs this concept belongs to.</param>
    /// <param name="parents">Broader concept IDs forming the hierarchy chain.</param>
    /// <param name="position">Resolved rendering position within a category (lower = earlier).</param>
    public ConceptView(string id, string name, IReadOnlyDictionary<string, string> views, IReadOnlyList<string> aliases, IReadOnlyList<string> categories, IReadOnlyList<string> parents, double position)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Views = views ?? throw new ArgumentNullException(nameof(views));
        Aliases = aliases ?? throw new ArgumentNullException(nameof(aliases));
        Categories = categories ?? throw new ArgumentNullException(nameof(categories));
        Parents = parents ?? throw new ArgumentNullException(nameof(parents));
        Position = position;
    }

    /// <summary>Unique concept identifier.</summary>
    public string Id { get; }

    /// <summary>Human-readable display name.</summary>
    public string Name { get; }

    /// <summary>Display representations keyed by view name; includes at least <c>'default'</c>.</summary>
    public IReadOnlyDictionary<string, string> Views { get; }

    /// <summary>Alternative strings that parse to this concept.</summary>
    public IReadOnlyList<string> Aliases { get; }

    /// <summary>Category IDs this concept belongs to.</summary>
    public IReadOnlyList<string> Categories { get; }

    /// <summary>Broader concept IDs forming the hierarchy chain.</summary>
    public IReadOnlyList<string> Parents { get; }

    /// <summary>Resolved rendering position within a category (lower = earlier).</summary>
    public double Position { get; }

    /// <summary>Portable scheme-default display color pair, present only on concepts in the scheme's category-based <c>color_from</c> mode. Absent otherwise.</summary>
    public ConceptColor? Color { get; set; }

    /// <summary>Resolved severity rank for max/min rollup aggregation (higher = more severe).</summary>
    public double? Rank { get; set; }

    /// <summary>Tier label: marks this concept as a partial marking.</summary>
    public string? Tier { get; set; }

    /// <summary>For group concepts: the member concept IDs.</summary>
    public IReadOnlyList<string>? Members { get; set; }

    /// <summary>Scope IDs this concept is visible in.</summary>
    public IReadOnlyList<string>? Scopes { get; set; }

    /// <summary>Whether this concept is deprecated.</summary>
    public bool? Deprecated { get; set; }

    /// <summary>Human-readable deprecation message.</summary>
    public string? DeprecatedMessage { get; set; }

    /// <summary>Concept ID that should be used instead of this deprecated concept.</summary>
    public string? DeprecatedReplacement { get; set; }

    /// <summary>Definition text from the concept's documentation block.</summary>
    public string? Definition { get; set; }

    /// <summary>Where this concept came from: the provenance text the scheme records against it.</summary>
    public string? Citation { get; set; }

    /// <summary>Usage guidance from the concept's documentation block.</summary>
    public string? Guidance { get; set; }

    /// <summary>Regulatory or policy notices shown with this concept.</summary>
    public IReadOnlyList<ConceptNotice>? Notices { get; set; }

    /// <summary>Human-readable summary of rules that reference this concept. Set by <c>buildSchemeView</c>; absent from <c>getConceptsInCategory</c>.</summary>
    public IReadOnlyList<string>? RulesReferencing { get; set; }

    /// <summary>Text-input configuration for free-text concepts.</summary>
    public JsonElement? TextInput { get; set; }
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
public sealed record CategoryView
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="id">Unique category identifier.</param>
    /// <param name="label">Resolved human-readable label (always a plain string).</param>
    /// <param name="rollup">Rollup operator used when aggregating multiple markings.</param>
    public CategoryView(string id, string label, string rollup)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Label = label ?? throw new ArgumentNullException(nameof(label));
        Rollup = rollup ?? throw new ArgumentNullException(nameof(rollup));
    }

    /// <summary>Unique category identifier.</summary>
    public string Id { get; }

    /// <summary>Resolved human-readable label (always a plain string).</summary>
    public string Label { get; }

    /// <summary>Rollup operator used when aggregating multiple markings.</summary>
    public string Rollup { get; }

    /// <summary>Whether at least one concept is required for a valid marking.</summary>
    public bool? Required { get; set; }

    /// <summary>Whether only one concept can be selected in this category.</summary>
    public bool? Single { get; set; }

    /// <summary>Sort order for concepts within this category.</summary>
    public JsonElement? Sort { get; set; }

    /// <summary>Concept positions used only by this category's position sort.</summary>
    public IReadOnlyDictionary<string, double>? PositionOverrides { get; set; }

    /// <summary>Scope IDs this category is visible in.</summary>
    public IReadOnlyList<string>? Scopes { get; set; }

    /// <summary>Declared rendering position (lower = earlier). Not resolved from array order.</summary>
    public double? Position { get; set; }

    /// <summary>When true, the category grades its concepts by severity <c>rank</c>, and a coverage comparison counts a whole-marking concept here as covering a part's concept of equal or lower rank. It does not change rendering.</summary>
    public bool? HierarchyRoot { get; set; }

    /// <summary>Concept ID to assert automatically when no selection exists in this category. With <c>single: true</c>, this provides a default level. Mirrors the scheme's <c>default_concept</c> for the category.</summary>
    public JsonElement? DefaultConcept { get; set; }
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
public sealed record FormatView
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="id">The output this entry describes, named by callers choosing an output or input format: a format id, or a <c>surface/view</c> address for a scheme that declares surfaces.</param>
    /// <param name="surface">The part of a marking this format writes: a scheme scope id.</param>
    /// <param name="mode">Whether the fields run together or each goes on its own line.</param>
    /// <param name="categoryOrder">The categories this format writes, in the order it writes them.</param>
    public FormatView(string id, string surface, RenderFormatMode mode, IReadOnlyList<string> categoryOrder)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Surface = surface ?? throw new ArgumentNullException(nameof(surface));
        Mode = mode;
        CategoryOrder = categoryOrder ?? throw new ArgumentNullException(nameof(categoryOrder));
    }

    /// <summary>The output this entry describes, named by callers choosing an output or input format: a format id, or a <c>surface/view</c> address for a scheme that declares surfaces.</summary>
    public string Id { get; }

    /// <summary>The part of a marking this format writes: a scheme scope id.</summary>
    public string Surface { get; }

    /// <summary>Whether the fields run together or each goes on its own line.</summary>
    public RenderFormatMode Mode { get; }

    /// <summary>The categories this format writes, in the order it writes them.</summary>
    public IReadOnlyList<string> CategoryOrder { get; }

    /// <summary>Default concept view for fields that do not name one.</summary>
    public string? View { get; set; }

    /// <summary>Written once before the nonempty whole, when it is written unconditionally.</summary>
    public string? Prefix { get; set; }

    /// <summary>Written once after the nonempty whole, when it is written unconditionally.</summary>
    public string? Suffix { get; set; }

    /// <summary>What the format writes between two adjacent categories by default. Null for a format with no global separator, such as an XML-attribute encoding.</summary>
    public string? Separator { get; set; }

    /// <summary>What is written between two values of one category, keyed by category id.</summary>
    public IReadOnlyDictionary<string, string>? CategorySeparators { get; set; }

    /// <summary>What is written immediately before a category, keyed by category id.</summary>
    public IReadOnlyDictionary<string, string>? SeparatorBefore { get; set; }

    /// <summary>The concept view a category is written in, where it differs from <c>View</c>.</summary>
    public IReadOnlyDictionary<string, string>? CategoryViews { get; set; }

    /// <summary>How a category's tiered concepts compose into one token, outermost tier first.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<TierComposition>>? CategoryTiers { get; set; }

    /// <summary>What is written around every single value of a category.</summary>
    public IReadOnlyDictionary<string, EachAffix>? CategoryEach { get; set; }

    /// <summary>Headings this format writes, paired with the lists they head.</summary>
    public IReadOnlyList<FormatHeaderAssociation>? Headers { get; set; }

    /// <summary>The encoding, or null when the format is plain text.</summary>
    public FormatSyntax? Syntax { get; set; }

    /// <summary>The view part of <see cref="Id"/>, unescaped, for a scheme that declares surfaces. This names an output, not a concept spelling; that is <see cref="View"/>. Null for a scheme whose formats are its outputs.</summary>
    public string? OutputView { get; set; }

    /// <summary>For a scheme that declares surfaces, the roles this view is its surface's default for: a marking of each listed role is written in this view when a request names no output. Empty on the surface's other views. Null for a scheme whose formats are its outputs, which has the one default its summary reports.</summary>
    public IReadOnlyList<StructuralRole>? DefaultFor { get; set; }

    /// <summary>For a scheme that declares surfaces, the role of the markings this view is a view of. Null for a view with no role, and for a scheme whose formats are its outputs.</summary>
    public StructuralRole? Role { get; set; }
}

/// <summary>How one tier of a category's concepts attaches to the tier above it.</summary>
public sealed record TierComposition
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="tier">The tier this step writes.</param>
    /// <param name="attachedBy">The text that joins a concept of this tier to the one it belongs to.</param>
    public TierComposition(string tier, string attachedBy)
    {
        Tier = tier ?? throw new ArgumentNullException(nameof(tier));
        AttachedBy = attachedBy ?? throw new ArgumentNullException(nameof(attachedBy));
    }

    /// <summary>The tier this step writes.</summary>
    public string Tier { get; }

    /// <summary>The text that joins a concept of this tier to the one it belongs to.</summary>
    public string AttachedBy { get; }

    /// <summary>The concept view this tier is written in, when it differs.</summary>
    public string? View { get; set; }

    /// <summary>The view the owning concept is written in when this tier attaches to it.</summary>
    public string? HeadView { get; set; }
}

/// <summary>What is written around each single value of a category.</summary>
public sealed record EachAffix
{
    /// <summary>Written before each value.</summary>
    public string? Prefix { get; set; }

    /// <summary>Written after each value.</summary>
    public string? Suffix { get; set; }
}

/// <summary>A heading concept a format writes, paired with the list it heads.</summary>
public sealed record FormatHeaderAssociation
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="concept">The heading concept.</param>
    /// <param name="headerCategory">The category the heading concept belongs to.</param>
    /// <param name="category">The category whose values the heading introduces.</param>
    /// <param name="position">Whether the heading comes before or after the list.</param>
    public FormatHeaderAssociation(string concept, string headerCategory, string category, FormatHeaderPosition position)
    {
        Concept = concept ?? throw new ArgumentNullException(nameof(concept));
        HeaderCategory = headerCategory ?? throw new ArgumentNullException(nameof(headerCategory));
        Category = category ?? throw new ArgumentNullException(nameof(category));
        Position = position;
    }

    /// <summary>The heading concept.</summary>
    public string Concept { get; }

    /// <summary>The category the heading concept belongs to.</summary>
    public string HeaderCategory { get; }

    /// <summary>The category whose values the heading introduces.</summary>
    public string Category { get; }

    /// <summary>Whether the heading comes before or after the list.</summary>
    public FormatHeaderPosition Position { get; }
}

/// <summary>A format's encoding.</summary>
public sealed record FormatSyntax
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="type">The framing this format writes its text in.</param>
    public FormatSyntax(FormatSyntaxType type)
    {
        Type = type;
    }

    /// <summary>The framing this format writes its text in.</summary>
    public FormatSyntaxType Type { get; }
}

/// <summary>One scope a scheme declares, as the scopes resource lists it.</summary>
public sealed record SchemeScope
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="id">Scope identifier, referenced by <c>scopes</c> on concepts and categories.</param>
    /// <param name="label">Display label for the scope selector.</param>
    public SchemeScope(string id, string label)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Label = label ?? throw new ArgumentNullException(nameof(label));
    }

    /// <summary>Scope identifier, referenced by <c>scopes</c> on concepts and categories.</summary>
    public string Id { get; }

    /// <summary>Display label for the scope selector.</summary>
    public string Label { get; }
}

/// <summary>
/// One entry in a scheme's public label preset list. Name and scope only: the
/// preset's marking string and tag list stay server-side, so a preset is
/// expanded by calling the apply-preset operation rather than by reading it.
/// </summary>
public sealed record LabelPresetView
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="id">Preset id, which <c>applyPreset</c> takes.</param>
    /// <param name="label">Display label for the preset.</param>
    public LabelPresetView(string id, string label)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Label = label ?? throw new ArgumentNullException(nameof(label));
    }

    /// <summary>Preset id, which <c>applyPreset</c> takes.</summary>
    public string Id { get; }

    /// <summary>Display label for the preset.</summary>
    public string Label { get; }

    /// <summary>Longer explanation of when to use the preset.</summary>
    public string? Description { get; set; }

    /// <summary>Id of the scope the preset belongs to. A preset without one belongs to the scheme's <c>defaultScope</c>.</summary>
    public string? Scope { get; set; }
}
