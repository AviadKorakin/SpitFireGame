Here's the updated README with the **gravity feature** and **3D audio support** highlighted.

---

# Spitfire Flight Simulator 🛫

> **A WWII-inspired, action-packed Spitfire flight simulator with realistic controls, animations, and immersive gameplay.**

## 📖 Overview
The Spitfire Flight Simulator is a thrilling Unity-based game that immerses players in a WWII flying experience. It features:
- **Realistic plane mechanics** with animations for lifelike movement
- **Dynamic gameplay** with both first-person and third-person perspectives
- **Missile shooting system** for engaging air-to-air combat
- **Challenging desert terrain** that adds complexity to the flight path

## 🕹️ Features
- **Missile and Combat System** 🎯  
  Shoot missiles at floating balloons in the sky, with scoring and sound effects to heighten the action.
  
- **First-Person and Third-Person Views** 👀  
  Switch between cockpit view and external view to experience the action from different perspectives.

- **Random Balloon Generation** 🎈  
  10 balloons appear randomly across the terrain at the start, presenting targets for missile strikes.

- **Realistic Desert Terrain** 🏜️  
  The desert environment makes navigation and targeting more challenging, enhancing the game experience.

- **Audio & Visual Effects** 🔊🎨  
  Engine sounds, crash effects, and missile launch sounds make each flight more immersive.

## 🎥 Gameplay Preview
**Watch the Demo Video**: 
[![Spitfire Flight Simulator Demo](https://img.youtube.com/vi/UXWjiwgtOK8/0.jpg)](https://www.youtube.com/watch?v=UXWjiwgtOK8)  
(*Click the thumbnail to watch the video.*)

## 🎮 How to Play
1. **Launch** the game and select your view (first-person or third-person).
2. **Throttle Up** with `Left Shift` and **Throttle Down** with `Left Ctrl`.
3. **Aim** with the `Right Mouse Button` and **Fire** with the `Left Mouse Button`.
4. **Avoid Terrain** – Stay within the safe zone, or the plane will reset.
5. **Keep an Eye on Ammo** – Out of missiles? Game over!

## 🛠️ Code Highlights

### Gravity Simulation
The Spitfire Flight Simulator includes a gravity feature that dynamically pulls the plane towards the ground based on altitude. The force adjusts proportionally to height, adding realism as players navigate challenging terrains. Additionally, the game supports **3D spatial audio**, allowing players to experience sounds from different directions and depths for heightened immersion.

```csharp
private void ApplyGravityIfNeeded()
{
    if (transform.position.y > 2.5f)
    {
        transform.position += Vector3.down * gravity * Time.deltaTime * ((transform.position.y + 800f) / 800f);
        // simulating gravity per height
    }
}
```

### Missile Mechanics
Missiles are launched from the Spitfire and seek out balloon targets, adding to the score on impact:
```csharp
private void OnTriggerEnter(Collider other)
{
    if (other.gameObject.CompareTag("Balloon"))
    {
        flightSimulator.StopShootSound();
        PlayHitSound();
        DisableMissile();
        other.gameObject.SetActive(false);
        flightSimulator.IncreaseScore(1);
        StartCoroutine(DestroyAfterSound());
    }
}
```

### Plane Movement & Control
Realistic control inputs for pitch, yaw, and roll create an authentic flying experience:
```csharp
private void HandleRotationInput()
{
    float yaw = Input.GetAxis("Yaw") * currentSpeed * Time.deltaTime;
    float pitch = Input.GetAxis("Pitch") * currentSpeed * Time.deltaTime;
    float roll = Input.GetAxis("Roll") * currentSpeed * Time.deltaTime;
    transform.Rotate(pitch, yaw, roll);
}
```

## 👨‍💻 Credits
- **Development**: Aviad Korakin
- **Graphics & Sound Design**: Unity Asset Store

## 📫 Contact
For inquiries, reach out via [aviad825@gmail.com](mailto:aviad825@gmail.com).

--- 

This version now highlights the gravity feature and 3D spatial audio support, enhancing the overall presentation. Let me know if there’s anything more you’d like to add!
