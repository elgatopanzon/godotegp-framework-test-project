namespace Godot.EGP.Extensions;

using Godot;
using System;
using System.Collections.Generic;

public static class ObjectExtensions
{
	public static bool TryCast<T>(this object obj, out T result)
	{
    	if (obj is T)
    	{
        	result = (T)obj;
        	return true;
    	}

    	result = default(T);
    	return false;
	}	

	public static Dictionary<string,string> ToStringDictionary(this object obj)
	{
    	{
        	var dict = new Dictionary<string, string>();
        	if (obj != null)
        	{
        		foreach (var prop in obj.GetType().GetProperties())
        		{
                	dict.Add(prop.Name, prop.GetValue(obj).ToString());

            		// if (prop.PropertyType.IsPrimitive || prop.PropertyType == typeof(string))
            		// {
                	// 	dict.Add(prop.Name, prop.GetValue(obj).ToString());
            		// }
            		// else
            		// {
                	// 	LoggerManager.LogDebug("asd", "", "asd", prop.GetType());
                	// 	var subObj = prop.GetValue(obj);
                	// 	if (subObj != null)
                	// 	{
                	// 		Dictionary<string, string> subDict = subObj.ToStringDictionary();
                    //
                	// 		foreach (var subProp in subDict)
                	// 		{
                    // 			dict.Add($"{prop.Name}.{subProp.Key}", subProp.Value);
                	// 		}
                	// 	}
            		// }
        		}
        	}
        	return dict;
    	}
	}
}
