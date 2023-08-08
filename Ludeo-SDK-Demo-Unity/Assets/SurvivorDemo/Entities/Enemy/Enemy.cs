using System;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    Transform _target;
    public float Speed = 5;
    public int Damage = 10;
    public int MaxHp = 10;
    [field: SerializeField]
    public int HP { get; set; }

    public event Action<Enemy> OnDeath;

    private void Awake()
    {
        HP = MaxHp;
    }

    public void Init(Transform target)
    {
        _target = target;
    }

    private void Update()
    {
        if (_target != null)
            Follow();
    }
     
    private void Follow()
    {
        transform.Translate((_target.position - transform.position).normalized * Time.deltaTime * Speed, Space.World);
    }

    bool _dead = false;
    public void TakeDamage(int damage)
    {
        HP -= damage;

        if (HP <= 0)
            Die();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player p))
        {
            p.TakeDamage(this);
            Die();
        }
    }

    private void Die()
    {
        if (_dead)
            return;

        _dead = true;
        Destroy(gameObject);
        OnDeath?.Invoke(this);
    }
}
