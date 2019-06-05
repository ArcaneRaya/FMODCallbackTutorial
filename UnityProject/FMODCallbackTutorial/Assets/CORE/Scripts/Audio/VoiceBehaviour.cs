using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System;

public class VoiceBehaviour : MonoBehaviour
{
    [EventRef] public string FMODPathHit;

    private EVENT_CALLBACK voiceCallback;
    private EventInstance eventInstanceVoice;
    private bool isTalking;

    private void Start() {
        voiceCallback = new EVENT_CALLBACK(VoiceEventCallback);
    }

    private void OnMouseDown() {
        PlayVoiceLine(FMODPathHit);
    }

    private void PlayVoiceLine(string fmodPath) {
        // if character is talking, return and do not continue with method
        if (isTalking) {
            return;
        }

        isTalking = true;
        eventInstanceVoice = RuntimeManager.CreateInstance(fmodPath);
        eventInstanceVoice.setCallback(voiceCallback);
        eventInstanceVoice.start();
    }

    [AOT.MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
    private FMOD.RESULT VoiceEventCallback(EVENT_CALLBACK_TYPE callbackType, EventInstance eventInstance, IntPtr parameters) {
        switch (callbackType) {
            case EVENT_CALLBACK_TYPE.START_FAILED:
            case EVENT_CALLBACK_TYPE.STOPPED:
                isTalking = false;
                break;
        }
        return FMOD.RESULT.OK;
    }

    private void OnDestroy() {
        eventInstanceVoice.setCallback(null);
    }
}
