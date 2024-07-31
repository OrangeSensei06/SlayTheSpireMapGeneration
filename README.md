# SlayTheSpireMapGeneration
Project that generates unique Slay the Spire style maps with multiple starting points and paths to a boss node!
  This project uses this page logic as for its implementation: https://steamcommunity.com/sharedfiles/filedetails/?id=2830078257

## Features (Map Generates from Left to Right)

- Weighted random node placement
- Custom floor-specific node weights
- Node type restrictions and rules
- Path generation with validation
- Node and path visualizatio
- Almost instant map generation logic runtime ( Avg. Record time: 0.003896 seconds) WOW!

  
## Getting Started

### Prerequisites

- Unity 2019.4 or later
- .NET Framework 4.7.1 or later

###Installation
- Download the project files as zip and open it with unity Hub
- Open the "Main Scene" for checking how the scene is setup!
- Have fun!


### Usage

1. Configure the map generation settings in the `GenerationData` ScriptableObject.
2. Run the scene to generate the map.
3. Press "Generate" Button on `MapGeneration` Inspector to regenerate maps mid game!
   

## Generation Data Scriptable Object Instructions

### 1. Basic Map Data
   
![image](https://github.com/user-attachments/assets/2d1b3146-94e3-44f3-b585-bb250b5fb51b)

- `MapFloors` is for the number of floors (X axis) you want, The map will generate +1 of the Map Floors as total amount (the extra one is for the boss node that will generated through `MapGeneration` Script
- `NodesPerFloor` is the number of vertical nodes (Y axis) that will be used to make paths and find neighbouring nodes

- `MinStartPoints` and `MaxStartPoints` are values you would use to control the number of starting points you would like for your map, after reaching the MaxStartPoints the logic would make sure to choose the same starting points if we have to generate more paths

- `StartingPaths` is the value for the total number of path that would connect the start points to the boss nodes (WARNING: The value of `StartingPaths` should always be greater or equals to `MinStartPoints`

### 2. Node Weights

![image](https://github.com/user-attachments/assets/8e04154b-71ff-481a-be42-5671efe1f134)

- The `NodeWeight` List contains the basic chances for a node type to get selected when generating a node, this is the one that will be used if there are no restrictions or any rules alloted to a specific floor
            - NOTE: The weight value is relative, you can easily exceed 100 and it will still return a value based on the respective weights, The Function to select a value can be found on  `Utils\OrangeUtils.cs`

- The `CustomWeightedFloors` Contains data that have info like 
        - "Floor" -> the floor you want this weight to use
        - "Target Node" -> the node type you want to have custom weight for
        - "Custom Weight" -> Set it 1 to make sure this is the only one that gets selected!

- The `NodeRestrictions` Contains the main focus of this project, It have some basic properties like "Min Floor" that makes sure it doesnt appear on floors below the value given, A check to make sure the same node type can appear consecutive or not, And a dont appear on floor that makes sure it doesnt appear on a specific floor!

### 3. Encounter Data
![image](https://github.com/user-attachments/assets/96716812-50cc-4e01-b1f7-6b79ceaba574)
![image](https://github.com/user-attachments/assets/baaad225-494a-4038-8ad1-c838b52ffcb7)

- The `Encounter Chances` is where you can have all the different types of encounters that a map can have with thier chances, IT works sames as the `NodeWeight` list!
  - In this project I used this to change the spirte of the MapNode to show a example of how this class can be used!


## Contributing

1. Fork the repository.
2. Create a new branch: `git checkout -b feature-name`
3. Make your changes and commit them: `git commit -m 'Add some feature'`
4. Push to the branch: `git push origin feature-name`
5. Open a pull request.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

## Acknowledgments

- thanks to `https://game-icons.net/ ` for using ingame icons



