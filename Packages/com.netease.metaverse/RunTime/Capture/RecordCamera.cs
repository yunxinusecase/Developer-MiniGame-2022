using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MetaVerse.FrameWork;

namespace MetaVerse.FrameWork.Capture
{
    public class RecordCamera : MonoBehaviour
    {
        private string mRecordName = string.Empty;
        private bool mRecording = false;

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            EventManager.TriggerEvent<RenderTexture>(CaptureEventsDefine.Event_OnCameraRendered, src);
            Graphics.Blit(src, dest);
        }
    }
}
