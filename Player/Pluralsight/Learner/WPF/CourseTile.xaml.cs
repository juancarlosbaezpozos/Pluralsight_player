using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;
using Pluralsight.Domain;

namespace Pluralsight.Learner.WPF;

public partial class CourseTile : UserControl, IComponentConnector
{
	public event Action DownloadStarted;

	public event Action<CourseDetail, CourseProgress> CoursePlayRequested;

	public CourseTile()
	{
		InitializeComponent();
	}

	private void CourseQueuedForDownload()
	{
		this.DownloadStarted?.Invoke();
	}

	private void CourseStartingToPlay(CourseDetail course, CourseProgress progress)
	{
		this.CoursePlayRequested?.Invoke(course, progress);
	}

}
