## Renaming fields
Both NS and STJ allow mapping provided property names to different names as shown below.

### NS Code:
    using Newtonsoft.Json;
    ...
    public class Student
    {
        [JsonProperty("StudentId")]
        public int Id { 
            get {
                // Do something and return Id
            }; 
            set {
                // Do something and set Id
            };
        }

        [JsonProperty("StudentName")]
        public string Name { get; set; }
    };

### STJ Code:
    using System.Text.Json.Serialization;
    ...
    public class Student
    {
        [JsonPropertyName("StudentId")]
        public int Id { 
            get {
                // Do something and return Id
            }; 
            set {
                // Do something and set Id
            };
        }

        [JsonPropertyName("StudentName")]
        public string? Name { get; set;}
    };

### Reasoning:
-   The included namespace "Newtonsoft.Json" is replaced with "System.Text.Json.Serialization" as the JsonPropertyName is present in that namespace
-   The Id field is annotated with the `[JsonProperty("StudentId")]` attribute. It is replaced with `[JsonPropertyName("StudentId")]` attribute. The name "StudentId" remains unchanged. The body of the get and set methods also remains unchanged.
-   The Name field is annotated with the `[JsonProperty("StudentName")]` attribute. It is replaced with `[JsonPropertyName("StudentName")]` attribute. The name "StudentName" remains unchanged. The body of the get and set methods also remains unchanged.

Important: Regardless of what is written inside the body of the `get` and `set` methods, do not replace with `[JsonIgnore]` unless explicitly stated.

## Specific Case
In some cases class properties will be annotate simply using the `[JsonProperty]` attribute without any arguments. Here, it is not used to change the property's name during serialization/deserialization In such cases, it can be safely removed:

### NS Code:

    public class Student
    {
        public string Name { get; set; }

        [JsonProperty]
        public int Id { get; set; }
        ...
    }

### STJ Code:

    public class Student
    {
        public string Name { get; set; }

        public int Id { get; set; }
        ...
    }

Important: The `JsonProperty` attribute can only be removed if no argument is passed to it.
