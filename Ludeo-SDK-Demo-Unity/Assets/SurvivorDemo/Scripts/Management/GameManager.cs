using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Asset Referenes")]
    public Player Player;

    [Header("Scene References")]
    public EnemyManager EnemyManager;
    public UpgradeManager UpgradeManager;
    public UIManager UIManager;
    public LudeoWrapper LudeoWrapper;
    public Transform PlayerSpawn;
    public CameraFollower CameraFollower;

    // runtime
    Player _player;
    GameObject _loading;

    // level
    int _level = 1;
    bool _upgradesShowing = false;
    bool _ludeoInitialized = false;

    private void Awake()
    {
        UIManager.SetLudeoHighlightOnClick(() =>
        {
            var ok = LudeoSDK.LudeoManager.MarkHighlight();
            print($"Marked highlight: {ok}");
        });
        UIManager.SetPlayButtonOnClick(() => StartCoroutine(StartGame()));
        UIManager.BindEnemiesLeft(() => EnemyManager.EnemiesLeft);
        UIManager.BindHP(() => _player.HP / (float)_player.MaxHP);
        UIManager.SetApplyConfig(s =>
        {
            LudeoWrapper.SteamUserId = s;
            UIManager.SetPlayButtonInteractable(LudeoWrapper.CanInit);
        });

        UpgradeManager.Upgraded += NextLevel;
        LudeoWrapper.InitDone += FinishedLudeoInit;

        UIManager.SetPlayButtonInteractable(LudeoWrapper.CanInit);
    }

    private void InitiazlizeLudeo()
    {
        _loading = Instantiate(UIManager.Loading);
        var guid = "9e954a1a-4e56-4923-9c30-017a56da83ad";
        guid = null;
        LudeoWrapper.Init(guid);
    }

    private void FinishedLudeoInit()
    {
        if (_loading != null)
            Destroy(_loading);

        UIManager.SetPlayButtonInteractable(_ludeoInitialized);
        _ludeoInitialized = true;
    }

    private void Update()
    {
        if (_player == null || _upgradesShowing)
            return;

        if (_player.HP <= 0)
        {
            // game over
            EndGame();
            Debug.Log("Game Over");
            return;
        }

        if (!EnemyManager.AnyEnemyAlive)
        {
            _upgradesShowing = true;
            UpgradeManager.ShowUpgrades();
        }
    }

    private void EndGame()
    {
        UpgradeManager.ResetUpgrades();
        UIManager.MainMenuTransition();
        LudeoSDK.LudeoManager.SetGameplayState("PlayerDeath", true);
        LudeoSDK.LudeoManager.EndGameplay();
        _level = 1;
        Time.timeScale = 1;
        Destroy(_player.gameObject);
    }

    public IEnumerator StartGame()
    {
        if (LudeoWrapper.CanInit)
        {
            InitiazlizeLudeo();
            yield return new WaitUntil(() => _ludeoInitialized);
        }

        LudeoSDK.LudeoManager.SetGameplayState("PlayerDeath", false);
        
        UIManager.GameplayTransition();
        
        _level = 1;
        LudeoSDK.LudeoManager.SetGameplayState(LudeoWrapper.WAVE, _level);
        SpawnPlayer();
        CameraFollower.Init(_player.transform);
        GenerateLevel(_level);
    }

    private void NextLevel(UpgradeDefinition definition)
    {
        if (EnemyManager.AnyEnemyAlive)
            return;

        _upgradesShowing = false;
        _level++;
        LudeoSDK.LudeoManager.SetGameplayState(LudeoWrapper.WAVE, _level);
        GenerateLevel(_level);
    }

    private void GenerateLevel(int level)
    {
        UIManager.SetWave(level);

        StartCoroutine(EnemyManager.GenerateEnemies(level, _player.transform));
    }

    private void SpawnPlayer()
    {
        _player = Instantiate(Player);
        _player.transform.position = PlayerSpawn.position;
        _player.Init(EnemyManager);
    }
}
