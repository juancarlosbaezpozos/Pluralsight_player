using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Pluralsight.Learner.WPF;

public abstract class TableOfContentsViewModel : INotifyPropertyChanged
{
	public string Title { get; protected set; }

	public event PropertyChangedEventHandler PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
