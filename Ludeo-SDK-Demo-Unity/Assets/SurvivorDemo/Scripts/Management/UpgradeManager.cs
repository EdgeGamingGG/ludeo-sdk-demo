using System;
using System.Collections.Generic;
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
                FindObjectOfType<ShootProjectile>().Damage += (int)upgrade.Value;
                break;
            case "attack_speed":
                var abilities = FindObjectsOfType<ShootProjectile>();
                foreach (var item in abilities)
                {
                    item.Cooldown *= upgrade.Value;
                }
                break;
            case "bullet_count":
                var shoot = FindObjectOfType<ShootProjectile>();
                var shoot2 = Instantiate(shoot, shoot.transform.parent);
                shoot2.Cooldown += 0.1f;
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
