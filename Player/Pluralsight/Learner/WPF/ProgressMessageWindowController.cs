namespace Pluralsight.Learner.WPF;

internal class ProgressMessageWindowController : MessageWindowController
{
	public ProgressMessageWindowController(string message, string title)
	{
		base.MessageContent = message;
		base.Title = title;
		base.CancelButtonText = null;
		base.AcceptButtonText = null;
	}
}
