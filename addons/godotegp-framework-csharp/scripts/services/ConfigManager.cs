namespace Godot.EGP;

using Godot;
using System;
using System.IO;
using System.Collections.Generic;

using Godot.EGP.ValidatedObjects;

public partial class ConfigManager : Service
{
	private string _configBaseDir = "config";
	public string ConfigBaseDir
	{
		get { return _configBaseDir; }
		set { _configBaseDir= value; }
	}

	private List<String> _configDataDirs { get; set; }

	private Dictionary<Type, ConfigFileObject> _configObjects = new Dictionary<Type, ConfigFileObject>();

	public ConfigManager() : base()
	{
		_configDataDirs = new List<string>();
		AddConfigDataDir(ProjectSettings.GlobalizePath("res://"));
		AddConfigDataDir(OS.GetUserDataDir());
	}

	public void AddConfigDataDir(string dataDir)
	{
		LoggerManager.LogDebug("Adding config data directory", "", "dir", dataDir);
		_configDataDirs.Add(dataDir);
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// _SetServiceReady(true);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	// Called when service is registered in manager
	public override void _OnServiceRegistered()
	{
		// var obj = ValidatedObjectWrapper.Create("Godot.EGP.Config.CoreEngineConfig");
		// LoggerManager.LogDebug("VVV", "", "obj", obj);
		
		Queue<Dictionary<string, object>> fileQueue = new Queue<Dictionary<string, object>>();

		foreach (string configDataPath in _configDataDirs)
		{
			string configPath = Path.Combine(configDataPath, _configBaseDir);

			if (Directory.Exists(configPath))
			{
				DirectoryInfo d = new DirectoryInfo(configPath);
				foreach (DirectoryInfo dir in d.GetDirectories())
				{
					string configDirName = dir.ToString().GetFile();
					Type configDirType = Type.GetType(configDirName);

					LoggerManager.LogDebug("Config manager config directory", "", "dirName", configDirName);
					LoggerManager.LogDebug("Type valid", "", "typeValid", configDirType);

					// if it's a valid config object, and if the base type is
					// ValidatedObject, then let's load the content
					if (configDirType != null && configDirType.BaseType.Equals(typeof(ValidatedObject)))
					{
						// trigger creation of base object in the register
						// before queueing files for loading
						GetConfigObjectInstance(Type.GetType(configDirName.ToString()));

						foreach (FileInfo file in dir.GetFiles("*.json"))
						{
							LoggerManager.LogDebug("Queueing file for content load", "", "file", file.ToString());

							fileQueue.Enqueue(new Dictionary<string, object> {{"configType", configDirName}, {"path", "/"+file.ToString().Replace(System.Environment.CurrentDirectory, "")}});
						}
					}
				}
			}
			else
			{
				LoggerManager.LogDebug("Config path doesn't exist", "", "path", configPath);
			}
		}

		if (fileQueue.Count > 0)
		{
			// load all the config objects using ConfigManagerLoader
			ConfigManagerLoader configLoader = new ConfigManagerLoader(fileQueue);

			configLoader.Subscribe<EventConfigManagerLoaderCompleted>(_On_ConfigManagerLoaderCompleted, oneshot: true, isHighPriority: true).Owner(configLoader);
			configLoader.Subscribe<EventConfigManagerLoaderError>(_On_ConfigManagerLoaderError, oneshot: true, isHighPriority: true).Owner(configLoader);
		}
	}

	public void _On_ConfigManagerLoaderCompleted(IEvent e)
	{
		if (e is EventConfigManagerLoaderCompleted ec)
		{
			LoggerManager.LogDebug("ConfigManager: loader completed cb", "", "e", ec.ConfigObjects);	

			MergeConfigObjects(ec.ConfigObjects);

			_SetServiceReady(true);
		}
	}

	public void _On_ConfigManagerLoaderError(IEvent e)
	{
		if (e is EventConfigManagerLoaderError ee)
		{
			throw ee.RunWorkerCompletedEventArgs.Error;
		}
	}

	public void MergeConfigObjects(Dictionary<Type, ConfigFileObject> configObjects)
	{
    	foreach (KeyValuePair<Type, ConfigFileObject> pair in configObjects)
    	{
        	Type key = pair.Key;
        	ConfigFileObject value = pair.Value;


        	if (GetConfigObjectInstance(key).RawValue is ValidatedObject vo)
        	{
        		LoggerManager.LogDebug("Merging config object", "", "objType", key);
        		vo.MergeFrom(value.RawValue as ValidatedObject);
        	}
    	}
	}

	public bool RegisterConfigObjectInstance(Type configInstanceType, ConfigFileObject configFileObject)
	{
		// return true if we added the object
		if (_configObjects.TryAdd(configInstanceType, configFileObject))
		{
			LoggerManager.LogDebug("Registering config file object", "", "obj", configInstanceType.Name);

			return true;
		}

		return false;
	}

	public void SetConfigObjectInstance(ConfigFileObject configFileObject)
	{
		_configObjects[configFileObject.GetType()] = configFileObject;
	}

	public ConfigFileObject GetConfigObjectInstance(Type configInstanceType)
	{
		if(!_configObjects.TryGetValue(configInstanceType, out ConfigFileObject obj))
		{
			LoggerManager.LogDebug("Creating config file object", "", "objType", configInstanceType.Name);

			obj = ConfigFileObject.Create(configInstanceType.ToString());
			RegisterConfigObjectInstance(configInstanceType, obj);

			return obj;
		}

		return obj;
	}

	public T Get<T>() where T : ConfigFileObject
	{
		return (T) GetConfigObjectInstance(typeof(T));
	}

	// Called when service is deregistered from manager
	public override void _OnServiceDeregistered()
	{
	}

	// Called when service is considered ready
	public override void _OnServiceReady()
	{
	}
}
