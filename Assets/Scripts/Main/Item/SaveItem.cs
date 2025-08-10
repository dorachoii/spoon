using UnityEngine;

public class SaveItem : ItemBase
{
    protected override void ApplyEffect(GameObject player)
    {
        PersistenceManager persistenceManager = FindObjectOfType<PersistenceManager>();
        if (persistenceManager != null)
        {
            persistenceManager.SaveGame();
            ShowStatusText("Game Saved!", PlayerColor.Green.ToColor());
        }
    }
}
