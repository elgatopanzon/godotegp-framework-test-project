namespace Godot.EGP;

using System;
using System.Collections.Generic;
using Godot;
using System.Threading;
using System.ComponentModel;

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


public class EventBackgroundJob : Event
{
	public object JobOwner;
	public DoWorkEventArgs DoWorkEventArgs;
	public ProgressChangedEventArgs ProgressChangedEventArgs;
	public RunWorkerCompletedEventArgs RunWorkerCompletedEventArgs;
}
static class EventBackgroundJobExtensionMethods
{
	static public T SetJobOwner<T>(this T o, object jobOwner) where T : EventBackgroundJob
    {
		o.JobOwner = jobOwner;
        return o;
    }
	static public T SetDoWorkEventArgs<T>(this T o, DoWorkEventArgs e) where T : EventBackgroundJob
    {
		o.DoWorkEventArgs = e;
        return o;
    }
	static public T SetProgressChangesEventArgs<T>(this T o, ProgressChangedEventArgs e) where T : EventBackgroundJob
    {
		o.ProgressChangedEventArgs = e;
        return o;
    }
	static public T SetRunWorkerCompletedEventArgs<T>(this T o, RunWorkerCompletedEventArgs e) where T : EventBackgroundJob
    {
		o.RunWorkerCompletedEventArgs = e;
        return o;
    }
}

public class EventBackgroundJobWorking : EventBackgroundJob
{
}
public class EventBackgroundJobProgress : EventBackgroundJob
{
}
public class EventBackgroundJobComplete : EventBackgroundJob
{
}
public class EventBackgroundJobError : EventBackgroundJob
{
}

public class EventDataOperationWorking : EventBackgroundJob
{
}
public class EventDataOperationProgress : EventBackgroundJob
{
}
public class EventDataOperationComplete : EventBackgroundJob
{
}
public class EventDataOperationError : EventBackgroundJob
{
}
