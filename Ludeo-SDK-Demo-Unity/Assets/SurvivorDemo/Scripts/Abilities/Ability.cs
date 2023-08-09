using LudeoSDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    public bool CanUse => Time.time - _lastTimeAbiltyUsed >= Cooldown;

    [SerializeField]
    private float _cooldown = 1f;
    public float Cooldown
    {
        get => _cooldown; 
        set
        {
            _cooldown = value;
            LudeoManager.SetGameplayState(gameObject.GetInstanceID()
                + LudeoWrapper.ABILITY_COOLDOWN, _cooldown);
        }
    }

    private float _lastTimeAbiltyUsed = 0f;

    public virtual void Use(Transform target)
    {
        if (CanUse)
        {
            _lastTimeAbiltyUsed = Time.time;
        }
    }
}
