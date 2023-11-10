/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : SaveData
 * @created     : Thursday Nov 09, 2023 17:03:15 CST
 */

namespace GodotEGP.SaveData;

using System;
using System.Collections.Generic;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class Data : VObject
{
	internal readonly VValue<DateTime> _dateCreated;

	public DateTime DateCreated
	{
		get { return _dateCreated.Value; }
		set { _dateCreated.Value = value; }
	}

	internal readonly VValue<DateTime> _dateSaved;

	public DateTime DateSaved
	{
		get { return _dateSaved.Value; }
		set { _dateSaved.Value = value; }
	}

	internal readonly VValue<DateTime> _dateLoaded;

	public DateTime DateLoaded
	{
		get { return _dateSaved.Value; }
		set { _dateSaved.Value = value; }
	}

	public Data()
	{
        _dateCreated = AddValidatedValue<DateTime>(this)
        	.Default(DateTime.Now)
        	.ChangeEventsEnabled();

        _dateSaved = AddValidatedValue<DateTime>(this)
        	.Default(DateTime.Now)
        	.ChangeEventsEnabled();

        _dateLoaded = AddValidatedValue<DateTime>(this)
        	.Default(DateTime.Now)
        	.ChangeEventsEnabled();
	}

	public void UpdateDateSaved()
	{
		_dateSaved.Value = DateTime.Now;

		LoggerManager.LogDebug("Updating saved date", "", "date", _dateSaved.Value);
	}
	public void UpdateDateLoaded()
	{
		_dateLoaded.Value = DateTime.Now;

		LoggerManager.LogDebug("Updating loaded date", "", "date", _dateLoaded.Value);
	}
}

// single save data with VValues
public partial class SystemData : Data
{
	public SystemData() : base()
	{
	}
}


// complex container with encompassing SaveData objects
public partial class GameSaveFile : Data
{
	public GameSaveFile() : base()
	{
	}
}
