using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get { return _instance; } }
    private static SoundManager _instance;

    public enum AS
    {
        UI,
        playerAction1, playerAction2, playerAction3,
        playerInteract
    }

    public enum BGM
    {
        JourneysReflection, 
        Bittersweet,
        TheFutureAncientNow,
        TheWitch
}

    public string[] BGM_description =
    {
    };

    public enum PlayerAction
    {
        walk1, walk2, walk3, walk4, walk5, walk6, flyStart,
        attack1, attack2, attack3,
        roll,
        damage1, damage2, damage3
    }

    public enum UISound
    {
        click1, click2,
        die, itemGet, savePoint
    }

    [SerializeField] AudioSource audioSource_BGM;
    [SerializeField] AudioSource[] audioSources;
    [SerializeField] AudioClip[] audioClip_BGM;
    [SerializeField] AudioClip[] audioClip_playerAction;
    [SerializeField] AudioClip[] audioClip_UI;

    [Header("Others")]
    [SerializeField] GameObject BGMDescription;

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Play(AS ASname, string playerACname)
    {
        AudioSource audioSource = audioSources[(int)ASname];

        audioSource.clip = audioClip_playerAction[(int)(PlayerAction)System.Enum.Parse(typeof(PlayerAction), playerACname)];
        audioSource.Play();
    }

    public void Play(AS ASname, PlayerAction playerACname)
    {
        AudioSource audioSource = audioSources[(int)ASname];

        audioSource.clip = audioClip_playerAction[(int)playerACname];
        audioSource.Play();
    }

    public void Play(AS ASname, UISound UIACname)
    {
        AudioSource audioSource = audioSources[(int)ASname];

        audioSource.clip = audioClip_UI[(int)UIACname];
        audioSource.Play();
    }

    //Change background music
    public void ChangeBGM(BGM name, float maxVolume)
    {
        BGMDescription.SetActive(true);
        BGMDescription.GetComponent<Text>().text = "BGM - " + BGM_description[(int)name];
        BGMDescription.GetComponent<Animator>().SetTrigger("show");

        StartCoroutine(RunBGMFade(audioClip_BGM[(int)name], maxVolume));
    }

    //BGM fade coroutine
    public IEnumerator RunBGMFade(AudioClip audioClip, float maxVolume)
    {
        int channeling = 1;
        float IncreaseVal = maxVolume / 100;

        while (channeling < 750)
        {
            channeling += 1;

            if (channeling < 200)
                audioSource_BGM.volume -= IncreaseVal;
            else if (channeling == 200)
            {
                audioSource_BGM.clip = audioClip;
                audioSource_BGM.Play();
            }
            else
                if (audioSource_BGM.volume < maxVolume) audioSource_BGM.volume += IncreaseVal;

            yield return new WaitForSeconds(0.01f);
        }

        BGMDescription.GetComponent<Animator>().ResetTrigger("show");
        BGMDescription.SetActive(false);
    }
}
