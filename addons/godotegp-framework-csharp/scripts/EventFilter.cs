namespace Godot.EGP;

using Godot;
using System;

public class EventFilter : IEventFilter
{
	public bool Match(IEvent matchEvent)
	{
		return true;
	}
}
