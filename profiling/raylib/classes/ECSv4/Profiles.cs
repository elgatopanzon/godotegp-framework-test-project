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

	public ProfileBase(ulong entities)
	{
		Entities = entities;

		Setup();
		Run();
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
		_ecs.RegisterComponent<Render>();

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
		_ecs.RegisterSystem<PreRenderSystem, OnStartupPhase>(_ecs.CreateQuery()
				.Has<Render>()
				.Build()
			);
		_ecs.RegisterSystem<RenderSystem, PostUpdatePhase>(_ecs.CreateQuery()
				.Has<Health>()
				.Has<Position>()
				.Has<Sprite>()
				.Build()
			);
		_ecs.RegisterSystem<PostRenderSystem, FinalPhase>(_ecs.CreateQuery()
				.Has<Render>()
				.Build()
			);

		// render entity to trigger pre/post render steps
		EntityHandle render_e = _ecs.Create($"render");
		render_e.Set<Render>(new Render());

		// create number of required entities
		for (ulong i = 0; i < Entities; i++)
		{
			EntityHandle e = _ecs.Create($"entity_{i+1}");
		}

		// add components in contiguous order
		for (ulong i = 0; i < Entities; i++)
		{
			EntityHandle e = _ecs.Create($"entity_{i+1}");
			e.Set<Position>(new Position());
		}
		for (ulong i = 0; i < Entities; i++)
		{
			EntityHandle e = _ecs.Create($"entity_{i+1}");
			e.Set<Velocity>(new Velocity());
		}
		for (ulong i = 0; i < Entities; i++)
		{
			EntityHandle e = _ecs.Create($"entity_{i+1}");
			e.Set<Health>(new Health());
		}
		for (ulong i = 0; i < Entities; i++)
		{
			EntityHandle e = _ecs.Create($"entity_{i+1}");
			e.Set<Damage>(new Damage());
		}
		for (ulong i = 0; i < Entities; i++)
		{
			EntityHandle e = _ecs.Create($"entity_{i+1}");
			e.Set<DataComponent>(new DataComponent());
		}
		for (ulong i = 0; i < Entities; i++)
		{
			EntityHandle e = _ecs.Create($"entity_{i+1}");
			e.Set<Sprite>(new Sprite());
		}
	}

	public void Run()
	{
		float deltaTime = 0;
		float deltaTimeTotal = 0;

		int frames = 0;
		int fps = 0;

		PackedArray<int> fpsSamples = new();

		float elapsedTime = 0;
		long prevTicks = Stopwatch.GetTimestamp();

		while (true)
		{
			_ecs.Update(deltaTime);

			long ticks = Stopwatch.GetTimestamp() - prevTicks;
			prevTicks += ticks;
			deltaTime = (float) ((double) ticks / Stopwatch.Frequency);

			// deltaTime = ((float) ts.TotalSeconds - deltaTimeTotal);
			deltaTimeTotal += deltaTime;
			elapsedTime += deltaTime;

			// updates frame counter
			frames++;

			// log FPS after a second passed
			if (elapsedTime >= 1)
			{
				fps = frames;
				fpsSamples.Add(fps);

				LoggerManager.LogInfo("FPS", "", "fps", $"{fps} @ {Entities.ToString()}e (avg:{Convert.ToInt32(fpsSamples.Span.ToArray().TakeLast(50).Average())}) [({deltaTime * 1000}ms) ({deltaTime * 1000000}us) ({deltaTime * 1000000000}ns)] cpe:{(int) ((deltaTime * 1000000000) / Entities)}ns)");

				frames = 0;

				elapsedTime = 0;
			}
		}
	}

}

public partial class ECSv4Profile_Update_6 : ProfileBase
{
	public ECSv4Profile_Update_6(ulong entities) : base(entities)
	{
		ProfileType = UpdateProfileType.Update7Systems;
	}
}
