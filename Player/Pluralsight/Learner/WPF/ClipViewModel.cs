using Pluralsight.Domain;

namespace Pluralsight.Learner.WPF;

public class ClipViewModel : TableOfContentsViewModel
{
	private bool hasBeenViewed;

	private bool isPlaying;

	public Clip Clip { get; }

	public Module Module { get; }

	public bool IsPlaying
	{
		get
		{
			return isPlaying;
		}
		set
		{
			isPlaying = value;
			OnPropertyChanged("IsPlaying");
		}
	}

	public bool FirstClip => Clip.Index == 0;

	public bool LastClip => Clip.Index == Module.Clips.Count - 1;

	public bool HasBeenViewed
	{
		get
		{
			return hasBeenViewed;
		}
		set
		{
			hasBeenViewed = value;
			OnPropertyChanged("HasBeenViewed");
		}
	}

	public ClipViewModel(Module module, Clip clip, bool hasBeenViewed)
	{
		this.hasBeenViewed = hasBeenViewed;
		Module = module;
		Clip = clip;
		base.Title = clip.Title;
	}

	public void Viewed()
	{
		HasBeenViewed = true;
	}
}
