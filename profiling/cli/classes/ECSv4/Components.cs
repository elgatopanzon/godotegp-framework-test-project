/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : Components
 * @created     : Wednesday May 08, 2024 23:26:13 CST
 */

namespace GodotEGP.Profiling.CLI.ECSv4;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;
using GodotEGP.Random;

using GodotEGP.Collections;
using GodotEGP.ECSv4;
using GodotEGP.ECSv4.Components;

public struct Position : IDataComponent
{
	public static int Id { get; set; }
	public double X;
	public double Y;
}

public struct Velocity : IDataComponent
{
	public static int Id { get; set; }
	public double X;
	public double Y;
}

public struct DataComponent : IDataComponent
{
	public static int Id { get; set; }
	public int Type;
	public int RandomInt;
	public double RandomDouble;
}

public struct Health : IDataComponent
{
	public static int Id { get; set; }
	public int Hp;
	public int HpMax;
	public int Status;
}

public struct Damage : IDataComponent
{
	public static int Id { get; set; }
	public int Attack;
	public int Defense;
}

public struct Sprite : IDataComponent
{
	public static int Id { get; set; }
	public char SpriteId;
}
