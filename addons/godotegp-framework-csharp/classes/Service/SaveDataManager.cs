/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : GameSaveManager
 * @created     : Thursday Nov 09, 2023 16:46:44 CST
 */

namespace GodotEGP.Service;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

	private SaveDataManagerConfig _config = new SaveDataManagerConfig();

	private Dictionary<string, Config.Object> _saveData = new Dictionary<string, Config.Object>();

	private Timer _timedAutosaveTimer;

	public SaveDataManager()
	{
		_saveBaseDir = "Save";

		// create base System data
		Create<SystemData>("System");
	}

	public void _On_TimedAutoSave_timeout(IEvent e)
	{
		LoggerManager.LogDebug("TimedAutosave timeout", "");
		_timedAutosaveTimer.WaitTime = _config.AutosaveTimeDefaultSec;

		CreateAutosaves();
	}

	public void SetConfig(SaveDataManagerConfig config)
	{
		LoggerManager.LogDebug("Setting config", "", "config", config);

		_config = config;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// TimedAutosave timer
		_timedAutosaveTimer = new Timer();
		_timedAutosaveTimer.OneShot = false;
		_timedAutosaveTimer.SetMeta("id", "SaveDataManager.TimedAutosaveTimer");
		_timedAutosaveTimer.SubscribeSignal("timeout", false, _On_TimedAutoSave_timeout);

		AddChild(_timedAutosaveTimer);
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
					foreach (FileInfo file in dir.GetFiles("*.json").OrderBy((f) => f.ToString()))
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

			if (!GetReady())
			{
				_SetServiceReady(true);
			}
		}
	}

	public void _On_SaveDataLoad_Error(IEvent e)
	{
		if (e is ConfigManagerLoaderError ee)
		{
			throw ee.RunWorkerCompletedEventArgs.Error;
		}
	}

	public T Get<T>(string saveName) where T : SaveData.Data, new()
	{
		if (_saveData.TryGetValue(saveName, out Config.Object obj))
		{
			return (T) obj.RawValue;
		}

		throw new SaveDataNotFoundException($"Save data with the name {saveName} doesn't exist!");
	}

	public Config.Object Get(string saveName)
	{
		if (_saveData.TryGetValue(saveName, out Config.Object obj))
		{
			return obj;
		}

		throw new SaveDataNotFoundException($"Save data with the name {saveName} doesn't exist!");
	}

	public T Create<T>(string saveName) where T : SaveData.Data, new()
	{
		Config.Object<T> save = new Config.Object<T>();

		if (_saveData.TryAdd(saveName, save))
		{
			save.Name = saveName;
			save.Value.Name = saveName;
			save.DataEndpoint = new FileEndpoint(OS.GetUserDataDir()+"/"+_saveBaseDir+"/"+save.RawValue.ToString()+"/"+save.Name+".json");

			LoggerManager.LogDebug("Creating new save data instance", "", "saveData", save);

			Save(saveName);

			return save.Value;
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

	public void CreateAutosave(string saveName)
	{
		string autosaveName = saveName+"_autosave";

		LoggerManager.LogDebug("Creating autosave", "", "saveName", saveName);
	}

	public void SetLoaded(string saveName, bool loadedState = true)
	{
		if (Exists(saveName))
		{
			if (Get(saveName).RawValue is SaveData.Data sd && sd.SaveType == SaveDataType.Manual)
			{
				sd.Loaded = loadedState;

				StartTimedAutosave();
			}
		}
	}

	public void StartTimedAutosave()
	{
		_timedAutosaveTimer.WaitTime = _config.AutosaveTimeDefaultSec;
		if (_config.TimedAutosaveEnabled)
		{
			_timedAutosaveTimer.Start();
		}
	}

	public void CreateAutosaves()
	{
		foreach (var save in GetLoadedSaves())
		{
			if (save.RawValue is SaveData.Data sd)
			{
				CreateAutosave(sd.Name);
			}
		}
	}

	public List<Config.Object> GetLoadedSaves()
	{
		List<Config.Object> loadedSaves = new List<Config.Object>();

		foreach (var save in GetSaves())
		{
			if (save.Value.RawValue is SaveData.Data sd)
			{
				if (sd.Loaded)
				{
					loadedSaves.Add(save.Value);
				}
			}
		}

		return loadedSaves;
	}

	public void Copy(string fromName, string toName)
	{
		if (!Exists(fromName))
		{
			var ex = new SaveDataNotFoundException($"Cannot copy from non-existant save data {fromName}!");
			this.Emit<SaveDataMoveError>((e) => {
				e.SetName(toName);
				e.SetException(ex);
				});

			throw ex;
		}
		if (Exists(toName))
		{
			var ex = new SaveDataExistsException($"Cannot copy to existing save data {toName}!");
			this.Emit<SaveDataMoveError>((e) => {
				e.SetName(toName);
				e.SetException(ex);
				});

			throw ex;
		}

		if (Exists(fromName) && !Exists(toName))
		{
			// copy the save file on the filesystem
			var obj = Get(fromName);

			if (obj.DataEndpoint is FileEndpoint fe)
			{
				string filePath = fe.Path;
				string filePathNew = Path.Combine(filePath.GetBaseDir(), toName+"."+filePath.GetExtension());

				LoggerManager.LogDebug("Copy save data", "", "fromTo", $"{filePath} => {filePathNew}");

				File.Copy(filePath, filePathNew);

				// trigger config loader for the copy
				Queue<Dictionary<string, object>> fileQueue = new Queue<Dictionary<string, object>>();
				fileQueue.Enqueue(new Dictionary<string, object> {{"configType", obj.RawValue.GetType().Namespace+"."+obj.RawValue.GetType().Name}, {"path", filePathNew}, {"name", toName}});

				Config.Loader configLoader = new Config.Loader(fileQueue);

				// subscribe to the loaded event
				configLoader.SubscribeOwner<ConfigManagerLoaderCompleted>(_On_SaveDataCopy_Completed, oneshot: true, isHighPriority: true);
				configLoader.SubscribeOwner<ConfigManagerLoaderError>(_On_SaveDataCopy_Error, oneshot: true, isHighPriority: true);
			}
		}
	}

	public void _On_SaveDataCopy_Completed(IEvent e)
	{
		_On_SaveDataLoad_Completed(e);

		LoggerManager.LogDebug("Copy process complete", "", "e", e);

		if (e is ConfigManagerLoaderCompleted ec)
		{
			foreach (var obj in ec.ConfigObjects)
			{
				this.Emit<SaveDataCopyComplete>((ee) => {
						ee.SetName(obj.Name);
						ee.SetSaveData(obj);
					});
			}
		}
	}
	public void _On_SaveDataCopy_Error(IEvent e)
	{
		_On_SaveDataLoad_Error(e);

		if (e is ConfigManagerLoaderError ele)
		{
			foreach (var obj in ele.ConfigObjects)
			{
				this.Emit<SaveDataCopyError>((en) => {
						en.SetName(obj.Name); 
						en.SetRunWorkerCompletedEventArgs(ele.RunWorkerCompletedEventArgs);
						en.SetSaveData(obj);
						en.SetException(ele.RunWorkerCompletedEventArgs.Error);
					});
			}
		}
	}

	public void Move(string fromName, string toName)
	{
		try
		{
			Copy(fromName, toName);
		}
		catch (System.Exception ex)
		{
			this.Emit<SaveDataMoveError>((e) => {
				e.SetName(toName);
				e.SetException(ex);
				});
		}

		this.SubscribeOwner<SaveDataCopyComplete>((e) => {
			if (e is SaveDataCopyComplete ec)
			{
				// delete the old save if the event is for the copied one
				if (ec.SaveData.Name == toName)
				{
					LoggerManager.LogDebug("Deleting moved save data", "", "saveName", fromName);

					Remove(fromName);

					this.Emit<SaveDataMoveComplete>((en) => {
							en.SetRunWorkerCompletedEventArgs(ec.RunWorkerCompletedEventArgs);
							en.SetName(ec.SaveData.Name);
							en.SetSaveData(ec.SaveData);
						});
				}
			}
			});
		this.SubscribeOwner<SaveDataCopyError>((e) => {
			if (e is SaveDataCopyError ec)
			{
				this.Emit<SaveDataMoveError>((en) => {
						en.SetRunWorkerCompletedEventArgs(ec.RunWorkerCompletedEventArgs);
						en.SetName(ec.SaveData.Name);
						en.SetSaveData(ec.SaveData);
						en.SetException(ec.RunWorkerCompletedEventArgs.Error);
					});
			}
			});
	}

	public void Remove(string saveName)
	{
		try
		{
			if (Get(saveName).DataEndpoint is FileEndpoint fe)
			{
				File.Delete(fe.Path);
				_saveData.Remove(saveName);

				this.Emit<SaveDataRemoveComplete>((e) => {
					e.SetName(saveName);
					});
			}
		}
		catch (System.Exception ex)
		{
			this.Emit<SaveDataRemoveError>((e) => {
				e.SetName(saveName);
				e.SetException(ex);
				});
		}
	}

	public Dictionary<string, Config.Object> GetSaves()
	{
		return _saveData;
	}

	// save slot wrapper methods
	public GameSaveFile GetSlot(int slotNumber)
	{
		return Get<GameSaveFile>($"Save{slotNumber}");
	}

	public GameSaveFile CreateSlot(int slotNumber)
	{
		 return Create<GameSaveFile>($"Save{slotNumber}");
	}

	public bool SlotExists(int slotNumber)
	{
		return Exists($"Save{slotNumber}");
	}

	public void SetSlot(int slotNumber, Config.Object saveData)
	{
		 Set($"Save{slotNumber}", saveData);
	}

	public void SaveSlot(int slotNumber)
	{
		Save($"Save{slotNumber}");
	}

	public void RemoveSlot(int slotNumber)
	{
		Remove($"Save{slotNumber}");
	}

	public void CopySlot(int fromSlot, int toSlot)
	{
		Copy($"Save{fromSlot}", $"Save{toSlot}");
	}

	public void MoveSlot(int fromSlot, int toSlot)
	{
		Move($"Save{fromSlot}", $"Save{toSlot}");
	}

	public List<GameSaveFile> GetSlotSaves(int maxSlots)
	{
		List<GameSaveFile> saveSlots = new List<GameSaveFile>();

		for (int i = 0; i < maxSlots; i++)
		{
			if (SlotExists(i))
			{
				saveSlots.Add(GetSlot(i));
			}
			else
			{
				saveSlots.Add(null);
			}
		}

		return saveSlots;
	}
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
