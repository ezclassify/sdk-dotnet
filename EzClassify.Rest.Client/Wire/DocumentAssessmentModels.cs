#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EzClassify;

/// <summary>Immutable artifact family.</summary>
[JsonConverter(typeof(WireTokenJsonConverter<ArtifactKind>))]
public readonly struct ArtifactKind : IEquatable<ArtifactKind>, IWireToken<ArtifactKind>
{
    private const string SchemeValue = "scheme";
    private const string LayerValue = "layer";
    private const string GuideValue = "guide";
    private const string AccessValue = "access";
    private const string ContextValue = "context";
    private const string CrosswalkValue = "crosswalk";

    private readonly string? _value;

    /// <summary>Wraps a wire token, whether or not this SDK declares it.</summary>
    /// <param name="value">The token exactly as it travels on the wire.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public ArtifactKind(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary>Scheme artifact.</summary>
    public static ArtifactKind Scheme { get; } = new(SchemeValue);

    /// <summary>Layer artifact.</summary>
    public static ArtifactKind Layer { get; } = new(LayerValue);

    /// <summary>Guide artifact.</summary>
    public static ArtifactKind Guide { get; } = new(GuideValue);

    /// <summary>Access-policy artifact.</summary>
    public static ArtifactKind Access { get; } = new(AccessValue);

    /// <summary>Context artifact.</summary>
    public static ArtifactKind Context { get; } = new(ContextValue);

    /// <summary>Crosswalk artifact.</summary>
    public static ArtifactKind Crosswalk { get; } = new(CrosswalkValue);

    /// <summary>Whether this version of the SDK declares the token. False for a value a newer server sent.</summary>
    public bool IsKnown => _value is SchemeValue or LayerValue or GuideValue or AccessValue or ContextValue or CrosswalkValue;

    /// <summary>Decode a token a caller supplies; only a declared artifact kind is accepted.</summary>
    /// <param name="value">The wire token.</param>
    /// <returns>The declared value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="value"/> is not a artifact kind this SDK declares.</exception>
    public static ArtifactKind Parse(string value) => WireToken.Parse<ArtifactKind>(value, "artifact kind");

    /// <summary>Decode a token a caller supplies without throwing; only a declared artifact kind is accepted.</summary>
    /// <param name="value">The wire token.</param>
    /// <param name="result">The declared value, or <c>default</c> when the token is not declared.</param>
    /// <returns>True when <paramref name="value"/> is a declared token.</returns>
    public static bool TryParse(string? value, out ArtifactKind result) => WireToken.TryParse(value, out result);

    /// <summary>The wire token, exactly as it was received or constructed.</summary>
    /// <exception cref="InvalidOperationException">The value is <c>default(ArtifactKind)</c>, which carries no token.</exception>
    public string ToWire() => WireToken.Require(_value, nameof(ArtifactKind));

    /// <summary>The wire token; empty for <c>default(ArtifactKind)</c>.</summary>
    public override string ToString() => _value ?? string.Empty;

    /// <summary>Ordinal comparison of the wire tokens.</summary>
    public bool Equals(ArtifactKind other) => string.Equals(_value, other._value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is ArtifactKind other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => WireToken.Hash(_value);

    /// <summary>Ordinal comparison of the wire tokens.</summary>
    public static bool operator ==(ArtifactKind left, ArtifactKind right) => left.Equals(right);

    /// <summary>Ordinal comparison of the wire tokens.</summary>
    public static bool operator !=(ArtifactKind left, ArtifactKind right) => !left.Equals(right);

    string? IWireToken<ArtifactKind>.Token => _value;

    ArtifactKind IWireToken<ArtifactKind>.FromToken(string value) => new(value);
}

/// <summary>Document response detail profile.</summary>
[JsonConverter(typeof(WireTokenJsonConverter<DocumentResponseProfile>))]
public readonly struct DocumentResponseProfile : IEquatable<DocumentResponseProfile>, IWireToken<DocumentResponseProfile>
{
    private const string SummaryValue = "summary";
    private const string StandardValue = "standard";
    private const string FullValue = "full";

    private readonly string? _value;

    /// <summary>Wraps a wire token, whether or not this SDK declares it.</summary>
    /// <param name="value">The token exactly as it travels on the wire.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public DocumentResponseProfile(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary>Counts and semantic statuses only.</summary>
    public static DocumentResponseProfile Summary { get; } = new(SummaryValue);

    /// <summary>Default decision-ready detail.</summary>
    public static DocumentResponseProfile Standard { get; } = new(StandardValue);

    /// <summary>Complete evidence and breakdown detail.</summary>
    public static DocumentResponseProfile Full { get; } = new(FullValue);

    /// <summary>Whether this version of the SDK declares the token. False for a value a newer server sent.</summary>
    public bool IsKnown => _value is SummaryValue or StandardValue or FullValue;

    /// <summary>Decode a token a caller supplies; only a declared document response profile is accepted.</summary>
    /// <param name="value">The wire token.</param>
    /// <returns>The declared value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="value"/> is not a document response profile this SDK declares.</exception>
    public static DocumentResponseProfile Parse(string value) => WireToken.Parse<DocumentResponseProfile>(value, "document response profile");

    /// <summary>Decode a token a caller supplies without throwing; only a declared document response profile is accepted.</summary>
    /// <param name="value">The wire token.</param>
    /// <param name="result">The declared value, or <c>default</c> when the token is not declared.</param>
    /// <returns>True when <paramref name="value"/> is a declared token.</returns>
    public static bool TryParse(string? value, out DocumentResponseProfile result) => WireToken.TryParse(value, out result);

    /// <summary>The wire token, exactly as it was received or constructed.</summary>
    /// <exception cref="InvalidOperationException">The value is <c>default(DocumentResponseProfile)</c>, which carries no token.</exception>
    public string ToWire() => WireToken.Require(_value, nameof(DocumentResponseProfile));

    /// <summary>The wire token; empty for <c>default(DocumentResponseProfile)</c>.</summary>
    public override string ToString() => _value ?? string.Empty;

    /// <summary>Ordinal comparison of the wire tokens.</summary>
    public bool Equals(DocumentResponseProfile other) => string.Equals(_value, other._value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is DocumentResponseProfile other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => WireToken.Hash(_value);

    /// <summary>Ordinal comparison of the wire tokens.</summary>
    public static bool operator ==(DocumentResponseProfile left, DocumentResponseProfile right) => left.Equals(right);

    /// <summary>Ordinal comparison of the wire tokens.</summary>
    public static bool operator !=(DocumentResponseProfile left, DocumentResponseProfile right) => !left.Equals(right);

    string? IWireToken<DocumentResponseProfile>.Token => _value;

    DocumentResponseProfile IWireToken<DocumentResponseProfile>.FromToken(string value) => new(value);
}

/// <summary>Source domain for an access-comparison operand.</summary>
[JsonConverter(typeof(WireTokenJsonConverter<AccessOperandSource>))]
public readonly struct AccessOperandSource : IEquatable<AccessOperandSource>, IWireToken<AccessOperandSource>
{
    private const string SubjectValue = "subject";
    private const string ContextValue = "context";
    private const string AssertionValue = "assertion";
    private const string LiteralValue = "literal";

    private readonly string? _value;

    /// <summary>Wraps a wire token, whether or not this SDK declares it.</summary>
    /// <param name="value">The token exactly as it travels on the wire.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public AccessOperandSource(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary>Immutable subject data.</summary>
    public static AccessOperandSource Subject { get; } = new(SubjectValue);

    /// <summary>Relationship context data.</summary>
    public static AccessOperandSource Context { get; } = new(ContextValue);

    /// <summary>Document assertion data.</summary>
    public static AccessOperandSource Assertion { get; } = new(AssertionValue);

    /// <summary>Policy-authored literal value.</summary>
    public static AccessOperandSource Literal { get; } = new(LiteralValue);

    /// <summary>Whether this version of the SDK declares the token. False for a value a newer server sent.</summary>
    public bool IsKnown => _value is SubjectValue or ContextValue or AssertionValue or LiteralValue;

    /// <summary>Decode a token a caller supplies; only a declared access operand source is accepted.</summary>
    /// <param name="value">The wire token.</param>
    /// <returns>The declared value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="value"/> is not a access operand source this SDK declares.</exception>
    public static AccessOperandSource Parse(string value) => WireToken.Parse<AccessOperandSource>(value, "access operand source");

    /// <summary>Decode a token a caller supplies without throwing; only a declared access operand source is accepted.</summary>
    /// <param name="value">The wire token.</param>
    /// <param name="result">The declared value, or <c>default</c> when the token is not declared.</param>
    /// <returns>True when <paramref name="value"/> is a declared token.</returns>
    public static bool TryParse(string? value, out AccessOperandSource result) => WireToken.TryParse(value, out result);

    /// <summary>The wire token, exactly as it was received or constructed.</summary>
    /// <exception cref="InvalidOperationException">The value is <c>default(AccessOperandSource)</c>, which carries no token.</exception>
    public string ToWire() => WireToken.Require(_value, nameof(AccessOperandSource));

    /// <summary>The wire token; empty for <c>default(AccessOperandSource)</c>.</summary>
    public override string ToString() => _value ?? string.Empty;

    /// <summary>Ordinal comparison of the wire tokens.</summary>
    public bool Equals(AccessOperandSource other) => string.Equals(_value, other._value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is AccessOperandSource other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => WireToken.Hash(_value);

    /// <summary>Ordinal comparison of the wire tokens.</summary>
    public static bool operator ==(AccessOperandSource left, AccessOperandSource right) => left.Equals(right);

    /// <summary>Ordinal comparison of the wire tokens.</summary>
    public static bool operator !=(AccessOperandSource left, AccessOperandSource right) => !left.Equals(right);

    string? IWireToken<AccessOperandSource>.Token => _value;

    AccessOperandSource IWireToken<AccessOperandSource>.FromToken(string value) => new(value);
}

/// <summary>Guide-compilation status.</summary>
[JsonConverter(typeof(WireTokenJsonConverter<GuideCompilationStatus>))]
public readonly struct GuideCompilationStatus : IEquatable<GuideCompilationStatus>, IWireToken<GuideCompilationStatus>
{
    private const string NotEvaluatedValue = "not-evaluated";
    private const string NoFindingsValue = "no-findings";
    private const string PendingDeterminationValue = "pending-determination";
    private const string DeterminedSufficientValue = "determined-sufficient";
    private const string DeterminedInsufficientValue = "determined-insufficient";

    private readonly string? _value;

    /// <summary>Wraps a wire token, whether or not this SDK declares it.</summary>
    /// <param name="value">The token exactly as it travels on the wire.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public GuideCompilationStatus(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary>Guide compilation did not run.</summary>
    public static GuideCompilationStatus NotEvaluated { get; } = new(NotEvaluatedValue);

    /// <summary>No guide findings were produced.</summary>
    public static GuideCompilationStatus NoFindings { get; } = new(NoFindingsValue);

    /// <summary>One or more nominations await determination.</summary>
    public static GuideCompilationStatus PendingDetermination { get; } = new(PendingDeterminationValue);

    /// <summary>Determined guide result is sufficient.</summary>
    public static GuideCompilationStatus DeterminedSufficient { get; } = new(DeterminedSufficientValue);

    /// <summary>Determined guide result is insufficient.</summary>
    public static GuideCompilationStatus DeterminedInsufficient { get; } = new(DeterminedInsufficientValue);

    /// <summary>Whether this version of the SDK declares the token. False for a value a newer server sent.</summary>
    public bool IsKnown => _value is NotEvaluatedValue or NoFindingsValue or PendingDeterminationValue or DeterminedSufficientValue or DeterminedInsufficientValue;

    /// <summary>Decode a token a caller supplies; only a declared guide compilation status is accepted.</summary>
    /// <param name="value">The wire token.</param>
    /// <returns>The declared value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="value"/> is not a guide compilation status this SDK declares.</exception>
    public static GuideCompilationStatus Parse(string value) => WireToken.Parse<GuideCompilationStatus>(value, "guide compilation status");

    /// <summary>Decode a token a caller supplies without throwing; only a declared guide compilation status is accepted.</summary>
    /// <param name="value">The wire token.</param>
    /// <param name="result">The declared value, or <c>default</c> when the token is not declared.</param>
    /// <returns>True when <paramref name="value"/> is a declared token.</returns>
    public static bool TryParse(string? value, out GuideCompilationStatus result) => WireToken.TryParse(value, out result);

    /// <summary>The wire token, exactly as it was received or constructed.</summary>
    /// <exception cref="InvalidOperationException">The value is <c>default(GuideCompilationStatus)</c>, which carries no token.</exception>
    public string ToWire() => WireToken.Require(_value, nameof(GuideCompilationStatus));

    /// <summary>The wire token; empty for <c>default(GuideCompilationStatus)</c>.</summary>
    public override string ToString() => _value ?? string.Empty;

    /// <summary>Ordinal comparison of the wire tokens.</summary>
    public bool Equals(GuideCompilationStatus other) => string.Equals(_value, other._value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is GuideCompilationStatus other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => WireToken.Hash(_value);

    /// <summary>Ordinal comparison of the wire tokens.</summary>
    public static bool operator ==(GuideCompilationStatus left, GuideCompilationStatus right) => left.Equals(right);

    /// <summary>Ordinal comparison of the wire tokens.</summary>
    public static bool operator !=(GuideCompilationStatus left, GuideCompilationStatus right) => !left.Equals(right);

    string? IWireToken<GuideCompilationStatus>.Token => _value;

    GuideCompilationStatus IWireToken<GuideCompilationStatus>.FromToken(string value) => new(value);
}

/// <summary>How a proposed analyst decision entered the workflow.</summary>
[JsonConverter(typeof(WireTokenJsonConverter<DecisionProposalOrigin>))]
public readonly struct DecisionProposalOrigin : IEquatable<DecisionProposalOrigin>, IWireToken<DecisionProposalOrigin>
{
    private const string AnalystAuthoredValue = "analyst-authored";
    private const string AgentDraftedValue = "agent-drafted";
    private const string HostImportedValue = "host-imported";

    private readonly string? _value;

    /// <summary>Wraps a wire token, whether or not this SDK declares it.</summary>
    /// <param name="value">The token exactly as it travels on the wire.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public DecisionProposalOrigin(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary>Authored directly by an analyst.</summary>
    public static DecisionProposalOrigin AnalystAuthored { get; } = new(AnalystAuthoredValue);

    /// <summary>Drafted by an agent for review.</summary>
    public static DecisionProposalOrigin AgentDrafted { get; } = new(AgentDraftedValue);

    /// <summary>Imported by the host application.</summary>
    public static DecisionProposalOrigin HostImported { get; } = new(HostImportedValue);

    /// <summary>Whether this version of the SDK declares the token. False for a value a newer server sent.</summary>
    public bool IsKnown => _value is AnalystAuthoredValue or AgentDraftedValue or HostImportedValue;

    /// <summary>Decode a token a caller supplies; only a declared decision proposal origin is accepted.</summary>
    /// <param name="value">The wire token.</param>
    /// <returns>The declared value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="value"/> is not a decision proposal origin this SDK declares.</exception>
    public static DecisionProposalOrigin Parse(string value) => WireToken.Parse<DecisionProposalOrigin>(value, "decision proposal origin");

    /// <summary>Decode a token a caller supplies without throwing; only a declared decision proposal origin is accepted.</summary>
    /// <param name="value">The wire token.</param>
    /// <param name="result">The declared value, or <c>default</c> when the token is not declared.</param>
    /// <returns>True when <paramref name="value"/> is a declared token.</returns>
    public static bool TryParse(string? value, out DecisionProposalOrigin result) => WireToken.TryParse(value, out result);

    /// <summary>The wire token, exactly as it was received or constructed.</summary>
    /// <exception cref="InvalidOperationException">The value is <c>default(DecisionProposalOrigin)</c>, which carries no token.</exception>
    public string ToWire() => WireToken.Require(_value, nameof(DecisionProposalOrigin));

    /// <summary>The wire token; empty for <c>default(DecisionProposalOrigin)</c>.</summary>
    public override string ToString() => _value ?? string.Empty;

    /// <summary>Ordinal comparison of the wire tokens.</summary>
    public bool Equals(DecisionProposalOrigin other) => string.Equals(_value, other._value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is DecisionProposalOrigin other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => WireToken.Hash(_value);

    /// <summary>Ordinal comparison of the wire tokens.</summary>
    public static bool operator ==(DecisionProposalOrigin left, DecisionProposalOrigin right) => left.Equals(right);

    /// <summary>Ordinal comparison of the wire tokens.</summary>
    public static bool operator !=(DecisionProposalOrigin left, DecisionProposalOrigin right) => !left.Equals(right);

    string? IWireToken<DecisionProposalOrigin>.Token => _value;

    DecisionProposalOrigin IWireToken<DecisionProposalOrigin>.FromToken(string value) => new(value);
}

/// <summary>Analyst decision for a lexical nomination.</summary>
[JsonConverter(typeof(WireTokenJsonConverter<GuideFindingDecision>))]
public readonly struct GuideFindingDecision : IEquatable<GuideFindingDecision>, IWireToken<GuideFindingDecision>
{
    private const string AcceptedValue = "accepted";
    private const string RejectedValue = "rejected";

    private readonly string? _value;

    /// <summary>Wraps a wire token, whether or not this SDK declares it.</summary>
    /// <param name="value">The token exactly as it travels on the wire.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public GuideFindingDecision(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary>Accept the nomination as an authoritative fact.</summary>
    public static GuideFindingDecision Accepted { get; } = new(AcceptedValue);

    /// <summary>Reject the nomination.</summary>
    public static GuideFindingDecision Rejected { get; } = new(RejectedValue);

    /// <summary>Whether this version of the SDK declares the token. False for a value a newer server sent.</summary>
    public bool IsKnown => _value is AcceptedValue or RejectedValue;

    /// <summary>Decode a token a caller supplies; only a declared guide finding decision is accepted.</summary>
    /// <param name="value">The wire token.</param>
    /// <returns>The declared value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="value"/> is not a guide finding decision this SDK declares.</exception>
    public static GuideFindingDecision Parse(string value) => WireToken.Parse<GuideFindingDecision>(value, "guide finding decision");

    /// <summary>Decode a token a caller supplies without throwing; only a declared guide finding decision is accepted.</summary>
    /// <param name="value">The wire token.</param>
    /// <param name="result">The declared value, or <c>default</c> when the token is not declared.</param>
    /// <returns>True when <paramref name="value"/> is a declared token.</returns>
    public static bool TryParse(string? value, out GuideFindingDecision result) => WireToken.TryParse(value, out result);

    /// <summary>The wire token, exactly as it was received or constructed.</summary>
    /// <exception cref="InvalidOperationException">The value is <c>default(GuideFindingDecision)</c>, which carries no token.</exception>
    public string ToWire() => WireToken.Require(_value, nameof(GuideFindingDecision));

    /// <summary>The wire token; empty for <c>default(GuideFindingDecision)</c>.</summary>
    public override string ToString() => _value ?? string.Empty;

    /// <summary>Ordinal comparison of the wire tokens.</summary>
    public bool Equals(GuideFindingDecision other) => string.Equals(_value, other._value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is GuideFindingDecision other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => WireToken.Hash(_value);

    /// <summary>Ordinal comparison of the wire tokens.</summary>
    public static bool operator ==(GuideFindingDecision left, GuideFindingDecision right) => left.Equals(right);

    /// <summary>Ordinal comparison of the wire tokens.</summary>
    public static bool operator !=(GuideFindingDecision left, GuideFindingDecision right) => !left.Equals(right);

    string? IWireToken<GuideFindingDecision>.Token => _value;

    GuideFindingDecision IWireToken<GuideFindingDecision>.FromToken(string value) => new(value);
}

/// <summary>Authority state of a guide fact.</summary>
[JsonConverter(typeof(WireTokenJsonConverter<GuideFactKind>))]
public readonly struct GuideFactKind : IEquatable<GuideFactKind>, IWireToken<GuideFactKind>
{
    private const string AcceptedFindingValue = "accepted-finding";
    private const string ManualSelectionValue = "manual-selection";
    private const string ProjectedFindingValue = "projected-finding";

    private readonly string? _value;

    /// <summary>Wraps a wire token, whether or not this SDK declares it.</summary>
    /// <param name="value">The token exactly as it travels on the wire.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public GuideFactKind(string value) => _value = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary>Accepted lexical finding.</summary>
    public static GuideFactKind AcceptedFinding { get; } = new(AcceptedFindingValue);

    /// <summary>Analyst manual selection.</summary>
    public static GuideFactKind ManualSelection { get; } = new(ManualSelectionValue);

    /// <summary>Unresolved projected finding.</summary>
    public static GuideFactKind ProjectedFinding { get; } = new(ProjectedFindingValue);

    /// <summary>Whether this version of the SDK declares the token. False for a value a newer server sent.</summary>
    public bool IsKnown => _value is AcceptedFindingValue or ManualSelectionValue or ProjectedFindingValue;

    /// <summary>Decode a token a caller supplies; only a declared guide fact kind is accepted.</summary>
    /// <param name="value">The wire token.</param>
    /// <returns>The declared value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="value"/> is not a guide fact kind this SDK declares.</exception>
    public static GuideFactKind Parse(string value) => WireToken.Parse<GuideFactKind>(value, "guide fact kind");

    /// <summary>Decode a token a caller supplies without throwing; only a declared guide fact kind is accepted.</summary>
    /// <param name="value">The wire token.</param>
    /// <param name="result">The declared value, or <c>default</c> when the token is not declared.</param>
    /// <returns>True when <paramref name="value"/> is a declared token.</returns>
    public static bool TryParse(string? value, out GuideFactKind result) => WireToken.TryParse(value, out result);

    /// <summary>The wire token, exactly as it was received or constructed.</summary>
    /// <exception cref="InvalidOperationException">The value is <c>default(GuideFactKind)</c>, which carries no token.</exception>
    public string ToWire() => WireToken.Require(_value, nameof(GuideFactKind));

    /// <summary>The wire token; empty for <c>default(GuideFactKind)</c>.</summary>
    public override string ToString() => _value ?? string.Empty;

    /// <summary>Ordinal comparison of the wire tokens.</summary>
    public bool Equals(GuideFactKind other) => string.Equals(_value, other._value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is GuideFactKind other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => WireToken.Hash(_value);

    /// <summary>Ordinal comparison of the wire tokens.</summary>
    public static bool operator ==(GuideFactKind left, GuideFactKind right) => left.Equals(right);

    /// <summary>Ordinal comparison of the wire tokens.</summary>
    public static bool operator !=(GuideFactKind left, GuideFactKind right) => !left.Equals(right);

    string? IWireToken<GuideFactKind>.Token => _value;

    GuideFactKind IWireToken<GuideFactKind>.FromToken(string value) => new(value);
}

/// <summary>Identifies an immutable artifact participating in an operation.</summary>
public record ArtifactReference
{
    /// <summary>Artifact family.</summary>
    public required ArtifactKind Kind { get; init; }

    /// <summary>Artifact identifier.</summary>
    public required string Id { get; init; }

    /// <summary>Resolved immutable artifact version.</summary>
    public required string Version { get; init; }

    /// <summary>Content digest when the resolver can supply one.</summary>
    public string? Digest { get; init; }
}

/// <summary>Identifies the exact resolved semantics of one policy rule.</summary>
public record RuleReference
{
    /// <summary>Artifact that owns the rule.</summary>
    public required ArtifactReference Artifact { get; init; }

    /// <summary>Rule identifier within the owning artifact.</summary>
    public required string RuleId { get; init; }

    /// <summary>RFC 8785/SHA-256 digest of the canonical resolved rule content.</summary>
    public required string RuleDigest { get; init; }
}

/// <summary>Compact assertion identity without production metadata.</summary>
public record AssertionReference
{
    /// <summary>Category containing the asserted concept.</summary>
    public required string CategoryId { get; init; }

    /// <summary>Asserted concept identifier.</summary>
    public required string ConceptId { get; init; }

    /// <summary>Captured value for value-bearing concepts.</summary>
    public string? Value { get; init; }
}

/// <summary>Single Context resolution shared by an operation's result branches.</summary>
public record ContextResolution
{
    /// <summary>Resolved Context identifier.</summary>
    public required string ContextId { get; init; }

    /// <summary>Concrete Context version used by the operation.</summary>
    public required string ResolvedVersion { get; init; }

    /// <summary>Ordered artifacts participating in the composition.</summary>
    public required IReadOnlyList<ArtifactReference> Artifacts { get; init; }

    /// <summary>The version the request named, an alias such as <c>latest</c> included, and <c>latest</c> when the request named none. <c>resolvedVersion</c> is the concrete version that answered. Absent only from a resolution produced in process without a request.</summary>
    public string? RequestedVersion { get; init; }
}

/// <summary>Stable explanation of a constrained assessment dimension.</summary>
public record AssessmentLimitation
{
    /// <summary>Stable machine-readable limitation code.</summary>
    public required string Code { get; init; }

    /// <summary>Human-readable explanation safe for the selected response profile.</summary>
    public required string Reason { get; init; }

    /// <summary>Optional operation scope affected by the limitation.</summary>
    public string? Scope { get; init; }
}

/// <summary>Counts from the full result before response projection.</summary>
public record DocumentResponseCounts
{
    /// <summary>Number of lexical nominations.</summary>
    public required int Nominations { get; init; }

    /// <summary>Number of evidence spans.</summary>
    public required int EvidenceSpans { get; init; }

    /// <summary>Number of evaluated guide units.</summary>
    public required int GuideUnits { get; init; }
}

/// <summary>Explicit detail projection applied after full evaluation.</summary>
public record DocumentResponseProjection
{
    /// <summary>Requested response detail profile.</summary>
    public required DocumentResponseProfile Profile { get; init; }

    /// <summary>True only when the full profile returned every detail.</summary>
    public required bool DetailsComplete { get; init; }

    /// <summary>JSON-pointer paths omitted by the selected profile.</summary>
    public required IReadOnlyList<string> OmittedFields { get; init; }

    /// <summary>Counts from the full result before projection.</summary>
    public required DocumentResponseCounts Counts { get; init; }
}

/// <summary>Typed value source captured during access evaluation.</summary>
public record AccessOperand
{
    /// <summary>Source domain from which the value was read.</summary>
    public required AccessOperandSource Source { get; init; }

    /// <summary>Dot-path for subject or context operands.</summary>
    public string? Path { get; init; }

    /// <summary>Category addressed by an assertion operand.</summary>
    public string? CategoryId { get; init; }

    /// <summary>Assertions addressed by an assertion operand.</summary>
    public IReadOnlyList<AssertionReference>? Assertions { get; init; }

    /// <summary>Literal or resolved opaque value.</summary>
    public JsonElement? Value { get; init; }
}

/// <summary>Machine-readable result of one access comparison.</summary>
public record AccessComparisonResult
{
    /// <summary>Left comparison operand.</summary>
    public required AccessOperand Left { get; init; }

    /// <summary>Policy operator applied to the operands.</summary>
    public required string Operator { get; init; }

    /// <summary>Right comparison operand.</summary>
    public required AccessOperand Right { get; init; }

    /// <summary>Whether the comparison succeeded.</summary>
    public required bool Matched { get; init; }

    /// <summary>Required values missing from the evaluated operand, when applicable.</summary>
    public IReadOnlyList<JsonElement?>? Missing { get; init; }
}

/// <summary>Machine-readable record of why an access rule activated.</summary>
public record AccessRuleActivation
{
    /// <summary>Whether the rule's activation predicate matched.</summary>
    public required bool Matched { get; init; }

    /// <summary>Assertions that activated the rule.</summary>
    public required IReadOnlyList<AssertionReference> Assertions { get; init; }

    /// <summary>Context paths consulted by activation.</summary>
    public required IReadOnlyList<string> ContextPaths { get; init; }
}

/// <summary>UTF-8 byte range containing guide evidence.</summary>
public record ContentEvidenceSpan
{
    /// <summary>Zero-based inclusive byte offset.</summary>
    public required int Start { get; init; }

    /// <summary>Zero-based exclusive byte offset.</summary>
    public required int End { get; init; }

    /// <summary>Frozen offset encoding for document evidence.</summary>
    public required string OffsetEncoding { get; init; }

    /// <summary>Qualified rule that produced the evidence.</summary>
    public required RuleReference Rule { get; init; }

    /// <summary>Matched source text, present only in the full response profile.</summary>
    public string? Matched { get; init; }
}

/// <summary>Lexical nomination requiring analyst review.</summary>
public record GuideNomination
{
    /// <summary>Deterministic identity of this evidence occurrence.</summary>
    public required string FindingId { get; init; }

    /// <summary>Digest of the exact scanned document bytes.</summary>
    public required string DocumentDigest { get; init; }

    /// <summary>Qualified rule nominated by the evidence.</summary>
    public required RuleReference Rule { get; init; }

    /// <summary>Evidence occurrences supporting the nomination.</summary>
    public required IReadOnlyList<ContentEvidenceSpan> Evidence { get; init; }

    /// <summary>Assertions the rule would propose if accepted.</summary>
    public required IReadOnlyList<AssertionReference> ProposedAssertions { get; init; }

    /// <summary>Nomination source; lexical evidence is never authoritative itself.</summary>
    public required string Origin { get; init; }

    /// <summary>Rule-authored explanation for the nomination.</summary>
    public string? Rationale { get; init; }
}

/// <summary>Explicit analyst decision about one nomination.</summary>
public record GuideFindingDetermination
{
    /// <summary>Stable identifier of the exact nominated occurrence.</summary>
    public required string FindingId { get; init; }

    /// <summary>Digest of the exact document revision reviewed.</summary>
    public required string DocumentDigest { get; init; }

    /// <summary>Analyst decision for the nominated occurrence.</summary>
    public required GuideFindingDecision Decision { get; init; }

    /// <summary>Analyst-supplied reason for the decision.</summary>
    public required string Reason { get; init; }

    /// <summary>Optional provenance for how the decision proposal was drafted.</summary>
    public DecisionProposalOrigin? ProposalOrigin { get; init; }
}

/// <summary>Analyst-selected rule input that lexical scanning missed.</summary>
public record ManualGuideSelectionInput
{
    /// <summary>Digest of the exact document revision reviewed.</summary>
    public required string DocumentDigest { get; init; }

    /// <summary>Qualified loaded rule selected by the analyst.</summary>
    public required RuleReference Rule { get; init; }

    /// <summary>Evidence in the submitted document supporting the selection.</summary>
    public required IReadOnlyList<ContentEvidenceSpan> Evidence { get; init; }

    /// <summary>Analyst-supplied reason for the selection.</summary>
    public required string Reason { get; init; }

    /// <summary>Optional provenance for how the selection proposal was drafted.</summary>
    public DecisionProposalOrigin? ProposalOrigin { get; init; }
}

/// <summary>Validated manual guide fact bound to a document revision.</summary>
public record ManualGuideSelection
{
    /// <summary>Digest of the exact document revision reviewed.</summary>
    public required string DocumentDigest { get; init; }

    /// <summary>Qualified loaded rule selected by the analyst.</summary>
    public required RuleReference Rule { get; init; }

    /// <summary>Evidence in the submitted document supporting the selection.</summary>
    public required IReadOnlyList<ContentEvidenceSpan> Evidence { get; init; }

    /// <summary>Analyst-supplied reason for the selection.</summary>
    public required string Reason { get; init; }

    /// <summary>Deterministic selection identity.</summary>
    public required string SelectionId { get; init; }

    /// <summary>Optional provenance for how the selection proposal was drafted.</summary>
    public DecisionProposalOrigin? ProposalOrigin { get; init; }
}

/// <summary>Analyst determinations submitted with a document operation.</summary>
public record GuideDeterminationInput
{
    /// <summary>Decisions for lexical nominations.</summary>
    public required IReadOnlyList<GuideFindingDetermination> Findings { get; init; }

    /// <summary>Qualified rules manually selected by the analyst.</summary>
    public required IReadOnlyList<ManualGuideSelectionInput> ManualSelections { get; init; }
}

/// <summary>Join from assertion support to an authority-bearing guide fact.</summary>
public record GuideFactReference
{
    /// <summary>Authority state of the referenced guide fact.</summary>
    public required GuideFactKind Kind { get; init; }

    /// <summary>Finding or manual-selection identifier.</summary>
    public required string FactId { get; init; }
}

/// <summary>De-duplicated content lineage for one compiled assertion.</summary>
public record AssertionSupport
{
    /// <summary>Content-derived stable support identifier.</summary>
    public required string SupportId { get; init; }

    /// <summary>Assertion supported by the cited facts and rules.</summary>
    public required AssertionReference Assertion { get; init; }

    /// <summary>Direct and computed rules contributing to the assertion.</summary>
    public required IReadOnlyList<RuleReference> Rules { get; init; }

    /// <summary>Canonically ordered evidence for the assertion.</summary>
    public required IReadOnlyList<ContentEvidenceSpan> Evidence { get; init; }

    /// <summary>Human-decision or projected facts supplying the assertion.</summary>
    public required IReadOnlyList<GuideFactReference> FactReferences { get; init; }
}

/// <summary>Deterministic guide compilation for one authority basis and unit.</summary>
public record GuideCompiledResult
{
    /// <summary>Effective compiled assertions, listed in print order as a validate result's are.</summary>
    public required IReadOnlyList<Assertion> Assertions { get; init; }

    /// <summary>Scheme-rendered marking for the compiled assertions.</summary>
    public required string Rendered { get; init; }

    /// <summary>Whether the compiled marking stands once settled, as <c>ValidateResult.valid</c> defines it.</summary>
    public required bool Valid { get; init; }

    /// <summary>Validation errors from scheme processing.</summary>
    public required IReadOnlyList<ValidationError> Errors { get; init; }

    /// <summary>Non-fatal scheme-processing warnings.</summary>
    public required IReadOnlyList<Warning> Warnings { get; init; }

    /// <summary>Direct rules contributing to the result.</summary>
    public required IReadOnlyList<RuleReference> DirectRules { get; init; }

    /// <summary>Computed rules contributing to the result.</summary>
    public required IReadOnlyList<RuleReference> ComputedRules { get; init; }

    /// <summary>Rules replaced by active overrides.</summary>
    public required IReadOnlyList<RuleReference> OverriddenRules { get; init; }

    /// <summary>References into the evaluation support catalog.</summary>
    public required IReadOnlyList<string> SupportIds { get; init; }

    /// <summary>Complete evaluation and process breakdown for the full profile.</summary>
    public GuideCompiledDetail? Detail { get; init; }
}

/// <summary>The guide evaluation and the validate answer a compiled result was built from.</summary>
public record GuideCompiledDetail
{
    /// <summary>Complete guide evaluation breakdown.</summary>
    public required EvaluationResult Evaluation { get; init; }

    /// <summary>What validate answers for the compiled assertions: the facts settled, whether they stand, and why not. A compiled marking is checked, not corrected, so it carries no fix paths.</summary>
    public required ValidateResult Process { get; init; }
}

/// <summary>Determined and projected compilation for one document unit.</summary>
public record GuideUnitEvaluation
{
    /// <summary>Opaque stable scope identifier.</summary>
    public required string ScopeId { get; init; }

    /// <summary>Zero-based inclusive UTF-8 byte offset.</summary>
    public required int Start { get; init; }

    /// <summary>Zero-based exclusive UTF-8 byte offset.</summary>
    public required int End { get; init; }

    /// <summary>Frozen byte-offset encoding.</summary>
    public required string OffsetEncoding { get; init; }

    /// <summary>Nomination IDs whose evidence belongs to this unit.</summary>
    public required IReadOnlyList<string> FindingIds { get; init; }

    /// <summary>Compilation over accepted findings and manual selections.</summary>
    public required GuideCompiledResult? Determined { get; init; }

    /// <summary>Advisory compilation additionally assuming unresolved nominations.</summary>
    public required GuideCompiledResult? Projected { get; init; }
}

/// <summary>Complete evidence and compilation result for document guide evaluation.</summary>
public record GuideContentEvaluation
{
    /// <summary>Digest of the exact evaluated document bytes.</summary>
    public required string DocumentDigest { get; init; }

    /// <summary>Canonically ordered lexical nominations.</summary>
    public required IReadOnlyList<GuideNomination> Nominations { get; init; }

    /// <summary>Validated analyst decisions.</summary>
    public required IReadOnlyList<GuideFindingDetermination> Determinations { get; init; }

    /// <summary>Validated qualified manual selections.</summary>
    public required IReadOnlyList<ManualGuideSelection> ManualSelections { get; init; }

    /// <summary>Nomination IDs awaiting analyst decision.</summary>
    public required IReadOnlyList<string> UnresolvedFindingIds { get; init; }

    /// <summary>Accepted nomination IDs.</summary>
    public required IReadOnlyList<string> AcceptedFindingIds { get; init; }

    /// <summary>Rejected nomination IDs.</summary>
    public required IReadOnlyList<string> RejectedFindingIds { get; init; }

    /// <summary>De-duplicated assertion support graph.</summary>
    public required IReadOnlyList<AssertionSupport> SupportCatalog { get; init; }

    /// <summary>Compilation results for the document and authored parts.</summary>
    public required IReadOnlyList<GuideUnitEvaluation> Units { get; init; }

    /// <summary>Guide-dimension limitations.</summary>
    public required IReadOnlyList<AssessmentLimitation> Limitations { get; init; }
}

/// <summary>Coverage of one guide unit against determined and projected requirements.</summary>
public record GuideScopeCoverage
{
    /// <summary>Unit scope identifier.</summary>
    public required string ScopeId { get; init; }

    /// <summary>Coverage against currently accepted/manual facts.</summary>
    public required CoverageReport? DeterminedCoverage { get; init; }

    /// <summary>Advisory coverage assuming unresolved nominations are accepted.</summary>
    public required CoverageReport? ProjectedCoverage { get; init; }
}

/// <summary>Guide dimension of a document audit.</summary>
public record DocumentGuideAssessment
{
    /// <summary>Conservative status of the determined compilation.</summary>
    public required GuideCompilationStatus Status { get; init; }

    /// <summary>Number of lexical nominations.</summary>
    public required int NominationCount { get; init; }

    /// <summary>Number of unresolved nominations.</summary>
    public required int UnresolvedCount { get; init; }

    /// <summary>Number of accepted nominations.</summary>
    public required int AcceptedCount { get; init; }

    /// <summary>Number of rejected nominations.</summary>
    public required int RejectedCount { get; init; }

    /// <summary>Number of evidence spans.</summary>
    public required int EvidenceSpanCount { get; init; }

    /// <summary>Determined/projected coverage for each evaluation unit.</summary>
    public required IReadOnlyList<GuideScopeCoverage> UnitCoverage { get; init; }

    /// <summary>Whole-document requirement from accepted/manual facts.</summary>
    public required GuideCompiledResult? DeterminedRequiredWhole { get; init; }

    /// <summary>Advisory whole-document requirement including unresolved facts.</summary>
    public required GuideCompiledResult? ProjectedRequiredWhole { get; init; }

    /// <summary>Authored whole coverage of the determined requirement.</summary>
    public required CoverageReport? DeterminedWholeCoverage { get; init; }

    /// <summary>Authored whole coverage of the projected requirement.</summary>
    public required CoverageReport? ProjectedWholeCoverage { get; init; }

    /// <summary>Evidence and unit detail, omitted by the summary profile.</summary>
    public GuideContentEvaluation? Evaluation { get; init; }
}

/// <summary>Resolved Context envelope returned by document audit.</summary>
public record ContextDocumentAuditResult
{
    /// <summary>Single resolved composition used throughout the operation.</summary>
    public required ContextResolution Resolution { get; init; }

    /// <summary>Typed operation result.</summary>
    public required DocumentAuditReport Result { get; init; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public required DocumentResponseProjection ResponseProjection { get; init; }
}
