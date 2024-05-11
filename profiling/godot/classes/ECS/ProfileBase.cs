/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ProfileBase
 * @created     : Friday May 10, 2024 16:57:28 CST
 */

namespace GodotEGP.Profiling.CLI.ECS;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;
using GodotEGP.Collections;

using GodotEGP.Profiling.CLI;
using GodotEGP.ECS;

using System;
using System.Linq;
using System.Diagnostics;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public enum UpdateProfileType
{
	Update7Systems = 0,
}

public partial class ProfileBase : ProfilingContext
{
	public int Entities { get; set; }
	public UpdateProfileType ProfileType { get; set; }

	private ECS _ecs;
	private PackedArray<int> _entities;

	public ProfileBase(int entities, bool run = true)
	{
		Entities = entities;

		Setup();

		if (run)
		{
			Run();
		}
	}

	public void Setup()
	{
		_ecs = new ECS(Entities, 32, 32);
		_entities = new();

		// register systems with queries
		_ecs.RegisterSystem<MovementSystem>();
		_ecs.RegisterSystem<HealthSystem>();
		_ecs.RegisterSystem<DamageSystem>();
		_ecs.RegisterSystem<DataSystem>();
		_ecs.RegisterSystem<DirectionSystem>();
		_ecs.RegisterSystem<SpriteSystem>();

		// register components
		_ecs.RegisterComponent<Position>();
		_ecs.RegisterComponent<Velocity>();
		_ecs.RegisterComponent<DataComponent>();
		_ecs.RegisterComponent<Health>();
		_ecs.RegisterComponent<Damage>();
		_ecs.RegisterComponent<Sprite>();

		// set system archetypes
		_ecs.SetSystemComponentArchetypeState<MovementSystem, Position>(true);
		_ecs.SetSystemComponentArchetypeState<MovementSystem, Velocity>(true);
		_ecs.SetSystemComponentArchetypeState<HealthSystem, Health>(true);
		_ecs.SetSystemComponentArchetypeState<DamageSystem, Health>(true);
		_ecs.SetSystemComponentArchetypeState<DamageSystem, Damage>(true);
		_ecs.SetSystemComponentArchetypeState<DataSystem, DataComponent>(true);
		_ecs.SetSystemComponentArchetypeState<SpriteSystem, Damage>(true);
		_ecs.SetSystemComponentArchetypeState<SpriteSystem, Health>(true);
		_ecs.SetSystemComponentArchetypeState<SpriteSystem, Sprite>(true);

		// create number of required entities
		for (int i = 0; i < Entities; i++)
		{
			int e = _ecs.CreateEntity();
			_ecs.AddComponent<Position>(e, new Position());
			_ecs.AddComponent<Velocity>(e, new Velocity());
			_ecs.AddComponent<DataComponent>(e, new DataComponent());
			_ecs.AddComponent<Health>(e, new Health());
			_ecs.AddComponent<Damage>(e, new Damage());
			_ecs.AddComponent<Sprite>(e, new Sprite());

			_entities.Add(e);
		}
	}

	public void Run()
	{
		DateTime lastUpdate = DateTime.Now;
		float deltaTime = 0;

		DateTime lastFrameCount = DateTime.Now;
		int frames = 0;
		int fps = 0;

		PackedArray<int> fpsSamples = new();

		Stopwatch stopWatch = new Stopwatch();

		float elapsedTime = 0;

		while (true)
		{
			DateTime timeNow = DateTime.Now;
			deltaTime = (timeNow.Ticks - lastUpdate.Ticks) / 10000000f;
			// stopWatch.Start();

			_ecs._Process(deltaTime);

			// stopWatch.Stop();

			// deltaTime = ((float) stopWatch.ElapsedMilliseconds) / 1000f;
			elapsedTime += deltaTime;

			// stopWatch.Reset();

			lastUpdate = timeNow;

			// updates fps
			frames++;

			if ((timeNow - lastFrameCount).TotalSeconds >= 1)
			{
				fps = frames;
				fpsSamples.Add(fps);

				LoggerManager.LogInfo("FPS", "", "fps", $"{fps} @ {Entities.ToString()}e (avg:{Convert.ToInt32(fpsSamples.Span.ToArray().TakeLast(50).Average())}) [({deltaTime * 1000}ms) ({deltaTime * 1000000}us) ({deltaTime * 1000000000}ns)] cpe:{(deltaTime * 1000) / Entities}ms)");

				frames = 0;

				lastFrameCount = timeNow;
				lastUpdate = lastFrameCount;
			}


			// if (elapsedTime >= 1)
			// {
			// 	fps = frames;
			// 	fpsSamples.Add(fps);
            //
			// 	LoggerManager.LogInfo("FPS", "", "fps", $"{fps} @ {Entities.ToString()}e (avg:{Convert.ToInt32(fpsSamples.Span.ToArray().TakeLast(50).Average())}) [({deltaTime * 1000}ms) ({deltaTime * 1000000}us) ({deltaTime * 1000000000}ns)] cpe:{(deltaTime * 1000) / Entities}ms)");
            //
			// 	frames = 0;
			// 	elapsedTime = 0;
            //
			// 	// lastFrameCount = timeNow;
			// 	// lastUpdate = lastFrameCount;
			// }

		}
	}

	public void Update(double deltaTime)
	{
		_ecs._Process(deltaTime);
		// foreach (var entity in _entities.Span)
		// {
		// 	LoggerManager.LogInfo("Position", entity.ToString(), "position", _ecs.GetComponent<Position>(entity));
		// }
	}
}

public partial class ECSProfile_Update_6 : ProfileBase
{
	public ECSProfile_Update_6(int entities, bool run = true) : base(entities, run)
	{
		ProfileType = UpdateProfileType.Update7Systems;
	}
}
