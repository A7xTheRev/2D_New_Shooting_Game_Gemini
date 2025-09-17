using UnityEngine;
using System;
using UnityEngine.UI;

public class AbilityController : MonoBehaviour
{
    [HideInInspector]
    public SpecialAbility equippedAbility;

    private float currentCharge;
    private PlayerStats playerStats;
    private PlayerController playerController;

    public event Action<float, float, Sprite> OnChargeChanged;

    void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        playerController = GetComponent<PlayerController>();

        if (ProgressionManager.Instance != null)
        {
            equippedAbility = ProgressionManager.Instance.GetEquippedAbility();
        }
    }

    void Start()
    {
        if (equippedAbility == null)
        {
            Debug.LogWarning("Nessuna abilità speciale equipaggiata o trovata.");
            Image abilityChargeImage = GetComponentInChildren<Image>();
            if (abilityChargeImage != null)
                abilityChargeImage.gameObject.SetActive(false);
            this.enabled = false;
            return;
        }

        currentCharge = 0;
        OnChargeChanged?.Invoke(currentCharge, equippedAbility.maxCharge, equippedAbility.icon);
    }

    void Update()
    {
        if (equippedAbility != null && equippedAbility.chargeType == ChargeType.Time && currentCharge < equippedAbility.maxCharge)
        {
            AddCharge(equippedAbility.chargePerSecond * Time.deltaTime);
        }
    }

    public void AddChargeFromKill()
    {
        if (equippedAbility != null && equippedAbility.chargeType == ChargeType.Kills)
        {
            AddCharge(equippedAbility.chargePerKill);
        }
    }

    public void AddCharge(float amount)
    {
        if (currentCharge >= equippedAbility.maxCharge) return;
        currentCharge += amount;
        if (currentCharge > equippedAbility.maxCharge)
        {
            currentCharge = equippedAbility.maxCharge;
        }
        OnChargeChanged?.Invoke(currentCharge, equippedAbility.maxCharge, equippedAbility.icon);
    }

    public void ActivateAbility()
    {
        if (equippedAbility == null || currentCharge < equippedAbility.maxCharge) { return; }

        AudioManager.Instance.PlaySound(AudioManager.Instance.abilityActivateSound);

        currentCharge = 0;
        OnChargeChanged?.Invoke(currentCharge, equippedAbility.maxCharge, equippedAbility.icon);

        float finalDuration = equippedAbility.duration + (playerStats.abilityPower * equippedAbility.durationBonusPerPower);
        int finalDPS = Mathf.RoundToInt(playerStats.abilityPower * equippedAbility.damageMultiplier);

        if (equippedAbility.abilityPrefab != null)
        {
            if (playerController.firePoint == null) { return; }
            Transform firePoint = playerController.firePoint;
            
            // VERSIONE ORIGINALE, CHIEDERE A GEMINI COME SISTEMARE IL RAGGIO LASER A 180°
            // GameObject abilityInstance = Instantiate(equippedAbility.abilityPrefab, firePoint.position, firePoint.rotation, firePoint);
            // SOLUZIONE PROVVISORIA: NON PARENTIAMO L'ABILITÀ AL FIREPOINT
            Quaternion flippedRotation = firePoint.rotation * Quaternion.Euler(0, 0, 180);
            GameObject abilityInstance = Instantiate(equippedAbility.abilityPrefab, firePoint.position, flippedRotation, firePoint);
            // FINE SOLUZIONE PROVVISORIA

            LaserBeam beam = abilityInstance.GetComponent<LaserBeam>();
            if (beam != null) { beam.Activate(playerStats, finalDPS); }
            Destroy(abilityInstance, finalDuration);
        }
        else
        {
            playerStats.ActivateTemporaryInvulnerability(finalDuration);
        }
    }
}