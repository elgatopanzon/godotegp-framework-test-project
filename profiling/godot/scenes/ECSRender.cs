/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ECSRender
 * @created     : Saturday May 18, 2024 19:21:56 CST
 */

namespace GodotEGP.Profiling.G.ECSv4;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;
using GodotEGP.Collections;

using System;
using System.Linq;
using System.Diagnostics;

using GodotEGP.ECSv4;
using GodotEGP.ECSv4.Components;
using GodotEGP.ECSv4.Systems;
using GodotEGP.ECSv4.Queries;
using GodotEGP.Random;

public partial class ECSRender : Node2D
{
	private ECS _ecs;
	private bool _active = false;

	private int _frames;
	private int _fps;

	private TimeSpan _lastUpdate;
	private TimeSpan _lastFrameCount = new TimeSpan();
	private PackedArray<int> _fpsSamples = new();

	[Export]
	private int _entities = 1000;
	private double _deltaTime;

	private Stopwatch _stopwatch;

	public override void _Ready()
	{
		LoggerManager.LogDebug("Ready!");

		var args = OS.GetCmdlineArgs();

		LoggerManager.LogDebug("Args", "", "args", args);

		Setup();

		LoggerManager.Instance.SetConfig(new LoggerConfig() {
			LogLevel = Logging.Message.LogLevel.Info,
			});

	    _stopwatch = new();
	    _stopwatch.Start();
	}

	public void Setup()
	{
		// setup the ECS
		_ecs = new();

		// register components and systems
		_ecs.RegisterComponent<GodotNode>();

		_ecs.RegisterSystem<NodeMovementSystem, OnUpdatePhase>(_ecs.CreateQuery()
				.Has<GodotNode>()
				.Has<RNG>()
				.Build()
			);
		_ecs.RegisterSystem<RNGSystem, FinalPhase>(_ecs.CreateQuery()
				.Has<RNG>()
				.Build()
			);

		// create number of entities and nodes
		for (int i = 0; i < _entities; i++)
		{
			EntityHandle e = _ecs.Create($"entity_{i+1}");
			

			// map godot node to entity
			var scene = GD.Load<PackedScene>("res://scenes/Square.tscn");
			var node = (Node2D) scene.Instantiate();

			node.Position = new Vector2(GetViewportRect().Size.X / 2, GetViewportRect().Size.Y / 2);

			e.Set<GodotNode>(new GodotNode() {
				Node2DId = _ecs.RegisterObject(node),
				});

			// define an RNG instance
			e.Set<RNG>(new RNG());

			AddChild(node);
		}
	}

	public override void _Process(double deltaTime)
	{
		_ecs.Update(deltaTime);

		_lastUpdate = _stopwatch.Elapsed;
		_frames++;

		if ((_lastUpdate - _lastFrameCount).TotalSeconds >= 1)
		{
			_fps = _frames;

			_frames = 0;
			_fpsSamples.Add(_fps);

			LoggerManager.LogInfo("FPS", "", "fps", $"ECSv4 {_fps} @ {_entities}e (avg:{Convert.ToInt32(_fpsSamples.RawArray.TakeLast(50).Average())}) [({deltaTime * 1000}ms) ({deltaTime * 1000000}us) ({deltaTime * 1000000000}ns)] cpe:{(int) ((deltaTime * 1000000000) / _entities)}ns)");

			_lastFrameCount = _lastUpdate;
			_lastUpdate = _lastFrameCount;
		}
	}
}

/********************
*  ECS components  *
********************/

// holds a reference to a godot node object
public struct GodotNode : IComponentData
{
	public static int Id { get; set; }

	// godot node instance ID
	public Entity Node2DId { get; set; }
}

public struct RNG : IComponentData
{
	public static int Id { get; set; }
	public double d1;
	public double d2;
	public NumberGenerator NumberGenerator { get; } = new();

	public RNG()
	{
		NumberGenerator.Randomize();
	}
}

/*****************
*  ECS systems  *
*****************/

// update node position
public class NodeMovementSystem : ISystem
{
	public void Update(Entity entity, int index, SystemInstance system, double deltaTime, ECS core, Query query)
	{
		ref GodotNode nodeComponent = ref query.Results.GetComponent<GodotNode>(entity);
		ref RNG rng = ref query.Results.GetComponent<RNG>(entity);

		Node2D node = core.GetObject<Node2D>(nodeComponent.Node2DId); 
		float delta = (float) deltaTime;

		Vector2 pos = node.Position;

		node.Position = node.Position with {
			X = pos.X + (float) rng.d1 * delta,
			Y = pos.Y + (float) rng.d2 * delta,
		};
	}
}

// randomly update some data values
public class RNGSystem : ISystem
{
	public void Update(Entity entity, int index, SystemInstance system, double deltaTime, ECS core, Query query)
	{
		double minValue = -500;
		double maxValue = 500;

		ref RNG data = ref query.Results.GetComponent<RNG>(entity);

		data.d1 = minValue + (Random.Shared.NextDouble() * (maxValue - minValue));
		data.d2 = minValue + (Random.Shared.NextDouble() * (maxValue - minValue));
	}
}
