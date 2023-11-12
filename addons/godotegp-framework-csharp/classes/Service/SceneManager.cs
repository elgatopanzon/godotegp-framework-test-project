/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : SceneManager
 * @created     : Saturday Nov 11, 2023 22:36:53 CST
 */

namespace GodotEGP.Service;

using System;
using System.Collections.Generic;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Event.Filter;
using GodotEGP.Config;
using GodotEGP.Resource;

public partial class SceneManager : Service
{
	private Dictionary<string, ResourceBase> _sceneDefinitions = new Dictionary<string, ResourceBase>();

	private string _currentSceneId {
		get {
			return _sceneIdHistory.Peek();
		}
		set {
			_sceneIdHistory.Push(value);
		}
	}
	private Node _currentSceneInstance;

	private Stack<string> _sceneIdHistory = new Stack<string>();

	public SceneManager()
	{
		
	}

	public void SetConfig(Dictionary<string, ResourceBase> config)
	{
		LoggerManager.LogDebug("Setting scene definition config", "", "scenes", config);
		
		_sceneDefinitions = config;

		if (!GetReady())
		{
			_SetServiceReady(true);
		}
	}

	/*******************
	*  Godot methods  *
	*******************/
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// _SetServiceReady(true);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	/*********************
	*  Service methods  *
	*********************/

	// Called when service is registered in manager
	public override void _OnServiceRegistered()
	{
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
	*  Scene management methods  *
	******************************/
	
	public void LoadScene(string sceneId)
	{
		if (IsValidScene(sceneId))
		{
			LoggerManager.LogDebug("Loading scene", "", "sceneId", sceneId);

			UnloadManagedScenes();

			_currentSceneId = sceneId;
			_currentSceneInstance = GetSceneInstance(sceneId);

			this.Subscribe<NodeRemoved>(_On_NodeRemoved, oneshot: true, isHighPriority: true).Filters(new OwnerObjectType(_currentSceneInstance.GetType()));
		}
		else
		{
			throw new InvalidSceneException($"Invalid scene ID {sceneId}");
		}
	}

	public void AddCurrentScene()
	{
		AddChild(_currentSceneInstance);
	}

	public Node GetSceneInstance(string sceneId)
	{
		if (IsValidScene(sceneId))
		{
			if (_sceneDefinitions[sceneId].RawValue is PackedScene ps)
			{
				return ps.Instantiate();
			}
		}

		return null;
	}

	public bool IsValidScene(string sceneId)
	{
		return _sceneDefinitions.ContainsKey(sceneId);
	}

	public void UnloadManagedScenes()
	{
		foreach (Node node in ServiceRegistry.Get<NodeManager>().GetSceneTreeNodes())
		{
			if (node.SceneFilePath.Length > 0 && IsValidScene(node.SceneFilePath))
			{
				node.QueueFree();
			}
		}
	}

	public void LoadPreviousScene()
	{
		if (_sceneIdHistory.TryPop(out string prevId))
		{
			LoggerManager.LogDebug("Loading previous scene", "", "scene", prevId);

			LoadScene(prevId);
		}
	}

	/**********************
	*  Callback methods  *
	**********************/
	
	public void _On_NodeRemoved(IEvent e)
	{
		CallDeferred("AddCurrentScene");
	}

	/****************
	*  Exceptions  *
	****************/
	
	public class InvalidSceneException : Exception
	{
		public InvalidSceneException() {}
		public InvalidSceneException(string message) : base(message) {}
		public InvalidSceneException(string message, Exception inner) : base(message, inner) {}
	}
}

