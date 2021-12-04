using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using DG.Tweening;

public class MainMenu : MonoBehaviour
{
    private static bool isShown = false;
    private bool isMusicOn, isSfxOn;
    public Button musicButton, sfxButton;
    public Sprite musicOn, musicOff, sfxOn, sfxOff;
    public GameObject creditsObj;
    public GameObject settingsObj;
    public AudioMixer sfxMixer, musicMixer;

    public RectTransform mainMenu, serverMenu;

    private void Start()
    {
        mainMenu.DOAnchorPos(Vector2.zero, 0.25f);
        creditsObj.gameObject.SetActive(false);
        settingsObj.gameObject.SetActive(false);
    }
    //Transition
    public void swipeRight()
    {
        mainMenu.DOAnchorPos(new Vector2(-2000, 0), 0.25f);
        serverMenu.DOAnchorPos(new Vector2(0, 0), 0.25f);
    }
    public void swipeLeft()
    {
        mainMenu.DOAnchorPos(new Vector2(0, 0), 0.25f);
        serverMenu.DOAnchorPos(new Vector2(2000, 0), 0.25f);
    }
    //Settings
    public void showSettings()
    {
        isShown = !isShown;
        settingsObj.gameObject.SetActive(isShown);
        creditsObj.gameObject.SetActive(false);
    }

    public void setSFX()
    {
        isSfxOn = !isSfxOn;
        switch (isSfxOn)
        {
            case true:
                {
                    sfxMixer.SetFloat("sfxVolume", -80);
                    sfxButton.GetComponent<Image>().sprite = sfxOff;
                    break;
                }
            case false:
                {
                    sfxMixer.SetFloat("sfxVolume", 0);
                    sfxButton.GetComponent<Image>().sprite = sfxOn;
                    break;
                }
        }

    }

    public void showCredits()
    {
        isShown = !isShown;
        creditsObj.gameObject.SetActive(isShown);
        settingsObj.gameObject.SetActive(false);
    }

    public void setMusic()
    {
        isMusicOn = !isMusicOn;
        switch (isMusicOn)
        {
            case true:
                {
                    musicMixer.SetFloat("musicVolume", -80);
                    musicButton.GetComponent<Image>().sprite = musicOff;
                    break;
                }
            case false:
                {
                    musicMixer.SetFloat("musicVolume", 0);
                    musicButton.GetComponent<Image>().sprite = musicOn;
                    break;
                }
        }
    }
}