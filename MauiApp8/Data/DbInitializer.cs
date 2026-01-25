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
        // Seed moods
          if (!await db.Moods.AnyAsync()) 
       { var moods = new List<Mood> 
          { 
            // Positive
            new() { Name="Happy",
                Category=MoodCategory.Positive }, 
            new() { Name="Excited", Category=MoodCategory.Positive },
            new() { Name="Relaxed", Category=MoodCategory.Positive }, 
            new() { Name="Grateful", Category=MoodCategory.Positive },
            new() { Name="Confident", Category=MoodCategory.Positive },
            // Neutral
            new() { Name="Calm", Category=MoodCategory.Neutral }, 
            new() { Name="Thoughtful", Category=MoodCategory.Neutral }, 
            new() { Name="Curious", Category=MoodCategory.Neutral }, 
            new() { Name="Nostalgic", Category=MoodCategory.Neutral }, 
            new() { Name="Bored", Category=MoodCategory.Neutral }, 
           // Negative
           new() { Name="Sad", Category=MoodCategory.Negative }, 
            new() { Name="Angry", Category=MoodCategory.Negative },
            new() { Name="Stressed", Category=MoodCategory.Negative }, 
            new() { Name="Lonely", Category=MoodCategory.Negative }, 
            new() { Name="Anxious", Category=MoodCategory.Negative }
        }; 
             db.Moods.AddRange(moods);
            await db.SaveChangesAsync();
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

