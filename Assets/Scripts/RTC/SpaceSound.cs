using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using nertc;

public class SpaceSound : MonoBehaviour
{
    public GameObject selfGameObject;
    public Text log;

    private IRtcEngine engine;
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

    private void Start()
    {
        engine = IRtcEngine.GetInstance();
    }


    public void Enable3DAudio()
    {
        //open 3d Audio
        engine.EnableSpatializer(true);
        engine.SetSpatializerRenderMode(Audio3DRenderMode);
        engine.UpdateSpatializerAudioRecvRange(AudibleDistance, ConversationalDistance, RollOffModel);

        //audio profile must be stereo,2 channels
        engine.SetAudioProfile(RtcAudioProfileType.kNERtcAudioProfileMiddleQualityStereo, RtcAudioScenarioType.kNERtcAudioScenarioMusic);

        //enable room effect if need
        engine.EnableSpatializerRoomEffects(EnableRoomEffect);
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

        StartCoroutine(UpdateMySelfPosition());
    }

    public IEnumerator UpdateMySelfPosition()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);
            if (selfGameObject != null)
            {
                var info = new RtcSpatializerPositionInfo();
                info.headPosition = new float[3] { selfGameObject.transform.position.x, selfGameObject.transform.position.y, selfGameObject.transform.position.z };
                info.headQuaternion = new float[4] { selfGameObject.transform.rotation.x, selfGameObject.transform.rotation.y, selfGameObject.transform.rotation.z, selfGameObject.transform.rotation.w };
                info.speakerPosition = info.headPosition;
                info.speakerQuaternion = info.headQuaternion;
                int result = engine.UpdateSpatializerSelfPosition(info);
                if (result != (int)RtcErrorCode.kNERtcNoError)
                {

                }
            }
        }

    }
    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}