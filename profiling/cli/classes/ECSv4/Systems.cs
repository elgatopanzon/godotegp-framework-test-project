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
	public void Update(SystemInstance system, double deltaTime, ECS core, Query query)
	{
		ComponentArray<Position> position = query.Results.GetComponents<Position>();
		ComponentArray<Velocity> velocity = query.Results.GetComponents<Velocity>();

		foreach (var entity in query.Results.Entities.Values)
		{
			position.Data[entity.Id].X += (velocity.Data[entity.Id].X * deltaTime);
			position.Data[entity.Id].Y += (velocity.Data[entity.Id].Y * deltaTime);
		}
	}
}

// control entity state based on hp
public struct HealthSystem : ISystem
{
	public void Update(SystemInstance system, double deltaTime, ECS core, Query query)
	{
		ComponentArray<Health> health = query.Results.GetComponents<Health>();

		foreach (var entity in query.Results.Entities.Values)
		{
			if (health.Data[entity.Id].Hp <= 0 && health.Data[entity.Id].Status != 0) 
			{
      			health.Data[entity.Id].Hp = 0;
      			health.Data[entity.Id].Status = 0;
    		} 
    		else if (health.Data[entity.Id].Status == 0 && health.Data[entity.Id].Hp == 0)
    		{
      			health.Data[entity.Id].Hp = health.Data[entity.Id].HpMax;
      			health.Data[entity.Id].Status = 1;
    		} 
    		else if (health.Data[entity.Id].Hp >= health.Data[entity.Id].HpMax && health.Data[entity.Id].Status != 2)
    		{
      			health.Data[entity.Id].Hp = health.Data[entity.Id].HpMax;
      			health.Data[entity.Id].Status = 2;
    		} 
    		else 
    		{
      			health.Data[entity.Id].Status = 2;
    		}
    	}
	}
}

// calculate damage and deal damage to health component
public struct DamageSystem : ISystem
{
	public void Update(SystemInstance system, double deltaTime, ECS core, Query query)
	{
		ComponentArray<Damage> damage = query.Results.GetComponents<Damage>();
		ComponentArray<Health> health = query.Results.GetComponents<Health>();
		ComponentArray<DataComponent> dataComponent = query.Results.GetComponents<DataComponent>();

		foreach (var entity in query.Results.Entities.Values)
		{
			int total = damage.Data[entity.Id].Attack - damage.Data[entity.Id].Defense;
			if (total <= 0)
			{
				damage.Data[entity.Id].Attack = Random.Shared.Next(10, 40);
				damage.Data[entity.Id].Defense = Random.Shared.Next(10, 40);
				health.Data[entity.Id].HpMax = Random.Shared.Next(100, 200);
			}

			if (health.Data[entity.Id].Hp > 0 && total > 0)
			{
				health.Data[entity.Id].Hp = Math.Max(0, health.Data[entity.Id].Hp - total);
			}
		}
	}
}

// randomly update some data values
public struct DataSystem : ISystem
{
	public void Update(SystemInstance system, double deltaTime, ECS core, Query query)
	{
		ComponentArray<DataComponent> data = query.Results.GetComponents<DataComponent>();

		foreach (var entity in query.Results.Entities.Values)
		{
			data.Data[entity.Id].RandomInt = Random.Shared.Next();
			data.Data[entity.Id].RandomDouble = Random.Shared.NextDouble();
		}
	}
}


// direction system randomly updates direction based on data
public struct DirectionSystem : ISystem
{
	public void Update(SystemInstance system, double deltaTime, ECS core, Query query)
	{
		ComponentArray<Position> position = query.Results.GetComponents<Position>();
		ComponentArray<Velocity> velocity = query.Results.GetComponents<Velocity>();
		ComponentArray<DataComponent> data = query.Results.GetComponents<DataComponent>();

		foreach (var entity in query.Results.Entities.Values)
		{
			if (data.Data[entity.Id].RandomInt % 10 == 0)
			{
				if (position.Data[entity.Id].X > position.Data[entity.Id].Y)
				{
					velocity.Data[entity.Id].X = Random.Shared.Next(3, 19) - 10;
					velocity.Data[entity.Id].Y = Random.Shared.Next(0, 5);
				}
				else
				{
					velocity.Data[entity.Id].X = Random.Shared.Next(0, 5);
					velocity.Data[entity.Id].Y = Random.Shared.Next(3, 19) - 10;
				}
			}
		}
	}
}

// update sprite type based on character status and health
public struct SpriteSystem : ISystem
{
	public void Update(SystemInstance system, double deltaTime, ECS core, Query query)
	{
		ComponentArray<Damage> damage = query.Results.GetComponents<Damage>();
		ComponentArray<Health> health = query.Results.GetComponents<Health>();
		ComponentArray<Sprite> sprite = query.Results.GetComponents<Sprite>();

		foreach (var entity in query.Results.Entities.Values)
		{
			switch (health.Data[entity.Id].Status)
			{
				case 0:
					sprite.Data[entity.Id].SpriteId = '_';
					break;
				case 1:
					sprite.Data[entity.Id].SpriteId = '/';
					break;
				case 2:
					if (health.Data[entity.Id].Hp == health.Data[entity.Id].HpMax)
					{
						sprite.Data[entity.Id].SpriteId = '+';
					}
					else {
						sprite.Data[entity.Id].SpriteId = '|';
					}
					break;
				default:
					break;
			}
		}
	}
}
