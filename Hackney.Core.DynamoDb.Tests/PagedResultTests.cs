using AutoFixture;
using FluentAssertions;
using Hackney.Core.DynamoDb;
using System.Linq;
using Xunit;
namespace Hackney.Core.DynamoDb.Tests
{
    public class PagedResultTests
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void PagedResultDefaultConstructorTest()
        {
            var sut = new PagedResult<string>();
            sut.Results.Should().BeEmpty();
            sut.PaginationDetails.Should().BeEquivalentTo(new PaginationDetails());
        }

        [Fact]
        public void PagedResultConstructorNullResultsIsEmpty()
        {
            var sut = new PagedResult<string>(null, null);
            sut.Results.Should().BeEmpty();
        }

        [Fact]
        public void PagedResultConstructorEmptyResultsIsEmpty()
        {
            var sut = new PagedResult<string>(Enumerable.Empty<string>(), null);
            sut.Results.Should().BeEmpty();
        }

        [Fact]
        public void PagedResultConstructorSetResults()
        {
            var list = _fixture.CreateMany<string>(10);
            var sut = new PagedResult<string>(list);
            sut.Results.Should().BeEquivalentTo(list);
            sut.PaginationDetails.Should().BeEquivalentTo(new PaginationDetails());
        }

        [Fact]
        public void PagedResultConstructorSetPaginationToken()
        {
            var list = _fixture.CreateMany<string>(10);
            var paginationDetails = new PaginationDetails();
            var sut = new PagedResult<string>(list, paginationDetails);
            sut.Results.Should().BeEquivalentTo(list);
            sut.PaginationDetails.Should().Be(paginationDetails);
        }
    }
}
