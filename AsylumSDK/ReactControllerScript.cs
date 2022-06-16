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
        private List<AsylumItem> _itemsList = new();

        private Dictionary<ItemCombineID, Dictionary<string, string>> _itemLoadedMetadata = new();
        private Dictionary<InterpretationCombineID, byte[]> _interpretationLoadedData = new();
        private Dictionary<InterpretationCombineID, Dictionary<string, string>> _interpretationLoadedMetadata = new();

        [SerializeField] private string path = @"http://127.0.0.1:8080/ipfs/";

        public Action<List<AsylumItem>> OnItemsAddedAction;

        public Action<ItemCombineID, Dictionary<string, string>> OnItemMetadataLoadedAction;

        public Action<InterpretationCombineID, string[], byte[]> OnInterpretationSourceLoadedAction;

        public Action<InterpretationCombineID, Dictionary<string, string>> OnInterpretationMetadataLoadedAction;

        public Action OnPauseRequestedAction;

        public bool itemsInited = false;
        public List<AsylumItem> ItemsList => _itemsList;

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
            foreach(var item in _itemsList)
            {

                var currentItemID = new ItemCombineID(item.templateId, item.id);
                //Start loading item metadata
                StartCoroutine(LoadingCoroutine<ItemCombineID>($"{path}{item.metadata}", currentItemID, OnItemMetadaLoaded));
                
                foreach (var interpritation in item.interpretations)
                {
                    string interpretationPath = $"{path}{interpritation.interpretation.src}";

                    InterpretationCombineID interpretationID = new InterpretationCombineID
                    {
                        itemCombineID = currentItemID,
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

        private void OnItemMetadaLoaded(ItemCombineID itemID, byte[] rawData)
        {
            if (_itemLoadedMetadata.ContainsKey(itemID))
            {
                UnityEngine.Debug.LogWarning($"React Already have such item metadata!");
                return;
            }

            if (rawData != null && rawData.Length > 0)
            {
                string jsonReturned = Encoding.UTF8.GetString(rawData);

                Dictionary<string, string> metadataDictionary = new Dictionary<string, string>();
                metadataDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonReturned);

                if (metadataDictionary != null && metadataDictionary.Count > 0)
                {
                    _itemLoadedMetadata.Add(itemID, metadataDictionary);
                    OnItemMetadataLoadedAction?.Invoke(itemID, metadataDictionary);
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"React ERROR Item.metadata parsing ID : {itemID} ");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning($"React ERROR Item.metadata null or empty ID : {itemID}");
            }
        }

        public Dictionary<string, string> GetItemMetadata(ItemCombineID itemID)
        {
            if (_itemLoadedMetadata.TryGetValue(itemID, out Dictionary<string, string> loadedData))
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
                UnityEngine.Debug.LogWarning($"React Already have such source data!");
                return;
            }

            if (rawData != null && rawData.Length > 0)
            {
                _interpretationLoadedData.Add(interpretationID, rawData);

                OnInterpretationSourceLoadedAction?.Invoke(interpretationID, GetInterpretationTags(interpretationID), rawData);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"React ERROR Intr.source null or empty ID : {interpretationID.itemCombineID.ToString()} {interpretationID.interpretationID}");
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
            var foundItem = _itemsList.Find(item => item.templateId == interpretationID.itemCombineID.templateID
                                                    && item.id == interpretationID.itemCombineID.itemID);

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
                UnityEngine.Debug.LogWarning($"React Already have such interpretation metadata!");
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
                    UnityEngine.Debug.LogWarning($"React ERROR Intr.metadata parsing ID : {interpretationID.itemCombineID.ToString()} {interpretationID.interpretationID}");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning($"React ERROR Intr.metadata null or empty ID : {interpretationID.itemCombineID.ToString()} {interpretationID.interpretationID}");
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
                _itemsList.Add(item); //MB Start loading by single item right after it parsing?
            }

            StartLoadItemsInterpretationData();

            itemsInited = true;

            OnItemsAddedAction?.Invoke(_itemsList); // MB Before loading starting
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
