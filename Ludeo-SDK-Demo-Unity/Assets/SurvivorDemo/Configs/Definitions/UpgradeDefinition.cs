using UnityEngine;

[CreateAssetMenu(menuName = "Ludeo Demo/Configs/Upgrade")]
public class UpgradeDefinition : ScriptableObject
{
    public string Key;
    public float Value;
    public string Name;
    [Multiline]
    public string Description;
    public Sprite View;
}
