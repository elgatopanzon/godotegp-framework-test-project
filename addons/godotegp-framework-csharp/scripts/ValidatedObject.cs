namespace Godot.EGP.ValidatedObject;

using Godot;
using System;
using System.Collections.Generic;
using Godot.EGP.Extensions;
using System.Reflection;
using System.Linq;

public class ValidatedObject
{
	protected List<ValidatedValue> Properties { get; } = new List<ValidatedValue>();
    protected ValidatedValue<T> AddValidatedValue<T>()
        {
            var val = new ValidatedValue<T>();
            Properties.Add(val);
            return val;
        }

    protected ValidatedNative<T> AddValidatedNative<T>() where T : ValidatedObject
        {
            var val = new ValidatedNative<T>();
            Properties.Add(val);
            return val;
        }

	public ValidatedObject()
	{
		ValidateFields();
	}

	public void ValidateFields()
	{
		Type t = this.GetType();

		LoggerManager.LogDebug($"Validating object fields for {t.Name}");

		foreach (FieldInfo field in t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
		{
			LoggerManager.LogDebug($"Validating object field {field.Name}");

			if (field.GetType().GetMethod("Validate") != null)
			{
				if (field.GetValue(this) is IValidatedValue vv)
				{
					vv.Validate();
				}
			}
		}
	}

	public virtual void MergeFrom(ValidatedObject sourceObj)
	{
		LoggerManager.LogDebug($"Merging {sourceObj.GetType().Name}");

		for (int i = 0; i < sourceObj.Properties.Count; i++)
		{
			ValidatedValue sourceProperty = sourceObj.Properties[i];

			// LoggerManager.LogDebug("Evaluating property from source object", "", "sourcePropType", sourceProperty.GetType());
			// LoggerManager.LogDebug("", "", "sourceProp", sourceProperty);
			// LoggerManager.LogDebug("", "", "thisProp", Properties[i]);
			// LoggerManager.LogDebug("", "", "isDefault", sourceProperty.IsDefault());
			// LoggerManager.LogDebug("", "", "isNull", sourceProperty.IsNull());

			if (!sourceProperty.IsNull())
			{
				if (sourceProperty.RawValue is ValidatedObject validatedObjectSource)
				{
					if (Properties[i].RawValue is ValidatedObject validatedObjectThis)
					{
						// LoggerManager.LogDebug($"Property is type {sourceObj.GetType().Name}, recursive merging");

						validatedObjectThis.MergeFrom(validatedObjectSource);
					}
				}
				else
				{
					if (!sourceProperty.IsDefault())
					{
						// LoggerManager.LogDebug("Merging!", "", "sourceProp", sourceProperty);

						Properties[i].RawValue = sourceProperty.RawValue;
					}
				}
			}

		}
	}

	public List<ValidatedValue> GetProperties()
	{
		return Properties;
	}
}

public interface IMergeFrom<in T>
{
    void MergeFrom(T sourceObj);
}

public class ValidatedObjectTest : ValidatedObject
{
	private readonly ValidatedValue<List<string>> _stringListTest;
	private readonly ValidatedValue<Dictionary<string, int>> _dictionarySizeTest;

	// private ValidatedValue<List<string>> _stringListTest = new ValidatedValue<List<string>>()
	// 	.Default(new List<string> {"a", "b", "c"})
	// 	.AllowedSize(3, 8)
	// 	.NotNull()
	// 	;

	public List<string> StringListTest
	{
		get { return _stringListTest.Value; }
		set { _stringListTest.Value = value; }
	}

	// private ValidatedValue<Dictionary<string, int>> _dictionarySizeTest = new ValidatedValue<Dictionary<string, int>>()
	// 	.Default(new Dictionary<string, int> {{"a", 1}, {"b", 1}, {"c", 1}})
	// 	.AllowedSize(3, 8)
	// 	;

	public Dictionary<string, int> DictionarySizeTest
	{
		get { return _dictionarySizeTest.Value; }
		set { _dictionarySizeTest.Value = value; }
	}

	private readonly ValidatedValue<string> _stringTest;
	// private ValidatedValue<string> _stringTest = new ValidatedValue<string>()
	// 	.Default("string")
	// 	.AllowedLength(5, 15)
	// 	.AllowedValues(new string[] {"string"})
	// 	;

	public string StringTest
	{
		get { return _stringTest.Value; }
		set { _stringTest.Value = value; }
	}

	private readonly ValidatedValue<int> _intTest;
	// private ValidatedValue<int> _intTest = new ValidatedValue<int>()
	// 	.Default(5)
	// 	.AllowedRange(2, 8)
	// 	;

	public int IntTest
	{
		get { return _intTest.Value; }
		set { _intTest.Value = value; }
	}

	private readonly ValidatedValue<double> _doubleTest;
	// private ValidatedValue<double> _doubleTest = new ValidatedValue<double>()
	// 	.Default(5)
	// 	.AllowedRange(2.5, 8.8)
	// 	;

	public double DoubleTest
	{
		get { return _doubleTest.Value; }
		set { _doubleTest.Value = value; }
	}

	private readonly ValidatedValue<ulong> _ulongTest;
	// private ValidatedValue<ulong> _ulongTest = new ValidatedValue<ulong>()
	// 	.Default(5)
	// 	.AllowedRange(2, 8)
	// 	;

	public ulong UlongTest
	{
		get { return _ulongTest.Value; }
		set { _ulongTest.Value = value; }
	}

	private ValidatedValue<int[]> _intArrayTest;
	// private ValidatedValue<int[]> _intArrayTest = new ValidatedValue<int[]>()
	// 	.Default(new int[] {1,2,3})
	// 	.AllowedSize(3, 8)
	// 	.AllowedValues(new List<int> {1,2,3})
	// 	.UniqueItems()
	// 	;

	public int[] IntArrayTest
	{
		get { return _intArrayTest.Value; }
		set { _intArrayTest.Value = value; }
	}

	// public ValidatedValue<int[]> IntPrototypeTest = new ValidatedValue<int[]>()
	// 	.Prototype(IntArrayTest)
	// 	.Default(new int[] {1,2,3,4})
	// 	;

	private readonly  ValidatedValue<Vector2> _vector2Test;
	// private ValidatedValue<Vector2> _vector2Test = new ValidatedValue<Vector2>()
	// 	.Default(new Vector2(1, 1))
	// 	.AddConstraint(new ValidationConstraintVector2MinMaxValue<Vector2>(1, 1, 1, 1))
	// 	;

	public Vector2 Vector2Test
	{
		get { return _vector2Test.Value; }
		set { _vector2Test.Value = value; }
	}

	private readonly ValidatedValue<List<ValidatedValue<Vector2>>> _recursiveTest;
	// private ValidatedValue<List<ValidatedValue<Vector2>>> _recursiveTest = new ValidatedValue<List<ValidatedValue<Vector2>>>()
	// 	.Default(new List<ValidatedValue<Vector2>>()
	// 			{
	// 				new ValidatedValue<Vector2>().Default(new Vector2(1, 1)),
	// 				new ValidatedValue<Vector2>().Default(new Vector2(2, 2)),
	// 				new ValidatedValue<Vector2>().Default(new Vector2(3, 3)),
	// 			}
	// 			)
	// 	// .AddConstraint(new ValidationConstraintVector2MinMaxValue<Vector2>(1, 1, 1, 1))
	// 	;

	public List<ValidatedValue<Vector2>> RecursiveTest
	{
		get { return _recursiveTest.Value; }
		set { _recursiveTest.Value = value; }
	}

	// public ValidatedObjectTest(List<string> stringListTest)
	// {
	// 	StringListTest = stringListTest;
	// }
	//
	private ValidatedNative<ValidatedObjectTest2> _objectTest;
		// .Default(new Vector2(1, 1))
		// .AddConstraint(new ValidationConstraintVector2MinMaxValue<Vector2>(1, 1, 1, 1))
		// ;

	public ValidatedObjectTest2 ObjectTest
	{
		get { return _objectTest.Value; }
		set { _objectTest.Value = value; }
	}

	public ValidatedObjectTest()
	{
		
		_stringListTest = AddValidatedValue<List<string>>()
            .Default(new List<string> { "a", "b", "c" })
            .AllowedSize(3, 8)
            .NotNull()
            ;
        _dictionarySizeTest = AddValidatedValue<Dictionary<string, int>>()
            .Default(new Dictionary<string, int> { { "a", 1 }, { "b", 1 }, { "c", 1 } })
            .AllowedSize(3, 8)
            ;
        _stringTest = AddValidatedValue<string>()
            .Default("string")
            .AllowedLength(5, 15)
            .AllowedValues(new string[] { "string" })
            ;
        _intTest = AddValidatedValue<int>()
            .Default(5)
            .AllowedRange(2, 8)
            ;
        _doubleTest = AddValidatedValue<double>()
            .Default(5)
            .AllowedRange(2.5, 8.8)
            ;
        _ulongTest = AddValidatedValue<ulong>()
            .Default(5)
            .AllowedRange(2, 8)
            ;
        _intArrayTest = AddValidatedValue<int[]>()
            .Default(new int[] { 1, 2, 3 })
            .AllowedSize(3, 8)
            .AllowedValues(new List<int> { 1, 2, 3 })
            .UniqueItems()
            ;
        _vector2Test = AddValidatedValue<Vector2>()
            .Default(new Vector2(1, 1))
            .AddConstraint(new ValidationConstraintVector2MinMaxValue<Vector2>(1, 1, 1, 1))
            ;
        _recursiveTest = AddValidatedValue<List < ValidatedValue<Vector2>>>()
            .Default(new List<ValidatedValue<Vector2>>()
                    {
                    new ValidatedValue<Vector2>().Default(new Vector2(1, 1)),
                    new ValidatedValue<Vector2>().Default(new Vector2(2, 2)),
                    new ValidatedValue<Vector2>().Default(new Vector2(3, 3)),
                    }
                    )
            // .AddConstraint(new ValidationConstraintVector2MinMaxValue<Vector2>(1, 1, 1, 1))
            ;

        _objectTest = AddValidatedNative<ValidatedObjectTest2>()
        	.Default(new ValidatedObjectTest2());
	}

}

public class ValidatedObjectTest2 : ValidatedObject
{
	private readonly ValidatedValue<string> _stringTest;

	public string StringTest
	{
		get { return _stringTest.Value; }
		set { _stringTest.Value = value; }
	}

	private ValidatedValue<int> _intTest;

	public int IntTest
	{
		get { return _intTest.Value; }
		set { _intTest.Value = value; }
	}

	private ValidatedValue<double> _doubleTest;


	public ValidatedObjectTest2()
	{
        _stringTest = AddValidatedValue<string>()
            .Default("string100")
            .AllowedLength(5, 15)
            // .AllowedValues(new string[] { "string" })
            ;
        _intTest = AddValidatedValue<int>()
            .Default(50)
            .AllowedRange(20, 80)
            ;
	}
}
