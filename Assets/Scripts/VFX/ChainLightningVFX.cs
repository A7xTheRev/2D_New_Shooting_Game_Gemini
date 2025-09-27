using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class ChainLightningVFX : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public float lifeTime = 0.15f; // Durata molto breve

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    // Metodo per impostare l'inizio e la fine del fulmine
    public void Setup(Vector3 start, Vector3 end)
    {
        // Imposta le posizioni del Line Renderer
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        
        // Distruggi l'oggetto dopo un breve periodo
        StartCoroutine(Deactivate());
    }

    private IEnumerator Deactivate()
    {
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }
}