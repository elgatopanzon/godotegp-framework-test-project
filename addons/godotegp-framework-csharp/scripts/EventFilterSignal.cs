namespace Godot.EGP;

using Godot;
using System;

public class EventFilterSignal : IEventFilter
{
	private string _matchSignal;

	public EventFilterSignal(string matchSignal)
	{
		_matchSignal = matchSignal;
	}

	public bool Match(IEvent matchEvent)
	{
		EventSignal e = (EventSignal) matchEvent;
		LoggerManager.LogDebug("match result", "", "result", e.SignalName == _matchSignal);
		return e.SignalName == _matchSignal;
	}
}
