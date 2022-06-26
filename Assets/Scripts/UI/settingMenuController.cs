using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class settingMenuController : MonoBehaviour
{
    [SerializeField] Toggle[] toggles ;
    [SerializeField] Slider volumeSlider;

    public void OnClickFullScreenToggle()
    {
        Screen.fullScreen = toggles[0].isOn;
    }

    public void OnClickUpscaleToggle()
    {
        Camera.main.GetComponent<UnityEngine.Experimental.Rendering.Universal.PixelPerfectCamera>().upscaleRT 
            = toggles[1].isOn;
    }

    public void OnClickSoundToggle()
    {
        volumeSlider.interactable = toggles[2].isOn;

        if (toggles[2].isOn)
            AudioListener.volume = volumeSlider.value;
        else
            AudioListener.volume = 0f;
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
    }

    public void OnClickExitButton()
    {
        Application.Quit();
    }

    public void OnClickSettingExitButton()
    {
        gameObject.SetActive(false);
    }
}
