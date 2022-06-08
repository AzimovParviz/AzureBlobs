using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

record Email {
    [Key]
    public string Key { get; set; } = default!;
    public string email { get; set; } = default!;
    [Column(TypeName = "json")]
    /*
    IMPORTANT!
    whenever you plan to do a migration, change the Attributes to a public string, because MySQL doesn't like arrays or lists
    */
    /* Also comment out the MapPost method since it calls List methods and will prevent you from building since you changed it to string */
    public List<string> Attributes { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}