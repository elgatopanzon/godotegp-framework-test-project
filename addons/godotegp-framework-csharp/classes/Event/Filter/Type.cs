namespace Godot.EGP;

using Godot;
using System;

public class EventFilterType : IEventFilter
{
	private Type _matchType;

	public EventFilterType(Type matchType)
	{
		_matchType = matchType;
	}

	public bool Match(IEvent matchEvent)
	{
		return matchEvent.GetType() == _matchType;
	}
}
