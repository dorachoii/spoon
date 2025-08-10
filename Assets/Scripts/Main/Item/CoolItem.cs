using UnityEngine;

public class CoolItem : ItemBase
{
    protected override void ApplyEffect(GameObject player)
    {
        PlayerStat.Instance.CureHeat(20f); 
        ShowStatusText("Cooled", Color.blue);
    }   
}
