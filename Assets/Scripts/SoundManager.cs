using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public string[] BGMdescription =
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

    private AudioSource audioSource_BGM;
    [SerializeField] AudioSource[] audioSources;
    [SerializeField] GameObject BGMDescription;

    [SerializeField] AudioClip[] audioClip_BGM;
    [SerializeField] AudioClip[] audioClip_playerAction;

    private void Awake()
    {
        audioSource_BGM = Camera.main.GetComponent<AudioSource>();
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public void Play(AS ASname, string ACname)
    {
        AudioSource audioSource = audioSources[(int)ASname];
        audioSource.clip = audioClip_playerAction[(int)(PlayerAction)System.Enum.Parse(typeof(PlayerAction), ACname)];
        audioSource.Play();
    }

    public void Play(int PlayerASNum, PlayerAction ACname)
    {
        AudioSource audioSource;

        if (PlayerASNum == 1)
            audioSource = audioSources[(int)AS.playerAction];
        else
            audioSource = audioSources[(int)AS.playerAction2];

        audioSource.clip = audioClip_playerAction[(int)ACname];
        audioSource.Play();
    }

    //Change background music
    public void changeBGM(BGM name)
    {
        BGMDescription.SetActive(true);
        BGMDescription.GetComponent<Text>().text = "BGM - " + BGMdescription[(int)name];
        BGMDescription.GetComponent<Animator>().SetTrigger("show");

        StartCoroutine(RunBGMFade(audioClip_BGM[(int)name]));
    }

    //BGM fade coroutine
    public IEnumerator RunBGMFade(AudioClip audioClip)
    {
        int channeling = 1;

        while (channeling < 750)
        {
            channeling += 1;

            if (channeling < 175)
                audioSource_BGM.volume -= 0.01f;
            else if (channeling == 175)
            {
                audioSource_BGM.clip = audioClip;
                audioSource_BGM.Play();
            }
            else
                if (audioSource_BGM.volume < 1) audioSource_BGM.volume += 0.01f;

            yield return new WaitForSeconds(0.01f);
        }

        BGMDescription.GetComponent<Animator>().ResetTrigger("show");
        BGMDescription.SetActive(false);
    }
}
