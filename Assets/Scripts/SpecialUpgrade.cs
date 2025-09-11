using UnityEngine;

// Tipi di potenziamenti speciali, più potenti e unici
public enum SpecialUpgradeType
{
    SecondChance, // Permette di resuscitare una volta per partita
    StartingPowerUp, // Inizia la partita con un power-up casuale già attivo
    PowerUpReroll // Permette di "rilanciare" le 3 opzioni di power-up una volta per livello
}

[System.Serializable]
public class SpecialUpgrade
{
    public string upgradeName;
    public SpecialUpgradeType upgradeType;
    public int cost = 1; // Il costo in valuta speciale
    public string description; // Descrizione di cosa fa il potenziamento

    [HideInInspector]
    public bool isUnlocked = false; // Questi potenziamenti si sbloccano una sola volta
}