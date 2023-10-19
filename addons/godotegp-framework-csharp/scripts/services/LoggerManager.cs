namespace Godot.EGP;

using Godot;
using System;
using System.Diagnostics;
using System.Collections.Generic;

/// <summary>
/// Manage instances of <c>Logger</c> objects based on class type.
/// </summary>
public partial class LoggerManager : Service
{
	// Lazy singleton instance
	private static readonly Lazy<LoggerManager> _instance = 
		new Lazy<LoggerManager>(
			() => new LoggerManager(), isThreadSafe: true
		);

	public static LoggerManager Instance {
		get { return _instance.Value; }
	}

	// Default LoggerDestinationCollection instance used for new Logger
	// instances
	private LoggerDestinationCollection _loggerDestinationCollectionDefault;

	public LoggerDestinationCollection LoggerDestinationCollectionDefault
	{
		get { 
			if (_loggerDestinationCollectionDefault == null)
			{
				_loggerDestinationCollectionDefault = new LoggerDestinationCollection();

				// Add Godot console as default destination
				_loggerDestinationCollectionDefault.AddDestination(new LoggerDestinationGodotConsole());
			}

			return _loggerDestinationCollectionDefault;
		}
		set { _loggerDestinationCollectionDefault = value; }
	}

	private LoggerManager() { }

	// Dictionary of Logger instances
	private Dictionary<Type, Logger> _loggers = new Dictionary<Type, Logger>();

	/// <summary>
	/// Log a message with the given <c>LogLevel</c>.
	/// Automatically creates or forwards Log request to <c>Logger</c> instance.
	/// </summary>
	private static void _Log(LoggerMessage.LogLevel logLevel = LoggerMessage.LogLevel.Debug, 
			string logMessage = "", 
			string logCustom = "", 
			string logDataName = "", 
			object logData = null,
			string sourceFilename = "",
			int sourceLineNumber = 0
		)
	{
		// StackFrame instance 2 back, because that's the caller of the outter
		// logging method not this object
		StackFrame frame = new StackFrame(2);
		Type sourceType = frame.GetMethod().DeclaringType;
		string sourceMethodName = frame.GetMethod().Name;
		string sourceName = sourceType.Name;

		// Queue the LoggerMessage object for logging
		GetLoggerInstance(sourceType).ProcessLoggerMessage(new LoggerMessage(logLevel, logMessage, logCustom, logDataName, logData, sourceName, sourceMethodName, sourceFilename, sourceLineNumber));
	}

	public static Logger GetLoggerInstance(Type loggerType)
	{
        if (!Instance._loggers.TryGetValue(loggerType, out var obj) || obj is not Logger logger)
        {
            logger = new Logger(Instance.LoggerDestinationCollectionDefault);
            Instance._loggers.Add(loggerType, logger);

            LoggerManager.LogDebug($"Creating Logger instance", "", "instanceName", loggerType.FullName);
        }

        return logger;
	}

	public static void SetLoggerDestinationCollection<T>(LoggerDestinationCollection ldc)
	{
		GetLoggerInstance(typeof(T)).LoggerDestinationCollection = ldc;
	}

	/***********************************
	*  Logging methods for LogLevels  *
	***********************************/
	public static void LogTrace( 
			string logMessage = "", 
			string logCustom = "", 
			string logDataName = "", 
			object logData = null,
			[System.Runtime.CompilerServices.CallerFilePath] string sourceFilename = "",
        	[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
		)
	{
		_Log(LoggerMessage.LogLevel.Trace, logMessage, logCustom, logDataName, logData, sourceFilename, sourceLineNumber);
	}
	public static void LogDebug( 
			string logMessage = "", 
			string logCustom = "", 
			string logDataName = "", 
			object logData = null,
			[System.Runtime.CompilerServices.CallerFilePath] string sourceFilename = "",
        	[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
		)
	{
		_Log(LoggerMessage.LogLevel.Debug, logMessage, logCustom, logDataName, logData, sourceFilename, sourceLineNumber);
	}
	public static void LogInfo( 
			string logMessage = "", 
			string logCustom = "", 
			string logDataName = "", 
			object logData = null,
			[System.Runtime.CompilerServices.CallerFilePath] string sourceFilename = "",
        	[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
		)
	{
		_Log(LoggerMessage.LogLevel.Info, logMessage, logCustom, logDataName, logData, sourceFilename, sourceLineNumber);
	}
	public static void LogWarning( 
			string logMessage = "", 
			string logCustom = "", 
			string logDataName = "", 
			object logData = null,
			[System.Runtime.CompilerServices.CallerFilePath] string sourceFilename = "",
        	[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
		)
	{
		_Log(LoggerMessage.LogLevel.Warning, logMessage, logCustom, logDataName, logData, sourceFilename, sourceLineNumber);
	}
	public static void LogError( 
			string logMessage = "", 
			string logCustom = "", 
			string logDataName = "", 
			object logData = null,
			[System.Runtime.CompilerServices.CallerFilePath] string sourceFilename = "",
        	[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
		)
	{
		_Log(LoggerMessage.LogLevel.Error, logMessage, logCustom, logDataName, logData, sourceFilename, sourceLineNumber);
	}
	public static void LogCritical( 
			string logMessage = "", 
			string logCustom = "", 
			string logDataName = "", 
			object logData = null,
			[System.Runtime.CompilerServices.CallerFilePath] string sourceFilename = "",
        	[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
		)
	{
		_Log(LoggerMessage.LogLevel.Critical, logMessage, logCustom, logDataName, logData, sourceFilename, sourceLineNumber);
	}
}
