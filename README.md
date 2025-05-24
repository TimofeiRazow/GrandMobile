# Grand Mobile - Social Deduction Game

A multiplayer social deduction game set in an abandoned mansion, where players must work together to survive while identifying and eliminating the maniacs among them.

## Features

- Multiplayer gameplay with up to 10 players
- Multiple roles: Maniac, Detective, and Civilian
- Day/Night cycle with different gameplay phases
- Interactive environment with object manipulation
- Voting system for player elimination
- Atmospheric 3D environment
- Cross-platform support (PC, Android)

## Requirements

- Unity 2021.3 or later
- Photon PUN 2
- TextMeshPro
- Universal Render Pipeline (URP)

## Setup

1. Clone the repository
2. Open the project in Unity
3. Install required packages:
   - Photon PUN 2 (from Asset Store)
   - TextMeshPro (from Package Manager)
   - Universal RP (from Package Manager)
4. Set up your Photon App ID:
   - Create a Photon account at https://www.photonengine.com/
   - Create a new app in the Photon Dashboard
   - Copy your App ID
   - Open `PhotonServerSettings` in the project
   - Paste your App ID in the "App Id PUN" field

## Project Structure

```
Assets/
├── Scripts/
│   ├── Managers/
│   │   ├── GameManager.cs
│   │   ├── LobbyManager.cs
│   │   ├── SettingsManager.cs
│   │   └── UIManager.cs
│   ├── Player/
│   │   └── PlayerController.cs
│   ├── UI/
│   │   └── MainMenuUI.cs
│   └── Network/
│       └── LobbyManager.cs
├── Scenes/
│   ├── MainMenu.unity
│   ├── Lobby.unity
│   └── Game.unity
├── Prefabs/
│   ├── UI/
│   └── Player/
├── Materials/
├── Models/
└── Audio/
```

## Game Mechanics

### Roles

1. **Maniac**
   - Can kill players during the night phase
   - Can drag and hide bodies
   - Must eliminate all civilians to win

2. **Detective**
   - Can investigate evidence
   - Receives clues about the maniac
   - Helps civilians identify the maniac

3. **Civilian**
   - Can vote during meetings
   - Can report dead bodies
   - Must identify and eliminate all maniacs to win

### Game Phases

1. **Day Phase**
   - Players can freely explore
   - Can interact with objects
   - Can report dead bodies

2. **Night Phase**
   - Limited visibility
   - Maniacs can kill
   - Detectives can investigate

3. **Voting Phase**
   - Triggered by body discovery
   - Players vote to eliminate suspects
   - Majority vote determines elimination

## Development

### Adding New Features

1. Create new scripts in appropriate folders
2. Follow existing code structure and patterns
3. Update documentation as needed

### Building for Different Platforms

1. **PC**
   - Set platform to Windows/Mac/Linux
   - Build and run

2. **Android**
   - Set platform to Android
   - Configure player settings
   - Build APK

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Photon PUN 2 for networking
- Unity Technologies for the game engine
- All contributors and testers 