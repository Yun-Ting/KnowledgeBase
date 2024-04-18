## Using Callbacks
NS lets you execute custom code at several points in the serialization or deserialization process:

- OnDeserializing (when beginning to deserialize an object)
- OnDeserialized (when finished deserializing an object)
- OnSerializing (when beginning to serialize an object)
- OnSerialized (when finished serializing an object)

STJ exposes the same notifications during serialization and deserialization. To use them, implement one or more of the following interfaces from the `System.Text.Json.Serialization` namespace

- IJsonOnDeserializing
- IJsonOnDeserialized
- IJsonOnSerializing
- IJsonOnSerialized

### Implementing Callbacks using Newtonsoft code:
Newtonsoft makes use of 4 attributes, to mark a specific user-defined method as a callback function. As shown below, the `MyOnSerializing` function is annotated with the `[OnSerializing]` attribute, and as such, will be called prior to serialization being initiated.


    public class Student
    {
        public string? Name {get; set;}
        public string? Grade {get; set;}
        public int Id {get; set;}

        [OnSerializing]
        void MyOnSerializing(StreamingContext context) {
                Console.WriteLine("Started Serializing");
                // Do Something
        }

        [OnSerialized]
        void MyOnSerialized(StreamingContext context) {
                Console.WriteLine("Completed Serializing");
                // Do Something
        } 

        [OnDeserializing]
        void MyOnDeserializing(StreamingContext context) {
                Console.WriteLine("Started Deserializing");
                // Do Something
        }

        [OnDeserialized]
        void MyOnDeserialized(StreamingContext context) {
                Console.WriteLine("Completed Deserializing");
                // Do Something
        }
         ...
    }

### Implementing Callbacks using STJ code:
To implement callbacks in STJ, you need to implement the interface functions as shown below. The interface is composed of `IJsonOnSerializing`, `IJsonOnSerialized`, `IJsonOnDeserializing` and `IJsonOnDeserialized`. Implementing any of these functions enables that particular callback

    public class Student: IJsonOnSerializing, IJsonOnSerialized, IJsonOnDeserializing, IJsonOnDeserialized
    {
        public string? Name {get; set;}
        public string? Grade {get; set;}
        public int Id {get; set;}

        void IJsonOnSerializing.OnSerializing() {
                Console.WriteLine("Started Serializing");
                // Do Something
        }

        void IJsonOnSerialized.OnSerialized() {
                Console.WriteLine("Completed Serializing");
                // Do Something
        }

        void IJsonOnDeserializing.OnDeserializing() {
                Console.WriteLine("Started Deserializing");
                // Do Something
        }

        void IJsonOnDeserialized.OnDeserialized() {
                Console.WriteLine("Completed Deserializing");
                // Do Something
        }
        ...
    }

