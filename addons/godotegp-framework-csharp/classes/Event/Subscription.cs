namespace GodotEGP.Event;

using System;
using System.Collections.Generic;
using Godot;

using GodotEGP.Event.Events;
using GodotEGP.Event.Filter;


public class EventSubscription<T> : IEventSubscription<Event>
{
    public object Subscriber { get; }
    public Action<IEvent> CallbackMethod { get; }
    public bool IsHighPriority { get; }
    public bool Oneshot { get; set; }
    public Type EventType { get; }
    public List<IFilter> EventFilters { get; set; }

    public EventSubscription(object subscriberObj, Action<IEvent> callbackMethod, bool isHighPriority = false, bool oneshot = false, List<IFilter> eventFilters = null)
    {
        EventType = typeof(T);
        Subscriber = subscriberObj;
        CallbackMethod = callbackMethod;
        IsHighPriority = isHighPriority;
        Oneshot = oneshot;

        if (eventFilters == null)
        {
        	eventFilters = new List<IFilter>();
        }

        EventFilters = eventFilters;
    }
}

public static class EventSubscriptionExtensionMethods
{
	public static IEventSubscription<Event> Filters(this IEventSubscription<Event> obj, params IFilter[] filters)
	{
		foreach (IFilter filter in filters)
		{
			obj.EventFilters.Add(filter);
		}
		return obj;
	}

	public static IEventSubscription<Event> Owner(this IEventSubscription<Event> obj, object ownerObject)
	{
		obj.Filters(new OwnerObject(ownerObject));

		return obj;
	}
}
