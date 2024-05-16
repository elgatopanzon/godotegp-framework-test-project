/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ClassVsStructBenchmarks
 * @created     : Wednesday May 15, 2024 22:17:07 CST
 */

namespace GodotEGP.Benchmarks.Benchmarks;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.Random;
using GodotEGP.Collections;
using GodotEGP.ECSv4.Components;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

public partial class ClassVsStructBenchmarks
{
#if (!DEBUG)
	[Fact]
	public void ClassVsStructBenchmarks_Update()
	{
		BenchmarkRunner.Run<ClassVsStructBenchmarks_Update>();
	}
#endif
}

/***********************
*  Struct components  *
***********************/

public struct Position : IComponentData
{
	public static int Id { get; set; }
	public double X;
	public double Y;
}

public struct Velocity : IComponentData
{
	public static int Id { get; set; }
	public double X;
	public double Y;
}

public struct DataComponent : IComponentData
{
	public static int Id { get; set; }
	public int Type;
	public int RandomInt;
	public double RandomDouble;
	public NumberGenerator RNG { get; } = new();

	public DataComponent()
	{
		RNG.Randomize();
	}
}

public struct Health : IComponentData
{
	public static int Id { get; set; }
	public int Hp;
	public int HpMax;
	public int Status;
}

public struct Damage : IComponentData
{
	public static int Id { get; set; }
	public int Attack;
	public int Defense;
}

public struct Sprite : IComponentData
{
	public static int Id { get; set; }
	public CharBuffer<Buffer16<char>> SpriteId;
}

public struct DebugSpeed : IComponentData
{
	public static int Id { get; set; }
	public CharBuffer<Buffer128<char>> SpeedText;
}

public struct DebugData : IComponentData
{
	public static int Id { get; set; }
	public CharBuffer<Buffer128<char>> DataText;
}

public struct Robot : IComponent { public static int Id { get; set; } }

/***********************************
*  OOP entity class + components  *
***********************************/

public interface IActor
{
	public void Update();
}

public class Actor : IActor
{
	public virtual DataComponentOOP DataComponent { get; set; } = new();

	public virtual void Update()
	{
		DataComponent.Update();
	}
}


public class MovableActor : Actor
{
	public virtual PositionOOP Position { get; set; } = new PositionOOP();
	public virtual VelocityOOP Velocity { get; set; } = new VelocityOOP();

	public override void Update()
	{
		// update the moving parts
		Position.Update(Velocity);
		Velocity.Update(DataComponent, Position);

		// update the base
		base.Update();
	}
}

public class DamagableActor : MovableActor
{
	public virtual HealthOOP Health { get; set; } = new HealthOOP();
	public virtual DamageOOP Damage { get; set; } = new DamageOOP();

	public override void Update()
	{
		// update the renderables
		Health.Update();
		Damage.Update(Health, DataComponent);

		// update the base
		base.Update();
	}
}

public class NonRenderableActor : MovableActor
{
	public virtual DebugSpeedOOP DebugSpeed { get; set; } = new DebugSpeedOOP();

	public override void Update()
	{
		// update the debug speed
		DebugSpeed.Update(Position, Velocity);

		// update the base
		base.Update();
	}
}

public class NonRenderableActor2 : MovableActor
{
	public virtual DebugDataOOP DebugData { get; set; } = new DebugDataOOP();

	public override void Update()
	{
		// update the debug speed
		DebugData.Update(DataComponent);

		// update the base
		base.Update();
	}
}

public class RenderableActor : DamagableActor
{
	public virtual SpriteOOP Sprite { get; set; } = new SpriteOOP();

	public override void Update()
	{
		// update the renderables
		Sprite.Update(this.Health);

		// update the base
		base.Update();
	}
}

public class RobotActor : RenderableActor
{
	public override VelocityOOP Velocity { get; set; } = new RobotVelocityOOP();
	public override void Update()
	{
		// update the renderables
		Sprite.Update(this.Health);

		// update the base
		base.Update();
	}
}


public class PositionOOP : IComponentData
{
	public static int Id { get; set; }
	public double X;
	public double Y;
	
	public virtual void Update(VelocityOOP velocity)
	{
		X += (velocity.X * 0.0166);
		Y += (velocity.Y * 0.0166);
	}
}

public class VelocityOOP : IComponentData
{
	public static int Id { get; set; }
	public double X;
	public double Y;

	public virtual void Update(DataComponentOOP dataComponent, PositionOOP position)
	{
		if (dataComponent.RandomInt % 10 == 0)
		{
			if (position.X > position.Y)
			{
				X = dataComponent.RNG.RandfRange(3, 19) - 10;
				Y = dataComponent.RNG.RandfRange(0, 5);
			}
			else
			{
				X = dataComponent.RNG.RandfRange(0, 5);
				Y = dataComponent.RNG.RandfRange(3, 19) - 10;
			}
		}
	}
}

public class DataComponentOOP : IComponentData
{
	public static int Id { get; set; }
	public int Type;
	public int RandomInt;
	public double RandomDouble;
	public NumberGenerator RNG { get; } = new();

	public DataComponentOOP()
	{
		RNG.Randomize();
	}

	public virtual void Update()
	{
		RandomInt = RNG.Randi();
		RandomDouble = (double) RNG.Randf();
	}
}

public class HealthOOP : IComponentData
{
	public static int Id { get; set; }
	public int Hp;
	public int HpMax;
	public int Status;

	public virtual void Update()
	{
		if (Hp <= 0 && Status != 0) 
		{
      		Hp = 0;
      		Status = 0;
    	} 
    	else if (Status == 0 && Hp == 0)
    	{
      		Hp = HpMax;
      		Status = 1;
    	} 
    	else if (Hp >= HpMax && Status != 2)
    	{
      		Hp = HpMax;
      		Status = 2;
    	} 
    	else 
    	{
      		Status = 2;
    	}
	}
}

public class DamageOOP : IComponentData
{
	public static int Id { get; set; }
	public int Attack;
	public int Defense;

	public virtual void Update(HealthOOP health, DataComponentOOP dataComponent)
	{
		if (Attack == 0 || Defense == 0)
		{
			Attack = dataComponent.RNG.RandiRange(10, 40);
			Defense = dataComponent.RNG.RandiRange(10, 40);
			health.HpMax = dataComponent.RNG.RandiRange(100, 200);
		}

		int total = Attack - Defense;

		if (health.Hp > 0 && total > 0)
		{
			health.Hp = Math.Max(0, health.Hp - total);
		}
	}
}

public class SpriteOOP : IComponentData
{
	public static int Id { get; set; }
	public CharBuffer<Buffer16<char>> SpriteId;

	public virtual void Update(HealthOOP health)
	{
		switch (health.Status)
		{
			case 0:
				SpriteId = "_";
				break;
			case 1:
				SpriteId = "/";
				break;
			case 2:
				if (health.Hp == health.HpMax)
				{
					SpriteId = "+";
				}
				else {
					SpriteId = "|";
				}
				break;
			default:
				break;
		}
	}
}
public class DebugSpeedOOP : IComponentData
{
	public static int Id { get; set; }
	public CharBuffer<Buffer128<char>> SpeedText;

	public virtual void Update(PositionOOP position, VelocityOOP velocity)
	{
		SpeedText = $"At position ({position.X}, {position.Y}) with velocity ({velocity.X}, {velocity.Y})";
	}
}

public class DebugDataOOP : IComponentData
{
	public static int Id { get; set; }
	public CharBuffer<Buffer128<char>> DataText;

	public virtual void Update(DataComponentOOP dataComponent)
	{
		DataText = $"int: {dataComponent.RandomInt}, double: {dataComponent.RandomDouble}";
	}
}

public class RobotVelocityOOP : VelocityOOP
{
	public override void Update(DataComponentOOP dataComponent, PositionOOP position)
	{
		if (dataComponent.RandomDouble % 10 == 0)
		{
			if (position.X > position.Y)
			{
				X = dataComponent.RNG.RandfRange(6, 23) - 12;
				Y = dataComponent.RNG.RandfRange(0, 8);
			}
			else
			{
				X = dataComponent.RNG.RandfRange(0, 8);
				Y = dataComponent.RNG.RandfRange(6, 23) - 12;
			}
		}
	}
}

/***********************
*  Benchmark classes  *
***********************/

public partial class ClassVsStructBenchmarksBase
{
	protected int _fpsCount = (60 * 100);
	protected int _entityCount = 1000;
	protected IndexMap<int> _entities;
	protected IndexMap<int> _entitiesRenderable;
	protected IndexMap<int> _entitiesPositionBase;
	protected IndexMap<int> _entitiesNonRenderable1;
	protected IndexMap<int> _entitiesNonRenderable2;
	protected IndexMap<int> _entitiesRobots;

	protected int _entitiesRenderableCount;
	protected int _entitiesPositionBaseCount;
	protected int _entitiesNonRenderable1Count;
	protected int _entitiesNonRenderable2Count;
	protected int _entitiesRobotsCount;

	protected IActor[] _entitiesOop;

	// component arrays
	protected Position[] _positionStruct;
	protected Velocity[] _velocityStruct;
	protected Health[] _healthStruct;
	protected Damage[] _damageStruct;
	protected DataComponent[] _dataStruct;
	protected Sprite[] _spriteStruct;
	protected DebugSpeed[] _debugSpeedStruct;
	protected DebugData[] _debugDataStruct;
	protected Robot[] _robotsStruct;

	[IterationSetup]
	public void Setup()
	{
		CreateEntities();
		CreateStructs();
	}

	public void CreateEntities()
	{
		_entities = new();
		_entitiesRenderable = new();
		_entitiesPositionBase = new();
		_entitiesNonRenderable1 = new();
		_entitiesNonRenderable2 = new();
		_entitiesRobots = new();

		_entitiesOop = new IActor[_entityCount];

		int c = 0;
		for (int i = 0; i < _entityCount; i++)
		{
			_entities[i] = i;

			if (c == 2)
			{
				// non-renderable
				_entitiesNonRenderable1[i] = i;
				_entitiesPositionBase[i] = i;

				_entitiesOop[i] = new NonRenderableActor();
			}
			else if (c == 3)
			{
				// non-renderable 2
				_entitiesNonRenderable2[i] = i;
				_entitiesPositionBase[i] = i;

				_entitiesOop[i] = new NonRenderableActor2();
			}
			else if (c == 4)
			{
				// robot!
				_entitiesRobots[i] = i;
				_entitiesPositionBase[i] = i;
				_entitiesRenderable[i] = i;

				_entitiesOop[i] = new NonRenderableActor2();
				c = 0;
			}
			else
			{
				_entitiesRenderable[i] = i;
				_entitiesPositionBase[i] = i;

				_entitiesOop[i] = new RenderableActor();
			}

			_entitiesRenderableCount = _entitiesRenderable.Count;
			_entitiesNonRenderable1Count = _entitiesNonRenderable1.Count;
			_entitiesNonRenderable2Count = _entitiesNonRenderable2.Count;
			_entitiesPositionBaseCount = _entitiesPositionBase.Count;
			_entitiesRobotsCount = _entitiesRobots.Count;

			c++;
		}

		Console.WriteLine($"Renderable: {_entitiesRenderableCount}");
		Console.WriteLine($"Position base: {_entitiesPositionBaseCount}");
		Console.WriteLine($"Non renderable 1: {_entitiesNonRenderable1Count}");
		Console.WriteLine($"Non renderable 2: {_entitiesNonRenderable2Count}");
		Console.WriteLine($"Robots: {_entitiesRobotsCount}");
	}

	public void CreateStructs()
	{
		_positionStruct = new Position[_entityCount];
		_velocityStruct = new Velocity[_entityCount];
		_healthStruct = new Health[_entityCount];
		_damageStruct = new Damage[_entityCount];
		_dataStruct = new DataComponent[_entityCount];
		_spriteStruct = new Sprite[_entityCount];
		_debugSpeedStruct = new DebugSpeed[_entityCount];
		_debugDataStruct = new DebugData[_entityCount];
		_robotsStruct = new Robot[_entityCount];

		for (int i = 0; i < _entityCount; i++)
		{
			if (_entitiesPositionBase.IndexOfData(i) != -1)
			{
				_positionStruct[i] = new();
				_velocityStruct[i] = new();
				_dataStruct[i] = new();
			}
			else if (_entitiesRenderable.IndexOfData(i) != -1) {
				_healthStruct[i] = new();
				_damageStruct[i] = new();
				_spriteStruct[i] = new();
			}
			else if (_entitiesNonRenderable1.IndexOfData(i) != -1) {
				_debugSpeedStruct[i] = new();
			}
			else if (_entitiesNonRenderable2.IndexOfData(i) != -1) {
				_debugDataStruct[i] = new();
			}
			else if (_entitiesRobots.IndexOfData(i) != -1) {
				_robotsStruct[i] = new();
			}
		}
	}
}

public partial class ClassVsStructBenchmarks_Update : ClassVsStructBenchmarksBase
{
	[Benchmark(Baseline = true)]
	public void ArrayOfStructs_Update()
	{
		for (int fps = 0; fps < _fpsCount; fps++)
		{
			// MovementSystem
			for (int i = 0; i < _entitiesPositionBaseCount; i++)
			{
				ref Position position = ref _positionStruct[i];
				Velocity velocity = _velocityStruct[i];

				position.X += (velocity.X * 0.0166);
				position.Y += (velocity.Y * 0.0166);
			}

			// HealthSystem
			for (int i = 0; i < _entitiesRenderableCount; i++)
			{
				ref Health health = ref _healthStruct[i];
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

    		// DamageSystem
			for (int i = 0; i < _entitiesRenderableCount; i++)
			{
    			ref Damage damage = ref _damageStruct[i];
				ref Health health = ref _healthStruct[i];
				DataComponent dataComponent = _dataStruct[i];

				if (damage.Attack == 0 || damage.Defense == 0)
				{
					damage.Attack = dataComponent.RNG.RandiRange(10, 40);
					damage.Defense = dataComponent.RNG.RandiRange(10, 40);
					health.HpMax = dataComponent.RNG.RandiRange(100, 200);
				}


				int total = damage.Attack - damage.Defense;
				if (health.Hp > 0 && total > 0)
				{
					health.Hp = Math.Max(0, health.Hp - total);
				}
			}

    		// DataSystem
			for (int i = 0; i < _entitiesPositionBaseCount; i++)
			{
				ref DataComponent data = ref _dataStruct[i];
				data.RandomInt = data.RNG.Randi();
				data.RandomDouble = (double) data.RNG.Randf();
			}

    		// DirectionSystem
			for (int i = 0; i < _entitiesPositionBaseCount; i++)
			{
				Position position = _positionStruct[i];
				ref Velocity velocity = ref _velocityStruct[i];
				DataComponent data = _dataStruct[i];

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

    		// SpriteSystem
			for (int i = 0; i < _entitiesRenderableCount; i++)
			{
				Health health = _healthStruct[i];
				ref Sprite sprite = ref _spriteStruct[i];

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

    		// DebugSpeedSystem
			for (int i = 0; i < _entitiesNonRenderable1Count; i++)
			{
				ref DebugSpeed debugSpeed = ref _debugSpeedStruct[i];
				Position position = _positionStruct[i];
				Velocity velocity = _velocityStruct[i];

				debugSpeed.SpeedText = $"At position ({position.X}, {position.Y}) with velocity ({velocity.X}, {velocity.Y})";
			}

    		// DebugDataSystem
			for (int i = 0; i < _entitiesNonRenderable2Count; i++)
			{
				ref DebugData debugData = ref _debugDataStruct[i];
				DataComponent dataComponent = _dataStruct[i];

				debugData.DataText = $"int: {dataComponent.RandomInt}, double: {dataComponent.RandomDouble}";
			}

			// RobotVelocitySystem
			for (int i = 0; i < _entitiesRobotsCount; i++)
			{
				ref Velocity velocity = ref _velocityStruct[i];
				DataComponent dataComponent = _dataStruct[i];
				Position position = _positionStruct[i];

				if (dataComponent.RandomDouble % 10 == 0)
				{
					if (position.X > position.Y)
					{
						velocity.X = dataComponent.RNG.RandfRange(6, 23) - 12;
						velocity.Y = dataComponent.RNG.RandfRange(0, 8);
					}
					else
					{
						velocity.X = dataComponent.RNG.RandfRange(0, 8);
						velocity.Y = dataComponent.RNG.RandfRange(6, 23) - 12;
					}
				}
			}
		}
	}

	[Benchmark]
	public void ClassesOOP_Update()
	{
		for (int fps = 0; fps < _fpsCount; fps++)
		{
			for (int i = 0; i < _entityCount; i++)
			{
				// update each entity with required components in same order
				IActor e = _entitiesOop[i];

				e.Update();
			}
		}
	}
}
