﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AzureTest.Migrations
{
    [DbContext(typeof(EmailDb))]
    [Migration("20220602124055_primitivemigration")]
    partial class primitivemigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Email", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("email")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Key");

                    b.ToTable("Emails");
                });

            modelBuilder.Entity("Primitive", b =>
                {
                    b.Property<int>("PrimitiveId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<double>("Data")
                        .HasColumnType("double");

                    b.Property<string>("EmailClassKey")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("PrimitiveId");

                    b.HasIndex("EmailClassKey");

                    b.ToTable("Primitive");
                });

            modelBuilder.Entity("Primitive", b =>
                {
                    b.HasOne("Email", "EmailClass")
                        .WithMany("Attributes")
                        .HasForeignKey("EmailClassKey")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EmailClass");
                });

            modelBuilder.Entity("Email", b =>
                {
                    b.Navigation("Attributes");
                });
#pragma warning restore 612, 618
        }
    }
}
