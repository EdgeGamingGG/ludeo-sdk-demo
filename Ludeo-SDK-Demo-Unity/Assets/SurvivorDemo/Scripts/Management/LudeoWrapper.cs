using LudeoSDK;
using System;
using UnityEngine;

public class LudeoWrapper : MonoBehaviour
{
    [SerializeField]
    private string _steamUserId;
    public string SteamUserId
    {
        get => _steamUserId; 
        set
        {
            _steamUserId = value;
            PlayerPrefs.SetString("SteamUserId", _steamUserId);
        }
    }

    //public TMP_InputField SteamUserInput;
    public string APIKey;

    public event Action InitDone;

    public bool CanInit 
    { 
        get 
        {
            if (_isInitialized)
                return false;

            if (string.IsNullOrEmpty(APIKey))
                return false;

            if (string.IsNullOrEmpty(_steamUserId))
                _steamUserId = PlayerPrefs.GetString("SteamUserId", null);

            if (!string.IsNullOrEmpty(_steamUserId))
                return true;

            return false;
        } 
    }

    bool _isInitialized = false;

    public void Init()
    {
        if (_isInitialized) return;
        LudeoManager.Init(_steamUserId, LudeoLauncher.Steam, APIKey, new CallbackWithLudeoFlowState(OnLudeoFlowState));
        _isInitialized = true;
    }

    private void OnLudeoFlowState(LudeoFlowState ludeoFlowState, object data)
    {
        print("<color=red>" + ludeoFlowState.ToString() + "</color>");
        LudeoMessage? msg = null;

        switch (ludeoFlowState)
        {
            case LudeoFlowState.WaitingForUserInteraction:
            case LudeoFlowState.WaitingForReadyForGameplay:
            case LudeoFlowState.WaitingForGetGameplayDefinitions:
            case LudeoFlowState.NewLudeoSelected:
            case LudeoFlowState.GameplayOn:
            case LudeoFlowState.Initialization:
                break;
            case LudeoFlowState.WaitingForLoadGameplayData:
                msg = LudeoManager.LoadGameplayData();
                break;
            case LudeoFlowState.WaitingForSetGameplayDefinitions:
                var definitions = new GameplayDefinitions();

                definitions.AddDefinition("wave", 0);
                definitions.AddDefinition("playerPosition", 0);
                definitions.AddDefinition("playerParameters", null);
                definitions.AddDefinition("enemyPositions", null);
                definitions.AddDefinition("enemyParamters", null);

                InitDone?.Invoke();
                break;
            case LudeoFlowState.WaitingForGameplayBegin:
                LudeoManager.BeginGameplay();
                break;
            case LudeoFlowState.Error:
                Debug.LogError($"Unhandled {ludeoFlowState}");
                break;
            default:
                Debug.LogError($"Unhandled {ludeoFlowState}");
                break;
        }

        print("<color=green>" + (msg.HasValue ? msg.Value.ToString() : "no message") + "</color>");
    }
}
