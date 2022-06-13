# Asylum Unity SDK

Asylum Unity SDK. Provides plugins, editor scripts, and a set of utilities to integrate with Asylum ecosystem

**Installing:**
- Just put files into your Assets/Plugins folder
- Create GameObject named "ReactController" in the scene and add ReactControllerScript.cs as component to it 

NOTE: _Asylum SDK using Newton.json package for (de)serializing Dictionary. You have to load it manually from Unity Package Manager_. 

The Asylum SDK providing functionality:
- OnItemsAddedAction - event's calling when all user items was parsed and initialized in the ReactController
- OnItemMetadataLoadedAction - event calls when item metadata was loaded
- OnInterpretationSourceLoadedAction - event calls when interpretation source data was loaded
- OnInterpretationMetadataLoadedAction - event calls when interpretation metadata was loaded
- OnPauseRequestedAction - event calls when react.application is requested game pause

Also provides posibility to get data manually:
- UnicItems - return list of the loaded items
- GetItemMetadata - return item's metadata
- GetInterpretationSourceData - return interpretation source data
- GetInterpretationTags - return array of the interpretation tags
- GetInterpretationMetadata -return interpretation metadata
