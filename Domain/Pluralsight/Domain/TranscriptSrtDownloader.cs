using Pluralsight.Domain.Persistance;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pluralsight.Domain;

public class TranscriptSrtDownloader
{
    private readonly IDownloadFileLocator downloadLocator;
    private readonly ITranscriptRepository transcriptRepository;

    public TranscriptSrtDownloader(IDownloadFileLocator downloadLocator, ITranscriptRepository transcriptRepository)
    {
        this.downloadLocator = downloadLocator;
        this.transcriptRepository = transcriptRepository;
    }

    public Task Generate(CourseDetail course)
    {
        if (downloadLocator.DownloadLocationExists())
        {
            foreach (Module module in course.Modules)
            {
                List<TranscriptEntry> transcriptEntries = new List<TranscriptEntry>();
                foreach (Clip clip in module.Clips)
                {
                    var clipTranscripts = transcriptRepository.LoadTranscriptForClip(course.Name, module.Name, clip.Index);
                    if (clipTranscripts.Count() > 0)
                    {
                        for (int i = 0; i < clipTranscripts.Count(); i++)
                        {
                            transcriptEntries.Add(new TranscriptEntry
                            {
                                StartTime = TimeSpan.FromMilliseconds(clipTranscripts[i].StartTime),
                                EndTime = TimeSpan.FromMilliseconds(clipTranscripts[i].EndTime),
                                Text = clipTranscripts[i].Text
                            });
                        }

                        var srtContent = new StringBuilder();
                        int entryIndex = 1;
                        foreach (var entry in transcriptEntries)
                        {
                            srtContent.AppendLine(entryIndex.ToString());
                            srtContent.AppendLine($"{FormatTimeSpan(entry.StartTime)} --> {FormatTimeSpan(entry.EndTime)}");
                            srtContent.AppendLine(entry.Text);
                            srtContent.AppendLine();
                            entryIndex++;
                        }

                        var srtFileName = FixName($"{clip.Index} - {clip.Title}.srt");
                        var clipInfo = downloadLocator.GetClipFileInfo(course, module, clip);
                        var directoryLocation = clipInfo.DirectoryName;

                        File.WriteAllText(Path.Combine(directoryLocation, srtFileName), srtContent.ToString());

                        transcriptEntries.Clear();
                        srtContent.Clear();
                    }
                }
            }
        }

        return Task.CompletedTask;
    }

    private string FormatTimeSpan(TimeSpan time)
    {
        // Format TimeSpan to "HH:MM:SS,mmm"
        return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2},{time.Milliseconds:D3}";
    }

    private string FixName(string name)
    {
        var illegal = name;
        var invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

        foreach (char c in invalid)
        {
            illegal = illegal.Replace(c.ToString(), "");
        }

        return illegal;
    }
}
