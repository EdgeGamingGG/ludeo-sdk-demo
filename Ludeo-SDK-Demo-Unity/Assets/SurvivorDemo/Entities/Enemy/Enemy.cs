using LudeoSDK;
using System;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    Transform _target;

    public int HP { get; private set; }

    [SerializeField]
    private int _maxHp = 10;
    public int MaxHp
    {
        get => _maxHp; set
        {
            LudeoManager.SetGameplayState(gameObject.GetInstanceID()
                                              + LudeoWrapper.ENEMY_MAXHP, value);
            _maxHp = value;
            HP = value;
        }
    }

    [SerializeField]
    private int damage = 10;
    public int Damage
    {
        get => damage;
        set
        {
            LudeoManager.SetGameplayState(gameObject.GetInstanceID()
                                              + LudeoWrapper.ENEMY_DAMAGE, value);
            damage = value;
        }
    }

    [SerializeField]
    private float _speed = 5;
    public float Speed
    {
        get => _speed; set
        {
            LudeoManager.SetGameplayState(gameObject.GetInstanceID()
                                                             + LudeoWrapper.ENEMY_SPEED, value);
            _speed = value;
        }
    }

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
        transform.Translate((_target.position - transform.position).normalized
            * Time.deltaTime * Speed, Space.World);

        var pos = transform.position;
        LudeoManager.SetGameplayState(gameObject.GetInstanceID()
                                  + LudeoWrapper.ENEMY_POSITION,
                                  new Vec3(pos.x, pos.y, pos.z));
    }

    bool _dead = false;
    public void TakeDamage(int damage)
    {
        HP -= damage;
        LudeoManager.SetGameplayState(gameObject.GetInstanceID()
                       + LudeoWrapper.ENEMY_HP, HP < 0 ? 0 : HP);
        if (HP <= 0)
        {
            HP = 0;
            Die();
        }
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
