# 🔵 Shadow Bot – Exposure-Aware Indoor Navigation in Unity

A Unity-based simulation for a stealth robot that navigates an indoor environment while minimizing exposure to visible observation points. The robot uses dynamic heatmap analysis based on real-time visibility and obstacles such as chairs and desks.

---

## 🎯 Project Goals

- Simulate a robot navigating an indoor grid.
- Calculate a **visibility heatmap** from randomized overhead viewpoints.
- Use **A\*** to plan the least-exposed path to a defined target.
- Color the environment to reflect exposure (green = hidden, red = highly visible).
- Mark visited visible tiles in **blue** if the robot saw them while moving.

---

## 🛠️ How It Works

1. **Grid Generation**: The floor is divided into cells (`rows x cols`) represented by colored cubes.
2. **Viewpoint Sampling**: A set of random points above the robot are sampled if visible from its camera.
3. **Heatmap Computation**: Each cell checks how many sampled points can "see" it via raycasting.
4. **Color Mapping**: Each tile is colored from green (low exposure) to red (high exposure).
5. **Pathfinding**: A\* algorithm chooses the path with the lowest cumulative exposure score.
6. **Robot Navigation**: The robot moves smoothly to the goal, coloring visible tiles in **blue** if exposed.

---

## 🎮 How to Use

### ✅ Requirements

- Unity 2021.3+ (tested on LTS versions)
- A `cellPrefab` (cube or plane) with a visible material
- A `robot` GameObject with a `Camera` component attached

### 📦 Setup

1. Clone this repo or copy the script into a Unity project.
2. Assign the `SimpleHeatmap` script to an empty GameObject in your scene.
3. Link the required public fields in the Unity Inspector:
   - `Cell Prefab` → A cube or floor tile
   - `Robot` → Your robot object
4. Place "desk"/"chair" objects in the scene with colliders (auto-added if missing).
5. Hit **Play**!

---

## 📄 Code Structure

| Function | Description |
|----------|-------------|
| `Start()` | Initializes the heatmap, grid, camera, and blockers |
| `GenerateBlockers()` | Detects and stores obstacle objects |
| `GenerateViewPoints()` | Randomly samples visible 3D points within camera FOV |
| `CreateGrid()` | Creates and scales cell prefabs on the ground |
| `ComputeHeatmap()` | Calculates how visible each cell is to the viewPoints |
| `FindPath()` | A\*-based pathfinding from robot to goal using heatmap as cost |
| `UpdateHeatmapAndNavigate()` | Moves robot step-by-step toward the goal |

---

## 🎨 Color Legend

- 🟥 **Red**: Highly visible area  
- 🟩 **Green**: Fully hidden  
- 🔵 **Blue**: Tile seen by robot and part of its path  
- 🟦 **Blue (goal)**: The target tile

---

## 🎥 Demo & Screenshots

[![Watch Demo](https://github.com/Shira1805ozeri/Shaddow-bot/blob/main/Screenshots/scene1/demo_thumbnail.png?raw=true)](https://drive.google.com/file/d/1Bvg5MCHppiVkguxGwKEf3OZzHPtCohY3/view?usp=sharing)

> The robot uses a heatmap to evaluate exposure levels and selects the safest path to reach the goal.

---

### 🖼️ Initial Scene Setup

![Scene Setup](https://github.com/Shira1805ozeri/Shaddow-bot/blob/main/Screenshots/scene1/initial_scene.png?raw=true)  
*The robot is placed in the environment, surrounded by obstacles like chairs and desks.*

---

### 🖼️ Heatmap in Action

![Heatmap](https://github.com/Shira1805ozeri/Shaddow-bot/blob/main/Screenshots/scene1/mid_navigation.png?raw=true)  
*Exposure levels are computed in real time. Red = highly visible, green = hidden. Each tile's color indicates how many camera-visible points can see it.*

---

### 🖼️ Final Path to Target

![Final Path](https://github.com/Shira1805ozeri/Shaddow-bot/blob/main/Screenshots/scene1/result_path.png?raw=true)  
*The robot follows the least exposed path (in blue) to the target tile. The heatmap updates dynamically as it progresses.*

---

### 🖼️ Another Example

![Scene Setup](https://github.com/Shira1805ozeri/Shaddow-bot/blob/main/Screenshots/scene2/initial_scene.png?raw=true)  
![Heatmap](https://github.com/Shira1805ozeri/Shaddow-bot/blob/main/Screenshots/scene2/mid_navigation.png?raw=true)  
![Final Path](https://github.com/Shira1805ozeri/Shaddow-bot/blob/main/Screenshots/scene2/result_path.png?raw=true)  

---

## 🧠 Theoretical Background

This project simulates **vision-based navigation**, where the robot must reach a goal while minimizing **visibility-based risk**. Instead of relying on a predefined map or external controller, the robot makes decisions based on what it can currently "see" through its own simulated camera.

The core pathfinding logic uses the **A\*** (A-Star) algorithm, a well-known heuristic search technique that finds the shortest or most efficient path between two points. A\* works by combining the actual movement cost from the start with an estimated cost to the goal (heuristic). In our case, instead of minimizing distance, we **minimize exposure** by treating heatmap values as movement "costs" — areas more exposed to overhead viewpoints are more "expensive" to cross. This makes A\* ideal for stealth navigation.

What makes this system dynamic is that the **heatmap is updated every frame**, based on the robot's current surroundings. As the robot moves, its camera sees new areas and "forgets" old ones. New random 3D viewpoints are sampled, and each cell's exposure is recalculated via raycasting. This means the optimal path **can change** as the robot uncovers safer or more dangerous regions in real time. Thus, the simulation models a realistic scenario where a robot must **adapt its path based on changing visual information**, mimicking human-like stealth behavior.

---

## 📌 Notes

- Visibility is computed using raycasts from sampled 3D points to each floor tile.
- The robot navigates only over **visible and discovered** tiles.
- Heatmap is **dynamic**: recalculated every movement step.
- The path avoids exposed areas as much as possible.

---

## 🌟 Future Improvements

- Add memory: robot keeps track of previously discovered safe zones.
- Implement smoother rotations and obstacle avoidance.
- Add moving obstacles or guards to simulate dynamic threats.
- Improve viewpoint generation using actual camera frustum.

---

## 🧑‍💻 Author

Developed by **Shira Ozeri** as part of an advanced simulation project  
Course: *Semester project*  
University of Haifa

