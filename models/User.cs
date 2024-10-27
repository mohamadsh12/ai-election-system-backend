using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication16.models;


public class User
{
    public string UserId { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }

    public string ImageBase64 { get; set; } // URL לתמונה
    public int AnnualVoteLimit { get; set; } // כמות הצבעות מותרת שנתית

    [Column(TypeName = "nvarchar(max)")]
    public float[] FaceEmbedding { get; set; } // שדה לשמירת הטבעות הפנים

    [NotMapped]
    public float dot {  get; set; }


}
public class UserMultiply:User
{
    public string[] ImagesBase64 { get; set; } // URL לתמונות
}

public class UserAdmin : User
{

    public bool isUserVotedOnce { get; set; }
}
