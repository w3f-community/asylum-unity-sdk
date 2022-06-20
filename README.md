# Asylum Unity SDK

Asylum Unity SDK. Provides plugins, editor scripts, and a set of utilities to integrate with Asylum ecosystem

The plugin is created for the Unity and WebGL builds. You can run this build natively within html page or inside React application with the help of [react-unity-webgl](https://www.npmjs.com/package/react-unity-webgl). To ease the integration with Asylum ecosystem you can run the build via [Game Developers Console](https://gitlab.com/asylum-space/asylum-ui/-/tree/main/packages/game-developers-console) ([see below](#run-the-build-inside-game-developer-console))

> Unity version supported: ^2021.3

## Asylum plugin:
- `jslib` file responsible for connection between Unity and JS code, which runs the build
- `AsylumEntities.cs` file contains type and entities you need to initialize Asylum NFT Items
- `ReactControllerScript.cs` file responsible for connection between Unity life circle and `jslib`, parsing and downloading on-chain data

## Installation
- Create or open Unity project (supports version 2021.3 and above)
- [Import](https://docs.unity3d.com/Manual/AssetPackagesImport.html) [Newtonsoft Json Unity Package](https://docs.unity3d.com/Packages/com.unity.nuget.newtonsoft-json@2.0/manual/index.html) (dependency for Asylum Unity SDK)
- Put Asylum Unity SDK into Unity `Assets/Plugins` folder

## Usage
- Create empty `GameObject` named `ReactController` in the scene and add `ReactControllerScript.c`s as its component
- Create new C# script(e.g. `ItemsController`) and link `ReactControllerScript` component to it. You can do it via inspector, using public/serializable fields or [FindObjectOfType method](https://docs.unity3d.com/ScriptReference/Object.FindObjectOfType.html) or with the help of dependency injection (like Zenject)
    ```cs
        //ItemsController.cs

        void Start
        {
            ReactControllerScript reactControllerInstance = FindObjectOfType<ReactControllerScript>();
        }
    ```
- Get user items list in the `ItemsController` simply calling property `ItemsList` or subsribe on the event `OnItemsAddedAction`
    ```cs
        // ItemsController.cs

        ReactControllerScript reactControllerInstance;

        void Start()
        {
            // Manual call
            // List<AsylumItem> items = reactControllerInstance.ItemsList;

            // Subsribe on event
            reactControllerInstance.OnItemsAddedAction += OnItemAdded;
        }

        void OnItemAdded(List<AsylumItem> items)
        {
            ...
        }
    ```
- Get user items metadata inside `ItemsController` by calling `GetItemMetadata` or subsribe on the event `OnItemMetadataLoaded`
    ```cs
        //ItemsController.cs

        ReactControllerScript reactControllerInstance;
        ItemCombineID id;

        void Start()
        {
            // Manual call
            // Dictionary<string, string> itemMetadata = reactControllerInstance.GetItemMetadata(id);
            
            // Subsribe on event
            reactControllerInstance.OnItemMetadataLoadedAction += OnItemMetadataLoaded;
        }

        void OnItemMetadataLoaded(ItemCombineID itemID , Dictionary<string, string> metadata)
        {
            ...
        }
    ```
- After getting the item's data it’s possible to display it in-game. Let's try to load `default-view` interpretation using [UI:Image](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/UIElements.Image.html))

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
## Run the build inside Game Developers Console
1. Firstly, you have to form WebGL build: **File > Build Settings > Build**. The WebGL build consists of 4 files:
    - BuildName.data
    - BuildName.framework.js
    - BuildName.loader.js
    - BuildName.wasm
2. Place these files inside **asylum-ui/packages/connection-library/data/build_name** and the path to the build within `const games: IGameMockData[]` inside **asylum-ui/packages/connection-library/seed/mocks.ts**:
```ts
export const games: IGameMockData[] = [
...
{
    id: 'game_id',
    title: 'Your Game',
    img: 'image_url',
    genre: '...',
    shortDescription: '...',
    description: '...',
    gallery: [...],
    supportedTemplates: [0, 1, 2, 3],
    gameClient: {
         data: 'data/build_name/BuildName.data',
         framework: 'data/build_name/BuildName.framework.js',
         loader: 'data/build_name/BuildName.loader.js',
         wasm: 'data/build_name/BuildName.wasm',
    }
}
...
```

3. Follow the steps to run [Game Developers Console (manual setup)](https://gitlab.com/asylum-space/asylum-ui/-/tree/main/packages/game-developers-console#run-game-developers-console-manual-setup) locally and seed the data.

## ReactControllerScript API
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
