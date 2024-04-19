/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ValidatedObjectTests
 * @created     : Thursday Apr 18, 2024 17:46:41 CST
 */

namespace GodotEGP.Tests.Objects;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.Objects.Validated.Constraint;

public partial class ValidatedObjectTests : TestContext
{
	[Fact]
	public void ValidatedObjectTests_merging_objects()
	{
		var obj1 = new ObjectTest();		
		var obj2 = new ObjectTest();		

		obj1.StringListTest = new() { "list", "asd", "ddd" };
		obj1.DictionarySizeTest = new() {{ "item1", 55 }, { "item2", 55 }, { "item3", 55 }, };
		obj1.IntTest = 7;

		obj2.MergeFrom(obj1);

		// the list won't be merged, it will instead be overritten
		Assert.Equal(new() { "list", "asd", "ddd" }, obj2.StringListTest);

		// the dictionary values will be merged by key
		Assert.Equal(55, obj2.DictionarySizeTest["item1"]);

		Assert.Equal(7, obj2.IntTest);
	}

	[Fact]
	public async void ValidatedObjectTests_change_events()
	{
		var obj = new ObjectTest();

		var tcs = new TaskCompletionSource();

		obj.SubscribeOwner<ValidatedValueChanged>((e) => {
			tcs.SetResult();
			}, isHighPriority:true);

		obj.IntTest = 5;

		await tcs.Task;
	}

	[Fact]
	public async void ValidatedObjectTests_nested_change_events()
	{
		var obj = new ObjectTest();

		var tcs = new TaskCompletionSource();

		obj.SubscribeOwner<ValidatedValueChanged>((e) => {
			tcs.SetResult();
			}, isHighPriority:true);

		obj.ObjectTestt.IntTest = 50;

		await tcs.Task;
	}
}

public partial class ObjectTest : VObject
{
	private readonly VValue<List<string>> _stringListTest;

	public List<string> StringListTest
	{
		get { return _stringListTest.Value; }
		set { _stringListTest.Value = value; }
	}

	private readonly VValue<Dictionary<string, int>> _dictionarySizeTest;

	public Dictionary<string, int> DictionarySizeTest
	{
		get { return _dictionarySizeTest.Value; }
		set { _dictionarySizeTest.Value = value; }
	}

	private readonly VValue<string> _stringTest;

	public string StringTest
	{
		get { return _stringTest.Value; }
		set { _stringTest.Value = value; }
	}

	private readonly VValue<int> _intTest;

	public int IntTest
	{
		get { return _intTest.Value; }
		set { _intTest.Value = value; }
	}

	private readonly VValue<double> _doubleTest;

	public double DoubleTest
	{
		get { return _doubleTest.Value; }
		set { _doubleTest.Value = value; }
	}

	private readonly VValue<ulong> _ulongTest;

	public ulong UlongTest
	{
		get { return _ulongTest.Value; }
		set { _ulongTest.Value = value; }
	}

	private VValue<int[]> _intArrayTest;

	public int[] IntArrayTest
	{
		get { return _intArrayTest.Value; }
		set { _intArrayTest.Value = value; }
	}

	private readonly  VValue<Vector2> _vector2Test;

	public Vector2 Vector2Test
	{
		get { return _vector2Test.Value; }
		set { _vector2Test.Value = value; }
	}

	private readonly VValue<List<VValue<Vector2>>> _recursiveTest;

	public List<VValue<Vector2>> RecursiveTest
	{
		get { return _recursiveTest.Value; }
		set { _recursiveTest.Value = value; }
	}

	private VNative<ObjectTest2> _objectTest;

	public ObjectTest2 ObjectTestt
	{
		get { return _objectTest.Value; }
		set { _objectTest.Value = value; }
	}

	public ObjectTest(VObject parent = null) : base(parent)
	{
		
		_stringListTest = AddValidatedValue<List<string>>(this)
            .Default(new List<string> { "a", "b", "c" })
            .AllowedSize(3, 8)
            .NotNull()
        	.ChangeEventsEnabled()
            ;
        _dictionarySizeTest = AddValidatedValue<Dictionary<string, int>>(this)
            .Default(new Dictionary<string, int> { { "a", 1 }, { "b", 1 }, { "c", 1 } })
            .AllowedSize(3, 8)
        	.ChangeEventsEnabled()
            ;
        _dictionarySizeTest.MergeCollections = true;

        _stringTest = AddValidatedValue<string>(this)
            .Default("string")
            .AllowedLength(5, 15)
            .AllowedValues(new string[] { "string" })
        	.ChangeEventsEnabled()
            ;
        _intTest = AddValidatedValue<int>(this)
            .Default(5)
            .AllowedRange(2, 8)
        	.ChangeEventsEnabled()
            ;
        _doubleTest = AddValidatedValue<double>(this)
            .Default(5)
            .AllowedRange(2.5, 8.8)
        	.ChangeEventsEnabled()
            ;
        _ulongTest = AddValidatedValue<ulong>(this)
            .Default(5)
            .AllowedRange(2, 8)
        	.ChangeEventsEnabled()
            ;
        _intArrayTest = AddValidatedValue<int[]>(this)
            .Default(new int[] { 1, 2, 3 })
            .AllowedSize(3, 8)
            .AllowedValues(new List<int> { 1, 2, 3 })
            .UniqueItems()
        	.ChangeEventsEnabled()
            ;
        _vector2Test = AddValidatedValue<Vector2>(this)
            .Default(new Vector2(1, 1))
            .AddConstraint(new Vector2MinMaxValue<Vector2>(1, 1, 1, 1))
        	.ChangeEventsEnabled()
            ;
        _recursiveTest = AddValidatedValue<List < VValue<Vector2>>>(this)
            .Default(new List<VValue<Vector2>>()
                    {
                    new VValue<Vector2>().Default(new Vector2(1, 1)),
                    new VValue<Vector2>().Default(new Vector2(2, 2)),
                    new VValue<Vector2>().Default(new Vector2(3, 3)),
                    }
                    )
            // .AddConstraint(new ValidationConstraintVector2MinMaxValue<Vector2>(1, 1, 1, 1))
        	.ChangeEventsEnabled()
            ;

        _objectTest = AddValidatedNative<ObjectTest2>(this)
        	.Default(new ObjectTest2(this))
        	.ChangeEventsEnabled()
        	;
	}

}

public partial class ObjectTest2 : VObject
{
	private readonly VValue<string> _stringTest;

	public string StringTest
	{
		get { return _stringTest.Value; }
		set { _stringTest.Value = value; }
	}

	private VValue<int> _intTest;

	public int IntTest
	{
		get { return _intTest.Value; }
		set { _intTest.Value = value; }
	}

	private VValue<double> _doubleTest;


	public ObjectTest2(VObject parent = null) : base(parent)
	{
        _stringTest = AddValidatedValue<string>(this)
            .Default("string100")
            .AllowedLength(5, 15)
            // .AllowedValues(new string[] { "string" })
        	.ChangeEventsEnabled()
            ;
        _intTest = AddValidatedValue<int>(this)
            .Default(50)
            .AllowedRange(20, 80)
        	.ChangeEventsEnabled()
            ;
	}
}
