﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AzureTest.Migrations
{
    [DbContext(typeof(EmailDb))]
    partial class EmailDbModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Email", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Attributes")
                        .IsRequired()
                        .HasColumnType("json");

                    b.Property<string>("email")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Key");

                    b.ToTable("Emails");
                });

            modelBuilder.Entity("SendEmail", b =>
                {
                    b.Property<string>("email")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Attributes")
                        .IsRequired()
                        .HasColumnType("json");

                    b.Property<int>("attributesReceivedToday")
                        .HasColumnType("int");

                    b.Property<string>("body")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("email");

                    b.ToTable("SendEmails");
                });
#pragma warning restore 612, 618
        }
    }
}
