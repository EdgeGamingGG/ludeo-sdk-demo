# LudeoSDK - UnityDemo
![](https://img.shields.io/badge/Unity%20Version-2019.4.40-blue.svg)
![](https://img.shields.io/badge/Built%20In%20Render%20Pipeline-supported-blue.svg)

this project was hastily done by one person, it does not pretend to clean code or any of the sorts, it is about proving the usability of the Ludeo SDK. 

<< [Project](#project) | [Running the App](#Running-the-App) | [Scripting](#scripting) >>

## Project

### Import (first time use)
Important! you will get compiler errors when first importing the project. Make sure you import the appropriate packages: LudeoSDK and Coherent.

### Structure

The project structre is fairly simple:
- all of the demo files are inside the Survivor Demo folder
- /Configs contain gameplay upgrade configuration. You can play with those to affect the progression curve of the player
- Entities are prefabs for the 3 major entities in the game: player, enemy and projectile, you will find their respective scripts there.
- UI related scripts are attached to their prefab folder under UI. all UI scripts are prefixed with UIView_

### Scenes
One scene to rule them all, with a baked navmesh. Heirarchy explanation:
- The scene has a `Player Positioner` object that will position the player in the location specified.
- UI object conatains all the static canvases
- Management is where the magic happens

### Running the App
Simple as pressing the play button while the scene demo is loaded. You need to make sure you have a vaild API Key and insert it into the Ludeo Wrapper component in the scene's herirarchy (under Management object)

## Scripting

### UI Elements
All UI scripts are prefixed with UIView_ to easily identify them. They contain only UI related logic, they don't do anything else. Simple and straight forward

### Ability
Abilities are used by the player to hurt enemies. Currently only one ability is available, Shoot Projectile. If you want to add more abitilies, simply inherit from `Ability.cs`

If you do add more abilities you will have to modify the Upgrades code as well, as it currently only searches for ShootProjectile components.

### Management
5 management objects that pretty much do everything
- `Game Manager`: in charge of the main flow and hooking everything up. the classical god object.
- `UIManager`: deals with UI only, doesn't run logic on its own. The Game Manager utilizes this manager to inject values and actions to the UI.
- `Enemy Manager`: in charge of creating enemies and keeping track of their attributes.
- `Upgrade Manager`: in charge of the upgrading logic. note that all new upgrades need to be aded to this manager in order to be seen in gameplay.
- `Ludo Wrapper`: in charge of running the core logic of the Ludo SDK

You can find all the management components in the demo scene under Management