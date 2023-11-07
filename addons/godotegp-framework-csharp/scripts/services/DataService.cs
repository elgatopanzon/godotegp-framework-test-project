namespace Godot.EGP;

using Godot;
using System;

public partial class DataService : Service
{
	// load/save to/from file endpoint
	public DataOperation LoadFromFile<T>(string filePath, Action<IEvent> onWorkingCb = null, Action<IEvent> onProgressCb = null, Action<IEvent> onCompleteCb = null, Action<IEvent> onErrorCb = null)
	{
		DataOperationProcessFile<T> dopf = new DataOperationProcessFile<T>(filePath, null, onWorkingCb, onProgressCb, onCompleteCb, onErrorCb);
		dopf.Load();

		return dopf.DataOperation;
	}

	public DataOperation SaveToFile<T>(string filePath, object dataObject, Action<IEvent> onWorkingCb = null, Action<IEvent> onProgressCb = null, Action<IEvent> onCompleteCb = null, Action<IEvent> onErrorCb = null)
	{
		DataOperationProcessFile<T> dopf = new DataOperationProcessFile<T>(filePath, dataObject, onWorkingCb, onProgressCb, onCompleteCb, onErrorCb);
		dopf.Save();

		return dopf.DataOperation;
	}
}
