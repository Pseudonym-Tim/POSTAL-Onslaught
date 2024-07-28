using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Handles everything related to the in-game music...
/// </summary>
public class MusicManager : Singleton<MusicManager>
{
    [SerializeField] private List<MusicTrackInfo> musicDatabase;
    [SerializeField] private AudioMixerGroup masterMixerGroup;
    private AudioSource currentMusicSource;

    private void Awake()
    {
        float savedMasterVolume = PlayerPrefs.GetFloat(OptionsUI.MUSIC_VOLUME_PREF_KEY, 1f);
        UpdateMasterVolume(savedMasterVolume);

        // Setup music tracks...
        foreach(MusicTrackInfo musicTrack in musicDatabase)
        {
            SetupMusicTrack(musicTrack);
        }
    }

    public void UpdateMasterVolume(float volume)
    {
        masterMixerGroup.audioMixer.SetFloat("MusicVolume", volume);
    }

    public void PlayTrack(string trackName)
    {
        MusicTrackInfo trackToPlay = GetMusicTrackInfo(trackName);
        if(trackToPlay == null) { Debug.LogWarning($"Couldn't play music: [{trackName}]! It doesn't exist!"); return; }

        if(currentMusicSource != null && currentMusicSource.isPlaying)
        {
            currentMusicSource.Stop();
        }

        currentMusicSource = trackToPlay.musicAudioSource;
        currentMusicSource.Play();
    }

    public void PlayRandom()
    {
        if(musicDatabase.Count == 0)
        {
            Debug.LogWarning("No music tracks available to play.");
            return;
        }

        MusicTrackInfo randomTrack = musicDatabase[Random.Range(0, musicDatabase.Count)];

        if(currentMusicSource != null && currentMusicSource.isPlaying)
        {
            currentMusicSource.Stop();
        }

        currentMusicSource = randomTrack.musicAudioSource;
        currentMusicSource.Play();
    }

    public void StopMusic()
    {
        if(currentMusicSource != null && currentMusicSource.isPlaying)
        {
            currentMusicSource.Stop();
        }
    }

    public bool IsPlayingMusic()
    {
        return currentMusicSource != null && currentMusicSource.isPlaying;
    }

    private void SetupMusicTrack(MusicTrackInfo musicTrack)
    {
        // We add the audio components to separate objects so we don't lag the editor...
        GameObject newAudioObject = new GameObject($"[{musicTrack.trackName}] Audio Source");
        AudioSource newAudioSource = newAudioObject.AddComponent<AudioSource>();
        newAudioSource.transform.parent = transform;

        // Setup audio source settings...
        musicTrack.musicAudioSource = newAudioSource;
        musicTrack.musicAudioSource.clip = musicTrack.audioClip;
        musicTrack.musicAudioSource.outputAudioMixerGroup = masterMixerGroup;
        musicTrack.musicAudioSource.volume = musicTrack.volume;
        musicTrack.musicAudioSource.loop = true;
    }

    private MusicTrackInfo GetMusicTrackInfo(string trackName)
    {
        return musicDatabase.FirstOrDefault(trackInfo => trackInfo.trackName == trackName);
    }
}

[System.Serializable]
public class MusicTrackInfo
{
    public string trackName = "NewMusicTrack";
    public AudioClip audioClip;
    [Range(0f, 1f)] public float volume = 1f;
    public AudioSource musicAudioSource;
}
