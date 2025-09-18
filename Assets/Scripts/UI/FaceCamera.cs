using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    // Usiamo LateUpdate per assicurarci che la camera abbia finito di muoversi
    void LateUpdate()
    {
        if (mainCamera != null)
        {
            // Fa in modo che questo oggetto abbia la stessa rotazione della camera,
            // così apparirà sempre "dritto" allo spettatore.
            transform.rotation = mainCamera.transform.rotation;
        }
    }
}