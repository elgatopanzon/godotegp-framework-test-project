/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ClassVsStructBenchmarks
 * @created     : Wednesday May 15, 2024 22:17:07 CST
 */

namespace GodotEGP.Benchmarks.ClassVsStruct;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.Random;
using GodotEGP.Collections;

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

public struct Position
{
	public double X;
	public double Y;
}

public struct Velocity
{
	public double X;
	public double Y;
}

public struct DataComponent
{
	public int Type;
	public int RandomInt;
	public double RandomDouble;
	public NumberGenerator RNG { get; } = new();

	public DataComponent()
	{
		RNG.Randomize();
	}
}

public struct Health
{
	public int Hp;
	public int HpMax;
	public int Status;
}

public struct Damage
{
	public int Attack;
	public int Defense;
}

public struct Sprite
{
	public CharBuffer<Buffer16<char>> SpriteId;
}

public struct DebugSpeed
{
	public CharBuffer<Buffer128<char>> SpeedText;
}

public struct DebugData
{
	public CharBuffer<Buffer128<char>> DataText;
}

public struct Robot {}


/***********************************
*  OOP entity class + components  *
***********************************/

/****************
*  Interfaces  *
****************/
public interface IActor
{
	public void Update();
	public DataComponentOOP DataComponent { get; }
}

public interface IVelocity 
{
	public void Update(IData dataComponent, IPosition position);
	public double X { get; set; }
	public double Y { get; set; }
}
public interface IPosition 
{
	public void Update(IVelocity velocity);
	public double X { get; set; }
	public double Y { get; set; }
}
public interface IHealth 
{
	public int Hp { get; set; }
	public int HpMax { get; set; }
	public int Status { get; set; }
	public void Update();
}

public interface IData
{
	public int Type { get; set; }
	public int RandomInt { get; set; }
	public double RandomDouble { get; set; }
	public NumberGenerator RNG { get; }
	public void Update();
}

public interface IDamage
{
	public int Attack { get; set; }
	public int Defense { get; set; }
	public void Update(IHealth health, IData dataComponent);
}

public interface ISprite
{
	public CharBuffer<Buffer16<char>> SpriteId { get; set; }
	public void Update(IHealth health);
}

public interface IDebugSpeed
{
	public CharBuffer<Buffer128<char>> SpeedText { get; set; }
	public void Update(IPosition position, IVelocity velocity);
}

public interface IDebugData
{
	public CharBuffer<Buffer128<char>> DataText { get; set; }

	public void Update(IData dataComponent);
}


/*************
*  Classes  *
*************/

public class Actor : IActor
{
	public virtual DataComponentOOP DataComponent { get; } = new();

	public virtual void Update()
	{
		DataComponent.Update();
	}
}


public class MovableActor : Actor
{
	public virtual IPosition Position { get; } = new PositionOOP();
	public virtual IVelocity Velocity { get; } = new VelocityOOP();

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
	public virtual IHealth Health { get; } = new HealthOOP();
	public virtual IDamage Damage { get; } = new DamageOOP();

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
	public virtual IDebugSpeed DebugSpeed { get; } = new DebugSpeedOOP();

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
	public virtual IDebugData DebugData { get; } = new DebugDataOOP();

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
	public virtual ISprite Sprite { get; } = new SpriteOOP();

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
	public override IVelocity Velocity { get; } = new RobotVelocityOOP();
	public override void Update()
	{
		// update the renderables
		Sprite.Update(this.Health);

		// update the base
		base.Update();
	}
}


public class PositionOOP : IPosition
{
	public double X { get; set; }
	public double Y { get; set; }
	
	public virtual void Update(IVelocity velocity)
	{
		X += (velocity.X * 0.0166);
		Y += (velocity.Y * 0.0166);
	}
}

public class VelocityOOP : IVelocity
{
	public double X { get; set; }
	public double Y { get; set; }

	public virtual void Update(IData dataComponent, IPosition position)
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

public class DataComponentOOP : IData
{
	public int Type { get; set; }
	public int RandomInt { get; set; }
	public double RandomDouble { get; set; }
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

public class HealthOOP : IHealth
{
	public int Hp { get; set; }
	public int HpMax { get; set; }
	public int Status { get; set; }

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

public class DamageOOP : IDamage
{
	public int Attack { get; set; }
	public int Defense { get; set; }

	public virtual void Update(IHealth health, IData dataComponent)
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

public class SpriteOOP : ISprite
{
	public CharBuffer<Buffer16<char>> SpriteId { get; set; }

	public virtual void Update(IHealth health)
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

public class DebugSpeedOOP : IDebugSpeed
{
	public CharBuffer<Buffer128<char>> SpeedText { get; set; }

	public virtual void Update(IPosition position, IVelocity velocity)
	{
		SpeedText = $"At position ({position.X}, {position.Y}) with velocity ({velocity.X}, {velocity.Y})";
	}
}

public class DebugDataOOP : IDebugData
{
	public CharBuffer<Buffer128<char>> DataText { get; set; }

	public virtual void Update(IData dataComponent)
	{
		DataText = $"int: {dataComponent.RandomInt}, double: {dataComponent.RandomDouble}";
	}
}

public class RobotVelocityOOP : VelocityOOP
{
	// custom update function for robots to control velocity with different
	// logic
	public override void Update(IData dataComponent, IPosition position)
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
	protected int _fpsCount = (60 * 1);
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
				// update each actor
				 _entitiesOop[i].Update();
			}
		}
	}
}
