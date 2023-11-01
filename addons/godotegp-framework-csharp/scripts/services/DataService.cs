namespace Godot.EGP;

using Godot;
using System;
using Godot.EGP.ValidatedObjects;

using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

public partial class DataService : Service
{
	// load data from an endpoint object into a validated T object
	public DataOperation Load(DataOperation dataOperation)
	{
		// trigger the load request
		dataOperation.Load();

		// TODO: subscribe to EventBackgroundJobComplete and Error to populate
		// DataOperationResult<T> object and create a T
		dataOperation.OnComplete = (e) => {
			LoggerManager.LogDebug("DataService load result", "", "result", e.Result);
		};
		dataOperation.OnError = (e) => {
			LoggerManager.LogDebug("DataService load error", "", "result", e.Error);
		};

		return dataOperation;
	}
}

// holds information about an endpoint to be read/write
public interface IDataEndpointObject
{
	// ???
}

// File object holding information about the provided filename and path
public class DataEndpointFile : IDataEndpointObject
{
	private string _path;
	private string _extension;
	private string _mimetype;

	public string Path
	{
		get { return _path; }
		set { _path = value; }
	}

	public string Extension
	{
		get { return _extension; }
		set { _extension = value; }
	}

	public string Mimetype
	{
		get { return _mimetype; }
		set { _mimetype = value; }
	}

	public DataEndpointFile(string filePath)
	{
        // get platform safe path from a provided unix path (because we use
        // that, because godot uses that even for windows)
        _path = System.IO.Path.Combine(filePath.Split("/"));
        _extension = System.IO.Path.GetExtension(_path);
        _mimetype = MimeType.GetMimeType(_extension);

        LoggerManager.LogDebug("Creating new instance", "", "file", this);
	}
}

// interface for classes which perform direct data operations using
// IDataEndpointObject instances
public interface IDataOperator
{
	void SetDataEndpoint(IDataEndpointObject dataEndpoint);
	void Load();
	void Save();
}

public abstract class DataOperator : BackgroundJob
{

}

// operates on file based objects 
public class DataOperatorFile : DataOperator, IDataOperator
{
	private DataEndpointFile _fileEndpoint;
	private int _operationType;

	public void Load()
	{
		LoggerManager.LogDebug($"Load from endpoint", "", "endpoint", _fileEndpoint);

		_operationType = 0;

		Run();
	}

	public void Save()
	{
		LoggerManager.LogDebug($"Save to endpoint", "", "endpoint", _fileEndpoint);

		_operationType = 1;

		Run();
	}

	public void SetDataEndpoint(IDataEndpointObject dataEndpoint) {
		_fileEndpoint = (DataEndpointFile) dataEndpoint;
	}

	public DataEndpointFile GetDataEndpoint()
	{
		return _fileEndpoint;
	}

	// background job methods
	public override void DoWork(object sender, DoWorkEventArgs e)
	{
		switch (_operationType)
		{
			case 0:
				LoadOperationDoWork(sender, e);
				break;
			case 1:
			default:
				break;
		}
	}

	public void LoadOperationDoWork(object sender, DoWorkEventArgs e)
	{
		LoggerManager.LogDebug("Load operation starting");
    	using (StreamReader reader = new StreamReader(_fileEndpoint.Path))
    	{
    		e.Result = reader.ReadToEnd();
    		ReportProgress(100);
    	}
	}

	public override void ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		LoggerManager.LogDebug("Progress changed!", "", "progress", e.ProgressPercentage);
	}

	public override void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		switch (_operationType)
		{
			case 0:
				LoadOperationRunWorkerCompleted(sender, e);
				break;
			case 1:
			default:
				break;
		}
	}

	public void LoadOperationRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		LoggerManager.LogDebug("Load operation completed", "", "result", e.Result);
	}

	public override void RunWorkerError(object sender, RunWorkerCompletedEventArgs e)
	{
		LoggerManager.LogDebug("Data operator failed");
	}
}

public interface IDataOperation
{
	public void Load();
	public void Save();

}

public abstract class DataOperation : BackgroundJob, IDataOperation
{
	public abstract void Load();
	public abstract void Save();
}

// base class for operation classes interfacing with operator classes
public abstract class DataOperation<T> : DataOperation, IDataOperation
{
	public abstract IDataOperator CreateOperator();
	public abstract DataOperator GetOperator();

	protected RunWorkerCompletedEventArgs _completedArgs;

	public void __On_OperatorComplete(RunWorkerCompletedEventArgs e)
	{
		// once operator worker is completed, run the operation worker
		_completedArgs = e;
		Run();
	}
	public void __On_OperatorError(RunWorkerCompletedEventArgs e)
	{
		// forward the completed args to simulate an error
		_On_RunWorkerError(this, e);
	}

	// operation thread methods
	public override void DoWork(object sender, DoWorkEventArgs e)
	{
		LoggerManager.LogDebug("Starting operation thread");

		DataOperationResult<T> resultObj = new DataOperationResult<T>(_completedArgs.Result);
		LoggerManager.LogDebug($"Created object instance of {typeof(T).Name}", "", "object", resultObj);

		e.Result = resultObj;

		ReportProgress(100);
	}

	public override void ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		LoggerManager.LogDebug("Data operation thread progress", "", "progress", e.ProgressPercentage);
	}

	public override void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		LoggerManager.LogDebug("Data operation thread completed");
	}

	public override void RunWorkerError(object sender, RunWorkerCompletedEventArgs e)
	{
		LoggerManager.LogDebug("Data operation thread error");
	}
}

// operation class for File operators
class DataOperationFile<T> : DataOperation<T>
{
	DataOperatorFile _dataOperator;

	public override IDataOperator CreateOperator()
	{
		var dataOperator = new DataOperatorFile();

		dataOperator.OnComplete = __On_OperatorComplete;
		dataOperator.OnError = __On_OperatorError;

		return dataOperator;
	}

	public override DataOperator GetOperator()
	{
		return _dataOperator;
	}

	public DataOperationFile(DataEndpointFile fileEndpoint)
	{
		LoggerManager.LogDebug($"Creating instance");
		LoggerManager.LogDebug($"fileEndpoint {fileEndpoint}");

		// create instance of the operator
		_dataOperator = (DataOperatorFile) CreateOperator();

		// set the data endpoint object
		_dataOperator.SetDataEndpoint(fileEndpoint);
	}

	public override void Load() {
		_dataOperator.Load();
	}

	public override void Save() {
		_dataOperator.Save();
	}
}

// accept a result object and create a ValidatedObject from T
public class DataOperationResult<T>
{
	private T _resultObject;

	public T ResultObject
	{
		get { return _resultObject; }
		set { _resultObject = value; }
	}

	public DataOperationResult(object rawObject)
	{
		LoggerManager.LogDebug("Creating result object from raw data", "", "raw", rawObject);

		if (typeof(T).BaseType == typeof(ValidatedObject))
		{
			// hold deserialisation errors
			List<string> errors = new List<string>();

			// create deserialised T object, for now it only supports strings of
			// JSON
			T deserialisedObj = Newtonsoft.Json.JsonConvert.DeserializeObject<T>((string) rawObject,
				new JsonSerializerSettings
    			{
        			Error = (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args) =>
        			{
            			errors.Add(args.ErrorContext.Error.Message);
            			args.ErrorContext.Handled = true;
        			},
        			ObjectCreationHandling = ObjectCreationHandling.Replace
    			}
			);
			
			LoggerManager.LogDebug($"{typeof(T).BaseType} object deserialised as {typeof(T).Name}", "", "object", deserialisedObj);

			// store the deserialsed object
			ResultObject = deserialisedObj;
		}

		// TODO: implement different types of raw result to T stuff?
	}
}
