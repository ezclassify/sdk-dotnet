using System.Collections.Generic;

namespace EzClassify.CSharp73
{
    /// <summary>
    /// Builds each record a caller hands the client, in C# 7.3: the members a
    /// record requires go through its constructor and the optional ones through
    /// an object initializer.
    /// </summary>
    public static class RequestRecords
    {
        /// <summary>A stated fact with every optional member set.</summary>
        public static Assertion Fact()
        {
            return new Assertion("level", "CONFIDENTIAL", Confidence.Certain, "user")
            {
                Value = "text",
                SourceRule = "rule-1",
                ReplacedConceptId = "SECRET",
            };
        }

        /// <summary>A guide rule picked for a scan.</summary>
        public static GuideRuleSelection Selection()
        {
            return new GuideRuleSelection("guide-1", "rule-1");
        }

        /// <summary>A reviewer's determinations, with one finding and one manual selection.</summary>
        public static GuideDeterminationInput Determinations()
        {
            var guide = new ArtifactReference(ArtifactKind.Guide, "guide-1", "1.0.0") { Digest = "sha256:guide" };
            var rule = new RuleReference(guide, "rule-1", "sha256:rule");
            var evidence = new List<ContentEvidenceSpan>
            {
                new ContentEvidenceSpan(0, 4, "utf-8-bytes", rule) { Matched = "text" },
            };
            var finding = new GuideFindingDetermination("finding-1", "sha256:document", GuideFindingDecision.Accepted, "Matches policy.")
            {
                ProposalOrigin = DecisionProposalOrigin.AnalystAuthored,
            };
            var manual = new ManualGuideSelectionInput("sha256:document", rule, evidence, "Applies to the whole document.")
            {
                ProposalOrigin = DecisionProposalOrigin.AnalystAuthored,
            };
            return new GuideDeterminationInput(new[] { finding }, new[] { manual });
        }

        /// <summary>Caller authorization without an application id.</summary>
        public static RestClientCredentials Tokens()
        {
            return new RestClientCredentials("caller-token", "context-token");
        }

        /// <summary>Caller authorization with the id the application is registered under.</summary>
        public static RestClientCredentials TokensWithApplication()
        {
            return new RestClientCredentials("caller-token", "context-token", "case-intake-portal");
        }

        /// <summary>Cache choices for a connected client, with caching turned off.</summary>
        public static ContextCacheOptions CacheOff()
        {
            return new ContextCacheOptions { Enabled = false };
        }
    }
}
