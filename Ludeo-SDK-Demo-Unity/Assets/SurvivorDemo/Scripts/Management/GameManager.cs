using LudeoSDK;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Asset Referenes")]
    public Player Player;

    [Header("Scene References")]
    public EnemyManager EnemyManager;
    public UpgradeManager UpgradeManager;
    public LudeoWrapper LudeoScripting;
    public Transform PlayerSpawn;
    public CameraFollower CameraFollower;

    [Header("UI References")]
    public SliderExtender HealthBar;
    public GameObject MainMenu;
    public Button Play;
    public TMP_Text Wave;
    public TMP_Text EnemiesLeft;
    public GameObject Loading;
    public Button LudeoHighlight;

    // runtime
    Player _player;

    // level
    int _level = 1;
    bool _upgradesShowing = false;

    private void Awake()
    {
        LudeoHelpers.SetLogLevel(LudeoLogLevel.Error);
        LudeoHighlight.onClick.AddListener(() => 
        {
            var ok = LudeoManager.MarkHighlight();
            print($"Marked highlight: {ok}");
        } );

        LudeoHighlight.gameObject.SetActive(false);
        UpgradeManager.Upgraded += NextLevel;
        LudeoScripting.InitDone += RemoveLoading;
        LudeoScripting.Init();
        Play.onClick.AddListener(StartGame);
    }

    private void RemoveLoading()
    {
        Loading.SetActive(false);
    }

    private void Update()
    {
        if (_player == null || _upgradesShowing)
            return;

        EnemiesLeft.text = $"Enemies Left: {EnemyManager.EnemiesLeft}";

        if (_player.HP <= 0)
        {
            // game over
            EndGame();
            Debug.Log("Game Over");
            return;
        }

        var control = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftControl);
        var l = Input.GetKeyDown(KeyCode.L);
        if (LudeoHighlight.gameObject.activeInHierarchy && control && l)
        {
            var ok = LudeoManager.MarkHighlight();
            print($"<color=red>Marked highlight: {ok}</color>");

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
        LudeoManager.SetGameplayState("PlayerDeath", true);
        LudeoHighlight.gameObject.SetActive(false);
        LudeoManager.EndGameplay();
        _level = 1;
        MainMenu.SetActive(true);
        Time.timeScale = 1;
        Destroy(_player.gameObject);
    }

    public void StartGame()
    {
        LudeoManager.SetGameplayState("PlayerDeath", false);
        LudeoHighlight.gameObject.SetActive(true);
        LudeoManager.ReadyForGameplay();
        
        _level = 1;
        LudeoManager.SetGameplayState("wave", _level);
        MainMenu.SetActive(false);
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
        LudeoManager.SetGameplayState("wave", _level);
        GenerateLevel(_level);
    }

    private void GenerateLevel(int level)
    {
        Wave.text = $"Wave: {level}";

        StartCoroutine(EnemyManager.GenerateEnemies(level, _player.transform));
    }

    private void SpawnPlayer()
    {
        _player = Instantiate(Player);
        _player.transform.position = PlayerSpawn.position;
        _player.Init(EnemyManager);
        HealthBar.Bind(() => _player.HP / (float)_player.MaxHP);
    }
}
