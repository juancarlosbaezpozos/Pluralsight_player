using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Pluralsight.Domain;

namespace Pluralsight.Learner.WPF;

public class ModuleViewModel : TableOfContentsViewModel
{
	private readonly Module module;

	private readonly List<ClipViewModel> clipModels;

	private const string ViewedPropertyName = "HasBeenViewed";

	private bool completed;

	public int Index { get; }

	public string DurationString
	{
		get
		{
			string text = ((module.DurationInMilliseconds.Hours > 0) ? "h\\:mm\\:ss" : "m\\:ss");
			return module.DurationInMilliseconds.ToString(text);
		}
	}

	public bool Completed
	{
		get
		{
			return completed;
		}
		set
		{
			completed = value;
			OnPropertyChanged("Completed");
		}
	}

	public ModuleViewModel(Module module, int moduleIndex, List<ClipViewModel> clipViewModels)
	{
		this.module = module;
		Index = moduleIndex;
		base.Title = module.Title;
		clipModels = clipViewModels;
		foreach (ClipViewModel clipModel in clipModels)
		{
			clipModel.PropertyChanged += ClipModelPropertyChanged;
		}
		UpdateModuleViewed();
	}

	private void ClipModelPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == "HasBeenViewed")
		{
			UpdateModuleViewed();
		}
	}

	private void UpdateModuleViewed()
	{
		Completed = clipModels.Aggregate(seed: true, (bool current, ClipViewModel clip) => current & clip.HasBeenViewed);
	}
}
