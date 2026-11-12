// Code generated from json-schemas@ff89206818fc0916b2cbe5a83fe14c015543af6a4d0deec649ce11e92bc5c360 by gen.mjs@75f2fcb0c19a039775bfff57328b8295df0840efe327ff36e7f5a6965b31b13c. DO NOT EDIT.
// Re-run via: make regen
#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json;

namespace EzClassify;

/// <summary>A concept asserted in a category during processing, with confidence and provenance metadata.</summary>
public sealed record Assertion
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="categoryId">The id of the category the concept is asserted in.</param>
    /// <param name="conceptId">The id of the concept.</param>
    /// <param name="confidence">How the assertion was produced: <c>certain</c> when read or handed in as stated, <c>inferred</c> when an inference rule added it, or <c>repaired</c> when the engine added or rewrote it while repairing the marking.</param>
    /// <param name="source">Where this assertion came from. An open string: the engine labels the facts it adds, and a caller or a host may label the facts it hands in however it likes.</param>
    public Assertion(string categoryId, string conceptId, Confidence confidence, string source)
    {
        CategoryId = categoryId ?? throw new ArgumentNullException(nameof(categoryId));
        ConceptId = conceptId ?? throw new ArgumentNullException(nameof(conceptId));
        Confidence = confidence;
        Source = source ?? throw new ArgumentNullException(nameof(source));
    }

    /// <summary>The id of the category the concept is asserted in.</summary>
    public string CategoryId { get; }

    /// <summary>The id of the concept.</summary>
    public string ConceptId { get; }

    /// <summary>How the assertion was produced: <c>certain</c> when read or handed in as stated, <c>inferred</c> when an inference rule added it, or <c>repaired</c> when the engine added or rewrote it while repairing the marking.</summary>
    public Confidence Confidence { get; }

    /// <summary>Where this assertion came from. An open string: the engine labels the facts it adds, and a caller or a host may label the facts it hands in however it likes.</summary>
    public string Source { get; }

    /// <summary>The deprecated concept this assertion was modernized from.</summary>
    public string? ReplacedConceptId { get; set; }

    /// <summary>The id of the scheme rule that produced this assertion, when one did. Otherwise an engine-owned id beginning with <c>_</c>, set when the scheme's structure, not a rule, added the assertion. The engine writes two: <c>_tier_child_requires_parent</c> (a tier parent required by a stated child) and <c>_header_of_stated_member</c> (a header filled in for a stated member). A scheme rule id never begins with <c>_</c>.</summary>
    public string? SourceRule { get; set; }

    /// <summary>The value of a concept that takes one (<c>text_input</c>).</summary>
    public string? Value { get; set; }
}

/// <summary>A constraint violation detected during validation.</summary>
public sealed record ValidationError
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="constraintId">The id of the constraint that was violated: a scheme rule's id, or an engine-reserved id beginning with <c>_</c>.</param>
    /// <param name="message">What is wrong, in words a person can act on.</param>
    public ValidationError(string constraintId, string message)
    {
        ConstraintId = constraintId ?? throw new ArgumentNullException(nameof(constraintId));
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    /// <summary>The id of the constraint that was violated: a scheme rule's id, or an engine-reserved id beginning with <c>_</c>.</summary>
    public string ConstraintId { get; }

    /// <summary>What is wrong, in words a person can act on.</summary>
    public string Message { get; }

    /// <summary>Concept IDs involved in this validation failure.</summary>
    public IReadOnlyList<string>? AffectedConcepts { get; set; }

    /// <summary>Category ID this error relates to (when determinable from the constraint).</summary>
    public string? CategoryId { get; set; }

    /// <summary>The ids of the concepts the unread token was likely meant as. Never empty. They are ids for a program to act on, not spellings to show an author: <c>message</c> gives the same hint in words, naming the spellings.</summary>
    public IReadOnlyList<string>? DidYouMean { get; set; }
}

/// <summary>A non-blocking informational signal: a deprecated concept, an authored warn rule, or an engine notice.</summary>
public sealed record Warning
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="kind">Warning kind: 'deprecation' for a deprecated concept, 'rule' for a scheme's authored <c>warn</c> rule, 'notice' for the engine's own notice.</param>
    /// <param name="message">Human-readable message</param>
    public Warning(WarningKind kind, string message)
    {
        Kind = kind;
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    /// <summary>Warning kind: 'deprecation' for a deprecated concept, 'rule' for a scheme's authored <c>warn</c> rule, 'notice' for the engine's own notice.</summary>
    public WarningKind Kind { get; }

    /// <summary>Human-readable message</summary>
    public string Message { get; }

    /// <summary>Category this warning relates to (when applicable)</summary>
    public string? CategoryId { get; set; }

    /// <summary>Concept this warning relates to (when applicable). A rule warning names one when the rule's <c>when</c> names exactly one concept the marking holds.</summary>
    public string? ConceptId { get; set; }

    /// <summary>Resolved operational display for <c>conceptId</c> (the concept's rendered marking view), for a consumer that shows the warning next to the concept. <c>conceptId</c> stays as the machine key; this is the human-visible string.</summary>
    public string? ConceptName { get; set; }

    /// <summary>When kind=deprecation, the suggested replacement concept id (if any)</summary>
    public string? Replacement { get; set; }

    /// <summary>When kind=deprecation, the resolved operational display for <c>replacement</c> (the successor concept's rendered marking view). <c>replacement</c> stays as the machine key; this is the human-visible string.</summary>
    public string? ReplacementName { get; set; }

    /// <summary>The scheme rule that gave a <c>rule</c> warning, or the engine-reserved id of a <c>notice</c>. Absent on a deprecation warning.</summary>
    public string? RuleId { get; set; }
}

/// <summary>An autocomplete suggestion for a partial marking string.</summary>
public sealed record Suggestion
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="categoryId">The category the concept belongs to.</param>
    /// <param name="conceptId">The id of the suggested concept.</param>
    /// <param name="description">Secondary label (longer name or alternate view).</param>
    /// <param name="group">Category label for UI grouping.</param>
    /// <param name="label">Primary display label in the current render format.</param>
    /// <param name="name">Human-readable concept name.</param>
    /// <param name="order">Sort order.</param>
    /// <param name="selected">True if this concept is already in the current marking.</param>
    /// <param name="views">All views for this concept.</param>
    public Suggestion(string categoryId, string conceptId, string description, string group, string label, string name, int order, bool selected, IReadOnlyDictionary<string, string> views)
    {
        CategoryId = categoryId ?? throw new ArgumentNullException(nameof(categoryId));
        ConceptId = conceptId ?? throw new ArgumentNullException(nameof(conceptId));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Group = group ?? throw new ArgumentNullException(nameof(group));
        Label = label ?? throw new ArgumentNullException(nameof(label));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Order = order;
        Selected = selected;
        Views = views ?? throw new ArgumentNullException(nameof(views));
    }

    /// <summary>The category the concept belongs to.</summary>
    public string CategoryId { get; }

    /// <summary>The id of the suggested concept.</summary>
    public string ConceptId { get; }

    /// <summary>Secondary label (longer name or alternate view).</summary>
    public string Description { get; }

    /// <summary>Category label for UI grouping.</summary>
    public string Group { get; }

    /// <summary>Primary display label in the current render format.</summary>
    public string Label { get; }

    /// <summary>Human-readable concept name.</summary>
    public string Name { get; }

    /// <summary>Sort order.</summary>
    public int Order { get; }

    /// <summary>True if this concept is already in the current marking.</summary>
    public bool Selected { get; }

    /// <summary>All views for this concept.</summary>
    public IReadOnlyDictionary<string, string> Views { get; }

    /// <summary>True when this concept matches the active filter but is currently gated by the scheme's rules (a rule prerequisite is unmet, or a constraint forbids it). Gated suggestions are surfaced as non-selectable, explanatory rows so the user can see why the concept is unavailable. Only populated when the caller passes <c>includeGated: true</c> and a filter is active.</summary>
    public bool? Gated { get; set; }

    /// <summary>When <c>gated</c> is true, the unmet prerequisite concept ids that would unlock this concept. Empty array when the concept is gated by a constraint with no specific prerequisite (e.g. mutual exclusion).</summary>
    public IReadOnlyList<string>? Requires { get; set; }

    /// <summary>Resolved operational display (rendered marking view) for each id in <c>requires</c>, index-aligned, so the unlock row can read "add X first" with the marking rather than the raw id. <c>requires</c> stays as the machine keys.</summary>
    public IReadOnlyList<string>? RequiresDisplay { get; set; }

    /// <summary><c>preset</c> on a row that offers a label preset, whose <c>conceptId</c> is <c>preset:</c> followed by the preset id. A concept row leaves it unset.</summary>
    public string? Source { get; set; }
}

/// <summary>What suggest answers: the concepts a picker may offer, and nothing about the marking itself.</summary>
public sealed record SuggestResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="suggestions">The concepts on offer, in display order.</param>
    /// <param name="warnings">Advisories raised while building the options, such as a concept source that could not answer in full. Empty when there were none. Advisories about the marking itself belong to validate and process.</param>
    public SuggestResult(IReadOnlyList<Suggestion> suggestions, IReadOnlyList<Warning> warnings)
    {
        Suggestions = suggestions ?? throw new ArgumentNullException(nameof(suggestions));
        Warnings = warnings ?? throw new ArgumentNullException(nameof(warnings));
    }

    /// <summary>The concepts on offer, in display order.</summary>
    public IReadOnlyList<Suggestion> Suggestions { get; }

    /// <summary>Advisories raised while building the options, such as a concept source that could not answer in full. Empty when there were none. Advisories about the marking itself belong to validate and process.</summary>
    public IReadOnlyList<Warning> Warnings { get; }
}

/// <summary>Portable scheme-default background and text colors for a resolved marking.</summary>
public sealed record ConceptColor
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="background">Background color as a CSS hex string.</param>
    /// <param name="text">Text/foreground color as a CSS hex string.</param>
    public ConceptColor(string background, string text)
    {
        Background = background ?? throw new ArgumentNullException(nameof(background));
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }

    /// <summary>Background color as a CSS hex string.</summary>
    public string Background { get; }

    /// <summary>Text/foreground color as a CSS hex string.</summary>
    public string Text { get; }
}

/// <summary>One span of recognized input, by offset and source text.</summary>
public sealed record InputSpan
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="end">End offset in the input string.</param>
    /// <param name="start">Start offset in the input string.</param>
    /// <param name="text">The text at that span.</param>
    public InputSpan(int end, int start, string text)
    {
        End = end;
        Start = start;
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }

    /// <summary>End offset in the input string.</summary>
    public int End { get; }

    /// <summary>Start offset in the input string.</summary>
    public int Start { get; }

    /// <summary>The text at that span.</summary>
    public string Text { get; }
}

/// <summary>One input span rewritten to its canonical representation.</summary>
public sealed record RecognitionCorrection
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="end">End offset in the input string.</param>
    /// <param name="original">The text as supplied.</param>
    /// <param name="reason">What kind of departure from canonical this was.</param>
    /// <param name="replacement">The canonical text it was read as.</param>
    /// <param name="start">Start offset in the input string.</param>
    public RecognitionCorrection(int end, string original, RecognitionCorrectionReason reason, string replacement, int start)
    {
        End = end;
        Original = original ?? throw new ArgumentNullException(nameof(original));
        Reason = reason;
        Replacement = replacement ?? throw new ArgumentNullException(nameof(replacement));
        Start = start;
    }

    /// <summary>End offset in the input string.</summary>
    public int End { get; }

    /// <summary>The text as supplied.</summary>
    public string Original { get; }

    /// <summary>What kind of departure from canonical this was.</summary>
    public RecognitionCorrectionReason Reason { get; }

    /// <summary>The canonical text it was read as.</summary>
    public string Replacement { get; }

    /// <summary>Start offset in the input string.</summary>
    public int Start { get; }
}

/// <summary>What recognition made of an input string: how far it got, what it was read as, and what it could not account for.</summary>
public sealed record RecognitionResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="status">How far recognition got.</param>
    /// <param name="unmatched">Input not accounted for as content, syntax, or permitted whitespace.</param>
    public RecognitionResult(RecognitionStatus status, IReadOnlyList<InputSpan> unmatched)
    {
        Status = status;
        Unmatched = unmatched ?? throw new ArgumentNullException(nameof(unmatched));
    }

    /// <summary>How far recognition got.</summary>
    public RecognitionStatus Status { get; }

    /// <summary>Input not accounted for as content, syntax, or permitted whitespace.</summary>
    public IReadOnlyList<InputSpan> Unmatched { get; }

    /// <summary>What the text was read as, when that is decided.</summary>
    public string? InputFormat { get; set; }

    /// <summary>Whether the text is spelled as a whole or a part marking: the role of the view it was read as, or, when no one view is decided, the role the scheme's <c>render.surfaces</c> decides for it.</summary>
    public StructuralRole? Role { get; set; }

    /// <summary>The surface the input was read as, by scheme scope id.</summary>
    public string? Surface { get; set; }
}

/// <summary>What recognition made of an input string, with the corrections that turn it into canonical text. Only process returns this form.</summary>
public sealed record RecognitionWithCorrections
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="corrections">Departures from canonical that recognition read through.</param>
    /// <param name="status">How far recognition got.</param>
    /// <param name="unmatched">Input not accounted for as content, syntax, or permitted whitespace.</param>
    public RecognitionWithCorrections(IReadOnlyList<RecognitionCorrection> corrections, RecognitionStatus status, IReadOnlyList<InputSpan> unmatched)
    {
        Corrections = corrections ?? throw new ArgumentNullException(nameof(corrections));
        Status = status;
        Unmatched = unmatched ?? throw new ArgumentNullException(nameof(unmatched));
    }

    /// <summary>Departures from canonical that recognition read through.</summary>
    public IReadOnlyList<RecognitionCorrection> Corrections { get; }

    /// <summary>How far recognition got.</summary>
    public RecognitionStatus Status { get; }

    /// <summary>Input not accounted for as content, syntax, or permitted whitespace.</summary>
    public IReadOnlyList<InputSpan> Unmatched { get; }

    /// <summary>What the text was read as, when that is decided.</summary>
    public string? InputFormat { get; set; }

    /// <summary>Whether the text is spelled as a whole or a part marking: the role of the view it was read as, or, when no one view is decided, the role the scheme's <c>render.surfaces</c> decides for it.</summary>
    public StructuralRole? Role { get; set; }

    /// <summary>The surface the input was read as, by scheme scope id.</summary>
    public string? Surface { get; set; }
}

/// <summary>What validate answers about one marking: the facts it states once settled, whether they stand, and what a reader needs to show why not.</summary>
public sealed record ValidateResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="appliedFixes">Every change the engine actually made while settling the marking: a departure from canonical spelling or order that recognition read through, a repair, a fact a rule required, a handed-in fact settling removed, or text read in the other role from the one the request names. A removed fact is named with the reason it was removed, including a fact handed in with a derived source label that the run does not derive again. A requirement that conflicts with an existing concept is not added and appears in errors and proposed fixes, with no applied change. Repair never removes an author-stated concept. These are the reasons a marking can be <c>valid</c> while <c>inputWasValid</c> is false.</param>
    /// <param name="assertions">The facts the marking states, as the engine settled them.</param>
    /// <param name="assessment">The assessment of <c>rendered</c>: what it wrote of these facts, what it left out, what collapsed into an equivalent, and what it could not write at all. When <c>rendered</c> carries more than one surface, this is the combined assessment <c>RenderResult</c> describes. Read it before treating <c>rendered</c> as a transport.</param>
    /// <param name="errors">What the scheme's rules find wrong with the settled marking. Any entry makes <c>valid</c> false.</param>
    /// <param name="inputWasValid">True when the text or facts as given needed no change to stand.</param>
    /// <param name="notices">The notices the scheme attaches to the concepts in <c>assertions</c>, collected the same way the notices operation collects them. Always present, empty when the marking carries none, so a consumer can read the field without checking that it exists.</param>
    /// <param name="rendered">The settled marking, in one string.</param>
    /// <param name="valid">True when the marking stands once the engine has settled it: read, repaired, completed by what the scheme's rules imply, and judged with no error left.</param>
    /// <param name="warnings">Advisories that do not affect <c>valid</c>, such as a deprecated concept. On a process result whose fix paths were not all proven, this carries a notice whose <c>ruleId</c> is <c>_fixes_unproven</c>, as <c>ProcessResult.proposedFixes</c> describes.</param>
    public ValidateResult(IReadOnlyList<Change> appliedFixes, IReadOnlyList<Assertion> assertions, RenderAssessment assessment, IReadOnlyList<ValidationError> errors, bool inputWasValid, IReadOnlyList<Notice> notices, string rendered, bool valid, IReadOnlyList<Warning> warnings)
    {
        AppliedFixes = appliedFixes ?? throw new ArgumentNullException(nameof(appliedFixes));
        Assertions = assertions ?? throw new ArgumentNullException(nameof(assertions));
        Assessment = assessment ?? throw new ArgumentNullException(nameof(assessment));
        Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        InputWasValid = inputWasValid;
        Notices = notices ?? throw new ArgumentNullException(nameof(notices));
        Rendered = rendered ?? throw new ArgumentNullException(nameof(rendered));
        Valid = valid;
        Warnings = warnings ?? throw new ArgumentNullException(nameof(warnings));
    }

    /// <summary>Every change the engine actually made while settling the marking: a departure from canonical spelling or order that recognition read through, a repair, a fact a rule required, a handed-in fact settling removed, or text read in the other role from the one the request names. A removed fact is named with the reason it was removed, including a fact handed in with a derived source label that the run does not derive again. A requirement that conflicts with an existing concept is not added and appears in errors and proposed fixes, with no applied change. Repair never removes an author-stated concept. These are the reasons a marking can be <c>valid</c> while <c>inputWasValid</c> is false.</summary>
    public IReadOnlyList<Change> AppliedFixes { get; }

    /// <summary>The facts the marking states, as the engine settled them.</summary>
    public IReadOnlyList<Assertion> Assertions { get; }

    /// <summary>The assessment of <c>rendered</c>: what it wrote of these facts, what it left out, what collapsed into an equivalent, and what it could not write at all. When <c>rendered</c> carries more than one surface, this is the combined assessment <c>RenderResult</c> describes. Read it before treating <c>rendered</c> as a transport.</summary>
    public RenderAssessment Assessment { get; }

    /// <summary>What the scheme's rules find wrong with the settled marking. Any entry makes <c>valid</c> false.</summary>
    public IReadOnlyList<ValidationError> Errors { get; }

    /// <summary>True when the text or facts as given needed no change to stand.</summary>
    public bool InputWasValid { get; }

    /// <summary>The notices the scheme attaches to the concepts in <c>assertions</c>, collected the same way the notices operation collects them. Always present, empty when the marking carries none, so a consumer can read the field without checking that it exists.</summary>
    public IReadOnlyList<Notice> Notices { get; }

    /// <summary>The settled marking, in one string.</summary>
    public string Rendered { get; }

    /// <summary>True when the marking stands once the engine has settled it: read, repaired, completed by what the scheme's rules imply, and judged with no error left.</summary>
    public bool Valid { get; }

    /// <summary>Advisories that do not affect <c>valid</c>, such as a deprecated concept. On a process result whose fix paths were not all proven, this carries a notice whose <c>ruleId</c> is <c>_fixes_unproven</c>, as <c>ProcessResult.proposedFixes</c> describes.</summary>
    public IReadOnlyList<Warning> Warnings { get; }

    /// <summary>Portable scheme-derived display color for the settled marking. Embedding hosts may override this presentation default.</summary>
    public ConceptColor? DisplayColor { get; set; }

    /// <summary>What recognition made of the text: how far it got, what it read the text as, and what it could not account for.</summary>
    public RecognitionResult? Recognition { get; set; }
}

/// <summary>What process answers: everything validate answers, plus what an author needs to correct and finish the marking.</summary>
public sealed record ProcessResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="appliedFixes">Every change the engine actually made while settling the marking: a departure from canonical spelling or order that recognition read through, a repair, a fact a rule required, a handed-in fact settling removed, or text read in the other role from the one the request names. A removed fact is named with the reason it was removed, including a fact handed in with a derived source label that the run does not derive again. A requirement that conflicts with an existing concept is not added and appears in errors and proposed fixes, with no applied change. Repair never removes an author-stated concept. These are the reasons a marking can be <c>valid</c> while <c>inputWasValid</c> is false.</param>
    /// <param name="assertions">The facts the marking states, as the engine settled them.</param>
    /// <param name="assessment">The assessment of <c>rendered</c>: what it wrote of these facts, what it left out, what collapsed into an equivalent, and what it could not write at all. When <c>rendered</c> carries more than one surface, this is the combined assessment <c>RenderResult</c> describes. Read it before treating <c>rendered</c> as a transport.</param>
    /// <param name="errors">What the scheme's rules find wrong with the settled marking. Any entry makes <c>valid</c> false.</param>
    /// <param name="inputWasValid">True when the text or facts as given needed no change to stand.</param>
    /// <param name="notices">The notices the scheme attaches to the concepts in <c>assertions</c>, collected the same way the notices operation collects them. Always present, empty when the marking carries none, so a consumer can read the field without checking that it exists.</param>
    /// <param name="proposedFixes">The ways to make the resulting value valid, computed from that value after applied changes, each a whole marking the engine has read back and found valid before offering it. Empty when the marking is valid, or when no change the engine could prove would make it so.</param>
    /// <param name="rendered">The settled marking, in one string.</param>
    /// <param name="renders">The outputs the request named in <c>outputs</c>, one entry each, keyed by output address. Empty when the request named none.</param>
    /// <param name="valid">True when the marking stands once the engine has settled it: read, repaired, completed by what the scheme's rules imply, and judged with no error left.</param>
    /// <param name="warnings">Advisories that do not affect <c>valid</c>, such as a deprecated concept. On a process result whose fix paths were not all proven, this carries a notice whose <c>ruleId</c> is <c>_fixes_unproven</c>, as <c>ProcessResult.proposedFixes</c> describes.</param>
    public ProcessResult(IReadOnlyList<Change> appliedFixes, IReadOnlyList<Assertion> assertions, RenderAssessment assessment, IReadOnlyList<ValidationError> errors, bool inputWasValid, IReadOnlyList<Notice> notices, IReadOnlyList<Fix> proposedFixes, string rendered, IReadOnlyDictionary<string, RenderResult> renders, bool valid, IReadOnlyList<Warning> warnings)
    {
        AppliedFixes = appliedFixes ?? throw new ArgumentNullException(nameof(appliedFixes));
        Assertions = assertions ?? throw new ArgumentNullException(nameof(assertions));
        Assessment = assessment ?? throw new ArgumentNullException(nameof(assessment));
        Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        InputWasValid = inputWasValid;
        Notices = notices ?? throw new ArgumentNullException(nameof(notices));
        ProposedFixes = proposedFixes ?? throw new ArgumentNullException(nameof(proposedFixes));
        Rendered = rendered ?? throw new ArgumentNullException(nameof(rendered));
        Renders = renders ?? throw new ArgumentNullException(nameof(renders));
        Valid = valid;
        Warnings = warnings ?? throw new ArgumentNullException(nameof(warnings));
    }

    /// <summary>Every change the engine actually made while settling the marking: a departure from canonical spelling or order that recognition read through, a repair, a fact a rule required, a handed-in fact settling removed, or text read in the other role from the one the request names. A removed fact is named with the reason it was removed, including a fact handed in with a derived source label that the run does not derive again. A requirement that conflicts with an existing concept is not added and appears in errors and proposed fixes, with no applied change. Repair never removes an author-stated concept. These are the reasons a marking can be <c>valid</c> while <c>inputWasValid</c> is false.</summary>
    public IReadOnlyList<Change> AppliedFixes { get; }

    /// <summary>The facts the marking states, as the engine settled them.</summary>
    public IReadOnlyList<Assertion> Assertions { get; }

    /// <summary>The assessment of <c>rendered</c>: what it wrote of these facts, what it left out, what collapsed into an equivalent, and what it could not write at all. When <c>rendered</c> carries more than one surface, this is the combined assessment <c>RenderResult</c> describes. Read it before treating <c>rendered</c> as a transport.</summary>
    public RenderAssessment Assessment { get; }

    /// <summary>What the scheme's rules find wrong with the settled marking. Any entry makes <c>valid</c> false.</summary>
    public IReadOnlyList<ValidationError> Errors { get; }

    /// <summary>True when the text or facts as given needed no change to stand.</summary>
    public bool InputWasValid { get; }

    /// <summary>The notices the scheme attaches to the concepts in <c>assertions</c>, collected the same way the notices operation collects them. Always present, empty when the marking carries none, so a consumer can read the field without checking that it exists.</summary>
    public IReadOnlyList<Notice> Notices { get; }

    /// <summary>The ways to make the resulting value valid, computed from that value after applied changes, each a whole marking the engine has read back and found valid before offering it. Empty when the marking is valid, or when no change the engine could prove would make it so.</summary>
    public IReadOnlyList<Fix> ProposedFixes { get; }

    /// <summary>The settled marking, in one string.</summary>
    public string Rendered { get; }

    /// <summary>The outputs the request named in <c>outputs</c>, one entry each, keyed by output address. Empty when the request named none.</summary>
    public IReadOnlyDictionary<string, RenderResult> Renders { get; }

    /// <summary>True when the marking stands once the engine has settled it: read, repaired, completed by what the scheme's rules imply, and judged with no error left.</summary>
    public bool Valid { get; }

    /// <summary>Advisories that do not affect <c>valid</c>, such as a deprecated concept. On a process result whose fix paths were not all proven, this carries a notice whose <c>ruleId</c> is <c>_fixes_unproven</c>, as <c>ProcessResult.proposedFixes</c> describes.</summary>
    public IReadOnlyList<Warning> Warnings { get; }

    /// <summary>Portable scheme-derived display color for the settled marking. Embedding hosts may override this presentation default.</summary>
    public ConceptColor? DisplayColor { get; set; }

    /// <summary>What recognition made of the text, with the corrections that turn it into canonical text. Present exactly when there was text to recognize.</summary>
    public RecognitionWithCorrections? Recognition { get; set; }

    /// <summary>The options for each category the request named in <c>suggestFor</c>, answered against these settled facts exactly as <c>suggestForCategories</c> answers them, in the request's output format and surface.</summary>
    public IReadOnlyDictionary<string, SuggestResult>? SuggestionsFor { get; set; }
}

/// <summary>Output of combining multiple inputs into a single rendered marking.</summary>
public sealed record AggregateResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="appliedFixes">The changes the rollup made to reach the combined marking.</param>
    /// <param name="assertions">The assertions of the combined marking, listed in print order as a validate result's are: the order the rendered marking places each concept; a concept the rendering leaves out appears where it would print.</param>
    /// <param name="errors">The constraints the combined marking violates.</param>
    /// <param name="notices">The notices the scheme attaches to the concepts in <c>assertions</c>, collected the same way the notices operation collects them. Always present, empty when the aggregate carries none, so a consumer can read the field without checking that it exists.</param>
    /// <param name="proposedFixes">Always empty: aggregate applies every fix it can.</param>
    /// <param name="rendered">The combined marking, written in the output format.</param>
    /// <param name="valid">True when the combined marking stands once settled, as <c>ValidateResult.valid</c> defines it.</param>
    /// <param name="warnings">Advisory signals about the combined marking.</param>
    public AggregateResult(IReadOnlyList<Change> appliedFixes, IReadOnlyList<Assertion> assertions, IReadOnlyList<ValidationError> errors, IReadOnlyList<Notice> notices, IReadOnlyList<Fix> proposedFixes, string rendered, bool valid, IReadOnlyList<Warning> warnings)
    {
        AppliedFixes = appliedFixes ?? throw new ArgumentNullException(nameof(appliedFixes));
        Assertions = assertions ?? throw new ArgumentNullException(nameof(assertions));
        Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        Notices = notices ?? throw new ArgumentNullException(nameof(notices));
        ProposedFixes = proposedFixes ?? throw new ArgumentNullException(nameof(proposedFixes));
        Rendered = rendered ?? throw new ArgumentNullException(nameof(rendered));
        Valid = valid;
        Warnings = warnings ?? throw new ArgumentNullException(nameof(warnings));
    }

    /// <summary>The changes the rollup made to reach the combined marking.</summary>
    public IReadOnlyList<Change> AppliedFixes { get; }

    /// <summary>The assertions of the combined marking, listed in print order as a validate result's are: the order the rendered marking places each concept; a concept the rendering leaves out appears where it would print.</summary>
    public IReadOnlyList<Assertion> Assertions { get; }

    /// <summary>The constraints the combined marking violates.</summary>
    public IReadOnlyList<ValidationError> Errors { get; }

    /// <summary>The notices the scheme attaches to the concepts in <c>assertions</c>, collected the same way the notices operation collects them. Always present, empty when the aggregate carries none, so a consumer can read the field without checking that it exists.</summary>
    public IReadOnlyList<Notice> Notices { get; }

    /// <summary>Always empty: aggregate applies every fix it can.</summary>
    public IReadOnlyList<Fix> ProposedFixes { get; }

    /// <summary>The combined marking, written in the output format.</summary>
    public string Rendered { get; }

    /// <summary>True when the combined marking stands once settled, as <c>ValidateResult.valid</c> defines it.</summary>
    public bool Valid { get; }

    /// <summary>Advisory signals about the combined marking.</summary>
    public IReadOnlyList<Warning> Warnings { get; }

    /// <summary>Portable scheme-derived display color for the final post-rollup marking. Embedding hosts may override this presentation default.</summary>
    public ConceptColor? DisplayColor { get; set; }
}

/// <summary>Comparison of two assertion sets: added, removed, and common assertions.</summary>
public sealed record DiffResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="added">Assertions in the second marking and not the first.</param>
    /// <param name="common">Assertions in both markings, as the first marking holds them. Concept and category decide sameness; confidence, source and value are not compared.</param>
    /// <param name="removed">Assertions in the first marking and not the second.</param>
    public DiffResult(IReadOnlyList<Assertion> added, IReadOnlyList<Assertion> common, IReadOnlyList<Assertion> removed)
    {
        Added = added ?? throw new ArgumentNullException(nameof(added));
        Common = common ?? throw new ArgumentNullException(nameof(common));
        Removed = removed ?? throw new ArgumentNullException(nameof(removed));
    }

    /// <summary>Assertions in the second marking and not the first.</summary>
    public IReadOnlyList<Assertion> Added { get; }

    /// <summary>Assertions in both markings, as the first marking holds them. Concept and category decide sameness; confidence, source and value are not compared.</summary>
    public IReadOnlyList<Assertion> Common { get; }

    /// <summary>Assertions in the first marking and not the second.</summary>
    public IReadOnlyList<Assertion> Removed { get; }
}

/// <summary>A single marking found during a free-text scan of the source text, with what validate answers for it.</summary>
public sealed record ScanMatch
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="end">End position in the (possibly stripped) input text</param>
    /// <param name="marking">The matched marking string (as found in the document)</param>
    /// <param name="result">What validate answers for this marking, read where it was found and in the role <c>role</c> names. A scan reads documents; it does not author them, so it carries no fix paths. A caller that wants to correct one region processes that region.</param>
    /// <param name="role">Scheme-resolved structural role of this marking: <c>'whole'</c> (a document-wide marking: a page-top marking, a TLP document tag) or <c>'part'</c> (a per-region marking: a paragraph mark, a segment, a tag).</param>
    /// <param name="start">Start position in the (possibly stripped) input text</param>
    /// <param name="type">The output this marking is written in when the scan names no output format: <c>result.rendered</c> is written in it.</param>
    /// <param name="wrapped">Whether the marking was found inside a wrapper pattern (e.g., parentheses)</param>
    public ScanMatch(int end, string marking, ValidateResult result, StructuralRole role, int start, string type, bool wrapped)
    {
        End = end;
        Marking = marking ?? throw new ArgumentNullException(nameof(marking));
        Result = result ?? throw new ArgumentNullException(nameof(result));
        Role = role;
        Start = start;
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Wrapped = wrapped;
    }

    /// <summary>End position in the (possibly stripped) input text</summary>
    public int End { get; }

    /// <summary>The matched marking string (as found in the document)</summary>
    public string Marking { get; }

    /// <summary>What validate answers for this marking, read where it was found and in the role <c>role</c> names. A scan reads documents; it does not author them, so it carries no fix paths. A caller that wants to correct one region processes that region.</summary>
    public ValidateResult Result { get; }

    /// <summary>Scheme-resolved structural role of this marking: <c>'whole'</c> (a document-wide marking: a page-top marking, a TLP document tag) or <c>'part'</c> (a per-region marking: a paragraph mark, a segment, a tag).</summary>
    public StructuralRole Role { get; }

    /// <summary>Start position in the (possibly stripped) input text</summary>
    public int Start { get; }

    /// <summary>The output this marking is written in when the scan names no output format: <c>result.rendered</c> is written in it.</summary>
    public string Type { get; }

    /// <summary>Whether the marking was found inside a wrapper pattern (e.g., parentheses)</summary>
    public bool Wrapped { get; }

    /// <summary>The marking text inside the wrapper: the capture group of the scheme's <c>embeddedMarking</c> pattern. Present only when <c>wrapped</c> is true. Consumers that need to re-process the marking must use this rather than stripping delimiters from <c>marking</c>, because the wrapper shape is scheme-declared and is not always a pair of parentheses.</summary>
    public string? Inner { get; set; }
}

/// <summary>How far a scan got, and what stopped it when something did.</summary>
public sealed record ScanLimits
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="regionsFound">Candidate regions the scan considered.</param>
    /// <param name="status">Whether the document was read to the end, and if not, why not.</param>
    public ScanLimits(int regionsFound, ScanStatus status)
    {
        RegionsFound = regionsFound;
        Status = status;
    }

    /// <summary>Candidate regions the scan considered.</summary>
    public int RegionsFound { get; }

    /// <summary>Whether the document was read to the end, and if not, why not.</summary>
    public ScanStatus Status { get; }

    /// <summary>The ceiling that stopped the scan, when one did.</summary>
    public RecognitionLimit? LimitHit { get; set; }

    /// <summary>Whether the per-document region cap was reached.</summary>
    public bool? RegionsCapped { get; set; }
}

/// <summary>A wrapped mark a scan found but could not read.</summary>
public sealed record UnreadMark
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="end">End position in the input text</param>
    /// <param name="inner">The text inside the wrapper: the capture group of the scheme's <c>embeddedMarking</c> pattern.</param>
    /// <param name="leading">Whether the mark stands where a part mark goes: at the start of its paragraph. Both of these hold for a leading mark.</param>
    /// <param name="start">Start position in the input text</param>
    /// <param name="text">The wrapped text as found in the document, wrapper included.</param>
    public UnreadMark(int end, string inner, bool leading, int start, string text)
    {
        End = end;
        Inner = inner ?? throw new ArgumentNullException(nameof(inner));
        Leading = leading;
        Start = start;
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }

    /// <summary>End position in the input text</summary>
    public int End { get; }

    /// <summary>The text inside the wrapper: the capture group of the scheme's <c>embeddedMarking</c> pattern.</summary>
    public string Inner { get; }

    /// <summary>Whether the mark stands where a part mark goes: at the start of its paragraph. Both of these hold for a leading mark.</summary>
    public bool Leading { get; }

    /// <summary>Start position in the input text</summary>
    public int Start { get; }

    /// <summary>The wrapped text as found in the document, wrapper included.</summary>
    public string Text { get; }
}

/// <summary>A byte-offset span where a guide rule keyword or pattern matched in scanned text.</summary>
public sealed record GuideScanSpan
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="end">Offset in the scanned text just past the end of the match, in UTF-16 code units.</param>
    /// <param name="keyword">The keyword that matched, or the label of the matcher that did.</param>
    /// <param name="start">Offset in the scanned text where the match starts, in UTF-16 code units.</param>
    public GuideScanSpan(int end, string keyword, int start)
    {
        End = end;
        Keyword = keyword ?? throw new ArgumentNullException(nameof(keyword));
        Start = start;
    }

    /// <summary>Offset in the scanned text just past the end of the match, in UTF-16 code units.</summary>
    public int End { get; }

    /// <summary>The keyword that matched, or the label of the matcher that did.</summary>
    public string Keyword { get; }

    /// <summary>Offset in the scanned text where the match starts, in UTF-16 code units.</summary>
    public int Start { get; }
}

/// <summary>A rule in a marking guide, paired with its scan metadata.</summary>
public sealed record GuideRule
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="id">Unique identifier within the guide.</param>
    /// <param name="name">Human-readable display name.</param>
    /// <param name="outcome">What this rule contributes when active. At least one of rendered or concepts must be set.</param>
    public GuideRule(string id, string name, JsonElement outcome)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Outcome = outcome;
    }

    /// <summary>Unique identifier within the guide.</summary>
    public string Id { get; }

    /// <summary>Human-readable display name.</summary>
    public string Name { get; }

    /// <summary>What this rule contributes when active. At least one of rendered or concepts must be set.</summary>
    public JsonElement Outcome { get; }

    /// <summary>Guide category ID for grouping in the UI.</summary>
    public string? Category { get; set; }

    /// <summary>Description of the data element this rule covers.</summary>
    public string? Description { get; set; }

    /// <summary>Searchable terms for literal keyword filtering (absent on computed rules).</summary>
    public IReadOnlyList<string>? Keywords { get; set; }

    /// <summary>Named pattern matchers for content evaluation. Each entry is a <c>MatchNode</c>: a leaf spec referencing a built-in matcher (e.g. <c>{ type: 'ipv4' }</c>) or a custom regex (<c>{ type: 'regex', pattern: '...' }</c>), or a combinator node (<c>any</c> / <c>all</c> / <c>not</c> / <c>near</c> / <c>context</c> / <c>validate</c>) composed over child nodes. When content is evaluated, both <c>keywords</c> and <c>match</c> are checked.</summary>
    public IReadOnlyList<JsonElement>? Match { get; set; }

    /// <summary>Rule IDs whose outcomes this rule replaces when active. When omitted (default), this rule's outcome is additive: it contributes alongside the rules it depends on.</summary>
    public IReadOnlyList<string>? Overrides { get; set; }

    /// <summary>Explanation of why this outcome applies.</summary>
    public string? Rationale { get; set; }

    /// <summary>When present, the rule is computed: it activates when this block holds over the rules the user selected, and it is not offered as a checkbox.</summary>
    public JsonElement? When { get; set; }
}

/// <summary>A guide rule matched by text scanning, with keyword spans.</summary>
public sealed record GuideScanResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="guideId">The id of the guide the rule belongs to. Rule ids are unique only within a guide, so a selection built from this hit names this guide.</param>
    /// <param name="matchedKeywords">Each keyword or matcher label that fired, once each.</param>
    /// <param name="rule">The guide rule that matched.</param>
    /// <param name="spans">Every place in the text where the rule fired, in the order found.</param>
    public GuideScanResult(string guideId, IReadOnlyList<string> matchedKeywords, GuideRule rule, IReadOnlyList<GuideScanSpan> spans)
    {
        GuideId = guideId ?? throw new ArgumentNullException(nameof(guideId));
        MatchedKeywords = matchedKeywords ?? throw new ArgumentNullException(nameof(matchedKeywords));
        Rule = rule ?? throw new ArgumentNullException(nameof(rule));
        Spans = spans ?? throw new ArgumentNullException(nameof(spans));
    }

    /// <summary>The id of the guide the rule belongs to. Rule ids are unique only within a guide, so a selection built from this hit names this guide.</summary>
    public string GuideId { get; }

    /// <summary>Each keyword or matcher label that fired, once each.</summary>
    public IReadOnlyList<string> MatchedKeywords { get; }

    /// <summary>The guide rule that matched.</summary>
    public GuideRule Rule { get; }

    /// <summary>Every place in the text where the rule fired, in the order found.</summary>
    public IReadOnlyList<GuideScanSpan> Spans { get; }
}

/// <summary>Output of evaluating selected guide rules into a single aggregate marking.</summary>
public sealed record EvaluationResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="activeComputedRules">The computed rules whose <c>when</c> condition the selection satisfied.</param>
    /// <param name="aggregatedMarking">The effective markings rolled up into one marking by the target scheme. Empty when no rule contributed, or when the result spans more than one target scheme, in which case <c>aggregatedMarkings</c> holds one per scheme.</param>
    /// <param name="breakdown">One entry per evaluated rule: for each guide, its selected rules, then its computed rules.</param>
    /// <param name="contributingBreakdown">Subset of <c>breakdown</c> that actually contributed to <c>aggregatedMarking</c> (i.e. <c>breakdown.filter(b =&gt; !b.wasOverridden)</c>). Precomputed so UI layers stop re-filtering the full breakdown.</param>
    /// <param name="effectiveMarkings">The marking each contributing rule's outcome resolves to. Overridden rules contribute none.</param>
    /// <param name="selectedRules">The selected rules that were evaluated. Rules with a <c>when</c> condition are computed, not selected, and are not listed here.</param>
    public EvaluationResult(IReadOnlyList<GuideRule> activeComputedRules, string aggregatedMarking, IReadOnlyList<GuideBreakdownEntry> breakdown, IReadOnlyList<GuideBreakdownEntry> contributingBreakdown, IReadOnlyList<string> effectiveMarkings, IReadOnlyList<GuideRule> selectedRules)
    {
        ActiveComputedRules = activeComputedRules ?? throw new ArgumentNullException(nameof(activeComputedRules));
        AggregatedMarking = aggregatedMarking ?? throw new ArgumentNullException(nameof(aggregatedMarking));
        Breakdown = breakdown ?? throw new ArgumentNullException(nameof(breakdown));
        ContributingBreakdown = contributingBreakdown ?? throw new ArgumentNullException(nameof(contributingBreakdown));
        EffectiveMarkings = effectiveMarkings ?? throw new ArgumentNullException(nameof(effectiveMarkings));
        SelectedRules = selectedRules ?? throw new ArgumentNullException(nameof(selectedRules));
    }

    /// <summary>The computed rules whose <c>when</c> condition the selection satisfied.</summary>
    public IReadOnlyList<GuideRule> ActiveComputedRules { get; }

    /// <summary>The effective markings rolled up into one marking by the target scheme. Empty when no rule contributed, or when the result spans more than one target scheme, in which case <c>aggregatedMarkings</c> holds one per scheme.</summary>
    public string AggregatedMarking { get; }

    /// <summary>One entry per evaluated rule: for each guide, its selected rules, then its computed rules.</summary>
    public IReadOnlyList<GuideBreakdownEntry> Breakdown { get; }

    /// <summary>Subset of <c>breakdown</c> that actually contributed to <c>aggregatedMarking</c> (i.e. <c>breakdown.filter(b =&gt; !b.wasOverridden)</c>). Precomputed so UI layers stop re-filtering the full breakdown.</summary>
    public IReadOnlyList<GuideBreakdownEntry> ContributingBreakdown { get; }

    /// <summary>The marking each contributing rule's outcome resolves to. Overridden rules contribute none.</summary>
    public IReadOnlyList<string> EffectiveMarkings { get; }

    /// <summary>The selected rules that were evaluated. Rules with a <c>when</c> condition are computed, not selected, and are not listed here.</summary>
    public IReadOnlyList<GuideRule> SelectedRules { get; }

    /// <summary>Aggregated output keyed by target scheme id. Present when guide evaluation routed through more than one target scheme.</summary>
    public IReadOnlyDictionary<string, string>? AggregatedMarkings { get; set; }

    /// <summary>The id of the guide evaluated. Absent when the result combines several guides.</summary>
    public string? GuideId { get; set; }
}

/// <summary>One deprecated-to-current concept substitution that processing applied before an access rule was evaluated.</summary>
public sealed record ConceptModernization
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="conceptDisplay">Resolved operational display for <c>conceptId</c>.</param>
    /// <param name="conceptId">Current concept the access rule was evaluated against.</param>
    /// <param name="replacedConceptDisplay">Resolved operational display for <c>replacedConceptId</c>.</param>
    /// <param name="replacedConceptId">Deprecated concept the data was actually marked with.</param>
    public ConceptModernization(string conceptDisplay, string conceptId, string replacedConceptDisplay, string replacedConceptId)
    {
        ConceptDisplay = conceptDisplay ?? throw new ArgumentNullException(nameof(conceptDisplay));
        ConceptId = conceptId ?? throw new ArgumentNullException(nameof(conceptId));
        ReplacedConceptDisplay = replacedConceptDisplay ?? throw new ArgumentNullException(nameof(replacedConceptDisplay));
        ReplacedConceptId = replacedConceptId ?? throw new ArgumentNullException(nameof(replacedConceptId));
    }

    /// <summary>Resolved operational display for <c>conceptId</c>.</summary>
    public string ConceptDisplay { get; }

    /// <summary>Current concept the access rule was evaluated against.</summary>
    public string ConceptId { get; }

    /// <summary>Resolved operational display for <c>replacedConceptId</c>.</summary>
    public string ReplacedConceptDisplay { get; }

    /// <summary>Deprecated concept the data was actually marked with.</summary>
    public string ReplacedConceptId { get; }
}

/// <summary>Result of evaluating a single access rule entry.</summary>
public sealed record AccessRuleResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="activation">Machine-readable activation basis.</param>
    /// <param name="comparisons">Machine-readable comparisons evaluated by the rule.</param>
    /// <param name="granted">Whether this rule passed.</param>
    /// <param name="name">Rule name from the access policy (for display).</param>
    /// <param name="rule">Qualified rule identity and semantic digest.</param>
    /// <param name="ruleIndex">Zero-based index of the rule in the access policy's evaluated rules.</param>
    /// <param name="skipped">Whether the rule was skipped because its activation did not match.</param>
    public AccessRuleResult(AccessRuleActivation activation, IReadOnlyList<AccessComparisonResult> comparisons, bool granted, string name, RuleReference rule, int ruleIndex, bool skipped)
    {
        Activation = activation ?? throw new ArgumentNullException(nameof(activation));
        Comparisons = comparisons ?? throw new ArgumentNullException(nameof(comparisons));
        Granted = granted;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Rule = rule ?? throw new ArgumentNullException(nameof(rule));
        RuleIndex = ruleIndex;
        Skipped = skipped;
    }

    /// <summary>Machine-readable activation basis.</summary>
    public AccessRuleActivation Activation { get; }

    /// <summary>Machine-readable comparisons evaluated by the rule.</summary>
    public IReadOnlyList<AccessComparisonResult> Comparisons { get; }

    /// <summary>Whether this rule passed.</summary>
    public bool Granted { get; }

    /// <summary>Rule name from the access policy (for display).</summary>
    public string Name { get; }

    /// <summary>Qualified rule identity and semantic digest.</summary>
    public RuleReference Rule { get; }

    /// <summary>Zero-based index of the rule in the access policy's evaluated rules.</summary>
    public int RuleIndex { get; }

    /// <summary>Whether the rule was skipped because its activation did not match.</summary>
    public bool Skipped { get; }

    /// <summary>Deprecated-to-current concept substitutions that processing applied to the data before this rule was evaluated.</summary>
    public IReadOnlyList<ConceptModernization>? ModernizedFrom { get; set; }

    /// <summary>Denial reason from the rule definition or generated explanation.</summary>
    public string? Reason { get; set; }
}

/// <summary>Overall access decision with per-rule breakdown for a labeled artifact.</summary>
public sealed record AccessDecision
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="granted">True when every rule that was not skipped granted access.</param>
    /// <param name="rules">One result per rule in the policy, in policy order.</param>
    /// <param name="warnings">Warnings from preparing and evaluating the target, including automatic changes. Present on granted and denied decisions; empty when none apply.</param>
    public AccessDecision(bool granted, IReadOnlyList<AccessRuleResult> rules, IReadOnlyList<Warning> warnings)
    {
        Granted = granted;
        Rules = rules ?? throw new ArgumentNullException(nameof(rules));
        Warnings = warnings ?? throw new ArgumentNullException(nameof(warnings));
    }

    /// <summary>True when every rule that was not skipped granted access.</summary>
    public bool Granted { get; }

    /// <summary>One result per rule in the policy, in policy order.</summary>
    public IReadOnlyList<AccessRuleResult> Rules { get; }

    /// <summary>Warnings from preparing and evaluating the target, including automatic changes. Present on granted and denied decisions; empty when none apply.</summary>
    public IReadOnlyList<Warning> Warnings { get; }
}
