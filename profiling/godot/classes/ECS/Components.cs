/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : Components
 * @created     : Friday May 10, 2024 17:03:44 CST
 */

namespace GodotEGP.Profiling.CLI.ECS;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;
using GodotEGP.Random;
using GodotEGP.Collections;

using GodotEGP.ECS;

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
