using System.Collections;
using System.Collections.Generic;
using MetaVerse.FrameWork;
using UnityEngine;
using UnityEngine.Networking;

#if !UNITY_EDITOR && UNITY_ANDROID
using UnityEngine.Android;
#endif

using nertc;

namespace Mini.Battle.Core
{
    public class MiniGameBooter : SingletonMonoBehaviour<MiniGameBooter>
    {
        // Check Android Permission
        private List<string> mCheckedPermissions = new List<string>();
#if !UNITY_EDITOR && UNITY_ANDROID
        private void CheckNextPermission(string permissionName)
        {
            if (permissionName.Equals(Permission.Microphone))
                CheckPermission(Permission.Camera);
            else
                CheckPermission(Permission.Microphone);
        }

        internal void PermissionCallbacks_PermissionDeniedAndDontAskAgain(string permissionName)
        {
            Debug.Log($"{permissionName} PermissionDeniedAndDontAskAgain");
            CheckNextPermission(permissionName);
        }

        internal void PermissionCallbacks_PermissionGranted(string permissionName)
        {
            Debug.Log($"{permissionName} PermissionCallbacks_PermissionGranted");
            CheckNextPermission(permissionName);
        }

        internal void PermissionCallbacks_PermissionDenied(string permissionName)
        {
            Debug.Log($"{permissionName} PermissionCallbacks_PermissionDenied");
            CheckNextPermission(permissionName);
        }

        private void CheckPermission(string permission)
        {
            Debug.Log("CheckPermission " + permission);
            if (mCheckedPermissions.Contains(permission))
            {
                Debug.Log("Has Checked " + permission);
                return;
            }
            mCheckedPermissions.Add(permission);
            if (Permission.HasUserAuthorizedPermission(permission))
            {
                YXUnityLog.LogInfo(TAG, $"{permission} permission has been granted.");
                CheckNextPermission(permission);
            }
            else
            {
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
                callbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
                callbacks.PermissionDeniedAndDontAskAgain += PermissionCallbacks_PermissionDeniedAndDontAskAgain;
                Permission.RequestUserPermission(permission, callbacks);
            }
        }
#endif

        private string TAG = "MiniGameBooter";

        // 游戏启动申请，麦克风，摄像头权限，网络权限
        // 请开启上述三个权限
        void Awake()
        {
            var mainThread = Dispatcher.Current;
            Application.runInBackground = true;
#if !UNITY_EDITOR && UNITY_ANDROID
        // request permission
        CheckPermission(Permission.Microphone);
#endif
        }

        IEnumerator Start()
        {
            yield return null;
            GameManager.Instance.BootGame();

            // make test web request
            UnityWebRequest request = UnityWebRequest.Get("www.baidu.com");
            yield return request.SendWebRequest();
            YXUnityLog.LogInfo(TAG, $"request code: {request.responseCode.ToString()}");
        }
    }
}

