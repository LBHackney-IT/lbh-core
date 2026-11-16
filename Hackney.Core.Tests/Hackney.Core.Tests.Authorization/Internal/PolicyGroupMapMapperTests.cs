using System;
using System.Collections.Generic;
using System.Text.Json;
using AwesomeAssertions;
using Hackney.Core.Authorization.Internal;
using Xunit;

namespace Hackney.Core.Tests.Authorization.Internal;

public class PolicyGroupMapMapperTests
{
    private const string PayloadFailureMessage = "SSM Parameter payload could not be deserialized.";

    [Fact]
    public void Map_BuildsPolicyKeysIgnoringKeyCase_AndKeepingGroupsCaseExactly()
    {
        // arrange
        const string json = """
            {
              "Housing-Officers": ["group-a", "Group-A", "group-a"],
              "patches-admin-groups": ["patches-manage"]
            }
            """;

        // act
        var map = PolicyGroupMapMapper.Map(json);

        // assert
        map.Policies.Comparer.Should().Be(StringComparer.OrdinalIgnoreCase);
        map.Policies.Should().ContainKey("housing-officers");

        var groups = map.Policies["HOUSING-OFFICERS"];

        groups.Comparer.Should().Be(StringComparer.Ordinal);
        groups.Should().HaveCount(2);
        groups.Should().BeEquivalentTo("group-a", "Group-A");
        groups.Contains("GROUP-A").Should().BeFalse();

        map.Policies["patches-admin-groups"].Should().BeEquivalentTo("patches-manage");
    }

    [Fact]
    public void Map_ReturnsAnEmptyMap_WhenTheDocumentIsEmpty()
    {
        // act
        var map = PolicyGroupMapMapper.Map("{}");

        // assert
        map.Policies.Should().BeEmpty();
    }

    [Fact]
    public void Map_StoresAnEmptyGroupList()
    {
        // act
        var map = PolicyGroupMapMapper.Map("""{ "patches-admin-groups": [] }""");

        // assert
        var groups = map.Policies["patches-admin-groups"];

        groups.Should().BeEmpty();
        groups.Comparer.Should().Be(StringComparer.Ordinal);
    }

    [Fact]
    public void Map_KeepsTheGroupsFromTheLaterPolicyKey_WhenKeysDifferOnlyByCase()
    {
        // arrange
        const string json = """
            {
              "Housing-Officers": ["group-a"],
              "housing-officers": ["group-b"]
            }
            """;

        // act
        var map = PolicyGroupMapMapper.Map(json);

        // assert
        map.Policies.Should().ContainSingle();
        map.Policies["HOUSING-OFFICERS"].Should().BeEquivalentTo("group-b");
    }

    [Fact]
    public void Map_ThrowsInvalidOperationException_WhenTheDocumentIsJsonNull()
    {
        // act
        var act = () => PolicyGroupMapMapper.Map("null");

        // assert
        var exception = act.Should().Throw<InvalidOperationException>();

        exception.Which.Message.Should().Be(PayloadFailureMessage);
        exception.Which.InnerException.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("{")]
    [InlineData("[]")]
    [InlineData("\"housing-officers\"")]
    [InlineData("{ \"wildlife-api\": \"officers\" }")]
    [InlineData("{ \"wildlife-api\": 1 }")]
    public void Map_ThrowsJsonException_WhenTheDocumentIsNotAGroupMap(string json)
    {
        // act
        var act = () => PolicyGroupMapMapper.Map(json);

        // assert
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Map_ThrowsArgumentNullException_WhenAPolicyGroupListIsNull()
    {
        // act
        var act = () => PolicyGroupMapMapper.Map("""{ "wildlife-api": null }""");

        // assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("collection");
    }
}
