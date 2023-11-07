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

		dataOperation.OnComplete = (e) => {
			LoggerManager.LogDebug("DataService load result", "", "result", e.Result);
		};
		dataOperation.OnError = (e) => {
			LoggerManager.LogDebug("DataService load error", "", "result", e.Error);
		};

		return dataOperation;
	}

	// save data from an object to a file
	public DataOperation Save(DataOperation dataOperation)
	{
		// trigger the load request
		dataOperation.Save();

		dataOperation.OnComplete = (e) => {
			LoggerManager.LogDebug("DataService save result", "", "result", e.Result);
		};
		dataOperation.OnError = (e) => {
			LoggerManager.LogDebug("DataService save error", "", "result", e.Error);
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
	void Save(object dataObj);
}

public abstract class DataOperator : BackgroundJob
{

}

// operates on file based objects 
public class DataOperatorFile : DataOperator, IDataOperator
{
	private DataEndpointFile _fileEndpoint;
	private int _operationType;

	private object _dataObject;

	public void Load()
	{
		LoggerManager.LogDebug($"Load from endpoint", "", "endpoint", _fileEndpoint);

		_operationType = 0;

		Run();
	}

	public void Save(object dataObj)
	{
		LoggerManager.LogDebug($"Save to endpoint", "", "endpoint", _fileEndpoint);
		LoggerManager.LogDebug($"", "", "dataObj", dataObj);

		_dataObject = dataObj;

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
				SaveOperationDoWork(sender, e);
				break;
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

	public void SaveOperationDoWork(object sender, DoWorkEventArgs e)
	{
		LoggerManager.LogDebug("Save operation starting", "", "object", _dataObject);

    	using (StreamWriter writer = new StreamWriter(_fileEndpoint.Path))
    	{
			// for now, serialise the object as json
			var jsonString = JsonConvert.SerializeObject(
        	_dataObject, Formatting.Indented);

    		writer.WriteLine(jsonString);

    		e.Result = true;
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
				SaveOperationRunWorkerCompleted(sender, e);
				break;
			default:
				break;
		}
	}

	public void LoadOperationRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		LoggerManager.LogDebug("Load operation completed", "", "result", e.Result);
	}
	public void SaveOperationRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		LoggerManager.LogDebug("Save operation completed", "", "result", e.Result);
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

	protected object _dataObject;

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

		// for now, if the _dataObject is null then we can assume that this is a
		// load request, therefore we proceed to create the loaded instance
		if (_dataObject == null)
		{
			DataOperationResult<T> resultObj = new DataOperationResult<T>(_completedArgs.Result);
			LoggerManager.LogDebug($"Created object instance of {typeof(T).Name}", "", "object", resultObj);

			e.Result = resultObj;
		}
		else
		{
			// copy over the completed args from the operator thread
			e.Result = _completedArgs.Result;
		}

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

	// override event methods to send different events
	public override void EmitEventDoWork(object sender, DoWorkEventArgs e)
	{
		this.Emit<EventDataOperationWorking>((ev) => ev.SetDoWorkEventArgs(e));
	}

	public override void EmitEventProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		this.Emit<EventDataOperationProgress>((ev) => ev.SetProgressChangesEventArgs(e));
	}

	public override void EmitEventRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		this.Emit<EventDataOperationComplete>((ev) => ev.SetRunWorkerCompletedEventArgs(e));
	}

	public override void EmitEventRunWorkerError(object sender, RunWorkerCompletedEventArgs e)
	{
		this.Emit<EventDataOperationError>((ev) => ev.SetRunWorkerCompletedEventArgs(e));
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

	public DataOperationFile(DataEndpointFile fileEndpoint, object dataObject = null)
	{
		LoggerManager.LogDebug($"Creating instance");
		LoggerManager.LogDebug($"fileEndpoint {fileEndpoint}");

		_dataObject = dataObject;

		// create instance of the operator
		_dataOperator = (DataOperatorFile) CreateOperator();

		// set the data endpoint object
		_dataOperator.SetDataEndpoint(fileEndpoint);
	}

	public override void Load() {
		_dataOperator.Load();
	}

	public override void Save() {
		_dataOperator.Save(_dataObject);
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


public class BackgroundJob
{
	protected BackgroundWorker worker = new BackgroundWorker();
	public Action<DoWorkEventArgs> OnWorking;
	public Action<ProgressChangedEventArgs> OnProgress;
	public Action<RunWorkerCompletedEventArgs> OnComplete;
	public Action<RunWorkerCompletedEventArgs> OnError;

	public bool IsCompleted { get; set; }

	private RunWorkerCompletedEventArgs _completedArgs;

	public RunWorkerCompletedEventArgs CompletedArgs {
		get { return _completedArgs; }
	}

	public BackgroundJob()
	{
	}

	public void _setup()
	{
		worker.DoWork += new DoWorkEventHandler(_On_DoWork);
		worker.ProgressChanged += new ProgressChangedEventHandler(_On_ProgressChanged);
		worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_On_RunWorkerCompleted);
		worker.WorkerReportsProgress = true;
		worker.WorkerSupportsCancellation = true;
	}

	public virtual void Run()
	{
		_setup();
		worker.RunWorkerAsync();
	}

	public virtual void ReportProgress(int progress)
	{
		worker.ReportProgress(progress);
	}

	// handlers for background worker events
	public virtual void _On_DoWork(object sender, DoWorkEventArgs e)
	{
		DoWork(sender, e);

		if (OnWorking != null)
		{
			OnWorking(e);
		}

		EmitEventDoWork(sender, e);
	}

	public virtual void _On_ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		ProgressChanged(sender, e);

		if (OnProgress != null)
		{
			OnProgress(e);
		}

		EmitEventProgressChanged(sender, e);
	}

	public virtual void _On_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
	 	_completedArgs = e;

		if (e.Error != null)
		{
			_On_RunWorkerError(sender, e);
			return;
		}

		RunWorkerCompleted(sender, e);

		if (OnComplete != null)
		{
			OnComplete(e);
		}

		EmitEventRunWorkerCompleted(sender, e);

		IsCompleted = true;
	}

	public virtual void _On_RunWorkerError(object sender, RunWorkerCompletedEventArgs e)
	{
		RunWorkerError(sender, e);

		if (OnError != null)
		{
			OnError(e);
		}

		EmitEventRunWorkerError(sender, e);
	}

	public virtual void EmitEventDoWork(object sender, DoWorkEventArgs e)
	{
		this.Emit<EventBackgroundJobWorking>((ev) => ev.SetDoWorkEventArgs(e));
	}

	public virtual void EmitEventProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		this.Emit<EventBackgroundJobProgress>((ev) => ev.SetProgressChangesEventArgs(e));
	}

	public virtual void EmitEventRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		this.Emit<EventBackgroundJobComplete>((ev) => ev.SetRunWorkerCompletedEventArgs(e));
	}

	public virtual void EmitEventRunWorkerError(object sender, RunWorkerCompletedEventArgs e)
	{
		this.Emit<EventBackgroundJobError>((ev) => ev.SetRunWorkerCompletedEventArgs(e));
	}

	// override these to do the work
	public virtual void DoWork(object sender, DoWorkEventArgs e)
	{
	}

	public virtual void ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
	}

	public virtual void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
	}

	public virtual void RunWorkerError(object sender, RunWorkerCompletedEventArgs e)
	{
	}
}
