using System.Text.Json.Nodes;
using Xunit;

namespace EzClassify.Tests;

public class JsonSubsetTests
{
    private static readonly JsonNode Spans = JsonNode.Parse("""[{"inner":"A","start":0},{"inner":"B","start":9}]""")!;

    [Fact]
    public void AnObjectExpectationReadsAnArrayByIndexAndLength()
    {
        Assert.True(JsonSubset.Matches(Spans, JsonNode.Parse("""{"1":{"inner":"B"},"length":2}""")));
        Assert.False(JsonSubset.Matches(Spans, JsonNode.Parse("""{"length":1}""")));
        Assert.False(JsonSubset.Matches(JsonNode.Parse("[]"), JsonNode.Parse("""{"0":{"inner":"A"}}""")));
        Assert.False(JsonSubset.Matches(Spans, JsonNode.Parse("""{"01":{"inner":"B"}}""")));
    }
}
