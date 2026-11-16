using EzClassify.CSharp73;
using Xunit;

namespace EzClassify.Tests;

/// <summary>
/// The records a caller sends are built in EzClassify.Rest.CSharp73, a C# 7.3
/// project against the netstandard2.0 client, which is what a .NET Framework
/// project gets by default. That it compiles is the language-floor proof; these
/// tests confirm what it builds carries the values it was given.
/// </summary>
public class LanguageFloorTests
{
    [Fact]
    public void AFactBuiltInCSharp73CarriesItsRequiredAndOptionalMembers()
    {
        var fact = RequestRecords.Fact();

        Assert.Equal("level", fact.CategoryId);
        Assert.Equal("CONFIDENTIAL", fact.ConceptId);
        Assert.Equal(Confidence.Certain, fact.Confidence);
        Assert.Equal("user", fact.Source);
        Assert.Equal("text", fact.Value);
        Assert.Equal("rule-1", fact.SourceRule);
        Assert.Equal("SECRET", fact.ReplacedConceptId);
    }

    [Fact]
    public void GuideInputsBuiltInCSharp73CarryTheirMembers()
    {
        Assert.Equal(("guide-1", "rule-1"), (RequestRecords.Selection().GuideId, RequestRecords.Selection().RuleId));

        var determinations = RequestRecords.Determinations();
        var finding = Assert.Single(determinations.Findings);
        Assert.Equal(GuideFindingDecision.Accepted, finding.Decision);
        Assert.Equal(DecisionProposalOrigin.AnalystAuthored, finding.ProposalOrigin);

        var manual = Assert.Single(determinations.ManualSelections);
        var span = Assert.Single(manual.Evidence);
        Assert.Equal((0, 4, "text"), (span.Start, span.End, span.Matched));
        Assert.Equal("sha256:guide", manual.Rule.Artifact.Digest);
    }

    [Fact]
    public void CacheOptionsBuiltInCSharp73KeepTheChoice()
    {
        Assert.False(RequestRecords.CacheOff().Enabled);
    }
}
