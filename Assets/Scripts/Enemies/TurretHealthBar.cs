using UnityEngine;
using UnityEngine.UI;
using TMPro; // Aggiunto per poter usare TextMeshPro

public class TurretHealthBar : MonoBehaviour
{
    [Header("Riferimenti")]
    public BossTurret turretStats;
    public Image healthFill;
    public TextMeshProUGUI healthText; // NUOVO RIFERIMENTO AL TESTO

    void Start()
    {
        if (turretStats == null)
            turretStats = GetComponentInParent<BossTurret>();

        if (turretStats != null)
        {
            turretStats.OnHealthChanged += UpdateHealthUI;
        }
    }

    private void OnDestroy()
    {
        if (turretStats != null)
            turretStats.OnHealthChanged -= UpdateHealthUI;
    }

    private void UpdateHealthUI(int current, int max)
    {
        // Aggiorna la barra (come prima)
        if (healthFill != null)
        {
            healthFill.fillAmount = (float)current / max;
        }

        // --- NUOVA LOGICA ---
        // Aggiorna il testo con i punti vita attuali
        if (healthText != null)
        {
            healthText.text = current.ToString();
        }
        // --- FINE NUOVA LOGICA ---
    }
}