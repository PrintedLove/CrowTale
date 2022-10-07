using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SettingMenuController : MonoBehaviour
{
    [SerializeField] Toggle[] toggles;      // 0: fullscreen, 1: upscale, 2: sound
    [SerializeField] Slider volumeSlider;

    public void OnClickFullScreenToggle()
    {
        if(toggles[0].isOn)
            Screen.SetResolution(Screen.width, Screen.height, true);
        else
            Screen.SetResolution(1920, 1080, false);

        Screen.fullScreen = toggles[0].isOn;
        SoundManager.Instance.Play(SoundManager.AS.UI, SoundManager.UISound.click2);
    }

    public void OnClickUpscaleToggle()
    {
        Camera.main.GetComponent<UnityEngine.Experimental.Rendering.Universal.PixelPerfectCamera>().upscaleRT 
            = toggles[1].isOn;
        SoundManager.Instance.Play(SoundManager.AS.UI, SoundManager.UISound.click2);
    }

    public void OnClickSoundToggle()
    {
        volumeSlider.interactable = toggles[2].isOn;

        if (toggles[2].isOn)
            AudioListener.volume = volumeSlider.value;
        else
            AudioListener.volume = 0f;

        SoundManager.Instance.Play(SoundManager.AS.UI, SoundManager.UISound.click2);
    }

    public void OnClickSoundSlider()
    {
        AudioListener.volume = volumeSlider.value;
    }

    public void OnClickLanguageButton()
    {
        GameManager.Instance.LoadLanguageData(true);
        GameManager.Instance.TranslateManualText();
        GameManager.Instance.TranslateUI();

        SoundManager.Instance.Play(SoundManager.AS.UI, SoundManager.UISound.click2);
    }

    public void OnClickExitButton()
    {
        Application.Quit();
    }

    public void OnClickSettingExitButton()
    {
        gameObject.SetActive(false);
        SoundManager.Instance.Play(SoundManager.AS.UI, SoundManager.UISound.click1);
    }
    
    private void OnEnable()
    {
        GameObject.FindWithTag("Player").GetComponent<PlayerManager>().PlayerStop();
    }

    private void OnDisable()
    {
        GameObject.FindWithTag("Player").GetComponent<PlayerManager>().PlayerPlay();
    }
}
