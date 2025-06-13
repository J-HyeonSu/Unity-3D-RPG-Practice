using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RpgPractice
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager instance;

        [Header("BGM")] 
        public AudioClip bgmClip;
        public float bgmVolume;
        private AudioSource bgmPlayer;
        private AudioHighPassFilter bgmEffect;
        
        [Header("SFX")] 
        public AudioClip[] sfxClips;
        public float sfxVolume;
        public int channels;
        private AudioSource[] sfxPlayers;
        private int channelIndex;
        
        public enum Sfx{Dead, Hit, LevelUp=3, Lose, Melee, Range=7, Select, Win}
        
        private void Awake()
        {
            instance = this;
            Init();
        }
        
        public void Init()
        {
            // bgm
            GameObject bgmObejct = new GameObject("BgmPlayer");
            bgmObejct.transform.parent = transform;
            bgmPlayer = bgmObejct.AddComponent<AudioSource>();
            bgmPlayer.playOnAwake = false;
            bgmPlayer.loop = true;
            bgmPlayer.volume = bgmVolume;
            bgmPlayer.clip = bgmClip;
            bgmEffect = Camera.main.GetComponent<AudioHighPassFilter>();
        
            GameObject sfxObejct = new GameObject("SfxPlayer");
            sfxObejct.transform.parent = transform;
            sfxPlayers = new AudioSource[channels];

            for (int index = 0; index < sfxPlayers.Length; index++)
            {
                sfxPlayers[index] = sfxObejct.AddComponent<AudioSource>();
                sfxPlayers[index].playOnAwake = false;
                sfxPlayers[index].bypassListenerEffects = true;
                sfxPlayers[index].volume = sfxVolume;
            }


        }

        public void PlayBgm(bool isPlay)
        {
            if (isPlay)
            {
                bgmPlayer.Play();
            }
            else
            {
                bgmPlayer.Stop();
            }
        }
    
        public void EffectBgm(bool isPlay)
        {
            bgmEffect.enabled = isPlay;
        }

        public void PlaySfx(Sfx sfx)
        {
            for (int index = 0; index < sfxPlayers.Length; index++)
            {
                //전채채널인덱스 0+16 % 16
                int loopIndex = (index + channelIndex) % sfxPlayers.Length;

                if (sfxPlayers[loopIndex].isPlaying)
                {
                    continue;
                }

                int ranIndex = 0;
                if (sfx == Sfx.Hit || sfx == Sfx.Melee)
                {
                    ranIndex = Random.Range(0, 1);
                }

                channelIndex = loopIndex;
                sfxPlayers[loopIndex].clip = sfxClips[ranIndex];
                sfxPlayers[loopIndex].Play();
                break;
            }
        
        }
        
        


    }
}
