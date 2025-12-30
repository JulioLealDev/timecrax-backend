using System.Collections.Concurrent;

namespace Timecrax.Api.Services;

public class RateLimitService
{
    private readonly ConcurrentDictionary<string, RateLimitEntry> _entries = new();
    private readonly TimeSpan _windowDuration = TimeSpan.FromMinutes(15);
    private readonly int _maxAttempts = 5;

    public bool IsRateLimited(string key) => IsRateLimited(key, _maxAttempts, (int)_windowDuration.TotalMinutes);

    public bool IsRateLimited(string key, int maxAttempts, int windowMinutes)
    {
        CleanupExpiredEntries();

        if (_entries.TryGetValue(key, out var entry))
        {
            if (entry.ExpiresAt < DateTimeOffset.UtcNow)
            {
                _entries.TryRemove(key, out _);
                return false;
            }

            return entry.Attempts >= maxAttempts;
        }

        return false;
    }

    public void RecordAttempt(string key) => RecordAttempt(key, (int)_windowDuration.TotalMinutes);

    public void RecordAttempt(string key, int windowMinutes)
    {
        var now = DateTimeOffset.UtcNow;
        var window = TimeSpan.FromMinutes(windowMinutes);

        _entries.AddOrUpdate(
            key,
            _ => new RateLimitEntry { Attempts = 1, ExpiresAt = now.Add(window) },
            (_, existing) =>
            {
                if (existing.ExpiresAt < now)
                {
                    return new RateLimitEntry { Attempts = 1, ExpiresAt = now.Add(window) };
                }
                existing.Attempts++;
                return existing;
            });
    }

    private void CleanupExpiredEntries()
    {
        var now = DateTimeOffset.UtcNow;
        var expiredKeys = _entries.Where(e => e.Value.ExpiresAt < now).Select(e => e.Key).ToList();
        foreach (var key in expiredKeys)
        {
            _entries.TryRemove(key, out _);
        }
    }

    private class RateLimitEntry
    {
        public int Attempts { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
    }
}
