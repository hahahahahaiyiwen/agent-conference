using System;

namespace AgentConference.Service.Notes;

/// <summary>
/// Represents a single entry in a meeting transcript.
/// </summary>
public readonly struct MeetingNoteEntry
{
    public MeetingNoteEntry(string actor, DateTimeOffset timestamp, string content)
    {
        Actor = actor ?? throw new ArgumentNullException(nameof(actor));
        Timestamp = timestamp;
        Content = content ?? throw new ArgumentNullException(nameof(content));
    }

    public string Actor { get; }

    public string Content { get; }

    public DateTimeOffset Timestamp { get; }
}
