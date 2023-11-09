namespace Godot.EGP;

using Godot;
using System;
using Godot.EGP.Extensions;

public class EventFilterSignal : IEventFilter
{
	private string _matchSignal;

	public EventFilterSignal(string matchSignal)
	{
		_matchSignal = matchSignal;
	}

	public bool Match(IEvent matchEvent)
	{
		if (matchEvent.TryCast(out EventSignal e))
		{
			LoggerManager.LogDebug("match result", "", "result", e.SignalName == _matchSignal);

			return e.SignalName == _matchSignal;
		}

		return false;
	}
}
