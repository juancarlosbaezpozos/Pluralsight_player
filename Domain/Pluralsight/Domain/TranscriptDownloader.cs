using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Pluralsight.Domain.Persistance;

namespace Pluralsight.Domain;

public class TranscriptDownloader
{
	private readonly IRestHelper restHelper;

	private readonly ITranscriptRepository transcriptRepository;

	private readonly ISettingsRepository settingsRepository;

	private readonly ICourseRepository courseRepository;

	public TranscriptDownloader(IRestHelper restHelper, ITranscriptRepository transcriptRepository, ISettingsRepository settingsRepository, ICourseRepository courseRepository)
	{
		this.restHelper = restHelper;
		this.transcriptRepository = transcriptRepository;
		this.settingsRepository = settingsRepository;
		this.courseRepository = courseRepository;
	}

	public async Task<bool> TranscriptsForCourse(CourseDetail course)
	{
		if (!course.HasTranscript)
		{
			return true;
		}
		string text = settingsRepository.Load("CaptionsLanguageCode") ?? "en";
		RestResponse<CourseTranscriptDao> restResponse = await restHelper.AuthenticatedGet<CourseTranscriptDao>("/library/coursetranscripts/" + course.Name + "/" + text);
		if (restResponse.StatusCode == HttpStatusCode.OK)
		{
			return transcriptRepository.SaveTranscriptsForCourse(CourseTranscriptMapper.ToCourseTranscriptDto(restResponse.Data, course), course.UrlSlug);
		}
		return false;
	}

	public async Task DownloadCourseTranscripts(List<CourseDetail> coursesToLoadTranscripts)
	{
		List<(Task<bool>, CourseDetail)> transcriptsTasks = new List<(Task<bool>, CourseDetail)>();
		int retryCount = 0;
		int maxRetryCount = 5;
		while (coursesToLoadTranscripts.Count > 0 && retryCount++ < maxRetryCount)
		{
			await Task.Delay(TimeSpan.FromMinutes(Math.Pow(2.0, retryCount - 1) - 1.0));
			transcriptsTasks.Clear();
			foreach (CourseDetail coursesToLoadTranscript in coursesToLoadTranscripts)
			{
				transcriptsTasks.Add((TranscriptsForCourse(coursesToLoadTranscript), coursesToLoadTranscript));
			}
			await Task.WhenAll(transcriptsTasks.Select(((Task<bool>, CourseDetail) t) => t.Item1));
			coursesToLoadTranscripts.Clear();
			foreach (var (task, item) in transcriptsTasks)
			{
				if (!task.Result)
				{
					coursesToLoadTranscripts.Add(item);
				}
			}
		}
	}

	public async Task UpdateDownloadedCourseTranscripts()
	{
		await DownloadCourseTranscripts(courseRepository.LoadAllDownloaded());
	}
}
