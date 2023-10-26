namespace Godot.EGP;

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ServiceRegistry : Node
{
	// Static ServiceRegistry instance
	public static ServiceRegistry Instance { get; private set; }

	// Dictionary of BaseService objects
	private Dictionary<Type, Service> _serviceObjs = new Dictionary<Type, Service>();

	/// <summary>
	/// Access service objects using []
	/// <example>
	/// <code>
	/// ServiceRegistry.Instance[Service]
	/// </code>
	/// </example>
	/// </summary>
	public Service this[Type serviceType] {
		get {
			return GetService(serviceType);
		}
	}

	private ServiceRegistry() 
	{
		Instance = this;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	/// <summary>
	/// Register a <c>BaseService</c> object to the registry, with short-form
	/// name
	/// <param name="serviceObj">Instance of a BaseService object</param>
	/// <param name="serviceName">Short-name for the service object</param>
	/// </summary>
	public void RegisterService(Service serviceObj)
	{
		_serviceObjs.Add(serviceObj.GetType(), serviceObj);

		AddChild(serviceObj);

		serviceObj._OnServiceRegistered();

		Get<EventManager>().Emit(new EventServiceRegistered(serviceObj));
	}

	/// <summary>
	/// Get a service object
	/// <param name="serviceType">Short-name of the service object</param>
	/// </summary>
	public Service GetService(Type serviceType)
	{
		if (_serviceObjs.ContainsKey(serviceType))
			return _serviceObjs[serviceType];

		return null;
	}

	/// <summary>
	/// Get a service object by the given type
	/// </summary>
	public static T Get<T>() where T : Service, new()
	{
		if (!Instance._serviceObjs.TryGetValue(typeof(T), out Service obj))
		{
			LoggerManager.LogDebug("Lazy-creating service instance", "", "service", typeof(T).Name);

			obj = new T();
			Instance.RegisterService(obj);
		}

		return (T) obj;
	}
}
