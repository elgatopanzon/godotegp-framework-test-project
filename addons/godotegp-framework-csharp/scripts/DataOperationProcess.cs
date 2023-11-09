namespace Godot.EGP;
using System;

using System.Collections.Generic;
using System.Net.Http;

public class DataOperationProcess<T>
{
	protected object _dataObject;

	public DataOperation<T> DataOperation;

	public DataOperationProcess(DataOperation<T> dataOperation, Action<IEvent> onWorkingCb = null, Action<IEvent> onProgressCb = null, Action<IEvent> onCompleteCb = null, Action<IEvent> onErrorCb = null)
	{
		DataOperation = dataOperation;

		// subscribe to event subscriptions
		if (onWorkingCb != null)
		{
			DataOperation.SubscribeOwner<EventDataOperationWorking>(onWorkingCb, oneshot: true, isHighPriority: true);
		}
		if (onProgressCb != null)
		{
			DataOperation.SubscribeOwner<EventDataOperationProgress>(onProgressCb, oneshot: true, isHighPriority: true);
		}
		if (onCompleteCb != null)
		{
			DataOperation.SubscribeOwner<EventDataOperationComplete>(onCompleteCb, oneshot: true, isHighPriority: true);
		}
		if (onErrorCb != null)
		{
			DataOperation.SubscribeOwner<EventDataOperationError>(onErrorCb, oneshot: true, isHighPriority: true);
		}
	}

	public void Load()
	{
		DataOperation.Load();
	}
	public void Save()
	{
		DataOperation.Save();
	}
}

public class DataOperationProcessFile<T> : DataOperationProcess<T>
{
	public DataOperationProcessFile(string filePath, object dataObject, Action<IEvent> onWorkingCb = null, Action<IEvent> onProgressCb = null, Action<IEvent> onCompleteCb = null, Action<IEvent> onErrorCb = null) : base(new DataOperationFile<T>(new DataEndpointFile(filePath), dataObject), onWorkingCb, onProgressCb, onCompleteCb, onErrorCb)
	{
	}

	public DataOperationProcessFile(DataEndpointFile fileEndpoint, object dataObject, Action<IEvent> onWorkingCb = null, Action<IEvent> onProgressCb = null, Action<IEvent> onCompleteCb = null, Action<IEvent> onErrorCb = null) : base(new DataOperationFile<T>(fileEndpoint, dataObject), onWorkingCb, onProgressCb, onCompleteCb, onErrorCb)
	{
	}
}

public class DataOperationProcessHTTP<T> : DataOperationProcess<T>
{
	public DataOperationProcessHTTP(string hostname, int port = 443, string path = "/", Dictionary<string,object> urlParams = null, HttpMethod requestMethod = null, bool verifySSL = true, int timeout = 30, Dictionary<string, string> headers = null, object dataObject = null, Action<IEvent> onWorkingCb = null, Action<IEvent> onProgressCb = null, Action<IEvent> onCompleteCb = null, Action<IEvent> onErrorCb = null) : base(new DataOperationHTTP<T>(new DataEndpointHTTP(hostname, port, path, urlParams, requestMethod, verifySSL, timeout, headers), dataObject), onWorkingCb, onProgressCb, onCompleteCb, onErrorCb)
	{
	}

	public DataOperationProcessHTTP(DataEndpointHTTP httpEndpoint, object dataObject = null, Action<IEvent> onWorkingCb = null, Action<IEvent> onProgressCb = null, Action<IEvent> onCompleteCb = null, Action<IEvent> onErrorCb = null) : base(new DataOperationHTTP<T>(httpEndpoint, dataObject), onWorkingCb, onProgressCb, onCompleteCb, onErrorCb)
	{
	}
}
