namespace Godot.EGP;

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
