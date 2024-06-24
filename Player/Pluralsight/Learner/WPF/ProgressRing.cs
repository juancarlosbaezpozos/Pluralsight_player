using System.Windows;
using System.Windows.Controls;

namespace Pluralsight.Learner.WPF;

[TemplateVisualState(Name = "Inactive", GroupName = "ActiveStates")]
[TemplateVisualState(Name = "Active", GroupName = "ActiveStates")]
public class ProgressRing : Control
{
	public static readonly DependencyProperty EllipseDiameterTemplateSettingProperty = DependencyProperty.Register("EllipseDiameterTemplateSetting", typeof(double), typeof(ProgressRing), new PropertyMetadata(0.0));

	public static readonly DependencyProperty EllipseOffsetTemplateSettingProperty = DependencyProperty.Register("EllipseOffsetTemplateSetting", typeof(Thickness), typeof(ProgressRing), new PropertyMetadata(default(Thickness)));

	public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive", typeof(bool), typeof(ProgressRing), new PropertyMetadata(false, IsActiveChanged));

	public static readonly DependencyProperty MaxSideLengthTemplateSettingProperty = DependencyProperty.Register("MaxSideLengthTemplateSetting", typeof(double), typeof(ProgressRing), new PropertyMetadata(0.0));

	public double EllipseDiameterTemplateSetting
	{
		get
		{
			return (double)GetValue(EllipseDiameterTemplateSettingProperty);
		}
		private set
		{
			SetValue(EllipseDiameterTemplateSettingProperty, value);
		}
	}

	public Thickness EllipseOffsetTemplateSetting
	{
		get
		{
			return (Thickness)GetValue(EllipseOffsetTemplateSettingProperty);
		}
		private set
		{
			SetValue(EllipseOffsetTemplateSettingProperty, value);
		}
	}

	public bool IsActive
	{
		get
		{
			return (bool)GetValue(IsActiveProperty);
		}
		set
		{
			SetValue(IsActiveProperty, value);
		}
	}

	public double MaxSideLengthTemplateSetting
	{
		get
		{
			return (double)GetValue(MaxSideLengthTemplateSettingProperty);
		}
		private set
		{
			SetValue(MaxSideLengthTemplateSettingProperty, value);
		}
	}

	public ProgressRing()
	{
		base.SizeChanged += OnSizeChanged;
	}

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		UpdateActiveState();
	}

	private static void IsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is ProgressRing progressRing)
		{
			progressRing.UpdateActiveState();
		}
	}

	private void OnSizeChanged(object sender, SizeChangedEventArgs e)
	{
		UpdateMaxSideLength();
		UpdateEllipseDiameter();
		UpdateEllipseOffset();
	}

	private void UpdateActiveState()
	{
		if (IsActive)
		{
			VisualStateManager.GoToState(this, "Active", useTransitions: true);
		}
		else
		{
			VisualStateManager.GoToState(this, "Inactive", useTransitions: true);
		}
	}

	private void UpdateEllipseDiameter()
	{
		if (base.ActualWidth <= 25.0)
		{
			EllipseDiameterTemplateSetting = 3.0;
		}
		else
		{
			EllipseDiameterTemplateSetting = base.ActualWidth / 10.0 + 0.5;
		}
	}

	private void UpdateEllipseOffset()
	{
		if (base.ActualWidth <= 25.0)
		{
			EllipseOffsetTemplateSetting = new Thickness(0.0, 7.0, 0.0, 0.0);
		}
		else if (base.ActualWidth <= 30.0)
		{
			double top = base.ActualWidth * 0.0 - 4.0;
			EllipseOffsetTemplateSetting = new Thickness(0.0, top, 0.0, 0.0);
		}
		else
		{
			double top2 = base.ActualWidth * 0.0 - 2.0;
			EllipseOffsetTemplateSetting = new Thickness(0.0, top2, 0.0, 0.0);
		}
	}

	private void UpdateMaxSideLength()
	{
		if (base.ActualWidth <= 25.0)
		{
			MaxSideLengthTemplateSetting = 20.0;
		}
		else
		{
			MaxSideLengthTemplateSetting = base.ActualWidth - 5.0;
		}
	}
}
