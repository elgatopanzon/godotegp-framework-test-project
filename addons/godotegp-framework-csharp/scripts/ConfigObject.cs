namespace Godot.EGP;

using System;
using Godot.EGP.ValidatedObjects;

public interface IConfigFileObject
{
}

public abstract class ConfigObject : IConfigFileObject
{
	internal abstract object RawValue { get; set; }
	internal abstract IDataEndpointObject DataEndpoint { get; set; }
	internal abstract bool Loading { get; set; }

	public abstract void Load();
	public abstract void Save();

	public static ConfigObject Create(string parameterTypeName)
    {
        Type parameterType = Type.GetType(parameterTypeName);
        Type genericType = typeof(ConfigObject<>).MakeGenericType(parameterType);
        return (ConfigObject) Activator.CreateInstance(genericType);
    }
}

public class ConfigObject<T> : ConfigObject where T : ValidatedObject, new()
{
	private bool _loading;
	internal override bool Loading
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

	private IDataEndpointObject _dataEndpoint;
	internal override IDataEndpointObject DataEndpoint
	{
		get { return _dataEndpoint; }
		set { _dataEndpoint = value; }
	}

	public ConfigObject()
	{
		_validatedObject = new ValidatedNative<T>();
		_validatedObject.Value = new T();
	}

	public override void Load()
	{
		_loading = true;
		var dopf = ServiceRegistry.Get<DataService>().DataOperationFromEndpoint<T>(_dataEndpoint, Value, onCompleteCb: _OnCompleteCb, onErrorCb: _OnErrorCb);
		dopf.Load();
	}
	public override void Save()
	{
		var dopf = ServiceRegistry.Get<DataService>().DataOperationFromEndpoint<T>(_dataEndpoint, Value, onCompleteCb: _OnCompleteCb, onErrorCb: _OnErrorCb);
		dopf.Save();
	}

	public void _OnCompleteCb(IEvent e)
	{
		if (e is EventDataOperationComplete ee)
		{
			if (ee.RunWorkerCompletedEventArgs.Result is DataOperationResult<T> resultObj)
			{
				if (RawValue is T co)
				{
					co.MergeFrom((T) resultObj.ResultObject);
				}

				LoggerManager.LogDebug("Config object merged with registery object", "", "configObj", RawValue);

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
