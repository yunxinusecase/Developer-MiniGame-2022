using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class NetworkPermission : MonoBehaviour
{
    // Start is called before the first frame update
#if UNITY_IOS
    [DllImport("__Internal", EntryPoint = "triggerNetworkPermission")]
    internal static extern void triggerNetworkPermission();
#endif
    void Start()
    {

    }


    public void CheckPermission()
    {

        Console.WriteLine("CheckPermission Only for iOS");
#if UNITY_IOS
        triggerNetworkPermission();
#endif

    }

}