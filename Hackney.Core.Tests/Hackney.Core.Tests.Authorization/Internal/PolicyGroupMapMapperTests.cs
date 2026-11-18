using System;
using System.Text.Json;
using AwesomeAssertions;
using Hackney.Core.Authorization.Exceptions;
using Hackney.Core.Authorization.Internal;
using Xunit;

namespace Hackney.Core.Tests.Authorization.Internal;

public class PolicyGroupMapMapperTests
{
    private const string CouldNotBeDeserializedMessage = "The group document could not be deserialized.";
    private const string NotAGroupDocumentMessage = "The group document is not a policy map.";

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
    public void Map_ThrowsInvalidGroupDocumentException_WhenPolicyKeysDifferOnlyByCase()
    {
        // arrange
        const string json = """
            {
              "Housing-Officers": ["group-a"],
              "housing-officers": ["group-b"]
            }
            """;

        // act
        var act = () => PolicyGroupMapMapper.Map(json);

        // assert
        var exception = act.Should().Throw<InvalidGroupDocumentException>();

        exception.Which.Message.Should().Be(
            "Policy 'housing-officers' duplicates another policy key when compared without case.");
        exception.Which.InnerException.Should().BeNull();
    }

    [Fact]
    public void Map_ThrowsInvalidGroupDocumentException_WhenTheDocumentIsJsonNull()
    {
        // act
        var act = () => PolicyGroupMapMapper.Map("null");

        // assert
        var exception = act.Should().Throw<InvalidGroupDocumentException>();

        exception.Which.Message.Should().Be(CouldNotBeDeserializedMessage);
        exception.Which.InnerException.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("{")]
    [InlineData("[]")]
    [InlineData("\"housing-officers\"")]
    [InlineData("{ \"wildlife-api\": \"officers\" }")]
    [InlineData("{ \"wildlife-api\": 1 }")]
    public void Map_ThrowsInvalidGroupDocumentException_WhenTheDocumentIsNotAGroupMap(string json)
    {
        // act
        var act = () => PolicyGroupMapMapper.Map(json);

        // assert
        var exception = act.Should().Throw<InvalidGroupDocumentException>();

        exception.Which.Message.Should().Be(NotAGroupDocumentMessage);
        exception.Which.InnerException.Should().BeOfType<JsonException>();
    }

    [Fact]
    public void Map_ThrowsInvalidGroupDocumentException_WhenAPolicyGroupListIsNull()
    {
        // act
        var act = () => PolicyGroupMapMapper.Map("""{ "wildlife-api": null }""");

        // assert
        var exception = act.Should().Throw<InvalidGroupDocumentException>();

        exception.Which.Message.Should().Be("Policy 'wildlife-api' has no group list.");
        exception.Which.InnerException.Should().BeNull();
    }
}
