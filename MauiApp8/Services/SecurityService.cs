using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MauiApp8.Data;
using Microsoft.Maui.Storage;
using MauiApp8.Entities;


namespace MauiApp8.Services;

public class SecurityService
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    private bool _unlocked;

    public bool IsUnlocked => _unlocked;

    public SecurityService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    // ✅ Safe: auto-creates AppSettings row if missing
    public async Task<bool> HasPinAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();

        var settings = await db.AppSettings.FirstOrDefaultAsync(s => s.Id == 1);
        if (settings == null)
        {
            settings = new AppSetting { Id = 1 };
            db.AppSettings.Add(settings);
            await db.SaveChangesAsync();
            return false;
        }

        return !string.IsNullOrWhiteSpace(settings.PinHash);
    }

    // ✅ Safe: auto-creates AppSettings row if missing
    public async Task SetPinAsync(string pin)
    {
        await using var db = await _factory.CreateDbContextAsync();

        var settings = await db.AppSettings.FirstOrDefaultAsync(s => s.Id == 1);
        if (settings == null)
        {
            settings = new AppSetting { Id = 1 };
            db.AppSettings.Add(settings);
        }

        settings.PinHash = BCrypt.Net.BCrypt.HashPassword(pin);
        await db.SaveChangesAsync();

        _unlocked = false; // lock again after setting/changing PIN
    }

    // ✅ Safe: handles missing row + empty PIN
    public async Task<bool> VerifyPinAsync(string pin)
    {
        await using var db = await _factory.CreateDbContextAsync();

        var settings = await db.AppSettings.FirstOrDefaultAsync(s => s.Id == 1);
        if (settings == null || string.IsNullOrWhiteSpace(settings.PinHash))
            return true; // no PIN set => unlocked

        return BCrypt.Net.BCrypt.Verify(pin, settings.PinHash);
    }

    public void MarkUnlocked() => _unlocked = true;
    public void Lock() => _unlocked = false;

    // 🔁 Compatibility wrappers (so your pages don't break)
    public Task SavePinAsync(string pin) => SetPinAsync(pin);
    public Task<bool> ValidatePinAsync(string pin) => VerifyPinAsync(pin);
}