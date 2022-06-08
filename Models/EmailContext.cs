using Microsoft.EntityFrameworkCore;


class EmailDb: DbContext {
    public EmailDb(DbContextOptions<EmailDb> options): base(options) {

    }
    public DbSet<Email> Emails => Set<Email>();
    public DbSet<SendEmail> SendEmails => Set<SendEmail>();
    // so the CreatedAt in the Email record has the timestamp at the time of posting the request
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Email>()
            .Property(b => b.CreatedAt)
            .HasDefaultValueSql("CURRENT_DATE");
    }

}