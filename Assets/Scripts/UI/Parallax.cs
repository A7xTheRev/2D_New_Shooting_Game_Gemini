using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    Material mat;
    float distance;
    
    [Range(0f, 0.5f)]
    public float speed = 0.2f;

    void Start()
    {
        // Ottieni un'istanza unica del materiale per non modificare l'asset originale
        mat = GetComponent<Renderer>().material;
        
        // --- NUOVA LOGICA ---
        // Chiedi al BackgroundManager quale texture usare e applicala
        if (BackgroundManager.Instance != null)
        {
            Texture2D bgTexture = BackgroundManager.Instance.GetCurrentBackgroundTexture();
            if (bgTexture != null)
            {
                mat.SetTexture("_MainTex", bgTexture);
            }
        }
        // --- FINE NUOVA LOGICA ---
    }

    void Update()
    {
        distance += Time.deltaTime * speed;
        mat.SetTextureOffset("_MainTex", Vector2.up * distance);
    }
}