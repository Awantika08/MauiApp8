using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MauiApp8.Entities;


namespace MauiApp8.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<Mood> Moods => Set<Mood>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<EntryTag> EntryTags => Set<EntryTag>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // One entry per day
        modelBuilder.Entity<JournalEntry>()
            .HasIndex(e => e.EntryDate)
            .IsUnique();

        // Many-to-many
        modelBuilder.Entity<EntryTag>()
            .HasKey(et => new { et.JournalEntryId, et.TagId });

        modelBuilder.Entity<EntryTag>()
            .HasOne(et => et.JournalEntry)
            .WithMany(e => e.EntryTags)
            .HasForeignKey(et => et.JournalEntryId);

        modelBuilder.Entity<EntryTag>()
            .HasOne(et => et.Tag)
            .WithMany(t => t.EntryTags)
            .HasForeignKey(et => et.TagId);

        base.OnModelCreating(modelBuilder);
    }
}
