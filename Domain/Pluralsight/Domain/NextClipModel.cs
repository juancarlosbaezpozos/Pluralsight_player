namespace Pluralsight.Domain;

public class NextClipModel
{
	public Module NextModule { get; set; }

	public Clip NextClip { get; set; }

	public string OverlayTitle { get; set; }

	public string ContinueText { get; set; }

	public bool ShouldContinue()
	{
		return NextClip != null;
	}
}
