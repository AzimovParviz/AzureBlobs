using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

/* POST request for the Note(Todo) item */
app.MapPost("/api/emails", async(Email e, EmailDb db)=> {
    db.Emails.Add(e);
    await db.SaveChangesAsync();

    return Results.Created($"/api/emails/{e.Key}", e);
});

app.UseHttpsRedirection();
app.Run();

record Email {
    [Key]
    public string Key { get; set; } = default!;
    public string email { get; set; } = default!;
    public string[] Attributes { get; set; } = default!;
}
class EmailDb: DbContext {
    public EmailDb(DbContextOptions<EmailDb> options): base(options) {

    }
    public DbSet<Email> Emails => Set<Email>();
}