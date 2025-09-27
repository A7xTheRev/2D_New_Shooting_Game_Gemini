using UnityEngine;
using System.Collections.Generic;

// --- CLASSE BASE ASTRATTA ---
// Definisce cosa tutti i PowerUp DEVONO avere.
public abstract class PowerUpEffect : ScriptableObject
{
    [Header("Informazioni Comuni")]
    public PowerUpType type;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;
    
    [Tooltip("Se spuntato, questo potenziamento può essere ottenuto una sola volta per partita.")]
    public bool isUnique = false;

    [Header("Prerequisiti")]
    [Tooltip("Se spuntato, questo potenziamento apparirà solo dopo aver ottenuto il prerequisito.")]
    public bool hasPrerequisite = false;
    public PowerUpEffect prerequisite; // Ora il prerequisito è un altro PowerUpEffect

    [Header("Regole di Esclusione")]
    [Tooltip("IT: Se il giocatore prende questo potenziamento, non potrà più trovare quelli in questa lista.")]
    public List<PowerUpEffect> mutuallyExclusivePowerUps;

    // Metodo astratto che ogni tipo di power-up dovrà implementare a modo suo.
    public abstract void Apply(PlayerStats player);
}