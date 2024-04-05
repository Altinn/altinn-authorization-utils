# Altinn.Urn

Altinn.Urn is a .NET library designed to simplify working with URNs (Uniform Resource Names) in .NET applications. It provides functionalities to create custom URN types, serialize and deserialize them to/from JSON, and streamline the process of creating URNs within your applications.

## Features

Altinn.Urn offers the following key features:

1. **Custom URN Types**: Easily define and create custom URN types in your .NET applications. This allows you to establish contracts that require specific types of URNs, providing clarity and consistency in your codebase.

2. **JSON Serialization and Deserialization**: Seamlessly serialize and deserialize custom URN types to and from JSON. This simplifies the integration of URNs into your application's data interchange format, making it easier to work with URNs in JSON-based APIs and data storage.

3. **Simplified URN Creation**: Once you've defined your custom URN types, Altinn.Urn simplifies the process of creating URNs within your application. This reduces boilerplate code and enhances code readability, and reduces the chance of typos.

## Getting Started

To get started with Altinn.Urn, follow these steps:

1. **Installation**: Install the Altinn.Urn package from NuGet into your .NET project.

   ```bash
   dotnet add package Altinn.Urn
   ```

2. **Define Custom URN Types**: Define your custom URN types by creating a partial record and adding the `Urn` attributes provided by Altinn.Urn. Specify the desired URN prefix and implement any additional logic specific to your URN type.

   ```csharp
   [Urn]
   public partial record MyCustomUrn
   {
       [UrnType("book")]
       public partial bool IsBook(out BookIdentifier bookId);

       [UrnType("cd")]
       [UrnType("record")]
       public partial bool IsSoundMedium(out int id);
   }
   ```

3. **Serialize and Deserialize**: Altinn.Urn automatically handles the serialization and deserialization of custom URN types to and from JSON. Simply use your custom URN types as you would any other .NET types.

   ```csharp
   var urn = MyCustomUrn.SoundMedium.Create(123456);
   string json = JsonSerializer.Serialize(urn);
   MyCustomUrn deserializedUrn = JsonSerializer.Deserialize<MyCustomUrn>(json);
   ```

4. **Create URNs**: Create instances of your custom URN types using the provided constructors, passing the necessary parameters.

   ```csharp
   var urn = MyCustomUrn.SoundMedium.Create(123456);
   ```

## Example

Here's a quick example demonstrating the usage of Altinn.Urn:

```csharp
// Define custom URN type
[Urn]
public partial record MyCustomUrn
{
    [UrnType("book")]
    public partial bool IsBook(out BookIdentifier bookId);

    [UrnType("cd")]
    [UrnType("record")]
    public partial bool IsSoundMedium(out int id);
}

// Create and serialize URN
var urn = MyCustomUrn.SoundMedium.Create(123456);
string json = JsonSerializer.Serialize(urn);

// Deserialize URN
MyCustomUrn deserializedUrn = JsonSerializer.Deserialize<MyCustomUrn>(json);
```

## Contributing

Contributions to Altinn.Urn are welcome! Feel free to open issues for bug reports, feature requests, or submit pull requests with improvements.

## License

Altinn.Urn is licensed under the [MIT License](LICENSE).
