# Networking design document for Robot Controller
## Design modes
### Single HoloLens mode
### Single HoloLens + Robot mode
### Single PC mode
### Single PC + Robot mode

1. Select connection (Select Host only, if select join, have to cancel back)
2. Share the anchor
3. Show main menu (Vuforia or solo mode, input robot IP and port if not set in config)
4. Start vuforia from main menu, scan the robot markers, or pick a position for solo mode
5. Spawn the Synced Robot, Waypoints manager, etc

### Multiple HoloLens mode
### Multiple HoloLens + Robot mode
### Multiple HoloLens + PC mode
### Multiple HoloLens + PC + Robot mode
### Multiple PC mode

1. Select connection (Host or join)
2. Host shares the anchor
3. If Host, start main menu (Vuforia or solo mode, input robot IP and port if not set in config) (Client shows waiting~~)
4. Start vuforia from main menu, scan the robot markers, or pick a position for solo mode
5. Host Spawns the Synced Robot, Waypoints manager, etc
6. If PC is the host, then don't ask for vuforia, only support solo mode (With or without robot)
7. If PC is a client, then the camera needs to follow the robot once they are positioned.


### Spectator View mode (Multiple HoloLens + PC with Camera mode)
### Spectator View mode (Multiple HoloLens + PC with Camera + Robot mode)

1. Host has to be PC. PC auto become host. One HoloLens is dedicated spectator view (Conflicts with Vuforia)
2. **Watch for cam device conflicts**
3. Other HoloLens autojoin the session (Auto join (re-deploy) is required)
4. One HoloLens start menu (Vuforia or solo mode, input robot IP and port if not set in config)
5. Spawn the robot from the position


## Steps
### Enable HoloLens + PC with network manager and synced robot prefab (Including everything) on UNet
### Take the prefab, and then start the spectator view project

