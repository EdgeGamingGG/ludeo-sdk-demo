using LudeoSDK;
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
    private string _guid = null;

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
        UIManager.SetPlayLudeo(s =>
        {
            _guid = s;
            StartCoroutine(StartGame());
        }
        );
    }

    private void InitializeLudeo(string guid)
    {
        _guid = guid;
        UIManager.SetPlayLudeoInteractable(false);
        _loading = Instantiate(UIManager.Loading);
        LudeoWrapper.Init(_guid);
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
        LudeoSDK.LudeoManager.SetGameplayState
            (LudeoWrapper.PLAYER_DEATH, true);
        LudeoSDK.LudeoManager.EndGameplay();
        _level = 1;
        Time.timeScale = 1;
        Destroy(_player.gameObject);
    }

    public IEnumerator StartGame()
    {
        if (LudeoWrapper.CanInit)
        {
            InitializeLudeo(_guid);
            yield return new WaitUntil(() => _ludeoInitialized);
        }

        LudeoManager.SetGameplayState(LudeoWrapper.PLAYER_DEATH, false);
        LudeoManager.SetGameplayState(LudeoWrapper.WAVE, _level);

        UIManager.GameplayTransition();

        SpawnPlayer();
        CameraFollower.Init(_player.transform);
        _level = 1;

        // if playing a ludeo...
        if (!string.IsNullOrEmpty(_guid))
        {
            InitializeLudeo();
        }

        GenerateLevel(_level);
    }

    private void InitializeLudeo()
    {
        LudeoManager.GetGameplayState(LudeoWrapper.WAVE, out _level);
        var ts = Time.timeScale;
        LudeoManager.GetGameplayState(LudeoWrapper.TIMESCALE, out ts);
        Time.timeScale = ts;
        LudeoManager.GetGameplayState(LudeoWrapper.PLAYER_POSITION, out Vec3 pos);
        _player.transform.position = new Vector3(pos.x, pos.y, pos.z);
        LudeoManager.GetGameplayState(LudeoWrapper.PLAYER_MAXHP, out int maxhp);
        _player.MaxHP = maxhp;
        LudeoManager.GetGameplayState(LudeoWrapper.PLAYER_HP, out int hp);
        _player.HP = hp;
    }

    private void NextLevel(UpgradeDefinition definition)
    {
        if (EnemyManager.AnyEnemyAlive)
            return;

        _upgradesShowing = false;
        _level++;
        LudeoManager.SetGameplayState(LudeoWrapper.WAVE, _level);
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
