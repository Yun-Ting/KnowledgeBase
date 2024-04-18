# Newtonsoft.Json to System.Text.Json Migration guide
This article shows how to migrate from `Newtonsoft.Json` to `System.Text.Json`

The System.Text.Json namespace provides functionality for serializing to and deserializing from JavaScript Object Notation (JSON).

`System.Text.Json` focuses primarily on performance, security, and standards compliance. It has some key differences in default behavior and doesn't aim to have feature parity with `Newtonsoft.Json`. For some scenarios, System.Text.Json currently has no built-in functionality, but there are recommended workarounds. For other scenarios, workarounds are impractical.

Note: We'll refer to `System.Text.Json` as STJ and `Newtonsoft.Json` as NS henceforth.
## Simple serialization

Assuming the following initialization of an object of the Employee class:

        Employee employee_obj = new Employee
        {
            Name = "John Doe",
            Id = 34457,
            Role = "Junior Dev"
        };

- Serialization (Using Newtonsoft):

        using Newtonsoft.Json;
        ...
        string json = JsonConvert.SerializeObject(employee_obj);

- Serialization (Using System.Text.Json):

        using System.Text.Json;
        ...
        string json = JsonSerializer.Serialize(employee_obj);

## Simple Deserialization

- Deserialization (Using Newtonsoft):

        using Newtonsoft.Json;
        ...
        string json_str = r.ReadToEnd();
        Employee employee_obj = JsonConvert.DeserializeObject<Employee>(json_str);

- Deserialization (Using System.Text.Json):

        using System.Text.Json;
        ...
        string json_str = r.ReadToEnd();
        Employee employee_obj = JsonSerializer.Deserialize<Employee>(json_str);
