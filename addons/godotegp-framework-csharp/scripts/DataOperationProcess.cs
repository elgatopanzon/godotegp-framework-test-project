namespace Godot.EGP;
using System;

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
			DataOperation.Subscribe<EventDataOperationWorking>(onWorkingCb, oneshot: true, isHighPriority: true)
				.Owner(DataOperation);
		}
		if (onProgressCb != null)
		{
			DataOperation.Subscribe<EventDataOperationProgress>(onProgressCb, oneshot: true, isHighPriority: true)
				.Owner(DataOperation);
		}
		if (onCompleteCb != null)
		{
			DataOperation.Subscribe<EventDataOperationComplete>(onCompleteCb, oneshot: true, isHighPriority: true)
				.Owner(DataOperation);
		}
		if (onErrorCb != null)
		{
			DataOperation.Subscribe<EventDataOperationError>(onErrorCb, oneshot: true, isHighPriority: true)
				.Owner(DataOperation);
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
}
