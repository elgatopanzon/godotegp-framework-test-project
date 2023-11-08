namespace Godot.EGP;

using System;
using System.Collections.Generic;
using System.ComponentModel;

public class ConfigManagerLoader : BackgroundJob
{
	private Queue<Dictionary<string, object>> _loadQueue = new Queue<Dictionary<string, object>>();

	private ConfigFileObject _currentlyLoadingObj;
	private double _queueSize = 0;

	private double _queueSizeCurrent { get {
		return _loadQueue.Count;
	} }

	// private List<ConfigFileObject> _loadedConfigObjects = new List<ConfigFileObject>();
	// public List<ConfigFileObject> ConfigObjects
	// {
	// 	get { return _loadedConfigObjects; }
	// 	set { _loadedConfigObjects = value; }
	// }

	private Dictionary<Type, ConfigFileObject> _configObjects = new Dictionary<Type, ConfigFileObject>();

	public ConfigManagerLoader(Queue<Dictionary<string, object>> loadQueue)
	{
		_loadQueue = loadQueue;

		Run();
	}

	public override void DoWork(object sender, DoWorkEventArgs e)
	{
		LoggerManager.LogDebug($"{_loadQueue.Count} config files queued for loading");

		_queueSize = _loadQueue.Count;

		// keep running until the queue is empty
		while (_queueSizeCurrent > 0)
		{
			if (_loadQueue.TryPeek(out var queuedItem))
			{
				// if currently loading object is null, then we can queue a new
				// load
				if (_currentlyLoadingObj == null)
				{
					LoggerManager.LogDebug("Loading config item", "", "config", queuedItem);

					// gets the existing object from the registry so we can
					// merge with it
					// var obj = ServiceRegistry.Get<ConfigManager>().GetConfigObjectInstance(Type.GetType(queuedItem["configType"].ToString()));

					// fetch any existing and known objects of the same type
					// otherwise create a fresh one
					if (!_configObjects.TryGetValue(Type.GetType(queuedItem["configType"].ToString()), out ConfigFileObject obj))
					{
						obj = ConfigFileObject.Create(queuedItem["configType"].ToString());
					}


					// set file path to current instance's path
					obj.FilePath = queuedItem["path"].ToString();
					obj.Load();

					_currentlyLoadingObj = obj;
				}
				// while loading, just do nothing?
				else if (_currentlyLoadingObj.Loading)
				{
				}
				// if it's not loading, then assume the process ended and queue
				// the next one
				else if (!_currentlyLoadingObj.Loading)
				{
					LoggerManager.LogDebug("Loading config item process finished", "", "config", queuedItem);

					// add loaded object to list
					// _loadedConfigObjects.Add(_currentlyLoadingObj);
					_configObjects.TryAdd(_currentlyLoadingObj.RawValue.GetType(), _currentlyLoadingObj);

					e.Result = _configObjects;

					// reset currently loading object
					_currentlyLoadingObj = null;
					_loadQueue.Dequeue();


					// report progress of the load
					double progress = ((_queueSize - _queueSizeCurrent) / _queueSize) * 100;
					ReportProgress(Convert.ToInt32(progress));

					// this just allows for the progress reports to be sent
					// before the loop continues
					if (progress != 100)
					{
						System.Threading.Thread.Sleep(50);
					}
				}
			}
		}
	}

	public override void ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		LoggerManager.LogDebug("Loading configs progress", "", "progress", e.ProgressPercentage);

		this.Emit<EventConfigManagerLoaderProgress>((ee) => ee.SetProgressChangesEventArgs(e).SetProgressChangesEventArgs(e));
	}

	public override void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		LoggerManager.LogDebug("Loading configs completed", "", "result", e.Result);

		this.Emit<EventConfigManagerLoaderCompleted>((ee) => ee.SetConfigObjects(_configObjects).SetRunWorkerCompletedEventArgs(e));
	}

	public override void RunWorkerError(object sender, RunWorkerCompletedEventArgs e)
	{
		LoggerManager.LogDebug("Loading configs error");

		this.Emit<EventConfigManagerLoaderError>((ee) => ee.SetConfigObjects(_configObjects).SetRunWorkerCompletedEventArgs(e));
	}
}
