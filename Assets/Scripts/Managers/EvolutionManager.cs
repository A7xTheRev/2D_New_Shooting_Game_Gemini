using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EvolutionManager : MonoBehaviour
{
    public static EvolutionManager Instance { get; private set; }

    [Header("IT: Configurazione")]
    [Tooltip("IT: La lista di tutte le possibili evoluzioni nel gioco.")]
    public List<WeaponEvolutionData> allEvolutions;

    // Lista per tenere traccia delle evoluzioni già avvenute in questa partita
    private HashSet<WeaponData> completedEvolutionsThisRun = new HashSet<WeaponData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // All'inizio di una nuova partita, resetta le evoluzioni completate
    public void ResetRun()
    {
        completedEvolutionsThisRun.Clear();
    }

    public WeaponEvolutionData CheckForAvailableEvolutions(PlayerStats player, WeaponData currentWeapon)
    {
        // Cerca tra tutte le ricette di evoluzione
        foreach (var evolution in allEvolutions)
        {
            // Controlla se l'evoluzione è già stata fatta e se l'arma base è quella giusta
            if (!completedEvolutionsThisRun.Contains(evolution.baseWeapon) && evolution.baseWeapon == currentWeapon)
            {
                bool requirementsMet = true;
                // Controlla se tutti i requisiti dei potenziamenti sono soddisfatti
                foreach (var requirement in evolution.powerUpRequirements)
                {
                    if (!player.powerUpTracker.ContainsKey(requirement.requiredPowerUp.type) || 
                        player.powerUpTracker[requirement.requiredPowerUp.type] < requirement.requiredCount)
                    {
                        requirementsMet = false;
                        break;
                    }
                }

                if (requirementsMet)
                {
                    return evolution; // Trovata un'evoluzione valida!
                }
            }
        }
        return null; // Nessuna evoluzione disponibile
    }

    public void EvolveWeapon(PlayerStats player, PlayerController controller, WeaponEvolutionData evolution)
    {
        if (player == null || controller == null || evolution == null) return;
        
        // Equipaggia la nuova arma
        controller.EquipWeapon(evolution.evolvedWeapon);
        
        // Segna questa evoluzione come completata per questa partita
        completedEvolutionsThisRun.Add(evolution.baseWeapon);

        // Rimuovi i potenziamenti "consumati" dalla lista di quelli acquisiti
        // per evitare che appaiano di nuovo potenziamenti di basso livello
        foreach (var requirement in evolution.powerUpRequirements)
        {
            player.acquiredPowerUps.Remove(requirement.requiredPowerUp.type);
        }
        
        Debug.Log($"Arma {evolution.baseWeapon.weaponName} evoluta in {evolution.evolvedWeapon.weaponName}!");
    }
}