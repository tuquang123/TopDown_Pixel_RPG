# Project Overview
- **Game Title**: TopDown Pixel RPG (based on project directory `TopDown_Pixel_RPG`)
- **High-Level Concept**: A retro-style 2D TopDown Pixel RPG featuring enemy waves, bosses, level-up skills, quest rewards, and local/multiplayer functionality.
- **Players**: Single player with networking capability via Photon PUN2.
- **Inspiration / Reference Games**: Classic action pixel RPGs like Soul Knight, Archvale, or Zelda.
- **Tone / Art Direction**: Vibrant, retro pixel art with detailed UI panels (GUI PRO Kit - Fantasy RPG).
- **Target Platform**: WebGL and PC.
- **Screen Orientation / Resolution**: Landscape 1920x1080.
- **Render Pipeline**: Universal Render Pipeline (URP).

# Game Mechanics
## Core Gameplay Loop
Players engage in top-down combat against waves of enemies, defeat bosses, earn gold and item drops, complete quests from NPCs, level up skills through an interactive skill select system, and buy items from the NPC Shop.
## Controls and Input Methods
- Keyboard & Mouse: WASD/Arrows for movement, space/clicks for attacks, skill hotkeys.
- UI Navigation: Fully responsive mouse-click inputs for HUD windows (Shop, Settings, Inventory, Stats, Skill tree).

# UI
The game features highly-polished, unified medieval fantasy-themed popups (managed by `UIManager` on the `---Ui-Popup---` GameObject). The **Settings Panel** is accessed by clicking the gear/settings button in the game scene. The settings popup (`-SettingUI-.prefab`) displays configuration controls for:
- **BGM Volume** (Slider)
- **SFX Volume** (Slider)
- **Language Management** (Row with "English" and "Tiếng Việt" buttons)

# Key Asset & Context
- **`Assets/Scripts/Ui/LanguageManager.cs`**: Persistent global manager handling translation dictionary lookups, keeping track of current locale ("en" / "vi"), saving/loading language settings from `PlayerPrefs`, and triggering event callbacks.
- **`Assets/Scripts/Ui/LocalizedText.cs`**: Attached to `TextMeshProUGUI` fields to automatically translate their display text reactively when the language changes.
- **`Assets/Scripts/Ui/BtnSetting.cs`** (`UISettingController`): Exposes references to English (`enButton`) and Vietnamese (`viButton`) buttons, controls active/inactive visual states, and coordinates with `LanguageManager`.
- **`Assets/Prefab/Ui/UiMain/-SettingUI-.prefab`**: Prefab containing the Settings Popup layout, with the newly added **Language** row, containing text labels and EN/VI buttons.

---

# Options Analysis for Implementation

We have evaluated two main approaches for language management in this project:

### Option 1: Lightweight Embedded Dictionary System (Recommended)
- **Pros**:
  - Extremely fast compilation and ultra-low footprint (ideal for WebGL where package overhead must be minimized).
  - No external package dependencies (avoids complex setup files and binary serialization).
  - High performance, custom-tailored to the project's exact needs, and works seamlessly with standard `PlayerPrefs` and TMPro out-of-the-box.
- **Cons**:
  - Requires adding new strings directly in `LanguageManager.cs` code dictionary.
- **Alignment**: Perfectly aligns with the WebGL target platform requirements for small build size and immediate loading times.

### Option 2: Unity Localization Package
- **Pros**:
  - Standard Unity-supported package with CSV import/export.
- **Cons**:
  - Heavy performance and asset serialization overhead (increases WebGL build sizes).
  - Demands setup of Locales, String Table Collections, and Active Settings assets, adding unnecessary complexity for a simple English/Vietnamese toggler.
  - Prone to package-version mismatches or editor assembly locking on certain Unity versions.
- **Alignment**: Overkill for a lightweight WebGL pixel RPG with limited UI screens.

**Recommendation**: We proceed with **Option 1** because it is already successfully initialized, compiled, and integrated with the Settings Prefab.

---

# Implementation Steps

### Step 1: Initialize LanguageManager Core
- **Description**: Create and refine `LanguageManager.cs` as a persistent singleton to handle storing/loading language choices ("en" and "vi"), raising translation update events, and caching translation strings.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: No

### Step 2: Implement LocalizedText Behaviour
- **Description**: Implement `LocalizedText.cs` which binds to `TextMeshProUGUI` fields, listens for `OnLanguageChanged` events, and fetches translations for its configured keys.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: Yes

### Step 3: Integrate Language Row in Settings Prefab
- **Description**: Add the "Language" row into the `-SettingUI-.prefab` containing a label text and two stylized buttons (English and Tiếng Việt) using pixel-art assets matching the game's medieval aesthetic.
- **Assigned role**: developer
- **Dependencies**: Step 1, Step 2
- **Parallelizable**: No

### Step 4: Wire Language Selection in UISettingController
- **Description**: Update `UISettingController` script to reference the EN/VI buttons, register onClick event handlers to change languages, and highlight the selected button color.
- **Assigned role**: developer
- **Dependencies**: Step 3
- **Parallelizable**: No

---

# Verification & Testing

### Test Case 1: Runtime Dynamic Language Switching
- **Action**: Open Settings in `GameScene.unity` and click "Tiếng Việt".
- **Expected Result**: 
  - Settings popup title instantly changes to **"Cài Đặt"**.
  - Sliders update their labels to **"Nhạc Nền"** and **"Âm Thanh"**.
  - Language label updates to **"Ngôn Ngữ"**.
  - Vietnamese button gets highlighted in Green, English button becomes White.
- **Action 2**: Click "English".
- **Expected Result 2**: Texts instantly revert back to English equivalent and English button highlights.

### Test Case 2: PlayerPrefs Persistence
- **Action**: Switch language to "Tiếng Việt", close settings, restart/reload the scene, and reopen settings.
- **Expected Result**: The selected language must remain Vietnamese, with Vietnamese texts loaded instantly.

### Test Case 3: Vietnamese Glyph Integrity Check
- **Action**: Ensure all Vietnamese characters are fully readable without generic fallback boxes (such as `[]` or `?`).
- **Expected Result**: TextMeshPro uses `LiberationSans SDF` or configured fallbacks to properly render all Vietnamese diacritics.
