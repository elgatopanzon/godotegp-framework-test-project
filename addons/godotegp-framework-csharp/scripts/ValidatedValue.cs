namespace Godot.EGP.ValidatedObjects;

using Godot;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;

public abstract class ValidatedValue : IValidatedValue
{
	public abstract bool Validate();
	public abstract bool IsDefault();
	public abstract bool IsNull();

	internal abstract object RawValue {set; get;}
}

public class ValidatedValue<T> : ValidatedValue
{
	protected T _value;
	protected T _default;
	protected bool NullAllowed = true;

	internal override object RawValue {
		get {
			return Value;
		}
		set {
			Value = (T) value;
		}
	}

	public T Value
	{
		get { 
			return _value;
		}
		set { 
			LoggerManager.LogDebug($"Setting value {this.GetType().Name}<{this.GetType().GetTypeInfo().GenericTypeArguments[0]}>", "", "value", value);
			_value = ValidateValue(value);
		}
	}

	protected List<ValidatedValueConstraint<T>> _constraints = new List<ValidatedValueConstraint<T>>();

	public ValidatedValue()
	{
	}
	
	public override bool IsDefault()
	{
		return (_default != null && _value != null && _value.Equals(_default));
	}
	public override bool IsNull()
	{
		return (_value == null);
	}

	public T GetDefault()
	{
		return _default;
	}

	public virtual ValidatedValue<T> Default(T defaultValue)
	{
		_default = ValidateValue(defaultValue);
		_value = defaultValue;

		LoggerManager.LogDebug("Setting default value", "", "default", defaultValue);
		LoggerManager.LogDebug("", "", "current", Value);
		return this;
	}

	public virtual ValidatedValue<T> Reset()
	{
		LoggerManager.LogDebug("Resetting value");

		return Default(_default);
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
		_default = from._default;

		foreach (ValidatedValueConstraint<T> constraint in from._constraints)
		{
			_constraints.Add(constraint);
		}

		return this;
	}

	public ValidatedValue<T> AddConstraint(ValidatedValueConstraint<T> constraint)
	{
		_constraints.Add(constraint);

		if (Value != null && !Value.Equals(default(T)))
		{
			ValidateValue(Value);
		}

		return this;
	}

	public ValidatedValue<T> NotNull()
	{
		NullAllowed = false;
		return this;
	}

	public virtual T ValidateValue(T value)
	{
		LoggerManager.LogDebug("Validating value", "", "value", new Dictionary<string, string> { { "value", value?.ToString() } , { "default", _default?.ToString() }, { "type", value?.GetType().Name } });

		if (!NullAllowed && value == null)
		{
			throw new ValidationValueIsNullException($"The {typeof(T)} value is null and NullAllowed is false");
		}

		foreach (ValidatedValueConstraint<T> constraint in _constraints)
		{
			constraint.Validate(value);
		}
		return value;
	}

	public override bool Validate()
	{
		ValidateValue(_value);
		return true;
	}

	public class ValidationValueIsNullException : Exception
	{
		public ValidationValueIsNullException() { }
		public ValidationValueIsNullException(string message) : base(message) { }
		public ValidationValueIsNullException(string message, Exception inner) : base(message, inner) { }
		protected ValidationValueIsNullException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context)
				: base(info, context) { }
	}
}

public class ValidatedNative<T> : ValidatedValue<T> where T : ValidatedObject
{
	public override ValidatedNative<T> Default(T defaultValue)
	{
		_default = ValidateValue(defaultValue);
		_value = defaultValue;

		LoggerManager.LogDebug("Setting default value", "", "default", defaultValue);
		LoggerManager.LogDebug("", "", "current", Value);
		return this;
	}

	public override ValidatedNative<T> Reset()
	{
		LoggerManager.LogDebug("Resetting value");

		return Default(_default);
	}
}
