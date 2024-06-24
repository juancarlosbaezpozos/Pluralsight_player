using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Pluralsight.Domain;
using Pluralsight.Domain.Authentication;
using Pluralsight.Domain.Persistance;

namespace Pluralsight.Learner.WPF;

public partial class CourseDownloadPlayRow : UserControl, IComponentConnector
{
    public static User loggedInUser;

    private readonly CourseDownloadCommand downloadCommand;

    private CourseDetail Course => (base.DataContext as CourseTileViewModel)?.Course;

    public event Action<CourseDetail, CourseProgress> CoursePlayRequested;

    public event Action CourseQueuedForDownload;

    public CourseDownloadPlayRow()
    {
        InitializeComponent();
        downloadCommand = ObjectFactory.Get<CourseDownloadCommand>();
    }

    private async void CoursePlayButtonPressed(object sender, RoutedEventArgs e)
    {
        if (base.DataContext is CourseTileViewModel courseTileViewModel)
        {
            if (courseTileViewModel.MayView)
            {
                this.CoursePlayRequested?.Invoke(Course, (base.DataContext as CourseTileViewModel)?.CourseProgress);
                return;
            }
            await courseTileViewModel.UpdateMayDownload();
            CustomMessageBox.Show(Window.GetWindow(this), "Your current subscription does not include this course.", "Access Denied", "", "OK");
        }
    }

    private async void DownloadPressed(object sender, RoutedEventArgs e)
    {
        if (Course == null || !HasEnoughFreeSpaceOnDisk())
        {
            return;
        }
        string courseName = Course.Name;
        CourseDetailResult courseDetailResult = await Task.Run(async delegate
        {
            CourseDetailResult detailResult = await downloadCommand.GetCourseDetail(courseName);
            if (!detailResult.Success)
            {
                return detailResult;
            }
            downloadCommand.SaveForDownload(detailResult.Course);
            this.CourseQueuedForDownload?.Invoke();
            await downloadCommand.Download(detailResult.Course);
            return detailResult;
        });
        if (!courseDetailResult.Success)
        {
            CustomMessageBox.Show(Window.GetWindow(this), courseDetailResult.ErrorMessage, "Error", null, "OK");
        }
    }

    private bool HasEnoughFreeSpaceOnDisk()
    {
        DownloadManager.CanShowFirstClipDownloadFailedWindow = true;
        int num = 1073741824;
        if (ObjectFactory.Get<IDownloadFileLocator>().AvailableFreeSpaceOnDisk() < num)
        {
            MessageBoxResult num2 = CustomMessageBox.Show(Window.GetWindow(this), "You may not have enough free disk space to fully download the course.", "Low disk space", "Download anyway", "Cancel");
            ITracking tracking = ObjectFactory.Get<ITracking>();
            if (num2 != MessageBoxResult.Yes)
            {
                tracking.TrackEvent(Event.DownloadCanceledLowSpace);
                return false;
            }
            tracking.TrackEvent(Event.DownloadContinuedLowSpace);
        }
        return true;
    }

}
