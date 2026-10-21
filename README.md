<p align="center">
  <img src=".github/icon.png" width="128" height="128" alt="Universal URL icon">
</p>

# Universal URL for Unity

[![Unity](https://img.shields.io/badge/Unity-2021.2+-black.svg)](https://unity.com/)
[![Platform](https://img.shields.io/badge/Platform-WebGL%20|%20Windows%20|%20macOS%20|%20Linux%20|%20Android%20|%20iOS-green.svg)](https://unity.com/)

A lightweight, robust, cross-platform URL opener for Unity with comprehensive WebGL, desktop, and mobile support.

---

## 🚀 Features

- **True Cross-Platform Support**: Works seamlessly across **WebGL**, **Windows**, **macOS**, **Linux** (including WSL), **Android**, and **iOS**.
- **Modern WebGL Integration**: Uses a native `.jslib` bridge (`UTF8ToString`) to reliably open links in new browser tabs without popup blocker issues.
- **Smart Fallback System**:
  - **Linux / WSL**: Intelligently tries `wslview` -> `xdg-open` -> PowerShell -> `cmd.exe` -> `Application.OpenURL`.
  - **Desktop & Mobile**: Uses optimized native opening workflows.
- **Static Class Design**: Clean API accessible anywhere without requiring a `MonoBehaviour` or scene GameObject.
- **Unity Package Manager (UPM)**: Ready for instant installation via Git URL.

---

## 📦 Installation

### Option 1: Unity Package Manager (Recommended)

1. Open your Unity project.
2. Navigate to **Window** > **Package Manager**.
3. Click the **`+`** icon in the top-left corner and select **Add package from git URL...**
4. Enter the following URL:
   ```text
   https://github.com/DryreL/Unity-UniversalURL.git
   ```
5. Click **Add**.

### Option 2: Via `Packages/manifest.json`

Add the following line to your `Packages/manifest.json` under `"dependencies"`:

```json
{
  "dependencies": {
    "com.dryrelhub.unity.universalurl": "https://github.com/DryreL/Unity-UniversalURL.git"
  }
}
```

---

## 💻 Usage

```csharp
using UnityEngine;

public class ExampleUsage : MonoBehaviour
{
    void OnClickWebsiteButton()
    {
        // Open URL with explicit protocol
        UniversalURL.Open("https://example.com");
    }

    void OnClickCommunityButton()
    {
        // Open URL with automatic protocol detection (adds https:// if missing)
        UniversalURL.OpenURL("patreon.com/c/Viznity");
    }
}
```

### API Reference

| Method | Description |
|---|---|
| `UniversalURL.Open(string url)` | Opens the specified URL in the default browser. Requires protocol (e.g. `https://`). |
| `UniversalURL.OpenURL(string url)` | Opens the URL, automatically prefixing `https://` if protocol is missing. |

---

## 📂 Package Structure

```
Unity-UniversalURL/
├── Scripts/
│   ├── UniversalURL.asmdef
│   └── UniversalURL.cs
├── WebGL/
│   └── UniversalURLWebGL.jslib
├── package.json
└── README.md
```

---

## 📄 License

DryreL Hub © 2026. All rights reserved.