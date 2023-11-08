namespace Godot.EGP;

// operation class for File operators
class DataOperationHTTP<T> : DataOperation<T>
{
	DataOperatorHTTP _dataOperator;

	public override IDataOperator CreateOperator()
	{
		var dataOperator = new DataOperatorHTTP();

		dataOperator.OnComplete = __On_OperatorComplete;
		dataOperator.OnError = __On_OperatorError;

		return dataOperator;
	}

	public override DataOperator GetOperator()
	{
		return _dataOperator;
	}

	public DataOperationHTTP(DataEndpointHTTP httpEndpoint, object dataObject = null)
	{
		LoggerManager.LogDebug($"Creating instance");
		LoggerManager.LogDebug($"httpEndpoint {httpEndpoint}");

		_dataObject = dataObject;

		// create instance of the operator
		_dataOperator = (DataOperatorHTTP) CreateOperator();

		// set the data endpoint object
		_dataOperator.SetDataEndpoint(httpEndpoint);
	}

	public override void Load() {
		_dataOperator.Load();
	}

	public override void Save() {
		_dataOperator.Save(_dataObject);
	}
}

