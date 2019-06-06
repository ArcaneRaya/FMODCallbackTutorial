using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System;
using System.Runtime.InteropServices;

public class VoiceBehaviour : MonoBehaviour
{
    private class VoiceStateInfo {
        public VoiceState State;
    }
    private enum VoiceState {
        Talking,
        Silent
    }

    [EventRef] public string FMODPathHit;

    private EVENT_CALLBACK voiceCallback;
    private EventInstance eventInstanceVoice;
    private VoiceStateInfo voiceInfo;
    private GCHandle voiceInfoHandle;

    private void Start() {
        voiceCallback = new EVENT_CALLBACK(VoiceEventCallback);

        // create and pin voiceinfo in memory
        voiceInfo = new VoiceStateInfo();
        voiceInfo.State = VoiceState.Silent;
        voiceInfoHandle = GCHandle.Alloc(voiceInfo, GCHandleType.Pinned);
    }

    private void OnMouseDown() {
        PlayVoiceLine(FMODPathHit);
    }

    private void PlayVoiceLine(string voiceLinePath) {
        if(voiceInfo.State == VoiceState.Talking) {
            return;
        }

        voiceInfo.State = VoiceState.Talking;
        eventInstanceVoice = RuntimeManager.CreateInstance(voiceLinePath);
        eventInstanceVoice.setUserData(GCHandle.ToIntPtr(voiceInfoHandle));
        eventInstanceVoice.setCallback(voiceCallback);
        eventInstanceVoice.start();
    }

    [AOT.MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
    private FMOD.RESULT VoiceEventCallback(EVENT_CALLBACK_TYPE callbackType, EventInstance eventInstance, IntPtr parameters) {
        IntPtr voiceStatePtr;
        FMOD.RESULT result = eventInstance.getUserData(out voiceStatePtr);

        if(result != FMOD.RESULT.OK) {
            Debug.LogError("FMOD error: " + result);
        }
        else {
            if(voiceStatePtr != IntPtr.Zero) {
                GCHandle voiceInfoHandle = GCHandle.FromIntPtr(voiceStatePtr);
                VoiceStateInfo voiceInfo = (VoiceStateInfo)voiceInfoHandle.Target;

                switch (callbackType) {
                    case EVENT_CALLBACK_TYPE.START_FAILED:
                    case EVENT_CALLBACK_TYPE.STOPPED:
                        eventInstance.setUserData(IntPtr.Zero);
                        eventInstance.setCallback(null);
                        eventInstance.release();
                        voiceInfo.State = VoiceState.Silent;
                        break;
                }
            }
        }

        return FMOD.RESULT.OK;
    }

    private void OnDestroy() {
        if(voiceInfo.State == VoiceState.Talking) {
            eventInstanceVoice.setUserData(IntPtr.Zero);
            eventInstanceVoice.setCallback(null);
            eventInstanceVoice.release();
        }
        voiceInfoHandle.Free();
    }
}
