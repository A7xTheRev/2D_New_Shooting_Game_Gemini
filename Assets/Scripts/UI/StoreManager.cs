using UnityEngine;
using System.Collections.Generic;

public class StoreManager : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Lista di tutti i potenziamenti da mostrare nello store.")]
    public List<PermanentUpgradeData> upgradesToShow = new List<PermanentUpgradeData>();

    [Header("UI References")]
    [Tooltip("Il prefab del pulsante/pannello per un singolo potenziamento.")]
    public GameObject upgradeUIPrefab;
    [Tooltip("Il contenitore dove verranno creati gli elementi UI dei potenziamenti.")]
    public Transform upgradesContainer;

    private List<PermanentUpgradeButton> spawnedButtons = new List<PermanentUpgradeButton>();

    void OnEnable()
    {
        DrawStoreUI();
        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.OnValuesChanged += UpdateAllButtonsUI;
        }
    }

    void OnDisable()
    {
        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.OnValuesChanged -= UpdateAllButtonsUI;
        }
    }

    private void DrawStoreUI()
    {
        if (upgradeUIPrefab == null || upgradesContainer == null)
        {
            Debug.LogError("StoreManager: Prefab o contenitore non assegnati!");
            return;
        }

        // Pulisce il contenitore e la lista di pulsanti
        foreach (Transform child in upgradesContainer)
        {
            Destroy(child.gameObject);
        }
        spawnedButtons.Clear();

        // Istanzia e configura un pulsante per ogni potenziamento
        foreach (PermanentUpgradeData upgradeData in upgradesToShow)
        {
            GameObject buttonGO = Instantiate(upgradeUIPrefab, upgradesContainer);
            PermanentUpgradeButton buttonScript = buttonGO.GetComponent<PermanentUpgradeButton>();

            if (buttonScript != null)
            {
                buttonScript.Setup(upgradeData);
                spawnedButtons.Add(buttonScript);
            }
            else
            {
                Debug.LogWarning($"Il prefab {upgradeUIPrefab.name} non ha il component PermanentUpgradeButton!");
            }
        }
    }

    private void UpdateAllButtonsUI()
    {
        foreach (PermanentUpgradeButton button in spawnedButtons)
        {
            button.UpdateUI();
        }
    }
}
