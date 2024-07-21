using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Handles everything related to audio...
/// </summary>
public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private List<SoundInfo> soundDatabase;
    [SerializeField] private AudioMixerGroup masterMixerGroup;

    private void Awake()
    {
        // Setup sound information...
        foreach(SoundInfo soundInfo in soundDatabase)
        {
            SetupSoundInfo(soundInfo);
        }
    }

    public void Play2DSound(string soundName)
    {
        SoundInfo soundToPlayInfo = GetSoundInfo(soundName);
        if(soundToPlayInfo == null) { Debug.LogWarning($"Couldn't play sound: [{ soundName }]! It doesn't exist!"); return; }
        SetRandomPitch(soundToPlayInfo.soundAudioSource, soundToPlayInfo);
        soundToPlayInfo.soundAudioSource.Play();
    }

    private void SetRandomPitch(AudioSource audioSource, SoundInfo soundInfo)
    {
        float randPitch = Random.Range(soundInfo.pitchRandomMin, soundInfo.pitchRandomMax);
        audioSource.pitch = soundInfo.useRandomPitch ? randPitch : 1;
    }

    public void StopSound(string soundName)
    {
        SoundInfo soundToStop = GetSoundInfo(soundName);
        if(soundToStop == null) { Debug.LogWarning($"Couldn't stop sound: [{ soundName }]"); }
        soundToStop.soundAudioSource.Stop();
    }

    public bool IsPlayingSound(string soundName)
    {
        SoundInfo soundInfo = GetSoundInfo(soundName);
        if(soundInfo.soundAudioSource.isPlaying) { return true; }
        return false;
    }

    public void SetupSoundInfo(SoundInfo soundInfo)
    {
        // We add the audio components to seperate objects so we don't lag the editor...
        GameObject newAudioObject = new GameObject($"[{ soundInfo.soundName }] 2D audio source");
        AudioSource newAudioSource = newAudioObject.AddComponent<AudioSource>();
        newAudioSource.transform.parent = transform;

        // Setup audio source settings...
        soundInfo.soundAudioSource = newAudioSource;
        soundInfo.soundAudioSource.clip = soundInfo.audioClip;
        soundInfo.soundAudioSource.outputAudioMixerGroup = masterMixerGroup;
        soundInfo.soundAudioSource.volume = soundInfo.volume;
        soundInfo.soundAudioSource.pitch = soundInfo.pitch;
        soundInfo.soundAudioSource.loop = soundInfo.loopAudio;
    }

    public SoundInfo GetSoundInfo(string soundName)
    {
        return soundDatabase.FirstOrDefault(soundInfo => soundInfo.soundName == soundName);
    }
}

[System.Serializable]
public class SoundInfo
{
    public string soundName = "NewSound";
    public AudioClip audioClip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    [Range(0.1f, 3f)] public float pitchRandomMin = 1f;
    [Range(0.1f, 3f)] public float pitchRandomMax = 1f;
    public bool useRandomPitch = false;
    public bool loopAudio = false;
    public Spatial3DSoundInfo spatial3DSoundInfo;
    public AudioSource soundAudioSource;

    [System.Serializable]
    public class Spatial3DSoundInfo
    {
        public AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;
        public float minAudibleDistance = 1;
        public float maxAudibleDistance = 5;
    }
}