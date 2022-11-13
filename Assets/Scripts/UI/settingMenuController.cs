using Com.LuisPedroFonseca.ProCamera2D;
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
        Screen.fullScreen = toggles[0].isOn;
        SoundManager.Instance.Play(SoundManager.AS.UI, SoundManager.UISound.click2);
    }

    public void OnClickUpscaleToggle()
    {
        Camera.main.GetComponent<ProCamera2DPixelPerfect>().enabled
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
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnClickSettingExitButton()
    {
        gameObject.SetActive(false);
        GameManager.Instance.showFPS = false;
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
