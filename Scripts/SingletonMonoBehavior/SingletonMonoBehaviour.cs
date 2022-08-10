using System;
using UnityEngine;

public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                Type t = typeof(T);

                instance = (T)FindObjectOfType(t);
                if (instance == null)
                {
                    var item = Resources.Load<GameObject>("Asset/ThingsYouNeedToLoadInResourcesFolder");
                    var go = Instantiate(item);
                    instance = go.GetComponent<T>();
                }
            }

            return instance;
        }
    }

    virtual protected void Awake()
    {
        // Check if other GameObject is attached.
        // If it is attached, discard it.
        if (this != Instance)
        {
            Destroy(this);
            //Destroy(this.gameObject);
            Debug.LogError(
                typeof(T) +
                " Is already broken by other GameObject, so the component has been destroyed." +
                " Attached GameObject " + Instance.gameObject.name + " is.");
            return;
        }
        DontDestroyOnLoad(this.gameObject);
    }

}
