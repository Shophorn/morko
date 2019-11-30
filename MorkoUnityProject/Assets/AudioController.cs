using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioController : MonoBehaviour
{
    public AudioMixer mixer;
    public AudioMixerGroup master;
    public AudioMixerGroup game;
    public AudioMixerGroup ui;
    public AudioMixerGroup music;

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
}
