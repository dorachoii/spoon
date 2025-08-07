using UnityEngine;

public class SaveItem : ItemBase
{
    protected override void ApplyEffect(GameObject player)
    {
        PersistenceManager gameManager = FindObjectOfType<PersistenceManager>();
        if (gameManager != null)
        {
            gameManager.SaveGame();
            Debug.Log("Game saved successfully!");
            ShowStatusText("Game Saved!", PlayerColor.Green.ToColor());
        }
        else
        {
            Debug.LogError("GameManager not found!");
        }
    }
}
