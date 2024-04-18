## Serializing Enums as strings (global setting)
Consider the following class definition:

    public enum Climate { Hot, Cool, Humid, Pleasant};

    public class Weather
    {
        public int Temperature {get; set;}
        public Climate Summary {get; set;} 
    };

    Weather obj = new Weather{ Temperature=20, Summary=Climate.Cool};

Serializing the object `obj` without any additional settings will yield the JSON string: `{"Temperature":20, "Summary":2}`. This is because, the summary property is an Enum, and the value `Climate.Cool` is enumerated as 2. In order to serialize to and deserialize from `"Summary":"Cool"`, both NS and STJ provide custom converters:

### NS Code:

     JsonSerializerSettings settings = new JsonSerializerSettings();
     settings.Converters.Add(new StringEnumConverter());
     string json = JsonConvert.SerializeObject(obj, settings)
Note: The definition for `StringEnumConverter` is present in the `Newtonsoft.Json.Converters` namespace.


### STJ Code:
     string json = JsonSerializer.Serialize(obj, new JsonSerializerOptions
     {
         Converters = {
             new JsonStringEnumConverter()
         }
     });
Note: The definition for `JsonStringEnumConverter` is present in the `System.Text.Json.Serialization` namespace.
     
## Serializing Enums as strings (with attributes)
NS provides the ability to individually map an enum's constants to different strings. In the above example, the `Climate` enum would be serialized to one among "Hot", "Cool", "Humid", or "Pleasant". The below example shows how to map these constant to different names:

### NS Code:

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Climate 
    { 
        Hot, 
        Cool, 
        [EnumMember(Value="Unpleasant")]
        Humid, 
        [EnumMember(Value="Annoying")]
        Dry
    };

### STJ Code:

    class CustomNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            if (name == "Humid")
                return "Unpleasant";
            if (name == "Dry")
                return "Annoying";
            return name;
        }
    }
 
    class CustomJsonConverter : JsonStringEnumConverter
    {
        public CustomJsonConverter() : base(new CustomNamingPolicy()) {}
    }

    [JsonConverter(typeof(CustomJsonConverter))]
    public enum GameType
    {
        Hot, 
        Cool, 
        Humid, 
        Dry
    }
