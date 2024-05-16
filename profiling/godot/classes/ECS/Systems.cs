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
			ref Position position = ref _ecs.GetComponent<Position>(0, entity);
			ref Velocity velocity = ref _ecs.GetComponent<Velocity>(1, entity);

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
			ref Health health = ref _ecs.GetComponent<Health>(3, entity);
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
			ref Damage damage = ref _ecs.GetComponent<Damage>(4, entity);
			ref Health health = ref _ecs.GetComponent<Health>(3, entity);
			ref DataComponent dataComponent = ref _ecs.GetComponent<DataComponent>(2, entity);

			if (damage.Attack == 0 || damage.Defense == 0)
			{
				damage.Attack = dataComponent.RNG.RandiRange(10, 40);
				damage.Defense = dataComponent.RNG.RandiRange(10, 40);
				health.HpMax = dataComponent.RNG.RandiRange(100, 200);
			}

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
			ref DataComponent data = ref _ecs.GetComponent<DataComponent>(2, entity);
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
			ref Position position = ref _ecs.GetComponent<Position>(0, entity);
			ref Velocity velocity = ref _ecs.GetComponent<Velocity>(1, entity);
			ref DataComponent data = ref _ecs.GetComponent<DataComponent>(2, entity);

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
			ref Damage damage = ref _ecs.GetComponent<Damage>(4, entity);
			ref Health health = ref _ecs.GetComponent<Health>(3, entity);
			ref Sprite sprite = ref _ecs.GetComponent<Sprite>(5, entity);

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
