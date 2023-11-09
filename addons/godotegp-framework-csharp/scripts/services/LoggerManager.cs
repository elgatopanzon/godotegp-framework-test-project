namespace Godot.EGP;

using Godot;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Godot.EGP.Extensions;
using Godot.EGP.Config;

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

	private LoggerConfig _loggerConfig;
	public LoggerConfig Config
	{
		get { 
			return _loggerConfig;
		}
		set { 
			_loggerConfig = value;

			OnConfigObjectUpdated();
		}
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

	private LoggerManager() {
		// use default values for logger config
		AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
		{
			LogError(eventArgs.Exception.GetType().Name, eventArgs.Exception.TargetSite.Name, "exceptionData", eventArgs.Exception.Data);
			LogError(eventArgs.Exception.Message, eventArgs.Exception.TargetSite.Name, "stackTrace", eventArgs.Exception.StackTrace);
		};
	}

	public override void _Ready()
	{
		_SetServiceReady(true);
	}

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

		// Queue the LoggerMessage object for logging if the message is within
		// the allowed current log level value
		var currentLogLevel = LoggerMessage.DefaultLogLevel;
		if (Instance.Config != null)
		{
			currentLogLevel = Instance.Config.LogLevel;
		}

		if (logLevel >= currentLogLevel)
		{
			GetLoggerInstance(sourceType).ProcessLoggerMessage(new LoggerMessage(logLevel, logMessage, logCustom, logDataName, logData, sourceName, sourceMethodName, sourceFilename, sourceLineNumber));
		}
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

	// update config object in various moving parts
	public void OnConfigObjectUpdated()
	{
		LoggerManager.LogDebug("Updating config");
	}

	/***********************************
	*  Logging methods for LogLevels  *
	***********************************/
	public static void LogTrace( 
			object logMessage, 
			string logCustom = "", 
			string logDataName = "", 
			object logData = null,
			[System.Runtime.CompilerServices.CallerFilePath] string sourceFilename = "",
        	[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
		)
	{
		_Log(LoggerMessage.LogLevel.Trace, logMessage.ToString(), logCustom, logDataName, logData, sourceFilename, sourceLineNumber);
	}
	public static void LogDebug( 
			object logMessage, 
			string logCustom = "", 
			string logDataName = "", 
			object logData = null,
			[System.Runtime.CompilerServices.CallerFilePath] string sourceFilename = "",
        	[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
		)
	{
		_Log(LoggerMessage.LogLevel.Debug, logMessage.ToString(), logCustom, logDataName, logData, sourceFilename, sourceLineNumber);
	}
	public static void LogInfo( 
			object logMessage, 
			string logCustom = "", 
			string logDataName = "", 
			object logData = null,
			[System.Runtime.CompilerServices.CallerFilePath] string sourceFilename = "",
        	[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
		)
	{
		_Log(LoggerMessage.LogLevel.Info, logMessage.ToString(), logCustom, logDataName, logData, sourceFilename, sourceLineNumber);
	}
	public static void LogWarning( 
			object logMessage, 
			string logCustom = "", 
			string logDataName = "", 
			object logData = null,
			[System.Runtime.CompilerServices.CallerFilePath] string sourceFilename = "",
        	[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
		)
	{
		_Log(LoggerMessage.LogLevel.Warning, logMessage.ToString(), logCustom, logDataName, logData, sourceFilename, sourceLineNumber);
	}
	public static void LogError( 
			object logMessage, 
			string logCustom = "", 
			string logDataName = "", 
			object logData = null,
			[System.Runtime.CompilerServices.CallerFilePath] string sourceFilename = "",
        	[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
		)
	{
		_Log(LoggerMessage.LogLevel.Error, logMessage.ToString(), logCustom, logDataName, logData, sourceFilename, sourceLineNumber);
	}
	public static void LogCritical( 
			object logMessage, 
			string logCustom = "", 
			string logDataName = "", 
			object logData = null,
			[System.Runtime.CompilerServices.CallerFilePath] string sourceFilename = "",
        	[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
		)
	{
		_Log(LoggerMessage.LogLevel.Critical, logMessage.ToString(), logCustom, logDataName, logData, sourceFilename, sourceLineNumber);
	}
}

public class LoggerManagerConfigHandler
{
	public LoggerManagerConfigHandler()
	{
		ServiceRegistry.Get<ConfigManager>().Get<CoreEngineConfig>().SubscribeOwner<EventValidatedValueChanged>(_On_ConfigManager_ValueChanged);

	}

	private void _On_ConfigManager_ValueChanged(IEvent e)
	{
		if (e is EventValidatedValueChanged ev)
		{
			if (ev.Owner is CoreEngineConfig cec)
			{
				LoggerManager.Instance.Config = cec.LoggerConfig;
			}
		}
	}
}
