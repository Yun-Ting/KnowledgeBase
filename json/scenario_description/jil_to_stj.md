## Migrating JIL functions calls to STJ
JIL is a separate high performant library like NS, and usage of JIL code needs to be migrated to STJ as well
### Serialization 
- JIL code

        using JIL;
        ...
        using(var output = new StringWriter())
        {
            Student obj = new Student();
            JSON.Serialize(obj, output);
        }

- STJ Code

        string output = JsonSerializer.Serialize(obj);

### Deserialization
- JIL code

        using JIL;
        ...
        using(var input = new StringReader(myString))
        {
            Student result = JSON.Deserialize<Student>(input);
        }

- STJ Code

        Student result = JsonSerializer.Deserialize<Student>(myString);


## Serializing and Deserializing using streams
In STJ, serializing to and deserializing from a string consumes a lot more memory, and a better solution is to instead make use of streams. So, when migrating code, always prefer using streams whenever possible. 
Note: In addition to NS, a third library called JIL may also need to be migrated to STJ. Examples for JIL are also given below:

### Serialization (with streams):
- NS Code:

        var serializer = new JsonSerializer(options);
        using (var writer = new StreamWriter(stream))
                serializer.Serialize(writer, obj);

- STJ Code:

        JsonSerializer.Serialize(stream, obj, options);

### Deserialization (with streams)
- NS Code:

        var serializer = new JsonSerializer(options);
        using (var reader = new JsonTextReader(new StreamReader(stream)))
                serializer.Deserialize<Student>(reader);

- STJ Code:

        JsonSerializer.Deserialize<Student>(stream, options);
