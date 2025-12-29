using System.Collections.Concurrent;

namespace Timecrax.Api.Services;

public class RateLimitService
{
    private readonly ConcurrentDictionary<string, RateLimitEntry> _entries = new();
    private readonly TimeSpan _windowDuration = TimeSpan.FromMinutes(15);
    private readonly int _maxAttempts = 5;

    public bool IsRateLimited(string key)
    {
        CleanupExpiredEntries();

        if (_entries.TryGetValue(key, out var entry))
        {
            if (entry.ExpiresAt < DateTimeOffset.UtcNow)
            {
                _entries.TryRemove(key, out _);
                return false;
            }

            return entry.Attempts >= _maxAttempts;
        }

        return false;
    }

    public void RecordAttempt(string key)
    {
        var now = DateTimeOffset.UtcNow;

        _entries.AddOrUpdate(
            key,
            _ => new RateLimitEntry { Attempts = 1, ExpiresAt = now.Add(_windowDuration) },
            (_, existing) =>
            {
                if (existing.ExpiresAt < now)
                {
                    return new RateLimitEntry { Attempts = 1, ExpiresAt = now.Add(_windowDuration) };
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
