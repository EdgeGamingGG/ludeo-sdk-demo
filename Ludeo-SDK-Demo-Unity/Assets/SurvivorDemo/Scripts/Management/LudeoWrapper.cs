using LudeoSDK;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class LudeoWrapper : MonoBehaviour
{
    public string SteamUserId;
    public string APIKey;

    public event Action InitDone;
    public void Init()
    {
        LudeoManager.Init(SteamUserId, LudeoLauncher.Steam, APIKey, new CallbackWithLudeoFlowState(OnLudeoFlowState));
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
