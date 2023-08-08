using LudeoSDK;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class LudeoScripting : MonoBehaviour
{
    public string SteamUserId;
    public string APIKey;

    private bool _initDone = false;

    public event Action InitDone;
    public void Init()
    {
        _initDone = false;
        LudeoManager.Init(SteamUserId, LudeoLauncher.Steam, APIKey, new CallbackWithLudeoFlowState(OnLudeoFlowState));
    }

    public void BeginGameplay() => LudeoManager.BeginGameplay();

    public void EndGameplay() => LudeoManager.EndGameplay();

    private void OnLudeoFlowState(LudeoFlowState ludeoFlowState, object data)
    {
        print("<color=red>" + ludeoFlowState.ToString() +  "</color>");
        LudeoMessage? msg = null;

        switch (ludeoFlowState)
        {
            case LudeoFlowState.NewLudeoSelected:
                // Handle a new Ludeo selection
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

                //msg = LudeoManager.SetGameplayDefinitions(definitions);
                //if (msg == LudeoMessage.Success)
                //{
                //    // msg = ReadyForGameplay();
                //    msg = LudeoManager.BeginGameplay();
                //}
                _initDone = true;
                InitDone?.Invoke();
                break;
            case LudeoFlowState.WaitingForGetGameplayDefinitions:
                // Code for WaitingForGetGameplayDefinitions
                break;
            case LudeoFlowState.WaitingForReadyForGameplay:
                break;
            case LudeoFlowState.WaitingForGameplayBegin:
                BeginGameplay();
                break;
            case LudeoFlowState.WaitingForUserInteraction:
                // Code for WaitingForUserInteraciton
                break;
            case LudeoFlowState.Error:
                // Code for Error
                break;

            default:
                Debug.LogWarning($"Unhandled {ludeoFlowState}");
                break;
        }

        print(ludeoFlowState.ToString() + ", <color=green>" + (msg.HasValue ? msg.Value.ToString() : "no message") + "</color>");
    }
}
