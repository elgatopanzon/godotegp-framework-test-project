namespace Godot.EGP;

using Godot.EGP.ValidatedObjects;
using System.Collections.Generic;
using Newtonsoft.Json;

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

