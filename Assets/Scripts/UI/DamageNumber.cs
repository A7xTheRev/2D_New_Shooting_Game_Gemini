using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    [Header("Riferimenti")]
    public TextMeshProUGUI damageText;

    [Header("Animazione")]
    public float lifeTime = 1f;
    public float moveSpeed = 1f;
    public float fadeSpeed = 2f;
    
    [Header("Stile Critico")]
    public Color critColor = Color.yellow;
    public float critScaleMultiplier = 1.5f;

    private float lifeTimer;
    private Color originalColor;
    private Vector3 originalScale;
    
    // Il tag per tornare alla pool, lo imposteremo nell'Inspector
    public string vfxTag = "DamageNumber"; 

    void Awake()
    {
        if (damageText != null)
        {
            originalColor = damageText.color;
        }
        originalScale = transform.localScale;
    }

    // Questo metodo viene chiamato per inizializzare e mostrare il numero
    public void Show(int damageAmount, bool isCrit)
    {
        lifeTimer = lifeTime;
        damageText.text = damageAmount.ToString();

        // Applica lo stile per i colpi normali o critici
        if (isCrit)
        {
            damageText.color = critColor;
            transform.localScale = originalScale * critScaleMultiplier;
        }
        else
        {
            damageText.color = originalColor;
            transform.localScale = originalScale;
        }
    }

    void Update()
    {
        if (lifeTimer > 0)
        {
            // Muovi il testo verso l'alto
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;

            // Fai svanire il testo
            lifeTimer -= Time.deltaTime;
            float alpha = Mathf.Clamp01(lifeTimer * fadeSpeed);
            damageText.color = new Color(damageText.color.r, damageText.color.g, damageText.color.b, alpha);

            // Quando il timer finisce, torna alla pool
            if (lifeTimer <= 0)
            {
                VFXPool.Instance.ReturnVFX(vfxTag, gameObject);
            }
        }
    }
}