using System;
using System.Collections.Generic;
using System.Linq;

namespace AgentConference.Service.Notes;

internal static class Extensions
{
    public static string ToString(IEnumerable<MeetingNoteEntry> entries)
    {
        if (entries == null || !entries.Any())
        {
            return string.Empty;
        }

        return string.Join(Environment.NewLine, entries.Select(e => $"[{e.Timestamp:O}] {e.Content}"));
    }
}