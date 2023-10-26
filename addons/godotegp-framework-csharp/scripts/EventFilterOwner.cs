namespace Godot.EGP;

using Godot;
using System;

public class EventFilterOwner : IEventFilter
{
	private object _matchObject;

	public EventFilterOwner(object matchObject)
	{
		_matchObject = matchObject;
	}

	public bool Match(IEvent matchEvent)
	{
		LoggerManager.LogDebug("match result", "", "result", matchEvent.Owner.Equals(_matchObject));
		return matchEvent.Owner.Equals(_matchObject);
	}
}

