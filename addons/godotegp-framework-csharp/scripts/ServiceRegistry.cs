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
	private Dictionary<string, Service> _serviceObjs = new Dictionary<string, Service>();

	/// <summary>
	/// Access service objects using []
	/// <example>
	/// <code>
	/// ServiceRegistry.Instance["Base"]
	/// </code>
	/// </example>
	/// </summary>
	public Service this[string name] {
		get {
			return GetService(name);
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
	public void RegisterService(Service serviceObj, string serviceName)
	{
		_serviceObjs.Add(serviceName, serviceObj);

		serviceObj._OnServiceRegistered();
	}

	/// <summary>
	/// Get a service object
	/// <param name="name">Short-name of the service object</param>
	/// </summary>
	public Service GetService(string name)
	{
		if (_serviceObjs.ContainsKey(name))
			return _serviceObjs[name];

		return null;
	}

	/// <summary>
	/// Get a service object by the given type
	/// </summary>
	public static T Get<T>() where T : Service
	{
		return Instance._serviceObjs.Values.OfType<T>().FirstOrDefault() as T;
	}
}
