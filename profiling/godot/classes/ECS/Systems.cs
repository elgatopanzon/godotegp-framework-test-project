/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : Systems
 * @created     : Friday May 10, 2024 17:00:36 CST
 */

namespace GodotEGP.Profiling.CLI.ECS;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.ECS;

public partial class MovementSystem : SystemBase
{
	public override void _Process(double deltaTime)
	{
		foreach (int entity in _entities)
		{
			ref Position position = ref _ecs.GetComponent<Position>(entity);
			ref Velocity velocity = ref _ecs.GetComponent<Velocity>(entity);

			position.X += (velocity.X * deltaTime);
			position.Y += (velocity.Y * deltaTime);
		}
	}
}

public partial class HealthSystem : SystemBase
{
	public override void _Process(double deltaTime)
	{
		foreach (int entity in _entities)
		{
			ref Health health = ref _ecs.GetComponent<Health>(entity);
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
}

public partial class DamageSystem : SystemBase
{
	public override void _Process(double deltaTime)
	{
		foreach (int entity in _entities)
		{
			ref Damage damage = ref _ecs.GetComponent<Damage>(entity);
			ref Health health = ref _ecs.GetComponent<Health>(entity);

			int total = damage.Attack - damage.Defense;
			if (health.Hp > 0 && total > 0)
			{
				health.Hp = System.Math.Max(0, health.Hp - total);
			}
		}
	}
}

public partial class DataSystem : SystemBase
{
	public override void _Process(double deltaTime)
	{
		foreach (int entity in _entities)
		{
			ref DataComponent data = ref _ecs.GetComponent<DataComponent>(entity);
			data.RandomInt = (int) data.RNG.Randi();
			data.RandomDouble = (double) data.RNG.Randf();
		}
	}
}


public partial class DirectionSystem : SystemBase
{
	public override void _Process(double deltaTime)
	{
		foreach (int entity in _entities)
		{
			ref Position position = ref _ecs.GetComponent<Position>(entity);
			ref Velocity velocity = ref _ecs.GetComponent<Velocity>(entity);
			ref DataComponent data = ref _ecs.GetComponent<DataComponent>(entity);

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
}

public partial class SpriteSystem : SystemBase
{
	public override void _Process(double deltaTime)
	{
		foreach (int entity in _entities)
		{
			ref Damage damage = ref _ecs.GetComponent<Damage>(entity);
			ref Health health = ref _ecs.GetComponent<Health>(entity);
			ref Sprite sprite = ref _ecs.GetComponent<Sprite>(entity);

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
}
