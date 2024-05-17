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
using GodotEGP.ECSv4.Queries;

using System;

// update position based on velocity
public struct MovementSystem : ISystem
{
	public void Update(Entity entity, int index, SystemInstance system, ECS core, Query query)
	{
		ref Position position = ref query.GetComponent<Position>(entity);
		ref Velocity velocity = ref query.GetComponent<Velocity>(entity);

		position.X += (velocity.X * system.DeltaTime);
		position.Y += (velocity.Y * system.DeltaTime);
	}
}

// control entity state based on hp
public struct HealthSystem : ISystem
{
	public void Update(Entity entity, int index, SystemInstance system, ECS core, Query query)
	{
		ref Health health = ref query.GetComponent<Health>(entity);
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
	public void Update(Entity entity, int index, SystemInstance system, ECS core, Query query)
	{
		ref Damage damage = ref query.GetComponent<Damage>(entity);
		ref Health health = ref query.GetComponent<Health>(entity);
		ref DataComponent dataComponent = ref query.GetComponent<DataComponent>(entity);

		if (damage.Attack == 0 || damage.Defense == 0)
		{
			damage.Attack = dataComponent.RNG.RandiRange(10, 40);
			damage.Defense = dataComponent.RNG.RandiRange(10, 40);
			health.HpMax = dataComponent.RNG.RandiRange(100, 200);
		}

		if (health.Status == 0)
		{
			ref Position position = ref core.Get<Position>(entity);
			damage.Attack = Math.Max(1, Convert.ToInt32(position.X * 10));
			damage.Defense = Math.Max(1, Convert.ToInt32(position.Y * 10));
		}

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
	public void Update(Entity entity, int index, SystemInstance system, ECS core, Query query)
	{
		ref DataComponent data = ref query.GetComponent<DataComponent>(entity);
		data.RandomInt = (int) data.RNG.Randi();
		data.RandomDouble = (double) data.RNG.Randf();
	}
}


// direction system randomly updates direction based on data
public struct DirectionSystem : ISystem
{
	public void Update(Entity entity, int index, SystemInstance system, ECS core, Query query)
	{
		ref Position position = ref query.GetComponent<Position>(entity);
		ref Velocity velocity = ref query.GetComponent<Velocity>(entity);
		ref DataComponent data = ref query.GetComponent<DataComponent>(entity);

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
	public void Update(Entity entity, int index, SystemInstance system, ECS core, Query query)
	{
		ref Damage damage = ref query.GetComponent<Damage>(entity);
		ref Health health = ref query.GetComponent<Health>(entity);
		ref Sprite sprite = ref query.GetComponent<Sprite>(entity);

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
