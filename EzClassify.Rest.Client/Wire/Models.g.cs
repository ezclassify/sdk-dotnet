// Code generated from json-schemas@ff89206818fc0916b2cbe5a83fe14c015543af6a4d0deec649ce11e92bc5c360 by gen.mjs@479a632e91f14a1e23cabaf49d9ad052986d4df916f46bd2baf97c003bb3be8b. DO NOT EDIT.
// Re-run via: make regen
#nullable enable

using System.Collections.Generic;
using System.Text.Json;

namespace EzClassify;

/// <summary>A concept asserted in a category during processing, with confidence and provenance metadata.</summary>
public record Assertion
{
    /// <summary>The id of the category the concept is asserted in.</summary>
    public required string CategoryId { get; init; }

    /// <summary>The id of the concept.</summary>
    public required string ConceptId { get; init; }

    /// <summary>How the assertion was produced: <c>certain</c> when read or handed in as stated, <c>inferred</c> when an inference rule added it, or <c>repaired</c> when the engine added or rewrote it while repairing the marking.</summary>
    public required Confidence Confidence { get; init; }

    /// <summary>Where this assertion came from. An open string: the engine labels the facts it adds, and a caller or a host may label the facts it hands in however it likes.</summary>
    public required string Source { get; init; }

    /// <summary>The deprecated concept this assertion was modernized from.</summary>
    public string? ReplacedConceptId { get; init; }

    /// <summary>The id of the scheme rule that produced this assertion, when one did. Otherwise an engine-owned id beginning with <c>_</c>, set when the scheme's structure, not a rule, added the assertion. The engine writes two: <c>_tier_child_requires_parent</c> (a tier parent required by a stated child) and <c>_header_of_stated_member</c> (a header filled in for a stated member). A scheme rule id never begins with <c>_</c>.</summary>
    public string? SourceRule { get; init; }

    /// <summary>The value of a concept that takes one (<c>text_input</c>).</summary>
    public string? Value { get; init; }
}

/// <summary>A constraint violation detected during validation.</summary>
public record ValidationError
{
    /// <summary>The id of the constraint that was violated: a scheme rule's id, or an engine-reserved id beginning with <c>_</c>.</summary>
    public required string ConstraintId { get; init; }

    /// <summary>What is wrong, in words a person can act on.</summary>
    public required string Message { get; init; }

    /// <summary>Concept IDs involved in this validation failure.</summary>
    public IReadOnlyList<string>? AffectedConcepts { get; init; }

    /// <summary>Category ID this error relates to (when determinable from the constraint).</summary>
    public string? CategoryId { get; init; }

    /// <summary>The ids of the concepts the unread token was likely meant as. Never empty. They are ids for a program to act on, not spellings to show an author: <c>message</c> gives the same hint in words, naming the spellings.</summary>
    public IReadOnlyList<string>? DidYouMean { get; init; }
}

/// <summary>A non-blocking informational signal: a deprecated concept, an authored warn rule, or an engine notice.</summary>
public record Warning
{
    /// <summary>Warning kind: 'deprecation' for a deprecated concept, 'rule' for a scheme's authored <c>warn</c> rule, 'notice' for the engine's own notice.</summary>
    public required WarningKind Kind { get; init; }

    /// <summary>Human-readable message</summary>
    public required string Message { get; init; }

    /// <summary>Category this warning relates to (when applicable)</summary>
    public string? CategoryId { get; init; }

    /// <summary>Concept this warning relates to (when applicable). A rule warning names one when the rule's <c>when</c> names exactly one concept the marking holds.</summary>
    public string? ConceptId { get; init; }

    /// <summary>Resolved operational display for <c>conceptId</c> (the concept's rendered marking view), for a consumer that shows the warning next to the concept. <c>conceptId</c> stays as the machine key; this is the human-visible string.</summary>
    public string? ConceptName { get; init; }

    /// <summary>When kind=deprecation, the suggested replacement concept id (if any)</summary>
    public string? Replacement { get; init; }

    /// <summary>When kind=deprecation, the resolved operational display for <c>replacement</c> (the successor concept's rendered marking view). <c>replacement</c> stays as the machine key; this is the human-visible string.</summary>
    public string? ReplacementName { get; init; }

    /// <summary>The scheme rule that gave a <c>rule</c> warning, or the engine-reserved id of a <c>notice</c>. Absent on a deprecation warning.</summary>
    public string? RuleId { get; init; }
}

/// <summary>An autocomplete suggestion for a partial marking string.</summary>
public record Suggestion
{
    /// <summary>The category the concept belongs to.</summary>
    public required string CategoryId { get; init; }

    /// <summary>The id of the suggested concept.</summary>
    public required string ConceptId { get; init; }

    /// <summary>Secondary label (longer name or alternate view).</summary>
    public required string Description { get; init; }

    /// <summary>Category label for UI grouping.</summary>
    public required string Group { get; init; }

    /// <summary>Primary display label in the current render format.</summary>
    public required string Label { get; init; }

    /// <summary>Human-readable concept name.</summary>
    public required string Name { get; init; }

    /// <summary>Sort order.</summary>
    public required int Order { get; init; }

    /// <summary>True if this concept is already in the current marking.</summary>
    public required bool Selected { get; init; }

    /// <summary>All views for this concept.</summary>
    public required IReadOnlyDictionary<string, string> Views { get; init; }

    /// <summary>True when this concept matches the active filter but is currently gated by the scheme's rules (a rule prerequisite is unmet, or a constraint forbids it). Gated suggestions are surfaced as non-selectable, explanatory rows so the user can see why the concept is unavailable. Only populated when the caller passes <c>includeGated: true</c> and a filter is active.</summary>
    public bool? Gated { get; init; }

    /// <summary>When <c>gated</c> is true, the unmet prerequisite concept ids that would unlock this concept. Empty array when the concept is gated by a constraint with no specific prerequisite (e.g. mutual exclusion).</summary>
    public IReadOnlyList<string>? Requires { get; init; }

    /// <summary>Resolved operational display (rendered marking view) for each id in <c>requires</c>, index-aligned, so the unlock row can read "add X first" with the marking rather than the raw id. <c>requires</c> stays as the machine keys.</summary>
    public IReadOnlyList<string>? RequiresDisplay { get; init; }

    /// <summary><c>preset</c> on a row that offers a label preset, whose <c>conceptId</c> is <c>preset:</c> followed by the preset id. A concept row leaves it unset.</summary>
    public string? Source { get; init; }
}

/// <summary>What suggest answers: the concepts a picker may offer, and nothing about the marking itself.</summary>
public record SuggestResult
{
    /// <summary>The concepts on offer, in display order.</summary>
    public required IReadOnlyList<Suggestion> Suggestions { get; init; }

    /// <summary>Advisories raised while building the options, such as a concept source that could not answer in full. Empty when there were none. Advisories about the marking itself belong to validate and process.</summary>
    public required IReadOnlyList<Warning> Warnings { get; init; }
}

/// <summary>Portable scheme-default background and text colors for a resolved marking.</summary>
public record ConceptColor
{
    /// <summary>Background color as a CSS hex string.</summary>
    public required string Background { get; init; }

    /// <summary>Text/foreground color as a CSS hex string.</summary>
    public required string Text { get; init; }
}

/// <summary>One span of recognized input, by offset and source text.</summary>
public record InputSpan
{
    /// <summary>End offset in the input string.</summary>
    public required int End { get; init; }

    /// <summary>Start offset in the input string.</summary>
    public required int Start { get; init; }

    /// <summary>The text at that span.</summary>
    public required string Text { get; init; }
}

/// <summary>One input span rewritten to its canonical representation.</summary>
public record RecognitionCorrection
{
    /// <summary>End offset in the input string.</summary>
    public required int End { get; init; }

    /// <summary>The text as supplied.</summary>
    public required string Original { get; init; }

    /// <summary>What kind of departure from canonical this was.</summary>
    public required RecognitionCorrectionReason Reason { get; init; }

    /// <summary>The canonical text it was read as.</summary>
    public required string Replacement { get; init; }

    /// <summary>Start offset in the input string.</summary>
    public required int Start { get; init; }
}

/// <summary>What recognition made of an input string: how far it got, what it was read as, and what it could not account for.</summary>
public record RecognitionResult
{
    /// <summary>How far recognition got.</summary>
    public required RecognitionStatus Status { get; init; }

    /// <summary>Input not accounted for as content, syntax, or permitted whitespace.</summary>
    public required IReadOnlyList<InputSpan> Unmatched { get; init; }

    /// <summary>What the text was read as, when that is decided.</summary>
    public string? InputFormat { get; init; }

    /// <summary>Whether the text is spelled as a whole or a part marking: the role of the view it was read as, or, when no one view is decided, the role the scheme's <c>render.surfaces</c> decides for it.</summary>
    public StructuralRole? Role { get; init; }

    /// <summary>The surface the input was read as, by scheme scope id.</summary>
    public string? Surface { get; init; }
}

/// <summary>What recognition made of an input string, with the corrections that turn it into canonical text. Only process returns this form.</summary>
public record RecognitionWithCorrections
{
    /// <summary>Departures from canonical that recognition read through.</summary>
    public required IReadOnlyList<RecognitionCorrection> Corrections { get; init; }

    /// <summary>How far recognition got.</summary>
    public required RecognitionStatus Status { get; init; }

    /// <summary>Input not accounted for as content, syntax, or permitted whitespace.</summary>
    public required IReadOnlyList<InputSpan> Unmatched { get; init; }

    /// <summary>What the text was read as, when that is decided.</summary>
    public string? InputFormat { get; init; }

    /// <summary>Whether the text is spelled as a whole or a part marking: the role of the view it was read as, or, when no one view is decided, the role the scheme's <c>render.surfaces</c> decides for it.</summary>
    public StructuralRole? Role { get; init; }

    /// <summary>The surface the input was read as, by scheme scope id.</summary>
    public string? Surface { get; init; }
}

/// <summary>What validate answers about one marking: the facts it states once settled, whether they stand, and what a reader needs to show why not.</summary>
public record ValidateResult
{
    /// <summary>Every change the engine actually made while settling the marking: a departure from canonical spelling or order that recognition read through, a repair, a fact a rule required, a handed-in fact settling removed, or text read in the other role from the one the request names. A removed fact is named with the reason it was removed, including a fact handed in with a derived source label that the run does not derive again. A requirement that conflicts with an existing concept is not added and appears in errors and proposed fixes, with no applied change. Repair never removes an author-stated concept. These are the reasons a marking can be <c>valid</c> while <c>inputWasValid</c> is false.</summary>
    public required IReadOnlyList<Change> AppliedFixes { get; init; }

    /// <summary>The facts the marking states, as the engine settled them.</summary>
    public required IReadOnlyList<Assertion> Assertions { get; init; }

    /// <summary>The assessment of <c>rendered</c>: what it wrote of these facts, what it left out, what collapsed into an equivalent, and what it could not write at all. When <c>rendered</c> carries more than one surface, this is the combined assessment <c>RenderResult</c> describes. Read it before treating <c>rendered</c> as a transport.</summary>
    public required RenderAssessment Assessment { get; init; }

    /// <summary>What the scheme's rules find wrong with the settled marking. Any entry makes <c>valid</c> false.</summary>
    public required IReadOnlyList<ValidationError> Errors { get; init; }

    /// <summary>True when the text or facts as given needed no change to stand.</summary>
    public required bool InputWasValid { get; init; }

    /// <summary>The notices the scheme attaches to the concepts in <c>assertions</c>, collected the same way the notices operation collects them. Always present, empty when the marking carries none, so a consumer can read the field without checking that it exists.</summary>
    public required IReadOnlyList<Notice> Notices { get; init; }

    /// <summary>The settled marking, in one string.</summary>
    public required string Rendered { get; init; }

    /// <summary>True when the marking stands once the engine has settled it: read, repaired, completed by what the scheme's rules imply, and judged with no error left.</summary>
    public required bool Valid { get; init; }

    /// <summary>Advisories that do not affect <c>valid</c>, such as a deprecated concept. On a process result whose fix paths were not all proven, this carries a notice whose <c>ruleId</c> is <c>_fixes_unproven</c>, as <c>ProcessResult.proposedFixes</c> describes.</summary>
    public required IReadOnlyList<Warning> Warnings { get; init; }

    /// <summary>Portable scheme-derived display color for the settled marking. Embedding hosts may override this presentation default.</summary>
    public ConceptColor? DisplayColor { get; init; }

    /// <summary>What recognition made of the text: how far it got, what it read the text as, and what it could not account for.</summary>
    public RecognitionResult? Recognition { get; init; }
}

/// <summary>What process answers: everything validate answers, plus what an author needs to correct and finish the marking.</summary>
public record ProcessResult
{
    /// <summary>Every change the engine actually made while settling the marking: a departure from canonical spelling or order that recognition read through, a repair, a fact a rule required, a handed-in fact settling removed, or text read in the other role from the one the request names. A removed fact is named with the reason it was removed, including a fact handed in with a derived source label that the run does not derive again. A requirement that conflicts with an existing concept is not added and appears in errors and proposed fixes, with no applied change. Repair never removes an author-stated concept. These are the reasons a marking can be <c>valid</c> while <c>inputWasValid</c> is false.</summary>
    public required IReadOnlyList<Change> AppliedFixes { get; init; }

    /// <summary>The facts the marking states, as the engine settled them.</summary>
    public required IReadOnlyList<Assertion> Assertions { get; init; }

    /// <summary>The assessment of <c>rendered</c>: what it wrote of these facts, what it left out, what collapsed into an equivalent, and what it could not write at all. When <c>rendered</c> carries more than one surface, this is the combined assessment <c>RenderResult</c> describes. Read it before treating <c>rendered</c> as a transport.</summary>
    public required RenderAssessment Assessment { get; init; }

    /// <summary>What the scheme's rules find wrong with the settled marking. Any entry makes <c>valid</c> false.</summary>
    public required IReadOnlyList<ValidationError> Errors { get; init; }

    /// <summary>True when the text or facts as given needed no change to stand.</summary>
    public required bool InputWasValid { get; init; }

    /// <summary>The notices the scheme attaches to the concepts in <c>assertions</c>, collected the same way the notices operation collects them. Always present, empty when the marking carries none, so a consumer can read the field without checking that it exists.</summary>
    public required IReadOnlyList<Notice> Notices { get; init; }

    /// <summary>The ways to make the resulting value valid, computed from that value after applied changes, each a whole marking the engine has read back and found valid before offering it. Empty when the marking is valid, or when no change the engine could prove would make it so.</summary>
    public required IReadOnlyList<Fix> ProposedFixes { get; init; }

    /// <summary>The settled marking, in one string.</summary>
    public required string Rendered { get; init; }

    /// <summary>The outputs the request named in <c>outputs</c>, one entry each, keyed by output address. Empty when the request named none.</summary>
    public required IReadOnlyDictionary<string, RenderResult> Renders { get; init; }

    /// <summary>True when the marking stands once the engine has settled it: read, repaired, completed by what the scheme's rules imply, and judged with no error left.</summary>
    public required bool Valid { get; init; }

    /// <summary>Advisories that do not affect <c>valid</c>, such as a deprecated concept. On a process result whose fix paths were not all proven, this carries a notice whose <c>ruleId</c> is <c>_fixes_unproven</c>, as <c>ProcessResult.proposedFixes</c> describes.</summary>
    public required IReadOnlyList<Warning> Warnings { get; init; }

    /// <summary>Portable scheme-derived display color for the settled marking. Embedding hosts may override this presentation default.</summary>
    public ConceptColor? DisplayColor { get; init; }

    /// <summary>What recognition made of the text, with the corrections that turn it into canonical text. Present exactly when there was text to recognize.</summary>
    public RecognitionWithCorrections? Recognition { get; init; }

    /// <summary>The options for each category the request named in <c>suggestFor</c>, answered against these settled facts exactly as <c>suggestForCategories</c> answers them, in the request's output format and surface.</summary>
    public IReadOnlyDictionary<string, SuggestResult>? SuggestionsFor { get; init; }
}

/// <summary>Output of combining multiple inputs into a single rendered marking.</summary>
public record AggregateResult
{
    /// <summary>The changes the rollup made to reach the combined marking.</summary>
    public required IReadOnlyList<Change> AppliedFixes { get; init; }

    /// <summary>The assertions of the combined marking, listed in print order as a validate result's are: the order the rendered marking places each concept; a concept the rendering leaves out appears where it would print.</summary>
    public required IReadOnlyList<Assertion> Assertions { get; init; }

    /// <summary>The constraints the combined marking violates.</summary>
    public required IReadOnlyList<ValidationError> Errors { get; init; }

    /// <summary>The notices the scheme attaches to the concepts in <c>assertions</c>, collected the same way the notices operation collects them. Always present, empty when the aggregate carries none, so a consumer can read the field without checking that it exists.</summary>
    public required IReadOnlyList<Notice> Notices { get; init; }

    /// <summary>Always empty: aggregate applies every fix it can.</summary>
    public required IReadOnlyList<Fix> ProposedFixes { get; init; }

    /// <summary>The combined marking, written in the output format.</summary>
    public required string Rendered { get; init; }

    /// <summary>True when the combined marking stands once settled, as <c>ValidateResult.valid</c> defines it.</summary>
    public required bool Valid { get; init; }

    /// <summary>Advisory signals about the combined marking.</summary>
    public required IReadOnlyList<Warning> Warnings { get; init; }

    /// <summary>Portable scheme-derived display color for the final post-rollup marking. Embedding hosts may override this presentation default.</summary>
    public ConceptColor? DisplayColor { get; init; }
}

/// <summary>Comparison of two assertion sets: added, removed, and common assertions.</summary>
public record DiffResult
{
    /// <summary>Assertions in the second marking and not the first.</summary>
    public required IReadOnlyList<Assertion> Added { get; init; }

    /// <summary>Assertions in both markings, as the first marking holds them. Concept and category decide sameness; confidence, source and value are not compared.</summary>
    public required IReadOnlyList<Assertion> Common { get; init; }

    /// <summary>Assertions in the first marking and not the second.</summary>
    public required IReadOnlyList<Assertion> Removed { get; init; }
}

/// <summary>A single marking found during a free-text scan of the source text, with what validate answers for it.</summary>
public record ScanMatch
{
    /// <summary>End position in the (possibly stripped) input text</summary>
    public required int End { get; init; }

    /// <summary>The matched marking string (as found in the document)</summary>
    public required string Marking { get; init; }

    /// <summary>What validate answers for this marking, read where it was found and in the role <c>role</c> names. A scan reads documents; it does not author them, so it carries no fix paths. A caller that wants to correct one region processes that region.</summary>
    public required ValidateResult Result { get; init; }

    /// <summary>Scheme-resolved structural role of this marking: <c>'whole'</c> (a document-wide marking: a page-top marking, a TLP document tag) or <c>'part'</c> (a per-region marking: a paragraph mark, a segment, a tag).</summary>
    public required StructuralRole Role { get; init; }

    /// <summary>Start position in the (possibly stripped) input text</summary>
    public required int Start { get; init; }

    /// <summary>The output this marking is written in when the scan names no output format: <c>result.rendered</c> is written in it.</summary>
    public required string Type { get; init; }

    /// <summary>Whether the marking was found inside a wrapper pattern (e.g., parentheses)</summary>
    public required bool Wrapped { get; init; }

    /// <summary>The marking text inside the wrapper: the capture group of the scheme's <c>embeddedMarking</c> pattern. Present only when <c>wrapped</c> is true. Consumers that need to re-process the marking must use this rather than stripping delimiters from <c>marking</c>, because the wrapper shape is scheme-declared and is not always a pair of parentheses.</summary>
    public string? Inner { get; init; }
}

/// <summary>How far a scan got, and what stopped it when something did.</summary>
public record ScanLimits
{
    /// <summary>Candidate regions the scan considered.</summary>
    public required int RegionsFound { get; init; }

    /// <summary>Whether the document was read to the end, and if not, why not.</summary>
    public required ScanStatus Status { get; init; }

    /// <summary>The ceiling that stopped the scan, when one did.</summary>
    public RecognitionLimit? LimitHit { get; init; }

    /// <summary>Whether the per-document region cap was reached.</summary>
    public bool? RegionsCapped { get; init; }
}

/// <summary>A wrapped mark a scan found but could not read.</summary>
public record UnreadMark
{
    /// <summary>End position in the input text</summary>
    public required int End { get; init; }

    /// <summary>The text inside the wrapper: the capture group of the scheme's <c>embeddedMarking</c> pattern.</summary>
    public required string Inner { get; init; }

    /// <summary>Whether the mark stands where a part mark goes: at the start of its paragraph. Both of these hold for a leading mark.</summary>
    public required bool Leading { get; init; }

    /// <summary>Start position in the input text</summary>
    public required int Start { get; init; }

    /// <summary>The wrapped text as found in the document, wrapper included.</summary>
    public required string Text { get; init; }
}

/// <summary>A byte-offset span where a guide rule keyword or pattern matched in scanned text.</summary>
public record GuideScanSpan
{
    /// <summary>Offset in the scanned text just past the end of the match, in UTF-16 code units.</summary>
    public required int End { get; init; }

    /// <summary>The keyword that matched, or the label of the matcher that did.</summary>
    public required string Keyword { get; init; }

    /// <summary>Offset in the scanned text where the match starts, in UTF-16 code units.</summary>
    public required int Start { get; init; }
}

/// <summary>A rule in a marking guide, paired with its scan metadata.</summary>
public record GuideRule
{
    /// <summary>Unique identifier within the guide.</summary>
    public required string Id { get; init; }

    /// <summary>Human-readable display name.</summary>
    public required string Name { get; init; }

    /// <summary>What this rule contributes when active. At least one of rendered or concepts must be set.</summary>
    public required JsonElement Outcome { get; init; }

    /// <summary>Guide category ID for grouping in the UI.</summary>
    public string? Category { get; init; }

    /// <summary>Description of the data element this rule covers.</summary>
    public string? Description { get; init; }

    /// <summary>Searchable terms for literal keyword filtering (absent on computed rules).</summary>
    public IReadOnlyList<string>? Keywords { get; init; }

    /// <summary>Named pattern matchers for content evaluation. Each entry is a <c>MatchNode</c>: a leaf spec referencing a built-in matcher (e.g. <c>{ type: 'ipv4' }</c>) or a custom regex (<c>{ type: 'regex', pattern: '...' }</c>), or a combinator node (<c>any</c> / <c>all</c> / <c>not</c> / <c>near</c> / <c>context</c> / <c>validate</c>) composed over child nodes. When content is evaluated, both <c>keywords</c> and <c>match</c> are checked.</summary>
    public IReadOnlyList<JsonElement>? Match { get; init; }

    /// <summary>Rule IDs whose outcomes this rule replaces when active. When omitted (default), this rule's outcome is additive: it contributes alongside the rules it depends on.</summary>
    public IReadOnlyList<string>? Overrides { get; init; }

    /// <summary>Explanation of why this outcome applies.</summary>
    public string? Rationale { get; init; }

    /// <summary>When present, the rule is computed: it activates when this block holds over the rules the user selected, and it is not offered as a checkbox.</summary>
    public JsonElement? When { get; init; }
}

/// <summary>A guide rule matched by text scanning, with keyword spans.</summary>
public record GuideScanResult
{
    /// <summary>The id of the guide the rule belongs to. Rule ids are unique only within a guide, so a selection built from this hit names this guide.</summary>
    public required string GuideId { get; init; }

    /// <summary>Each keyword or matcher label that fired, once each.</summary>
    public required IReadOnlyList<string> MatchedKeywords { get; init; }

    /// <summary>The guide rule that matched.</summary>
    public required GuideRule Rule { get; init; }

    /// <summary>Every place in the text where the rule fired, in the order found.</summary>
    public required IReadOnlyList<GuideScanSpan> Spans { get; init; }
}

/// <summary>Output of evaluating selected guide rules into a single aggregate marking.</summary>
public record EvaluationResult
{
    /// <summary>The computed rules whose <c>when</c> condition the selection satisfied.</summary>
    public required IReadOnlyList<GuideRule> ActiveComputedRules { get; init; }

    /// <summary>The effective markings rolled up into one marking by the target scheme. Empty when no rule contributed, or when the result spans more than one target scheme, in which case <c>aggregatedMarkings</c> holds one per scheme.</summary>
    public required string AggregatedMarking { get; init; }

    /// <summary>One entry per evaluated rule: for each guide, its selected rules, then its computed rules.</summary>
    public required IReadOnlyList<GuideBreakdownEntry> Breakdown { get; init; }

    /// <summary>Subset of <c>breakdown</c> that actually contributed to <c>aggregatedMarking</c> (i.e. <c>breakdown.filter(b =&gt; !b.wasOverridden)</c>). Precomputed so UI layers stop re-filtering the full breakdown.</summary>
    public required IReadOnlyList<GuideBreakdownEntry> ContributingBreakdown { get; init; }

    /// <summary>The marking each contributing rule's outcome resolves to. Overridden rules contribute none.</summary>
    public required IReadOnlyList<string> EffectiveMarkings { get; init; }

    /// <summary>The selected rules that were evaluated. Rules with a <c>when</c> condition are computed, not selected, and are not listed here.</summary>
    public required IReadOnlyList<GuideRule> SelectedRules { get; init; }

    /// <summary>Aggregated output keyed by target scheme id. Present when guide evaluation routed through more than one target scheme.</summary>
    public IReadOnlyDictionary<string, string>? AggregatedMarkings { get; init; }

    /// <summary>The id of the guide evaluated. Absent when the result combines several guides.</summary>
    public string? GuideId { get; init; }
}

/// <summary>One deprecated-to-current concept substitution that processing applied before an access rule was evaluated.</summary>
public record ConceptModernization
{
    /// <summary>Resolved operational display for <c>conceptId</c>.</summary>
    public required string ConceptDisplay { get; init; }

    /// <summary>Current concept the access rule was evaluated against.</summary>
    public required string ConceptId { get; init; }

    /// <summary>Resolved operational display for <c>replacedConceptId</c>.</summary>
    public required string ReplacedConceptDisplay { get; init; }

    /// <summary>Deprecated concept the data was actually marked with.</summary>
    public required string ReplacedConceptId { get; init; }
}

/// <summary>Result of evaluating a single access rule entry.</summary>
public record AccessRuleResult
{
    /// <summary>Machine-readable activation basis.</summary>
    public required AccessRuleActivation Activation { get; init; }

    /// <summary>Machine-readable comparisons evaluated by the rule.</summary>
    public required IReadOnlyList<AccessComparisonResult> Comparisons { get; init; }

    /// <summary>Whether this rule passed.</summary>
    public required bool Granted { get; init; }

    /// <summary>Rule name from the access policy (for display).</summary>
    public required string Name { get; init; }

    /// <summary>Qualified rule identity and semantic digest.</summary>
    public required RuleReference Rule { get; init; }

    /// <summary>Zero-based index of the rule in the access policy's evaluated rules.</summary>
    public required int RuleIndex { get; init; }

    /// <summary>Whether the rule was skipped because its activation did not match.</summary>
    public required bool Skipped { get; init; }

    /// <summary>Deprecated-to-current concept substitutions that processing applied to the data before this rule was evaluated.</summary>
    public IReadOnlyList<ConceptModernization>? ModernizedFrom { get; init; }

    /// <summary>Denial reason from the rule definition or generated explanation.</summary>
    public string? Reason { get; init; }
}

/// <summary>Overall access decision with per-rule breakdown for a labeled artifact.</summary>
public record AccessDecision
{
    /// <summary>True when every rule that was not skipped granted access.</summary>
    public required bool Granted { get; init; }

    /// <summary>One result per rule in the policy, in policy order.</summary>
    public required IReadOnlyList<AccessRuleResult> Rules { get; init; }

    /// <summary>Warnings from preparing and evaluating the target, including automatic changes. Present on granted and denied decisions; empty when none apply.</summary>
    public required IReadOnlyList<Warning> Warnings { get; init; }
}
