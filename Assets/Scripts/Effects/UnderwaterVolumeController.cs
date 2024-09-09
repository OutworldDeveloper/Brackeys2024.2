using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Volume))]
public class UnderwaterVolumeController : MonoBehaviour
{

    private Volume _volume;
    private bool _wasNearEdgeLastFrame;

    private void Awake()
    {
        _volume = GetComponent<Volume>();
    }

    private void Update()
    {
        _volume.weight = (transform.position.y - 0.05f) < Water.Level ? 1 : 0;

        bool isNearEdge = Mathf.Abs(transform.position.y - 0.05f - Water.Level) < 0.2f;

        if (_wasNearEdgeLastFrame == false && isNearEdge == true)
        {
            ScreenFade.FadeOut();
        }

        if (_wasNearEdgeLastFrame == true && isNearEdge == false)
        {
            ScreenFade.FadeIn();
        }

        _wasNearEdgeLastFrame = isNearEdge;
    }

}
