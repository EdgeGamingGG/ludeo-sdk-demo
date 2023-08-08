using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorCheats : MonoBehaviour
{
    public Button Invincible;

    private void Start()
    {
        Invincible.onClick.AddListener(MakePlayerSuperStrong);
    }

    private void MakePlayerSuperStrong()
    {
        var player = FindObjectOfType<Player>();
        var upgrade = FindObjectOfType<UpgradeManager>(true);
        player.MaxHP = 100_000;
        player.Heal(100_000);
        upgrade.ChooseUpgrade(new UpgradeDefinition() { Key = "damage", Value = 100_000 });
        upgrade.ChooseUpgrade(new UpgradeDefinition() { Key = "bullet_count", Value = 1f });
        upgrade.ChooseUpgrade(new UpgradeDefinition() { Key = "bullet_count", Value = 1f });
        upgrade.ChooseUpgrade(new UpgradeDefinition() { Key = "bullet_count", Value = 1f });
        upgrade.ChooseUpgrade(new UpgradeDefinition() { Key = "bullet_count", Value = 1f });
        upgrade.ChooseUpgrade(new UpgradeDefinition() { Key = "bullet_count", Value = 1f });
        upgrade.ChooseUpgrade(new UpgradeDefinition() { Key = "bullet_count", Value = 1f });
        upgrade.ChooseUpgrade(new UpgradeDefinition() { Key = "attack_speed", Value = 0.3f });
    }
}
