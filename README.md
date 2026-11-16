# YTL-CSharp

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.1-purple.svg)
![Version](https://img.shields.io/badge/version-0.1.0-green.svg)

**YTL-CSharp** is a native C# library for parsing **YTL (Yggdrasil Template Language)**. It is designed for seamless integration with **Unity** and **Godot**, allowing you to load complex configuration files directly into your game.

Unlike the Rust LSP server, this library is optimized for runtime execution inside game engines, providing fast parsing and convenient data access.

## Features

- **Full Parser**: Supports blocks, fields, and nested structures.
- **Inheritance**: Supports inheritance (`Warrior{Character}`).
- **Realm Overrides**: Supports `Realm` blocks for environment-specific overrides.
- **Include**: Supports external file inclusion (via a dictionary of files).
- **Environment Variables**: Supports `$ENV_VAR` substitution.
- **Smart Validation**: Built-in validation for `IPv4`, `IPv6`, `MAC`, `CIDR`, and `UUID`.
- **Convenient API**: Extension methods like `GetInt()`, `GetIp()`, `GetMac()`, `GetUuid()`.

## Installation

1. Build the project in **Release** mode:
   ```bash
   dotnet build -c Release
   ```
Copy the resulting YTL-CSharp.dll into your Unity project's Assets/Plugins folder (or Godot's bin folder).

## Quick Start
```csharp
using Ytl;
using System.Linq;

// 1. Load your YTL text (e.g., from a file)
string ytlText = @"
Character:
    hp 100
    armor 50

Warrior{Character}:
    rage 100
    Realm ""Hardcore"":
        hp 50
";

// 2. Parse the document
var parser = new YtlParser(ytlText);
var document = parser.Parse();

// 3. Access the data
var warrior = document.Blocks.FirstOrDefault(b => b.Name == "Warrior");

if (warrior != null)
{
    int hp = warrior.GetInt("hp");      // 50 (overridden by Realm)
    int armor = warrior.GetInt("armor"); // 50 (inherited from Character)
    int rage = warrior.GetInt("rage");   // 100
    
    Debug.Log($"Warrior HP: {hp}, Armor: {armor}, Rage: {rage}");
}

// 4. Check for errors
if (document.Errors.Count > 0)
{
    foreach (var error in document.Errors)
        Debug.LogError(error);
}
```

## Network Types Support
The library automatically validates and provides typed access for network data:

```csharp
var server = document.Blocks.First(b => b.Name == "Server");

// IP Address
IPAddress ip = server.GetIp("ip"); // Returns System.Net.IPAddress

// MAC Address
PhysicalAddress mac = server.GetMac("mac"); // Returns System.Net.NetworkInformation.PhysicalAddress

// CIDR Subnet
var (subnetIp, prefix) = server.GetCidr("subnet");

// UUID
Guid uuid = server.GetUuid("uuid");
```

## Include Support
To include other files, pass a dictionary of external files to the parser:

```csharp
var externalFiles = new Dictionary<string, string>
{
    { "base.ytl", @"
BaseCharacter:
    hp 100
    armor 50
" }
};

var parser = new YtlParser(mainText, externalFiles);
var document = parser.Parse();
```

> [!WARNING]
> This project is under active development.
> The API may change, and there may be critical bugs in the current version.
> Use it at your own risk, and please report any issues you find in the [Issues](https://github.com/YTL-Software/YTL-CSharp/issues) section

## License
This project is licensed under the MIT License. See the LICENSE file for details.
