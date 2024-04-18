## Deserializing an anonymous type 
In some example settings, information regarding Type to which a given JSON string is to be deserialized to is only made available at run time. NS handles this case by making use of the `JsonConvert.DeserializeAnonymousType` function call.

### NS Code:
    var defn = new { Name = "", Score = (float) 0.0};
    var obj = JsonConvert.DeserializeAnonymousType(json, defn);
    if (obj != null)
    {
        string obj_name = obj.Name;
        float obj_score = obj.Score;
        ...
    }


Doing this in STJ requires passing the object's type as the second argument to the `JsonSerializer.Deserialize` function. In addition, accessing any member of the generated object requires a few extra steps

### STJ Code:
    using System.Reflection;
    ...
    var defn = new { Name = "", Score = (float) 0.0};
    var obj = JsonSerializer.Deserialize(json, defn.GetType(), new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });
    if (obj != null)
    {
        string obj_name = "";
        float obj_score = (float) 0.0;
        var index = null;
        Type obj_type = obj.GetType();
        PropertyInfo? name_info = obj_type.GetProperty("Name");
        PropertyInfo? score_info = obj_type.GetProperty("Score");
        if (name_info != null)
        {
            
            object? name_object = name_info.GetValue(obj, index);
            if (name_object != null)
                obj_name = name_object.ToString();           
        }
        if (score_info != null)
        {
            
            object? score_object = score_info.GetValue(obj, index);
            if (score_object != null)
            {
                string? score_string = score_object.ToString();
                if (score_string != null)
                    obj_score = float.Parse(score_string);
            }
        }    
    }

Notes:
In the above code, once the deserialized object `obj` is generated, we first need its Type. Then we need to search for a given property within the Type object using the `GetProperty` function. It returns a (possibly null if the property is not found) `PropertyInfo` object, for example `name_info` and `score_info`. We then use name_info object's `GetValue` method to parse the `obj` object and obtain the actual property (in this case `name`) from `obj`.
Since the returned property is of Type Object, we need to cast it to the required type. To do this, we use the `ToString` method, which returns a string representation. Lastly, in the case of the score property, we need one more step to parse the string as a floating point number, using the `float.Parse` method.