using cohtml.Net;
using LudeoSDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootProjectile : Ability
{
    public Transform Origin;
    public Projectile Projectile;

    [SerializeField]
    private int _damage = 10;
    public int Damage
    {
        get => _damage; 
        set
        {
            //LudeoManager.SetGameplayState(gameObject.GetInstanceID()
            //                   + LudeoWrapper.ABILITY_DAMAGE, value);
            _damage = value;
        }
    }

    private void Awake()
    {
        //LudeoManager.SetGameplayState(gameObject.GetInstanceID()
        //                       + LudeoWrapper.ABILITY_DAMAGE, _damage);
    }

    public override void Use(Transform target)
    {
        if (!CanUse)
            return;

        base.Use(target);

        var projectile = Instantiate(Projectile, Origin.position, Quaternion.identity);
        projectile.Damage = Damage;
        projectile.Init(target);
    }
}
