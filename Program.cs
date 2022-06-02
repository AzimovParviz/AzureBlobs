using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Azure.Storage.Blobs;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Design;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//https://docs.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-dotnet?tabs=environment-variable-linux#get-the-connection-string
//string connectionStringAzure = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
string connectionStringAzure = "DefaultEndpointsProtocol=https;AccountName=blobemails;AccountKey=EFwhw8VngfohdFDy4yZDmS8k7iDhFQuRbjmMoLImUb35DUw8PoxtS1RxnFIKQpe4mrZk09tlZYN6+AStoLBcRg==;EndpointSuffix=core.windows.net";
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
    bool isUnique = e.Attributes.Distinct().Count() == e.Attributes.Count();
    //checking if there every attribute is unique in the list. Assigning it to a variable so it's easier to read instead of
    //using the whole expression in the if statement
    int repeatCounter = 0;
    if(isUnique)
    {
        db.Emails.Add(e);
        await db.SaveChangesAsync();
    }
    else
    {
        for (int i = 0; i<e.Attributes.Count;i++)
        {
            foreach (var eItem in e.Attributes)
            {
                if (eItem == e.Attributes[i]) repeatCounter++;
                if (repeatCounter>1) e.Attributes.RemoveAt(i);
            }
        }
        db.Emails.Add(e);
        await db.SaveChangesAsync();
    }
    //creating filepaths
    string localPath = "./data/";
    string fileName = "quickstart" + Guid.NewGuid().ToString() + ".txt";
    string localFilePath = Path.Combine(localPath, JsonSerializer.Serialize(e));

    // Write email to the file
    await File.WriteAllTextAsync(localFilePath, fileName);

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
    public List<Primitive> Attributes { get; set; } = default!;
}

record Primitive
{
    public int PrimitiveId { get; set; }
    public double Data { get; set; }

    [Required]
    public Email EmailClass { get; set; }
}
class EmailDb: DbContext {
    public EmailDb(DbContextOptions<EmailDb> options): base(options) {

    }
    public DbSet<Email> Emails => Set<Email>();
}
//https://stackoverflow.com/questions/19720662/handling-realtime-data
//one of the probable method I could use for