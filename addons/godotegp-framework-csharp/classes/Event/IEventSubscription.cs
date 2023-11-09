namespace GodotEGP.Event;

using System;
using System.Collections.Generic;

using GodotEGP.Event.Events;
using GodotEGP.Event.Filter;

public interface IEventSubscription<in T> where T : Event
{
	object Subscriber { get; }
	Action<IEvent> CallbackMethod { get; }
	Type EventType { get; }
	bool IsHighPriority { get; }
	bool Oneshot { get; }
	List<IFilter> EventFilters { get; set; }
}

