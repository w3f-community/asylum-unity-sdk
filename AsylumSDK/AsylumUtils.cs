using System;
using System.Net;
using UnityEngine;

namespace Asylum
{
    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            WrappedAsylumItems<T> wrapper = JsonUtility.FromJson<WrappedAsylumItems<T>>(json);
            return wrapper.Items;
        }

        public static string ToJson<T>(T[] array, bool prettyPrint = false)
        {
            WrappedAsylumItems<T> wrapper = new WrappedAsylumItems<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        [Serializable]
        private class WrappedAsylumItems<T>
        {
            public T[] Items;
        }
    }
}
