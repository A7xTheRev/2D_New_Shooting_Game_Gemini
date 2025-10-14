using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PilotLevelUI : MonoBehaviour
{
    [Header("Riferimenti UI")]
    public TextMeshProUGUI levelText;
    public Slider xpSlider;
    public TextMeshProUGUI xpValueText;

    [Header("Animazione e Ricompense")]
    [Tooltip("Secondi necessari per riempire una barra XP completa (da 0% a 100%)")]
    public float durationPerLevelUp = 2.0f; 
    public PilotLevelRewardPopup rewardPopupPrefab;

    [Header("Riferimenti Esterni")]
    public TabMenuController tabMenuController;

    private Coroutine xpAnimationCoroutine;
    private bool isAnimating = false;

    void OnEnable()
    {
        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.OnValuesChanged += UpdateUI;
            int xpGained = ProgressionManager.lastRunXPGained;
            if (xpGained > 0 && !isAnimating)
            {
                ProgressionManager.lastRunXPGained = 0;
                ProgressionManager pm = ProgressionManager.Instance;
                int startLevel = pm.GetPilotLevel();
                long startTotalXp = pm.GetTotalExperience();
                long finalTotalXp = startTotalXp + xpGained;
                int finalLevel = pm.GetPilotLevelFromTotalXP(finalTotalXp);
                isAnimating = true;
                xpAnimationCoroutine = StartCoroutine(AnimateXPBar(startLevel, startTotalXp, finalLevel, finalTotalXp, xpGained));
            }
            else
            {
                UpdateUI();
            }
        }
    }

    void OnDisable()
    {
        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.OnValuesChanged -= UpdateUI;
        }
        if (xpAnimationCoroutine != null)
        {
            StopCoroutine(xpAnimationCoroutine);
            if (tabMenuController != null) tabMenuController.SetTabsInteractable(true);
            isAnimating = false;
        }
    }

    private void UpdateUI()
    {
        if (ProgressionManager.Instance == null) return;
        int level = ProgressionManager.Instance.GetPilotLevel();
        long currentXP = ProgressionManager.Instance.GetCurrentLevelXP();
        long xpForNext = ProgressionManager.Instance.GetXPForNextLevel();
        if (levelText != null) { levelText.text = $"PILOT LVL: {level}"; }
        if (xpValueText != null) { xpValueText.text = $"{currentXP} / {xpForNext} XP"; }
        if (xpSlider != null && xpForNext > 0) { xpSlider.value = (float)currentXP / xpForNext; }
    }

    private IEnumerator AnimateXPBar(int startLevel, long startTotalXp, int finalLevel, long finalTotalXp, int xpGained)
    {
        isAnimating = true;
        if (tabMenuController != null) tabMenuController.SetTabsInteractable(false);
        try
        {
            ProgressionManager pm = ProgressionManager.Instance;
            if (pm == null || startTotalXp >= finalTotalXp) { yield break; }

            long currentAnimatedXp = startTotalXp;
            int lastLevelAnimated = pm.GetPilotLevelFromTotalXP(startTotalXp);

            while (currentAnimatedXp < finalTotalXp)
            {
                int animLevel = pm.GetPilotLevelFromTotalXP(currentAnimatedXp);
                long xpNeededForThisLevel = pm.GetXPForNextLevel(animLevel);
                
                // Se siamo al livello massimo o c'è un errore, interrompiamo per sicurezza
                if (xpNeededForThisLevel <= 0) break;

                // Calcola la velocità in XP/sec basata sul tempo desiderato per completare il livello CORRENTE
                float xpPerSecond = (durationPerLevelUp > 0) ? (float)xpNeededForThisLevel / durationPerLevelUp : xpNeededForThisLevel * 1000f;
                
                currentAnimatedXp += (long)(xpPerSecond * Time.deltaTime);
                if (currentAnimatedXp > finalTotalXp) { currentAnimatedXp = finalTotalXp; }

                long xpForAnimLevel = pm.GetTotalXPRequiredForLevel(animLevel);
                long xpInCurrentAnimLevel = currentAnimatedXp - xpForAnimLevel;

                levelText.text = $"PILOT LVL: {animLevel}";
                xpValueText.text = $"{xpInCurrentAnimLevel} / {xpNeededForThisLevel} XP";
                if (xpSlider != null) { xpSlider.value = (float)xpInCurrentAnimLevel / xpNeededForThisLevel; }

                if (animLevel > lastLevelAnimated)
                {
                    PilotLevelReward reward = pm.GetRewardForLevel(animLevel);
                    if (reward != null && rewardPopupPrefab != null)
                    {
                        PilotLevelRewardPopup popup = Instantiate(rewardPopupPrefab, transform);
                        popup.Show(reward);
                        yield return new WaitUntil(() => popup == null);
                    }
                    lastLevelAnimated = animLevel;
                }
                yield return null;
            }
            
            // Applica l'esperienza guadagnata effettiva per sincronizzare i dati
            pm.ApplyPendingExperience(xpGained);
            UpdateUI();
        }
        finally
        {
            isAnimating = false;
            if (tabMenuController != null) tabMenuController.SetTabsInteractable(true);
        }
    }
}