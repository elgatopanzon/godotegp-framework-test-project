namespace Godot.EGP;

// interface for classes which perform direct data operations using
// IDataEndpointObject instances
public interface IDataOperator
{
	void SetDataEndpoint(IDataEndpointObject dataEndpoint);
	void Load();
	void Save(object dataObj);
}

