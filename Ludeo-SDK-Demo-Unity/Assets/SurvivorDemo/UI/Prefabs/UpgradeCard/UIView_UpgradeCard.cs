using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIView_UpgradeCard : MonoBehaviour
{
    public Image Icon;
    public TMP_Text Description;

    public void Init(Sprite icon, string description, Action onClick)
    {
        Icon.sprite = icon;
        Description.text = description;

        GetComponent<Button>().onClick.AddListener(() => onClick());
    }

    public void Init(UpgradeDefinition upgrade, Action onClick)
    {
        Init(upgrade.View, upgrade.Name, onClick);
    }
}
