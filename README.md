<h1 align="center">Toon Blast Clone 🎮</h1>
<p align="center">
  <img src="https://img.shields.io/badge/Unity-2021.3-blue?style=for-the-badge&logo=unity&logoColor=white">
  <img src="https://img.shields.io/badge/C%23-Game%20Development-orange?style=for-the-badge&logo=csharp&logoColor=white">
  <img src="https://img.shields.io/badge/SOLID-Principles-green?style=for-the-badge">
</p>

---
## 🎬 Gameplay Demo
https://github.com/user-attachments/assets/d03989ac-0bbf-4a68-b9ed-148d9626221b

## 📌 **About the Project**
Toon Blast Clone is a **tile-matching puzzle game** that features an **optimized collapse/blast mechanic**.  
This game is developed in **Unity** using **C#** with a focus on **performance optimizations**, **SOLID principles**, and **scalability**.

- **Designed with modular architecture** for flexibility and future expansions.
-  **Optimized CPU, GPU, and Memory usage** for smooth performance.
-  **Implemented intelligent shuffling algorithm** to prevent deadlocks.
-  **Applied Object Pooling & Flyweight Pattern** for better memory management.
-  **Profiling and optimizations** performed using Unity Profiler.

---

## 🎮 **Gameplay Features**
✔ **Collapse / Blast Mechanic** → Tap on groups of colored blocks to remove them.  
✔ **Dynamic Tile Generation** → New blocks fall from the top to fill the gaps.  
✔ **Deadlock Detection & Smart Shuffle Algorithm** → Prevents getting stuck.  
✔ **Multiple Board Configurations** → Supports different row & column sizes.  
✔ **Performance Optimized for Mobile** → Works smoothly on low-end devices.  

### **🚀 High-Level System Design**
The game follows a **modular component-based structure**:

- **BoardManager** → Manages game board and game state.
- **GridManager** → Handles tile placement and dynamic scaling.
- **GroupManager** → Detects block groups & updates visuals.
- **RefillManager** → Generates new tiles and fills empty spaces.
- **GravityManager** → Applies gravity mechanics for falling blocks.
- **GameStateManager** → Implements a **State Machine** for smooth transitions.
- **DeadlockSystem** → Identifies and resolves deadlocks using **Smart Shuffle Algorithm**.

---

## 🚀 **Performance Optimizations**
### 🎨 **GPU & Rendering Optimizations**
-  **Sprite Atlas** → Reduced draw calls from **120 to 3** using texture batching.  
-  **Batch Rendering** → Objects with the same material are processed together.  

### 🧠 **Memory Optimizations**
-  **Object Pooling** → Reuses frequently instantiated objects to reduce GC overhead.  
-  **Flyweight Pattern** → Minimizes memory usage by centrally managing visual data.  
-  **Vector2Int Usage** → Stored on stack memory for efficiency.  

### ⚙️ **CPU Optimizations**
-  **Optimized BFS Algorithm** → Eliminates stack overflow risks.  
-  **Smart Shuffle Algorithm** → Efficiently resolves deadlocks by analyzing patterns.  
-  **Unity Profiler Used** → Bottlenecks detected & optimized.  

---

## 📊 **Profiling Results**
### **Unity Editor Performance**
| Grid Size | FPS (Avg) | CPU (ms) | Memory (MB) | Draw Calls |
|-----------|----------|----------|-------------|------------|
| **4x4** | 145 | 2.1 | 3080 | 2 |
| **8x8** | 140 | 3.8 | 3100 | 2 |
| **12x10** | 140 | 2.0 | 3100 | 2 |

### **Mobile Performance**
| Grid Size | FPS (Avg) | CPU (ms) | Memory (MB) | Draw Calls |
|-----------|----------|----------|-------------|------------|
| **4x4** | 30 | 32 | 240 | 3 |
| **8x8** | 30 | 32 | 262 | 3 |
| **12x10** | 30 | 33 | 270 | 3 |

---

## 🎯 **Challenges & Solutions**
### 📌**Deadlock Situations**
✔ Implemented a **Smart Shuffle Algorithm** instead of random shuffling.  
✔ Detects and minimizes unnecessary shuffles for a smooth gameplay experience.  

### 🚀 **Performance Bottlenecks**
✔ **Reduced GC Allocation** by using Object Pooling.  
✔ **Optimized CPU usage** by replacing recursive algorithms with **iterative BFS**.  
✔ **Improved memory efficiency** using Flyweight Pattern.  

---

## 🔮 **Future Improvements**
 **AI-Based Shuffle Algorithms** → Machine Learning for better deadlock resolution.  
 **Unity Job System** → Multi-threaded architecture for better performance.  
 **Union-Find Algorithm** → More efficient block group detection for larger grids.  
 **PrimeTween Integration** → Replace DoTween to minimize GC Load.  

---

## 📦 **Installation & Usage**
```
# Clone this repository
git clone https://github.com/Baris34/ToonBlastClone.git

# Open in Unity 2021.3 or later
