using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using BoxedAppSDK;
using Microsoft.Win32.SafeHandles;
using Pluralsight.Domain;
using Pluralsight.Domain.Persistance;
using Pluralsight.Domain.WPF.Persistance;
using Pluralsight.Learner.WPF.ViewModels;

namespace Pluralsight.Learner.WPF;

public partial class VideoPlayer : UserControl, IComponentConnector
{
    public static readonly RoutedEvent SessionCompletedEvent;

    private VideoViewModel videoViewModel;

    private bool _isPlaying;

    private bool isVideoLoaded;

    private readonly double millisecondsTooCloseToZero = 150.0;

    private DispatcherTimer updatePositionTimer = new DispatcherTimer();

    private DispatcherTimer updatePositionTextTimer = new DispatcherTimer();

    private bool playbackRateDragging;

    private bool isScrubbing;

    private DispatcherTimer autoHideControlsTimer = new DispatcherTimer();

    private Storyboard fadeOutStoryboard;

    private Point lastPosition;

    private CircleAdorner progressBarThumb;

    private AdornerLayer thumbAdornerLayer;

    private bool progressBarThumbShowing;

    private ICourseProgressRepository courseProgressRepository;

    private bool hasFadedOut;

    private string nameOfPseudoFile;

    private VirtualFileStream playingFileStream;

    private ITracking tracking;

    private bool volumeDragging;

    private ClipTranscript[] transcripts;

    private bool captionsShowing;

    private ISettingsRepository settingsRepository;

    private bool isFullScreen;

    public CourseDetail Course => videoViewModel?.Course;

    private Module Module => videoViewModel?.PlayingModule;

    private Clip PlayingClip => videoViewModel?.PlayingClip;

    private bool isPlaying
    {
        get
        {
            return _isPlaying;
        }
        set
        {
            _isPlaying = value;
            if (_isPlaying)
            {
                updatePositionTimer.Start();
                SetCaptionText();
            }
            else
            {
                updatePositionTimer.Stop();
            }
        }
    }

    public bool HasInternetConnection { get; set; }

    public event RoutedEventHandler SessionCompleted
    {
        add
        {
            AddHandler(SessionCompletedEvent, value);
        }
        remove
        {
            RemoveHandler(SessionCompletedEvent, value);
        }
    }

    public event Action AutoplaySettingsRequested;

    public VideoPlayer()
    {
        settingsRepository = ObjectFactory.Get<ISettingsRepository>();
        InitializeComponent();
        VideoProgressBar.LayoutUpdated += delegate
        {
            Track track = (Track)VideoProgressBar.Template.FindName("PART_Track", VideoProgressBar);
            if (track != null && progressBarThumb == null)
            {
                Thumb thumb = track.Thumb;
                progressBarThumb = new CircleAdorner(thumb);
                progressBarThumb.MouseLeave += HideThumb;
                thumbAdornerLayer = AdornerLayer.GetAdornerLayer(thumb);
            }
        };
        PlayerWindow.ScrubbingEnabled = true;
        PlayerWindow.MediaOpened += MediaOpenedEvent;
        updatePositionTimer.Interval = TimeSpan.FromMilliseconds(100.0);
        updatePositionTimer.Tick += TickTockEvent;
        updatePositionTextTimer.Interval = TimeSpan.FromMilliseconds(200.0);
        updatePositionTextTimer.Tick += delegate
        {
            UpdatePositionAndCaptions();
        };
        isPlaying = false;
        autoHideControlsTimer.Interval = TimeSpan.FromMilliseconds(3000.0);
        autoHideControlsTimer.Tick += HideControlsEvent;
        fadeOutStoryboard = base.Resources["FadeOutStoryboard"] as Storyboard;
        courseProgressRepository = ObjectFactory.Get<ICourseProgressRepository>();
        tracking = ObjectFactory.Get<ITracking>();
        InitializeAutoplayTracking();
    }

    private void InitializeAutoplayTracking()
    {
        AutoplaySetting autoplaySetting = settingsRepository.LoadEnum("Autoplay", AutoplaySetting.Course);
        tracking.SetCustomAspect(CustomAspect.Autoplay, autoplaySetting.ToString());
    }

    private void LoadSource(string encryptedSourceFilename)
    {
        CloseVideo();
        nameOfPseudoFile = encryptedSourceFilename + ".mp4";
        playingFileStream = new VirtualFileStream(encryptedSourceFilename);
        using (new SafeFileHandle(NativeMethods.BoxedAppSDK_CreateVirtualFileBasedOnIStream(nameOfPseudoFile, NativeMethods.EFileAccess.GenericWrite, NativeMethods.EFileShare.Read, IntPtr.Zero, NativeMethods.ECreationDisposition.New, NativeMethods.EFileAttributes.Normal, IntPtr.Zero, playingFileStream), ownsHandle: true))
        {
        }
        PlayerWindow.Source = new Uri(nameOfPseudoFile, UriKind.Absolute);
        updatePositionTextTimer.Start();
        double volume = settingsRepository.LoadDouble("PlaybackVolume", 1.0);
        SetVolume(volume);
        VolumeSlider.Value = PlayerWindow.Volume;
        bool tocOpen = settingsRepository.LoadBool("TocOpen");
        SetTocOpen(tocOpen);
        captionsShowing = settingsRepository.LoadBool("CloseCaption");
        UpdateCaptionsIcon();
        double speed = settingsRepository.LoadDouble("PlaybackSpeed", 1.0);
        UpdatePlaybackSpeed(speed);
    }

    private void TickTockEvent(object sender, EventArgs e)
    {
        if (isScrubbing || PlayerWindow == null || !PlayerWindow.NaturalDuration.HasTimeSpan)
        {
            return;
        }
        try
        {
            double value = PlayerWindow.Position.TotalMilliseconds / PlayerWindow.NaturalDuration.TimeSpan.TotalMilliseconds;
            VideoProgressBar.Value = value;
        }
        catch
        {
        }
    }

    private void MediaOpenedEvent(object sender, EventArgs e)
    {
        isVideoLoaded = true;
        try
        {
            ClipLength.Text = string.Format("{0}:{1}", PlayerWindow.NaturalDuration.TimeSpan.Minutes.ToString("00"), PlayerWindow.NaturalDuration.TimeSpan.Seconds.ToString("00"));
        }
        catch
        {
        }
    }

    private async void TogglePlayPause(object sender, RoutedEventArgs e)
    {
        if (isPlaying)
        {
            autoHideControlsTimer.Stop();
            PlayerWindow.Pause();
            isPlaying = false;
            PlayPauseImage.Source = Application.Current.Resources["IconPlayerPlay"] as ImageSource;
            FadeAnOverlayNamed("OverlayPause");
            tracking.TrackEvent(Event.ClipPaused);
        }
        else
        {
            FadeAnOverlayNamed("OverlayPlay");
            PlayPauseImage.Source = Application.Current.Resources["IconPlayerPause"] as ImageSource;
            PlayPauseButton.IsEnabled = false;
            await Task.Delay(470);
            PlayPauseButton.IsEnabled = true;
            PlayPauseButton.Focus();
            isPlaying = true;
            PlayerWindow.Play();
            autoHideControlsTimer.Start();
            tracking.TrackEvent(Event.ClipUnpaused);
        }
    }

    private void FadeAnOverlayNamed(string sourceName)
    {
        Storyboard obj = base.Resources["FadeOverlayStoryboard"] as Storyboard;
        obj?.Stop();
        OverlayImage.Source = Application.Current.Resources[sourceName] as ImageSource;
        obj?.Begin();
    }

    public void WindowSetToFullScreen()
    {
        FullScreenButtonImage.Source = Application.Current.Resources["IconPlayerFullScreenExit"] as ImageSource;
        isFullScreen = true;
        PlayPauseButton.Focus();
    }

    public void WindowSetToWindowed()
    {
        FullScreenButtonImage.Source = Application.Current.Resources["IconPlayerFullScreenEnter"] as ImageSource;
        isFullScreen = false;
        PlayPauseButton.Focus();
    }

    private void VolumeClicked(object sender, RoutedEventArgs e)
    {
        VolumePopup.IsOpen = true;
        tracking?.TrackEvent(Event.VolumeSettingOpen);
        VolumeSlider.Focus();
    }

    private void VolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        SetVolume(e.NewValue);
        if (!volumeDragging)
        {
            NewVolumeSelected();
        }
    }

    private void NewVolumeSelected()
    {
        if (base.IsInitialized)
        {
            settingsRepository.Save("PlaybackVolume", PlayerWindow.Volume);
        }
        tracking?.TrackEvent(Event.VolumeSettingSelected);
    }

    private void SetVolume(double volume)
    {
        PlayerWindow.Volume = volume;
        if (volume < 0.01)
        {
            VolumeIcon.Source = Application.Current.Resources["IconPlayerVolumeMuted"] as ImageSource;
        }
        else if (volume < 0.3)
        {
            VolumeIcon.Source = Application.Current.Resources["IconPlayerVolumeLow"] as ImageSource;
        }
        else if (volume < 0.6)
        {
            VolumeIcon.Source = Application.Current.Resources["IconPlayerVolumeMedium"] as ImageSource;
        }
        else
        {
            VolumeIcon.Source = Application.Current.Resources["IconPlayerVolumeHigh"] as ImageSource;
        }
    }

    private void PlaybackRateChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        PlaybackSpeedLabel.Text = e.NewValue + "x";
        if (!playbackRateDragging)
        {
            UpdatePlaybackRate(e.NewValue);
        }
    }

    private void UpdatePlaybackRate(double newRate)
    {
        PlayerWindow.SpeedRatio = newRate;
        tracking?.TrackEvent(Event.SpeedPlaybackSelected);
        if (base.IsInitialized)
        {
            settingsRepository.Save("PlaybackSpeed", PlayerWindow.SpeedRatio);
        }
    }

    private void PlaybackSpeedClicked(object sender, RoutedEventArgs e)
    {
        PlaybackSpeedPopup.IsOpen = true;
        tracking?.TrackEvent(Event.SpeedPlaybackOpen);
        PlaybackSpeedSlider.Focus();
    }

    private void DisableRateChange(object sender, DragStartedEventArgs e)
    {
        playbackRateDragging = true;
    }

    private void RateChangeCompleted(object sender, DragCompletedEventArgs e)
    {
        playbackRateDragging = false;
        UpdatePlaybackRate(PlaybackSpeedSlider.Value);
    }

    private void ScrubStarted(object sender, DragStartedEventArgs e)
    {
        PlayerWindow.Pause();
        isScrubbing = true;
    }

    private void ScrubFinished(object sender, DragCompletedEventArgs e)
    {
        SetPlayerPositionToProgressBar();
        isScrubbing = false;
        if (isPlaying)
        {
            PlayerWindow.Play();
        }
        PlayPauseButton.Focus();
    }

    private void SetPlayerPositionToProgressBar()
    {
        if (!PlayerWindow.NaturalDuration.HasTimeSpan)
        {
            return;
        }
        try
        {
            DismissStopOverlay();
            double num = VideoProgressBar.Value * PlayerWindow.NaturalDuration.TimeSpan.TotalMilliseconds;
            if (num < millisecondsTooCloseToZero)
            {
                num = millisecondsTooCloseToZero;
            }
            PlayerWindow.Position = TimeSpan.FromMilliseconds(num);
            SetCaptionText();
        }
        catch
        {
        }
    }

    private void ShowControls(object sender, MouseEventArgs e)
    {
        Point position = e.GetPosition(PlayerWindow);
        if (position != lastPosition || IsButtonPressed(e))
        {
            if (hasFadedOut)
            {
                fadeOutStoryboard.Stop(this);
            }
            ResetAutoHideControlsTimer();
            Mouse.OverrideCursor = Cursors.Arrow;
            lastPosition = position;
        }
    }

    private void ResetAutoHideControlsTimer()
    {
        if (isPlaying)
        {
            autoHideControlsTimer.Stop();
            autoHideControlsTimer.Start();
        }
    }

    private bool IsButtonPressed(MouseEventArgs mouseEventArgs)
    {
        if (mouseEventArgs.LeftButton != MouseButtonState.Pressed && mouseEventArgs.RightButton != MouseButtonState.Pressed)
        {
            return mouseEventArgs.MiddleButton == MouseButtonState.Pressed;
        }
        return true;
    }

    private void HideControlsEvent(object sender, EventArgs e)
    {
        if (!progressBarThumbShowing)
        {
            autoHideControlsTimer.Stop();
            PlaybackSpeedPopup.IsOpen = false;
            VolumePopup.IsOpen = false;
            fadeOutStoryboard.Begin(this, isControllable: true);
            hasFadedOut = true;
            if (isFullScreen)
            {
                Mouse.OverrideCursor = Cursors.None;
            }
            lastPosition = Mouse.GetPosition(PlayerWindow);
        }
    }

    private void SkipForward10(object sender, RoutedEventArgs e)
    {
        if (isVideoLoaded && PlayerWindow.NaturalDuration.HasTimeSpan)
        {
            ResetAutoHideControlsTimer();
            TimeSpan timeSpan = PlayerWindow.Position.Add(TimeSpan.FromSeconds(10.0));
            try
            {
                TimeSpan position = ((timeSpan > PlayerWindow.NaturalDuration.TimeSpan) ? PlayerWindow.NaturalDuration.TimeSpan : timeSpan);
                PlayerWindow.Position = position;
            }
            catch
            {
            }
            TickTockEvent(sender, e);
            FadeAnOverlayNamed("OverlaySkipForward");
            tracking.TrackEvent(Event.ClipSkipForward);
        }
    }

    private void SkipBackwards10(object sender, RoutedEventArgs e)
    {
        if (isVideoLoaded && PlayerWindow.NaturalDuration.HasTimeSpan)
        {
            DismissStopOverlay();
            ResetAutoHideControlsTimer();
            TimeSpan timeSpan = PlayerWindow.Position.Add(TimeSpan.FromSeconds(-10.0));
            TimeSpan position = ((timeSpan < TimeSpan.FromMilliseconds(millisecondsTooCloseToZero)) ? TimeSpan.FromMilliseconds(millisecondsTooCloseToZero) : timeSpan);
            PlayerWindow.Position = position;
            TickTockEvent(sender, e);
            FadeAnOverlayNamed("OverlaySkipBackward");
            tracking.TrackEvent(Event.ClipSkipBack);
        }
    }

    private void DisplayThumb(object sender, MouseEventArgs e)
    {
        if (!progressBarThumbShowing && thumbAdornerLayer != null)
        {
            thumbAdornerLayer.Add(progressBarThumb);
            progressBarThumbShowing = true;
            VideoProgressBar.Height = 7.0;
        }
    }

    public void HideThumb(object sender, MouseEventArgs e)
    {
        if (!progressBarThumb.IsMouseOver && !VideoProgressBar.IsMouseOver)
        {
            thumbAdornerLayer.Remove(progressBarThumb);
            progressBarThumbShowing = false;
            VideoProgressBar.Height = 5.0;
        }
    }

    private void UpdatePositionAndCaptions()
    {
        if (PlayerWindow != null && PlayerWindow.NaturalDuration.HasTimeSpan)
        {
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(PlayerWindow.Position.TotalMilliseconds);
            CurrentPosition.Text = $"{timeSpan:mm\\:ss}";
        }
        SetCaptionText();
    }

    public void StopPlayingAndCloseThePlayerWindow()
    {
        RaiseSessionOver(null, null);
    }

    private void RaiseSessionOver(object sender, RoutedEventArgs e)
    {
        OnSessionCompleted();
        tracking.TrackEvent(Event.ReturnHome);
    }

    private void OnSessionCompleted()
    {
        if (isPlaying)
        {
            TogglePlayPause(null, null);
        }
        Mouse.OverrideCursor = Cursors.Arrow;
        CloseVideo();
        updatePositionTextTimer.Stop();
        //RaiseEvent(new RoutedEventArgs(SessionCompleted));
        RaiseEvent(new RoutedEventArgs(SessionCompletedEvent));
    }

    private void CloseVideo()
    {
        PlayerWindow.Stop();
        PlayerWindow.Close();
        PlayerWindow.Source = null;
        playingFileStream?.Dispose();
        NativeMethods.BoxedAppSDK_DeleteFileFromVirtualFileSystem(nameOfPseudoFile);
    }

    public void LaunchCourse(CourseDetail course, Module lastWatchedModule, Clip lastWatchedClip)
    {
        videoViewModel = new VideoViewModel(course, LoadCourseProgress(course));
        videoViewModel.PropertyChanged += ViewModelPropertyChanged;
        TableOfContents.DataContext = videoViewModel;
        base.DataContext = videoViewModel;
        videoViewModel.PlayClipAndModule(lastWatchedModule, lastWatchedClip);
        if (!isPlaying)
        {
            TogglePlayPause(null, null);
        }
    }

    private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "PlayingClip")
        {
            PlayFromBeginning(videoViewModel.PlayingModule, videoViewModel.PlayingClip);
        }
    }

    private void PlayFromBeginning(Module module, Clip clip)
    {
        DismissStopOverlay();
        CourseTitle.Text = Course.Title;
        ClipTitle.Text = clip.Title;
        FileInfo clipFileInfo = new DownloadFileLocator().GetClipFileInfo(Course, module, clip);
        LoadSource(clipFileInfo.FullName);
        if (!isPlaying)
        {
            TogglePlayPause(null, null);
        }
        else
        {
            PlayerWindow.Play();
        }
        ITranscriptRepository transcriptRepository = ObjectFactory.Get<ITranscriptRepository>();
        transcripts = transcriptRepository.LoadTranscriptForClip(Course.Name, module.Name, clip.Index);
        SetCaptionText();
        new ClipViewRepository(ObjectFactory.Get<DatabaseConnectionManager>()).Save(new ClipView
        {
            CourseName = Course.Name,
            AuthorHandle = module.AuthorHandle,
            ModuleName = module.Name,
            ClipIndex = clip.Index,
            StartViewTime = DateTimeOffset.UtcNow
        });
        Dictionary<string, object> dictionary = new Dictionary<string, object>
        {
            { "Origin", "Player" },
            { "Course ID", Course.Name },
            { "Course Title", Course.Title },
            { "Module Title", module.Title },
            { "Clip Title", clip.Title },
            { "Is Downloaded", true },
            {
                "Captions Enabled",
                captionsShowing && Course.HasTranscript
            },
            { "Playback Speed", PlayerWindow.SpeedRatio },
            {
                "Network Type",
                HasInternetConnection ? "Wi-Fi" : "Offline"
            }
        };
        if (captionsShowing && Course.HasTranscript)
        {
            dictionary["Captions Language"] = "en";
        }
        tracking.TrackEvent(Event.ClipStarted, dictionary);
        UpdateLocalProgress(module, clip);
    }

    private void UpdateLocalProgress(Module module, Clip clip)
    {
        CourseProgress courseProgress = LoadCourseProgress(Course);
        ModuleProgress moduleProgress = courseProgress.ViewedModules.FirstOrDefault((ModuleProgress x) => x.ModuleName == Module.Name);
        if (moduleProgress == null)
        {
            moduleProgress = new ModuleProgress
            {
                ModuleName = module.Name,
                ModuleAuthorHandle = module.AuthorHandle,
                ViewedClipIndexes = new List<int> { clip.Index }
            };
            courseProgress.ViewedModules.Add(moduleProgress);
        }
        else if (!moduleProgress.ViewedClipIndexes.Contains(clip.Index))
        {
            moduleProgress.ViewedClipIndexes.Add(clip.Index);
        }
        courseProgress.LastViewed = new LastViewedClipInfo
        {
            ClipModuleIndex = clip.Index,
            ModuleAuthorHandle = module.AuthorHandle,
            ModuleName = module.Name,
            ViewTime = DateTimeOffset.UtcNow
        };
        courseProgressRepository.Save(courseProgress);
    }

    private CourseProgress LoadCourseProgress(CourseDetail course)
    {
        CourseProgress courseProgress = courseProgressRepository.Load(course.Name);
        if (courseProgress == null)
        {
            courseProgress = new CourseProgress
            {
                CourseName = course.Name,
                ViewedModules = new List<ModuleProgress>()
            };
        }
        return courseProgress;
    }

    private void NextClip(object sender, RoutedEventArgs e)
    {
        DismissStopOverlay();
        videoViewModel.NextClip();
    }

    private void PreviousClip(object sender, RoutedEventArgs e)
    {
        DismissStopOverlay();
        videoViewModel.PreviousClip();
    }

    private void ToggleToCVisibility(object sender, RoutedEventArgs e)
    {
        bool flag;
        if (TableOfContents.Visibility == Visibility.Collapsed)
        {
            tracking.TrackEvent(Event.PlayerTocOpened);
            flag = true;
        }
        else
        {
            tracking.TrackEvent(Event.PlayerTocClosed);
            flag = false;
        }
        SetTocOpen(flag);
        settingsRepository.Save("TocOpen", flag);
    }

    private void SetTocOpen(bool isOpen)
    {
        if (isOpen)
        {
            TableOfContents.ScrollToSelected();
            TableOfContents.Visibility = Visibility.Visible;
            ToCToggleImage.Source = Application.Current.Resources["IconPlayerHideToC"] as DrawingImage;
        }
        else
        {
            TableOfContents.Visibility = Visibility.Collapsed;
            ToCToggleImage.Source = Application.Current.Resources["IconPlayerShowToC"] as DrawingImage;
        }
    }

    private void DecreasePlaybackSpeed(object sender, ExecutedRoutedEventArgs e)
    {
        AlterPlaybackSpeedBy(-1);
    }

    private void IncreasePlaybackSpeed(object sender, ExecutedRoutedEventArgs e)
    {
        AlterPlaybackSpeedBy(1);
    }

    private void AlterPlaybackSpeedBy(int delta)
    {
        int num = (int)(PlayerWindow.SpeedRatio * 10.0) + delta;
        if (num >= 5 && num <= 20)
        {
            double speed = (double)num / 10.0;
            UpdatePlaybackSpeed(speed);
        }
    }

    private void UpdatePlaybackSpeed(double speed)
    {
        PlayerWindow.SpeedRatio = speed;
        PlaybackSpeedLabel.Text = speed + "x";
        PlaybackSpeedSlider.Value = speed;
    }

    private void CanMoveNextClip(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = Course?.Modules?.Last() != Module || Module?.Clips?.Last() != PlayingClip;
    }

    private void CanMovePreviousClip(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = Course?.Modules?.First() != Module || Module?.Clips?.First() != PlayingClip;
    }

    private void ResetFocus(object sender, EventArgs e)
    {
        PlayPauseButton.Focus();
    }

    private void VolumeDragStarted(object sender, DragStartedEventArgs e)
    {
        volumeDragging = true;
    }

    private void VolumeDragCompleted(object sender, DragCompletedEventArgs e)
    {
        volumeDragging = false;
        NewVolumeSelected();
    }

    private void ClickToPoint(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        double num = Math.Abs(e.NewValue - e.OldValue);
        if (isScrubbing || num > 0.005)
        {
            SetPlayerPositionToProgressBar();
        }
    }

    private void ToggleCaptions(object sender, RoutedEventArgs e)
    {
        captionsShowing = !captionsShowing;
        UpdateCaptionsIcon();
        settingsRepository.Save("CloseCaption", captionsShowing);
    }

    private void UpdateCaptionsIcon()
    {
        if (!captionsShowing)
        {
            tracking.SetCustomAspect(CustomAspect.CloseCaption, "CC_Off");
            CaptionsIcon.Source = Application.Current.Resources["IconCaptionsOff"] as DrawingImage;
            Captions.Visibility = Visibility.Collapsed;
        }
        else
        {
            tracking.SetCustomAspect(CustomAspect.CloseCaption, "CC_On");
            CaptionsIcon.Source = Application.Current.Resources["IconCaptionsOn"] as DrawingImage;
            Captions.Text = "";
            Captions.Visibility = Visibility.Visible;
            SetCaptionText();
        }
    }

    private void SetCaptionText()
    {
        if (captionsShowing)
        {
            int currentTime = (int)PlayerWindow.Position.TotalMilliseconds;
            ClipTranscript clipTranscript = transcripts?.FirstOrDefault((ClipTranscript x) => currentTime >= x.StartTime && currentTime <= x.EndTime);
            if (clipTranscript == null)
            {
                Captions.Text = "";
                Captions.Visibility = Visibility.Collapsed;
            }
            else
            {
                Captions.Text = clipTranscript.Text;
                Captions.Visibility = Visibility.Visible;
            }
        }
    }

    private void ScaleCaptionFont(object sender, SizeChangedEventArgs e)
    {
        if (e.HeightChanged)
        {
            double fontSize = Math.Max(e.NewSize.Height / 25.0, 14.0);
            Captions.FontSize = fontSize;
        }
    }

    private void CaptureSpacebar(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            TogglePlayPause(sender, null);
            e.Handled = true;
        }
    }

    private void ClipCompleted(object sender, RoutedEventArgs e)
    {
        AutoplaySetting autoplay = settingsRepository.LoadEnum("Autoplay", AutoplaySetting.Course);
        videoViewModel.ExecuteClipCompletedStrategy(autoplay);
    }

    private void DismissStopOverlay()
    {
        videoViewModel.HideOverlay();
    }

    private void ContinueClicked(object sender, RoutedEventArgs e)
    {
        if (videoViewModel.IsLastClip())
        {
            RaiseSessionOver(sender, e);
        }
        else
        {
            NextClip(sender, e);
        }
    }

    private void AutoplayClicked(object sender, RoutedEventArgs e)
    {
        tracking.TrackEvent(Event.EditAutoplaySettings);
        this.AutoplaySettingsRequested?.Invoke();
    }

    static VideoPlayer()
    {
        SessionCompletedEvent = EventManager.RegisterRoutedEvent("SessionCompleted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VideoPlayer));
    }
}
