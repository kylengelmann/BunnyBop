using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    public static bool bHasInstance { get; private set; }
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (bHasInstance)
        {
            Debug.LogErrorFormat("An instance of type {0} already exists", typeof(T).ToString());
            DestroyImmediate(this);
        }
        else
        {
            Instance = (T)this;
            bHasInstance = true;
        }
    }
}
