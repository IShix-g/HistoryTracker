
![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black)

[README - 日本語版](Docs/README_jp.md)

> [!IMPORTANT]
> This plugin is designed to save/restore existing game data files. It does not include save system functionalities such as data serialization or deserialization.

# HistoryTracker
A Unity plugin for saving and restoring game data (persistent data).

![HistoryTracker](Docs/header.png)

## Features
- Save game data (persistent data).
- Restore saved game data.
- Manage data via a clean, easy-to-read UI.
- Use data saved in the Editor on actual devices.

### Technical Requirements

#### Asset Loading Methods

- Prefabs are loaded using [Resources.Load](https://docs.unity3d.com/ja/2023.2/ScriptReference/Resources.Load.html) \*
- For actual device game data output, [StreamingAssets](https://docs.unity3d.com/ja/2023.2/Manual/StreamingAssets.html)
  is used \*

\* **Build Optimization:** These assets are included in the game build only when the plugin is enabled. When the plugin
  is disabled, the assets are excluded and not packaged with the application.

#### Testing Environment

- Device testing has been conducted on **iOS** and **Android**.

## Why HistoryTracker?

### 1. Focused Debugging of RNG Elements
When verifying "items with a 1% drop rate" or "events triggered under specific conditions," restarting the game from the beginning is a waste of time. By saving the state immediately before the check and loading it after the check to retry repeatedly, you can verify the behavior of low-probability events dozens of times in a short period.

### 2. Comprehensive Testing of Quest Branches & Multi-Endings
This is useful in RPGs or adventure games when you want to test both "Option A" and "Option B." By saving the state just before a branching point, you can check one route and then immediately return to check the other, minimizing backtracking during debugging.

### 3. As a "Bug Reproduction" Tool for QA
When testers find a bug, reproducing "how that state was reached" is often difficult. Since this plugin works on actual devices, by saving periodically, you can rewind to the moment a bug occurred. This makes it easy to share the exact occurrence context with developers or identify reproduction steps.

### 4. Accelerating Game Balance Adjustments
Useful for situations like adjusting boss difficulty, where you want to "slightly increase attack power and retry." By preserving the state before the battle starts, adjusting enemy parameters in the Inspector, and repeating the "Restore & Retry" process, you can efficiently find the ideal game balance.

### 5. Test Initialization
When performing integration tests, you often need specific states like "1000G, Level 10." Instead of setting up from new data every time, restoring an "ideal state" created in advance with this plugin allows you to start testing immediately, significantly reducing execution time and ensuring a clean testing environment.

## Getting Started

### Install from Git URL

"Unity Editor : Window > Package Manager > Add package from git URL...".

```
https://github.com/IShix-g/HistoryTracker.git?path=Packages/HistoryTracker#v1
```

<img src="Docs/add_package.png" width="850"/>

## Scripts

### Implementation

Implement `IHistSaveDataHandler` to link your save system with HistoryTracker.

| Method             | Description                                                                                                                                |
|--------------------|--------------------------------------------------------------------------------------------------------------------------------------------|
| OnBeforeSave()     | Called immediately before saving. Save necessary game data and return the title and description. This content will be displayed in the UI. |
| GetSaveFilePaths() | Returns an array of full paths to game data files. e.g., `Application.persistentDataPath` + "/data.bytes"                                  
| ApplyData(info)        | Called after game data has been restored. Reflect the game data by reloading it or by calling `Application.Quit()` to close the app once.  |

```csharp
using HistoryTracker;

public sealed class TestModelRepository : ModelRepository, IHistSaveDataHandler
{
    public HistRecordInfo OnBeforeSave()
    {
        // Save data
        for (var i = 0; i < Models.Count; i++)
        {
            var model = Models[i];
            var path = GetFullPath(model.Id);
            Save(model, path);
        }
        // Return the title and description
        var title = "Saved Count: " + Models[0].SaveCount;
        var description = "[Test]";
        return new HistRecordInfo(title, description);
    }

    // Determine and return the file path
    // e.g. `Application.persistentDataPath` + "/data.bytes" 
    public IReadOnlyList<string> GetSaveFilePaths() => Paths.Values.ToList();

    public void ApplyData(HistAppliedInfo info) => Restored();
}
```

Example of loading game data with `ApplyData()` and then closing the game:

```csharp
public void ApplyData(HistAppliedInfo info)
{
    // Reload game data
    _saveSystem.Load();

    // Quit the application
#if UNITY_EDITOR
    Debug.Log("Stopping play mode to apply save data.");
    UnityEditor.EditorApplication.isPlaying = false;
#else
    Debug.Log("Quitting application to apply save data.");
    Application.Quit();
#endif
}
```

### HistRecordInfo

The title and description set in `OnBeforeSave()` above will be displayed in the UI as follows:

```csharp
var title = "Saved Count: " + Models[0].SaveCount;
var description = "[Test]";
return new HistRecordInfo(title, description);
```

<img src="Docs/hist_record_info.png" width="830"/>

### Setting Dependencies

Set the `IHistSaveDataHandler` implemented above to HistoryTracker. Configure it in `Awake` as early as possible after
the game starts.

```csharp
using HistoryTracker;

void Awake()
{
    // Initialize the repository that implements IHistSaveDataHandler in Awake
    Hist.Configure(_repository);
}
```

### Opening the History Dialog

The dialog is opened via script. You can release it by calling `Hist.Release()` when it's no longer needed, but since
it's lightweight, this is unlikely to cause any issues.

```csharp
using HistoryTracker;

void OnDialogButtonClicked()
{
    Hist.OpenDialog();
}
```

## History Dialog Explanation

### History List

Displays a list of saved history.

<img src="Docs/dialog1.jpg" width="400"/>

| No | Description          |
|----|----------------------|
| ①  | Save Game Data       |
| ②  | Record Count         |
| ③  | Saved Game Data Item |
| ④  | Open Details         |
| ⑤  | Previous Page        |
| ⑥  | Next Page            |
| ⑦  | Trash                |
| ⑧  | Close                |

### Details

Details and operations for saved game data.

<img src="Docs/dialog2.jpg" width="400"/>

| No | Description                        |
|----|------------------------------------|
| ①  | Restore Game Data                  |
| ②  | Delete Game Data                   |
| ③  | Title (Long press to edit) *       |
| ④  | Description (Long press to edit) * |
| ⑤  | List of saved file paths           |
| ⑥  | Badge displayed if saved in Editor |
| ⑦  | Close                              |
| ⑧  | Date Saved                         |

* Note: On actual devices, you cannot edit data generated in the Editor.

## Plugin Settings / History Recorder

By default, this plugin operates only in the debug environment. You can adjust this in the settings.

### Open via `Window > open History Tracker`
<img src="Docs/menu.jpg" width="700"/>

### Dialog

<img src="Docs/settings.jpg" width="450"/>

| No | Description |
|----|-----------------------------------------------------------------------------------|
| ①  | Show GitHub Page (External Link)                                                  |
| ②  | Open History Dialog (Runtime only)                                                |
| ③  | Plugin Scope (EditorOnly / DevelopmentBuild / All)                                |
| ④  | Use game data saved in Editor on actual device?                                   |
| ⑤  | Enable adding title and description to History Dialog save feature (Runtime only) |

## Saving Game Data via Script

While you can save game data using the Save button in the History Dialog, you can also save via code using the snippet below:

```csharp
Hist.SaveHistory();
```

## Use Cases

### Save Game Data on Level Up

If your save system has level-up events, saving game data each time you level up makes it easier to revert if issues
occur.

<b>Code Example:</b>

```csharp
using HistoryTracker;

void OnLevelUp(int level)
{
    // You can add a title and description.
    var title = $"Level Up {level}";
    var description = JsonUtility.ToJson(_user, true);
    var info = new HistRecordInfo(title, description);
    Hist.SaveHistory(info);
}
```

### Save Game Data When Errors Occur

To use the pre-prepared code, execute the following. This is a singleton component that monitors for errors and calls
`Hist.SaveHistory()`.

<b>Code Example:</b>

```csharp
using HistoryTracker;

void Start()
{
    HistErrorSaver.Create();
}
```
