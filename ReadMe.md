# Portfolio - Interactive Bento Grid Experience

This project is a high-performance, interactive portfolio application built with **Uno Platform** and **WinUI 3**. It features a modern "Bento Grid" layout where each card represents a different project or aspect of the developer's profile, brought to life with advanced animations, physics-based interactions, and custom rendering.

## 🏗️ Architecture
The application uses a **MainPage** acting as a dashboard, hosting varying sized `Border` elements (Cards). Each card contains a specialized `UserControl` that encapsulates specific logic, rendering, and interactivity.

---

## 🧩 Grid Feature Breakdown

### 1. Profile Interactive Card (Top Left)
**Component:** `InteractiveProfile`
**Purpose:** Personal introduction and avatar display.
**Features:**
- **Parallax Effect:** The avatar, background particles, and text layers move at different rates relative to the mouse cursor, creating a sense of depth.
- **Holographic Rings:** Rotating rings around the avatar simulate a futuristic interface.
- **Scanning Animation:** A vertical scan line moves across the card on hover, revealing a "digital twin" or highlighting the profile.
- **Particle System:** Floating background particles that react to general motion.
- **Dynamic Text:** Name and title with drop shadows for readability.

### 2. Experience Time-Warp (Top Center)
**Component:** `HolographicTimeline`
**Purpose:** Visualizing professional work history.
**Features:**
- **Interactive Timeline:** A vertical line with nodes representing different roles.
- **Hover Expansion:** Hovering over a timeline node expands the details (Role, Company, Date) with a smooth easing animation.
- **Glow Effects:** The active node glows intensely, pulling focus.
- **Staggered Entrance:** Timeline items animate in sequentially when the grid loads.

### 3. Skill Set Radar (Top Right)
**Component:** `InteractiveSkillSet`
**Purpose:** quantifying technical proficiency.
**Features:**
- **Polygon Radar Chart:** A dynamically drawn polygon representing skill levels (React, Kotlin, etc.) on a hexagonal grid.
- **3D Tilt:** The entire chart tilts in 3D space based on cursor position, tracking the mouse like a gyroscope.
- **Floating Tags:** Tech stack labels (e.g., "AWS", "Next.js") orbit the chart in 3D space, scaling and fading to simulate depth (Z-axis).
- **Interactive Nodes:** Hovering over a vertex of the skill polygon scales it up and reveals the exact percentage value.
- **Pulse Animation:** The background grid pulses rhythmically.

### 4. Poetique / Morphing Blob (Middle Left)
**Component:** `MorphingBlob`
**Purpose:** Artistic representation for a poetry/creative app.
**Features:**
- **Organic Shape:** A Bezier curve-based blob that constantly changes shape.
- **Perlin-like Noise:** Random movement of control points creates a fluid, liquid-like animation.
- **Gradient Fill:** A rich gradient that shifts as the shape morphs.
- **Mouse Repulsion:** The blob reacts to the mouse, seemingly "running away" or deforming when touched.

### 5. FreeShare / Secure Transfer (Middle Center)
**Component:** `SecureTransfer`
**Purpose:** Visualizing a file transfer or security tool.
**Features:**
- **Central Core:** A rotating shield/hub representing security.
- **Data Packets:** Small particles "travel" from the center outward along specific paths, simulating data transmission.
- **Burst Interaction:** Clicking the card triggers a "burst" of packets and significantly speeds up the shield rotation, simulating a high-speed transfer event.
- **Hover Acceleration:** The entire simulation speeds up when the user hovers over the card.

### 6. TouchReno / Room Scanner (Middle Right)
**Component:** `RoomScanner`
**Purpose:** Demonstrating AR/3D capabilities for a renovation app.
**Features:**
- **3D Wireframe Scene:** A manually rendered 3D perspective of a room (walls, floor grid, furniture table).
- **Physics-Based Rotation:** Users can click and drag to rotate the room. It features **inertia/momentum**, meaning it continues to spin and slows down gradually after the mouse is released.
- **Auto-Rotation:** After a period of inactivity, the room gently starts rotating on its own.
- **Edge Scanning:** A horizontal laser line scans vertically through the 3D model, highlighting edges as it passes them (simulating LiDAR scanning).

### 7. Contact Constellation (Middle Far Right)
**Component:** `FloatingConstellation`
**Purpose:** displaying contact links in a gamified way.
**Features:**
- **Physics Nodes:** Contact icons (GitHub, LinkedIn, Mail) act as physics bodies.
- **Spring Connections:** Nodes are connected by spring-like lines that stretch and contract.
- **Mouse Influence:** The cursor acts as a gravity well or repulsion field, pushing nodes around while they try to maintain their structure.
- **Dynamic Geometry:** Lines are redrawn every frame to maintain connections between moving nodes.

### 8. Dev.to / Solar System (Bottom Left)
**Component:** `SolarSystem`
**Purpose:** A link to blog posts or developer community presence.
**Features:**
- **Orbital Mechanics:** Planets orbit a central sun at different speeds.
- **Warp Speed:** Clickening the card triggers a "warp speed" effect where stars streak past and planets accelerate.
- **Starfield:** Background stars are generated and move to create a space travel effect.

### 9. NotePage / Bug Blaster (Bottom Center-Left)
**Component:** `BugBlaster`
**Purpose:** A playable mini-game representing a note-taking or productivity tool (gamification of "debugging").
**Features:**
- **Playable Game:** A full `Canvas`-based shooter game.
- **Player Ship:** Controlled by the mouse, follows x-axis.
- **Projectile System:** Auto-fires lasers upwards.
- **Enemy Spawning:** "Bugs" float down from the top.
- **Collision Detection:** Simple AABB collision destroys bugs and increases score.
- **Game Loop:** Runs on a dedicated visual loop for 60fps performance.

### 10. Blog / Matrix Rain (Bottom Center-Right)
**Component:** `MatrixRain`
**Purpose:** Visual flair for coding articles.
**Features:**
- **Digital Rain:** Characters (Kata-kana, numbers) fall vertically in columns.
- **Trail Effect:** Characters fade out slowly, creating the iconic "Matrix" trail.
- **Randomization:** Speeds, characters, and start times are randomized for an organic digital look.

### 11. Code Quote / Snake Grid (Bottom Full Width)
**Component:** `SnakeGrid`
**Purpose:** Footer and motivational quote.
**Features:**
- **Grid Traversal:** A "snake" light trail continually moves along the grid lines of the background.
- **Random Pathing:** The snake chooses random cardinal directions at intersections, never retracing immediately.
- **Glowing Head:** The leading edge of the snake glows brighter.
- **Terminal Text:** The overlay text "&lt;Code with Creativity /&gt;" mimics a code editor style.

---

## 🛠️ Technical Implementation Details
- **Rendering:** mostly uses standard WinUI `Canvas`, `Shape` (Rectangle, Ellipse, Path, Polygon), and `Composition` layer for high performance.
- **Loop:** `DispatcherTimer` or `CompositionTarget.Rendering` is used for 60fps animation loops.
- **Math:** Extensive use of `System.Numerics.Vector2`, `Matrix4x4` (for 3D), and trigonometry (sin/cos for orbits and waves).
- **Interactivity:** `PointerEntered`, `PointerExited`, `PointerMoved`, `PointerPressed` events capture user intent and feed into physics modifications.

## 🚀 Getting Started
1. **Restore:** Run `dotnet restore`
2. **Build:** Run `dotnet build`
3. **Run:** Deploy to Windows Machine (WinUI) or Browser (WASM).

Enjoy the experience!