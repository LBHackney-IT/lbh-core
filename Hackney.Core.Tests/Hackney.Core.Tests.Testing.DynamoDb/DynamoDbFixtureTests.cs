using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using AwesomeAssertions;
using Hackney.Core.Testing.DynamoDb;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Hackney.Core.Tests.Testing.DynamoDb;

public class DynamoDbFixtureTests
{
    private readonly Mock<IAmazonDynamoDB> _mockDynamoDb = new();
    private readonly Mock<IDynamoDBContext> _mockContext = new();
    private readonly DynamoDbFixture _dynamoDbFixture;

    public DynamoDbFixtureTests()
    {
        _dynamoDbFixture = new DynamoDbFixture(_mockContext.Object, _mockDynamoDb.Object);
    }

    // Stubs CreateTableAsync and records the last request so tests can assert on its shape.
    private CreateTableRequestCaptor SetupCreateTableCapture()
    {
        var capturer = new CreateTableRequestCaptor();
        _mockDynamoDb
            .Setup(x => x.CreateTableAsync(It.IsAny<CreateTableRequest>(), It.IsAny<CancellationToken>()))
            .Callback<CreateTableRequest, CancellationToken>((request, _) => capturer.Request = request)
            .ReturnsAsync(new CreateTableResponse());
        return capturer;
    }

    private sealed class CreateTableRequestCaptor
    {
        public CreateTableRequest Request { get; set; }
    }

    [Fact]
    public void Constructor_GivenInjectedClientAndContext_ExposesThemOnProperties()
    {
        // arrange
        // _dynamoDbFixture is already created in the test class constructor - asserting against that

        // act
        var client = _dynamoDbFixture.DynamoDb;
        var context = _dynamoDbFixture.DynamoDbContext;

        // assert
        client.Should().BeSameAs(_mockDynamoDb.Object);
        context.Should().BeSameAs(_mockContext.Object);
    }

    [Fact]
    public void EnsureTablesExist_GivenTableDefWithPartitionKeyOnly_SendsMatchingCreateTableRequest()
    {
        // arrange
        // Consumers can define tables with only a partition key (KeyName). That must map to HASH
        // key schema plus the fixture's default provisioned throughput.
        var capturer = SetupCreateTableCapture();

        // act
        _dynamoDbFixture.EnsureTablesExist(
        [
            new TableDef
            {
                Name = "Things",
                KeyName = "id",
                KeyType = ScalarAttributeType.S
            }
        ]);

        // assert
        var captured = capturer.Request;
        captured.Should().NotBeNull();
        captured.TableName.Should().Be("Things");
        captured.KeySchema.Should().BeEquivalentTo(
        [
            new KeySchemaElement("id", KeyType.HASH)
        ]);

        captured.AttributeDefinitions.Should().BeEquivalentTo(
        [
            new AttributeDefinition("id", ScalarAttributeType.S)
        ]);

        captured.ProvisionedThroughput.ReadCapacityUnits.Should().Be(3);
        captured.ProvisionedThroughput.WriteCapacityUnits.Should().Be(3);

        _mockDynamoDb.Verify(
            x => x.CreateTableAsync(It.IsAny<CreateTableRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void EnsureTablesExist_GivenTableDefWithPartitionAndSortKeys_SendsBothKeySchemaElements()
    {
        // arrange
        // Tables with a sort key (RangeKeyName), e.g. Notes targetId + id, must emit HASH and RANGE.
        var capturer = SetupCreateTableCapture();

        // act
        _dynamoDbFixture.EnsureTablesExist(
        [
            new TableDef
            {
                Name = "Notes",
                KeyName = "targetId",
                KeyType = ScalarAttributeType.S,
                RangeKeyName = "id",
                RangeKeyType = ScalarAttributeType.S
            }
        ]);

        // assert
        var captured = capturer.Request;
        captured.KeySchema.Should().BeEquivalentTo(
        [
            new KeySchemaElement("targetId", KeyType.HASH),
            new KeySchemaElement("id", KeyType.RANGE)
        ]);

        captured.AttributeDefinitions.Should().BeEquivalentTo(
        [
            new AttributeDefinition("targetId", ScalarAttributeType.S),
            new AttributeDefinition("id", ScalarAttributeType.S)
        ]);
    }

    [Fact]
    public void EnsureTablesExist_GivenTableDefWithLocalSecondaryIndex_SendsIndexAndAddsMissingAttribute()
    {
        // arrange
        // LSI range keys are often not table keys; the fixture must add a missing attribute
        // definition (currently always as string / S) so CreateTable succeeds.
        var capturer = SetupCreateTableCapture();

        var localIndex = new LocalSecondaryIndex
        {
            IndexName = "NotesByCreated",
            KeySchema =
            [
                new KeySchemaElement("targetId", KeyType.HASH),
                new KeySchemaElement("createdAt", KeyType.RANGE)
            ],
            Projection = new Projection { ProjectionType = ProjectionType.ALL }
        };

        // act
        _dynamoDbFixture.EnsureTablesExist(
        [
            new TableDef
            {
                Name = "Notes",
                KeyName = "targetId",
                KeyType = ScalarAttributeType.S,
                RangeKeyName = "id",
                RangeKeyType = ScalarAttributeType.S,
                LocalSecondaryIndexes = { localIndex }
            }
        ]);

        // assert
        var captured = capturer.Request;
        captured.LocalSecondaryIndexes.Should().ContainSingle()
            .Which.IndexName.Should().Be("NotesByCreated");

        captured.AttributeDefinitions.Should().ContainEquivalentOf(
            new AttributeDefinition("createdAt", ScalarAttributeType.S));
    }

    [Fact]
    public void EnsureTablesExist_GivenTableDefWithGlobalSecondaryIndex_SendsIndexAndAddsMissingAttributes()
    {
        // arrange
        // GSI keys are separate from the table key; missing attribute definitions must be added
        // (currently always as string / S).
        var capturer = SetupCreateTableCapture();

        var globalIndex = new GlobalSecondaryIndex
        {
            IndexName = "ByStatus",
            KeySchema =
            [
                new KeySchemaElement("status", KeyType.HASH),
                new KeySchemaElement("createdAt", KeyType.RANGE)
            ],
            Projection = new Projection { ProjectionType = ProjectionType.ALL },
            ProvisionedThroughput = new ProvisionedThroughput(3, 3)
        };

        // act
        _dynamoDbFixture.EnsureTablesExist(
        [
            new TableDef
            {
                Name = "Things",
                KeyName = "id",
                KeyType = ScalarAttributeType.S,
                GlobalSecondaryIndexes = { globalIndex }
            }
        ]);

        // assert
        var captured = capturer.Request;
        captured.GlobalSecondaryIndexes.Should().ContainSingle()
            .Which.IndexName.Should().Be("ByStatus");

        captured.AttributeDefinitions.Should().ContainEquivalentOf(
            new AttributeDefinition("status", ScalarAttributeType.S));

        captured.AttributeDefinitions.Should().ContainEquivalentOf(
            new AttributeDefinition("createdAt", ScalarAttributeType.S));
    }

    [Fact]
    public void EnsureTablesExist_GivenMultipleTableDefs_CallsCreateTableOncePerTable()
    {
        // arrange
        SetupCreateTableCapture();

        // act
        _dynamoDbFixture.EnsureTablesExist(
        [
            new TableDef { Name = "One", KeyName = "id", KeyType = ScalarAttributeType.S },
            new TableDef { Name = "Two", KeyName = "id", KeyType = ScalarAttributeType.S }
        ]);

        // assert
        _mockDynamoDb.Verify(
            x => x.CreateTableAsync(It.IsAny<CreateTableRequest>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public void EnsureTablesExist_GivenCreateTableThrowsResourceInUse_DoesNotThrow()
    {
        // arrange
        // Test hosts call EnsureTablesExist on every run; an already-created local table must not fail setup.
        _mockDynamoDb
            .Setup(x => x.CreateTableAsync(It.IsAny<CreateTableRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ResourceInUseException("already exists"));

        // act
        Action act = () => _dynamoDbFixture.EnsureTablesExist(
        [
            new TableDef { Name = "Things", KeyName = "id", KeyType = ScalarAttributeType.S }
        ]);

        // assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureTablesExist_GivenCreateTableThrowsOtherError_PropagatesException()
    {
        // arrange
        _mockDynamoDb
            .Setup(x => x.CreateTableAsync(It.IsAny<CreateTableRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonDynamoDBException("boom"));

        // act
        Action act = () => _dynamoDbFixture.EnsureTablesExist(
        [
            new TableDef { Name = "Things", KeyName = "id", KeyType = ScalarAttributeType.S }
        ]);

        // assert
        act.Should().Throw<AmazonDynamoDBException>().WithMessage("boom");
    }

    [Fact]
    public void EnsureTablesExist_GivenEmptyTableList_DoesNotCallCreateTable()
    {
        // arrange
        // no client setup required

        // act
        _dynamoDbFixture.EnsureTablesExist([]);

        // assert
        _mockDynamoDb.Verify(
            x => x.CreateTableAsync(It.IsAny<CreateTableRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SaveEntityAsync_GivenEntity_CallsContextSaveAsync()
    {
        // arrange
        var entity = new TestEntity { Id = "1" };

        _mockContext
            .Setup(x => x.SaveAsync(entity, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // act
        await _dynamoDbFixture.SaveEntityAsync(entity);

        // assert
        _mockContext.Verify(x => x.SaveAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveEntityAsync_GivenSavedEntity_WhenDisposed_CallsContextDeleteAsync()
    {
        // arrange
        // SaveEntityAsync is meant to seed data that is removed when the fixture is disposed.
        var entity = new TestEntity { Id = "1" };

        _mockContext
            .Setup(x => x.SaveAsync(entity, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockContext
            .Setup(x => x.DeleteAsync(entity, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // act
        await _dynamoDbFixture.SaveEntityAsync(entity);

        _dynamoDbFixture.Dispose();

        // assert
        _mockContext.Verify(x => x.DeleteAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveEntityAsync_GivenDeleteThrowsConditionalCheckFailed_WhenDisposed_DoesNotThrow()
    {
        // arrange
        // Entity may already have been deleted by the test; cleanup must tolerate that.
        var entity = new TestEntity { Id = "1" };

        _mockContext
            .Setup(x => x.SaveAsync(entity, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockContext
            .Setup(x => x.DeleteAsync(entity, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConditionalCheckFailedException("already gone"));

        await _dynamoDbFixture.SaveEntityAsync(entity);

        // act
        Action act = () => _dynamoDbFixture.Dispose();

        // assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task SaveEntityAsync_GivenMultipleSavedEntities_WhenDisposed_DeletesEachEntity()
    {
        // arrange
        var first = new TestEntity { Id = "1" };
        var second = new TestEntity { Id = "2" };

        _mockContext
            .Setup(x => x.SaveAsync(It.IsAny<TestEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockContext
            .Setup(x => x.DeleteAsync(It.IsAny<TestEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // act
        await _dynamoDbFixture.SaveEntityAsync(first);
        await _dynamoDbFixture.SaveEntityAsync(second);

        _dynamoDbFixture.Dispose();

        // assert
        _mockContext.Verify(x => x.DeleteAsync(first, It.IsAny<CancellationToken>()), Times.Once);
        _mockContext.Verify(x => x.DeleteAsync(second, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Dispose_GivenAlreadyDisposed_DoesNotRunCleanupAgain()
    {
        // arrange
        var entity = new TestEntity { Id = "1" };
        _mockContext
            .Setup(x => x.SaveAsync(entity, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockContext
            .Setup(x => x.DeleteAsync(entity, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _dynamoDbFixture.SaveEntityAsync(entity);

        // act
        _dynamoDbFixture.Dispose();
        _dynamoDbFixture.Dispose();

        // assert
        _mockContext.Verify(x => x.DeleteAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Dispose_GivenPriorFixtureSavedEntity_WhenNewFixtureDisposed_DoesNotRerunPriorCleanup()
    {
        // arrange
        // Cleanup is per fixture instance; disposing another fixture must not replay earlier deletes.
        var entity = new TestEntity { Id = "shared" };
        var firstContext = new Mock<IDynamoDBContext>();
        var secondContext = new Mock<IDynamoDBContext>();

        firstContext
            .Setup(x => x.SaveAsync(entity, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        firstContext
            .Setup(x => x.DeleteAsync(entity, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var firstFixture = new DynamoDbFixture(firstContext.Object, _mockDynamoDb.Object);
        await firstFixture.SaveEntityAsync(entity);

        firstFixture.Dispose();
        firstContext.Verify(x => x.DeleteAsync(entity, It.IsAny<CancellationToken>()), Times.Once);

        var secondFixture = new DynamoDbFixture(secondContext.Object, _mockDynamoDb.Object);

        // act
        secondFixture.Dispose();

        // assert
        firstContext.Verify(x => x.DeleteAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
        secondContext.Verify(x => x.DeleteAsync(It.IsAny<TestEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DisposeAsync_GivenSavedEntity_DeletesEntity()
    {
        // arrange
        var entity = new TestEntity { Id = "1" };

        _mockContext
            .Setup(x => x.SaveAsync(entity, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockContext
            .Setup(x => x.DeleteAsync(entity, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _dynamoDbFixture.SaveEntityAsync(entity);

        // act
        await _dynamoDbFixture.DisposeAsync();

        // assert
        _mockContext.Verify(x => x.DeleteAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    private sealed class TestEntity
    {
        public string Id { get; set; }
    }
}
