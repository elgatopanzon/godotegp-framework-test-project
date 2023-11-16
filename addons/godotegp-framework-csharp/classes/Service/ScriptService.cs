/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ScriptService
 * @created     : Thursday Nov 16, 2023 14:19:07 CST
 */

namespace GodotEGP.Service;

using System;
using System.Linq;
using System.Collections.Generic;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.Scripting;
using GodotEGP.Scripting.Functions;

public partial class ScriptService : Service
{
	private string _scriptFunctionsNamespace = "GodotEGP.Scripting.Functions";

	private Dictionary<string, IScriptFunction> _scriptFunctions = new Dictionary<string, IScriptFunction>();

	public Dictionary<string, IScriptFunction> ScriptFunctions
	{
		get { return _scriptFunctions; }
		set { _scriptFunctions = value; }
	}

	public ScriptService()
	{
	}


	/*******************
	*  Godot methods  *
	*******************/
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
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
		// create instance of function objects and register them
		var scriptFunctionClasses = AppDomain.CurrentDomain.GetAssemblies()
                       .SelectMany(t => t.GetTypes())
                       .Where(t => t.IsClass && t.Namespace == _scriptFunctionsNamespace);

		foreach (Type functionType in scriptFunctionClasses)
		{
			LoggerManager.LogDebug("Registering function", "", "func", $"{functionType.Name.ToLower()} as {functionType}");
			_scriptFunctions.Add(functionType.Name.ToLower(), (IScriptFunction) Activator.CreateInstance(functionType));
		}

		_SetServiceReady(true);
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
}

