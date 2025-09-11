using UnityEngine;

public class VFX_LifeCycle : MonoBehaviour
{
    public float lifeTime = 0.5f; // Durata dell'effetto, adatta al tuo caso

    void OnEnable()
    {
        Invoke(nameof(Deactivate), lifeTime);
    }

    void Deactivate()
    {
        gameObject.SetActive(false);
        // Se in futuro userai un pool per gli effetti, qui dovresti restituirlo al pool
    }

    void OnDisable()
    {
        CancelInvoke(nameof(Deactivate));
    }
}