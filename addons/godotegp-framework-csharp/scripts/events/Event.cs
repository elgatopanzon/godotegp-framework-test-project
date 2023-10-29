namespace Godot.EGP;

using System;
using System.Collections.Generic;
using Godot;

public class Event : IEvent
{
	public object Owner { get; set; }
	public object Data { get; set; }
	public DateTime Created { get; set; }

	public Event()
	{
		Created = DateTime.Now;
	}
}

static class EventExtensionMethods
{
	static public T SetOwner<T>(this T o, object owner) where T : Event
    {
		o.Owner = owner;
        return o;
    }
	static public T SetData<T>(this T o, params object[] data) where T : Event
    {
		o.Data = data;
        return o;
    }

    static public T Invoke<T>(this T o) where T : Event
    {
		ServiceRegistry.Get<EventManager>().Emit(o);

		return o;
    }
}

public class EventServiceRegistered : Event
{
}

public class EventServiceDeregistered : Event
{
}

public class EventSignal : Event
{
 	public string SignalName { get; set; }
 	public Variant[] SignalParams { get; set; }

}
static class EventSignalExtensionMethods
{
	static public T SetSignalName<T>(this T o, string signalName) where T : EventSignal
    {
		o.SignalName = signalName;
        return o;
    }
	static public T SetSignalParams<T>(this T o, Variant[] signalParams) where T : EventSignal
    {
		o.SignalParams = signalParams;
        return o;
    }
}

public class EventCustom : Event
{
}

public class EventNode : Event
{
	public Node Node;
}
static class EventNodeExtensionMethods
{
	static public T SetNode<T>(this T o, Node node) where T : EventNode
    {
		o.Node = node;
        return o;
    }
}

public class EventNodeAdded : EventNode
{
}

public class EventNodeRemoved : EventNode
{
}


public class EventServiceReady : Event
{
}
