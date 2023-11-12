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
using GodotEGP.Config;
using GodotEGP.Resource;

public partial class SceneManager : Service
{
	private Dictionary<string, Definition> _sceneDefinitions = new Dictionary<string, Definition>();

	public SceneManager()
	{
		
	}

	public void SetConfig(Dictionary<string, Definition> config)
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
		_SetServiceReady(true);
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
	
	public void ChangeScene(string sceneId)
	{
		if (SceneIdValid(sceneId))
		{
			LoggerManager.LogDebug("Changing scene", "", "sceneId", sceneId);
		}
	}

	public bool SceneIdValid(string sceneId)
	{
		return _sceneDefinitions.ContainsKey(sceneId);
	}
}

