using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Pluralsight.Learner.WPF;

public class CircleAdorner : Adorner
{
	public CircleAdorner(UIElement adornedElement)
		: base(adornedElement)
	{
	}

	protected override void OnRender(DrawingContext drawingContext)
	{
		Rect rect = new Rect(base.AdornedElement.DesiredSize);
		SolidColorBrush white = Brushes.White;
		Point center = new Point(rect.Left + rect.Width / 2.0, rect.Top + rect.Height / 2.0);
		Pen pen = new Pen(Application.Current.Resources["midGreyBrush"] as SolidColorBrush, 0.25);
		drawingContext.DrawEllipse(white, pen, center, 10.0, 10.0);
	}

	protected override void OnMouseDown(MouseButtonEventArgs e)
	{
		base.AdornedElement.RaiseEvent(e);
	}

	protected override void OnMouseUp(MouseButtonEventArgs e)
	{
		base.AdornedElement.RaiseEvent(e);
	}
}
