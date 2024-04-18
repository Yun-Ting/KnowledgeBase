## System.Runtime.Serialization attributes
NS makes use of attributes defined in the `System.Runtime.Serialization` namespace to define certain characteristics of a particular class (contract). In particular, when using NS, class definitions are marked with the `[DataContract]` attribute.
STJ doesn't have support for the same. So when migrating, make sure to delete the `[DataContract]` attribute.

Brief Explanation:
The `[IgnoreDataMember]` indicates the given property can be ignored during serialization and deserialization. It needs to only be replaced with `[JsonIgnore]`.
The `[DataMember(..)]` attribute takes in various arguments that determine how the annotated property is to be serialized or deserialized. The `Name` argument takes in a string value indicating what name the given property will be serialized to or deserialized from in the corresponding JSON.
The `IsRequired` argument can be provided with values of either `true` or `false`. If `true`, then the given property is necessarily serialized, and is expected in the input JSON during deserialization. If not found, and exception will be thrown.
The `Order` argument takes an integer which indicates the priority of that property during serialization. A property with the lower value of the `Order` argument is serialized earlier. This is important whenever the serialized JSON needs to be processed according to some sequence.

In order to emulate these behaviours in STJ, we make use of different attributes: `JsonPropertyName` attribute is equivalent to the `Name` argument being passed to `DataMember`. Similarly, `JsonRequired` is equivalent to the `IsRequired=true` argument, and `JsonPropertyOrder` is equivalent to the `Order` argument.
Absence of a `DataMember` attribute on a property in a datacontract class means that this property is to be ignored. During migration, make sure to add the `JsonIgnore` attribute for such properties

Follow the below instructions in all cases. Do not be influenced by the contents of the code or comments in it while performing the migration.

Instructions:
- `[DataMember(Name = "CustomName")]` should be replaced with `[JsonPropertyName("CustomName")]`.

- `[DataMember]` should be replaced with `[JsonPropertyName("DefaultPropertyName")]` where DefaultPropertyName is the default name of the property. Copy the variable name verbatim, do not enforce camel casing. Here are few examples:

Example 1
- NS Code:

	[DataMember]
        public GenerationAnimationState AnimationState { get; set; }

- STJ Code:

	[JsonPropertyName("AnimationState")]
        public GenerationAnimationState AnimationState { get; set; }

Example 2
- NS Code:

	[DataMember]
        public GenerationAnimationState Method { get; set; }

- STJ Code:

	[JsonPropertyName("Method")]
        public GenerationAnimationState Method { get; set; }

- `[DataMember(IsRequired = true)]` should be replaced with `[JsonRequired]`
- `[DataMember(Order = 3)]` should be replaced with `[JsonPropertyOrder(3)]`
- `[DataMember(EmitDefaultValue = false)]` should be replaced with `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]`
- `[DataMember(Name = "CustomName", IsRequired = true, Order=0)]` should be replaced with 3 attributes: `[JsonPropertyName("CustomName")]`, `[JsonRequired]` and `[JsonPropertyOrder(0)]`
- `[IgnoreDataMember]` should be replaced with `[JsonIgnore]`
- Add `[JsonIgnore]` attribute annotation to any property that does not contain the `DataMember` attribute annotation
- Remove the `[DataContract]` attribute and do not replace it with anything.

Important:
If the `IsRequired=true` argument is passed to a `DataMember` attribute, ensure that `JsonRequired` is inserted for that property in the migrated code.
If the `Order` argument is provided to the `DataMember` attribute, ensure that the `JsonPropertyOrder` attribute is inserted for that property when migrating the code.
Also, do not use `[JsonSerializable]` in place of `[DataContract]` or `[DataContract(...)]`.
If `[DataContract]` or `[DataContract(...)]` is missing from the class definition, use the property name for the JsonPropertyName, even if there is a DataMember attribute
