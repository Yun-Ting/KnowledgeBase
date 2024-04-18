## Serializing Default or Null valued attributes:
NS allows omitting a particular field for serialization if its value is null or default. This is done by setting the `NullValueHandling` argument in the `[JsonProperty]` attribute to `NullValueHandling.Ignore`.

### NS Code:
    public class MyClass
    {
        public int property1 {get; set;}

        [JsonProperty("property2_name", NullValueHandling = NullValueHandling.Ignore)]
        public string property2 {get; set;}

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string property3 {get; set;}
    }

### STJ Code:
The equivalent code in STJ involves replacing the `JsonProperty` attribute with the `JsonPropertyName` attribute (to rename the property) and `JsonIgnore` attribute (to state the ignore condition as shown below). The `property2` property is to be omitted only when its value is null, whereas the `property3` is to be omitted when its value is either null or default.

    using System.Text.Json.Serialization;
    ...
    public class MyClass
    {
        public int property1 {get; set;}

        [JsonPropertyName("property2_name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string property2 {get; set;}

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string property3 {get; set;}
    }

## Very Important Special Case:

### NS Code:

    public class MyClass
    {
        ...

        [JsonPropertyName(NullValueHandling = NullValueHandling.Ignore)]
        public int some_int_property { get; set; }

        [JsonPropertyName(NullValueHandling = NullValueHandling.Ignore)]
        public string some_str_property { get; set; }

        [JsonPropertyName(NullValueHandling = NullValueHandling.Ignore)]
        public SomeRecord some_class_property { get; set; }
        ...
    }

Refer to the above example. Each of the properties in the class `MyClass` are annotated with with the `JsonProperty` attribute having the `NullValueHandling.Ignore` argument passed to it. So, to migrate the code correctly, check the datatype of each property. If the datatype is NOT one among the following, add the `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]` attribute to the property
- `int`
- `uint`
- `long`
- `ulong`
- `short`
- `ushort`
- `bool`
- `byte`
- `char`
- `float`
- `double`
- `Guid`
- `DateTime`
- `DateTimeOffset`
- `TimeSpan`

Note: If the datatype has a `?` suffix (eg: `int?`, `float?`, etc) add the `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]` attribute to the property

### STJ Code

    public class MyClass
    {
        ...

        public int some_int_property { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string some_str_property { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public SomeRecord some_class_property { get; set; }
        ...
    }

Reasoning:
- The property `some_int_property` has datatype `int`, therefore `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]` is not added.
- The properties `some_str_property` and `some_class_property` have datatypes `string` and `SomeClass` respectively. These datatypes are not present in the above list. Therefore, `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]` should be added.

Note:
When inserting the `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]` attribute, ensure that it is inserted after a `[JsonPropertyName(...)]` attribute (if it is present). Same goes for the `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]` attribute.

## Handling default or null valued attributes using a global option
The above section demonstrates how can configure default ignore (when null or default) at a per property level. But this behaviour can be enabled globally:
### NS Code for default value handling:

    JsonSerializerSettings settings = new JsonSerializerSettings
    {
        DefaultValueHandling = DefaultValueHandling.Ignore,
    };
    string json_string = JsonConvert.SerializeObject(some_obj, settings);

### STJ Code for default value handling:

    JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    string json_string = JsonSerializer.Serialize(some_obj, options);

Similarly, the following code snippets describe how to migrate when handling null values.
### NS Code for null value handling:

    JsonSerializerSettings settings = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
    };
    string json_string = JsonConvert.SerializeObject(some_obj, settings);

### STJ Code for null value handling

    JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    string json_string = JsonSerializer.Serialize(some_obj, options);

Note: When migrating to STJ, do not use the flag `IgnoreNullValues`. This parameter is obsolete.

### Incorrect STJ code:

    JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        IgnoreNullValues = true;
    };
