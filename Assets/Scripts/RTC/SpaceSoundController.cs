using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using nertc;

namespace Mini.Battle.RTC
{
    public class SpaceSoundController : MonoBehaviour
    {
        public Transform mTargetTransform;
        private IRtcEngine engine = IRtcEngine.GetInstance();
        private int AudibleDistance = 30;
        private int ConversationalDistance = 1;
        private RtcDistanceRolloffModel RollOffModel = RtcDistanceRolloffModel.kNERtcDistanceRolloffLinear;
        private RtcSpatializerRenderMode Audio3DRenderMode = RtcSpatializerRenderMode.kNERtcSpatializerRenderStereoPanning;
        private bool EnableRoomEffect = false;
        private RtcSpatializerRoomCapacity RoomCapacity = RtcSpatializerRoomCapacity.kNERtcSpatializerRoomCapacityLarge;
        private RtcSpatializerMaterialName RoomMaterial = RtcSpatializerMaterialName.kNERtcSpatializerMaterialTransparent;
        private float RoomReflectionScalar = 1.0f;
        private float RoomReverbGain = 1.0f;
        private float RoomReverbTime = 1.0f;
        private float RoomReverbBrightness = 1.0f;

        private string TAG = "SpaceSoundController";

        public void Enable3DAudio()
        {
            //open 3d Audio
            int ret = engine.EnableSpatializer(true);
            YXUnityLog.LogInfo(TAG, $"EnableSpatializer ret: {ret}");
            ret = engine.SetSpatializerRenderMode(Audio3DRenderMode);
            YXUnityLog.LogInfo(TAG, $"SetSpatializerRenderMode ret: {ret}");
            ret = engine.UpdateSpatializerAudioRecvRange(AudibleDistance, ConversationalDistance, RollOffModel);
            YXUnityLog.LogInfo(TAG, $"UpdateSpatializerAudioRecvRange ret: {ret}");
            //audio profile must be stereo,2 channels
            ret = engine.SetAudioProfile(RtcAudioProfileType.kNERtcAudioProfileMiddleQualityStereo, RtcAudioScenarioType.kNERtcAudioScenarioMusic);
            YXUnityLog.LogInfo(TAG, $"SetAudioProfile ret: {ret}");
            //enable room effect if need
            engine.EnableSpatializerRoomEffects(EnableRoomEffect);
            YXUnityLog.LogInfo(TAG, $"EnableSpatializerRoomEffects ret: {ret}");

            if (EnableRoomEffect)
            {
                var roomProperties = new RtcSpatializerRoomProperty
                {
                    roomCapacity = RoomCapacity,
                    material = RoomMaterial,
                    reflectionScalar = RoomReflectionScalar,
                    reverbGain = RoomReverbGain,
                    reverbTime = RoomReverbTime,
                    reverbBrightness = RoomReverbBrightness,
                };
                engine.SetSpatializerRoomProperty(roomProperties);
            }
        }

        public void Update3DSpaceSoundTransform()
        {
            if (mTargetTransform != null && engine != null)
            {
                var info = new RtcSpatializerPositionInfo();
                info.headPosition = new float[3] { mTargetTransform.position.x, mTargetTransform.position.y, mTargetTransform.position.z };
                info.headQuaternion = new float[4] { mTargetTransform.rotation.x, mTargetTransform.rotation.y, mTargetTransform.rotation.z, mTargetTransform.rotation.w };
                info.speakerPosition = info.headPosition;
                info.speakerQuaternion = info.headQuaternion;
                int result = engine.UpdateSpatializerSelfPosition(info);
                if (result != (int)RtcErrorCode.kNERtcNoError)
                {
                    YXUnityLog.LogError(TAG, $"update 3d space sound transform fail code: {result}");
                }
            }
        }

        private void OnDestroy()
        {
            // StopAllCoroutines();
        }
    }
}
