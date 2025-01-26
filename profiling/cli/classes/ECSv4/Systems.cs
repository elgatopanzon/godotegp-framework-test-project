/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : Systems
 * @created     : Wednesday May 08, 2024 23:32:04 CST
 */

namespace GodotEGP.Profiling.CLI.ECSv4;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.ECSv4;
using GodotEGP.ECSv4.Systems;
using GodotEGP.ECSv4.Components;
using GodotEGP.ECSv4.Queries;

using System;

// update position based on velocity
public struct MovementSystem : IEcsSystem
{
	public static void Update(double deltaTimeSys, double deltaTime, ECS core, Query query)
	{
		ComponentStore<Position> position = core.GetComponentStore<Position>();
		ComponentStore<Velocity> velocity = core.GetComponentStore<Velocity>();

		foreach (var entity in query.Entities.Entities)
		{
			position.GetMutable(entity).X += (velocity.GetMutable(entity).X * deltaTime);
			position.GetMutable(entity).Y += (velocity.GetMutable(entity).Y * deltaTime);
		}
	}
}

// control entity state based on hp
public struct HealthSystem : IEcsSystem
{
	public static void Update(double deltaTimeSys, double deltaTime, ECS core, Query query)
	{
		ComponentStore<Health> health = core.GetComponentStore<Health>();

		foreach (var entity in query.Entities.Entities)
		{
			if (health.GetMutable(entity).Hp <= 0 && health.GetMutable(entity).Status != 0) 
			{
      			health.GetMutable(entity).Hp = 0;
      			health.GetMutable(entity).Status = 0;
    		} 
    		else if (health.GetMutable(entity).Status == 0 && health.GetMutable(entity).Hp == 0)
    		{
      			health.GetMutable(entity).Hp = health.GetMutable(entity).HpMax;
      			health.GetMutable(entity).Status = 1;
    		} 
    		else if (health.GetMutable(entity).Hp >= health.GetMutable(entity).HpMax && health.GetMutable(entity).Status != 2)
    		{
      			health.GetMutable(entity).Hp = health.GetMutable(entity).HpMax;
      			health.GetMutable(entity).Status = 2;
    		} 
    		else 
    		{
      			health.GetMutable(entity).Status = 2;
    		}
    	}
	}
}

// calculate damage and deal damage to health component
public struct DamageSystem : IEcsSystem
{
	public static void Update(double deltaTimeSys, double deltaTime, ECS core, Query query)
	{
		ComponentStore<Damage> damage = core.GetComponentStore<Damage>();
		ComponentStore<Health> health = core.GetComponentStore<Health>();
		ComponentStore<DataComponent> dataComponent = core.GetComponentStore<DataComponent>();

		foreach (var entity in query.Entities.Entities)
		{
			int total = damage.GetMutable(entity).Attack - damage.GetMutable(entity).Defense;
			if (total <= 0)
			{
				damage.GetMutable(entity).Attack = Random.Shared.Next(10, 40);
				damage.GetMutable(entity).Defense = Random.Shared.Next(10, 40);
				health.GetMutable(entity).HpMax = Random.Shared.Next(100, 200);
			}

			if (health.GetMutable(entity).Hp > 0 && total > 0)
			{
				health.GetMutable(entity).Hp = Math.Max(0, health.GetMutable(entity).Hp - total);
			}
		}
	}
}

// randomly update some data values
public struct DataSystem : IEcsSystem
{
	public static void Update(double deltaTimeSys, double deltaTime, ECS core, Query query)
	{
		ComponentStore<DataComponent> data = core.GetComponentStore<DataComponent>();

		foreach (var entity in query.Entities.Entities)
		{
			data.GetMutable(entity).RandomInt = Random.Shared.Next();
			data.GetMutable(entity).RandomDouble = Random.Shared.NextDouble();
		}
	}
}


// direction system randomly updates direction based on data
public struct DirectionSystem : IEcsSystem
{
	public static void Update(double deltaTimeSys, double deltaTime, ECS core, Query query)
	{
		ComponentStore<Position> position = core.GetComponentStore<Position>();
		ComponentStore<Velocity> velocity = core.GetComponentStore<Velocity>();
		ComponentStore<DataComponent> data = core.GetComponentStore<DataComponent>();

		foreach (var entity in query.Entities.Entities)
		{
			if (data.GetMutable(entity).RandomInt % 10 == 0)
			{
				if (position.GetMutable(entity).X > position.GetMutable(entity).Y)
				{
					velocity.GetMutable(entity).X = Random.Shared.Next(3, 19) - 10;
					velocity.GetMutable(entity).Y = Random.Shared.Next(0, 5);
				}
				else
				{
					velocity.GetMutable(entity).X = Random.Shared.Next(0, 5);
					velocity.GetMutable(entity).Y = Random.Shared.Next(3, 19) - 10;
				}
			}
		}
	}
}

// update sprite type based on character status and health
public struct SpriteSystem : IEcsSystem
{
	public static void Update(double deltaTimeSys, double deltaTime, ECS core, Query query)
	{
		ComponentStore<Damage> damage = core.GetComponentStore<Damage>();
		ComponentStore<Health> health = core.GetComponentStore<Health>();
		ComponentStore<Sprite> sprite = core.GetComponentStore<Sprite>();

		foreach (var entity in query.Entities.Entities)
		{
			switch (health.GetMutable(entity).Status)
			{
				case 0:
					sprite.GetMutable(entity).SpriteId = '_';
					break;
				case 1:
					sprite.GetMutable(entity).SpriteId = '/';
					break;
				case 2:
					if (health.GetMutable(entity).Hp == health.GetMutable(entity).HpMax)
					{
						sprite.GetMutable(entity).SpriteId = '+';
					}
					else {
						sprite.GetMutable(entity).SpriteId = '|';
					}
					break;
				default:
					break;
			}
		}
	}
}
