using System.Windows.Input;

namespace Pluralsight.Learner.WPF;

public class PlayerCommands
{
	public static readonly RoutedUICommand PlayPauseCommand = new RoutedUICommand("Play/Pause", "PlayPause", typeof(VideoPlayer), new InputGestureCollection
	{
		new KeyGesture(Key.MediaPlayPause)
	});

	public static readonly RoutedUICommand NextClipCommand = new RoutedUICommand("Next", "NextClip", typeof(VideoPlayer), new InputGestureCollection
	{
		new KeyGesture(Key.MediaNextTrack),
		new KeyGesture(Key.OemPeriod, ModifierKeys.Shift)
	});

	public static readonly RoutedUICommand PreviousClipCommand = new RoutedUICommand("Previous", "PreviousClip", typeof(VideoPlayer), new InputGestureCollection
	{
		new KeyGesture(Key.MediaPreviousTrack),
		new KeyGesture(Key.OemComma, ModifierKeys.Shift)
	});

	public static readonly RoutedUICommand SkipForwardCommand = new RoutedUICommand("Skip forward", "SkipForward", typeof(VideoPlayer), new InputGestureCollection
	{
		new KeyGesture(Key.Right, ModifierKeys.Control)
	});

	public static readonly RoutedUICommand SkipBackwardCommand = new RoutedUICommand("Skip backward", "SkipBackward", typeof(VideoPlayer), new InputGestureCollection
	{
		new KeyGesture(Key.Left, ModifierKeys.Control)
	});

	public static readonly RoutedUICommand IncreasePlaybackSpeedCommand = new RoutedUICommand("Increase playback speed", "IncreasePlaybackSpeed", typeof(VideoPlayer), new InputGestureCollection
	{
		new KeyGesture(Key.Add),
		new KeyGesture(Key.OemPlus, ModifierKeys.Shift)
	});

	public static readonly RoutedUICommand DecreasePlaybackSpeedCommand = new RoutedUICommand("Decrease playback speed", "DecreasePlaybackSpeed", typeof(VideoPlayer), new InputGestureCollection
	{
		new KeyGesture(Key.Subtract),
		new KeyGesture(Key.OemMinus)
	});
}
