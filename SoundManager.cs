using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace OnefallGames
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [System.Serializable]
        public class Sound
        {
            public AudioClip clip;
            [HideInInspector]
            public int simultaneousPlayCount = 0;
        }

        public Sound button;
        public Sound fire;
        public Sound explode;


        public delegate void OnMuteStatusChanged(bool isMuted);

        public static event OnMuteStatusChanged MuteStatusChanged;

        public delegate void OnMusicStatusChanged(bool isOn);

        public static event OnMusicStatusChanged MusicStatusChanged;

        enum PlayingState
        {
            Playing,
            Paused,
            Stopped
        }

        public AudioSource AudioSource
        {
            get
            {
                if (audioSource == null)
                {
                    audioSource = GetComponent<AudioSource>();
                }

                return audioSource;
            }
        }

        private AudioSource audioSource;
        private PlayingState musicState = PlayingState.Stopped;
        private const string MUTE_PREF_KEY = "MutePreference";
        private const int MUTED = 1;
        private const int UN_MUTED = 0;
        private const string MUSIC_PREF_KEY = "MusicPreference";
        private const int MUSIC_OFF = 0;
        private const int MUSIC_ON = 1;

        void Awake()
        {
            if (Instance)
            {
                DestroyImmediate(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        void Start()
        {
            SetMute(IsMuted());
        }

        public void PlaySound(Sound sound, bool autoScaleVolume = true, float maxVolumeScale = 1f)
        {
            StartCoroutine(CRPlaySound(sound, autoScaleVolume, maxVolumeScale));
        }

        IEnumerator CRPlaySound(Sound sound, bool autoScaleVolume = true, float maxVolumeScale = 1f)
        {
            if (sound.simultaneousPlayCount >= 7)
            {
                yield break;
            }

            sound.simultaneousPlayCount++;

            float vol = maxVolumeScale;

            if (autoScaleVolume && sound.simultaneousPlayCount > 0)
            {
                vol = vol / (float)(sound.simultaneousPlayCount);
            }

            AudioSource.PlayOneShot(sound.clip, vol);

            float delay = sound.clip.length * 0.7f;

            yield return new WaitForSeconds(delay);

            sound.simultaneousPlayCount--;
        }

        public void PlayMusic(Sound music, bool loop = true)
        {
            if (IsMusicOff())
            {
                return;
            }

            AudioSource.clip = music.clip;
            AudioSource.loop = loop;
            AudioSource.Play();
            musicState = PlayingState.Playing;
        }

        public void PauseMusic()
        {
            if (musicState == PlayingState.Playing)
            {
                AudioSource.Pause();
                musicState = PlayingState.Paused;
            }    
        }

        public void ResumeMusic()
        {
            if (musicState == PlayingState.Paused)
            {
                AudioSource.UnPause();
                musicState = PlayingState.Playing;
            }
        }

        public void StopMusic()
        {
            AudioSource.Stop();
            musicState = PlayingState.Stopped;
        }

        public bool IsMuted()
        {
            return (PlayerPrefs.GetInt(MUTE_PREF_KEY, UN_MUTED) == MUTED);
        }

        public bool IsMusicOff()
        {
            return (PlayerPrefs.GetInt(MUSIC_PREF_KEY, MUSIC_ON) == MUSIC_OFF);
        }

        public void ToggleMute()
        {
            bool mute = !IsMuted();

            if (mute)
            {
                PlayerPrefs.SetInt(MUTE_PREF_KEY, MUTED);

                if (MuteStatusChanged != null)
                {
                    MuteStatusChanged(true);
                }
            }
            else
            {
                PlayerPrefs.SetInt(MUTE_PREF_KEY, UN_MUTED);

                if (MuteStatusChanged != null)
                {
                    MuteStatusChanged(false);
                }
            }

            SetMute(mute);
        }

        public void ToggleMusic()
        {
            if (IsMusicOff())
            {
                PlayerPrefs.SetInt(MUSIC_PREF_KEY, MUSIC_ON);
                if (musicState == PlayingState.Paused)
                {
                    ResumeMusic();
                }

                if (MusicStatusChanged != null)
                {
                    MusicStatusChanged(true);
                }
            }
            else
            {
                PlayerPrefs.SetInt(MUSIC_PREF_KEY, MUSIC_OFF);
                if (musicState == PlayingState.Playing)
                {
                    PauseMusic();
                }

                if (MusicStatusChanged != null)
                {
                    MusicStatusChanged(false);
                }
            }
        }
        void SetMute(bool isMuted)
        {
            AudioSource.mute = isMuted;
        }
    }
}
