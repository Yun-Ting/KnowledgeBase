## Ensure Property is present in input JSON during Deserialization.
Both NS and STJ by default simply ignore a property's absence in the input JSON string. This behaviour can be changed to make them throw an error if an field is missing. NS does this by means of the `[JsonProperty(Required = Required.Always)]` attribute annotation. The exception raised when the required property is missing is `JsonSerializationException`

NS Code:

    public class SomeClass
    {
        [JsonProperty(Required = Required.Always)]
        public int property1 { get; set; }

        [JsonProperty(Required = Required.Default)]
        public string property2 { get; set; }
        ...
    };
    ...
    try {
        SomeClass? obj = JsonConvert.DeserializeObject<SomeClass>(json_string);
    } catch (JsonSerializationException jse) {
        Console.WriteLine(jse.Message);
        // do something
    }

To do the same in STJ, make use of the `[JsonRequired]` attribute instead. Also, STJ raises a `JsonException` when the required property is missing.

STJ Code:

    public class SomeClass
    {
        [JsonRequired]
        public int property1 { get; set; }

        public string property2 { get; set; }
        ...
    };
    ...
    try {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        SomeClass? obj = JsonSerializer.Deserialize<SomeClass>(json_string, options);
    } catch (JsonException je) {
        Console.WriteLine(je.Message);
        // do something
    }

Note: In the above migration `[JsonRequired]` is inserted because `Required.Always` is used in the `property1` property's `JsonProperty` annotation. In case `Required.Default` is used, `[JsonRequired]` is not inserted.

Note:
When inserting the `JsonRequired` attribute, ensure that it is inserted after a `[JsonPropertyName(...)]` attribute (if it is present).
