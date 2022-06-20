# Asylum Unity SDK

Asylum Unity SDK. Provides plugins, editor scripts, and a set of utilities to integrate with Asylum ecosystem

NOTE: This plugin version created for the Unity builds at the WebGL platform and using react-unity(link) for connection between web page context and in-game content. 

## Asylum plugin:
- jslib file responsible for connection between Unity and Asylum React.app
- AsylumEntities.cs C# code file contains entities presenting data
- ReactControllerScript.cs C# file responsible for connection between Unity life circle and jsib, parsing and downloading on-chain data

### ReactControllerScript API
Actions and properties:
- `UniqItems` - returns the list of the loaded items
- `GetItemMetadata` - returns item's metadata
- `GetInterpretationSourceData` - returns interpretation source data
- `GetInterpretationTags` - retursn array of the interpretation tags
- `GetInterpretationMetadata` -returns interpretation metadata

Events:
- `OnItemsAddedAction` - when all user items was parsed and initialized in the ReactController
- `OnItemMetadataLoadedAction` - when item metadata was loaded
- `OnInterpretationSourceLoadedAction` - when interpretation source data was loaded
- `OnInterpretationMetadataLoadedAction` - when interpretation metadata was loaded
- `OnPauseRequestedAction` - when react.application is requested game pause


## Installation
- Create or open Unity project (supports version 2021.3 and above)
- [Import](https://docs.unity3d.com/Manual/AssetPackagesImport.html) [Newtonsoft Json Unity Package](https://docs.unity3d.com/Packages/com.unity.nuget.newtonsoft-json@2.0/manual/index.html) (dependency for Asylum SDK)
- Put AsylumSDK into Unity **Assets/Plugins** folder or create [submodule](https://git-scm.com/book/en/v2/Git-Tools-Submodules) at that path

## Using in the Unity project
- Create empty GameObject named “**ReactController**” in the scene and add ReactControllerScript.cs as its component
- Create new C# script(f.e. _ItemsController_) and link ReactControllerScript component to it (via inspector using public/serializable fields or [FindObjectOfType method](https://docs.unity3d.com/ScriptReference/Object.FindObjectOfType.html) or DI like Zenject)
    ```cs
        //ItemsController.cs

        void Start
        {
            ReactControllerScript reactControllerInstance = FindObjectOfType<ReactControllerScript>();
        }
    ```
- Get user items list in the _ItemsController_ simply calling property ItemsList or subsribe on the event OnItemsAddedAction
    ```cs
        //ItemsController.cs

        ReactControllerScript reactControllerInstance;

        void Start()
        {
            List<AsylumItem> items = reactControllerInstance.ItemsList;
            //or
            reactControllerInstance.OnItemsAddedAction += OnItemAdded;
        }

        void OnItemAdded(List<AsylumItem> items)
        {
            ...
        }
    ```
- Get items data by the loading end event (f.e. OnItemMetadataLoadedAction) or manually by the method (f.e. GetItemMetadata)
    ```cs
        //ItemsController.cs

        ReactControllerScript reactControllerInstance;
        ItemCombineID id;

        void Start()
        {
            Dictionary<string, string> itemMetadata = reactControllerInstance.GetItemMetadata(id);
            //or
            reactControllerInstance.OnItemMetadataLoadedAction += OnItemMetadataLoaded;
        }

        void OnItemMetadataLoaded(ItemCombineID itemID , Dictionary<string, string> metadata)
        {
            ...
        }
    ```
- After getting the item's data it’s possible to display it in-game(f.e. required interpretation with “default-view” tag using [UI:Image](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/UIElements.Image.html))

    ```cs
        //ItemsController.cs

        ReactControllerScript reactControllerInstance;
        Image image;

        void Start()
        {
            reactControllerInstance.OnInterpretationSourceLoadedAction += OnInterpretationSourceLoaded;
        }

        private void OnInterpretationSourceLoaded(InterpretationCombineID interpretationID, string[] tags, byte[] rawData)
        {
            if (Array.Exists(tags, tag => tag == "default-view"))
            {
                Texture2D texture = new Texture2D(1, 1);
                if (texture.LoadImage(rawData))
                {
                    image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(texture.width / 2, texture.height / 2));
                }
            }
        }
    ```
## Testing
- For testing needed to switch platform into WebGL and press “Build” button in the “Build Settings” window (File > Build Settings or Ctrl+Shift+B)
- At the build end there will be 4 required files by the “BuildName/Build” path:
    - BuildName.data
    - BuildName.framework.js
    - BuildName.loader.js
    - BuildName.wasm

    Those files **must be** placed into “asylum-ui/packages/connection-library/data” with the same name as in the "asylum-ui/packages/      connection-library/seed/mocks.ts : IGameMockData : gameClient” - it's "**ReactBuild**" now
- For the next steps follow [Asylum react app installation](https://gitlab.com/asylum-space/asylum-ui/-/tree/main/packages/game-developers-console#run-game-developers-console-manual-setup).
