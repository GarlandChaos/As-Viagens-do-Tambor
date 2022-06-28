using System.Collections.Generic;
using UnityEngine;

public abstract class ALayer<T> : MonoBehaviour where T : IScreenController
{
    protected Dictionary<string, T> screens = new Dictionary<string, T>();

    public virtual void RegisterScreen(string screenID, T controller, Transform screenTransform)
    {
        if (!screens.ContainsKey(screenID))
        {
            controller.screenID = screenID;
            screens.Add(screenID, controller);
            screenTransform.SetParent(transform, false);
        }
    }

    public virtual void UnregisterScreen(string screenID)
    {
        if (screens.ContainsKey(screenID))
        {
            screens.Remove(screenID);
        }
    }

    public virtual void ShowScreen(string screenID)
    {
        screens[screenID].Show();
    }

    public virtual void HideScreen(string screenID)
    {
        screens[screenID].Hide();
    }

    public virtual bool HasScreen(string screenID)
    {
        if (screens.ContainsKey(screenID))
        {
            return true;
        }

        return false;
    }

    public virtual bool IsScreenVisible(string screenID)
    {
        return screens[screenID].isVisible;
    }

    public virtual void SaySize()
    {
        foreach (KeyValuePair<string, T> kvp in screens)
        {
            Debug.Log(kvp.Key + " " + kvp.Value);
        }
    }
}
