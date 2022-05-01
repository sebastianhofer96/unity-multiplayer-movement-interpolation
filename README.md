# Unity-Multiplayer-Movement-Interpolation

This project is part of the Master's thesis "Multiplayer Games: Client-Side Interpolation of Player Movement" at [University of Applied Sciences Technikum Vienna](https://www.technikum-wien.at/en/).

## Abstract
Interpolation allows the client of a multiplayer game to estimate the player movements between the game state updates received by the authority over the network. There are several factors that decide about the quality of the estimation including the interpolation algorithms, the frequency of the updates and possible network impairments. The game state consistency influences the experience, performance and gameplay time of the user. The thesis also covers network fundamentals, synchronization of multiplayer games in general and side effects that can occur in games. In addition, techniques to eliminate the latency caused by interpolation and mitigate the effects of packet loss are presented.

By developing a multiplayer game prototype and simulating the movement of a player under different conditions, several research questions are answered. The interpolation of the player position is performed linearly and using Catmull-Rom curves while the player rotation is interpolated using Slerp and Squad. First, the performance of algorithms which require four updates are compared to algorithms which require only two updates. Furthermore, the frequency of the game state updates is varied, and network packet loss is artificially added to observe the impact. The results show that the simpler linear interpolation and Slerp achieve either the same or better scores than the interpolation using Catmull-Rom curves and Squad regardless of the update frequency and the lost packets. An update frequency of at least 15 game states per second ensures smooth motions even with the simpler algorithms.

## Content

The repository includes the game client, game server and benchmark tool used to simulate player movement and evaluate the performance of client-side interpolation algorithms with configurable tick rates and packet loss.

### Game Client
![Game Client](https://user-images.githubusercontent.com/1010651/160008741-7cb2f28b-f9a9-42bc-a8b2-a85c7b21ecdd.png)

### Game Server
| Argument           | Default | Description                                   |
| ------------------ | ------- | --------------------------------------------- |
| --port             | 7777    | The port the game server listens on.          |
| --max-client-count | 2       | The maximum client count the server accepts.  |
| --simulation-rate  | 60      | The rate (in Hz) of simulation udpates.       |
| --network-rate     | 30      | The rate (in Hz) of network updates.          |
| --packet-loss      | 0       | The percentage of simulated packet loss.      |
| --save-samples     | false   | Whether benchmark samples are saved.          |

### Benchmark Tool
![Benchmark Tool](https://user-images.githubusercontent.com/1010651/161638515-585b28fb-2510-41eb-9fba-740cb931795d.png)

## Step-By-Step
Create symlink in *"GameClient\Assets"*: `mklink /D Shared ..\..\GameServer\Assets\Shared`

Path for export/import: *"%userprofile%\AppData\LocalLow\Sebastian Hofer\\"*

1. __Record Player Movement__
    1. Start the server.
    2. Start the client ("Main" scene) and connect to the server.
    3. Manually perform the desired movement and press "1" to save it as *"GameClient\intentions.csv"*.
    4. Copy the intentions to *"GameServer\intentions.csv"*.

2. __Generate Movement Samples__
    1. Start the server with "--save-samples true".
    2. Start the client ("Main" scene) and connect the client to the server with desired options including "Use Simulation Input" and "Save Samples".
    3. The movement is performed automatically and the samples are saved as *"GameServer\GameServer_samples.csv"* and *"GameClient\GameClient_samples.csv"*.
    4. Move the server samples to *"GameClient\GameServer_samples.csv"*.

3. __Generate Benchmark Report__
    1. Start the client ("Benchmark" scene).
    2. The samples are loaded and the paths are visualized.
    3. Generated report is saved as *"GameClient\report.csv"* which includes the interpolation errors for player position and rotation at every tick.