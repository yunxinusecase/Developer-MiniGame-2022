using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotate : MonoBehaviour
{
    public float mSpeed = 10.0f;
    public Transform mTarget;

    void Update()
    {
        mTarget?.Rotate(0f, 0f, Time.deltaTime * mSpeed);
    }
}
