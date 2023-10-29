namespace Godot.EGP;

using System;
using System.Collections.Generic;
using Godot;

public interface IEventSubscription<in T> where T : Event
{
	object Subscriber { get; }
	Action<IEvent> CallbackMethod { get; }
	Type EventType { get; }
	bool IsHighPriority { get; }
	bool Oneshot { get; }
	List<IEventFilter> EventFilters { get; }
}

public class EventSubscription<T> : IEventSubscription<Event>
{
    public object Subscriber { get; }
    public Action<IEvent> CallbackMethod { get; }
    public bool IsHighPriority { get; }
    public bool Oneshot { get; set; }
    public Type EventType { get; }
    public List<IEventFilter> EventFilters { get; }

    public EventSubscription(object subscriberObj, Action<IEvent> callbackMethod, bool isHighPriority = false, bool oneshot = false, List<IEventFilter> eventFilters = null)
    {
        EventType = typeof(T);
        Subscriber = subscriberObj;
        CallbackMethod = callbackMethod;
        IsHighPriority = isHighPriority;
        Oneshot = oneshot;

        if (eventFilters == null)
        {
        	eventFilters = new List<IEventFilter>();
        }

        EventFilters = eventFilters;
    }
}

