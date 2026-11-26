namespace Hackney.Core.Tests.DynamoDb.EntityUpdater;

public class DuelAcademyStudent
{
    public string Name { get; set; }
    public int Year { get; set; }
    public string Dorm { get; set; }
    public string FavouriteCard { get; set; }
    public DormRoom Room { get; set; }
}

public class DuelAcademyStudentUpdate
{
    public string Name { get; set; }
    public int Year { get; set; }
    public string Dorm { get; set; }
    public DormRoom Room { get; set; }
}

public class NameOnlyRegistration
{
    public string Name { get; set; }
}

public class DormRoom
{
    public string Bunk { get; set; }

    public override bool Equals(object obj) => obj is DormRoom other && Bunk == other.Bunk;

    public override int GetHashCode() => Bunk?.GetHashCode() ?? 0;
}
