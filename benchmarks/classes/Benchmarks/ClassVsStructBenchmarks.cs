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


/*************************************
*  Struct components with Update()  *
*************************************/

public struct PositionEC : IComponentData
{
	public static int Id { get; set; }
	public double X;
	public double Y;

	public void Update(VelocityEC velocity)
	{
		X += (velocity.X * 0.0166);
		Y += (velocity.Y * 0.0166);
	}
}

public struct VelocityEC : IComponentData
{
	public static int Id { get; set; }
	public double X;
	public double Y;

	public void Update(PositionEC position, DataComponentEC data)
	{
		if (data.RandomInt % 10 == 0)
		{
			if (position.X > position.Y)
			{
				X = data.RNG.RandfRange(3, 19) - 10;
				Y = data.RNG.RandfRange(0, 5);
			}
			else
			{
				X = data.RNG.RandfRange(0, 5);
				Y = data.RNG.RandfRange(3, 19) - 10;
			}
		}
	}
}

public struct DataComponentEC : IComponentData
{
	public static int Id { get; set; }
	public int Type;
	public int RandomInt;
	public double RandomDouble;
	public NumberGenerator RNG { get; } = new();

	public DataComponentEC()
	{
		RNG.Randomize();
	}

	public void Update()
	{
		RandomInt = RNG.Randi();
		RandomDouble = (double) RNG.Randf();
	}
}

public struct HealthEC : IComponentData
{
	public static int Id { get; set; }
	public int Hp;
	public int HpMax;
	public int Status;

	public void Update()
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

public struct DamageEC : IComponentData
{
	public static int Id { get; set; }
	public int Attack;
	public int Defense;

	public void Update(HealthEC health)
	{
		int total = Attack - Defense;
		if (health.Hp > 0 && total > 0)
		{
			health.Hp = Math.Max(0, health.Hp - total);
		}
	}
}

public struct SpriteEC : IComponentData
{
	public static int Id { get; set; }
	public CharBuffer<Buffer16<char>> SpriteId;

	public void Update(HealthEC health, DamageEC damage)
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

/**********************
*  Class components  *
**********************/

public class PositionClass : IComponentData
{
	public static int Id { get; set; }
	public double X;
	public double Y;
}

public class VelocityClass : IComponentData
{
	public static int Id { get; set; }
	public double X;
	public double Y;
}

public class DataComponentClass : IComponentData
{
	public static int Id { get; set; }
	public int Type;
	public int RandomInt;
	public double RandomDouble;
	public NumberGenerator RNG { get; } = new();

	public DataComponentClass()
	{
		RNG.Randomize();
	}
}

public class HealthClass : IComponentData
{
	public static int Id { get; set; }
	public int Hp;
	public int HpMax;
	public int Status;
}

public class DamageClass : IComponentData
{
	public static int Id { get; set; }
	public int Attack;
	public int Defense;
}

public class SpriteClass : IComponentData
{
	public static int Id { get; set; }
	public CharBuffer<Buffer16<char>> SpriteId;
}


/***********************************
*  OOP entity class + components  *
***********************************/

public class EntityClass 
{
	public PositionOOP Position { get; set; } = new PositionOOP();
	public VelocityOOP Velocity { get; set; } = new VelocityOOP();
	public HealthOOP Health { get; set; } = new HealthOOP();
	public DamageOOP Damage { get; set; } = new DamageOOP();
	public DataComponentOOP DataComponent { get; set; } = new DataComponentOOP();
	public SpriteOOP Sprite { get; set; } = new SpriteOOP();
}

public class PositionOOP : IComponentData
{
	public static int Id { get; set; }
	public double X;
	public double Y;
	
	public void Update(VelocityOOP velocity)
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

	public void Update(PositionOOP position, DataComponentOOP data)
	{
		if (data.RandomInt % 10 == 0)
		{
			if (position.X > position.Y)
			{
				X = data.RNG.RandfRange(3, 19) - 10;
				Y = data.RNG.RandfRange(0, 5);
			}
			else
			{
				X = data.RNG.RandfRange(0, 5);
				Y = data.RNG.RandfRange(3, 19) - 10;
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

	public void Update()
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

	public void Update()
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

	public void Update(HealthOOP health)
	{
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

	public void Update(HealthOOP health, DamageOOP damage)
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


/******************************************
*  OOP entity class + struct components  *
******************************************/

public struct EntityStruct 
{
	public PositionEC Position { get; set; } = new PositionEC();
	public VelocityEC Velocity { get; set; } = new VelocityEC();
	public HealthEC Health { get; set; } = new HealthEC();
	public DamageEC Damage { get; set; } = new DamageEC();
	public DataComponentEC DataComponent { get; set; } = new DataComponentEC();
	public SpriteEC Sprite { get; set; } = new SpriteEC();

	public EntityStruct() {}
}


/***********************
*  Benchmark classes  *
***********************/

public partial class ClassVsStructBenchmarksBase
{
	protected int _entityCount = 1000;
	protected int[] _entities;
	protected EntityClass[] _entitiesOop;
	protected EntityStruct[] _entitiesStructs;

	// component arrays
	protected Position[] _positionStruct;
	protected Velocity[] _velocityStruct;
	protected Health[] _healthStruct;
	protected Damage[] _damageStruct;
	protected DataComponent[] _dataStruct;
	protected Sprite[] _spriteStruct;

	protected PositionClass[] _positionClass;
	protected VelocityClass[] _velocityClass;
	protected HealthClass[] _healthClass;
	protected DamageClass[] _damageClass;
	protected DataComponentClass[] _dataClass;
	protected SpriteClass[] _spriteClass;

	[IterationSetup]
	public void Setup()
	{
		CreateEntities();
		CreateStructs();
		CreateClasses();
	}

	public void CreateEntities()
	{
		_entities = new int[_entityCount];

		for (int i = 0; i < _entityCount; i++)
		{
			_entities[i] = i;
		}

		_entitiesOop = new EntityClass[_entityCount];

		for (int i = 0; i < _entityCount; i++)
		{
			_entitiesOop[i] = new();
		}

		_entitiesStructs = new EntityStruct[_entityCount];

		for (int i = 0; i < _entityCount; i++)
		{
			_entitiesStructs[i] = new();
		}
	}

	public void CreateStructs()
	{
		_positionStruct = new Position[_entityCount];
		_velocityStruct = new Velocity[_entityCount];
		_healthStruct = new Health[_entityCount];
		_damageStruct = new Damage[_entityCount];
		_dataStruct = new DataComponent[_entityCount];
		_spriteStruct = new Sprite[_entityCount];

		for (int i = 0; i < _entityCount; i++)
		{
			_positionStruct[i] = new();
			_velocityStruct[i] = new();
			_healthStruct[i] = new();
			_damageStruct[i] = new();
			_dataStruct[i] = new();
			_spriteStruct[i] = new();
		}
	}
	public void CreateClasses()
	{
		_positionClass = new PositionClass[_entityCount];
		_velocityClass = new VelocityClass[_entityCount];
		_healthClass = new HealthClass[_entityCount];
		_damageClass = new DamageClass[_entityCount];
		_dataClass = new DataComponentClass[_entityCount];
		_spriteClass = new SpriteClass[_entityCount];

		for (int i = 0; i < _entityCount; i++)
		{
			_positionClass[i] = new();
			_velocityClass[i] = new();
			_healthClass[i] = new();
			_damageClass[i] = new();
			_dataClass[i] = new();
			_spriteClass[i] = new();
		}
	}
}

public partial class ClassVsStructBenchmarks_Update : ClassVsStructBenchmarksBase
{
	[Benchmark(Baseline = true)]
	public void Structs_Update()
	{
		// MovementSystem
		for (int i = 0; i < _entityCount; i++)
		{
			ref Position position = ref _positionStruct[i];
			ref Velocity velocity = ref _velocityStruct[i];

			position.X += (velocity.X * 0.0166);
			position.Y += (velocity.Y * 0.0166);
		}

		// HealthSystem
		for (int i = 0; i < _entityCount; i++)
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
		for (int i = 0; i < _entityCount; i++)
		{
    		ref Damage damage = ref _damageStruct[i];
			ref Health health = ref _healthStruct[i];

			int total = damage.Attack - damage.Defense;
			if (health.Hp > 0 && total > 0)
			{
				health.Hp = Math.Max(0, health.Hp - total);
			}
		}

    	// DataSystem
		for (int i = 0; i < _entityCount; i++)
		{
			ref DataComponent data = ref _dataStruct[i];
			data.RandomInt = data.RNG.Randi();
			data.RandomDouble = (double) data.RNG.Randf();
		}

    	// DirectionSystem
		for (int i = 0; i < _entityCount; i++)
		{
			ref Position position = ref _positionStruct[i];
			ref Velocity velocity = ref _velocityStruct[i];
			ref DataComponent data = ref _dataStruct[i];

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
		for (int i = 0; i < _entityCount; i++)
		{
			ref Damage damage = ref _damageStruct[i];
			ref Health health = ref _healthStruct[i];
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
	}

	[Benchmark]
	public void Classes_Update()
	{
		// MovementSystem
		for (int i = 0; i < _entityCount; i++)
		{
			PositionClass position = _positionClass[i];
			VelocityClass velocity = _velocityClass[i];

			position.X += (velocity.X * 0.0166);
			position.Y += (velocity.Y * 0.0166);
		}

		// HealthSystem
		for (int i = 0; i < _entityCount; i++)
		{
			HealthClass health = _healthClass[i];
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
		for (int i = 0; i < _entityCount; i++)
		{
    		DamageClass damage = _damageClass[i];
			HealthClass health = _healthClass[i];

			int total = damage.Attack - damage.Defense;
			if (health.Hp > 0 && total > 0)
			{
				health.Hp = Math.Max(0, health.Hp - total);
			}
		}

    	// DataSystem
		for (int i = 0; i < _entityCount; i++)
		{
			DataComponentClass data = _dataClass[i];
			data.RandomInt = data.RNG.Randi();
			data.RandomDouble = (double) data.RNG.Randf();
		}

    	// DirectionSystem
		for (int i = 0; i < _entityCount; i++)
		{
			PositionClass position = _positionClass[i];
			VelocityClass velocity = _velocityClass[i];
			DataComponentClass data = _dataClass[i];

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
		for (int i = 0; i < _entityCount; i++)
		{
			DamageClass damage = _damageClass[i];
			HealthClass health = _healthClass[i];
			SpriteClass sprite = _spriteClass[i];

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

	[Benchmark]
	public void EntityStructs_Update()
	{
		for (int i = 0; i < _entityCount; i++)
		{
			// update each entity with required components in same order
			ref EntityStruct e = ref _entitiesStructs[i];

			e.Position.Update(e.Velocity);
			e.Velocity.Update(e.Position, e.DataComponent);
			e.Health.Update();
			e.Damage.Update(e.Health);
			e.DataComponent.Update();
			e.Sprite.Update(e.Health, e.Damage);
		}
	}

	[Benchmark]
	public void ClassesOOP_Update()
	{
		for (int i = 0; i < _entityCount; i++)
		{
			// update each entity with required components in same order
			EntityClass e = _entitiesOop[i];

			e.Position.Update(e.Velocity);
			e.Velocity.Update(e.Position, e.DataComponent);
			e.Health.Update();
			e.Damage.Update(e.Health);
			e.DataComponent.Update();
			e.Sprite.Update(e.Health, e.Damage);
		}
	}
}
