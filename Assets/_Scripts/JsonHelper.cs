using System;
using UnityEngine;

namespace _Scripts
{
    public class JsonHelper
    {
        #region Json Helper for Multiple Array of object

        /// <summary>
        /// This method is only applicable if the data provided form the server is in multiplayer object array format
        /// </summary>
        /// <param name="json"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] FromJson<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>("{\"Items\":" + json + "}");
            return wrapper.Items;
        }


        /// <summary>
        /// This method is applicable for the array which is wrapped in a class but has single simple array data.
        /// </summary>
        /// <param name="json"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] FromJsonWithObject<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.Items;
        }

        public static string ToJsonForNonClassArray<T>(T[] singleArray)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = singleArray;
            string json = JsonUtility.ToJson(wrapper);

            string returnValue = String.Empty;
            int totalLoop = json.Length;
            for (int i = 9; i < totalLoop - 1; i++)
            {
                returnValue += json[i];
            }

            return returnValue;
        }

        public static string ToJson<T>(T[] array)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper);
        }

        public static string ToJson<T>(T[] array, bool prettyPrint)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }

        #endregion
    }
}