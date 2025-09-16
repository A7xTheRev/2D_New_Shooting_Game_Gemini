using UnityEngine;

public class VFX_LifeCycle : MonoBehaviour
{
    [Tooltip("L'etichetta (tag) di questo VFX, deve corrispondere a quella nel VFXPool.")]
    public string vfxTag;
    public float lifeTime = 0.5f;

    void OnEnable()
    {
        // Usiamo Invoke per chiamare il metodo Deactivate dopo 'lifeTime' secondi
        Invoke(nameof(Deactivate), lifeTime);
    }

    void Deactivate()
    {
        // Invece di disattivare l'oggetto, lo restituiamo alla pool
        if (VFXPool.Instance != null && !string.IsNullOrEmpty(vfxTag))
        {
            VFXPool.Instance.ReturnVFX(vfxTag, gameObject);
        }
        else
        {
            // Fallback di sicurezza se la pool non esiste
        gameObject.SetActive(false);
        }
    }

    void OnDisable()
    {
        // È buona norma cancellare l'Invoke se l'oggetto viene disattivato prima del tempo
        CancelInvoke(nameof(Deactivate));
    }
}