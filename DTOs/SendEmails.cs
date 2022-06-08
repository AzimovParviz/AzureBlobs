using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

record SendEmail {
    [Key]
    public string email { get; set; } = default!;
    /* whenver you perform a migration change this to string instead of List<string> */
    [Column(TypeName = "json")]
    public List<string> Attributes { get; set; } = default!;
    public int attributesReceivedToday { get; set; } = 0;

    public string body { get; set; } = default!;

    public static string populateBody(List<string> Attributes)
    {
        return ("Congratulate! We have received following " + Attributes.Count + " unique attributes from you: " +  string.Join( ",", Attributes) + " Best regards, Millisecond");
    }
}