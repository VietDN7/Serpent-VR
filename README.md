[Serpent VR Porject Proposal Document](https://docs.google.com/document/d/1zBL50LhAF7VWauPFi-SRTS3tk1QZCcPOWV377oveMLk/edit?usp=sharing)

# Serpent VR | A Stealth-Action Espionage Game

## By Dachi Sirbiladze & Viet Nguyen

Serpent VR is a single-player stealth-action espionage game heavily inspired by *Metal Gear Solid 3: Snake Eater*, as well as a prior VR Lab project, *Meow Gear Solid*, which was also influenced by the same franchise. Players take on the role of **Serpent**, an Ophidian operative sent into enemy territory deep within a remote jungle off the coast of a classified location. Their mission is to **sabotage an enemy base of operations and exfiltrate within a set time after planting and arming an explosive, all while remaining undetected**.

In this VR stealth-action game, players must navigate hostile enemy territory while avoiding patrols and surveillance cameras. The game emphasizes multiple approaches to each situation, including a loud, action-oriented playstyle for those who prefer direct engagement. Players can seamlessly blend into their environment using camouflage, climb designated surfaces for alternative points of entry, and choose to dispatch, distract, or completely evade enemy guards, providing a satisfying level of interactivity and tactical freedom.

---

## Components

### Objects

### **MVP Features:**
- **Enemy detection and evasion.**
- **A functional gun that shoots (basic functionality).**
- **Camouflage/stealth index mechanics** based on the player’s head distance from the ground (calculated relative to player height, likely set manually).
- **Locomotion options:** Short-distance teleportation or smooth movement, with smooth or snap turning.
- **Controls for crouching or proning** (accommodating seated players or those unable to engage in physical movement).
- **Rocks for distraction.**
- **Climbing mechanics.**
- **iPad-style device for map display.**
- **Objective completion via bomb detonation.**
- **Enemy Models:** Simple humanoid enemies with accurate but slightly lenient hitboxes.
- **Cameras:** Functional security camera for enemy surveillance.

### **Weapons:**
- **Tranquilizer Pistol:** Fires tranquilizer darts; planned features include slide and magazine functionality with unlimited ammo.
- **Stun Rod:** A non-lethal alternative to the combat knife, used to stun enemies.
- **Hands:**
  - Can form fists to knock out enemies during combat.
  - Used to grab specific ledges for climbing.

### **Tools:**
- **ALD (Advanced Location Displayer):** A handheld electronic map that the player can use to view their location within the area.
- **Rocks:** Objects that the player can pick up from the environment to create noise distractions or as an alternative method to knock out guards.
- **RED (Revolutionary Explosive Device):** A bomb that the player can plant only when standing in the designated objective area.

---

## Attributes

### **Toolbelt System:**
- Weapons and tools are stored on the player’s belt in easily accessible and comfortable positions.
- If the player drops an equipped weapon or tool, it automatically snaps back to its designated location on the belt.
- For tranquilizer gun magazines, interacting with the magazine area on the belt will duplicate a new magazine.
- If the number of actively dropped magazines exceeds a certain threshold, older dropped magazines will disappear (queue system).

### **Stealth:**
- In addition to enemy sightlines, the player has a **stealth index**, which is influenced by:
  - The camouflage they are wearing.
  - The material they are standing on or next to (constant value).
  - The height of the player’s head from the ground.
- The game accounts for manually set player height, allowing support for seated players and offering crouching/proning functionality without requiring physical movement.
- A **higher stealth index reduces enemy detection range** and increases the time it takes for enemies to detect the player.
- The stealth index is displayed on the **player’s wristwatch** (worn under the wrist in a military-style).
- The camouflage worn is **visually represented by the texture of the player’s sleeve**.
- **Camouflage selection** is streamlined through a camo menu, inspired by the *Half-Life: Alyx* weapon selection system.

### **Stamina:**
- Climbing **depletes a stamina bar**, preventing the player from hanging or climbing indefinitely.

---

## Relationships

### **Enemy to Player:**
- Hostile. Detects the player within a certain range based on the stealth index.
- If in the alert phase, the enemy will shoot the player if within range.
- If the player is outside a specific range, the enemy will chase them.
- If the line of sight is broken or the player moves beyond the chase range, the enemy will go to the last known location and search within a radius until the alert phase ends (player remains unseen for a set amount of time).

### **Player to Enemy:**
- Can knock out an enemy using a **tranquilizer dart, a Stun Rod, or fists** (requires multiple hits).
- Can **apprehend an enemy** by sneaking up behind them, aiming the tranquilizer gun, and pressing a button.

### **Camera to Player:**
- Functions similarly to enemy detection.
- If a camera spots the player, it alerts all nearby enemies to the player's position.

### **Player to Camera:**
- The player can **disable cameras by shooting them**.

---

## Environment

### **Jungle Cabin:**
- The jungle cabin will be **low-poly to conserve graphical resources**.
- It will be set in a **small jungle but will feature numerous points of entry** and strategic opportunities.
- **Multiple entry points:** Front, back, crawling from below, or climbing from above.
- **Inside:** Enemies will be present for the player to either sneak past or neutralize.

### **Alternate Environment:**
- If the jungle cabin proves too complex to implement within the allotted time, an **alternative environment will be a multi-leveled underground area** with verticality.
- **Less player agency:** One staircase leading upward.

---

## Development Timeline and Tasks

### **Week 1: Planning and Design**
- **Concept Finalization:** Define core gameplay mechanics, objectives, and unique selling points.
- **Technical Specifications:** Determine hardware and software requirements.
- **Team Assignment:** Assign roles to ensure a collaborative and efficient workflow.

### **Weeks 2-4: Development**
- **3D Modeling:** Create detailed assets, focusing on visual appeal and performance.
- **VR Interface Design:** Develop an intuitive inventory system, manual weapon interactions, and immersive HUD elements.
- **Gameplay Mechanics:** Establish core stealth functionalities, CQC combat, camouflage effectiveness, and enemy AI behavior.

### **Week 5: Testing and Feedback**
- **Internal Testing:** Identify and resolve critical bugs, performance issues, and stability concerns.
- **User Feedback Sessions:** Gather input from a select group of testers.

### **Week 6: Finalization and Deployment**
- **Polishing:** Optimize AI behavior and finalize stealth interactions.
- **Deployment:** Prepare the game for release on selected VR platforms.

---

## Equipment and Software Requirements

### **Hardware:**
- **VR Headsets:** Meta Quest 3, HTC Vive, etc.
- **Development Workstations:** High-performance PCs with sufficient processing power and graphics capabilities.

### **Software:**
- **Unity:** For robust VR development combined with the XR Toolkit.
- **Blender:** For creating detailed and optimized 3D assets.

By following this structured plan, the development team can deliver an immersive VR stealth-action espionage game, **bringing players closer than ever to the thrill of stealth and espionage**.
