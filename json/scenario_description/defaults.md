# Default options for STJ
The default behaviour of STJ differs from NSJ wrt serialization and deserialization.

## Defaults to be used during serialization

- Code before migration:

        using Newtonsoft.Json;
        ...
        string json_str = JsonConvert.SerializeObject(some_class_obj);

- Code after migration:

      using System.Text.Json;
      using System.Text.Json.Serialization;
      ...
      var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
      string json_str = JsonSerializer.Serialize(some_class_obj, options);

## Defaults to be used during deserialization

- Code before migration:

      using Newtonsoft.Json;
      ...
      SomeClass obj = JsonConvert.DeserializeObject<SomeClass>(json_str);

- Code after migration:

      using System.Text.Json;
      using System.Text.Json.Serialization;
      ...
      var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
      SomeClass obj = JsonSerializer.Deserialize<SomeClass>(json_str, options);

## Default DateTime Formats
STJ defaults to using the ISO 8601 format. So any references to converters, or serializer settings mentioning ISO format can be safely ignored.

## Setting additional serialization/deserialization options for STJ
If the migration task requires settings to be enabled in addition to `JsonSerializerDefaults.Web` they can specified as shown below:

### Code to initialize options to enable indenting
        new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
                WriteIndented = true;
        };

## Stream Optimisations:
Quite often the output from serializing an object or the input while deserializing to an object needs to be copied to or from a `Stream` object of some kind.
Since NS code does not provide inbuilt support for reading from or writing to streams, an intermediate buffer or StreamWriter object needs to be created.
STJ can avoid this entire overhead as it has inbuilt support for streams:

### NS Code:

        ...
        var stream_object = SomeObj.SomeMember.Stream;
        using (var reader = new StreamReader(stream_object))
        {
            return JsonConvert.DeserializeObject<T>(reader.EndToEnd());
        }

### STJ Code:

        ...
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        return JsonSerializer.Deserialize<T>(SomeObj.SomeMember.Stream, options);

## Static Member optimisation:
Often times, the `JsonSerializer.Serialize` or `JsonSerializer.Deserializer` methods will be called from inside class methods, which requires creating a default options object (as shown above) every time the method is called.
Since the defaults do not change, this is wasteful, and can be avoided by moving these options to a static member:

### Correctly migrated STJ code but with redundant default options objects:

        class someClass
        {
                public datatype1 method1(some_parameters)
                {
                        // Do Something
                        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
                        return JsonSerializer.Deserialize<datatype1>(json_str, options);
                }

                public datatype2 method2(some_parameters)
                {
                        // Do something
                        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
                        return JsonSerializer.Deserialize<datatype2>(json_str, options);
                }

                public datatype2 method3(some_parameters)
                {
                        // Do something else
                }

                ...

        }

### Corresponding STJ code with refactored default options:

        class someClass
        {
                private static readonly JsonSerializerOptions DefaultJsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

                public datatype1 method1(some_parameters)
                {
                        // Do Something
                        return JsonSerializer.Deserialize<datatype1>(json_str, DefaultJsonOptions);
                }

                public datatype2 method2(some_parameters)
                {
                        // Do something
                        return JsonSerializer.Deserialize<datatype2>(json_str, DefaultJsonOptions);
                }

                public datatype2 method3(some_parameters)
                {
                        // Do something else
                }
                ...

        }

Note:
Static classes, or classes with just a single method are also eligible for this optimisation