namespace Godot.EGP;

using Godot;
using System;
using System.Collections.Generic;

public class ObjectPool<T> where T: class
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
			GD.Print($"{typeof(T)}: Using instance from pool");

			return _objects.Pop();
		}
		else
		{
			GD.Print($"{typeof(T)}: Creating new instance");

			return (T) Activator.CreateInstance(typeof(T));
		}
	}

	public void Return(T obj)
	{
		if (_objects.Count < _capacityMax)
			_objects.Push(obj);
	}
}
