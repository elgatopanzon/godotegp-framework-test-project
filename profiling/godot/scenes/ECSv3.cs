/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ECSv3
 * @created     : Thursday May 09, 2024 18:29:43 CST
 */

namespace GodotEGP.Profiling.G.ECSv3;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;
using GodotEGP.Collections;

using GodotEGP.Profiling.CLI.ECSv3;
using System;
using System.Linq;

public partial class ECSv3 : Node2D
{
	private ProfileBase _profile;
	private bool _active = false;

	private int _frames;
	private int _fps;

	private DateTime _lastUpdate = DateTime.Now;
	private DateTime _lastFrameCount = DateTime.Now;
	private PackedArray<int> _fpsSamples = new();

	private ulong _entities = 65000;

	public override void _Ready()
	{
		LoggerManager.LogInfo("Ready!");

		var args = OS.GetCmdlineArgs();

		LoggerManager.LogInfo("Args", "", "args", args);

		_profile = new ECSv3Profile_Update_6(_entities, false);
	    _active = true;
	}

	public override void _Process(double deltaTime)
	{
		if (_active)
		{
			DateTime timeNow = DateTime.Now;

			LoggerManager.LogError("Updating ECS main thread");
			_profile.Update(deltaTime);

			_lastUpdate = timeNow;

			_frames++;

			if ((timeNow - _lastFrameCount).TotalSeconds >= 1)
			{
				_fps = _frames;

				_frames = 0;
				_fpsSamples.Add(_fps);

				Console.WriteLine($"ECS {_fps} @ {_entities}e (avg:{Convert.ToInt32(_fpsSamples.Span.ToArray().TakeLast(50).Average())}) [({deltaTime * 1000}ms) ({deltaTime * 1000000}us) ({deltaTime * 1000000000}ns)] cpe:{(deltaTime * 1000) / _entities}ms)");

				_lastFrameCount = timeNow;
				_lastUpdate = _lastFrameCount;
			}
		}
	}
}

