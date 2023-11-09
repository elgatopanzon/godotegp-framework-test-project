namespace GodotEGP.Objects;

using Godot;
using System;
using System.Collections.Generic;

using GodotEGP.Logging;

public partial class ObjectPool<T> where T: class
{
	private Stack<T> _objects;
	private int _capacityInitial;
	private int _capacityMax;

	public ObjectPool(int capacityInitial = 0, int capacityMax = 100)
	{
		_capacityMax = capacityMax;
		_capacityInitial = capacityInitial;

		_objects = new Stack<T>(capacityMax);

		for (int i = 0; i < capacityInitial; i++)
		{
			Return(Get());
		}
	}

	public T Get()
	{
		if (_objects.Count > 0)
		{
			LoggerManager.LogDebug("Using instance from pool", typeof(T).ToString());

			return _objects.Pop();
		}
		else
		{
			LoggerManager.LogDebug("Creating new instance", typeof(T).ToString());

			return (T) Activator.CreateInstance(typeof(T));
		}
	}

	public void Return(T obj)
	{
		if (_objects.Count < _capacityMax)
			_objects.Push(obj);
	}
}
