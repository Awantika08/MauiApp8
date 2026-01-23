using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp8.Entities;
public class JournalEntry
{
    public int Id { get; set; }

    // Enforce "one entry per day" using UNIQUE index in DbContext
    public DateTime EntryDate { get; set; }  // store date normalized to 00:00:00

    public string Title { get; set; } = "";
    public string Content { get; set; } = "";

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public int PrimaryMoodId { get; set; }
    public Mood? PrimaryMood { get; set; }

    public int? SecondaryMood1Id { get; set; }
    public Mood? SecondaryMood1 { get; set; }

    public int? SecondaryMood2Id { get; set; }
    public Mood? SecondaryMood2 { get; set; }

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public int WordCount { get; set; }

    public List<EntryTag> EntryTags { get; set; } = new();
}
