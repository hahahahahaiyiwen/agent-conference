using System;
using System.Collections.Generic;
using System.IO;

namespace AgentConference.Service.Notes;

/// <summary>
/// Thread-safe append-only meeting notes with snapshot reads.
/// </summary>
public sealed class MeetingNotes
{
    private readonly object _sync = new();
    private readonly List<MeetingNoteEntry> _entries = new();
    private readonly Func<DateTimeOffset> _clock;

    public MeetingNotes()
        : this(() => DateTimeOffset.UtcNow)
    {
    }

    internal MeetingNotes(Func<DateTimeOffset> clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public IReadOnlyList<MeetingNoteEntry> ReadAll(DateTimeOffset? since = null)
    {
        lock (_sync)
        {
            if (_entries.Count == 0)
            {
                return Array.Empty<MeetingNoteEntry>();
            }

            if (since is null)
            {
                return _entries.ToArray();
            }

            var cutoff = since.Value;

            var snapshot = new List<MeetingNoteEntry>();

            foreach (var entry in _entries)
            {
                if (entry.Timestamp >= cutoff)
                {
                    snapshot.Add(entry);
                }
            }

            return snapshot;
        }
    }

    public MeetingNoteEntry Append(string identifier, string content, DateTimeOffset? timestamp = null)
    {
        if (string.IsNullOrEmpty(content))
        {
            throw new ArgumentNullException(nameof(content));
        }

        var entry = new MeetingNoteEntry(
            identifier,
            timestamp ?? _clock(),
            content);

        lock (_sync)
        {
            _entries.Add(entry);
        }

        return entry;
    }
}