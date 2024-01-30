using System;
using _Scripts.Systems;
using UnityEngine;

namespace _Scripts.Managers
{
    public class AudioManager : MonoBehaviour
    {
        public Sound[] sounds;
        public static AudioManager instance;
        void Awake()
        {
            if (instance == null)
            {
                Debug.Log("Setting new AudioManager");
                instance = this;
            }
            else
            {
                Debug.Log("Destroying AudioManager");
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);
            foreach (Sound s in sounds)
            {
                s.source = gameObject.AddComponent<AudioSource>();
                s.source.clip = s.clip;
                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
                s.source.loop = s.loop;
            }
        }

        public void Play(string soundName)
        {
            Sound s = Array.Find(sounds, sound => sound.name == soundName);
            if (s == null)
            {
                Debug.LogWarning("Sound "+soundName+" not found");
                return;
            }
            s.source.Play();
        }
    
        public void StopPlaying(string soundName)
        {
            
            Sound s = Array.Find(sounds, item => item.name == soundName);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + soundName + " not found");
                return;
            }

            s.source.Stop();
        }
    }
}