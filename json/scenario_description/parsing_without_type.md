## Deserializing without Type information
A common way to deserialize a provided JSON string is to use `JToken.Parse` from the `Newtonsoft.Json.Linq` namespace.

### Deserializing json input:
    var json_token = JToken.Parse(json_str);

This approach is based on the idea that JSON is built upon 3 kinds of tokens:
- `objects` -> a dictionary like structure of key-token pairs
- `arrays` -> a list like ordered collection of tokens
- `values` -> Primitives such as true, false, null, numeric values or strings

In this vein, the `JToken` is a general base class from which the classes `JObject` (for json objects), `JArray` (for json arrays) and `JValue` (for json values) are derived.
Note: Most often the `JToken.Parse` method will be used to deserialize the provided JSON string. If in case `JObject.Parse` is used instead, it means that the expected structure of the JSON data is an dictionary. Similarly, if `JArray.Parse` is used, a list like JSON structure is assumed. However, unlike `JToken.Parse`, which will work in every case so long as the JSON string is well formed, `JObject.Parse` will throw an error if an array is found and similarly for `JArray.Parse` with dictionary data.

### Accessing fields within the JSON structure:
Consider the following JSON example:

    {
        "Name": "John Doe",
        "id": 2,
        "subjects": [
            "English", "Math", "Science"
        ],
        "percentage": 98.1,
        "data": {
            "role": "researcher",
            "year": 2,
            "team": "ai4code"
        }
    }

Here, the fields: `Name`, `id` and `percentage` are values. `subjects` field is an array of values. The `data` field is an object, and its nested fields `role`, `year` and `team` are also values.
So in order to access each of them:

    var json_structure = JToken.Parse(input_json);
    var data_field = json_structure["data"];
    var Name_field = json_structure["Name"];
    var id_field = json_structure["id"];
    var percentage_field = json_structure["percentage"];
    var subjects_field = json_structure["subjects"];
    var second_subject = subjects_field[1];                // 0 indexed list
    var role_field = data_field["role"];                   // "role" is nested within "data"

The field types are automatically inferred. For example, the `percentage_field` variable is saved as a float. It is possible to change this behaviour by means of a type-cast:
For example, the `id_field` variable is stored as an integer. To convert it to a float, simply use: `var id_field = ((float) json_structure["id"]);`

### Iterating over fields:
Each JToken Object (JObject and JArray) permits iterating over their values as shown below:

        foreach(Jtoken token in nested_obj){
            // Do Something
        }

Note: the tokens iterate over the values stored in `nested_obj`. Suppose `nested_obj` corresponded to the `data` field (a dictionary like structure with key-value pairs) in the JSON example above, the `token` variable would have iterated over the values: `researcher`, `2` and `ai4code`. To find the key corresponding to a given value such as `ai4code`, the `Path` member of a JToken object is used. If `nested_obj` corresponded to the `subjects` field (a list like structure), the `token` variable would have iterated over the values: "English", "Math" and "Science".

### Finding the Path with respect to the root element of the current JSON structure:
When iterating over a dictionary, in order to find the key corresponding to the value of the current token, we make use of the `Path` member. A token with a Path value of "data.role" indicates that the token contains the value of the `role` subfield in the `data` field. Similarly, a Path value of "subjects[1]", indicates that the token contains the value of the index 1 element in `subjects` array.

    Console.WriteLine(json_obj["data"]["year"].Path);      // Output: data.year
    Console.WriteLine(json_obj["subjects"][1].Path);       // Output: subjects[1]

### Deleting an object from the JSON tree:
Every JToken object provides a `Remove` method to delete itself from the JSON Tree. For example, the following code will delete the `data` subfield from the given JSON data:

    json_structure["data"].Remove();

### Casting to a particular object type
Every JToken object has a `ToObject` method which allows casting it to a particular data type.

    Data obj = json_structure["data"]<Data>.ToObject();

### Getting the string representation:
Every JToken object has a `ToString` method that prints the string contents of the JSON data. Without any arguments this method returns a formatted string, with tab indentations and newlines. This behaviour can be disabled by explicitly providing `Formatting.None` as an argument.

    json_structure["data"].ToString();
    json_structure["data"].ToString(Formatting.None);

## Migrating to STJ
Migrating the above uses of NS code require the use of `JsonNode` objects. `JsonNode.Parse` generates a JSON Tree that can be modified.

    JsonNode json_structure = JsonNode.Parse(json_string);         // Modifiable json_structure

### Type of JsonNode object:
Similar to JObject in NS code, JsonNode objects has 3 derived types: `JsonObject` (for dictionary like structures having key-value pairs), `JsonArray` (for list like structures) and `JsonValue` (for primitives such as true, null, int, float, strings, etc.)
To test a given JsonNode's type (refer to the provided JSON example):

    Console.WriteLine(json_structure["Name"] is JsonValue);        // Output: true
    Console.WriteLine(json_structure["Name"] is JsonObject);       // Output: false
    Console.WriteLine(json_structure["Name"] is JsonArray);        // Output: false
    Console.WriteLine(json_structure["subjects"] is JsonValue);    // Output: false
    Console.WriteLine(json_structure["subjects"] is JsonObject);   // Output: false
    Console.WriteLine(json_structure["subjects"] is JsonArray);    // Output: true
    Console.WriteLine(json_structure["data"] is JsonValue);        // Output: false
    Console.WriteLine(json_structure["data"] is JsonObject);       // Output: true
    Console.WriteLine(json_structure["data"] is JsonArray);        // Output: false

In the above JSON example, both `json_structure` and `json_structure["data"]` are JsonObjects. `json_structure["subjects"]` is a JsonArray and the fields `id`, `Name`, `percentage` are JsonValues. `json_structure["data"]["role"]` and json_structure["subjects"][1]  are also JsonValues (values nested within a JsonObject and JsonArray respectively)

### Iterating through a JsonNode object:
A JsonNode object (only JsonObject and JsonArray) permits iteration over its fields. But in order to do so, the JsonNode object needs to be cast to the appropriate type: JsonObject (if the field is a nested dictionary with key-value pairs) or JsonArray (if the field is a nested list)

### Iterating through a JsonObject:
You first need to ensure it is a JsonObject. Each token is a key-value pair, with a string key corresponding to field name, and a JsonNode corresponding to the field value.

    foreach(JsonNode token in json_structure["data"].AsObject()){
        string key = token.Key;
        JsonNode value = token.Value;
        // Do Something
    }

### Iterating through a JsonArray:
You first need to ensure it is a JsonArray. Each JsonNode iterates over the values in the array.

    foreach(JsonNode subject in json_structure["subject"].AsArray()){
        // Do Something
    }

### Deleting a field:
STJ provides a mechanism to delete a particular node's child node from the JSON Tree.
IMPORTANT: Only JsonObject and JsonArray objects have the remove method. It is necessary to cast a JsonNode object to one of these types in order to delete a subfield.
To delete a field within a JsonObject, you need to call the `Remove` method with the name of the field you want to delete. The below example removes the `data` subfield from the provided JSON

    string subfield_name = "data";
    JsonObject json_obj = json_structure.AsObject();
    json_obj.Remove(subfield_name);

To delete a field within a JsonArray at a particular index, you need to call the `RemoveAt` method with the index of the value you want to delete. The below example removes the index=1 (second element) from within the `subjects` list.

    int index = 1;
    JsonArray json_arr = json_structure["subjects"].AsArray();
    json_arr.RemoveAt(index);

It is also possible to delete a value from within a JsonArray directly. In the below example, a particular element from the `subjects` list is selected, and passed as an argument to the `Remove` method. This removes the first instance of that JsonNode from within the list.

    JsonNode value = json_structure["subjects"][1];
    ...
    json_arr["subjects"].Remove(value);

A crucial difference between the NS JToken Remove method and STJ JsonObject (or JsonArray) Remove method is that the JToken Remove method does not take any arguments, whereas the STJ code takes as an argument the name of the child property to be deleted.

###  Querying the Path for a given member:
Similar to NS's code, JsonNode objects offer a `GetPath` method to obtain the path of the given element with respect to the root JSON element

    json_structure["Name"].GetPath()          // Returns $.Name
    json_structure["data"]["role"].GetPath()  // Returns $.data.role
    json_structure["subjects"][1].GetPath()   // Returns $.subjects[1]

Important: The Path syntax for JsonNode objects in STJ is slightly different from JTokens in NS. Here every path starts with the `$.` prefix. For example: `json_obj["subjects"][1].GetPath()` queries the Path for the index=1 element in the  subject field in the input JSON. The resultant output is `$.subjects[1]`

### Casting to a particular object type
Just like NS, JsonNode objects provide a mechanism with which to cast stored JSON data to a particular type:

    Data obj = json_structure["data"].Deserialize<Data>();

### Getting the string representation:
Just like NS, JsonNode objects have a `ToString` method. In addition, to obtain a string with no formatting, a `ToJsonString` (equivalent to `JToken.ToString(Formatting.None)`) method is also provided.

    json_structure["data"].ToString();
    json_structure["data"].ToJsonString();     // JSON string with no formatting
