using LudeoSDK;
using System;
using UnityEditor;
using UnityEngine;

public class LudeoWrapper : MonoBehaviour
{
    // Level
    public const string WAVE = "Wave";
    public const string TIMESCALE = "TimeScale";
    public const string PLAYER_POSITION = "Player_Position";

    // Player
    public const string PLAYER_HP = "Player_HP";
    public const string PLAYER_MAXHP = "Player_MaxHP";
    public const string PLAYER_DEATH = "PlayerDeath";

    // Abilities
    public const string PLAYER_ABILITY_COUNT = "Player_Ability_Count";
    public const string ABILITY_COOLDOWN = "Ability_Cooldown";
    public const string ABILITY_DAMAGE = "Ability_Damage";

    // Enemies
    public const string ENEMY_COUNT = "Enemy_Count";
    public const string ENEMY_HP = "Enemy_HP";
    public const string ENEMY_MAXHP = "Enemy_MaxHP";
    public const string ENEMY_POSITION = "Enemy_Position";
    public const string ENEMY_DAMAGE = "Enemy_Damage";
    public const string ENEMY_SPEED = "Enemy_Speed";

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
    string _guid = null;

    public void Init(string guid)
    {
        if (_isInitialized) return;

        _guid = guid;
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
            case LudeoFlowState.LoadingGameplayData:
            case LudeoFlowState.NewLudeoSelected:
            case LudeoFlowState.Initialization:
                break;
            case LudeoFlowState.WaitingForLoadGameplayData:
                if (string.IsNullOrEmpty(_guid))
                    msg = LudeoManager.LoadGameplayData();
                else
                    msg = LudeoManager.LoadGameplayData(new Guid(_guid));
                break;
            case LudeoFlowState.WaitingForReadyForGameplay:
                LudeoSDK.LudeoManager.ReadyForGameplay();
                break;
            case LudeoFlowState.WaitingForSetGameplayDefinitions:
                var definitions = new GameplayDefinitions();
                
                LudeoManager.SetGameplayDefinitions(definitions);

                InitDone?.Invoke();
                break;
            case LudeoFlowState.WaitingForGameplayBegin:
                LudeoManager.BeginGameplay();
                break;
            case LudeoFlowState.Error:
                Debug.LogError($"Unhandled {ludeoFlowState}");
                break;
            // PLAYER FLOW
            case LudeoFlowState.WaitingForGetGameplayDefinitions:
                LudeoManager.GetGameplayStateKeys(LudeoParam.Vec3, out string[] keys1);
                LudeoManager.GetGameplayStateKeys(LudeoParam.Int, out string[] keys2);
                LudeoManager.GetGameplayStateKeys(LudeoParam.String, out string[] keys3);
                LudeoManager.GetGameplayStateKeys(LudeoParam.Objects, out string[] keys4);
                LudeoManager.GetGameplayStateKeys(LudeoParam.Float, out string[] keys5);
                LudeoManager.GetGameplayStateKeys(LudeoParam.Bool, out string[] keys6);
                LudeoManager.GetGameplayStateKeys(LudeoParam.Quatern, out string[] keys7);

                LudeoManager.ReadyForGameplay();

                break;
            case LudeoFlowState.GameplayOn:
                _isInitialized = true;
                InitDone?.Invoke();
                break;
            default:
                Debug.LogError($"Unhandled {ludeoFlowState}");
                break;
        }

        print("<color=green>" + (msg.HasValue ? msg.Value.ToString() : "no message") + "</color>");
    }
}
