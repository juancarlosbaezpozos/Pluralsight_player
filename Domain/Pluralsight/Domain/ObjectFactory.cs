using System;
using System.Collections.Generic;

namespace Pluralsight.Domain;

public class ObjectFactory
{
	private static readonly Dictionary<Type, Func<object>> RegisteredTypes = new Dictionary<Type, Func<object>>();

	public static T Get<T>()
	{
		if (!RegisteredTypes.ContainsKey(typeof(T)))
		{
			throw new Exception("Unregistered type requested: " + typeof(T).Name);
		}
		return (T)RegisteredTypes[typeof(T)]();
	}

	public static void Register<T>(Func<object> constructor)
	{
		RegisteredTypes[typeof(T)] = constructor;
	}
}
