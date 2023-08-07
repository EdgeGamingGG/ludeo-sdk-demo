using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootProjectile : Ability
{
    public Transform Origin;
    public Projectile Projectile;
    public int Damage = 10;

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
