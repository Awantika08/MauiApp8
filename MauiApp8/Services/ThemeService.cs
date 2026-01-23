using Microsoft.JSInterop;

namespace MauiApp8.Services;

public class ThemeService
{
    private readonly IJSRuntime _js;
    private bool _jsReady;

    public string Current { get; private set; } = "light"; // Default to light to match JS

    public event Action? OnChanged;

    public ThemeService(IJSRuntime js)
    {
        _js = js;
    }

    // Call this ONLY after first render
    public async Task InitAsync()
    {
        try
        {
            _jsReady = true;

            var stored = await _js.InvokeAsync<string>("theme.get");
            if (!string.IsNullOrWhiteSpace(stored))
            {
                Current = stored;
            }

            await ApplyAsync();
        }
        catch
        {
            // MAUI: JS not ready yet → ignore safely
            _jsReady = false;
        }
    }

    public async Task ToggleAsync()
    {
        Current = Current == "dark" ? "light" : "dark";
        await ApplyAsync();
    }

    public async Task SetAsync(string theme)
    {
        Current = theme;
        await ApplyAsync();
    }

    private async Task ApplyAsync()
    {
        if (!_jsReady)
            return;

        try
        {
            await _js.InvokeVoidAsync("theme.set", Current);
            OnChanged?.Invoke(); // Notify subscribers
        }
        catch
        {
            // MAUI: ignore if WebView not ready yet
        }
    }
}
