using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    // Creiamo un Singleton per accedere facilmente a questo script da qualsiasi altro.
    public static CameraShake Instance;

    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;

    void Awake()
    {
        // Impostazione del Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Salviamo la posizione originale della camera all'avvio
        originalPosition = transform.localPosition;
    }

    // Questo è il metodo pubblico che chiameremo da altri script
    public void StartShake(float duration, float magnitude)
    {
        // Se c'è già una scossa in corso, la fermiamo prima di iniziarne una nuova
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            transform.localPosition = originalPosition; // Resetta subito la posizione
        }
        
        shakeCoroutine = StartCoroutine(Shake(duration, magnitude));
    }

    private IEnumerator Shake(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // Genera un punto casuale all'interno di un cerchio (per il 2D è meglio di una sfera)
            Vector2 randomPoint = Random.insideUnitCircle * magnitude;
            
            // Applica l'offset alla posizione originale
            transform.localPosition = new Vector3(originalPosition.x + randomPoint.x, originalPosition.y + randomPoint.y, originalPosition.z);
            
            elapsed += Time.deltaTime;
            
            // Aspetta il prossimo frame
            yield return null;
        }
        
        // Alla fine, riporta la camera alla sua posizione originale
        transform.localPosition = originalPosition;
        shakeCoroutine = null; // Resetta la coroutine
    }
}