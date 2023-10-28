namespace Godot.EGP.ValidatedObject;

using Godot;
using System;
using System.Collections.Generic;
using Godot.EGP.Extensions;

public class ValidatedValue<T>
{
	private T _value;

	public T Value
	{
		get { return _value; }
		set { 
			_value = ValidateValue(value);
		}
	}

	private List<ValidatedValueConstraint<T>> _constraints = new List<ValidatedValueConstraint<T>>();

	public ValidatedValue()
	{
	}

	public virtual ValidatedValue<T> Default(T defaultValue)
	{
		Value = defaultValue;
		return this;
	}

	// constraint classes to activate constraints on an object
	public virtual ValidatedValue<T> MinLength(int minLength = 0)
	{
		_constraints.Add(new ValidationConstraintMinMaxLength<T>(minLength, 0));
		return this;
	}

	public virtual ValidatedValue<T> MaxLength(int maxLength = 0)
	{
		_constraints.Add(new ValidationConstraintMinMaxLength<T>(0, maxLength));
		return this;
	}

	public virtual ValidatedValue<T> Length(int minLength = 0, int maxLength = 0)
	{
		_constraints.Add(new ValidationConstraintMinMaxLength<T>(minLength, maxLength));
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


public class ValidatedObjectTest
{
	public ValidatedValue<string> StringTest = new ValidatedValue<string>()
		.Length(5, 15)
		.Default("str") // should invalidate
		;
}
