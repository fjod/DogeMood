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
            //Database.ExecuteSqlCommand(@"CREATE VIEW DogePictograms AS 
            //                                SELECT c.Id AS Id, c.Pictogram AS Pictogram
            //                                FROM DogeImages c");
        }

        public DbSet<DogePost> Posts { get; set; }
        public DbSet<DogeUser> DogeUsers { get; set; }
        public DbSet<DogeImage> Images { get; set; }

       // public DbQuery<Doge.Areas.Admin.Models.DogePictogram> DogePictograms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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

            //modelBuilder.Entity<DogeImage>()
            //        .HasOne(a => a.DogePost)
            //        .WithOne(b => b.DogeImage)
            //        .HasForeignKey<DogePost>(b => b.DogePostRef);

           
        }

        public DbSet<Doge.Models.UserPost> UserPost { get; set; }

        public DbSet<Doge.Areas.Admin.Controllers.LogEntry> LogEntry { get; set; }
    }
}
