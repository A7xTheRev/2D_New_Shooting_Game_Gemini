using UnityEngine;

// Classe base per tutti i modificatori degli Elite.
// Ogni nuovo modificatore (Scudo, Vortice, etc.) erediterà da questa classe.
public abstract class EliteModifier : MonoBehaviour
{
    // Metodo astratto per copiare le proprietà da un'istanza all'altra
    public abstract void CopyProperties(EliteModifier source);

    public virtual void Activate(EnemyStats stats) { }

    // Metodo che verrà chiamato alla morte del nemico
    public virtual void OnDeath() { }
}