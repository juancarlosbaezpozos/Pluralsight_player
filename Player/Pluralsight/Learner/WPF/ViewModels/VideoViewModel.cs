using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Pluralsight.Domain;
using Pluralsight.Domain.Persistance;

namespace Pluralsight.Learner.WPF.ViewModels;

public class VideoViewModel : INotifyPropertyChanged
{
	private TableOfContentsViewModel selectedClip;

	private Clip playingClip;

	private bool courseHasCaptions;

	private string overlayTitle;

	private string continueText;

	private Visibility overlayVisibility;

	public CourseDetail Course { get; private set; }

	public Module PlayingModule { get; private set; }

	public Clip PlayingClip
	{
		get
		{
			return playingClip;
		}
		private set
		{
			playingClip = value;
			OnPropertyChanged("PlayingClip");
			if (playingClip != null)
			{
				if (selectedClip != null)
				{
					(selectedClip as ClipViewModel).IsPlaying = false;
				}
				selectedClip = TableOfContents.First((TableOfContentsViewModel x) => x is ClipViewModel clipViewModel && clipViewModel.Module.Equals(PlayingModule) && clipViewModel.Clip.Equals(PlayingClip));
				(selectedClip as ClipViewModel)?.Viewed();
				(selectedClip as ClipViewModel).IsPlaying = true;
				OnPropertyChanged("SelectedClip");
				OnPropertyChanged("OverlayPreferenceLinkVisibility");
			}
		}
	}

	public List<TableOfContentsViewModel> TableOfContents { get; private set; }

	public string OverlayTitle
	{
		get
		{
			return overlayTitle;
		}
		private set
		{
			overlayTitle = value;
			OnPropertyChanged("OverlayTitle");
		}
	}

	public string ContinueText
	{
		get
		{
			return continueText;
		}
		private set
		{
			continueText = value;
			OnPropertyChanged("ContinueText");
		}
	}

	public Visibility OverlayVisibility
	{
		get
		{
			return overlayVisibility;
		}
		private set
		{
			overlayVisibility = value;
			OnPropertyChanged("OverlayVisibility");
		}
	}

	public Visibility OverlayPreferenceLinkVisibility
	{
		get
		{
			if (!IsLastClip())
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}
	}

	public TableOfContentsViewModel SelectedClip
	{
		get
		{
			return selectedClip;
		}
		set
		{
			if (value is ClipViewModel clipViewModel)
			{
				PlayingModule = clipViewModel.Module;
				PlayingClip = clipViewModel.Clip;
			}
		}
	}

	public bool HasCaptions => courseHasCaptions;

	public event PropertyChangedEventHandler PropertyChanged;

	public bool IsLastClip()
	{
		return PlayingClip == Course.Modules.Last().Clips.Last();
	}

	public void HideOverlay()
	{
		OverlayVisibility = Visibility.Collapsed;
	}

	public VideoViewModel(CourseDetail course, CourseProgress courseProgress)
	{
		Course = course;
		CheckForTranscripts();
		HideOverlay();
		TableOfContents = new List<TableOfContentsViewModel>();
		int num = 1;
		foreach (Module module in course.Modules)
		{
			ModuleProgress moduleProgress = courseProgress.ViewedModules.FirstOrDefault((ModuleProgress x) => x.ModuleName == module.Name);
			List<ClipViewModel> list = module.Clips.Select((Clip x) => new ClipViewModel(module, x, moduleProgress != null && moduleProgress.ViewedClipIndexes.Contains(x.Index))).ToList();
			ModuleViewModel item = new ModuleViewModel(module, num++, list);
			TableOfContents.Add(item);
			TableOfContents.AddRange(list);
		}
	}

	private void CheckForTranscripts()
	{
		ITranscriptRepository transcriptRepository = ObjectFactory.Get<ITranscriptRepository>();
		courseHasCaptions = transcriptRepository.TranscriptsAreSaved(Course.Name);
		OnPropertyChanged("HasCaptions");
	}

	public void PlayClipAndModule(Module module, Clip clip)
	{
		PlayingModule = module;
		PlayingClip = clip;
	}

	public void NextClip()
	{
		int num = PlayingModule.Clips.IndexOf(PlayingClip);
		if (num + 1 < PlayingModule.Clips.Count)
		{
			PlayingClip = PlayingModule.Clips[num + 1];
			return;
		}
		int num2 = Course.Modules.IndexOf(PlayingModule);
		if (num2 + 1 < Course.Modules.Count)
		{
			PlayingModule = Course.Modules[num2 + 1];
			PlayingClip = PlayingModule.Clips.First();
		}
	}

	public void PreviousClip()
	{
		int num = PlayingModule.Clips.IndexOf(PlayingClip);
		if (num - 1 >= 0)
		{
			PlayingClip = PlayingModule.Clips[num - 1];
			return;
		}
		int num2 = Course.Modules.IndexOf(PlayingModule);
		if (num2 - 1 >= 0)
		{
			PlayingModule = Course.Modules[num2 - 1];
			PlayingClip = PlayingModule.Clips.Last();
		}
	}

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public void ExecuteClipCompletedStrategy(AutoplaySetting autoplay)
	{
		NextClipModel nextClipModel = AutoplayStrategyFactory.GetStrategy(autoplay).ClipFinished(Course, PlayingModule, PlayingClip);
		if (nextClipModel.ShouldContinue())
		{
			PlayingModule = nextClipModel.NextModule;
			PlayingClip = nextClipModel.NextClip;
		}
		else
		{
			OverlayTitle = nextClipModel.OverlayTitle;
			ContinueText = nextClipModel.ContinueText;
			OverlayVisibility = Visibility.Visible;
		}
	}
}
