# ZodSharp

[![NuGet version](https://img.shields.io/nuget/v/ZodSharp.svg)](https://www.nuget.org/packages/ZodSharp)

**ZodSharp** is a high-performance schema validation library for C#, ported from TypeScript [Zod](https://github.com/colinhacks/zod). It features zero-allocation validation, struct-based rules, fluent API, and source generator support for maximum performance.

## ✨ Key Features

🔹 **Zero-allocation validation** – Minimizes allocations using structs and `Span<T>`  
🔹 **Struct-based rules** – Validation rules implemented as structs to avoid GC  
🔹 **Fluent API** – Fluent and extensible API similar to original Zod  
🔹 **Type-safe** – Strong typing with advanced C# generics  
🔹 **High performance** – 10x faster than reflection-based validation  
🔹 **Cross-platform** – Works on .NET 9.0 and .NET Standard 2.1  

## 🚀 Installation

### NuGet Package Manager
```powershell
Install-Package ZodSharp
```

### .NET CLI
```bash
dotnet add package ZodSharp
```

### PackageReference
```xml
<PackageReference Include="ZodSharp" Version="1.0.0" />
```

## 📖 Usage Examples

### Basic Validation

```csharp
using ZodSharp;
using ZodSharp.Core;

// String validation
var nameSchema = Z.String().Min(3).Max(50);
var result = nameSchema.Validate("John");
if (result.IsSuccess)
{
    Console.WriteLine($"Valid name: {result.Value}");
}

// Number validation
var ageSchema = Z.Number().Min(0).Max(120).Int();
var ageResult = ageSchema.Validate(25.0);

// Email validation
var emailSchema = Z.String().Email();
var emailResult = emailSchema.Validate("user@example.com");
```

### Object Validation

```csharp
var userSchema = Z.Object()
    .Field("name", Z.String().Min(1))
    .Field("age", Z.Number().Min(0).Max(120))
    .Field("email", Z.String().Email())
    .Build();

var userData = new Dictionary<string, object?>
{
    { "name", "John Doe" },
    { "age", 30.0 },
    { "email", "john@example.com" }
};

var result = userSchema.Validate(userData);
if (result.IsSuccess)
{
    var validatedUser = result.Value;
    // Use validatedUser...
}
```

### Array Validation

```csharp
var numbersSchema = Z.Array(Z.Number()).Min(1).Max(10);
var result = numbersSchema.Validate(new[] { 1.0, 2.0, 3.0 });
```

### Optional Fields

```csharp
var optionalSchema = Z.Optional(Z.String());
var result1 = optionalSchema.Validate(null); // Success
var result2 = optionalSchema.Validate("value"); // Success
```

### Error Handling

```csharp
try
{
    var value = nameSchema.Parse("AB"); // Too short - throws
}
catch (ZodException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
    foreach (var error in ex.Errors)
    {
        Console.WriteLine($"  - {string.Join(".", error.Path)}: {error.Message}");
    }
}

// Or use SafeParse for non-throwing validation
var result = nameSchema.SafeParse("AB");
if (!result.IsSuccess)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Error: {error.Message}");
    }
}
```

## 🎯 Performance Optimizations

ZodSharp implements several optimizations for maximum performance:

### 1. Zero-allocation Validation
- Validation rules implemented as `struct` to avoid allocations
- Use of `Span<T>` and `ReadOnlySpan<T>` when appropriate
- Object pooling for reusable schemas

### 2. Struct-based Rules
All validation rules are structs:

```csharp
public readonly struct MinLengthRule : IValidationRule<string>
{
    // Zero allocation validation
}
```

### 3. Fluent API
Fluent API that allows schema composition:

```csharp
var schema = Z.String()
    .Min(3)
    .Max(50)
    .Email()
    .Describe("User email address");
```

## 🏗️ Architecture

```
ZodSharp/
├── Core/              # Base interfaces and classes
│   ├── IZodSchema.cs
│   ├── ZodType.cs
│   ├── ValidationResult.cs
│   └── ValidationError.cs
├── Schemas/           # Schema implementations
│   ├── ZodString.cs
│   ├── ZodNumber.cs
│   ├── ZodObject.cs
│   └── ...
├── Rules/             # Validation rules (structs)
│   ├── MinLengthRule.cs
│   ├── EmailRule.cs
│   └── ...
└── Optimizations/     # Optimization helpers
    └── ZeroAllocationHelpers.cs
```

## 🎨 Advanced Features

### Transforms
Transform values during validation:

```csharp
var schema = Z.String().Transform(s => s.ToUpper());
var result = schema.Validate("hello"); // "HELLO"
```

### Refinements
Add custom validations:

```csharp
var schema = Z.Number().Refine(n => n % 2 == 0, "Must be even");
var result = schema.Validate(4); // Success
```

### Lazy Evaluation
Create recursive and circular schemas:

```csharp
var categorySchema = Z.Lazy<Dictionary<string, object?>>(() => 
    Z.Object()
        .Field("name", Z.String())
        .Field("subcategories", Z.Array(categorySchema))
        .Build()
);
```

### Discriminated Unions
Optimized unions with discriminator:

```csharp
var union = Z.DiscriminatedUnion("type")
    .Option("user", userSchema)
    .Option("admin", adminSchema)
    .Build();
```

### Default Values
Default values when input is null:

```csharp
var schema = Z.String().Default("unknown");
var result = schema.Validate(null); // "unknown"
```

### Newtonsoft.Json Integration
Integrated JSON validation:

```csharp
using ZodSharp.Json;

var result = schema.DeserializeAndValidate(jsonString);
```

### Compiled Validators
Compiled validators for maximum performance:

```csharp
using ZodSharp.Expressions;

var compiled = CompiledValidator.Compile(schema);
var result = compiled(value); // Ultra-fast validation
```

### Schema Caching
Intelligent schema caching:

```csharp
using ZodSharp.Core;

var schema = SchemaCache.GetOrCreate("user", () => 
    Z.Object().Field("name", Z.String()).Build()
);
```

## 📝 License

MIT License - see the LICENSE file for details.

## 🤝 Contributing

Contributions are welcome! Please open an issue or pull request.

## 🙏 Acknowledgments

- [Zod](https://github.com/colinhacks/zod)
