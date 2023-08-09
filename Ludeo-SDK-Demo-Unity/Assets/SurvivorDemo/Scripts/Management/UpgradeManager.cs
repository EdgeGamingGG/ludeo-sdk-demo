using LudeoSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class UpgradeManager : MonoBehaviour
{
    public UIView_UpgradeScreen p_UpgradeScreen;
    public List<UpgradeDefinition> Upgrades;

    public event Action<UpgradeDefinition> Upgraded;

    UIView_UpgradeScreen _runtimeScreen;

    public void ShowUpgrades()
    {
        _runtimeScreen = Instantiate(p_UpgradeScreen);
        _runtimeScreen.gameObject.SetActive(true);
        List<UpgradeDefinition> upgradePool;
        UpgradeDefinition[] upgradesChosen;
        upgradePool = new List<UpgradeDefinition>(Upgrades);
        upgradesChosen = new UpgradeDefinition[3];

        for (int i = 0; i < 3; i++)
        {
            var randomUpgrade = upgradePool[Random.Range(0, upgradePool.Count)];
            upgradePool.Remove(randomUpgrade);
            upgradesChosen[i] = randomUpgrade;
        }

        _runtimeScreen.Init(upgradesChosen, ChooseUpgrade);
    }

    float _cooldownAddon = 0f;
    float _atkSpeedBoosts = 1;
    public void ResetUpgrades()
    {
        _cooldownAddon = 0f;
        _atkSpeedBoosts = 1;
    }

    public void ChooseUpgrade(UpgradeDefinition upgrade)
    {
        switch (upgrade.Key)
        {
            case "hp":
                var player = FindObjectOfType<Player>();
                player.MaxHP += (int)upgrade.Value;
                player.Heal((int)upgrade.Value);
                break;
            case "heal":
                FindObjectOfType<Player>().Heal((int)upgrade.Value);
                break;
            case "damage":
                var shoots = FindObjectsOfType<ShootProjectile>();
                var value = upgrade.Value;
                foreach (var ability in shoots)
                {
                    ability.Damage += (int)value;
                    value--;
                    value = Mathf.Clamp(value, 1, 1000);
                }
                break;
            case "attack_speed":
                _atkSpeedBoosts *= upgrade.Value;
                var abilities = FindObjectsOfType<ShootProjectile>();
                foreach (var item in abilities)
                {
                    item.Cooldown *= upgrade.Value;
                    item.Cooldown = Mathf.Clamp(item.Cooldown, 0.1f, 1000f);
                }
                break;
            case "bullet_count":
                var shoot = FindObjectsOfType<ShootProjectile>().OrderBy(s => s.Cooldown).LastOrDefault();
                var shoot2 = Instantiate(shoot, shoot.transform.parent);
                _cooldownAddon += Random.Range(0.5f, 2f);
                shoot2.Cooldown += _cooldownAddon;
                for (int i = 0; i < _atkSpeedBoosts; i++)
                    shoot2.Cooldown *= _atkSpeedBoosts;
                FindObjectOfType<Player>().AddAbility(shoot2);
                break;
            case "time":
                Time.timeScale += upgrade.Value;
                break;
            default:
                throw new InvalidOperationException($"Invalid upgrade key: {upgrade.Key}");
        }

        if (_runtimeScreen != null)
            Destroy(_runtimeScreen.gameObject);

        Upgraded?.Invoke(upgrade);
    }
}
