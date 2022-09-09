using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif
public class PermissionHelper
{
    public static void RequestMicroPhonePermission()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
		    if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
		    {                 
			    Permission.RequestUserPermission(Permission.Microphone);
		    }
#endif
    }

    public static void RequestCameraPermission()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
		    if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
		    {                 
			    Permission.RequestUserPermission(Permission.Camera);
		    }
#endif
    }
}
