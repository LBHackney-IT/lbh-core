using AutoFixture;
using AwesomeAssertions;
using Hackney.Core.DynamoDb;
using System.Linq;
using Xunit;

namespace Hackney.Core.Tests.DynamoDb;

public class PagedResultTests
{
    private readonly Fixture _fixture = new Fixture();

    [Fact]
    public void PagedResult_DefaultConstructor_HasEmptyResultsAndDefaultPagination()
    {
        // act
        var sut = new PagedResult<string>();

        // assert
        sut.Results.Should().BeEmpty();
        sut.PaginationDetails.Should().BeEquivalentTo(new PaginationDetails());
    }

    [Fact]
    public void PagedResult_NullResults_IsEmpty()
    {
        // act
        var sut = new PagedResult<string>(null);

        // assert
        sut.Results.Should().BeEmpty();
        sut.PaginationDetails.Should().BeEquivalentTo(new PaginationDetails());
    }

    [Fact]
    public void PagedResult_NullResultsAndPagination_IsEmpty()
    {
        // act
        var sut = new PagedResult<string>(null, null);

        // assert
        sut.Results.Should().BeEmpty();
    }

    [Fact]
    public void PagedResult_EmptyResults_IsEmpty()
    {
        // act
        var sut = new PagedResult<string>(Enumerable.Empty<string>(), null);

        // assert
        sut.Results.Should().BeEmpty();
    }

    [Fact]
    public void PagedResult_WithResults_SetsResults()
    {
        // arrange
        var list = _fixture.CreateMany<string>(10);

        // act
        var sut = new PagedResult<string>(list);

        // assert
        sut.Results.Should().BeEquivalentTo(list);
        sut.PaginationDetails.Should().BeEquivalentTo(new PaginationDetails());
    }

    [Fact]
    public void PagedResult_WithResultsAndPagination_SetsBoth()
    {
        // arrange
        var list = _fixture.CreateMany<string>(10);
        var paginationDetails = new PaginationDetails();

        // act
        var sut = new PagedResult<string>(list, paginationDetails);

        // assert
        sut.Results.Should().BeEquivalentTo(list);
        sut.PaginationDetails.Should().Be(paginationDetails);
    }
}
