namespace Godot.EGP;

using Godot;
using System;
using System.Collections.Generic;

public partial class EventManager : Service
{
	private Dictionary<Type, List<IEventSubscription<Event>>> _eventSubscriptions = new Dictionary<Type, List<IEventSubscription<Event>>>();

	private Dictionary<Type, EventQueue> _eventQueues = new Dictionary<Type, EventQueue>();

	public EventManager()
	{
	}

	public void Subscribe(IEventSubscription<Event> eventSubscription)
	{
		if (!_eventSubscriptions.TryGetValue(eventSubscription.EventType, out List<IEventSubscription<Event>> subList))
		{
			subList = new List<IEventSubscription<Event>>();
			_eventSubscriptions.TryAdd(eventSubscription.EventType, subList);

			LoggerManager.LogDebug("Creating subscriber list for event type", "", "eventType", eventSubscription.EventType.Name);
		}

		subList.Add(eventSubscription);

		LoggerManager.LogDebug("Adding event subscription", "", "eventSubscription", new Dictionary<string, object> 
				{
					{ "subscriberType", eventSubscription.Subscriber.GetType().Name },
					{ "eventType", eventSubscription.EventType.Name },
					{ "isHighPriority", eventSubscription.IsHighPriority },
					{ "filterCount", eventSubscription.EventFilters.Count },
				}
			);
	}

	public bool Unsubscribe(IEventSubscription<Event> eventSubscription)
	{
		if (_eventSubscriptions.TryGetValue(eventSubscription.EventType, out List<IEventSubscription<Event>> subList))
		{
			LoggerManager.LogDebug("Removing event subscription", "", "eventSubscription", new Dictionary<string, object> 
				{
					{ "subscriberType", eventSubscription.Subscriber.GetType().Name },
					{ "eventType", eventSubscription.EventType.Name },
					{ "isHighPriority", eventSubscription.IsHighPriority },
					{ "filterCount", eventSubscription.EventFilters.Count },
				}
			);

			return subList.Remove(eventSubscription);
		}

		return false;
	}

	public T GetQueue<T>() where T : EventQueue, new()
	{
		if (!_eventQueues.TryGetValue(typeof(T), out EventQueue eventQueue))
		{
			eventQueue = new T();
			_eventQueues.TryAdd(typeof(T), eventQueue);

			LoggerManager.LogDebug("Creating event queue", "", "eventQueue", typeof(T).Name);
		}

		return (T) eventQueue;
	}

	public void Queue<T>(IEvent eventObj) where T : EventQueue, new()
	{
		GetQueue<T>().Queue(eventObj);
	}

	public Queue<IEvent> Fetch<T>(Type eventType, List<IEventFilter> eventFilters = null, int fetchCount = 1) where T : EventQueue, new()
	{
		// init eventFilters list if it's null
		if (Object.Equals(eventFilters, default(List<IEventFilter>)))
		{
			eventFilters = new List<IEventFilter>();
		}

		// add the eventType filter
		eventFilters.Add(new EventFilterType(eventType));

		return GetQueue<T>().Fetch(eventFilters, fetchCount);
	}

	public void Emit(IEvent eventObj)
	{
		// emit the event to high-priority subscribers
		if (_eventSubscriptions.TryGetValue(eventObj.GetType(), out List<IEventSubscription<Event>> subList))
		{
			foreach (IEventSubscription<Event> eventSubscription in subList)
			{
				if (eventObj.GetType() == eventSubscription.EventType && eventSubscription.IsHighPriority)
				{
					LoggerManager.LogDebug("Broadcasting high-priority event", "", "broadcast", new Dictionary<string, object> {{ "eventType", eventObj.GetType().Name }, { "subscriberType", eventSubscription.Subscriber.GetType().Name } });

					eventSubscription.CallbackMethod(eventObj);
				}
			}
		}

		// queue event for low-priority subscribers
		GetQueue<EventQueueDeferred>().Queue(eventObj);
	}

	public override void _Process(double delta)
	{
		// process events for each subscription type
		Queue<IEvent> eventQueue = GetQueue<EventQueueDeferred>().Fetch();

		while (eventQueue.TryPeek(out IEvent eventObj))
		{
			// remove item from the queue
			eventObj = eventQueue.Dequeue();

			bool eventConsumed = false;

			// process the event if there's a matching sub for the event type
			if (_eventSubscriptions.TryGetValue(eventObj.GetType(), out List<IEventSubscription<Event>> subList))
			{
				foreach (IEventSubscription<Event> eventSubscription in subList)
				{
					// only act upon subscription if it's not high priority
					if (!eventSubscription.IsHighPriority)
					{
						LoggerManager.LogDebug("Broadcasting deferred event", "", "broadcast", new Dictionary<string, object> {{ "eventType", eventObj.GetType().Name }, { "subscriberType", eventSubscription.Subscriber.GetType().Name } });

						eventSubscription.CallbackMethod(eventObj);

						eventConsumed = true;
					}
				}
				
			}

			LoggerManager.LogDebug("Deferred event consumed state", "", "consumed", eventConsumed);
		}
	}
}

public class EventQueueDeferred : EventQueue
{
	
}
