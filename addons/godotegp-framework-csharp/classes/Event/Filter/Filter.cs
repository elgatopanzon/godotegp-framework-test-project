namespace GodotEGP.Event.Filter;

using Godot;
using System;

using GodotEGP.Event.Events;
using GodotEGP.Logging;
using GodotEGP.Objects.Extensions;

public partial class Filter : IFilter
{
	public bool Match(IEvent matchEvent)
	{
		return true;
	}
}

public partial class OwnerObject : IFilter
{
	private object _matchObject;

	public OwnerObject(object matchObject)
	{
		_matchObject = matchObject;
	}

	public bool Match(IEvent matchEvent)
	{
		// LoggerManager.LogDebug("match result", "", "result", matchEvent.Owner.Equals(_matchObject));
		return matchEvent.Owner.Equals(_matchObject);
	}
}

public partial class SignalType : IFilter
{
	private string _matchSignal;

	public SignalType(string matchSignal)
	{
		_matchSignal = matchSignal;
	}

	public bool Match(IEvent matchEvent)
	{
		if (matchEvent.TryCast(out Events.GodotSignal e))
		{
			// LoggerManager.LogDebug("match result", "", "result", e.SignalName == _matchSignal);

			return e.SignalName == _matchSignal;
		}

		return false;
	}
}

public partial class ObjectType : IFilter
{
	private Type _matchType;

	public ObjectType(Type matchType)
	{
		_matchType = matchType;
	}

	public bool Match(IEvent matchEvent)
	{
		return matchEvent.GetType() == _matchType;
	}
}
