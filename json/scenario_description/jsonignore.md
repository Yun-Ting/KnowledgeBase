## Ignoring fields:
NS allows specifying whether a particular field should be ignored while serializing/deserializing. This is done by means of the `[JsonIgnore]` attribute. Migrating this to STJ does not require any change, as STJ also supports this attribute.

### NS Code:
    public class Student
    {
        [JsonIgnore]
        public int Id {get; set;}
        public string Name {get; set;}
    }

### STJ Code:
    public class Student
    {
        [JsonIgnore]
        public int Id {get; set;}
        public string Name {get; set;}
    }

