using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private PlayerStats player;

    void Start()
    {
        player = GetComponent<PlayerStats>();
        if (player == null)
        {
            Debug.LogError("PlayerCollision: Nessun PlayerStats trovato sul GameObject!");
        }
    }
}
