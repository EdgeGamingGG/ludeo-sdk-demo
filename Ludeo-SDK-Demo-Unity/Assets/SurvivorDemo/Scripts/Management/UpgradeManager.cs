using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class UpgradeManager : MonoBehaviour
{
    public UIView_UpgradeCard p_UpgradeCard;
    public RectTransform Content;
    public List<UpgradeDefinition> Upgrades;

    public event Action<UpgradeDefinition> Upgraded;

    private void Awake()
    {
        gameObject.SetActive(false);
        CleanContent();
    }

    public void ShowUpgrades()
    {
        gameObject.SetActive(true);
        CleanContent();
        List<UpgradeDefinition> ups;
        ups = new List<UpgradeDefinition>(Upgrades);

        for (int i = 0; i < 3; i++)
        {
            var random = ups[Random.Range(0, ups.Count)];
            ups.Remove(random);
            var card = Instantiate(p_UpgradeCard, Content);
            card.Init(random, () => ChooseUpgrade(random));
        }
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

        gameObject.SetActive(false);

        Upgraded?.Invoke(upgrade);
    }

    private void CleanContent()
    {
        foreach (Transform child in Content.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
