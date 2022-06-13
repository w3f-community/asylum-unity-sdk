mergeInto(LibraryManager.library, {

   RequestTemplates: function () {
      window.dispatchReactUnityEvent("Templates");
   },
   RequestUserItems: function () {
      window.dispatchReactUnityEvent("UserItems");
   },
   RequestGameMeta: function () {
      window.dispatchReactUnityEvent("GameMeta");
   },
   RequestTemplateByID: function (templateId) {
      window.dispatchReactUnityEvent("TemplateByID", templateId);
   },
   RequestInterpretationsByTemplateID: function (templateId) {
      window.dispatchReactUnityEvent("InterpretationsByTemplateID", templateId);
   },
   RequestInterpretationsByItemID: function (templateId, itemId) {
      window.dispatchReactUnityEvent("InterpretationsByItemID", templateId, itemId);
   },
   RequestGameClose: function () {
      window.dispatchReactUnityEvent("GameClose");
   },
   RequestOpenMarketPlace: function () {
      window.dispatchReactUnityEvent("OpenMarketPlace");
   },
   OnControllerLoaded: function () {
      window.dispatchReactUnityEvent("ControllerLoaded");
   },
   OnControllerUnloaded: function () {
      window.dispatchReactUnityEvent("ControllerUnloaded");
   },
});