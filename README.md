# Asylum Unity SDK

Asylum Unity SDK. Provides plugins, editor scripts, and a set of utilities to integrate with Asylum ecosystem

### Installation
- Just put files into your `Assets/Plugins` folder
- Create `GameObject` named "ReactController" in the scene and add `ReactControllerScript.cs` as component to it 

> Note: Asylum SDK is using Newton.json package for (de)serializing `Dictionary`. You have to load it manually from Unity Package Manager. 

### API

Actions:
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
