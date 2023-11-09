namespace GodotEGP.Event.Events;

using System;
using System.Collections.Generic;
using Godot;
using System.ComponentModel;

using GodotEGP.Service;

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

public class ServiceRegistered : Event
{
}

public class ServiceDeregistered : Event
{
}

public class GodotSignal : Event
{
 	public string SignalName { get; set; }
 	public Variant[] SignalParams { get; set; }

}
static class SignalExtensionMethods
{
	static public T SetSignalName<T>(this T o, string signalName) where T : GodotSignal
    {
		o.SignalName = signalName;
        return o;
    }
	static public T SetSignalParams<T>(this T o, Variant[] signalParams) where T : GodotSignal
    {
		o.SignalParams = signalParams;
        return o;
    }
}

public class NodeEvent : Event
{
	public Node NodeObj;
}
static class NodeExtensionMethods
{
	static public T SetNode<T>(this T o, Node node) where T : NodeEvent
    {
		o.NodeObj = node;
        return o;
    }
}

public class NodeAdded : NodeEvent
{
}

public class NodeRemoved : NodeEvent
{
}


public class ServiceReady : Event
{
}


public class BackgroundJobEvent : Event
{
	public object JobOwner;
	public DoWorkEventArgs DoWorkEventArgs;
	public ProgressChangedEventArgs ProgressChangedEventArgs;
	public RunWorkerCompletedEventArgs RunWorkerCompletedEventArgs;
}
static class EventBackgroundJobExtensionMethods
{
	static public T SetJobOwner<T>(this T o, object jobOwner) where T : BackgroundJobEvent
    {
		o.JobOwner = jobOwner;
        return o;
    }
	static public T SetDoWorkEventArgs<T>(this T o, DoWorkEventArgs e) where T : BackgroundJobEvent
    {
		o.DoWorkEventArgs = e;
        return o;
    }
	static public T SetProgressChangesEventArgs<T>(this T o, ProgressChangedEventArgs e) where T : BackgroundJobEvent
    {
		o.ProgressChangedEventArgs = e;
        return o;
    }
	static public T SetRunWorkerCompletedEventArgs<T>(this T o, RunWorkerCompletedEventArgs e) where T : BackgroundJobEvent
    {
		o.RunWorkerCompletedEventArgs = e;
        return o;
    }
}

public class BackgroundJobWorking : BackgroundJobEvent
{
}
public class BackgroundJobProgress : BackgroundJobEvent
{
}
public class BackgroundJobComplete : BackgroundJobEvent
{
}
public class BackgroundJobError : BackgroundJobEvent
{
}

public class DataOperationWorking : BackgroundJobEvent
{
}
public class DataOperationProgress : BackgroundJobEvent
{
}
public class DataOperationComplete : BackgroundJobEvent
{
}
public class DataOperationError : BackgroundJobEvent
{
}

// events for ValidatedValue<T> objects
public class ValidatedValueEvent : Event
{
	public object Value;
	public object PrevValue;
}

static public class ValidatedValueExtensions
{
	static public T SetValue<T>(this T o, object value) where T : ValidatedValueEvent
    {
        o.Value = value;
        return o;
    }

	static public T SetPrevValue<T>(this T o, object value) where T : ValidatedValueEvent
    {
        o.PrevValue = value;
        return o;
    }
}

public class ValidatedValueChanged : ValidatedValueEvent
{
}

public class ValidatedValueSet : ValidatedValueEvent
{
}

public class ConfigManagerLoader : BackgroundJobEvent
{
	public List<Config.Object> ConfigObjects;
	
}
static public class ConfigManagerLoaderExtensions
{
	static public T SetConfigObjects<T>(this T o, List<Config.Object> configObjects) where T : ConfigManagerLoader
	{
		o.ConfigObjects = configObjects;
		return o;
	}
}

public class ConfigManagerLoaderProgress : ConfigManagerLoader
{
}
public class ConfigManagerLoaderCompleted : ConfigManagerLoader
{
}
public class ConfigManagerLoaderError : ConfigManagerLoader
{
}
