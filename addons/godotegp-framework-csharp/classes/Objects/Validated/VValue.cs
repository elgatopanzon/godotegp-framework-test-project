namespace GodotEGP.Objects.Validated;

using Godot;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;

using GodotEGP.Logging;
using GodotEGP.Objects.Validated.Constraint;

public abstract class VValue : Validated.IVValue
{
	public abstract bool Validate();
	public abstract bool IsDefault();
	public abstract bool IsNull();

	internal abstract object RawValue {set; get;}
	internal abstract object Parent {set; get;}
}

public class VValue<T> : VValue
{
	protected T _value;
	protected T _default;
	protected bool NullAllowed = true;
	protected bool ChangeEventsState = false;

	private object _parent;
	internal override object Parent {
		set
		{
			_parent = value;
		}
		get
		{
			return _parent;
		}
	}

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
			_SetValue(value);
		}
	}
	private void _SetValue(T newValue)
	{
		LoggerManager.LogDebug($"Setting value {this.GetType().Name}<{this.GetType().GetTypeInfo().GenericTypeArguments[0]}>", "", "value", newValue);

		newValue = ValidateValue(newValue);

		if (ChangeEventsState)
		{
			Type root = this.Parent.GetType().BaseType;
			if (this.Parent is VObject vo)
			{
				vo._onValueChange(this, _value, newValue);
			}
		}

		_value = newValue;
	}

	protected List<VConstraint<T>> _constraints = new List<VConstraint<T>>();

	public VValue()
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

	public virtual VValue<T> Default(T defaultValue)
	{
		_default = ValidateValue(defaultValue);
		_value = defaultValue;

		// LoggerManager.LogDebug("Setting default value", "", "default", defaultValue);
		// LoggerManager.LogDebug("", "", "current", Value);
		return this;
	}

	public virtual VValue<T> Reset()
	{
		LoggerManager.LogDebug("Resetting value");

		return Default(_default);
	}
	

	// constraint classes to activate constraints on an object
	public virtual VValue<T> AllowedLength(int minLength = 0, int maxLength = 0)
	{
		return AddConstraint(new MinMaxLength<T>(minLength, maxLength));
	}

	public virtual VValue<T> AllowedRange(T min, T max)
	{
		return AddConstraint(new MinMaxValue<T>(min, max));
	}

	public virtual VValue<T> AllowedSize(int min, int max)
	{
		return AddConstraint(new MinMaxItems<T>(min, max));
	}

	public virtual VValue<T> AllowedValues(IList allowedValues)
	{
		return AddConstraint(new AllowedValues<T>(allowedValues));
	}

	public virtual VValue<T> UniqueItems()
	{
		return AddConstraint(new UniqueItems<T>());
	}

	public virtual VValue<T> Prototype(VValue<T> from)
	{
		_default = from._default;

		foreach (VConstraint<T> constraint in from._constraints)
		{
			_constraints.Add(constraint);
		}

		return this;
	}

	public VValue<T> AddConstraint(VConstraint<T> constraint)
	{
		_constraints.Add(constraint);

		if (Value != null && !Value.Equals(default(T)))
		{
			ValidateValue(Value);
		}

		return this;
	}

	public VValue<T> NotNull()
	{
		NullAllowed = false;
		return this;
	}

	public virtual VValue<T> ChangeEventsEnabled(bool changeEventsState = true)
	{
		ChangeEventsState = changeEventsState;
		return this;
	}

	public virtual T ValidateValue(T value)
	{
		// LoggerManager.LogDebug("Validating value", "", "value", new Dictionary<string, string> { { "value", value?.ToString() } , { "default", _default?.ToString() }, { "type", value?.GetType().Name } });

		if (!NullAllowed && value == null)
		{
			throw new ValidationValueIsNullException($"The {typeof(T)} value is null and NullAllowed is false");
		}

		foreach (VConstraint<T> constraint in _constraints)
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

