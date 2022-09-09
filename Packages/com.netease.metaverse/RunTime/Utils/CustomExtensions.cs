using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CustomExtensions
{
    public static T GetOrAddComponent<T>(this GameObject target) where T : Behaviour
    {
        T comp = target.GetComponent<T>();
        if (comp == null)
            comp = target.AddComponent<T>();
        return comp;
    }

    public static void ResetTransformation(this Transform trans)
    {
        trans.position = Vector3.zero;
        trans.localRotation = Quaternion.identity;
        trans.localScale = new Vector3(1, 1, 1);
    }
}
