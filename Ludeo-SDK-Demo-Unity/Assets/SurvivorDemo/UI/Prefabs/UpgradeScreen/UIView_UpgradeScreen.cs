using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIView_UpgradeScreen : MonoBehaviour
{
    public RectTransform Content;
    public UIView_UpgradeCard p_UpgradeCard;

    public void Init(UpgradeDefinition[] upgrades, Action<UpgradeDefinition> chosen)
    {
        CleanContent();

        foreach (UpgradeDefinition upgrade in upgrades)
        {
            var card = Instantiate(p_UpgradeCard, Content);
            card.Init(upgrade, () => chosen(upgrade));
        }
    }

    private void CleanContent()
    {
        foreach (Transform child in Content.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
