using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float Speed = 10f;
    public int MaxHP = 100;
    [field: SerializeField]
    public int HP { get; private set; } = 100;

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
    }

    public void Heal(int amount)
    {
        HP += amount;
        if (HP > MaxHP)
        {
            HP = MaxHP;
        }
    }

    public void AddAbility(Ability ability)
    {
        Abilities.Add(ability);
    }
}
