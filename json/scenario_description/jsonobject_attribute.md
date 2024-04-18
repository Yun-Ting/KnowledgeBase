# Specifying default behaviour for an entire class
The NS namespace provides the `JsonObject` attribute which can be used to annotate a class. For example:

    [JsonObject]
    class SomeClass
    {
        ...
    }

Note: The `JsonObject` attribute is used to annotate a class and any settings it enables are enabled class-wide. That is, it sets the default behaviour for every property in the class.

## Setting default ignore behaviour
This attribute is used to annotate a class, and can be passed some arguments that change the default behaviour of serialization and deserialization. For example:

### NS Code:

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    class SomeClass
    {
        public type1 property1 { get; set; }

        [JsonProperty("property2_name")]
        public type2 property2 { get; set; }

        public type3 property3 { get; set; }
        ...
    }

In the above example, the `JsonObject` attribute with the `MemberSerialization = MemberSerialization.OptIn` argument is used to modify the default serielization/deserialization behaviour for the `SomeClass` class. This is equivalent to placing `[JsonIgnore]` on every property (not already annotated with `[JsonProperty(...)]`).

    class SomeClass
    {
        [JsonIgnore]
        public type1 property1 { get; set; }

        [JsonPropertyName("property2_name")]
        public type2 property2 { get; set; }

        [JsonIgnore]
        public type3 property3 { get; set; }
        ...
    }

Reasoning:
- `property2` has a `[JsonProperty]` annotation placed on it in the original code. So `[JsonIgnore]` will not be placed on it.
- Other properties will get `JsonIgnore` since `MemberSerialization = MemberSerialization.OptIn` is passed to `JsonObject`.

Note: This is required if and only if the `MemberSerialization.OptIn` is chosen. If `MemberSerialization.OptOut` is set, or if the JsonObject attribute is used without arguments, it can be safely removed.

## Default Required behaviour:
You know what `[JsonProperty(Required = Required.Always)]` means when placed on an attribute.
In the original code, when the `[JsonObject(...)]` attribute is placed on a class with the parameter: `ItemRequired = Required.Always`, it is the same as having `[JsonProperty(Required = Required.Always)]` on every property in that class.
In such a case, during the migration, insert `[JsonRequired]` before every property.
For example:

### NS Code:

    [JsonObject(ItemRequired = Required.Always)]
    class SomeClass
    {
        [JsonProperty("property2_name", Required = Required.Default)]
        public type2 property2 { get; set; }

        [JsonIgnore]
        public type3 property3 { get; set; }

        public type1 property1 { get; set; }
        ...
    }

Reasoning:
- In the above example, `property1` has no annotations that conflict with the `Required.Always` argument passed to JsonObject. So it is annotated with `JsonRequired`.
- The `Required.Default` argument provided to `JsonProperty` placed on `property2` overrides the `Required.Always` passed to the `JsonObject` attribute. So for `property2`, `[JsonRequired]` is omitted.
- Similarly, `property3` is marked with `JsonIgnore`, so `[JsonRequired]` should not be placed on it.

STJ Code:
Corresponding migrated code:

    class SomeClass
    {
        [JsonPropertyName("property2_name")]
        public type2 property2 { get; set; }

        [JsonIgnore]
        public type3 property3 { get; set; }

        [JsonRequired]
        public type1 property1 { get; set; }
        ...
    }

## Default null value handling behaviour:
You are aware of what the `[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]` does when placed on a given property.
In the original code, when the `[JsonObject(...)]` attribute is placed on a class with the parameter `ItemNullValueHandling = NullValueHandling.Ignore`, it is the same as having `[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]` on every property in that class.
In such a case, during the migration, insert `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]` before every property.

### NS Code:

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    class SomeClass
    {
        public int property2 { get; set; }

        [JsonProperty("property1_name")]
        public type1 property1 { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public type3 property3 { get; set; }
        ...
    }

Reasoning:
- The `ItemNullValueHandling = NullValueHandling.Ignore` argument to the `JsonObject` attribute placed on the class `SomeClass` requires that every single property in the class be annotated with `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]`.
- In the case of `property2`, since its datatype is an `int`, `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]` is not added.
- In the case of `property3`, the `NullValueHandling.Include` argument overrides the `NullValueHandling.Ignore` argument passed to `JsonObject`. Therefore, `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]` is not added.

### STJ Code:
Corresponding migrated code:

    class SomeClass
    {
        public int property2 { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("property1_name")]
        public type1 property1 { get; set; }

        public type3 property3 { get; set; }
        ...
    }

Important:
- DO NOT CONFUSE `Required.Default` with `JsonIgnoreCondition.WhenWritingDefault`.
