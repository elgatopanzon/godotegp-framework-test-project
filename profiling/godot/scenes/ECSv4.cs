/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ECSv3
 * @created     : Thursday May 09, 2024 18:29:43 CST
 */

namespace GodotEGP.Profiling.G.ECSv4;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;
using GodotEGP.Collections;

using GodotEGP.Profiling.CLI.ECSv4;
using System;
using System.Linq;
using System.Diagnostics;

public partial class ECSv4 : Node2D
{
	private ProfileBase _profile;
	private bool _active = false;

	private int _frames;
	private int _fps;

	private TimeSpan _lastUpdate;
	private TimeSpan _lastFrameCount = new TimeSpan();
	private PackedArray<int> _fpsSamples = new();

	private ulong _entities = 16000;
	private double _deltaTime;

	private Stopwatch _stopwatch;

	public override void _Ready()
	{
		LoggerManager.LogDebug("Ready!");

		var args = OS.GetCmdlineArgs();

		LoggerManager.LogDebug("Args", "", "args", args);

		LoggerManager.Instance.SetConfig(new LoggerConfig() {
			LogLevel = Logging.Message.LogLevel.Info,
			});

		_profile = new ECSv4Profile_Update_6(_entities, false);
	    _active = true;

	    // create label nodes to render label strings
	    foreach (var entity in _profile.GetECS().EntityManager.GetEntities())
	    {
	    	Label label = new Label();
			label.Name = $"entity{entity.Id}";
			label.Text = "";
			// LoggerManager.LogInfo("Entity label", "", "label", label.Name.ToString());
			AddChild(label);
	    }

	    _stopwatch = new();
	    _stopwatch.Start();
	}

	public override void _Process(double deltaTime)
	{
		// DateTime timeNow = DateTime.Now;
		// _deltaTime = (timeNow.Ticks - _lastUpdate.Ticks) / 10000000f;

		// LoggerManager.LogDebug("Updating ECS main thread");
		_profile.Update(deltaTime);

		_lastUpdate = _stopwatch.Elapsed;

		_frames++;

		if ((_lastUpdate - _lastFrameCount).TotalSeconds >= 1)
		{
			_fps = _frames;

			_frames = 0;
			_fpsSamples.Add(_fps);

			LoggerManager.LogInfo("FPS", "", "fps", $"ECSv4 {_fps} @ {_entities}e (avg:{Convert.ToInt32(_fpsSamples.Span.ToArray().TakeLast(50).Average())}) [({deltaTime * 1000}ms) ({deltaTime * 1000000}us) ({deltaTime * 1000000000}ns)] cpe:{(int) ((deltaTime * 1000000000) / _entities)}ns)");

			_lastFrameCount = _lastUpdate;
			_lastUpdate = _lastFrameCount;
		}
	}
}

