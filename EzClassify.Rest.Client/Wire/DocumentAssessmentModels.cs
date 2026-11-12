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
public sealed record ArtifactReference
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="kind">Artifact family.</param>
    /// <param name="id">Artifact identifier.</param>
    /// <param name="version">Resolved immutable artifact version.</param>
    public ArtifactReference(ArtifactKind kind, string id, string version)
    {
        Kind = kind;
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Version = version ?? throw new ArgumentNullException(nameof(version));
    }

    /// <summary>Artifact family.</summary>
    public ArtifactKind Kind { get; }

    /// <summary>Artifact identifier.</summary>
    public string Id { get; }

    /// <summary>Resolved immutable artifact version.</summary>
    public string Version { get; }

    /// <summary>Content digest when the resolver can supply one.</summary>
    public string? Digest { get; set; }
}

/// <summary>Identifies the exact resolved semantics of one policy rule.</summary>
public sealed record RuleReference
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="artifact">Artifact that owns the rule.</param>
    /// <param name="ruleId">Rule identifier within the owning artifact.</param>
    /// <param name="ruleDigest">RFC 8785/SHA-256 digest of the canonical resolved rule content.</param>
    public RuleReference(ArtifactReference artifact, string ruleId, string ruleDigest)
    {
        Artifact = artifact ?? throw new ArgumentNullException(nameof(artifact));
        RuleId = ruleId ?? throw new ArgumentNullException(nameof(ruleId));
        RuleDigest = ruleDigest ?? throw new ArgumentNullException(nameof(ruleDigest));
    }

    /// <summary>Artifact that owns the rule.</summary>
    public ArtifactReference Artifact { get; }

    /// <summary>Rule identifier within the owning artifact.</summary>
    public string RuleId { get; }

    /// <summary>RFC 8785/SHA-256 digest of the canonical resolved rule content.</summary>
    public string RuleDigest { get; }
}

/// <summary>Compact assertion identity without production metadata.</summary>
public sealed record AssertionReference
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="categoryId">Category containing the asserted concept.</param>
    /// <param name="conceptId">Asserted concept identifier.</param>
    public AssertionReference(string categoryId, string conceptId)
    {
        CategoryId = categoryId ?? throw new ArgumentNullException(nameof(categoryId));
        ConceptId = conceptId ?? throw new ArgumentNullException(nameof(conceptId));
    }

    /// <summary>Category containing the asserted concept.</summary>
    public string CategoryId { get; }

    /// <summary>Asserted concept identifier.</summary>
    public string ConceptId { get; }

    /// <summary>Captured value for value-bearing concepts.</summary>
    public string? Value { get; set; }
}

/// <summary>Single Context resolution shared by an operation's result branches.</summary>
public sealed record ContextResolution
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="contextId">Resolved Context identifier.</param>
    /// <param name="resolvedVersion">Concrete Context version used by the operation.</param>
    /// <param name="artifacts">Ordered artifacts participating in the composition.</param>
    public ContextResolution(string contextId, string resolvedVersion, IReadOnlyList<ArtifactReference> artifacts)
    {
        ContextId = contextId ?? throw new ArgumentNullException(nameof(contextId));
        ResolvedVersion = resolvedVersion ?? throw new ArgumentNullException(nameof(resolvedVersion));
        Artifacts = artifacts ?? throw new ArgumentNullException(nameof(artifacts));
    }

    /// <summary>Resolved Context identifier.</summary>
    public string ContextId { get; }

    /// <summary>Concrete Context version used by the operation.</summary>
    public string ResolvedVersion { get; }

    /// <summary>Ordered artifacts participating in the composition.</summary>
    public IReadOnlyList<ArtifactReference> Artifacts { get; }

    /// <summary>The version the request named, an alias such as <c>latest</c> included, and <c>latest</c> when the request named none. <c>resolvedVersion</c> is the concrete version that answered. Absent only from a resolution produced in process without a request.</summary>
    public string? RequestedVersion { get; set; }
}

/// <summary>Stable explanation of a constrained assessment dimension.</summary>
public sealed record AssessmentLimitation
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="code">Stable machine-readable limitation code.</param>
    /// <param name="reason">Human-readable explanation safe for the selected response profile.</param>
    public AssessmentLimitation(string code, string reason)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
    }

    /// <summary>Stable machine-readable limitation code.</summary>
    public string Code { get; }

    /// <summary>Human-readable explanation safe for the selected response profile.</summary>
    public string Reason { get; }

    /// <summary>Optional operation scope affected by the limitation.</summary>
    public string? Scope { get; set; }
}

/// <summary>Counts from the full result before response projection.</summary>
public sealed record DocumentResponseCounts
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="nominations">Number of lexical nominations.</param>
    /// <param name="evidenceSpans">Number of evidence spans.</param>
    /// <param name="guideUnits">Number of evaluated guide units.</param>
    public DocumentResponseCounts(int nominations, int evidenceSpans, int guideUnits)
    {
        Nominations = nominations;
        EvidenceSpans = evidenceSpans;
        GuideUnits = guideUnits;
    }

    /// <summary>Number of lexical nominations.</summary>
    public int Nominations { get; }

    /// <summary>Number of evidence spans.</summary>
    public int EvidenceSpans { get; }

    /// <summary>Number of evaluated guide units.</summary>
    public int GuideUnits { get; }
}

/// <summary>Explicit detail projection applied after full evaluation.</summary>
public sealed record DocumentResponseProjection
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="profile">Requested response detail profile.</param>
    /// <param name="detailsComplete">True only when the full profile returned every detail.</param>
    /// <param name="omittedFields">JSON-pointer paths omitted by the selected profile.</param>
    /// <param name="counts">Counts from the full result before projection.</param>
    public DocumentResponseProjection(DocumentResponseProfile profile, bool detailsComplete, IReadOnlyList<string> omittedFields, DocumentResponseCounts counts)
    {
        Profile = profile;
        DetailsComplete = detailsComplete;
        OmittedFields = omittedFields ?? throw new ArgumentNullException(nameof(omittedFields));
        Counts = counts ?? throw new ArgumentNullException(nameof(counts));
    }

    /// <summary>Requested response detail profile.</summary>
    public DocumentResponseProfile Profile { get; }

    /// <summary>True only when the full profile returned every detail.</summary>
    public bool DetailsComplete { get; }

    /// <summary>JSON-pointer paths omitted by the selected profile.</summary>
    public IReadOnlyList<string> OmittedFields { get; }

    /// <summary>Counts from the full result before projection.</summary>
    public DocumentResponseCounts Counts { get; }
}

/// <summary>Typed value source captured during access evaluation.</summary>
public sealed record AccessOperand
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="source">Source domain from which the value was read.</param>
    public AccessOperand(AccessOperandSource source)
    {
        Source = source;
    }

    /// <summary>Source domain from which the value was read.</summary>
    public AccessOperandSource Source { get; }

    /// <summary>Dot-path for subject or context operands.</summary>
    public string? Path { get; set; }

    /// <summary>Category addressed by an assertion operand.</summary>
    public string? CategoryId { get; set; }

    /// <summary>Assertions addressed by an assertion operand.</summary>
    public IReadOnlyList<AssertionReference>? Assertions { get; set; }

    /// <summary>Literal or resolved opaque value.</summary>
    public JsonElement? Value { get; set; }
}

/// <summary>Machine-readable result of one access comparison.</summary>
public sealed record AccessComparisonResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="left">Left comparison operand.</param>
    /// <param name="operator">Policy operator applied to the operands.</param>
    /// <param name="right">Right comparison operand.</param>
    /// <param name="matched">Whether the comparison succeeded.</param>
    public AccessComparisonResult(AccessOperand left, string @operator, AccessOperand right, bool matched)
    {
        Left = left ?? throw new ArgumentNullException(nameof(left));
        Operator = @operator ?? throw new ArgumentNullException(nameof(@operator));
        Right = right ?? throw new ArgumentNullException(nameof(right));
        Matched = matched;
    }

    /// <summary>Left comparison operand.</summary>
    public AccessOperand Left { get; }

    /// <summary>Policy operator applied to the operands.</summary>
    public string Operator { get; }

    /// <summary>Right comparison operand.</summary>
    public AccessOperand Right { get; }

    /// <summary>Whether the comparison succeeded.</summary>
    public bool Matched { get; }

    /// <summary>Required values missing from the evaluated operand, when applicable.</summary>
    public IReadOnlyList<JsonElement?>? Missing { get; set; }
}

/// <summary>Machine-readable record of why an access rule activated.</summary>
public sealed record AccessRuleActivation
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="matched">Whether the rule's activation predicate matched.</param>
    /// <param name="assertions">Assertions that activated the rule.</param>
    /// <param name="contextPaths">Context paths consulted by activation.</param>
    public AccessRuleActivation(bool matched, IReadOnlyList<AssertionReference> assertions, IReadOnlyList<string> contextPaths)
    {
        Matched = matched;
        Assertions = assertions ?? throw new ArgumentNullException(nameof(assertions));
        ContextPaths = contextPaths ?? throw new ArgumentNullException(nameof(contextPaths));
    }

    /// <summary>Whether the rule's activation predicate matched.</summary>
    public bool Matched { get; }

    /// <summary>Assertions that activated the rule.</summary>
    public IReadOnlyList<AssertionReference> Assertions { get; }

    /// <summary>Context paths consulted by activation.</summary>
    public IReadOnlyList<string> ContextPaths { get; }
}

/// <summary>UTF-8 byte range containing guide evidence.</summary>
public sealed record ContentEvidenceSpan
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="start">Zero-based inclusive byte offset.</param>
    /// <param name="end">Zero-based exclusive byte offset.</param>
    /// <param name="offsetEncoding">Frozen offset encoding for document evidence.</param>
    /// <param name="rule">Qualified rule that produced the evidence.</param>
    public ContentEvidenceSpan(int start, int end, string offsetEncoding, RuleReference rule)
    {
        Start = start;
        End = end;
        OffsetEncoding = offsetEncoding ?? throw new ArgumentNullException(nameof(offsetEncoding));
        Rule = rule ?? throw new ArgumentNullException(nameof(rule));
    }

    /// <summary>Zero-based inclusive byte offset.</summary>
    public int Start { get; }

    /// <summary>Zero-based exclusive byte offset.</summary>
    public int End { get; }

    /// <summary>Frozen offset encoding for document evidence.</summary>
    public string OffsetEncoding { get; }

    /// <summary>Qualified rule that produced the evidence.</summary>
    public RuleReference Rule { get; }

    /// <summary>Matched source text, present only in the full response profile.</summary>
    public string? Matched { get; set; }
}

/// <summary>Lexical nomination requiring analyst review.</summary>
public sealed record GuideNomination
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="findingId">Deterministic identity of this evidence occurrence.</param>
    /// <param name="documentDigest">Digest of the exact scanned document bytes.</param>
    /// <param name="rule">Qualified rule nominated by the evidence.</param>
    /// <param name="evidence">Evidence occurrences supporting the nomination.</param>
    /// <param name="proposedAssertions">Assertions the rule would propose if accepted.</param>
    /// <param name="origin">Nomination source; lexical evidence is never authoritative itself.</param>
    public GuideNomination(string findingId, string documentDigest, RuleReference rule, IReadOnlyList<ContentEvidenceSpan> evidence, IReadOnlyList<AssertionReference> proposedAssertions, string origin)
    {
        FindingId = findingId ?? throw new ArgumentNullException(nameof(findingId));
        DocumentDigest = documentDigest ?? throw new ArgumentNullException(nameof(documentDigest));
        Rule = rule ?? throw new ArgumentNullException(nameof(rule));
        Evidence = evidence ?? throw new ArgumentNullException(nameof(evidence));
        ProposedAssertions = proposedAssertions ?? throw new ArgumentNullException(nameof(proposedAssertions));
        Origin = origin ?? throw new ArgumentNullException(nameof(origin));
    }

    /// <summary>Deterministic identity of this evidence occurrence.</summary>
    public string FindingId { get; }

    /// <summary>Digest of the exact scanned document bytes.</summary>
    public string DocumentDigest { get; }

    /// <summary>Qualified rule nominated by the evidence.</summary>
    public RuleReference Rule { get; }

    /// <summary>Evidence occurrences supporting the nomination.</summary>
    public IReadOnlyList<ContentEvidenceSpan> Evidence { get; }

    /// <summary>Assertions the rule would propose if accepted.</summary>
    public IReadOnlyList<AssertionReference> ProposedAssertions { get; }

    /// <summary>Nomination source; lexical evidence is never authoritative itself.</summary>
    public string Origin { get; }

    /// <summary>Rule-authored explanation for the nomination.</summary>
    public string? Rationale { get; set; }
}

/// <summary>Explicit analyst decision about one nomination.</summary>
public sealed record GuideFindingDetermination
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="findingId">Stable identifier of the exact nominated occurrence.</param>
    /// <param name="documentDigest">Digest of the exact document revision reviewed.</param>
    /// <param name="decision">Analyst decision for the nominated occurrence.</param>
    /// <param name="reason">Analyst-supplied reason for the decision.</param>
    public GuideFindingDetermination(string findingId, string documentDigest, GuideFindingDecision decision, string reason)
    {
        FindingId = findingId ?? throw new ArgumentNullException(nameof(findingId));
        DocumentDigest = documentDigest ?? throw new ArgumentNullException(nameof(documentDigest));
        Decision = decision;
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
    }

    /// <summary>Stable identifier of the exact nominated occurrence.</summary>
    public string FindingId { get; }

    /// <summary>Digest of the exact document revision reviewed.</summary>
    public string DocumentDigest { get; }

    /// <summary>Analyst decision for the nominated occurrence.</summary>
    public GuideFindingDecision Decision { get; }

    /// <summary>Analyst-supplied reason for the decision.</summary>
    public string Reason { get; }

    /// <summary>Optional provenance for how the decision proposal was drafted.</summary>
    public DecisionProposalOrigin? ProposalOrigin { get; set; }
}

/// <summary>Analyst-selected rule input that lexical scanning missed.</summary>
public sealed record ManualGuideSelectionInput
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="documentDigest">Digest of the exact document revision reviewed.</param>
    /// <param name="rule">Qualified loaded rule selected by the analyst.</param>
    /// <param name="evidence">Evidence in the submitted document supporting the selection.</param>
    /// <param name="reason">Analyst-supplied reason for the selection.</param>
    public ManualGuideSelectionInput(string documentDigest, RuleReference rule, IReadOnlyList<ContentEvidenceSpan> evidence, string reason)
    {
        DocumentDigest = documentDigest ?? throw new ArgumentNullException(nameof(documentDigest));
        Rule = rule ?? throw new ArgumentNullException(nameof(rule));
        Evidence = evidence ?? throw new ArgumentNullException(nameof(evidence));
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
    }

    /// <summary>Digest of the exact document revision reviewed.</summary>
    public string DocumentDigest { get; }

    /// <summary>Qualified loaded rule selected by the analyst.</summary>
    public RuleReference Rule { get; }

    /// <summary>Evidence in the submitted document supporting the selection.</summary>
    public IReadOnlyList<ContentEvidenceSpan> Evidence { get; }

    /// <summary>Analyst-supplied reason for the selection.</summary>
    public string Reason { get; }

    /// <summary>Optional provenance for how the selection proposal was drafted.</summary>
    public DecisionProposalOrigin? ProposalOrigin { get; set; }
}

/// <summary>Validated manual guide fact bound to a document revision.</summary>
public sealed record ManualGuideSelection
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="documentDigest">Digest of the exact document revision reviewed.</param>
    /// <param name="rule">Qualified loaded rule selected by the analyst.</param>
    /// <param name="evidence">Evidence in the submitted document supporting the selection.</param>
    /// <param name="reason">Analyst-supplied reason for the selection.</param>
    /// <param name="selectionId">Deterministic selection identity.</param>
    public ManualGuideSelection(string documentDigest, RuleReference rule, IReadOnlyList<ContentEvidenceSpan> evidence, string reason, string selectionId)
    {
        DocumentDigest = documentDigest ?? throw new ArgumentNullException(nameof(documentDigest));
        Rule = rule ?? throw new ArgumentNullException(nameof(rule));
        Evidence = evidence ?? throw new ArgumentNullException(nameof(evidence));
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        SelectionId = selectionId ?? throw new ArgumentNullException(nameof(selectionId));
    }

    /// <summary>Digest of the exact document revision reviewed.</summary>
    public string DocumentDigest { get; }

    /// <summary>Qualified loaded rule selected by the analyst.</summary>
    public RuleReference Rule { get; }

    /// <summary>Evidence in the submitted document supporting the selection.</summary>
    public IReadOnlyList<ContentEvidenceSpan> Evidence { get; }

    /// <summary>Analyst-supplied reason for the selection.</summary>
    public string Reason { get; }

    /// <summary>Deterministic selection identity.</summary>
    public string SelectionId { get; }

    /// <summary>Optional provenance for how the selection proposal was drafted.</summary>
    public DecisionProposalOrigin? ProposalOrigin { get; set; }
}

/// <summary>Analyst determinations submitted with a document operation.</summary>
public sealed record GuideDeterminationInput
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="findings">Decisions for lexical nominations.</param>
    /// <param name="manualSelections">Qualified rules manually selected by the analyst.</param>
    public GuideDeterminationInput(IReadOnlyList<GuideFindingDetermination> findings, IReadOnlyList<ManualGuideSelectionInput> manualSelections)
    {
        Findings = findings ?? throw new ArgumentNullException(nameof(findings));
        ManualSelections = manualSelections ?? throw new ArgumentNullException(nameof(manualSelections));
    }

    /// <summary>Decisions for lexical nominations.</summary>
    public IReadOnlyList<GuideFindingDetermination> Findings { get; }

    /// <summary>Qualified rules manually selected by the analyst.</summary>
    public IReadOnlyList<ManualGuideSelectionInput> ManualSelections { get; }
}

/// <summary>Join from assertion support to an authority-bearing guide fact.</summary>
public sealed record GuideFactReference
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="kind">Authority state of the referenced guide fact.</param>
    /// <param name="factId">Finding or manual-selection identifier.</param>
    public GuideFactReference(GuideFactKind kind, string factId)
    {
        Kind = kind;
        FactId = factId ?? throw new ArgumentNullException(nameof(factId));
    }

    /// <summary>Authority state of the referenced guide fact.</summary>
    public GuideFactKind Kind { get; }

    /// <summary>Finding or manual-selection identifier.</summary>
    public string FactId { get; }
}

/// <summary>De-duplicated content lineage for one compiled assertion.</summary>
public sealed record AssertionSupport
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="supportId">Content-derived stable support identifier.</param>
    /// <param name="assertion">Assertion supported by the cited facts and rules.</param>
    /// <param name="rules">Direct and computed rules contributing to the assertion.</param>
    /// <param name="evidence">Canonically ordered evidence for the assertion.</param>
    /// <param name="factReferences">Human-decision or projected facts supplying the assertion.</param>
    public AssertionSupport(string supportId, AssertionReference assertion, IReadOnlyList<RuleReference> rules, IReadOnlyList<ContentEvidenceSpan> evidence, IReadOnlyList<GuideFactReference> factReferences)
    {
        SupportId = supportId ?? throw new ArgumentNullException(nameof(supportId));
        Assertion = assertion ?? throw new ArgumentNullException(nameof(assertion));
        Rules = rules ?? throw new ArgumentNullException(nameof(rules));
        Evidence = evidence ?? throw new ArgumentNullException(nameof(evidence));
        FactReferences = factReferences ?? throw new ArgumentNullException(nameof(factReferences));
    }

    /// <summary>Content-derived stable support identifier.</summary>
    public string SupportId { get; }

    /// <summary>Assertion supported by the cited facts and rules.</summary>
    public AssertionReference Assertion { get; }

    /// <summary>Direct and computed rules contributing to the assertion.</summary>
    public IReadOnlyList<RuleReference> Rules { get; }

    /// <summary>Canonically ordered evidence for the assertion.</summary>
    public IReadOnlyList<ContentEvidenceSpan> Evidence { get; }

    /// <summary>Human-decision or projected facts supplying the assertion.</summary>
    public IReadOnlyList<GuideFactReference> FactReferences { get; }
}

/// <summary>Deterministic guide compilation for one authority basis and unit.</summary>
public sealed record GuideCompiledResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="assertions">Effective compiled assertions, listed in print order as a validate result's are.</param>
    /// <param name="rendered">Scheme-rendered marking for the compiled assertions.</param>
    /// <param name="valid">Whether the compiled marking stands once settled, as <c>ValidateResult.valid</c> defines it.</param>
    /// <param name="errors">Validation errors from scheme processing.</param>
    /// <param name="warnings">Non-fatal scheme-processing warnings.</param>
    /// <param name="directRules">Direct rules contributing to the result.</param>
    /// <param name="computedRules">Computed rules contributing to the result.</param>
    /// <param name="overriddenRules">Rules replaced by active overrides.</param>
    /// <param name="supportIds">References into the evaluation support catalog.</param>
    public GuideCompiledResult(IReadOnlyList<Assertion> assertions, string rendered, bool valid, IReadOnlyList<ValidationError> errors, IReadOnlyList<Warning> warnings, IReadOnlyList<RuleReference> directRules, IReadOnlyList<RuleReference> computedRules, IReadOnlyList<RuleReference> overriddenRules, IReadOnlyList<string> supportIds)
    {
        Assertions = assertions ?? throw new ArgumentNullException(nameof(assertions));
        Rendered = rendered ?? throw new ArgumentNullException(nameof(rendered));
        Valid = valid;
        Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        Warnings = warnings ?? throw new ArgumentNullException(nameof(warnings));
        DirectRules = directRules ?? throw new ArgumentNullException(nameof(directRules));
        ComputedRules = computedRules ?? throw new ArgumentNullException(nameof(computedRules));
        OverriddenRules = overriddenRules ?? throw new ArgumentNullException(nameof(overriddenRules));
        SupportIds = supportIds ?? throw new ArgumentNullException(nameof(supportIds));
    }

    /// <summary>Effective compiled assertions, listed in print order as a validate result's are.</summary>
    public IReadOnlyList<Assertion> Assertions { get; }

    /// <summary>Scheme-rendered marking for the compiled assertions.</summary>
    public string Rendered { get; }

    /// <summary>Whether the compiled marking stands once settled, as <c>ValidateResult.valid</c> defines it.</summary>
    public bool Valid { get; }

    /// <summary>Validation errors from scheme processing.</summary>
    public IReadOnlyList<ValidationError> Errors { get; }

    /// <summary>Non-fatal scheme-processing warnings.</summary>
    public IReadOnlyList<Warning> Warnings { get; }

    /// <summary>Direct rules contributing to the result.</summary>
    public IReadOnlyList<RuleReference> DirectRules { get; }

    /// <summary>Computed rules contributing to the result.</summary>
    public IReadOnlyList<RuleReference> ComputedRules { get; }

    /// <summary>Rules replaced by active overrides.</summary>
    public IReadOnlyList<RuleReference> OverriddenRules { get; }

    /// <summary>References into the evaluation support catalog.</summary>
    public IReadOnlyList<string> SupportIds { get; }

    /// <summary>Complete evaluation and process breakdown for the full profile.</summary>
    public GuideCompiledDetail? Detail { get; set; }
}

/// <summary>The guide evaluation and the validate answer a compiled result was built from.</summary>
public sealed record GuideCompiledDetail
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="evaluation">Complete guide evaluation breakdown.</param>
    /// <param name="process">What validate answers for the compiled assertions: the facts settled, whether they stand, and why not. A compiled marking is checked, not corrected, so it carries no fix paths.</param>
    public GuideCompiledDetail(EvaluationResult evaluation, ValidateResult process)
    {
        Evaluation = evaluation ?? throw new ArgumentNullException(nameof(evaluation));
        Process = process ?? throw new ArgumentNullException(nameof(process));
    }

    /// <summary>Complete guide evaluation breakdown.</summary>
    public EvaluationResult Evaluation { get; }

    /// <summary>What validate answers for the compiled assertions: the facts settled, whether they stand, and why not. A compiled marking is checked, not corrected, so it carries no fix paths.</summary>
    public ValidateResult Process { get; }
}

/// <summary>Determined and projected compilation for one document unit.</summary>
public sealed record GuideUnitEvaluation
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="scopeId">Opaque stable scope identifier.</param>
    /// <param name="start">Zero-based inclusive UTF-8 byte offset.</param>
    /// <param name="end">Zero-based exclusive UTF-8 byte offset.</param>
    /// <param name="offsetEncoding">Frozen byte-offset encoding.</param>
    /// <param name="findingIds">Nomination IDs whose evidence belongs to this unit.</param>
    /// <param name="determined">Compilation over accepted findings and manual selections.</param>
    /// <param name="projected">Advisory compilation additionally assuming unresolved nominations.</param>
    public GuideUnitEvaluation(string scopeId, int start, int end, string offsetEncoding, IReadOnlyList<string> findingIds, GuideCompiledResult? determined, GuideCompiledResult? projected)
    {
        ScopeId = scopeId ?? throw new ArgumentNullException(nameof(scopeId));
        Start = start;
        End = end;
        OffsetEncoding = offsetEncoding ?? throw new ArgumentNullException(nameof(offsetEncoding));
        FindingIds = findingIds ?? throw new ArgumentNullException(nameof(findingIds));
        Determined = determined;
        Projected = projected;
    }

    /// <summary>Opaque stable scope identifier.</summary>
    public string ScopeId { get; }

    /// <summary>Zero-based inclusive UTF-8 byte offset.</summary>
    public int Start { get; }

    /// <summary>Zero-based exclusive UTF-8 byte offset.</summary>
    public int End { get; }

    /// <summary>Frozen byte-offset encoding.</summary>
    public string OffsetEncoding { get; }

    /// <summary>Nomination IDs whose evidence belongs to this unit.</summary>
    public IReadOnlyList<string> FindingIds { get; }

    /// <summary>Compilation over accepted findings and manual selections.</summary>
    public GuideCompiledResult? Determined { get; }

    /// <summary>Advisory compilation additionally assuming unresolved nominations.</summary>
    public GuideCompiledResult? Projected { get; }
}

/// <summary>Complete evidence and compilation result for document guide evaluation.</summary>
public sealed record GuideContentEvaluation
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="documentDigest">Digest of the exact evaluated document bytes.</param>
    /// <param name="nominations">Canonically ordered lexical nominations.</param>
    /// <param name="determinations">Validated analyst decisions.</param>
    /// <param name="manualSelections">Validated qualified manual selections.</param>
    /// <param name="unresolvedFindingIds">Nomination IDs awaiting analyst decision.</param>
    /// <param name="acceptedFindingIds">Accepted nomination IDs.</param>
    /// <param name="rejectedFindingIds">Rejected nomination IDs.</param>
    /// <param name="supportCatalog">De-duplicated assertion support graph.</param>
    /// <param name="units">Compilation results for the document and authored parts.</param>
    /// <param name="limitations">Guide-dimension limitations.</param>
    public GuideContentEvaluation(string documentDigest, IReadOnlyList<GuideNomination> nominations, IReadOnlyList<GuideFindingDetermination> determinations, IReadOnlyList<ManualGuideSelection> manualSelections, IReadOnlyList<string> unresolvedFindingIds, IReadOnlyList<string> acceptedFindingIds, IReadOnlyList<string> rejectedFindingIds, IReadOnlyList<AssertionSupport> supportCatalog, IReadOnlyList<GuideUnitEvaluation> units, IReadOnlyList<AssessmentLimitation> limitations)
    {
        DocumentDigest = documentDigest ?? throw new ArgumentNullException(nameof(documentDigest));
        Nominations = nominations ?? throw new ArgumentNullException(nameof(nominations));
        Determinations = determinations ?? throw new ArgumentNullException(nameof(determinations));
        ManualSelections = manualSelections ?? throw new ArgumentNullException(nameof(manualSelections));
        UnresolvedFindingIds = unresolvedFindingIds ?? throw new ArgumentNullException(nameof(unresolvedFindingIds));
        AcceptedFindingIds = acceptedFindingIds ?? throw new ArgumentNullException(nameof(acceptedFindingIds));
        RejectedFindingIds = rejectedFindingIds ?? throw new ArgumentNullException(nameof(rejectedFindingIds));
        SupportCatalog = supportCatalog ?? throw new ArgumentNullException(nameof(supportCatalog));
        Units = units ?? throw new ArgumentNullException(nameof(units));
        Limitations = limitations ?? throw new ArgumentNullException(nameof(limitations));
    }

    /// <summary>Digest of the exact evaluated document bytes.</summary>
    public string DocumentDigest { get; }

    /// <summary>Canonically ordered lexical nominations.</summary>
    public IReadOnlyList<GuideNomination> Nominations { get; }

    /// <summary>Validated analyst decisions.</summary>
    public IReadOnlyList<GuideFindingDetermination> Determinations { get; }

    /// <summary>Validated qualified manual selections.</summary>
    public IReadOnlyList<ManualGuideSelection> ManualSelections { get; }

    /// <summary>Nomination IDs awaiting analyst decision.</summary>
    public IReadOnlyList<string> UnresolvedFindingIds { get; }

    /// <summary>Accepted nomination IDs.</summary>
    public IReadOnlyList<string> AcceptedFindingIds { get; }

    /// <summary>Rejected nomination IDs.</summary>
    public IReadOnlyList<string> RejectedFindingIds { get; }

    /// <summary>De-duplicated assertion support graph.</summary>
    public IReadOnlyList<AssertionSupport> SupportCatalog { get; }

    /// <summary>Compilation results for the document and authored parts.</summary>
    public IReadOnlyList<GuideUnitEvaluation> Units { get; }

    /// <summary>Guide-dimension limitations.</summary>
    public IReadOnlyList<AssessmentLimitation> Limitations { get; }
}

/// <summary>Coverage of one guide unit against determined and projected requirements.</summary>
public sealed record GuideScopeCoverage
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="scopeId">Unit scope identifier.</param>
    /// <param name="determinedCoverage">Coverage against currently accepted/manual facts.</param>
    /// <param name="projectedCoverage">Advisory coverage assuming unresolved nominations are accepted.</param>
    public GuideScopeCoverage(string scopeId, CoverageReport? determinedCoverage, CoverageReport? projectedCoverage)
    {
        ScopeId = scopeId ?? throw new ArgumentNullException(nameof(scopeId));
        DeterminedCoverage = determinedCoverage;
        ProjectedCoverage = projectedCoverage;
    }

    /// <summary>Unit scope identifier.</summary>
    public string ScopeId { get; }

    /// <summary>Coverage against currently accepted/manual facts.</summary>
    public CoverageReport? DeterminedCoverage { get; }

    /// <summary>Advisory coverage assuming unresolved nominations are accepted.</summary>
    public CoverageReport? ProjectedCoverage { get; }
}

/// <summary>Guide dimension of a document audit.</summary>
public sealed record DocumentGuideAssessment
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="status">Conservative status of the determined compilation.</param>
    /// <param name="nominationCount">Number of lexical nominations.</param>
    /// <param name="unresolvedCount">Number of unresolved nominations.</param>
    /// <param name="acceptedCount">Number of accepted nominations.</param>
    /// <param name="rejectedCount">Number of rejected nominations.</param>
    /// <param name="evidenceSpanCount">Number of evidence spans.</param>
    /// <param name="unitCoverage">Determined/projected coverage for each evaluation unit.</param>
    /// <param name="determinedRequiredWhole">Whole-document requirement from accepted/manual facts.</param>
    /// <param name="projectedRequiredWhole">Advisory whole-document requirement including unresolved facts.</param>
    /// <param name="determinedWholeCoverage">Authored whole coverage of the determined requirement.</param>
    /// <param name="projectedWholeCoverage">Authored whole coverage of the projected requirement.</param>
    public DocumentGuideAssessment(GuideCompilationStatus status, int nominationCount, int unresolvedCount, int acceptedCount, int rejectedCount, int evidenceSpanCount, IReadOnlyList<GuideScopeCoverage> unitCoverage, GuideCompiledResult? determinedRequiredWhole, GuideCompiledResult? projectedRequiredWhole, CoverageReport? determinedWholeCoverage, CoverageReport? projectedWholeCoverage)
    {
        Status = status;
        NominationCount = nominationCount;
        UnresolvedCount = unresolvedCount;
        AcceptedCount = acceptedCount;
        RejectedCount = rejectedCount;
        EvidenceSpanCount = evidenceSpanCount;
        UnitCoverage = unitCoverage ?? throw new ArgumentNullException(nameof(unitCoverage));
        DeterminedRequiredWhole = determinedRequiredWhole;
        ProjectedRequiredWhole = projectedRequiredWhole;
        DeterminedWholeCoverage = determinedWholeCoverage;
        ProjectedWholeCoverage = projectedWholeCoverage;
    }

    /// <summary>Conservative status of the determined compilation.</summary>
    public GuideCompilationStatus Status { get; }

    /// <summary>Number of lexical nominations.</summary>
    public int NominationCount { get; }

    /// <summary>Number of unresolved nominations.</summary>
    public int UnresolvedCount { get; }

    /// <summary>Number of accepted nominations.</summary>
    public int AcceptedCount { get; }

    /// <summary>Number of rejected nominations.</summary>
    public int RejectedCount { get; }

    /// <summary>Number of evidence spans.</summary>
    public int EvidenceSpanCount { get; }

    /// <summary>Determined/projected coverage for each evaluation unit.</summary>
    public IReadOnlyList<GuideScopeCoverage> UnitCoverage { get; }

    /// <summary>Whole-document requirement from accepted/manual facts.</summary>
    public GuideCompiledResult? DeterminedRequiredWhole { get; }

    /// <summary>Advisory whole-document requirement including unresolved facts.</summary>
    public GuideCompiledResult? ProjectedRequiredWhole { get; }

    /// <summary>Authored whole coverage of the determined requirement.</summary>
    public CoverageReport? DeterminedWholeCoverage { get; }

    /// <summary>Authored whole coverage of the projected requirement.</summary>
    public CoverageReport? ProjectedWholeCoverage { get; }

    /// <summary>Evidence and unit detail, omitted by the summary profile.</summary>
    public GuideContentEvaluation? Evaluation { get; set; }
}

/// <summary>Resolved Context envelope returned by document audit.</summary>
public sealed record ContextDocumentAuditResult
{
    /// <summary>Creates the record from the members it requires.</summary>
    /// <param name="resolution">Single resolved composition used throughout the operation.</param>
    /// <param name="result">Typed operation result.</param>
    /// <param name="responseProjection">Projection metadata, present only for document operations.</param>
    public ContextDocumentAuditResult(ContextResolution resolution, DocumentAuditReport result, DocumentResponseProjection responseProjection)
    {
        Resolution = resolution ?? throw new ArgumentNullException(nameof(resolution));
        Result = result ?? throw new ArgumentNullException(nameof(result));
        ResponseProjection = responseProjection ?? throw new ArgumentNullException(nameof(responseProjection));
    }

    /// <summary>Single resolved composition used throughout the operation.</summary>
    public ContextResolution Resolution { get; }

    /// <summary>Typed operation result.</summary>
    public DocumentAuditReport Result { get; }

    /// <summary>Projection metadata, present only for document operations.</summary>
    public DocumentResponseProjection ResponseProjection { get; }
}
