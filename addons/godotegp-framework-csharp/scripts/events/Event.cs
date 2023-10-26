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
