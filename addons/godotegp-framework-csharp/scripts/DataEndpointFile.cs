namespace Godot.EGP;

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
