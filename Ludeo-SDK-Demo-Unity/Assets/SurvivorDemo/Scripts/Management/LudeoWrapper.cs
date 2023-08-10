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
    public const string NORMAL_KILL = "NormalKill";

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

    public UIView_Logger EditorLog;

    private bool _isLudeo = false;

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

    bool _replayLudeo = false;
    public event Action ReplayLudeo;
    private void OnLudeoFlowState(LudeoFlowState ludeoFlowState, object data)
    {
        print("<color=red>" + ludeoFlowState.ToString() + "</color>");
        LudeoMessage? msg = null;

        switch (ludeoFlowState)
        {
            case LudeoFlowState.GameplayAborted:
            case LudeoFlowState.LoadingGameplayData:
            case LudeoFlowState.NewLudeoSelected:
            case LudeoFlowState.Initialization:
                break;
            case LudeoFlowState.WaitingForLoadGameplayData:
                _replayLudeo = false;
                if (string.IsNullOrEmpty(_guid))
                {
                    msg = LudeoManager.LoadGameplayData();
                    _isLudeo = false;
                }
                else
                {
                    msg = LudeoManager.LoadGameplayData(new Guid(_guid));
                    _isLudeo = true;
                }

                break;
            case LudeoFlowState.WaitingForReadyForGameplay:
                LudeoManager.ReadyForGameplay();
                break;
            case LudeoFlowState.WaitingForSetGameplayDefinitions:
                var definitions = new GameplayDefinitions();

                LudeoManager.SetGameplayDefinitions(definitions);

                break;
            case LudeoFlowState.Error:
                Debug.LogError($"Unhandled {ludeoFlowState}");
                break;


            // PLAYER FLOW
            case LudeoFlowState.WaitingForGetGameplayDefinitions:

                LudeoManager.GetGameplayDefinitionsKeys(LudeoParam.Int, out string[] def1);

                LudeoManager.GetGameplayStateKeys(LudeoParam.Vec3, out string[] keys1);
                LudeoManager.GetGameplayStateKeys(LudeoParam.Int, out string[] keys2);
                LudeoManager.GetGameplayStateKeys(LudeoParam.String, out string[] keys3);
                LudeoManager.GetGameplayStateKeys(LudeoParam.Objects, out string[] keys4);
                LudeoManager.GetGameplayStateKeys(LudeoParam.Float, out string[] keys5);
                LudeoManager.GetGameplayStateKeys(LudeoParam.Bool, out string[] keys6);
                LudeoManager.GetGameplayStateKeys(LudeoParam.Quatern, out string[] keys7);

                LogKeys(keys1);
                LogKeys(keys2);
                LogKeys(keys3);
                LogKeys(keys4);
                LogKeys(keys5);
                LogKeys(keys6);
                LogKeys(keys7);

                LudeoManager.ReadyForGameplay();

                break;
            case LudeoFlowState.WaitingForUserInteraction:
                break;
            case LudeoFlowState.GameplayOn:
                break;
            case LudeoFlowState.WaitingForGameplayBegin:
                if (_isLudeo)
                {
                    if (_replayLudeo)
                        ReplayLudeo?.Invoke();

                    _isInitialized = true;

                    _replayLudeo = true;
                }

                InitDone?.Invoke();
                break;
            default:
                Debug.LogError($"Unhandled {ludeoFlowState}");
                break;
        }

        print("<color=green>" + (msg.HasValue ? msg.Value.ToString() : "no message") + "</color>");
    }

    private void LogKeys(string[] keys)
    {
        if (EditorLog == null)
            return;

        if (keys == null || keys.Length == 0)
            return;

        foreach (var key in keys)
        {
            var msg = key + ": ";
            if (LudeoManager.GetGameplayState(key, out bool val1) == LudeoMessage.Success)
            {
                msg += val1;
            }
            else if (LudeoManager.GetGameplayState(key, out float val2) == LudeoMessage.Success)
            {
                msg += val2;
            }
            else if (LudeoManager.GetGameplayState(key, out int val3) == LudeoMessage.Success)
            {
                msg += val3;
            }
            else if (LudeoManager.GetGameplayState(key, out Quatern val4) == LudeoMessage.Success)
            {
                msg += val4;
            }
            else if (LudeoManager.GetGameplayState(key, out Vec3 val5) == LudeoMessage.Success)
            {
                msg += val5;
            }
            EditorLog.QueueLog(msg);
        }
    }
}
