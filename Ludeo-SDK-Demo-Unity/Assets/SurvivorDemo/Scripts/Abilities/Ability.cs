using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    public bool CanUse => Time.time - _lastTimeAbiltyUsed >= Cooldown;
    [field: SerializeField]
    public float Cooldown { get; set; } = 5f;

    private float _lastTimeAbiltyUsed = 0f;

    public virtual void Use(Transform target)
    {
        if (CanUse)
        {
            _lastTimeAbiltyUsed = Time.time;
        }
    }
}
