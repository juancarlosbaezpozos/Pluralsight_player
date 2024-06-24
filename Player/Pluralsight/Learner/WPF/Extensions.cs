using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Pluralsight.Learner.WPF;

public static class Extensions
{
	public static IEnumerable<T> SelectAllRecursively<T>(this IEnumerable<T> items, Func<T, IEnumerable<T>> func)
	{
		return (items ?? Enumerable.Empty<T>()).SelectMany((T o) => new T[1] { o }.Concat(func(o).SelectAllRecursively(func)));
	}

	public static IEnumerable<DependencyObject> GetChildren(this DependencyObject obj)
	{
		return from i in Enumerable.Range(0, VisualTreeHelper.GetChildrenCount(obj))
			select VisualTreeHelper.GetChild(obj, i);
	}

	public static IEnumerable<DependencyObject> GetAllChildren(this DependencyObject obj)
	{
		return obj.GetChildren().SelectAllRecursively((DependencyObject o) => o.GetChildren());
	}
}
