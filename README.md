# unity-config-manager

A singleton manager for Unity3D that provides a simple interface for reading and writing JSON configuration files.

**Package Name:** com.gambit.config  
**GameObject Display Name:** gambit.config.ConfigManager (Singleton)  
**Namespace:** gambit.config  
**Assembly Definition:** gambit.config  
**Scripting Define Symbol:** GAMBIT_CONFIG  

---

## DEMO INSTRUCTIONS

The demo scene shows a basic implementation of how to create and manage a configuration file. It demonstrates how to initialize the manager and automatically create a config file from a backup if one doesn't exist.

-   Open the Unity Project in the editor.
-   Navigate to `Assets/Demos/` and open the `Demo.unity` scene.
-   Select the `Demo` GameObject in the Hierarchy.
-   In the Inspector, you will see the `Demo.cs` script component with the following properties:
    -   **Debug**: Toggle to show debug logs in the console.
    -   **Path**: The target path for the configuration file (e.g., `C:\Users\YourUser\AppData\Local\YourApp\config.json`).
    -   **Backup Path In Resources**: The path to a default JSON configuration file within the `Assets/Resources` folder (e.g., `config`). This backup will be used to populate a newly created config file.
-   Press Play to run the scene and observe the console logs for output from the `ConfigManager`.
-   The other 'key' arrays in the demo component are used to showcase helper functionality to pull data from the configuration json and log them to the console. This can also be accomplished directly via the TotalJSON plugin used as a dependency to manipulate JSON data.

---

## INSTALLATION INSTRUCTIONS

### Method 1: Unity Package Manager (via Git URL)

This is the recommended installation method.

1.  In your Unity project, open the **Package Manager** (`Window > Package Manager`).
2.  Click the **'+'** button in the top-left corner and select **"Add package from git URL..."**
3.  Enter the following URL:
    ```
    https://github.com/GambitGamesLLC/unity-config-manager.git?path=Assets/Plugins/Package
    ```
4.  To install a specific version, append the version tag to the URL:
    ```
    https://github.com/GambitGamesLLC/unity-config-manager.git?path=Assets/Plugins/Package#v1.0.0
    ```

**Alternatively, you can manually edit your project's `Packages/manifest.json` file:**

```json
{
  "dependencies": {
    "com.gambit.config": "https://github.com/GambitGamesLLC/unity-config-manager.git?path=Assets/Plugins/Package",
    ...
  }
}
```

### Method 2: Local Installation

1.  Download or clone this repository to your computer.
2.  In your Unity project, open the **Package Manager** (`Window > Package Manager`).
3.  Click the **'+'** button in the top-left corner and select **"Add package from disk..."**
4.  Navigate to the cloned repository folder and select the `package.json` file inside `Assets/Plugins/Package`.

---

## USAGE INSTRUCTIONS

The primary class for this package is **`ConfigManager.cs`**. It's a singleton that provides static methods for handling JSON configuration files.

### ▶ Initialization & Usage

**Step 1: Initialize the ConfigManager**
Before reading or writing, you must initialize the `ConfigManager` by creating a `ConfigManagerSystem`. This is typically done once when your application starts.

```csharp
#if GAMBIT_CONFIG
using gambit.config;
#endif

#if EXT_TOTALJSON
using Leguar.TotalJSON;
#endif

using UnityEngine;
using System;

public class MyConfigController : MonoBehaviour
{
    private ConfigManager.ConfigManagerSystem configSystem;

    void Start()
    {
        // 1. Initialize the ConfigManager
        ConfigManager.Create(
            new ConfigManager.Options()
            {
                showDebugLogs = true,
                path = "path/to/your/config.json",
                createIfMissing = true
            },
            // OnSuccess
            (system) => {
                Debug.Log("ConfigManager created successfully!");
                configSystem = system;
                // You can now read from or write to the file
                ReadFile();
            },
            // OnFailed
            (error) => {
                Debug.LogWarning("ConfigManager failed to create: " + error);
            }
        );
    }
    
    // ... See next steps
```

**Step 2: Reading from the Config File**
Use the `ReadFileContents` method to read the JSON data from the file.

```csharp
// ... Continued from previous example

    private void ReadFile()
    {
        ConfigManager.ReadFileContents(
            configSystem,
            // OnSuccess
            (JSON json) => {
                Debug.Log("Successfully read JSON: " + json.CreatePrettyString());
                // Access your data here
                string appName = json.GetJString("app/longname");
                Debug.Log("App Name: " + appName);
            },
            // OnFailed
            (error) => {
                Debug.LogWarning("Failed to read file: " + error);
            }
        );
    }
```

**Step 3: Writing to the Config File**
Use the `WriteFileContents` method to save a `TotalJSON` object to the file.

```csharp
// ... Continued from previous example

    private void WriteFile()
    {
        // Create a new JSON object
        JSON newJson = new JSON();
        newJson.Add("new_key", "new_value");

        ConfigManager.WriteFileContents(
            configSystem,
            newJson,
            // OnSuccess
            () => {
                Debug.Log("Successfully wrote to file!");
            },
            // OnFailed
            (error) => {
                Debug.LogWarning("Failed to write to file: " + error);
            }
        );
    }
}
```

### 🔧 Public Options

**`ConfigManager.Options`**
-   `showDebugLogs` (bool): Enables or disables internal state logs in the Unity console.
-   `path` (string): The full path to the configuration file.

---

## Additional Functionality
The ConfigManager provides several other utility functions for file and data management:

-   `GetNestedString(ConfigManagerSystem system, string[] keys, Action<string> OnSuccess, Action<string> OnFailed):` Recursively searches for a nested string value in the JSON data.  

-   `GetNestedPath(ConfigManagerSystem system, string[] keys, Action<string> OnSuccess, Action<string> OnFailed):` Similar to GetNestedString, but expands any environment variables in the path.  

-   `GetNestedInteger(ConfigManagerSystem system, string[] keys, Action<int> OnSuccess, Action<string> OnFailed):` Retrieves a nested integer value.  

-   `GetNestedFloat(ConfigManagerSystem system, string[] keys, Action<float> OnSuccess, Action<string> OnFailed):` Retrieves a nested float value.  

-   `GetNestedBool(ConfigManagerSystem system, string[] keys, Action<bool> OnSuccess, Action<string> OnFailed):` Retrieves a nested boolean value.  

-   `DoesFileExist(ConfigManagerSystem system):` Checks if the configuration file exists.  

-   `CreateFile(ConfigManagerSystem system, Action OnSuccess, Action<string> OnFailed):` Creates an empty configuration file if it doesn't already exist.  

-   `DestroyFile(ConfigManagerSystem system, Action OnSuccess, Action<string> OnFailed):` Deletes the configuration file.  

-   `ReplaceFileUsingResources(ConfigManagerSystem system, string replacementFilePathAndName, Action<JSON> OnSuccess, Action<string> OnFailed):` Replaces the current configuration file with a backup from the Resources folder.  

-   `ReplaceFile(ConfigManagerSystem system, string replacementFilePath, Action<JSON> OnSuccess, Action<string> OnFailed):` Replaces the current configuration file with another file from a specified path.  

-   `Log(ConfigManagerSystem system):` Prints the current JSON data to the console.  

-   `Destroy(ConfigManagerSystem system):` Nullifies the ConfigManagerSystem object.  

## DEPENDENCIES

This package relies on other open-source packages to function correctly. The required dependencies will be automatically installed by the Unity Package Manager.

-   **TotalJSON** [[Gambit Repo]](https://github.com/GambitGamesLLC/unity-plugin-totaljson)  
    A required dependency for handling JSON parsing and serialization. It is included via the manifest.json.

-   **Gambit Singleton** [[Gambit Repo]](https://github.com/GambitGamesLLC/unity-singleton)  
    Used as the base pattern for the singleton instance. It is recommended to use this package in any project with singletons to maintain a consistent pattern.

---

## SUPPORT

Created and maintained by **Gambit Games LLC** For support or feature requests, contact: **gambitgamesllc@gmail.com**
