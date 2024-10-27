namespace WebApplication16.models;


public class Vote
{
    public int VoteId { get; set; }  // זהו המפתח הראשי

    public string myUserId { get; set; }
    public string? Party { get; set; } // שם המפלגה
    public DateTime VoteDate { get; set; } // תאריך ההצבעה
}
//dotnet ef migrations add <MigrationName>
//dotnet ef database update
