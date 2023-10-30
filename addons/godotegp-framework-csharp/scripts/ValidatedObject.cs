namespace Godot.EGP.ValidatedObject;

using Godot;
using System;
using System.Collections.Generic;
using Godot.EGP.Extensions;
using System.Reflection;

public class ValidatedObject
{
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
}

public class ValidatedObjectTest : ValidatedObject
{
	private ValidatedValue<List<string>> _stringListTest = new ValidatedValue<List<string>>()
		.Default(new List<string> {"a", "b", "c"})
		.AllowedSize(3, 8)
		.NotNull()
		;

	public List<string> StringListTest
	{
		get { return _stringListTest.Value; }
		set { _stringListTest.Value = value; }
	}

	private ValidatedValue<Dictionary<string, int>> _dictionarySizeTest = new ValidatedValue<Dictionary<string, int>>()
		.Default(new Dictionary<string, int> {{"a", 1}, {"b", 1}, {"c", 1}})
		.AllowedSize(3, 8)
		;

	public Dictionary<string, int> DictionarySizeTest
	{
		get { return _dictionarySizeTest.Value; }
		set { _dictionarySizeTest.Value = value; }
	}

	private ValidatedValue<string> _stringTest = new ValidatedValue<string>()
		.Default("string")
		.AllowedLength(5, 15)
		.AllowedValues(new string[] {"string"})
		;

	public string StringTest
	{
		get { return _stringTest.Value; }
		set { _stringTest.Value = value; }
	}

	private ValidatedValue<int> _intTest = new ValidatedValue<int>()
		.Default(5)
		.AllowedRange(2, 8)
		;

	public int IntTest
	{
		get { return _intTest.Value; }
		set { _intTest.Value = value; }
	}

	private ValidatedValue<double> _doubleTest = new ValidatedValue<double>()
		.Default(5)
		.AllowedRange(2.5, 8.8)
		;

	public double DoubleTest
	{
		get { return _doubleTest.Value; }
		set { _doubleTest.Value = value; }
	}

	private ValidatedValue<ulong> _ulongTest = new ValidatedValue<ulong>()
		.Default(5)
		.AllowedRange(2, 8)
		;

	public ulong UlongTest
	{
		get { return _ulongTest.Value; }
		set { _ulongTest.Value = value; }
	}

	private ValidatedValue<int[]> _intArrayTest = new ValidatedValue<int[]>()
		.Default(new int[] {1,2,3})
		.AllowedSize(3, 8)
		.AllowedValues(new List<int> {1,2,3})
		.UniqueItems()
		;

	public int[] IntArrayTest
	{
		get { return _intArrayTest.Value; }
		set { _intArrayTest.Value = value; }
	}

	// public ValidatedValue<int[]> IntPrototypeTest = new ValidatedValue<int[]>()
	// 	.Prototype(IntArrayTest)
	// 	.Default(new int[] {1,2,3,4})
	// 	;

	private ValidatedValue<Vector2> _vector2Test = new ValidatedValue<Vector2>()
		.Default(new Vector2(1, 1))
		.AddConstraint(new ValidationConstraintVector2MinMaxValue<Vector2>(1, 1, 1, 1))
		;

	public Vector2 Vector2Test
	{
		get { return _vector2Test.Value; }
		set { _vector2Test.Value = value; }
	}

	// public ValidatedObjectTest(List<string> stringListTest)
	// {
	// 	StringListTest = stringListTest;
	// }
}
