using System;
using System.Collections.Generic;
using System.Text;
using Doge.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Doge.Areas.Admin.Controllers;

namespace Doge.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }   
        
        // public DbSet<DogeImage> Images { get; set; } //for migration

        public DbSet<DogePost> Posts { get; set; }
        public DbSet<DogeUser> DogeUsers { get; set; }

    

        public DbSet<DogeBigImage> BigImages { get; set; }
        public DbSet<DogeSmallImage> SmallImages { get; set; }
        public DbSet<UserPost> UserPost { get; set; }
        public DbSet<LogEntry> LogEntry { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {           
            //many-to-many configuration
            modelBuilder.Entity<UserPost>()
               .HasKey(_userPost => new { _userPost.UserId, _userPost.PostId });

            modelBuilder.Entity<UserPost>()
                .HasOne(_userPost => _userPost.DogePost)
                .WithMany(post => post.Users)
                .HasForeignKey(_userPost => _userPost.PostId);

            modelBuilder.Entity<UserPost>()
                .HasOne(_userPost => _userPost.DogeUser)
                .WithMany(c => c.FavoritePosts)
                .HasForeignKey(_userPost => _userPost.UserId);

            //table splitting configuration
            modelBuilder.Entity<DogeBigImage>().
                HasOne(p => p.DogeSmallImage).
                WithOne(p => p.DogeBigImage).
                HasForeignKey<DogeBigImage>(p => p.Id);
            modelBuilder.Entity<DogeSmallImage>().
                HasOne(p => p.DogeBigImage).WithOne(p => p.DogeSmallImage).
                HasForeignKey<DogeSmallImage>(p => p.Id);

            modelBuilder.Entity<DogeBigImage>().ToTable("Images");
            modelBuilder.Entity<DogeSmallImage>().ToTable("Images");

            base.OnModelCreating(modelBuilder);
        }

    }
}
