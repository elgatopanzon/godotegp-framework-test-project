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

	private ulong _entities = 128;
	private double _deltaTime;

	public override void _Ready()
	{
		LoggerManager.LogDebug("Ready!");

		var args = OS.GetCmdlineArgs();

		LoggerManager.LogDebug("Args", "", "args", args);

		LoggerManager.Instance.SetConfig(new LoggerConfig() {
			LogLevel = Logging.Message.LogLevel.Info,
			});

		_profile = new ECSv3Profile_Update_6(_entities, false);
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
			_deltaTime = (timeNow.Ticks - _lastUpdate.Ticks) / 10000000f;

			LoggerManager.LogDebug("Updating ECS main thread");
			_profile.Update(deltaTime);

			_lastUpdate = timeNow;

			_frames++;

			if ((timeNow - _lastFrameCount).TotalSeconds >= 1)
			{
				_fps = _frames;

				_frames = 0;
				_fpsSamples.Add(_fps);

				LoggerManager.LogInfo("FPS", "", "fps", $"ECS {_fps} @ {_entities}e (avg:{Convert.ToInt32(_fpsSamples.Span.ToArray().TakeLast(50).Average())}) [({_deltaTime * 1000}ms) ({_deltaTime * 1000000}us) ({_deltaTime * 1000000000}ns)] cpe:{(_deltaTime * 1000) / _entities}ms)");

				_lastFrameCount = timeNow;
				_lastUpdate = _lastFrameCount;
			}
		}
	}
}

