﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioController : MonoBehaviour, IAudioUIControllable
{
    public AudioMixer mixer;

    [Header("Mixer Channels")]
    public AudioMixerGroup master;
    public AudioMixerGroup game;
    public AudioMixerGroup ui;
    public AudioMixerGroup music;
    [Header("UI Sounds")]
    public AudioClip onSelect;
    public AudioClip[] onClick;
    public AudioSource audioSRC;
    /* Note(Leo): Implementing these explicitly we get a nice
    compiler error if the interface changes anytime */
    void IAudioUIControllable.SetMasterVolume(float value)
    {
        float volume = 0;

        if (value > 0.999f)
        {
            volume = -40f;
            volume += value * 4f;
        }
        master.audioMixer.SetFloat("MasterVolume", volume);
    }
    void IAudioUIControllable.SetMusicVolume(float value)
    {
        float volume = 0;

        if (value > 0.999f)
        {
            volume = -40f;
            volume += value * 4f;
        }
        master.audioMixer.SetFloat("MusicVolume", volume);
    }
    void IAudioUIControllable.SetCharacterVolume(float value) { /* Todo: Add functionality */ }
    void IAudioUIControllable.SetSfxVolume(float value) { /* Todo: Add functionality */ }


    private void Awake()
    {
        
    }
    /// <summary>
    /// volume 0 - 1
    /// </summary>
    /// <param name="volume"></param>
    public void MasterVolume(float volume)
    {
        master.audioMixer.SetFloat("Volume", volume);
    }

    public void MatchAudioFadeIn()
    {
        //KUTSUTAAN FADE TÄÄLTÄ
    }
    public void MatchAudioFadeOut()
    {
        //KUTSUTAAN FADE TÄÄLTÄ
    }

    private IEnumerator Fade(bool up)
    {
        yield return null;
        //TIMER TÄHÄN
    }

    public void MuffleGameAudio(bool doMuffle)
    {
        if (doMuffle)
            game.audioMixer.SetFloat("LowPass", 0.3f);
        else
            game.audioMixer.SetFloat("LowPass", 1f);
    }
    public void PlayButtonSelect()
    {
        Debug.Log("SOUND PLAYED HURRAH");
        audioSRC.PlayOneShot(onSelect);
    }
    public void PlayButtonClick()
    {
        audioSRC.PlayOneShot(onClick[0]);
    }
}
