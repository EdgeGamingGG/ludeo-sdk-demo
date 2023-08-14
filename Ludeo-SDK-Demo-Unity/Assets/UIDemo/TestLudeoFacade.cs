using LudeoSDK;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TestLudeoFacade : MonoBehaviour
{
    public string APIKey = "8d9dd7e3-22e8-4037-906d-0bf9b9dab554";
    public string SteamID = "76561198161034664";
    public string TestLudeo = "e80b550d-8f5e-4087-9d67-17722e4e873e";
    public GameplayVariableOvertime[] GameplayVariables;

    private async void Awake()
    {
        // init
        var facade = new LudeoFacade();
        await facade.InitializeWithSteam("76561198161034664", "8d9dd7e3-22e8-4037-906d-0bf9b9dab554");

        // Player
        await facade.Creator.BeginGameplay();
        //await AssimilateGameplayEnvironment(facade);
        await ShortGameplayEnvironment(facade);

        await facade.Creator.EndGameplay();

        // Ludeo
        if (string.IsNullOrEmpty(TestLudeo))
            return;

        await facade.Player.BeginGameplay(TestLudeo);

        foreach (var state in facade.Player.GameState)
        {
            Debug.Log($"Key: {state.Key} Value: {state.Value}");
        }

        while (facade.Player.IsRunningLudeo)
            await Task.Yield();

        await facade.Creator.BeginGameplay();
        await ShortGameplayEnvironment(facade);

        await facade.Creator.EndGameplay();
    }

    private async Task ShortGameplayEnvironment(LudeoFacade facade)
    {
        float count = 0f;

        await Task.Delay(2500);

        facade.SetGameplayVariable("PlayerDeath", false);
        facade.SetGameplayVariable("test1", 1);
        facade.SetGameplayVariable("NormalKill", 10);

        await Task.Delay(1000);

        facade.SetGameplayVariable("NormalKill", 20);
        facade.SetGameplayVariable("test1", 2);
        facade.SetGameplayVariable("PlayerDeath", false);

        await Task.Delay(1000);

        facade.Creator.MarkHIghlight();

        await Task.Delay(1000);
    }

    private async Task AssimilateGameplayEnvironment(LudeoFacade facade)
    {
        float count = 0f;

        await Task.Delay(2500);

        facade.SetGameplayVariable("PlayerDeath", false);
        facade.SetGameplayVariable("test1", 1);
        facade.SetGameplayVariable("NormalKill", 10);

        await Task.Delay(20000);

        facade.SetGameplayVariable("NormalKill", 20);
        facade.SetGameplayVariable("test1", 2);
        facade.SetGameplayVariable("PlayerDeath", false);

        await Task.Delay(2000);

        facade.Creator.MarkHIghlight();

        await Task.Delay(2000);
    }
}

[System.Serializable]
public struct GameplayVariableOvertime
{
    public string Key;
    public GameplayVariableSet[] ValuesOverTime;
}

[System.Serializable]
public struct GameplayVariableSet
{
    public float Time;
    public float Value;
}
