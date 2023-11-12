/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ResourceManager
 * @created     : Saturday Nov 11, 2023 14:07:45 CST
 */

namespace GodotEGP.Service;

using System;
using System.Collections.Generic;
using System.ComponentModel;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.Resource;

public partial class ResourceManager : Service
{
	private ResourceDefinitionConfig _resourceDefinitions;

	private Dictionary<string, ResourceBase> _resources = new Dictionary<string, ResourceBase>();

	public ResourceManager()
	{
		
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// _SetServiceReady(true);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	// Called when service is registered in manager
	public override void _OnServiceRegistered()
	{
		_resourceDefinitions = ServiceRegistry.Get<ConfigManager>().Get<ResourceDefinitionConfig>();

		LoadResources();
	}

	// Called when service is deregistered from manager
	public override void _OnServiceDeregistered()
	{
		// LoggerManager.LogDebug($"Service deregistered!", "", "service", this.GetType().Name);
	}

	// Called when service is considered ready
	public override void _OnServiceReady()
	{
	}

	/******************************
	*  Resource loading methods  *
	******************************/
	
	public void LoadResources()
	{
		LoggerManager.LogDebug("Loading resources from resource definition config");

		Queue<LoaderQueueItem> resourceQueue = new Queue<LoaderQueueItem>();

		foreach (var resourceCategory in _resourceDefinitions.Resources)
		{
			LoggerManager.LogDebug($"Queuing resources in category: {resourceCategory.Key}", "", "count", resourceCategory.Value.Count);

			foreach (var resourceDefinition in resourceCategory.Value)
			{
				resourceQueue.Enqueue(new LoaderQueueItem(resourceCategory.Key, resourceDefinition.Key, resourceDefinition.Value));
			}
		}

		ThreadedResourceLoader resourceLoader = new ThreadedResourceLoader(resourceQueue);

		// subscribe to resource loader events
		resourceLoader.SubscribeOwner<ResourceLoaderCompleted>(_On_ResourceLoader_Completed, oneshot: true);
		resourceLoader.SubscribeOwner<ResourceLoaderError>(_On_ResourceLoader_Error, oneshot: true);
	}

	/*********************************
	*  Resource management methods  *
	*********************************/
	
	public void SetResourceObject(string id, ResourceBase resource)
	{
		LoggerManager.LogDebug($"Setting resource object {id}", "", "resource", resource);
		_resources[id] = resource;
	}

	public ResourceBase GetResourceObject(string id)
	{
		if (_resources.TryGetValue(id, out ResourceBase r))
		{
			return r;
		}

		throw new InvalidResourcIdException($"No resource exists with the id {id}");
	}

	public T Get<T>(string id) where T : Godot.Resource
	{
		return (T) GetResourceObject(id).RawValue;
	}

	/**********************
	*  Callback methods  *
	**********************/
	
	public void _On_ResourceLoader_Completed(IEvent e)
	{
		LoggerManager.LogDebug("Loading resource objects completed");

		if (e is ResourceLoaderCompleted ec)
		{
			foreach (var resource in ec.Resources)
			{
				// set each resource
				SetResourceObject(resource.Id, resource.ResourceObject);
			}
			
			this.Emit<ResourceLoaderCompleted>();
		}
	}
	public void _On_ResourceLoader_Error(IEvent e)
	{
		LoggerManager.LogDebug("Loading resource objects error", "", "e", e);

		this.Emit<ResourceLoaderError>();
	}

	/****************
	*  Exceptions  *
	****************/
	
	public class InvalidResourcIdException : Exception
	{
		public InvalidResourcIdException() {}
		public InvalidResourcIdException(string message) : base(message) {}
		public InvalidResourcIdException(string message, Exception inner) : base(message, inner) {}
	}
}