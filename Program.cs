using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Azure.Storage.Blobs;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore.Design;
using System.ComponentModel.DataAnnotations.Schema;
using Pomelo.EntityFrameworkCore.MySql.Json.Newtonsoft;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//https://docs.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-dotnet?tabs=environment-variable-linux#get-the-connection-string
string connectionStringAzure = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
// Create a BlobServiceClient object which will be used to create a container client
BlobServiceClient blobServiceClient = new BlobServiceClient(connectionStringAzure);

//Create a unique name for the container
string containerName = "blobemails" + Guid.NewGuid().ToString();

// Create the container and return a container client object
BlobContainerClient containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<EmailDb>(options =>
    options.UseMySql(@"Server=localhost;Port=3306;Database=emailsdb;User=root;Password=new_password", new MySqlServerVersion(new Version(8, 0, 11)),
    mySqlOptions =>
    {
        mySqlOptions.EnableRetryOnFailure(
        maxRetryCount: 10,
        maxRetryDelay: TimeSpan.FromSeconds(30),
        errorNumbersToAdd: null);
        mySqlOptions.UseNewtonsoftJson();
    }));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

/* POST request with the email object*/
app.MapPost("/api/emails", async(Email e, EmailDb db) => {
    //checking if there every attribute is unique in the list. If there are dupes, they'll be removed
    e.Attributes = e.Attributes.Distinct().ToList();
    db.Emails.Add(e);
    await db.SaveChangesAsync();
    //creating filepaths
    string localPath = "./data/";
    string fileName = e.Key + Guid.NewGuid().ToString();
    string localFilePath = Path.Combine(localPath, fileName);

    // Write the data to the file
    using (FileStream stream = new FileStream(localFilePath, FileMode.Create, FileAccess.ReadWrite))
        {
            using (StreamWriter streamWriter = new StreamWriter(stream))
            {
                await streamWriter.WriteLineAsync("Key: "+e.Key);
                await streamWriter.WriteLineAsync("E-mail: "+e.email);
                await streamWriter.WriteLineAsync("Attributes: ");
                for (int i = 0; i<e.Attributes.Count;i++)
                {
                await streamWriter.WriteLineAsync(e.Attributes[i]);
                }
            }
        }
    // Get a reference to a blob
    BlobClient blobClient = containerClient.GetBlobClient(fileName);

    Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}\n", blobClient.Uri);

    // Upload data from the local file
    await blobClient.UploadAsync(localFilePath, true);

    return Results.Created($"/api/emails/{e.Key}", e);
});

app.UseHttpsRedirection();
app.Run();

record Email {
    [Key]
    public string Key { get; set; } = default!;
    public string email { get; set; } = default!;
    [Column(TypeName = "json")]
    /*
    IMPORTANT!
    whenever you plan to do a migration, change the Attributes to a public string, because MySQL doesn't like arrays or lists
    */
    public List<string> Attributes { get; set; } = default!;
}

record SendEmail {
    public string email { get; set; } = default!;
    public List<string> Attributes { get; set; } = default!;

    public string body { get; set; } = default!;

    public static string populateBody(List<string> Attributes)
    {
        return ("Congratulate! We have received following" + Attributes.Count + "unique attributes from you: " +  Attributes + "Best regards, Millisecond");
    }
}

class EmailDb: DbContext {
    public EmailDb(DbContextOptions<EmailDb> options): base(options) {

    }
    public DbSet<Email> Emails => Set<Email>();
}
//https://stackoverflow.com/questions/19720662/handling-realtime-data
//one of the probable method I could use for