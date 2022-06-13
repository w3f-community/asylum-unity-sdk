using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;
using System.Collections;
using System.Net;
using System;
using Newtonsoft.Json;
using System.Text;
using UnityEngine.UI;

namespace Asylum
{
    public class ReactControllerScript : MonoBehaviour
    {

#if UNITY_WEBGL && !UNITY_EDITOR

        [DllImport("__Internal")]
        public static extern void RequestUserItems();

        [DllImport("__Internal")]
        public static extern void RequestTemplates();

        [DllImport("__Internal")]
        public static extern void RequestGameMeta();

        [DllImport("__Internal")]
        public static extern void RequestTemplateByID(int templateId);

        [DllImport("__Internal")]
        public static extern void RequestInterpretationsByTemplateID(int templateId);

        [DllImport("__Internal")]
        public static extern void RequestInterpretationsByItemID(int templateId, int itemId);

        [DllImport("__Internal")]
        public static extern void RequestGameClose();

        [DllImport("__Internal")]
        public static extern void RequestOpenMarketPlace();
        
        [DllImport("__Internal")]
        public static extern void OnControllerLoaded();

        [DllImport("__Internal")]
        public static extern void OnControllerUnloaded();
#endif
        private List<AsylumItem> _unicItems = new();

        private Dictionary<string, Dictionary<string, string>> _itemLoadedMetadata = new();
        private Dictionary<InterpretationCombineID, byte[]> _interpretationLoadedData = new();
        private Dictionary<InterpretationCombineID, Dictionary<string, string>> _interpretationLoadedMetadata = new();

        [SerializeField] private string path = @"http://127.0.0.1:8080/ipfs/";

        public Action<List<AsylumItem>> OnItemsAddedAction;

        public Action<string, Dictionary<string, string>> OnItemMetadataLoadedAction;

        public Action<InterpretationCombineID, string[], byte[]> OnInterpretationSourceLoadedAction;

        public Action<InterpretationCombineID, Dictionary<string, string>> OnInterpretationMetadataLoadedAction;

        public Action OnPauseRequestedAction;

        public bool itemsInited = false;
        public List<AsylumItem> UnicItems => _unicItems;

        void Start()
        {

            DontDestroyOnLoad(this);

#if UNITY_WEBGL && !UNITY_EDITOR

            OnControllerLoaded();

            RequestUserItems();
#elif UNITY_EDITOR

#endif
        }
        private void OnDestroy()
        {
#if UNITY_WEBGL && !UNITY_EDITOR

            OnControllerUnloaded();
#endif
        }

        
        private void StartLoadItemsInterpretationData()
        {
            foreach(var item in _unicItems)
            {
                //Start loading item metadata
                StartCoroutine(LoadingCoroutine<string>($"{path}{item.metadata}", item.templateId, OnItemMetadaLoaded));
                
                foreach (var interpritation in item.interpretations)
                {
                    string interpretationPath = $"{path}{interpritation.interpretation.src}";

                    InterpretationCombineID interpretationID = new InterpretationCombineID
                    {
                        templateID = item.templateId,
                        interpretationID = interpritation.interpretation.id
                    };

                    //Start loading interpretation source
                    StartCoroutine(LoadingCoroutine<InterpretationCombineID>(interpretationPath, interpretationID, OnInterpretationSourceLoaded));

                    string metadataPath = $"{path}{interpritation.interpretation.metadata}";

                    //Start loading interpretation metadata
                    StartCoroutine(LoadingCoroutine<InterpretationCombineID>(metadataPath, interpretationID, OnInterpretationMetadataLoaded));
                }
            }
        }

        private IEnumerator LoadingCoroutine<T>(string url, T id, Action<T, byte[]> OnDataLoaded)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Request error : {request.error}");
                yield break;
            }

            var bytes = request.downloadHandler.data;

            OnDataLoaded?.Invoke(id, bytes);
        }

        private void OnItemMetadaLoaded(string templateID, byte[] rawData)
        {
            if (_itemLoadedMetadata.ContainsKey(templateID))
            {
                UnityEngine.Debug.LogWarning($"Already have such item metadata!");
                return;
            }

            if (rawData != null && rawData.Length > 0)
            {
                string jsonReturned = Encoding.UTF8.GetString(rawData);

                Dictionary<string, string> metadataDictionary = new Dictionary<string, string>();
                metadataDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonReturned);

                if (metadataDictionary != null && metadataDictionary.Count > 0)
                {
                    _itemLoadedMetadata.Add(templateID, metadataDictionary);
                    OnItemMetadataLoadedAction?.Invoke(templateID, metadataDictionary);
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"ERROR Item.metadata parsing ID : {templateID} ");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning($"ERROR Item.metadata null or empty ID : {templateID}");
            }
        }

        public Dictionary<string, string> GetItemMetadata(string templateID)
        {
            if (_itemLoadedMetadata.TryGetValue(templateID, out Dictionary<string, string> loadedData))
            {
                if (loadedData.Count > 0)
                {
                    return loadedData;
                }
            }

            return null;
        }

        private void OnInterpretationSourceLoaded(InterpretationCombineID interpretationID, byte[] rawData)
        {
            if (_interpretationLoadedData.ContainsKey(interpretationID))
            {
                UnityEngine.Debug.LogWarning($"Already have such source data!");
                return;
            }

            if (rawData != null && rawData.Length > 0)
            {
                _interpretationLoadedData.Add(interpretationID, rawData);

                OnInterpretationSourceLoadedAction?.Invoke(interpretationID, GetInterpretationTags(interpretationID), rawData);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"ERROR Intr.source null or empty ID : {interpretationID.templateID} {interpretationID.interpretationID}");
            }
        }

        public byte[] GetInterpretationSourceData(InterpretationCombineID interpretationID)
        {
            if (_interpretationLoadedData.TryGetValue(interpretationID, out byte[] loadedData))
            {
                if (loadedData.Length > 0)
                {
                    return loadedData;
                }
            }

            return null;
        }

        public string[] GetInterpretationTags(InterpretationCombineID interpretationID)
        {
            var foundItem = _unicItems.Find(item => item.templateId == interpretationID.templateID);

            if (foundItem != null)
            {
                var interpretationFounded = foundItem.interpretations.First(interpretation => 
                                        interpretation.interpretation.id == interpretationID.interpretationID);

                if (interpretationFounded != null && interpretationFounded.tags != null && interpretationFounded.tags.Length > 0)
                {
                    return interpretationFounded.tags;
                }
            }

            return null; //This, actually, should never happend
        }

        private void OnInterpretationMetadataLoaded(InterpretationCombineID interpretationID, byte[] rawData)
        {
            if (_interpretationLoadedMetadata.ContainsKey(interpretationID))
            {
                UnityEngine.Debug.LogWarning($"Already have such interpretation metadata!");
                return;
            }

            if (rawData != null && rawData.Length > 0)
            {
                string jsonReturned = Encoding.UTF8.GetString(rawData);

                Dictionary<string, string> metadataDictionary = new Dictionary<string, string>();
                metadataDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonReturned);

                if (metadataDictionary != null && metadataDictionary.Count > 0)
                {
                    _interpretationLoadedMetadata.Add(interpretationID, metadataDictionary);
                    OnInterpretationMetadataLoadedAction?.Invoke(interpretationID, metadataDictionary);
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"ERROR Intr.metadata parsing ID : {interpretationID.templateID} {interpretationID.interpretationID}");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning($"ERROR Intr.metadata null or empty ID : {interpretationID.templateID} {interpretationID.interpretationID}");
            }

        }

        public Dictionary<string, string> GetInterpretationMetadata(InterpretationCombineID interpretationID)
        {
            if (_interpretationLoadedMetadata.TryGetValue(interpretationID, out Dictionary<string, string> loadedData))
            {
                if (loadedData.Count > 0)
                {
                    return loadedData;
                }
            }

            return null;
        }

        public void OnExitRequested()
        {
#if UNITY_WEBGL && !UNITY_EDITOR

            RequestGameClose();
#endif
        }

        #region Calling from React

        public void PauseGame()
        {
            OnPauseRequestedAction?.Invoke();
        }

        public void ParseItems(string jsonString)
        {
            AsylumItem[] items = JsonHelper.FromJson<AsylumItem>(jsonString);

            foreach (var item in items)
            {
                if (_unicItems.Any(savedItem => savedItem.templateId == item.templateId))
                {
                    //We have this item already
                    continue;
                }

                _unicItems.Add(item); //MB Start loading by single item right after it parsing?

               // UnityEngine.Debug.Log($"ReactController Item w/ templateID {item.templateId} was parsed and loaded");
            }

            StartLoadItemsInterpretationData();

            itemsInited = true;

            OnItemsAddedAction?.Invoke(_unicItems); // MB Before loading starting
        }


        public void ParseTemplates(string jsonString)
        {
            AsylumTemplate[] items = JsonHelper.FromJson<AsylumTemplate>(jsonString);
            // Do something here
        }

        public void ParseGameMetadata(string jsonString)
        {
            GameMetadata gameMetadata = JsonUtility.FromJson<GameMetadata>(jsonString);
            // Do something here
        }

        public void ParseTemplate(string jsonString)
        {
            AsylumTemplate items = JsonUtility.FromJson<AsylumTemplate>(jsonString);
            // Do something here
        }

        public void ParseInterpretations(string jsonString)
        {
            Interpretation[] items = JsonHelper.FromJson<Interpretation>(jsonString);
            // Do something here
        }
        #endregion
    }
}
