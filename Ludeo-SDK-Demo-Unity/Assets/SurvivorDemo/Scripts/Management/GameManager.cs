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
        UIManager.SetPlayButtonOnClick(() =>
        {
            _guid = null;
            StartCoroutine(StartGame());
        });
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

    private void InitializeLudeoService(string guid)
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
        if (_player == null || _upgradesShowing || !_ludeoInitialized)
            return;

        LudeoManager.SetGameplayState(LudeoWrapper.WAVE, _level);
        LudeoManager.SetGameplayState(LudeoWrapper.TIMESCALE, Time.timeScale);
        LudeoManager.SetGameplayState(LudeoWrapper.PLAYER_POSITION,
            new Vec3(_player.transform.position.x, _player.transform.position.y, _player.transform.position.z));

        LudeoManager.SetGameplayState(LudeoWrapper.PLAYER_MAXHP, _player.MaxHP);
        LudeoManager.SetGameplayState(LudeoWrapper.PLAYER_HP, _player.HP);
        LudeoManager.SetGameplayState(LudeoWrapper.PLAYER_ABILITY_COUNT, _player.Abilities.Count);

        for (int i = 0; i < _player.Abilities.Count; i++)
        {
            Ability ability = _player.Abilities[i];
            LudeoManager.SetGameplayState(i + LudeoWrapper.ABILITY_COOLDOWN, ability.Cooldown);
            LudeoManager.SetGameplayState(i + LudeoWrapper.ABILITY_DAMAGE, (ability as ShootProjectile).Damage);
        }

        LudeoManager.SetGameplayState(LudeoWrapper.ENEMY_COUNT, EnemyManager.EnemiesLeft);

        if (_player.HP <= 0)
        {
            // game over
            EndGame();
            Debug.Log("Game Over");
            return;
        }
        else
        {
            LudeoManager.SetGameplayState(LudeoWrapper.PLAYER_DEATH, false);
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
            InitializeLudeoService(_guid);
            yield return new WaitUntil(() => _ludeoInitialized);
        }
        else
        {
            Debug.LogError("Cannot init ludeo service.");
        }

        UIManager.GameplayTransition();

        SpawnPlayer();
        CameraFollower.Init(_player.transform);
        _level = 1;

        if (!string.IsNullOrEmpty(_guid))
        {
            InitializeLudeoParameters();
        }

        GenerateLevel(_level);
    }

    private void InitializeLudeoParameters()
    {
        LudeoManager.GetGameplayState(LudeoWrapper.WAVE, out int level);
        _level = level;

        var ts = Time.timeScale;
        LudeoManager.GetGameplayState(LudeoWrapper.TIMESCALE, out ts);
        if (ts == 0)
            ts = 1;
        Time.timeScale = ts;

        LudeoManager.GetGameplayState(LudeoWrapper.PLAYER_POSITION, out Vec3 pos);
        _player.transform.position = new Vector3(pos.x, pos.y, pos.z);

        LudeoManager.GetGameplayState(LudeoWrapper.PLAYER_MAXHP, out int maxhp);
        _player.MaxHP = maxhp;

        LudeoManager.GetGameplayState(LudeoWrapper.PLAYER_HP, out int hp);
        _player.HP = hp;

        UpgradeManager.Upgraded -= NextLevel;

        LudeoManager.GetGameplayState(LudeoWrapper.PLAYER_ABILITY_COUNT, out int abilityCount);
        for (int i = 1; i < abilityCount; i++)
        {
            UpgradeManager.ChooseUpgrade(new UpgradeDefinition() { Key = "bullet_count", Value = 1 });
        }

        UpgradeManager.Upgraded += NextLevel;

        for (int i = 0; i < abilityCount; i++)
        {
            LudeoManager.GetGameplayState(i + LudeoWrapper.ABILITY_COOLDOWN, out float abilityCooldown);
            LudeoManager.GetGameplayState(i + LudeoWrapper.ABILITY_DAMAGE, out int abilityDamage);

            _player.Abilities[i].Cooldown = abilityCooldown;
            (_player.Abilities[i] as ShootProjectile).Damage = abilityDamage;
        }

        LudeoManager.GetGameplayState(LudeoWrapper.NORMAL_KILL, out int kills);
        EnemyManager.EnemiesKilledTotal = kills;
    }

    private void NextLevel(UpgradeDefinition definition)
    {
        if (EnemyManager.AnyEnemyAlive)
            return;

        _upgradesShowing = false;
        _level++;
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
