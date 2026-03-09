# 2d-movement-test-
aka
# 🚀 Advanced 2D Unity Player Controller

A polished, "juicy" 2D platformer controller built in Unity 6. This script handles physics-based movement, advanced jumping mechanics, and game-feel elements like squash-and-stretch and camera shaking.

## ✨ Features

* **Smooth Movement:** Physics-based horizontal movement with acceleration and friction.
* **Variable Jump:** Includes coyote time, jump buffering, and a "jump cut" (hold for higher, tap for lower).
* **Dash System:** Directional dashing with a dedicated cooldown and UI integration.
* **Juice Components:**
    * **Squash & Stretch:** Procedural scaling on jump and land.
    * **Camera Shake:** Integrated shake on dash and heavy landings.
    * **Audio Manager:** Easy-to-use singleton for triggering SFX and background music.

## 🛠️ Setup Instructions

### 1. Requirements
* **Unity 6** (or 2022.3 LTS+)
* **Input System Package** (Comes pre-configured for the New Input System)
* **TextMesh Pro** (For in-game signs/instructions)

### 2. Player Setup
1. Attach the `PlayerMovement.cs` script to your Player GameObject.
2. Assign a **Rigidbody 2D** (Set Collision Detection to *Continuous* and Interpolate to *Interpolate*).
3. Create a child GameObject called `GroundCheck` at the player's feet and assign it to the script.
4. Set your **Ground Layer** in the Inspector so the player knows what is walkable.

### 3. Audio & Camera Setup
1. **AudioManager:** Create an empty object with the `AudioManager.cs` script. Add two `AudioSource` components (one for Music, one for SFX). Link them in the Inspector.
2. **Camera:** Attach `CameraFollow.cs` to your Main Camera (or a Camera Holder). Adjust the **Dead Zone** and **Look Ahead** sliders to fit your level scale.

## 🎮 Controls

| Action | Key / Button |
| :--- | :--- |
| **Move** | A / D or Left Stick |
| **Jump** | Space / South Button |
| **Dash** | Left Shift / West Button |

## 📝 License
This project is free to use for personal and commercial projects.
