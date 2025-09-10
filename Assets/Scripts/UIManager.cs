using UnityEngine;
using UnityEngine.UI;
using TMPro;


// Gestisce la UI: vita, esperienza, coins di sessione, stage e livello player
public class UIManager : MonoBehaviour
{
[Header("UI Player")]
public Slider playerHealthBar;
public TextMeshProUGUI sessionCoinsText; // Mostra solo i coins della sessione
public TextMeshProUGUI xpText;
public TextMeshProUGUI stageText; // Mostra numero stage
public TextMeshProUGUI levelText; // Mostra livello player


private PlayerStats player;
private StageManager stageManager;


void Start()
{
player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
stageManager = GameObject.FindFirstObjectByType<StageManager>();


if (player != null)
{
player.OnLevelUp += UpdateLevelUI;
player.OnXPChanged += UpdateXPUI;
player.OnHealthChanged += UpdateHealthUI;
player.OnSessionCoinsChanged += UpdateSessionCoinsUI;
}


// Aggiorna subito la UI con i valori iniziali
UpdateSessionCoinsUI(player != null ? player.sessionCoins : 0);
if (player != null)
{
UpdateLevelUI(player.level);
UpdateXPUI(player.currentXP, player.xpToLevelUp);
UpdateHealthUI(player.currentHealth, player.maxHealth);
}
}


void Update()
{
if (stageManager != null && stageText != null)
stageText.text = "STAGE: " + stageManager.stageNumber;
}


public void UpdateLevelUI(int newLevel)
{
if (levelText != null)
levelText.text = "Level: " + newLevel;
}


public void UpdateXPUI(int currentXP, int xpToLevelUp)
{
if (xpText != null)
xpText.text = $"XP: {currentXP}/{xpToLevelUp}";
}


public void UpdateHealthUI(int currentHealth, int maxHealth)
{
if (playerHealthBar != null)
playerHealthBar.value = (float)currentHealth / maxHealth;
}


private void UpdateSessionCoinsUI(int sessionCoins)
{
if (sessionCoinsText != null)
sessionCoinsText.text = "Coins: " + sessionCoins;
}
}