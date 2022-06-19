# Rundo
## Unity Runtime Editor - Inspector, Hierarchy, Undo

Rundo is a simple yet powerfull runtime editor made in Unity which allows for using Unity's editor basic (but most important) functions in the runtime.

### Features
- **Runtime environment** - Rundo editor and editor-playmode work in the runtime environment, which allows for the same code-base for the whole app-context (editor, editor-playmode, standalone playmode). No need to use `#if UNITY_EDITOR` branching.
- **Seamless playmode** - because everything run in the same runtime app-context, switching between editor/editor-playmode is basically instant which boosts prototyping/iterations a lot.
- **JSON serialization** - all data is serialized into a standardized JSON which is broadly supported - so it can be used for example to easily extract the logic part of the data to be used in the data validator on the server-side.
- **Undo system** - using underlaying commands system allows for keeping all data changes in the Undo stack. It is also a good practice to avoid modifying data directly, but by using commands. The same command system can be used in the playmode as well, so even playmode can use Undo stack which speeds the game development even more !
- **Tab system (multi-scene edit)** - Rundo editor supports tab system where you can open multiple scenes at the same time, with an option to copy/paste between tabs. Each tab has its own instance of UI, World, and an Undo stack.
- **Inspector** - system to draw UI elements for any object and its serializable fields/properties. Supports multi-edit (draws the same fields/properties over the set of objects). Supports Undo system and customization.
- **Unlimited UI/UX customization** - thanks to runtime environment, Rundo editor is not bounded by some predefined layout system. You can create any UI/UX over the underlaying data model. Rundo editor by default provides implementations of Inspector, Hierarchy and a Project windows.

## Data model

### Instantiation

To instantiate any data always use data factory and it is a good practice to declare protected constructor for each of your own classes so the compiler forces you to use the external data factory.

```sh
public class Data
{
    public int Foo;
    
    protected Data() {}
}

var dataInstance = RundoEngine.DataFactory.Instantiate<Data>();
```

### Child -> Parent hierarchy

Implementing `IParentable` interface provides your data with a child -> parent hierarchy system. Rundo editor implicitly sets this hierarchy in the JSON deserialization postprocess. And in the data instantiation process, you can provide a parent instance as an argument.

If you are able to, you can use `BaseData` class as a base class for your datamodel. BaseData class implements all common low-level data functions - just extend from this class and you can use everything right-away. Or check this class to see how to implement these common interfaces.

```sh
public class DataParent : BaseData
{
    public string Foo = "Foo";
    protected DataParent() {}
}

public class DataChild : BaseData
{
    public string Boo = "Boo";
    protected DataChild() {}
}

// instantiate parent data
var dataParentInstance = RundoEngine.DataFactory.Instantiate<DataParent>();
// instantiate child data with the parent data as an argument
var dataChildInstance = RundoEngine.DataFactory.Instantiate<DataParent>(dataParentInstance);
// now we can access parent data from the child data instance
Debug.Log(dataChildInstance.GetParentInHierarchy<DataParentInstance>().Foo);

```

### Polymorphism

Because the standard JSON does not support data polymorphism, Rundo editor needs to provide the final type (into which should be data desrialized) by its own. And because serializing the final type by its name has drawbacks (for example, it is really hard to refactor the type name once that name has been serialized) - instead Rundo editor uses a guid value to represent the final type, which is independent of the type name (so its refactor-safe).

Polymorphism setup then requires these two simple steps:
- Add `IPolymorphismBase` empty interface to a base class
- Add `DataTypeId` attribute to the final type which extends from the base class from the previous step. This attribute takes a guid as an input value which is then implicitly serialized.

You can use various online tools to generate guids, for example:
https://www.guidgenerator.com/online-guid-generator.aspx

This is the base type which is extended into a different types:
```sh
public class BaseValue : IPolymorphismBase {}
```
Extended type - this type is represented by `[DataTypeId]` guid. Once serialized, this guid should always represents this class. But of course when needed, you can easily replace one guid for another in the serialized data to redirect data to a different type.
```sh
[DataTypeId("66d2d27d-8e3c-49fe-905c-9acfd72d493a")]
public class ExtendedValue : BaseValue {}
```
Main type declaring just the base type:
```sh
public class Data
{
    public BaseValue Value;
}
```
To prove polymorphism works, we will instantiate an extended type into the Value field and then serialize and deserialize Data instance.
```sh
var data = RundoEngine.DataFactory.Instantiate<Data>();
data.BaseValue = RundoEngine.DataFactory.Instantiate<ExtendedValue>();
```
Serialize data into a JSON string and then deserialize this string into a new instance (which is basically a copy of data instance).
```sh
string dataSerialized = RundoEngine.DataSerializer.Serialize(data);
var dataCopy = RundoEngine.DataSerializer.Deserialize<Data>(dataSerialized);
```
Now if we output the type of the Value in the copy instance, we should get "ExtendedValue" instead of "BaseValue"
```sh
Debug.Log(dataCopy.BaseValue.GetType().Name);
Console Output: "ExtendedValue"
```

### Modifying

### Serialization

### Typed model

## Predefined data

### DataScene

### DataGameObject

### DataComponent

## UI -> Data mapper

