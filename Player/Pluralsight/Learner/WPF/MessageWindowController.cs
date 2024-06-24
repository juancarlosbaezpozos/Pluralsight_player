using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Pluralsight.Learner.WPF;

public class MessageWindowController : INotifyPropertyChanged
{
	private string cancelButtonText;

	private string acceptButtonText;

	private string messageContent;

	private string title;

	public string CancelButtonText
	{
		get
		{
			return cancelButtonText;
		}
		set
		{
			cancelButtonText = value;
			OnPropertyChanged("CancelButtonText");
		}
	}

	public string AcceptButtonText
	{
		get
		{
			return acceptButtonText;
		}
		set
		{
			acceptButtonText = value;
			OnPropertyChanged("AcceptButtonText");
			OnPropertyChanged("AcceptButtonVisibility");
		}
	}

	public Visibility AcceptButtonVisibility
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(AcceptButtonText))
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}
	}

	public Visibility CancelButtonVisibility
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(CancelButtonText))
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}
	}

	public string MessageContent
	{
		get
		{
			return messageContent;
		}
		set
		{
			messageContent = value;
			OnPropertyChanged("MessageContent");
		}
	}

	public string Title
	{
		get
		{
			return title;
		}
		set
		{
			title = value;
			OnPropertyChanged("Title");
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public virtual void OnWindowShown()
	{
	}

	public virtual void Canceled()
	{
	}

	public virtual void Accepted()
	{
	}

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
