using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;
using System;

public class UIDemo : MonoBehaviour
{
    [Header("General")]
    public GameObject Loading;

    [Header("Initialization")]
    public GameObject InitializationPanel;
    public TMP_InputField SteamInput;
    public TMP_InputField APIInput;
    public Button InitializeButton;

    [Header("Choose Mode")]
    public GameObject ChooseModePanel;
    public Button ChooseGame;
    public Button ChooseLudeo;

    [Header("Game Mode Configuration")]
    public GameObject GameModePanel;

    LudeoFacade _facade;

    private void Awake()
    {
        _facade = new LudeoFacade();
        InitializationFlow();
    }

    #region Initialization

    private void InitializationFlow()
    {
        InitializationPanel.SetActive(true);

        InitializeButton.onClick.RemoveAllListeners();
        SteamInput.onValueChanged.AddListener(_ => SetInitializeButtonInteractability());
        APIInput.onValueChanged.AddListener(_ => SetInitializeButtonInteractability());
        InitializeButton.onClick.AddListener(async () => await MoveToChooseModeFlow());
    }

    private async Task MoveToChooseModeFlow()
    {
        Loading.SetActive(true);
        await _facade.InitializeWithSteam(SteamInput.text, APIInput.text);
        InitializationPanel.SetActive(false);
        Loading.SetActive(false);
        ChooseModeFlow();
    }

    private void SetInitializeButtonInteractability()
    {
        if (CheckSteamIDValidity(SteamInput.text) && CheckAPIKeyValidity(APIInput.text))
        {
            InitializeButton.gameObject.SetActive(true);
        }
        else { InitializeButton.gameObject.SetActive(false); }
    }

    private bool CheckAPIKeyValidity(string apiKey)
    {
        // TODO: check length and split lengths
        if (string.IsNullOrEmpty(apiKey))
            return false;

        return true;
    }

    private bool CheckSteamIDValidity(string steam)
    {
        if (string.IsNullOrEmpty(steam))
            return false;

        if (steam.Length != 17)
            return false;

        if (!double.TryParse(steam, out double _))
            return false;

        return true;
    }

    #endregion

    #region Choose Mode

    private void ChooseModeFlow()
    {
        InitializationPanel.SetActive(false);
        ChooseModePanel.gameObject.SetActive(true);
    }

    #endregion
}
