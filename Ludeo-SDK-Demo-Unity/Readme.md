# LudeoSDK - UnityDemo
![](https://img.shields.io/badge/Unity%20Version-2019.4.40-blue.svg)
![](https://img.shields.io/badge/Universal%20Rendering%20Pipeline-supported-blue.svg)

This project was done hastily by one person, don't expect ultra clean code.

<< [Project](#project) [Running the App](#Running-the-App) | [Scripting](#scripting) | [FAQ](#faq) >>

## Project

### Structure

The project structre is fairly simple, the one issues important to notice is the distribution of .cs files. .cs files reside in three main spaces
- Scenes: each scene has its own scripts to handle
- Backend: all the scripts related to the backend
- UIElements: scripts related to UI only, the directory of the script matches the prefab associated with it

All scripts related to UI start with UIView_[Element]

### Scenes
- Boot: is the scene that start up the app.
- Load: is a generic loading screen
- Login: handles user login
- Main: the main menu, inculding the persistant HUD
- Game Scenes: inside Games Folder, each game has its own scenes (game were freely downloaded from the asset store)

### Running the App
Run the boot scene in Scenes >> Boot and make sure that scene dependencies are inculded in the build settings

## Scripting

### UI Elements
All UI scripts are prefixed with UIView_ to easily identify them. They contain only UI related logic, they don't do anything else. Simple and straight forward

### Backend
`Backend` configuration is departmentalized into Client definition and Service. The client has its own definitions for how to retrieve data and configurations. A Service implements those definitions to provide the client with relevant data. This has a huge benefit when wanting to change backend services, making the client effectively service agnostic. Moreover, mocking the backend is straightforward, all you have to do is create a local "backend service" which implements the client's definitions. The services are not tied to a technology (cloud save, cloud code, config, etc.) rather they symoblize a game service the client needs.

The downside is writing a lot of code.

Current Supported services:
- Quests - under `IQuestService`
- Popups - under `IPopupService`

Other services are hidden inside 'IBackendService' and should be refactored into smaller services. They are:
- Progression of Player, XP and levels
- Economy - player currencies
- Shop

The current service implementation is Unity Gaming Services, All the logic is under `UnityGamingServices.cs`, `UGS_PopupService`, `UGS_QuestService`

### Main
This is where the shitty code relies, be aware

`Flow.cs` is responsible booting up the app, with mostly scene loading logic 

`MainMenuFlow.cs` is a semi-god object that basically does everything in the main menu, mostly updating and hooking UI. 

## FAQ

Q: How Xp and resources are granted?
A: the loaded games use a `GameModelAdapeter.cs` component which in turn utilizes a `GameModel` ScriptableObject (SO). When a game finishes, it sends an event using the SO with the score of the player, `MainMenuFlow.ShowRewardRoutine()` handles that request and grants semi-random resources

Q: something seems to be going wrong but I can't see no errors
A: async operations are used extensively and Unity mostly hides AggreageteExceptions from nested tasks. This obviously horrible. If you want (and should) to fix this, use UniTask or track Task errors by handling the Task.IsFaulted.