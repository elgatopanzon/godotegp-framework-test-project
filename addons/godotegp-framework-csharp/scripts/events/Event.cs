namespace Godot.EGP;

using System;
using Godot;

public class Event : IEvent
{
	public object Owner { get; set; }
	public DateTime Created { get; set; }

	public Event(object ownerObj)
	{
		Created = DateTime.Now;
		Owner = ownerObj;
	}
}

public class EventServiceRegistered : Event
{
	public EventServiceRegistered(object ownerObj) : base(ownerObj) { }
}

public class EventServiceDeregistered : Event
{
	public EventServiceDeregistered(object ownerObj) : base(ownerObj) { }
}

public class EventSignal : Event
{
	public string SignalName { get; }
	public Variant[] SignalParams { get; }

	public EventSignal(object ownerObj, string signalName, Variant[] signalParams = null) : base(ownerObj) 
	{
		SignalName = signalName;
		SignalParams = signalParams;
	}
}
