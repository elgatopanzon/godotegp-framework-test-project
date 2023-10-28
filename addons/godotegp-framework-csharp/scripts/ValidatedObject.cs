namespace Godot.EGP.ValidatedObject;

using Godot;
using System;
using System.Collections.Generic;
using System.Collections;
using Godot.EGP.Extensions;
using System.Linq;

public class ValidatedValue<T>
{
	protected T _value;

	public T Value
	{
		get { return _value; }
		set { 
			_value = ValidateValue(value);
		}
	}

	protected List<ValidatedValueConstraint<T>> _constraints = new List<ValidatedValueConstraint<T>>();

	public ValidatedValue()
	{
	}

	public virtual ValidatedValue<T> Default(T defaultValue)
	{
		Value = defaultValue;
		return this;
	}

	// constraint classes to activate constraints on an object
	public virtual ValidatedValue<T> AllowedLength(int minLength = 0, int maxLength = 0)
	{
		return AddConstraint(new ValidationConstraintMinMaxLength<T>(minLength, maxLength));
	}

	public virtual ValidatedValue<T> AllowedRange(T min, T max)
	{
		return AddConstraint(new ValidationConstraintMinMaxValue<T>(min, max));
	}

	public virtual ValidatedValue<T> AllowedSize(int min, int max)
	{
		return AddConstraint(new ValidationConstraintMinMaxItems<T>(min, max));
	}

	public virtual ValidatedValue<T> AllowedValues(IList allowedValues)
	{
		return AddConstraint(new ValidationConstraintAllowedValues<T>(allowedValues));
	}

	public virtual ValidatedValue<T> UniqueItems()
	{
		return AddConstraint(new ValidationConstraintUniqueItems<T>());
	}

	public virtual ValidatedValue<T> Prototype(ValidatedValue<T> from)
	{
		foreach (ValidatedValueConstraint<T> constraint in from._constraints)
		{
			_constraints.Add(constraint);

			Value = from.Value;
		}

		return this;
	}

	public ValidatedValue<T> AddConstraint(ValidatedValueConstraint<T> constraint)
	{
		_constraints.Add(constraint);

		if (_value != null)
		{
			ValidateValue(_value);
		}

		return this;
	}

	public virtual T ValidateValue(T value)
	{
		foreach (ValidatedValueConstraint<T> constraint in _constraints)
		{
			constraint.Validate(value);
		}
		return value;
	}
}


public class ValidatedValueConstraint<T>
{
	public virtual bool Validate(T value)
	{
		return true;
	}
}

public class ValidationConstraintUniqueItems<T> : ValidatedValueConstraint<T>
{
	public override bool Validate(T value)
	{
		if (value is IList list)
		{
			List<object> seen = new List<object>();

			foreach (var item in list)
			{
				if (seen.Contains(item))
				{
					Exception e = new ValidationNonUniqueItemException($"Item {item} is non-unique");
					e.Data.Add("nonUniqueItem", item);

					throw e;
				}

				seen.Add(item);
			}
		}
		return true;
	}

	public class ValidationNonUniqueItemException : Exception
	{
		public ValidationNonUniqueItemException() { }
		public ValidationNonUniqueItemException(string message) : base(message) { }
		public ValidationNonUniqueItemException(string message, Exception inner) : base(message, inner) { }
		protected ValidationNonUniqueItemException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context)
				: base(info, context) { }
	}
}

public class ValidationConstraintMinMaxLength<T> : ValidatedValueConstraint<T>
{
	private int _minLength;
	private int _maxLength;

	public ValidationConstraintMinMaxLength(int minLength, int maxLength)
	{
		_minLength = minLength;
		_maxLength = maxLength;
	}

	// validate method
	public override bool Validate(T value)
	{
		if (value.TryCast<string>(out string val))
		{
			if (val.Length < _minLength && _minLength > 0)
			{
				throw new ValidationMinLengthException($"'{value}' is less than minLength of {_minLength}");
			}
			if (val.Length > _maxLength && _maxLength > 0)
			{
				throw new ValidationMaxLengthException($"'{value}' is greater than maxLength of {_maxLength}");
			}
		}

		return true;
	}

	// exceptions
	public class ValidationMinLengthException : Exception
	{
		public ValidationMinLengthException(string message) : base(message) {}
	}

	public class ValidationMaxLengthException : Exception
	{
		public ValidationMaxLengthException(string message) : base(message) {}
	}
}

public class ValidationConstraintMinMaxItems<T> : ValidatedValueConstraint<T>
{
	private int _minItems;
	private int _maxItems;

	public ValidationConstraintMinMaxItems(int minItems, int maxItems)
	{
		_minItems = minItems;
		_maxItems = maxItems;
	}

	// validate method
	public override bool Validate(T value)
	{
		if (value is IList list)
		{
			if (list.Count < _minItems && _minItems > 0)
			{
				throw new ValidationMinItemsException($"{typeof(T).Name}'s '{list.Count}' items is less than the required {_minItems}");
			}
			if (list.Count > _maxItems && _maxItems > 0)
			{
				throw new ValidationMaxItemsException($"{typeof(T).Name}'s '{list.Count}' items is more than allowed {_maxItems}");
			}
		}
		else if (value is ICollection collection)
		{
			if (collection.Count < _minItems && _minItems > 0)
			{
				throw new ValidationMinItemsException($"{typeof(T).Name}'s '{collection.Count}' properties is less than the required {_minItems}");
			}
			if (collection.Count > _maxItems && _maxItems > 0)
			{
				throw new ValidationMaxItemsException($"{typeof(T).Name}'s '{collection.Count}' properties is more than allowed {_maxItems}");
			}
		}
		else 
		{
			throw new NotImplementedException($"Validation for {typeof(T).Name} is not implemented");
		}

		return true;
	}

	// exceptions
	public class ValidationMinItemsException : Exception
	{
		public ValidationMinItemsException(string message) : base(message) {}
	}

	public class ValidationMaxItemsException : Exception
	{
		public ValidationMaxItemsException(string message) : base(message) {}
	}
}

public class ValidationConstraintMinMaxValue<T> : ValidatedValueConstraint<T>
{
	private T _minValue;
	private T _maxValue;

	public ValidationConstraintMinMaxValue(T minValue, T maxValue)
	{
		_minValue = minValue;
		_maxValue = maxValue;
	}

	// validate method
	public override bool Validate(T value)
	{
		if (Comparer<T>.Default.Compare(value, _minValue) < 0 && !_minValue.Equals(0))
		{
			throw new ValidationMinValueException($"'{value}' is less than minValue of {_minValue}");
		}
		if (Comparer<T>.Default.Compare(value, _maxValue) > 0 && !_maxValue.Equals(0))
		{
			throw new ValidationMaxValueException($"'{value}' is greater than maxValue of {_maxValue}");
		}

		return true;
	}

	// exceptions
	public class ValidationMinValueException : Exception
	{
		public ValidationMinValueException(string message) : base(message) {}
	}

	public class ValidationMaxValueException : Exception
	{
		public ValidationMaxValueException(string message) : base(message) {}
	}
}

public class ValidationConstraintAllowedValues<T> : ValidatedValueConstraint<T>
{
	private IList _allowedValues;

	public ValidationConstraintAllowedValues(IList allowedValues)
	{
		_allowedValues = allowedValues;
	}

	public override bool Validate(T value)
	{
		if (value is IList list)
		{
			foreach (var item in list)
			{
				if (!_allowedValues.Contains(item))
				{
					Exception e = new ValidationIllegalItemException($"The item {item} is not an allowed item");
					e.Data.Add("allowedValues", _allowedValues);

					throw e;
				}
			}
		}
		else
		{
			if (!_allowedValues.Contains(value))
			{
				Exception e = new ValidationIllegalItemException($"{value} is not an allowed value");
				e.Data.Add("allowedValues", _allowedValues);

				throw e;
			}
		}

		return true;
	}

	public class ValidationIllegalItemException : Exception
	{
		public ValidationIllegalItemException() { }
		public ValidationIllegalItemException(string message) : base(message) { }
		public ValidationIllegalItemException(string message, Exception inner) : base(message, inner) { }
		protected ValidationIllegalItemException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context)
				: base(info, context) { }
	}
}

public class ValidatedObjectTest
{
	public ValidatedValue<List<string>> StringListTest = new ValidatedValue<List<string>>()
		.Default(new List<string> {"a", "b", "c"})
		.AllowedSize(3, 8)
		;

	public ValidatedValue<Dictionary<string, int>> DictionarySizeTest = new ValidatedValue<Dictionary<string, int>>()
		.Default(new Dictionary<string, int> {{"a", 1}, {"b", 1}, {"c", 1}})
		.AllowedSize(3, 8)
		;

	public ValidatedValue<string> StringTest = new ValidatedValue<string>()
		.Default("string")
		.AllowedLength(5, 15)
		.AllowedValues(new string[] {"string"})
		;

	public ValidatedValue<int> IntTest = new ValidatedValue<int>()
		.Default(5)
		.AllowedRange(2, 8)
		;

	public ValidatedValue<double> DoubleTest = new ValidatedValue<double>()
		.Default(5)
		.AllowedRange(2.5, 8.8)
		;

	public ValidatedValue<ulong> UlongTest = new ValidatedValue<ulong>()
		.Default(5)
		.AllowedRange(2, 8)
		;

	public ValidatedValue<int[]> IntArrayTest = new ValidatedValue<int[]>()
		.Default(new int[] {1,2,3})
		.AllowedSize(3, 8)
		.AllowedValues(new List<int> {1,2,3})
		.UniqueItems()
		;

	// public ValidatedValue<int[]> IntPrototypeTest = new ValidatedValue<int[]>()
	// 	.Prototype(IntArrayTest)
	// 	.Default(new int[] {1,2,3,4})
	// 	;
}
