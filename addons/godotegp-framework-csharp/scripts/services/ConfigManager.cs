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
			LoggerManager.LogDebug($"{fileQueue.Count} config files queued for loading");

			while (fileQueue.TryDequeue(out Dictionary<string, object> dict))
			{
				// retrieve the base object from the registry
				var obj = GetConfigObjectInstance(Type.GetType(dict["configType"].ToString()));

				// set file path to current instance's path
				obj.FilePath = dict["path"].ToString();
				obj.Load();
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

public interface IValidatedObjectWrapper
{
}

public abstract class ConfigFileObject : IValidatedObjectWrapper
{
	internal abstract object RawValue { get; set; }
	internal abstract string FilePath { get; set; }

	public abstract void Load();
	public abstract void Save();

	public static ConfigFileObject Create(string parameterTypeName)
    {
        Type parameterType = Type.GetType(parameterTypeName);
        Type genericType = typeof(ConfigFileObject<>).MakeGenericType(parameterType);
        return (ConfigFileObject) Activator.CreateInstance(genericType);
    }
}

public class ConfigFileObject<T> : ConfigFileObject where T : ValidatedObject, new()
{
	private ValidatedNative<T> _validatedObject;
	public T Value
	{
		get { return _validatedObject.Value; }
		set { _validatedObject.Value = value; }
	}

	internal override object RawValue {
		get {
			return Value;
		}
		set {
			Value = (T) value;
		}
	}

	private string _filePath;
	internal override string FilePath
	{
		get { return _filePath; }
		set { _filePath = value; }
	}

	public ConfigFileObject()
	{
		_validatedObject = new ValidatedNative<T>();
		_validatedObject.Value = new T();
	}

	public override void Load()
	{
		ServiceRegistry.Get<DataService>().LoadFromFile<T>(_filePath, onCompleteCb: _OnCompleteCb, onErrorCb: _OnErrorCb);
	}
	public override void Save()
	{
		ServiceRegistry.Get<DataService>().SaveToFile<T>(_filePath, dataObject: Value, onCompleteCb: _OnCompleteCb, onErrorCb: _OnErrorCb);
	}

	public void _OnCompleteCb(IEvent e)
	{
		if (e is EventDataOperationComplete ee)
		{
			if (ee.RunWorkerCompletedEventArgs.Result is DataOperationResult<T> resultObj)
			{
				var configObj = ServiceRegistry.Get<ConfigManager>().GetConfigObjectInstance(typeof(T));

				if (configObj.RawValue is T co)
				{
					co.MergeFrom((T) resultObj.ResultObject);
				}


				LoggerManager.LogDebug("Config object merged with registery object", "", "configObj", ServiceRegistry.Get<ConfigManager>().GetConfigObjectInstance(typeof(T)));
			}
		}
	}
	public void _OnErrorCb(IEvent e)
	{
		LoggerManager.LogDebug("ASDAS...");
	}
}
