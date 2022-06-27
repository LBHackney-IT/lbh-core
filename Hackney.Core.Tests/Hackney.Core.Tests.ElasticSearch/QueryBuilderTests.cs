using Elasticsearch.Net;
using Hackney.Core.ElasticSearch;
using Hackney.Core.ElasticSearch.Interfaces;
using Moq;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Hackney.Core.Tests.ElasticSearch
{
    public class QueryBuilderTests
    {
        private readonly QueryBuilder<TestSearchObject> _sut;
        private readonly Mock<IWildCardAppenderAndPrepender> _wildcardAppenderAndPrependerMock;
        private readonly QueryContainerDescriptor<TestSearchObject> _queryContainerDesc;
        public QueryBuilderTests()
        {
            _wildcardAppenderAndPrependerMock = new Mock<IWildCardAppenderAndPrepender>();
            _queryContainerDesc = new QueryContainerDescriptor<TestSearchObject>();

            _sut = new QueryBuilder<TestSearchObject>(_wildcardAppenderAndPrependerMock.Object);
        }

        [Theory]
        [InlineData(TextQueryType.BestFields)]
        [InlineData(TextQueryType.MostFields)]
        public void WhenCreatingQuery_WithSpecifiedType_ResultantQueryShouldBeThatType(TextQueryType queryType)
        {
            // Arrange 
            string searchText = "4 Annerdale House";
            var fields = new List<string> { "field1", "field2" };
            _wildcardAppenderAndPrependerMock.Setup(x => x.Process(It.IsAny<string>()))
                .Returns(new List<string> { "*4*", "*Annerdale*", "*House*" });

            // Act
            QueryContainer query = _sut.WithWildstarQuery(searchText, fields, queryType)
                .Build(_queryContainerDesc);

            // Assert
            var container = (query as IQueryContainer).Bool.Should.First() as IQueryContainer;

            Assert.Equal(queryType, container.QueryString.Type);
        }

        [Fact]
        public void WhenCreatingQuery_WithWildstar_ResultantQueryShouldHaveOneSubquery()
        {
            // Arrange 
            string searchText = "12 Pitcairn Street";
            var fields = new List<string> { "field1", "field2" };
            _wildcardAppenderAndPrependerMock.Setup(x => x.Process(It.IsAny<string>()))
                .Returns(new List<string> { "*12*", "*Pitcairn*", "*Street*" });

            // Act
            QueryContainer query = _sut.WithWildstarQuery(searchText, fields)
                .Build(_queryContainerDesc);

            // Assert
            var container = (query as IQueryContainer).Bool.Should;

            Assert.Equal(2, container.Count());
            Assert.Equal(1, container.Count(q => q != null));
        }
    }

    public class MatchQueryVisitor : QueryVisitor
    {
        public string Field { get; set; }
        public string Value { get; set; }
        public override void Visit(IMatchQuery query)
        {
            var inferrer = new Inferrer(new ConnectionSettings(new InMemoryConnection()));
            Field = inferrer.Field(query.Field);
            Value = query.Query;

        }
    }
}
