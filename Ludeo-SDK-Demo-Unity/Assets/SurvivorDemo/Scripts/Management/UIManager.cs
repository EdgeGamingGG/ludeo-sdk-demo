using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Asset Referenes")]
    public GameObject Loading;

    [Header("UI References")]
    [Header("Main")]
    public GameObject MainPanel;
    public GameObject ConfigPanel;
    public Button Play;
    public Button Config;
    public Button ApplyConfig;
    public TMP_InputField SteamIDInput;
    public TMP_Text SteamIDText;

    [Header("Gameplay")]
    public GameObject GameplayPanel;
    public SliderExtender HealthBar;
    public TMP_Text Wave;
    public TMP_Text EnemiesLeft;
    public Button LudeoHighlight;

    private Func<int> _enemiesLeft;
    bool _rerun = false;

    private void Awake()
    {
        MainMenuTransition();

        Config.onClick.AddListener(() => ConfigTransition());
        ApplyConfig.onClick.AddListener(() => MainMenuTransition());
        ApplyConfig.onClick.AddListener(() => SetSteamIDText());
        SetSteamIDText();
    }

    private void SetSteamIDText()
    {
        var id = PlayerPrefs.GetString("SteamUserId", "");
        SteamIDText.text = string.IsNullOrEmpty(id) ? "No steam id was set in configuration" : $"Steam ID: {id}";
    }

    public void MainMenuTransition()
    {
        GameplayPanel.SetActive(false);
        ConfigPanel.SetActive(false);
        MainPanel.SetActive(true);

        if (_rerun)
        {
            Config.interactable = false;
        }
    }

    public void GameplayTransition()
    {
        MainPanel.SetActive(false);
        ConfigPanel.SetActive(false);
        GameplayPanel.SetActive(true);

        _rerun = true;
    }

    public void ConfigTransition()
    {
        MainPanel.SetActive(false);
        ConfigPanel.SetActive(true);
        GameplayPanel.SetActive(false);

        SteamIDInput.text = PlayerPrefs.GetString("SteamUserId", "");
    }

    public void BindEnemiesLeft(Func<int> bind)
    {
        _enemiesLeft = bind;
    }

    public void BindHP(Func<float> bind)
    {
        HealthBar.Bind(bind);
    }

    public void SetApplyConfig(Action<string> onClick)
    {
        ApplyConfig.onClick.RemoveAllListeners();
        ApplyConfig.onClick.AddListener(() => onClick(SteamIDInput.text));
    }

    public void SetLudeoHighlightOnClick(Action onclick)
    {
        LudeoHighlight.onClick.AddListener(() => onclick());
    }

    public void SetPlayButtonOnClick(Action onClick)
    {
        Play.onClick.RemoveAllListeners();
        Play.onClick.AddListener(() => onClick());
    }
    
    public void SetPlayButtonInteractable(bool interactable)
    {
        Play.interactable = interactable;
    }

    private void Update()
    {
        if (EnemiesLeft.gameObject.activeInHierarchy)
        {
            EnemiesLeft.text = $"Enemies Left: {_enemiesLeft()}";
        }

        if (LudeoHighlight.gameObject.activeInHierarchy)
        {
            var control = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftControl);
            var l = Input.GetKeyDown(KeyCode.L);
            if (LudeoHighlight.gameObject.activeInHierarchy && control && l)
            {
                var ok = LudeoSDK.LudeoManager.MarkHighlight();
                print($"<color=red>Marked highlight: {ok}</color>");

                return;
            }
        }
    }

    internal void SetWave(int level)
    {
        if (Wave.gameObject.activeInHierarchy)
        {
            Wave.text = $"Wave: {level}";
        }
    }
}
