namespace GodotEGP;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.State;
using System;
using System.Collections.Generic;
using GodotEGP.Objects.Validated;
using Newtonsoft.Json;
using System.Net.Http;

using GodotEGP.Service;
using GodotEGP.Logging;
using GodotEGP.Event;
using GodotEGP.Event.Events;
using GodotEGP.Event.Filter;
using GodotEGP.Config;
using GodotEGP.Data.Operation;

public partial class Main : Node
{
	public Main()
	{
		// create instance of ServiceRegistry
		AddChild(new ServiceRegistry());

		// register LoggerManager singleton as service to trigger ready state
		ServiceRegistry.Instance.RegisterService(LoggerManager.Instance);

		ServiceRegistry.Get<EventManager>().Subscribe<ServiceReady>(_On_ConfigManager_Ready).Filters(new OwnerObjectType(typeof(ConfigManager)));
		ServiceRegistry.Get<EventManager>().Subscribe<ServiceReady>(_On_ResourceManager_Ready).Filters(new OwnerObjectType(typeof(ResourceManager)));

		// trigger lazy load ConfigManager to trigger initial load
		ServiceRegistry.Get<DataService>();
		ServiceRegistry.Get<ConfigManager>();
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	public void _On_ConfigManager_Ready(IEvent e)
	{
		ServiceRegistry.Get<NodeManager>();

		ServiceRegistry.Get<ResourceManager>().SetConfig(ServiceRegistry.Get<ConfigManager>().Get<ResourceDefinitionConfig>());


	}

	public void _On_ResourceManager_Ready(IEvent e)
	{
		// set scene definitions from loaded resources
		if (ServiceRegistry.Get<ConfigManager>().Get<ResourceDefinitionConfig>().Resources.TryGetValue("Scenes", out var sceneDefinitions))
		{
			ServiceRegistry.Get<SceneManager>().SetConfig(sceneDefinitions);
		}
	}
}
