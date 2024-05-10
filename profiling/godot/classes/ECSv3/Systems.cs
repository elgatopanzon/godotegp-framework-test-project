/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : Systems
 * @created     : Wednesday May 08, 2024 23:32:04 CST
 */

namespace GodotEGP.Profiling.CLI.ECSv3;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.ECSv3;
using GodotEGP.ECSv3.Systems;

using System;

// update position based on velocity
public struct MovementSystem : ISystem
{
	public void Update(Entity entity, int index, SystemInstance system, ECS core)
	{
		ref Position position = ref core.Get<Position>(entity);
		ref Velocity velocity = ref core.Get<Velocity>(entity);

		position.X += (velocity.X * system.DeltaTime);
		position.Y += (velocity.Y * system.DeltaTime);
	}
}

// control entity state based on hp
public struct HealthSystem : ISystem
{
	public void Update(Entity entity, int index, SystemInstance system, ECS core)
	{
		ref Health health = ref core.Get<Health>(entity);
		if (health.Hp <= 0 && health.Status != 0) 
		{
      		health.Hp = 0;
      		health.Status = 0;
    	} 
    	else if (health.Status == 0 && health.Hp == 0)
    	{
      		health.Hp = health.HpMax;
      		health.Status = 1;
    	} 
    	else if (health.Hp >= health.HpMax && health.Status != 2)
    	{
      		health.Hp = health.HpMax;
      		health.Status = 2;
    	} 
    	else 
    	{
      		health.Status = 2;
    	}
	}
}

// calculate damage and deal damage to health component
public struct DamageSystem : ISystem
{
	public void Update(Entity entity, int index, SystemInstance system, ECS core)
	{
		ref Damage damage = ref core.Get<Damage>(entity);
		ref Health health = ref core.Get<Health>(entity);

		int total = damage.Attack - damage.Defense;
		if (health.Hp > 0 && total > 0)
		{
			health.Hp = Math.Max(0, health.Hp - total);
		}
	}
}

// randomly update some data values
public struct DataSystem : ISystem
{
	public void Update(Entity entity, int index, SystemInstance system, ECS core)
	{
		ref DataComponent data = ref core.Get<DataComponent>(entity);
		data.RandomInt = (int) data.RNG.Randi();
		data.RandomDouble = (double) data.RNG.Randf();
	}
}


// direction system randomly updates direction based on data
public struct DirectionSystem : ISystem
{
	public void Update(Entity entity, int index, SystemInstance system, ECS core)
	{
		ref Position position = ref core.Get<Position>(entity);
		ref Velocity velocity = ref core.Get<Velocity>(entity);
		ref DataComponent data = ref core.Get<DataComponent>(entity);

		if (data.RandomInt % 10 == 0)
		{
			if (position.X > position.Y)
			{
				velocity.X = data.RNG.RandfRange(3, 19) - 10;
				velocity.Y = data.RNG.RandfRange(0, 5);
			}
			else
			{
				velocity.X = data.RNG.RandfRange(0, 5);
				velocity.Y = data.RNG.RandfRange(3, 19) - 10;
			}
		}
	}
}

// update sprite type based on character status and health
public struct SpriteSystem : ISystem
{
	public void Update(Entity entity, int index, SystemInstance system, ECS core)
	{
		ref Damage damage = ref core.Get<Damage>(entity);
		ref Health health = ref core.Get<Health>(entity);
		ref Sprite sprite = ref core.Get<Sprite>(entity);

		switch (health.Status)
		{
			case 0:
				sprite.SpriteId = "_";
				break;
			case 1:
				sprite.SpriteId = "/";
				break;
			case 2:
				if (health.Hp == health.HpMax)
				{
					sprite.SpriteId = "+";
				}
				else {
					sprite.SpriteId = "|";
				}
				break;
			default:
				break;
		}
	}
}
