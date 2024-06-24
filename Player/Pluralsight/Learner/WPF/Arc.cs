using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Pluralsight.Learner.WPF;

public class Arc : Shape
{
	public static readonly DependencyProperty CenterProperty;

	public static readonly DependencyProperty StartAngleProperty;

	public static readonly DependencyProperty EndAngleProperty;

	public static readonly DependencyProperty RadiusProperty;

	public static readonly DependencyProperty SmallAngleProperty;

	public Point Center
	{
		get
		{
			return (Point)GetValue(CenterProperty);
		}
		set
		{
			SetValue(CenterProperty, value);
		}
	}

	public double StartAngle
	{
		get
		{
			return (double)GetValue(StartAngleProperty);
		}
		set
		{
			SetValue(StartAngleProperty, value);
		}
	}

	public double EndAngle
	{
		get
		{
			return (double)GetValue(EndAngleProperty);
		}
		set
		{
			SetValue(EndAngleProperty, value);
		}
	}

	public double Radius
	{
		get
		{
			return (double)GetValue(RadiusProperty);
		}
		set
		{
			SetValue(RadiusProperty, value);
		}
	}

	public bool SmallAngle
	{
		get
		{
			return (bool)GetValue(SmallAngleProperty);
		}
		set
		{
			SetValue(SmallAngleProperty, value);
		}
	}

	protected override Geometry DefiningGeometry
	{
		get
		{
			double num = ((StartAngle < 0.0) ? (StartAngle + Math.PI * 2.0) : StartAngle);
			double num2 = ((EndAngle < 0.0) ? (EndAngle + Math.PI * 2.0) : EndAngle);
			double num3 = Math.PI * 2.0 - num;
			num = Math.PI * 2.0 - num2;
			num2 = num3;
			num -= Math.PI / 2.0;
			if (num < 0.0)
			{
				num += Math.PI * 2.0;
			}
			num2 -= Math.PI / 2.0;
			if (num2 < 0.0)
			{
				num2 += Math.PI * 2.0;
			}
			if (num2 < num)
			{
				num2 += Math.PI * 2.0;
			}
			SweepDirection sweepDirection = SweepDirection.Counterclockwise;
			bool isLargeArc;
			if (SmallAngle)
			{
				isLargeArc = false;
				sweepDirection = ((!(num2 - num > Math.PI)) ? SweepDirection.Clockwise : SweepDirection.Counterclockwise);
			}
			else
			{
				isLargeArc = Math.Abs(num2 - num) < Math.PI;
			}
			Point start = Center + new Vector(Math.Cos(num), Math.Sin(num)) * Radius;
			Point point = Center + new Vector(Math.Cos(num2), Math.Sin(num2)) * Radius;
			List<PathSegment> segments = new List<PathSegment>
			{
				new ArcSegment(point, new Size(Radius, Radius), 0.0, isLargeArc, sweepDirection, isStroked: true)
			};
			return new PathGeometry(new List<PathFigure>
			{
				new PathFigure(start, segments, closed: false)
			}, FillRule.EvenOdd, null);
		}
	}

	static Arc()
	{
		CenterProperty = DependencyProperty.Register("Center", typeof(Point), typeof(Arc), new FrameworkPropertyMetadata(new Point(0.0, 0.0), FrameworkPropertyMetadataOptions.AffectsRender));
		StartAngleProperty = DependencyProperty.Register("StartAngle", typeof(double), typeof(Arc), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));
		EndAngleProperty = DependencyProperty.Register("EndAngle", typeof(double), typeof(Arc), new FrameworkPropertyMetadata(Math.PI / 2.0, FrameworkPropertyMetadataOptions.AffectsRender));
		RadiusProperty = DependencyProperty.Register("Radius", typeof(double), typeof(Arc), new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsRender));
		SmallAngleProperty = DependencyProperty.Register("SmallAngle", typeof(bool), typeof(Arc), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(Arc), new FrameworkPropertyMetadata(typeof(Arc)));
	}
}
