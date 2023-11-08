namespace Godot.EGP;

using System;
using Godot.EGP.ValidatedObjects;

public interface IConfigFileObject
{
}

public abstract class ConfigFileObject : IConfigFileObject
{
	internal abstract object RawValue { get; set; }
	internal abstract string FilePath { get; set; }
	public abstract bool Loading { get; set; }

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
	private bool _loading;
	public override bool Loading
	{
		get { return _loading; }
		set { _loading = value; }
	}

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
		_loading = true;
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

				_loading = false;
			}
		}
	}
	public void _OnErrorCb(IEvent e)
	{
		LoggerManager.LogDebug("ASDAS...");
		_loading = false;
	}
}
