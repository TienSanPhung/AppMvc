﻿// <auto-generated />
using System;
using AppMvc.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AppMvc.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20240328171536_Product")]
    partial class Product
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("AppMvc.Models.Blog.PostCategory", b =>
                {
                    b.Property<int>("PostID")
                        .HasColumnType("int");

                    b.Property<int>("CategoryID")
                        .HasColumnType("int");

                    b.HasKey("PostID", "CategoryID");

                    b.HasIndex("CategoryID");

                    b.ToTable("PostCategory");
                });

            modelBuilder.Entity("AppMvc.Models.Product.CategoryProduct", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ParentCategoryId")
                        .HasColumnType("int");

                    b.Property<string>("Slug")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("ParentCategoryId");

                    b.HasIndex("Slug");

                    b.ToTable("CategoryProduct");
                });

            modelBuilder.Entity("AppMvc.Models.Product.ProductCategoryProduct", b =>
                {
                    b.Property<int>("ProductID")
                        .HasColumnType("int");

                    b.Property<int>("CategoryID")
                        .HasColumnType("int");

                    b.HasKey("ProductID", "CategoryID");

                    b.HasIndex("CategoryID");

                    b.ToTable("ProductCategoryProduct");
                });

            modelBuilder.Entity("AppMvc.Models.Product.ProductModel", b =>
                {
                    b.Property<int>("ProductID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ProductID"));

                    b.Property<string>("AuthorId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Content")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DateUpdated")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.Property<bool>("Published")
                        .HasColumnType("bit");

                    b.Property<string>("Slug")
                        .HasMaxLength(160)
                        .HasColumnType("nvarchar(160)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(160)
                        .HasColumnType("nvarchar(160)");

                    b.HasKey("ProductID");

                    b.HasIndex("AuthorId");

                    b.HasIndex("Slug")
                        .IsUnique()
                        .HasFilter("[Slug] IS NOT NULL");

                    b.ToTable("Product");
                });

            

            

            modelBuilder.Entity("AppMvc.Models.Product.CategoryProduct", b =>
                {
                    b.HasOne("AppMvc.Models.Product.CategoryProduct", "ParentCategory")
                        .WithMany("CategoryChildren")
                        .HasForeignKey("ParentCategoryId");

                    b.Navigation("ParentCategory");
                });

            modelBuilder.Entity("AppMvc.Models.Product.ProductCategoryProduct", b =>
                {
                    b.HasOne("AppMvc.Models.Product.CategoryProduct", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AppMvc.Models.Product.ProductModel", "Product")
                        .WithMany("ProductCategoryProducts")
                        .HasForeignKey("ProductID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("AppMvc.Models.Product.ProductModel", b =>
                {
                    b.HasOne("AppMvc.Models.AppUser", "Author")
                        .WithMany()
                        .HasForeignKey("AuthorId");

                    b.Navigation("Author");
                });


            modelBuilder.Entity("AppMvc.Models.Product.CategoryProduct", b =>
                {
                    b.Navigation("CategoryChildren");
                });

            modelBuilder.Entity("AppMvc.Models.Product.ProductModel", b =>
                {
                    b.Navigation("ProductCategoryProducts");
                });
#pragma warning restore 612, 618
        }
    }
}
