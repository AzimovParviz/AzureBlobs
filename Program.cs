using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using Azure.Storage.Blobs;
using System.ComponentModel.DataAnnotations;
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
    //creating filepaths
    string localPath = "./data/";
    string fileName = e.Key + Guid.NewGuid().ToString();
    string localFilePath = Path.Combine(localPath, fileName);
    string logFileName = e.CreatedAt.ToShortDateString().Replace("/","-") + Guid.NewGuid().ToString() + ".txt";
    /*
    finding all, if any, rows with the same email to count the attributes
    */
    List<string> totalAttributes = e.Attributes;//will hold total attributes for the letter we'll send
    //checking if there every attribute is unique in the list. If there are dupes, they'll be removed
    e.Attributes = e.Attributes.Distinct().ToList();
    /* searching for entries with the same date and email */
    var todaymail = from b in db.Emails
                   where b.CreatedAt.Equals(DateTime.Today)
                   where b.email.StartsWith(e.email)
                   select b;
    /* checking if there were any entries with given email today and if there weren't, we are logging it to the blob storage*/
    if (!todaymail.Any())
    {
        await File.WriteAllTextAsync(logFileName,e.email);
        // Get a reference to a blob for logging
        BlobClient blobClientLog = containerClient.GetBlobClient(logFileName);
        // Upload log from the local file
        await blobClientLog.UploadAsync(logFileName, true);
    }
    /* looking up whether there is a record in the table for the given Email */
    var findletters = db.SendEmails.Find(e.email);
    foreach (var item in todaymail)
    {
        totalAttributes = totalAttributes.Concat(item.Attributes).ToList();
    }
    //BUG: body column in the SendEmails table gets wiped whent there are 10 attributes and you add more
    if ((findletters is null) && (totalAttributes.Count>=10))
    {
        var findemails = from b in db.Emails
                   where b.email.StartsWith(e.email)
                   select b;
        foreach (var item in findemails)
        {
            totalAttributes = totalAttributes.Concat(item.Attributes).ToList();
        }
        totalAttributes = totalAttributes.Concat(e.Attributes).ToList();
        totalAttributes = totalAttributes.Distinct().ToList();
        /* creating a new entry for the db */
        SendEmail letter = new SendEmail() {
            email = e.email,
            Attributes = totalAttributes,
            attributesReceivedToday = totalAttributes.Count,
            body = SendEmail.populateBody(totalAttributes)
        };
        db.SendEmails.Add(letter);
        await db.SaveChangesAsync();
        await File.WriteAllTextAsync(localFilePath, SendEmail.populateBody(letter.Attributes));
        // Get a reference to a blob
        BlobClient blobClientBody = containerClient.GetBlobClient(fileName);
        // Upload data from the local file
        await blobClientBody.UploadAsync(localFilePath, true);

    }
    else if ((findletters is not null) && (totalAttributes.Count>=10))
    {
        findletters.attributesReceivedToday += e.Attributes.Count;
        findletters.Attributes = findletters.Attributes.Concat(e.Attributes).ToList();
        if (findletters.attributesReceivedToday>=10) findletters.body = SendEmail.populateBody(totalAttributes);
        else findletters.body = "";
        db.SendEmails.Update(findletters);
        await db.SaveChangesAsync();
        await File.WriteAllTextAsync(localFilePath, SendEmail.populateBody(findletters.Attributes));
    }
    else if ((findletters is not null) && (findletters.attributesReceivedToday>=10))
    {
        findletters.attributesReceivedToday += e.Attributes.Count;
        findletters.Attributes = findletters.Attributes.Concat(e.Attributes).ToList();
        findletters.body = SendEmail.populateBody(findletters.Attributes);
        db.SendEmails.Update(findletters);
        await db.SaveChangesAsync();
        await File.WriteAllTextAsync(localFilePath, SendEmail.populateBody(findletters.Attributes));
    }

    db.Emails.Add(e);
    await db.SaveChangesAsync();
    // Using StreamWriter here since just writingtofileasync won't work when writing multiple rows and using
    // WriteAllLinesAsync for the Attributes which are List<string>
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
    // Upload data from the local file
    await blobClient.UploadAsync(localFilePath, true);

    return Results.Created($"/api/emails/{e.Key}", e);
});

app.UseHttpsRedirection();
app.Run();
//https://stackoverflow.com/questions/19720662/handling-realtime-data
//one of the probable method I could use for