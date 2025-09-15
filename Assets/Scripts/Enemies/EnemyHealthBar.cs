using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("Riferimenti")]
    public EnemyStats enemyStats;
    public Image healthFill;
    public TextMeshProUGUI hpText;

    void Start()
    {
        if (enemyStats == null)
            enemyStats = GetComponentInParent<EnemyStats>();

        if (enemyStats != null)
        {
            // Aggancio evento
            enemyStats.OnHealthChanged += UpdateHealthUI;

            // Aggiorna subito la barra con i valori iniziali
            UpdateHealthUI(enemyStats.currentHealth, enemyStats.maxHealth);
        }
    }

    private void OnDestroy()
    {
        if (enemyStats != null)
            enemyStats.OnHealthChanged -= UpdateHealthUI;
    }

    private void UpdateHealthUI(int current, int max)
    {
        if (healthFill != null)
            healthFill.fillAmount = (float)current / max;

        if (hpText != null)
            hpText.text = $"{current}/{max}";
    }
}
