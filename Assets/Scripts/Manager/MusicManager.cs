using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

/// <summary>
/// Handles everything related to the in-game music...
/// </summary>
public class MusicManager : Singleton<MusicManager>
{
    private string DATABASE_PATH = "Music/music_database.json";
    [SerializeField] private List<MusicTrackInfo> musicDatabase;
    [SerializeField] private AudioMixerGroup masterMixerGroup;
    private AudioSource currentMusicSource;

    private void Awake()
    {
        LoadMusicTracks();
        float savedMasterVolume = PlayerPrefs.GetFloat(OptionsUI.MUSIC_VOLUME_PREF_KEY, 1f);
        UpdateMasterVolume(savedMasterVolume);
    }

    private void LoadMusicTracks()
    {
        musicDatabase = new List<MusicTrackInfo>();

        string path = Path.Combine(Application.streamingAssetsPath, DATABASE_PATH);

        if(File.Exists(path))
        {
            string json = File.ReadAllText(path);
            JArray tracksArray = JArray.Parse(json);

            foreach(JObject trackObject in tracksArray)
            {
                string trackName = trackObject.Value<string>("trackName");
                string trackId = trackObject.Value<string>("trackID");

                MusicTrackInfo track = new MusicTrackInfo
                {
                    trackName = trackName,
                    trackID = trackId,
                    audioClip = LoadStreamingAudio(trackName)
                };

                SetupMusicTrack(track);
                musicDatabase.Add(track);
            }
        }
        else
        {
            Debug.LogError($"Music JSON file not found at path: {path}");
        }
    }

    private AudioClip LoadStreamingAudio(string trackName)
    {
        string audioPath = Path.Combine(Application.streamingAssetsPath, "Music", $"{trackName}.ogg");

        // Debugging
        Debug.Log($"Loading audio clip from path: {audioPath}");
        if(!File.Exists(audioPath))
        {
            Debug.LogError($"File does not exist at path: {audioPath}");
            return null;
        }

        using(UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(audioPath, AudioType.OGGVORBIS))
        {
            www.SendWebRequest();
            while(!www.isDone) { }

            if(www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error loading audio clip: {www.error}");
                return null;
            }
            else
            {
                return DownloadHandlerAudioClip.GetContent(www);
            }
        }
    }


    public void UpdateMasterVolume(float volume)
    {
        masterMixerGroup.audioMixer.SetFloat("MusicVolume", volume);
    }

    public void Pause() => currentMusicSource?.Pause();
    public void Resume() => currentMusicSource?.Play();

    public void PlayTrack(string trackID)
    {
        MusicTrackInfo trackToPlay = GetMusicTrackInfo(trackID);

        if(trackToPlay == null)
        {
            Debug.LogWarning($"Couldn't play music: [{trackID}]! It doesn't exist!");
            return;
        }

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

        // Filter out the track with the "main_menu" id...
        List<MusicTrackInfo> filteredTracks = musicDatabase.Where(track => track.trackID != "main_menu").ToList();

        // Check if there are any tracks left after filtering...
        if(filteredTracks.Count == 0)
        {
            Debug.LogWarning("No music tracks available to play after filtering...");
            return;
        }

        // Select a random track from the filtered list...
        MusicTrackInfo randomTrack = filteredTracks[Random.Range(0, filteredTracks.Count)];

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
        GameObject newAudioObject = new GameObject($"[{musicTrack.trackName}] Audio Source");
        AudioSource newAudioSource = newAudioObject.AddComponent<AudioSource>();
        newAudioSource.transform.parent = transform;

        musicTrack.musicAudioSource = newAudioSource;
        musicTrack.musicAudioSource.clip = musicTrack.audioClip;
        musicTrack.musicAudioSource.outputAudioMixerGroup = masterMixerGroup;
        musicTrack.musicAudioSource.volume = musicTrack.volume;
        musicTrack.musicAudioSource.loop = true;
    }

    private MusicTrackInfo GetMusicTrackInfo(string trackID)
    {
        return musicDatabase.FirstOrDefault(trackInfo => trackInfo.trackID == trackID);
    }
}

[System.Serializable]
public class MusicTrackInfo
{
    public string trackName = "NewMusicTrack";
    public string trackID = "newMusicTrack";
    public AudioClip audioClip;
    [Range(0f, 1f)] public float volume = 1f;
    public AudioSource musicAudioSource;
}