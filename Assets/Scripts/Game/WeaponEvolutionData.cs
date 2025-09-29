using UnityEngine;
using System.Collections.Generic;

// Definiamo una piccola classe di supporto per tenere insieme
// il potenziamento richiesto e quante volte serve.
[System.Serializable]
public class PowerUpRequirement
{
    public PowerUpEffect requiredPowerUp;
    public int requiredCount = 1;
}

[CreateAssetMenu(fileName = "New Weapon Evolution", menuName = "Game Data/Weapon Evolution")]
public class WeaponEvolutionData : ScriptableObject
{
    [Header("IT: Requisiti per l'Evoluzione")]
    [Tooltip("IT: L'arma di base che deve essere equipaggiata.")]
    public WeaponData baseWeapon;

    [Tooltip("IT: La lista dei potenziamenti necessari e il loro conteggio minimo.")]
    public List<PowerUpRequirement> powerUpRequirements;

    [Header("IT: Risultato dell'Evoluzione")]
    [Tooltip("IT: L'arma potenziata che il giocatore otterrà.")]
    public WeaponData evolvedWeapon;

    [Tooltip("IT: Il prefab del pulsante speciale da mostrare nella UI per questa evoluzione.")]
    public GameObject evolutionButtonPrefab;
}