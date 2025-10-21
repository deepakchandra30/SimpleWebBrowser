using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using SimpleWebBrowser.Models;

namespace SimpleWebBrowser
{
    public class BrowserDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }
        public DbSet<HistoryEntry> History { get; set; }

        private static string DbPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "browser.db");

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={DbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired();
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // Configure Bookmark
            modelBuilder.Entity<Bookmark>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Url).IsRequired();
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Bookmarks)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure HistoryEntry
            modelBuilder.Entity<HistoryEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Url).IsRequired();
                entity.Property(e => e.VisitedAt).IsRequired();
                entity.HasOne(e => e.User)
                      .WithMany(u => u.History)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}