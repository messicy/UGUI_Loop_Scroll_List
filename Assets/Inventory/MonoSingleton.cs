using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    protected static T instance;

    /// <summary>
    /// 场景切换的时候销毁
    /// </summary>
    public static bool DestroyOnLoad = false;
    public static GameObject MonoSingletonGo;
    public static T Instance
    {
        get
        {
            if (MonoSingletonGo == null)
            {
                instance = FindObjectOfType<T>();
                if(instance != null)
                {
                    MonoSingletonGo = instance.gameObject;
                    DontDestroyOnLoad(MonoSingletonGo);
                }
            }

            if (MonoSingletonGo == null)
            {
                MonoSingletonGo = new GameObject("MonoSingleton");
                DontDestroyOnLoad(MonoSingletonGo);
            }

            if (MonoSingletonGo != null && instance == null)
                instance = MonoSingletonGo.AddComponent<T>();

            return instance;
        }
    }
}