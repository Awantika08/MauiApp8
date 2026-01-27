using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MauiApp8.Entities;
namespace MauiApp8.Data; public class DbInitializer
{
    private readonly IDbContextFactory<AppDbContext> _factory; public DbInitializer(IDbContextFactory<AppDbContext> factory) { _factory = factory; }
    public async Task InitializeAsync()
    {
        await using var db = await _factory.CreateDbContextAsync(); 
        await db.Database.EnsureCreatedAsync();
        // Seed AppSettings
        if (!await db.AppSettings.AnyAsync()) { db.AppSettings.Add(new AppSetting { Id = 1 });
            await db.SaveChangesAsync();
        }
        // Seed or Update Moods
        var existingMoods = await db.Moods.ToListAsync();
        
        var desiredMoods = new List<Mood> 
        { 
            // Positive
            new() { Name="Happy 😃", Category=MoodCategory.Positive }, 
            new() { Name="Excited 🤩", Category=MoodCategory.Positive },
            new() { Name="Relaxed 😌", Category=MoodCategory.Positive }, 
            new() { Name="Grateful 🙏", Category=MoodCategory.Positive },
            new() { Name="Confident 😎", Category=MoodCategory.Positive },
            // Neutral
            new() { Name="Calm 🌿", Category=MoodCategory.Neutral }, 
            new() { Name="Thoughtful 🤔", Category=MoodCategory.Neutral }, 
            new() { Name="Curious 🧐", Category=MoodCategory.Neutral }, 
            new() { Name="Nostalgic 🌇", Category=MoodCategory.Neutral }, 
            new() { Name="Bored 😐", Category=MoodCategory.Neutral }, 
            // Negative
            new() { Name="Sad 😢", Category=MoodCategory.Negative }, 
            new() { Name="Angry 😡", Category=MoodCategory.Negative },
            new() { Name="Stressed 😫", Category=MoodCategory.Negative }, 
            new() { Name="Lonely 🥀", Category=MoodCategory.Negative }, 
            new() { Name="Anxious 😰", Category=MoodCategory.Negative }
        };

        if (!existingMoods.Any())
        {
            db.Moods.AddRange(desiredMoods);
            await db.SaveChangesAsync();
        }
        else
        {
            // Migration: Update existing moods to have emojis if they match the base name
            // Using a simple mapping based on starts-with to avoid duplication
            bool anyUpdates = false;
            foreach (var desired in desiredMoods)
            {
                // Finding existing mood that matches the desired name or the base name without emoji
                var baseName = desired.Name.Split(' ')[0]; // "Happy"

                var existing = existingMoods.FirstOrDefault(m => m.Name.StartsWith(baseName));
                if (existing != null && existing.Name != desired.Name)
                {
                    existing.Name = desired.Name;
                    anyUpdates = true;
                }
            }
            
            if (anyUpdates)
            {
                await db.SaveChangesAsync();
            }
        } 
          // Seed prebuilt tags
          if (!await db.Tags.AnyAsync())
        { 
            string[] prebuilt =
            { "Work","Career","Studies","Family","Friends","Relationships","Health",
                "Fitness", "Personal Growth","Self-care","Hobbies","Travel","Nature","Finance","Spirituality",
                "Birthday","Holiday","Vacation","Celebration",
                "Exercise","Reading","Writing", "Cooking","Meditation",
                "Yoga","Music","Shopping","Parenting","Projects","Planning","Reflection"
            }; db.Tags.AddRange(prebuilt.Select(t => new Tag {
                Name = t, IsPrebuilt = true }));
            await db.SaveChangesAsync();
        }
    }
}

