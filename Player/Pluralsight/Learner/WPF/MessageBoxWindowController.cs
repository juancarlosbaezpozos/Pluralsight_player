using System.Windows;

namespace Pluralsight.Learner.WPF;

internal class MessageBoxWindowController : MessageWindowController
{
	public MessageBoxResult Result { get; set; }

	public MessageBoxWindowController(string message, string title, string yesText, string noText)
	{
		base.MessageContent = message;
		base.Title = title;
		base.CancelButtonText = noText;
		base.AcceptButtonText = yesText;
	}

	public MessageBoxWindowController(string message, string title)
		: this(message, title, "Yes", "No")
	{
	}

	public override void Accepted()
	{
		Result = MessageBoxResult.Yes;
	}

	public override void Canceled()
	{
		Result = MessageBoxResult.No;
	}
}
