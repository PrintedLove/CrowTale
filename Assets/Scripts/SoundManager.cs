using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class soundManager : MonoBehaviour
{
    public enum AS
    {
        UI,
        playerAction,
        playerAction2,
        playerInteract
    }

    public enum BGM
    {
        JourneysReflection, 
        Bittersweet
    }

    public string[] BGM_description =
    {
        "Journey's Reflection (Darren Curtis)",
        "Bittersweet (SYBS)"
    };

    public enum PlayerAction
    {
        walk1, walk2, walk3, walk4, walk5, walk6, flyStart,
        attack1, attack2, attack3,
        roll
    }

    [SerializeField] AudioSource audioSource_BGM;
    [SerializeField] AudioSource[] audioSources;
    [SerializeField] AudioClip[] audioClip_BGM;
    [SerializeField] AudioClip[] audioClip_playerAction;

    [Header("Others")]
    [SerializeField] GameObject BGMDescription;

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public void Play(AS ASname, string ACname)
    {
        AudioSource audioSource = audioSources[(int)ASname];

        audioSource.clip = audioClip_playerAction[(int)(PlayerAction)System.Enum.Parse(typeof(PlayerAction), ACname)];
        audioSource.Play();
    }

    public void Play(AS ASname, PlayerAction ACname)
    {
        AudioSource audioSource = audioSources[(int)ASname];

        audioSource.clip = audioClip_playerAction[(int)ACname];
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
