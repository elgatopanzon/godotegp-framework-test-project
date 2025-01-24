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

using Raylib_cs;

// update position based on velocity
public struct MovementSystem : ISystem
{
	public void Update(SystemInstance system, double deltaTime, ECS core, Query query)
	{
		ComponentArray<Position> position = query.Results.GetComponents<Position>();
		ComponentArray<Velocity> velocity = query.Results.GetComponents<Velocity>();

		foreach (var entity in query.Results.Entities.Values)
		{
			position.Data[entity].X += (velocity.Data[entity].X * deltaTime);
			position.Data[entity].Y += (velocity.Data[entity].Y * deltaTime);
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
			if (health.Data[entity].Hp <= 0 && health.Data[entity].Status != 0) 
			{
      			health.Data[entity].Hp = 0;
      			health.Data[entity].Status = 0;
    		} 
    		else if (health.Data[entity].Status == 0 && health.Data[entity].Hp == 0)
    		{
      			health.Data[entity].Hp = health.Data[entity].HpMax;
      			health.Data[entity].Status = 1;
    		} 
    		else if (health.Data[entity].Hp >= health.Data[entity].HpMax && health.Data[entity].Status != 2)
    		{
      			health.Data[entity].Hp = health.Data[entity].HpMax;
      			health.Data[entity].Status = 2;
    		} 
    		else 
    		{
      			health.Data[entity].Status = 2;
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
			int total = damage.Data[entity].Attack - damage.Data[entity].Defense;
			if (total <= 0)
			{
				damage.Data[entity].Attack = Random.Shared.Next(10, 40);
				damage.Data[entity].Defense = Random.Shared.Next(10, 40);
				health.Data[entity].HpMax = Random.Shared.Next(100, 200);
			}

			if (health.Data[entity].Hp > 0 && total > 0)
			{
				health.Data[entity].Hp = Math.Max(0, health.Data[entity].Hp - total);
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
			data.Data[entity].RandomInt = Random.Shared.Next();
			data.Data[entity].RandomDouble = Random.Shared.NextDouble();
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
			if (data.Data[entity].RandomInt % 10 == 0)
			{
				if (position.Data[entity].X > position.Data[entity].Y)
				{
					velocity.Data[entity].X = Random.Shared.Next(3, 19) - 10;
					velocity.Data[entity].Y = Random.Shared.Next(0, 5);
				}
				else
				{
					velocity.Data[entity].X = Random.Shared.Next(0, 5);
					velocity.Data[entity].Y = Random.Shared.Next(3, 19) - 10;
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
			switch (health.Data[entity].Status)
			{
				case 0:
					sprite.Data[entity].SpriteId = '_';
					break;
				case 1:
					sprite.Data[entity].SpriteId = '/';
					break;
				case 2:
					if (health.Data[entity].Hp == health.Data[entity].HpMax)
					{
						sprite.Data[entity].SpriteId = '+';
					}
					else {
						sprite.Data[entity].SpriteId = '|';
					}
					break;
				default:
					break;
			}
		}
	}
}


public struct PreRenderSystem : ISystem
{
	public void Update(SystemInstance system, double deltaTime, ECS core, Query query)
	{
		Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.White);
	}
}

// render sprite character in raylib
public struct RenderSystem : ISystem
{
	public void Update(SystemInstance system, double deltaTime, ECS core, Query query)
	{
		ComponentArray<Position> position = query.Results.GetComponents<Position>();
		ComponentArray<Health> health = query.Results.GetComponents<Health>();
		ComponentArray<Sprite> sprite = query.Results.GetComponents<Sprite>();

		foreach (var entity in query.Results.Entities.Values)
		{
			string renderString = $"{sprite.Data[entity].SpriteId.ToString()}";

			Raylib.DrawText(renderString.ToString(), (int) (position.Data[entity].X * 10), (int) (position.Data[entity].Y * 10), 20, Color.Black);
		}
	}
}

public struct PostRenderSystem : ISystem
{
	public void Update(SystemInstance system, double deltaTime, ECS core, Query query)
	{
		Raylib.EndDrawing();
	}
}
