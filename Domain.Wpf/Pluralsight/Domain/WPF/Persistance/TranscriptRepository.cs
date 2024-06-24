using System.Collections.Generic;
using System;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using Pluralsight.Domain.Persistance;
using System.IO;

namespace Pluralsight.Domain.WPF.Persistance;

public class TranscriptRepository : ITranscriptRepository
{
    private readonly DatabaseConnectionManager connectionManager;

    public TranscriptRepository(DatabaseConnectionManager connectionManager)
    {
        this.connectionManager = connectionManager;
    }

    public bool SaveTranscriptsForCourse(CourseTranscriptDto courseTranscript, string courseSlug)
    {
        bool result = false;
        string courseName = courseTranscript.CourseName;
        List<TranscriptEntry> allTranscriptEntries = new();

        foreach (ModuleTranscriptDto module in courseTranscript.Modules)
        {
            string moduleName = module.ModuleName;
            foreach (ClipTranscriptDto clipTranscript in module.ClipTranscripts)
            {
                int moduleIndexPosition = clipTranscript.ModuleIndexPosition;
                int clipId = FindDatabaseClipId(courseName, moduleName, moduleIndexPosition);
                using DbConnectionTs dbConnectionTs = connectionManager.Open();
                using SQLiteTransaction sQLiteTransaction = dbConnectionTs.BeginTransaction();
                dbConnectionTs.Execute("DELETE FROM ClipTranscript WHERE ClipId=@clipId", new { clipId });
                if (clipTranscript.Transcripts.Any())
                {
                    result = true;
                    foreach (TranscriptDto transcript in clipTranscript.Transcripts)
                    {
                        dbConnectionTs.Execute("INSERT INTO ClipTranscript (StartTime, EndTime, Text, ClipId) VALUES ( @Start, @End, @Text, @ClipId)", new
                        {
                            Start = transcript.RelativeStartTime,
                            End = transcript.RelativeEndTime,
                            Text = transcript.Text,
                            ClipId = clipId
                        });

                        //still on development
                        //allTranscriptEntries.Add(new TranscriptEntry
                        //{
                        //    StartTime = TimeSpan.FromMilliseconds(transcript.RelativeStartTime),
                        //    EndTime = TimeSpan.FromMilliseconds(transcript.RelativeEndTime),
                        //    Text = transcript.Text,
                        //    ClipId = clipId
                        //});
                    }
                }
                sQLiteTransaction.Commit();
            }
        }

        if (result)
        {
            GenerateSrtFiles(allTranscriptEntries);
        }

        return result;
    }

    private int FindDatabaseClipId(string courseName, string moduleName, int clipIndex)
    {
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        return dbConnectionTs.QuerySingle<int>("SELECT Clip.Id FROM Clip INNER JOIN Module ON Module.Id = Clip.ModuleId WHERE Module.CourseName = @courseName AND Module.Name = @moduleName AND Clip.ClipIndex = @clipIndex", new { courseName, moduleName, clipIndex });
    }

    public ClipTranscript[] LoadTranscriptForClip(string courseName, string moduleName, int clipIndex)
    {
        int clipId = FindDatabaseClipId(courseName, moduleName, clipIndex);
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        string sql = "SELECT StartTime, EndTime, Text FROM ClipTranscript WHERE ClipId=@clipId";
        return dbConnectionTs.Query<ClipTranscript>(sql, new { clipId }).ToArray();
    }

    public bool TranscriptsAreSaved(string courseName)
    {
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        return dbConnectionTs.QuerySingle<int>("SELECT COUNT(*) FROM ClipTranscript  INNER JOIN Clip on Clip.Id = ClipTranscript.ClipId INNER JOIN Module ON Module.Id = Clip.ModuleId WHERE Module.CourseName = @courseName", new { courseName }) > 0;
    }

    private void GetClipTitleById(int clipId, out string fullTitle)
    {
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        var clip = dbConnectionTs.QuerySingle<ClipDto>("SELECT * FROM Clip WHERE Clip.Id = @clipId", new { clipId });
        fullTitle = $"{clip.ClipIndex}-{clip.Title}";
    }

    public void GenerateSrtFiles(List<TranscriptEntry> transcriptEntries)
    {
        var groupedEntries = transcriptEntries.GroupBy(entry => entry.ClipId);

        foreach (var group in groupedEntries)
        {
            string srtName = string.Empty;

            var srtContent = new StringBuilder();
            int entryIndex = 1;
            foreach (var entry in group)
            {
                GetClipTitleById(entry.ClipId, out srtName);

                srtContent.AppendLine(entryIndex.ToString());
                srtContent.AppendLine($"{FormatTimeSpan(entry.StartTime)} --> {FormatTimeSpan(entry.EndTime)}");
                srtContent.AppendLine(entry.Text);
                srtContent.AppendLine(); // Empty line to separate entries
                entryIndex++;
            }

            if (!string.IsNullOrWhiteSpace(srtName))
            {
                string srtFileName = $"{srtName}.srt";
                File.WriteAllText(srtFileName, srtContent.ToString());
            }
        }
    }

    private string FormatTimeSpan(TimeSpan time)
    {
        // Format TimeSpan to "HH:MM:SS,mmm"
        return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2},{time.Milliseconds:D3}";
    }

}
