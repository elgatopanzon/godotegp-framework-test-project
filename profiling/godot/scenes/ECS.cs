/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ECS
 * @created     : Friday May 10, 2024 16:56:04 CST
 */

namespace GodotEGP.Profiling.G.ECS;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;
using GodotEGP.Collections;

using GodotEGP.Profiling.CLI.ECS;
using System;
using System.Linq;

public partial class ECS : Node2D
{
	private ProfileBase _profile;
	private bool _active = false;

	private int _frames;
	private int _fps;

	private DateTime _lastUpdate = DateTime.Now;
	private DateTime _lastFrameCount = DateTime.Now;
	private PackedArray<int> _fpsSamples = new();

	private int _entities = 16000;
	private double _deltaTime;

	public override void _Ready()
	{
		LoggerManager.LogDebug("Ready!");

		var args = OS.GetCmdlineArgs();

		LoggerManager.LogDebug("Args", "", "args", args);

		LoggerManager.Instance.SetConfig(new LoggerConfig() {
			LogLevel = Logging.Message.LogLevel.Info,
			});

		_profile = new ECSProfile_Update_6(_entities, false);
	    _active = true;
	}

	public override void _Process(double deltaTime)
	{
		if (LoggerManager.Instance.Config.LogLevel != Logging.Message.LogLevel.Info)
		{
			LoggerManager.Instance.Config.LogLevel = Logging.Message.LogLevel.Info;
		}
		if (_active)
		{
			DateTime timeNow = DateTime.Now;
			// _deltaTime = (timeNow.Ticks - _lastUpdate.Ticks) / 10000000f;

			// LoggerManager.LogDebug("Updating ECS main thread");
			_profile.Update(deltaTime);

			_lastUpdate = timeNow;

			_frames++;

			if ((timeNow - _lastFrameCount).TotalSeconds >= 1)
			{
				_fps = _frames;

				_frames = 0;
				_fpsSamples.Add(_fps);

				LoggerManager.LogInfo("FPS", "", "fps", $"ECS {_fps} @ {_entities}e (avg:{Convert.ToInt32(_fpsSamples.Span.ToArray().TakeLast(50).Average())}) [({deltaTime * 1000}ms) ({deltaTime * 1000000}us) ({deltaTime * 1000000000}ns)] cpe:{(deltaTime * 1000) / _entities}ms)");

				_lastFrameCount = timeNow;
				_lastUpdate = _lastFrameCount;
			}
		}
	}
}

