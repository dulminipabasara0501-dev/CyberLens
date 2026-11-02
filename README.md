# CyberLens AR

An interactive educational app that teaches university students about **SQL injection** and how to prevent it. It runs on Android and uses **camera-based augmented reality (AR)** to place a small 3D "lab" (computer, application server, database, login panel, data token and security shield) on a real surface, such as a table.

> **Safe by design:** every SQL injection demonstration is a **simulation**. The app never connects to a real database, never uses the network, and never attacks any real system. All data shown is fictional.

- **Author:** S. A. D. P. Subasingha
- **Course / module:** CCS Computer Graphics
- **Repository:** https://github.com/dulminipabasara0501-dev/CyberLens.git
- **Download the APK:** https://github.com/dulminipabasara0501-dev/CyberLens/releases/download/v1.0/CyberLens_final_test.apk

---

## What the app teaches

| Module | Topic |
|---|---|
| 1. Meet the Database | User, application server, database, and the path of a login request |
| 2. How SQL Injection Happens | Unsafe query construction, and how input can change a query's meaning (simulated login and database response) |
| 3. Parameterized Queries | Placeholders, separating input from SQL, and an unsafe vs safe comparison |
| 4. Defence in Layers | Parameterized queries, input validation, least privilege and safe error messages |
| 5. Fix the Scenario | Identify vulnerable code, choose the right fix, and apply a parameterized query |
| Security Challenge | A final set of scenario questions that uses ideas from all five modules |

Learning is measured with a **pre-quiz** and a **post-quiz** (5 questions each), followed by a **Results** screen that lists topics to review.

## Features

- Camera-based AR using Unity **AR Foundation** and **Google ARCore**: find a flat surface, then tap to place the lab
- Touch controls: **tap** objects for explanations, **drag** to rotate, **pinch** to zoom
- Step-by-step lessons with a code panel that highlights the active line
- Interactive activities: tap the roles, simulated login, unsafe vs safe comparison, security layers, and arranging code blocks
- Multiple-choice questions with correct and incorrect feedback
- Pre-quiz and post-quiz with score calculation and a review list
- Progress saved on the phone with **PlayerPrefs** (no account, no personal data)
- Help and Glossary screen
- Sound effects and quiet background music, with an on/off button
- Modern cybersecurity-style interface: dark purple, violet, magenta, cyan, rounded cards and glowing buttons

## Requirements

**To use the app**
- An Android phone that supports **Google ARCore** (check Google's "ARCore supported devices" list)
- Camera permission (needed only to place the 3D lab on a surface)

**To open the project**
- **Unity 6.3** with **Android Build Support** (OpenJDK, Android SDK and NDK)
- Blender (only if you want to edit the 3D models)

## How to open and build

1. Clone or download this repository.
2. In **Unity Hub**, click **Add > Add project from disk**, and choose the repository folder.
3. Open it with Unity 6.3. The first import takes a few minutes.
4. Open **File > Build Profiles**, choose **Android**, and check the scene order: `Splash`, `MainMenu`, `ARLearning`.
5. Connect an ARCore-supported phone with USB debugging on, then click **Build And Run**.

## Project structure

```
Assets/CyberLens/
  Scenes/      Splash, MainMenu, ARLearning (ModelTest is a development test scene)
  Scripts/
    Core/        screen navigation, progress saving, audio, app session
    UI/          colours, button style, sound button
    AR/          AR placement, touch controls, tappable objects
    Modules/     module content, lesson player, activities
    Simulation/  safe SQL simulation and data-token movement
    Quiz/        quiz questions, quiz logic, results
  Models/      FBX models exported from Blender
  Materials/   URP materials
  Prefabs/     the CyberLab prefab and the AR plane
  Resources/Audio/   sound effects and music
BlenderSource/ Blender script and .blend file used to create the models
```

## How it was built

- **Engine:** Unity 6.3, Universal Render Pipeline (URP), C#
- **AR:** AR Foundation with the Google ARCore XR plugin (horizontal plane detection)
- **UI:** Unity UI (uGUI) with TextMeshPro
- **3D models:** low-poly models generated with a Blender 5.2 Python script (`BlenderSource/CyberLens_BuildModels.py`), about 1,000 triangles in total
- **Audio:** sound effects and music generated from scratch (no third-party audio)
- **Design reference:** an HTML prototype supplied for the project, used for the colours, screens, learning content and quiz questions

## Privacy and safety

- No account, no network access, and no analytics
- Only quiz scores and module progress are stored, locally, with PlayerPrefs
- The camera is used only to display the live view for AR placement. Nothing is recorded or stored.
- No passwords, API keys or signing keys are stored in this repository

## Testing

- Developed and tested in the Unity editor
- Android device test: [PHONE MODEL AND ANDROID VERSION, ADD AFTER TESTING]

## Known limitations

- **Preview Mode:** the "Preview Mode" option currently opens the same AR lessons, so a camera is still required. A camera-free preview is planned future work.
- AR surface detection works best on a textured, well-lit surface. A "Place in front of me" button is provided as a backup.
- Requires a phone that supports ARCore.
- Some small visual speckles can appear on the 3D models in the editor. This is cosmetic.

## Credits

- Concept, design and development: S. A. D. P. Subasingha
- 3D models, sounds and code created for this project
- Built with Unity, AR Foundation, Google ARCore and Blender
