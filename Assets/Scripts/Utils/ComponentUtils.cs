using UnityEngine;

public static class ComponentUtils
{
    public static T Find<T>(string gameObjectName) where T : Component
    {
        return GameObject.Find(gameObjectName).GetComponent<T>();
    }

    public static T FindFirstWithTag<T>(string tag) where T : Component
    {
        return GameObject.FindGameObjectWithTag(tag).GetComponent<T>();
    }

}
