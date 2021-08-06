using System;
using UnityEngine;

static class JsonHelper
{
    //Usage:
    //YouObject[] objects = JsonHelper.getJsonArray<YouObject>(jsonString);
    public static T[] getJsonArray<T>(string json)
    {
        var wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.array;
    }

    //Usage:
    //string jsonString = JsonHelper.arrayToJson<YouObject>(objects);
    public static string arrayToJson<T>(T[] array)
    {
        var wrapper = new Wrapper<T>();
        wrapper.array = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static T getJsonObject<T>(string json)
    {
        return JsonUtility.FromJson<T>(json);
    }

    [Serializable]
    class Wrapper<T>
    {
        public T[] array;
    }
}