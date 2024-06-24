using System.Windows;

namespace Pluralsight.Learner.WPF;

internal class CustomMessageBox
{
	public static MessageBoxResult Show(Window owner, string message, string title)
	{
		MessageBoxWindowController logic = new MessageBoxWindowController(message, title);
		return ShowMessageBox(owner, logic);
	}

	private static MessageBoxResult ShowMessageBox(Window owner, MessageBoxWindowController logic)
	{
		DialogWindow dialogWindow = new DialogWindow(logic);
		dialogWindow.Owner = owner;
		dialogWindow.WindowStyle = WindowStyle.SingleBorderWindow;
		dialogWindow.ResizeMode = ResizeMode.NoResize;
		dialogWindow.ShowDialog();
		return logic.Result;
	}

	public static MessageBoxResult Show(Window owner, string message, string title, string yesText, string cancelText)
	{
		MessageBoxWindowController logic = new MessageBoxWindowController(message, title, yesText, cancelText);
		return ShowMessageBox(owner, logic);
	}
}
