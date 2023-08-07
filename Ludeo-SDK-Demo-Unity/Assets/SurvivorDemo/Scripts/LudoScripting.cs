using LudeoSDK;
using System;
using UnityEngine;
using static LudeoSDK.LudeoManager;

public class LudoScripting : MonoBehaviour
{
    private void Awake()
    {
        ReadyForGameplay();
        SetGameplayState("", 1);
        
        //Init("", LudeoSDK.LudeoLauncher.Steam, "", new LudeoSDK.CallbackWithLudeoFlowState(OnLudeoFlowState));
        //print(LudeoSDK.LudeoManager.GetLudeoStateInfo().flowState);
    }

    private void OnLudeoFlowState(LudeoFlowState ludeoFlowState, object data)
    {
        throw new NotImplementedException();
    }
}
