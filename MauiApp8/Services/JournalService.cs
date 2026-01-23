using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MauiApp8.Common;
using MauiApp8.Data;
using MauiApp8.Entities;

namespace MauiApp8.Services;

public class JournalService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public JournalService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    private static DateTime NormalizeDate(DateTime date) => date.Date;

    private static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        var parts = text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length;
    }

    public async Task<List<Mood>> GetMoodsAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Moods.OrderBy(m => m.Category).ThenBy(m => m.Name).ToListAsync();
    }

    public async Task<List<Tag>> GetAllTagsAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Tags.OrderByDescending(t => t.IsPrebuilt).ThenBy(t => t.Name).ToListAsync();
    }

    public async Task<ServiceResult<JournalEntry>> GetEntryByDateAsync(DateTime date)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var d = NormalizeDate(date);

        var entry = await db.JournalEntries
            .Include(e => e.PrimaryMood)
            .Include(e => e.SecondaryMood1)
            .Include(e => e.SecondaryMood2)
            .Include(e => e.EntryTags).ThenInclude(et => et.Tag)
            .FirstOrDefaultAsync(e => e.EntryDate == d);

        return entry == null
            ? ServiceResult<JournalEntry>.Fail("No entry found for this date.")
            : ServiceResult<JournalEntry>.Ok(entry);
    }

    public async Task<ServiceResult<JournalEntry>> UpsertTodayAsync(
        string title,
        string content,
        int primaryMoodId,
        int? secondary1Id,
        int? secondary2Id,
        List<string> tagNames)
    {
        try
        {
            await using var db = await _factory.CreateDbContextAsync();

            var today = NormalizeDate(DateTime.Now);
            var now = DateTime.Now;

            var existing = await db.JournalEntries
                .Include(e => e.EntryTags)
                .FirstOrDefaultAsync(e => e.EntryDate == today);

            // ensure tags exist
            var tags = new List<Tag>();
            foreach (var raw in tagNames.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var name = raw.Trim();
                if (string.IsNullOrWhiteSpace(name)) continue;

                var tag = await db.Tags.FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());
                if (tag == null)
                {
                    tag = new Tag { Name = name, IsPrebuilt = false };
                    db.Tags.Add(tag);
                    await db.SaveChangesAsync();
                }
                tags.Add(tag);
            }

            if (existing == null)
            {
                var entry = new JournalEntry
                {
                    EntryDate = today,
                    Title = title ?? "",
                    Content = content ?? "",
                    PrimaryMoodId = primaryMoodId,
                    SecondaryMood1Id = secondary1Id,
                    SecondaryMood2Id = secondary2Id,
                    CreatedAt = now,
                    UpdatedAt = now,
                    WordCount = CountWords(content)
                };

                foreach (var t in tags)
                    entry.EntryTags.Add(new EntryTag { TagId = t.Id });

                db.JournalEntries.Add(entry);
                await db.SaveChangesAsync();

                return ServiceResult<JournalEntry>.Ok(entry);
            }
            else
            {
                existing.Title = title ?? "";
                existing.Content = content ?? "";
                existing.PrimaryMoodId = primaryMoodId;
                existing.SecondaryMood1Id = secondary1Id;
                existing.SecondaryMood2Id = secondary2Id;
                existing.UpdatedAt = now;
                existing.WordCount = CountWords(content);

                // replace tags
                existing.EntryTags.Clear();
                foreach (var t in tags)
                    existing.EntryTags.Add(new EntryTag { TagId = t.Id });

                await db.SaveChangesAsync();
                return ServiceResult<JournalEntry>.Ok(existing);
            }
        }
        catch (Exception ex)
        {
            return ServiceResult<JournalEntry>.Fail(ex.Message);
        }
    }

    public async Task<ServiceResult<bool>> DeleteEntryByDateAsync(DateTime date)
    {
        try
        {
            await using var db = await _factory.CreateDbContextAsync();
            var d = NormalizeDate(date);

            var entry = await db.JournalEntries.FirstOrDefaultAsync(e => e.EntryDate == d);
            if (entry == null) return ServiceResult<bool>.Fail("Entry not found.");

            db.JournalEntries.Remove(entry);
            await db.SaveChangesAsync();

            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Fail(ex.Message);
        }
    }

    public async Task<List<JournalEntry>> GetPagedAsync(int page, int pageSize)
    {
        await using var db = await _factory.CreateDbContextAsync();

        return await db.JournalEntries
            .Include(e => e.PrimaryMood)
            .OrderByDescending(e => e.EntryDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<JournalEntry>> SearchAsync(string? query)
    {
        await using var db = await _factory.CreateDbContextAsync();
        query ??= "";

        return await db.JournalEntries
            .Include(e => e.PrimaryMood)
            .Where(e => e.Title.Contains(query) || e.Content.Contains(query))
            .OrderByDescending(e => e.EntryDate)
            .ToListAsync();
    }

    public async Task<(int currentStreak, int longestStreak, List<DateTime> missedDays)> GetStreakStatsAsync(int lookbackDays = 60)
    {
        await using var db = await _factory.CreateDbContextAsync();

        var from = DateTime.Today.AddDays(-lookbackDays).Date;
        var to = DateTime.Today.Date;

        var dates = await db.JournalEntries
            .Where(e => e.EntryDate >= from && e.EntryDate <= to)
            .Select(e => e.EntryDate)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();

        // missed days
        var dateSet = dates.Select(d => d.Date).ToHashSet();
        var missed = new List<DateTime>();
        for (var d = from; d <= to; d = d.AddDays(1))
            if (!dateSet.Contains(d.Date)) missed.Add(d.Date);

        // current streak (ending today)
        int current = 0;
        for (var d = to; d >= from; d = d.AddDays(-1))
        {
            if (dateSet.Contains(d.Date)) current++;
            else break;
        }

        // longest streak
        int longest = 0, run = 0;
        for (var d = from; d <= to; d = d.AddDays(1))
        {
            if (dateSet.Contains(d.Date)) { run++; longest = Math.Max(longest, run); }
            else run = 0;
        }

        return (current, longest, missed);
    }

    public async Task<List<JournalEntry>> GetEntriesInRangeAsync(DateTime start, DateTime end)
    {
        await using var db = await _factory.CreateDbContextAsync();

        var s = start.Date;
        var e = end.Date;

        return await db.JournalEntries
            .Include(x => x.PrimaryMood)
            .Include(x => x.EntryTags).ThenInclude(et => et.Tag)
            .Where(x => x.EntryDate >= s && x.EntryDate <= e)
            .OrderByDescending(x => x.EntryDate)
            .ToListAsync();
    }

    public async Task<List<JournalEntry>> FilterAsync(
        DateTime? startDate,
        DateTime? endDate,
        int? moodId,
        string? tagText)
    {
        await using var db = await _factory.CreateDbContextAsync();

        var q = db.JournalEntries
            .Include(x => x.PrimaryMood)
            .Include(x => x.EntryTags).ThenInclude(et => et.Tag)
            .AsQueryable();

        if (startDate.HasValue) q = q.Where(x => x.EntryDate >= startDate.Value.Date);
        if (endDate.HasValue) q = q.Where(x => x.EntryDate <= endDate.Value.Date);

        if (moodId.HasValue && moodId.Value > 0)
            q = q.Where(x => x.PrimaryMoodId == moodId.Value);

        if (!string.IsNullOrWhiteSpace(tagText))
        {
            var t = tagText.Trim().ToLower();
            q = q.Where(x => x.EntryTags.Any(et => et.Tag!.Name.ToLower().Contains(t)));
        }

        return await q.OrderByDescending(x => x.EntryDate).ToListAsync();
    }

    public async Task<(Dictionary<string, int> moodCategoryCounts, string mostFrequentMood, Dictionary<string, int> tagCounts, List<(string label, int value)> wordTrend)>
        GetAnalyticsAsync(DateTime start, DateTime end)
    {
        await using var db = await _factory.CreateDbContextAsync();

        var s = start.Date;
        var e = end.Date;

        var entries = await db.JournalEntries
            .Include(x => x.PrimaryMood)
            .Include(x => x.EntryTags).ThenInclude(et => et.Tag)
            .Where(x => x.EntryDate >= s && x.EntryDate <= e)
            .OrderBy(x => x.EntryDate)
            .ToListAsync();

        // Mood distribution by category
        var moodCat = new Dictionary<string, int>
    {
        { "Positive", 0 }, { "Neutral", 0 }, { "Negative", 0 }
    };

        foreach (var en in entries)
        {
            var cat = en.PrimaryMood?.Category.ToString() ?? "Neutral";
            if (!moodCat.ContainsKey(cat)) moodCat[cat] = 0;
            moodCat[cat]++;
        }

        // Most frequent mood
        var mostMood = entries
            .Where(x => x.PrimaryMood != null)
            .GroupBy(x => x.PrimaryMood!.Name)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault() ?? "-";

        // Tag counts
        var tagCounts = entries
            .SelectMany(e2 => e2.EntryTags.Select(t => t.Tag!.Name))
            .GroupBy(n => n)
            .OrderByDescending(g => g.Count())
            .ToDictionary(g => g.Key, g => g.Count());

        // Word count trend: daily values
        var wordTrend = entries
            .GroupBy(x => x.EntryDate.Date)
            .OrderBy(g => g.Key)
            .Select(g => (label: g.Key.ToString("yyyy-MM-dd"), value: (int)Math.Round(g.Average(x => x.WordCount))))
            .ToList();

        return (moodCat, mostMood, tagCounts, wordTrend);
    }

}


