using UnityEngine;

[RequireComponent(typeof(PlayerStats), typeof(SpriteRenderer))]
public class PlayerAppearance : MonoBehaviour
{
    private PlayerStats stats;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Carica la navicella scelta dal ProgressionManager
        if (ProgressionManager.Instance != null)
        {
            ShipData equippedShip = ProgressionManager.Instance.GetEquippedShip();
            if (equippedShip != null)
            {
                // Applica i dati e lo sprite della navicella scelta
                stats.playerData = equippedShip.baseStats;
                spriteRenderer.sprite = equippedShip.shipSprite;
            }
            else
            {
                Debug.LogError("Nessuna navicella equipaggiata trovata!");
            }
        }
    }
}