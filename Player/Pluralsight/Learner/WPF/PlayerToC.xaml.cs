using System;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Pluralsight.Learner.WPF;

public partial class PlayerToC : UserControl, IComponentConnector
{
	public event Action<ClipViewModel> ClipSelected;

	public PlayerToC()
	{
		InitializeComponent();
	}

	private void SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (Contents.SelectedItem is ClipViewModel)
		{
			ClipViewModel obj = (ClipViewModel)Contents.SelectedItem;
			this.ClipSelected?.Invoke(obj);
		}
	}

	public void ScrollToSelected()
	{
		Contents.ScrollIntoView(Contents.SelectedItem);
	}
}
