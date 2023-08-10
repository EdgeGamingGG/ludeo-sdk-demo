using LudeoSDK;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float Speed = 10f;
    [SerializeField]
    private int _maxHP = 100;
    public int MaxHP
    {
        get => _maxHP; 
        set
        {
            _maxHP = value;
            // LudeoManager.SetGameplayState(LudeoWrapper.PLAYER_MAXHP, MaxHP);
        }
    }
    [field: SerializeField]
    public int HP { get; set; } = 100;

    public List<Ability> Abilities;
    private EnemyManager _enemyManager;

    private void Awake()
    {
        HP = MaxHP;
    }

    public void Init(EnemyManager enemyManager)
    {
        _enemyManager = enemyManager;
    }


    List<Transform> _memo = new List<Transform>(100);
    private void Update()
    {
        var vertical = Input.GetAxis("Vertical");
        var horizonatal = Input.GetAxis("Horizontal");
        if (vertical != 0)
        {
            transform.position += Vector3.forward * vertical * Time.deltaTime * Speed;
        }
        if (horizonatal != 0)
        {
            transform.position += Vector3.right * horizonatal * Time.deltaTime * Speed;
        }


        _memo.Clear();
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            var enemyManager = FindObjectOfType<EnemyManager>();
            var ability = Abilities[0];
            var temp = ability.Cooldown;
            ability.Cooldown = 0f;

            for (int i = 0; i < enemyManager.EnemiesLeft; i++)
            {
                var enemy = enemyManager.GetClosestEnemyTo(transform.position, _memo);
                if (enemy != null)
                {
                    _memo.Add(enemy.transform);
                    ability.Use(enemy.transform);
                }
            }

            ability.Cooldown = temp;
        }
#endif

        foreach (var ability in Abilities)
        {
            var enemy = _enemyManager.GetClosestEnemyTo(transform.position, _memo);
            if (enemy != null)
            {
                _memo.Add(enemy.transform);
                ability.Use(enemy.transform);
            }
        }
    }

    public void TakeDamage(Enemy e)
    {
        HP -= e.Damage;
        if (HP <= 0)
        {
            HP = 0;
        }

        // LudeoManager.SetGameplayState(LudeoWrapper.PLAYER_HP, HP);
    }

    public void Heal(int amount)
    {
        HP += amount;
        if (HP > MaxHP)
        {
            HP = MaxHP;
        }

        // LudeoManager.SetGameplayState(LudeoWrapper.PLAYER_HP, HP);
    }

    public void AddAbility(Ability ability)
    {
        Abilities.Add(ability);
        // LudeoManager.SetGameplayState(LudeoWrapper.PLAYER_ABILITY_COUNT, Abilities.Count);
    }
}
