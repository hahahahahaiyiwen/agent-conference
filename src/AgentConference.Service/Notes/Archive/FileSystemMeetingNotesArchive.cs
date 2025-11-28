using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AgentConference.Service.Notes;

internal class FileSystemMeetingNotesArchive : IMeetingNotesArchive
{
    private readonly FileSystemMeetingNotesArchiveOptions _options;

    public FileSystemMeetingNotesArchive(IOptions<FileSystemMeetingNotesArchiveOptions> options)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public Task ArchiveAsync(MeetingNotes notes, CancellationToken cancellationToken)
    {
        if (notes == null)
        {
            throw new ArgumentNullException(nameof(notes));
        }

        string folderPath = _options.ArchiveDirectory;

        if (string.IsNullOrEmpty(folderPath))
        {
            folderPath = Path.Combine(AppContext.BaseDirectory, "MeetingNotesArchive");
        }

        Directory.CreateDirectory(folderPath);

        string filePath = Path.Combine(folderPath, $"MeetingNotes@{DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss")}.txt");

        foreach (var entry in notes.ReadAll())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var wrappedLines = Wrap(entry.Content, 120);
            var lines = new List<string>
            {
                $"[{entry.Timestamp:O}] {entry.Actor}:"
            };
            lines.AddRange(wrappedLines);
            lines.Add(string.Empty); // Blank line between entries

            File.AppendAllLines(filePath, lines);
        }

        return Task.CompletedTask;
    }

    private static IEnumerable<string> Wrap(string text, int maxWidth)
    {
        if (string.IsNullOrEmpty(text))
        {
            yield return string.Empty;
            yield break;
        }

        int index = 0;
        while (index < text.Length)
        {
            int length = Math.Min(maxWidth, text.Length - index);
            yield return text.Substring(index, length);
            index += length;
        }
    }
}