/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : Profiles
 * @created     : Wednesday May 08, 2024 22:41:19 CST
 */

namespace GodotEGP.Profiling.CLI.ECSv4;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;
using GodotEGP.Collections;

using GodotEGP.Profiling.CLI;
using GodotEGP.ECSv4;
using GodotEGP.ECSv4.Components;
using GodotEGP.ECSv4.Queries;
using GodotEGP.ECSv4.Systems;

using System;
using System.Linq;
using System.Diagnostics;

public enum UpdateProfileType
{
	Update7Systems = 0,
}

public partial class ProfileBase : ProfilingContext
{
	public ulong Entities { get; set; }
	public UpdateProfileType ProfileType { get; set; }

	private ECS _ecs;
	private PackedArray<Entity> _entities;

	public ProfileBase(ulong entities, bool run = true)
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
		_ecs = new ECS();

		// register components
		_ecs.RegisterComponent<Position>();
		_ecs.RegisterComponent<Velocity>();
		_ecs.RegisterComponent<DataComponent>();
		_ecs.RegisterComponent<Health>();
		_ecs.RegisterComponent<Damage>();
		_ecs.RegisterComponent<Sprite>();

		// register systems with queries
		_ecs.RegisterSystem<MovementSystem, OnUpdatePhase>(_ecs.CreateQuery()
				.Has<Position>()
				.Has<Velocity>()
				.Build()
			);
		_ecs.RegisterSystem<HealthSystem, PostUpdatePhase>(_ecs.CreateQuery()
				.Has<Health>()
				.Build()
			);
		_ecs.RegisterSystem<DamageSystem, OnUpdatePhase>(_ecs.CreateQuery()
				.Has<Health>()
				.Has<Damage>()
				.Has<DataComponent>()
				.Build()
			);
		_ecs.RegisterSystem<DataSystem, FinalPhase>(_ecs.CreateQuery()
				.Has<DataComponent>()
				.Build()
			);
		_ecs.RegisterSystem<DirectionSystem, OnStartupPhase>(_ecs.CreateQuery()
				.Has<Position>()
				.Has<Velocity>()
				.Has<DataComponent>()
				.Build()
			);
		_ecs.RegisterSystem<SpriteSystem, FinalPhase>(_ecs.CreateQuery()
				.Has<Damage>()
				.Has<Health>()
				.Has<Sprite>()
				.Build()
			);

		// create number of required entities
		for (ulong i = 0; i < Entities; i++)
		{
			EntityHandle e = _ecs.Create($"entity_{i+1}");
			e.Set<Position>(new Position());
			e.Set<Velocity>(new Velocity());
			e.Set<Health>(new Health());
			e.Set<Damage>(new Damage());
			e.Set<DataComponent>(new DataComponent());
			e.Set<Sprite>(new Sprite());
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

			_ecs.Update(deltaTime);

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
		_ecs.Update(deltaTime);
	}
}

public partial class ECSv4Profile_Update_6 : ProfileBase
{
	public ECSv4Profile_Update_6(ulong entities, bool run = true) : base(entities, run)
	{
		ProfileType = UpdateProfileType.Update7Systems;
	}
}
