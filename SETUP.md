# Setup
## For code changes
If you want to change anything in the code of this project, just download or clone the `WaterWheel.cs` file and open it in an editor of your choice. If you are using an IDE with autocompletion (i.e. IntelliSense in Visual Studio), you will have to add references to some of the `.dll` files in the `<Raft directory>/Raft_Data/Managed` folder.

## For asset changes
If you want to add or modify the assets of this project, you need to set up a Unity project with the Raft source files. As Unity generates unique IDs for each asset when the project is set up, you need to use a specific project setup. This mod's assets are built with a raft export for Raft version 9.05 (build 3847312), that is [available on GitLab](https://gitlab.com/traxam/raft-unity-project/tree/79200901d5e89b46800f9a6a5906e04446bbf0fa). Please contact me to request access to the repository as you need it to get these assets working.

1. Clone or download the project mentioned above and open it with Unity 2018.3.7f1.
2. Clone or download the WaterWheels repository and copy it to the Assets folder Unity project.
3. The WaterWheel assets should now be visible and editable in the Unity editor.

### Building an asset bundle
All assets for this mod should have `waterwheel.assets` selected in the `Asset Labels` menu. Use [this script](https://gitlab.com/snippets/1871560) to generate the asset bundle. It can then be found in the `Assets/AssetBundles` directory in your Unity project.

### Common issues
- **`Placeable_WaterWheel_Item` can not be opened**: If the `Placeable_WaterWheel_Item` can not be opened you most likely have used the wrong Raft export.
- **`NullReferenceException` in the Start block**: The `Placeable_WaterWheel_Item` settings might have lost the buildable block prefab. Drag the prefab in the empty array.
