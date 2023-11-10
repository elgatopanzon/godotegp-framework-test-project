/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : GameSaveManager
 * @created     : Thursday Nov 09, 2023 16:46:44 CST
 */

namespace GodotEGP.Service;

using System;
using System.Collections.Generic;
using System.IO;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;
using GodotEGP.SaveData;
using GodotEGP.Data.Endpoint;

public partial class SaveDataManager : Service
{
	private string _saveBaseDir = "Save";

	private Dictionary<string, Config.Object> _saveData = new Dictionary<string, Config.Object>();

	public SaveDataManager()
	{
		_saveBaseDir = "Save";

		// create base System data
		Create<SystemData>("System");
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	// Called when service is registered in manager
	public override void _OnServiceRegistered()
	{
		LoadSaveData();
	}

	// Called when service is deregistered from manager
	public override void _OnServiceDeregistered()
	{
	}

	// Called when service is considered ready
	public override void _OnServiceReady()
	{
	}

	public void LoadSaveData()
	{
		Queue<Dictionary<string, object>> fileQueue = new Queue<Dictionary<string, object>>();

		string saveDataPath = Path.Combine(OS.GetUserDataDir(), _saveBaseDir);

		if (Directory.Exists(saveDataPath))
		{
			DirectoryInfo d = new DirectoryInfo(saveDataPath);
			foreach (DirectoryInfo dir in d.GetDirectories())
			{
				string configDirName = dir.ToString().GetFile();
				Type configDirType = Type.GetType(configDirName);

				LoggerManager.LogDebug("Save data directory", "", "dirName", configDirName);
				LoggerManager.LogDebug("Type valid", "", "typeValid", configDirType);

				// if it's a valid config object, and if the base type is
				// ValidatedObject, then let's load the content
				if (configDirType != null && configDirType.Namespace == typeof(SaveData.Data).Namespace)
				{
					foreach (FileInfo file in dir.GetFiles("*.json"))
					{
						LoggerManager.LogDebug("Queueing file for content load", "", "file", file.ToString());

						fileQueue.Enqueue(new Dictionary<string, object> {{"configType", configDirName}, {"path", file.ToString()}, {"name", Path.GetFileNameWithoutExtension(file.ToString().GetFile())}});
					}
				}
			}
		}
		else
		{
			LoggerManager.LogDebug("Save path doesn't exist", "", "path", saveDataPath);
		}

		if (fileQueue.Count > 0)
		{
			// load all the save data objects using Config.Loader
			Config.Loader configLoader = new Config.Loader(fileQueue);

			configLoader.SubscribeOwner<ConfigManagerLoaderCompleted>(_On_SaveDataLoad_Completed, oneshot: true, isHighPriority: true);
			configLoader.SubscribeOwner<ConfigManagerLoaderError>(_On_SaveDataLoad_Error, oneshot: true, isHighPriority: true);
		}
		else
		{
			_SetServiceReady(true);
		}
		
	}

	public void _On_SaveDataLoad_Completed(IEvent e)
	{
		if (e is ConfigManagerLoaderCompleted ec)
		{
			LoggerManager.LogDebug("Loading of save files completed", "", "e", ec.ConfigObjects);	

			foreach (Config.Object obj in ec.ConfigObjects)
			{
				// when we load saves from disk, they are always overwritten
				// with the new objects
				_saveData.Remove(obj.Name);
				if (obj.RawValue is SaveData.Data sd)
				{
					sd.UpdateDateLoaded();
				}
				Register(obj.Name, obj);
			}

			_SetServiceReady(true);
		}
	}

	public void _On_SaveDataLoad_Error(IEvent e)
	{
		if (e is ConfigManagerLoaderError ee)
		{
			throw ee.RunWorkerCompletedEventArgs.Error;
		}
	}

	public Config.Object Get<T>(string saveName) where T : SaveData.Data, new()
	{
		if (_saveData.TryGetValue(saveName, out Config.Object obj))
		{
			return (Config.Object<T>) obj;
		}

		throw new SaveDataNotFoundException($"Save data with the name {saveName} doesn't exist!");
	}

	public void Create<T>(string saveName) where T : SaveData.Data, new()
	{
		if (_saveData.TryAdd(saveName, new Config.Object<T>()))
		{
			var obj = Get<T>(saveName);
			obj.Name= saveName;
			obj.DataEndpoint = new FileEndpoint(OS.GetUserDataDir()+"/"+_saveBaseDir+"/"+obj.RawValue.ToString()+"/"+obj.Name+".json");

			LoggerManager.LogDebug("Creating new save data instance", "", "saveData", obj);
		}
		else
		{
			throw new SaveDataExistsException($"Save data with the name {saveName} already exists!");
		}
	}

	public void Register(string saveName, Config.Object saveData)
	{
		if (_saveData.TryAdd(saveName, saveData))
		{
			LoggerManager.LogDebug("Registering new save data instance", "", "save", saveName);
		}
		else
		{
			throw new SaveDataExistsException($"Save data with the name {saveName} already exists!");
		}
	}

	public bool Exists(string saveName)
	{
		return _saveData.ContainsKey(saveName);
	}

	public void Set(string saveName, Config.Object saveData)
	{
		_saveData[saveName] = saveData;
	}

	public void Save(string saveName)
	{
		var obj = _saveData[saveName];

		if (obj.RawValue is SaveData.Data sd)
		{
			sd.UpdateDateSaved();
		}

		obj.SubscribeOwner<DataOperationComplete>(_On_SaveDataSave_Complete, oneshot: true);
		obj.SubscribeOwner<DataOperationError>(_On_SaveDataSave_Error, oneshot: true);

		obj.Save();
	}

	public void SaveAll()
	{
		foreach (var obj in _saveData)
		{
			Save(obj.Key);
		}
	}

	public void _On_SaveDataSave_Complete(IEvent e)
	{
		if (e is DataOperationComplete ee)
		{
			LoggerManager.LogDebug("Save data object saved", "", "saveName", (e.Owner as Config.Object).Name);
		}
	}
	public void _On_SaveDataSave_Error(IEvent e)
	{
		if (e is DataOperationError ee)
		{
			LoggerManager.LogDebug("Save data object save failed", "", "saveName", (e.Owner as Config.Object).Name);
		}
	}

	// public void LoadSaveData()
	// {
	// 	Queue<Dictionary<string, object>> fileQueue = new Queue<Dictionary<string, object>>();
    //
	// 	string configPath = Path.Combine(_saveDataPath, _configBaseDir);
    //
	// 	if (Directory.Exists(configPath))
	// 	{
	// 		DirectoryInfo d = new DirectoryInfo(configPath);
	// 		foreach (DirectoryInfo dir in d.GetDirectories())
	// 		{
	// 			string configDirName = dir.ToString().GetFile();
    //
	// 			LoggerManager.LogDebug("Save data directory", "", "dirName", configDirName);
    //
	// 			// trigger creation of base object in the register
	// 			// before queueing files for loading
	// 			GetConfigObjectInstance(Type.GetType(configDirName.ToString()));
    //
	// 			foreach (FileInfo file in dir.GetFiles("*.json"))
	// 			{
	// 				LoggerManager.LogDebug("Queueing file for content load", "", "file", file.ToString());
    //
	// 				fileQueue.Enqueue(new Dictionary<string, object> {{"configType", configDirName}, {"path", "/"+file.ToString().Replace(System.Environment.CurrentDirectory, "")}});
	// 			}
	// 		}
	// 	}
	// 	else
	// 	{
	// 		LoggerManager.LogDebug("Save data directory doesn't exist", "", "path", configPath);
	// 	}
    //
	// 	if (fileQueue.Count > 0)
	// 	{
	// 		// load all the save data files using ConfigManagerLoader
	// 		Config.Loader configLoader = new Config.Loader(fileQueue);
    //
	// 		configLoader.SubscribeOwner<ConfigManagerLoaderCompleted>(_On_ConfigManagerLoaderCompleted, oneshot: true, isHighPriority: true);
	// 		configLoader.SubscribeOwner<ConfigManagerLoaderError>(_On_ConfigManagerLoaderError, oneshot: true, isHighPriority: true);
	// 	}
	// }
    //
	// public new T Get<T>() where T : SaveData.Data
	// {
	// 	return (T) base.Get<T>();
	// }
    //
	// public new void Save<T>(IEndpoint dataEndpoint = null) where T : SaveData.Data
	// {
	// 	base.Save<T>();
	// }
}

public class SaveDataNotFoundException : Exception
{
	public SaveDataNotFoundException() {}
	public SaveDataNotFoundException(string message) : base(message) {}
	public SaveDataNotFoundException(string message, Exception inner) : base(message, inner) {}
}

public class SaveDataExistsException : Exception
{
	public SaveDataExistsException() {}
	public SaveDataExistsException(string message) : base(message) {}
	public SaveDataExistsException(string message, Exception inner) : base(message, inner) {}
}
