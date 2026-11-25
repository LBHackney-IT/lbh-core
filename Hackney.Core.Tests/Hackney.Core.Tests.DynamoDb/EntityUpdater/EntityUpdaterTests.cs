using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using Xunit;
using Updater = Hackney.Core.DynamoDb.EntityUpdater.EntityUpdater;

namespace Hackney.Core.Tests.DynamoDb.EntityUpdater;

public class EntityUpdaterTests
{
    private readonly Updater _updater;

    public EntityUpdaterTests()
    {
        _updater = new Updater(NullLogger<Updater>.Instance);
    }

    [Fact]
    public void UpdateEntity_NullEntity_Throws()
    {
        // act
        Action act = () => _updater.UpdateEntity<DuelAcademyStudent, DuelAcademyStudentUpdate>(null, "{}", new DuelAcademyStudentUpdate());

        // assert
        act.Should().Throw<ArgumentNullException>().Where(ex => ex.ParamName == "entityToUpdate");
    }

    [Fact]
    public void UpdateEntity_NullUpdate_Throws()
    {
        // arrange
        var student = new DuelAcademyStudent();

        // act
        Action act = () => _updater.UpdateEntity(student, "{}", (DuelAcademyStudentUpdate)null);

        // assert
        act.Should().Throw<ArgumentNullException>().Where(ex => ex.ParamName == "updateObject");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("{}")]
    public void UpdateEntity_WithNoProperties_ReturnsTheEntityUnchanged(string updateJson)
    {
        // arrange
        var student = new DuelAcademyStudent { Name = "Alexis Rhodes" };
        var update = new DuelAcademyStudentUpdate { Name = "Bastion Misawa" };

        // act
        var result = _updater.UpdateEntity(student, updateJson, update);

        // assert
        result.UpdatedEntity.Should().BeSameAs(student);
        student.Name.Should().Be("Alexis Rhodes");
        result.OldValues.Should().BeEmpty();
        result.NewValues.Should().BeEmpty();
        result.IgnoredProperties.Should().BeEmpty();
    }

    [Fact]
    public void UpdateEntity_AppliesChangedProperties_AndSkipsUnchangedOnes()
    {
        // arrange
        var student = new DuelAcademyStudent { Name = "Jaden Yuki", Year = 1 };
        var update = new DuelAcademyStudentUpdate { Name = "Syrus Truesdale", Year = 1 };

        // act
        var result = _updater.UpdateEntity(student, "{\"name\":\"Syrus Truesdale\",\"year\":1}", update);

        // assert
        result.UpdatedEntity.Should().BeSameAs(student);
        student.Name.Should().Be("Syrus Truesdale");
        student.Year.Should().Be(1);
        result.OldValues["name"].Should().Be("Jaden Yuki");
        result.NewValues["name"].Should().Be("Syrus Truesdale");
        result.OldValues.ContainsKey("year").Should().BeFalse();
        result.NewValues.ContainsKey("year").Should().BeFalse();
        result.IgnoredProperties.Should().BeEmpty();
    }

    [Fact]
    public void UpdateEntity_RecordsUnchangedProperties_WhenNotIgnoringThem()
    {
        // arrange
        var student = new DuelAcademyStudent { Name = "Jaden Yuki" };
        var update = new DuelAcademyStudentUpdate { Name = "Jaden Yuki" };

        // act
        var result = _updater.UpdateEntity(student, "{\"name\":\"Jaden Yuki\"}", update, false);

        // assert
        student.Name.Should().Be("Jaden Yuki");
        // same value, but is recorded
        result.OldValues["name"].Should().Be("Jaden Yuki");
        result.NewValues["name"].Should().Be("Jaden Yuki");
    }

    [Fact]
    public void UpdateEntity_SkipsProperty_WhenBothValuesAreNull()
    {
        // arrange
        var student = new DuelAcademyStudent { Dorm = null };
        var update = new DuelAcademyStudentUpdate { Dorm = null };

        // act
        var result = _updater.UpdateEntity(student, "{\"dorm\":null}", update);

        // assert
        student.Dorm.Should().BeNull();
        result.OldValues.Should().BeEmpty();
        result.NewValues.Should().BeEmpty();
    }

    [Fact]
    public void UpdateEntity_ClearsProperty_WhenUpdateValueIsNull()
    {
        // arrange
        var student = new DuelAcademyStudent { Dorm = "Slifer Red" };
        var update = new DuelAcademyStudentUpdate { Dorm = null };

        // act
        var result = _updater.UpdateEntity(student, "{\"dorm\":null}", update);

        // assert
        student.Dorm.Should().BeNull();
        result.OldValues["dorm"].Should().Be("Slifer Red");
        result.NewValues["dorm"].Should().BeNull();
    }

    [Fact]
    public void UpdateEntity_SetsProperty_WhenExistingValueIsNull()
    {
        // arrange
        var student = new DuelAcademyStudent { Dorm = null };
        var update = new DuelAcademyStudentUpdate { Dorm = "Slifer Red" };

        // act
        var result = _updater.UpdateEntity(student, "{\"dorm\":\"Slifer Red\"}", update);

        // assert
        student.Dorm.Should().Be("Slifer Red");
        result.OldValues["dorm"].Should().BeNull();
        result.NewValues["dorm"].Should().Be("Slifer Red");
    }

    [Fact]
    public void UpdateEntity_IgnoresJsonProperty_MissingFromTheEntity()
    {
        // arrange
        var student = new DuelAcademyStudent { Name = "Jaden Yuki" };
        var update = new DuelAcademyStudentUpdate();

        // act
        var result = _updater.UpdateEntity(student, "{\"wingedKuriboh\":\"Jaden's partner\"}", update);

        // assert
        student.Name.Should().Be("Jaden Yuki");
        result.IgnoredProperties.Should().Equal("wingedKuriboh");
    }

    [Fact]
    public void UpdateEntity_IgnoresEntityProperty_MissingFromTheRequest()
    {
        // arrange
        var student = new DuelAcademyStudent
        {
            Name = "Alexis Rhodes",
            FavouriteCard = "Cyber Angel" // ignored
        };

        var nameOnlyRegistration = new NameOnlyRegistration { Name = "Alexis Rhodes" };

        // act
        var result = _updater.UpdateEntity(student, "{\"favouriteCard\":\"Elemental Hero\"}", nameOnlyRegistration);

        // assert
        student.FavouriteCard.Should().Be("Cyber Angel");
        result.IgnoredProperties.Should().Equal("favouriteCard");
    }

    [Fact]
    public void UpdateEntity_SkipsNestedObject_WhenValuesAreEqual()
    {
        // arrange
        var student = new DuelAcademyStudent { Room = new DormRoom { Bunk = "Red-1" } };
        var update = new DuelAcademyStudentUpdate { Room = new DormRoom { Bunk = "Red-1" } };

        // act
        var updateJson = "{\"room\":{}}"; // UpdateEntity reads only the JSON keys and takes each value from the update object.
        var result = _updater.UpdateEntity(student, updateJson, update);

        // assert
        result.OldValues.Should().BeEmpty();
        student.Room.Bunk.Should().Be("Red-1");
    }

    [Fact]
    public void UpdateEntity_ReplacesNestedObject_WhenValuesDiffer()
    {
        // arrange
        var student = new DuelAcademyStudent { Room = new DormRoom { Bunk = "Red-1" } };
        var update = new DuelAcademyStudentUpdate { Room = new DormRoom { Bunk = "Red-2" } };

        // act
        var updateJson = "{\"room\":{}}"; // UpdateEntity reads only the JSON keys and takes each value from the update object.
        var result = _updater.UpdateEntity(student, updateJson, update);

        // assert
        student.Room.Bunk.Should().Be("Red-2");
        result.OldValues["room"].Should().BeEquivalentTo(new DormRoom { Bunk = "Red-1" });
        result.NewValues["room"].Should().BeEquivalentTo(new DormRoom { Bunk = "Red-2" });
    }
}
