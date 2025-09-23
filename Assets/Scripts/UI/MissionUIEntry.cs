using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MissionUIEntry : MonoBehaviour
{
    [Header("Riferimenti UI")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI progressText;
    public Slider progressBar;
    public Button claimButton;
    public GameObject completedIndicator; // Es. una spunta verde

    private MissionData currentMission;

    // Questo metodo viene chiamato per configurare la riga con i dati di una missione
    public void Setup(MissionData missionData)
    {
        currentMission = missionData;
        
        titleText.text = missionData.title;
        descriptionText.text = missionData.description;

        // Ottieni i dati dal ProgressionManager
        int progress = ProgressionManager.Instance.GetMissionProgress(missionData.missionID);
        int target = missionData.targetValue;
        bool isComplete = ProgressionManager.Instance.IsMissionComplete(missionData.missionID);
        bool isClaimed = ProgressionManager.Instance.IsMissionClaimed(missionData.missionID);

        // Aggiorna la barra e il testo del progresso
        progressText.text = $"{progress} / {target}";
        progressBar.maxValue = target;
        progressBar.value = progress;

        // Imposta lo stato del pulsante
        if (isClaimed)
        {
            claimButton.gameObject.SetActive(false);
            completedIndicator.SetActive(true);
        }
        else
        {
            completedIndicator.SetActive(false);
            claimButton.gameObject.SetActive(true);
            claimButton.interactable = isComplete; // Il pulsante è cliccabile solo se la missione è completa

            // Aggiungi l'azione al click del pulsante
            claimButton.onClick.RemoveAllListeners();
            claimButton.onClick.AddListener(OnClaimButtonPressed);
        }
    }

    private void OnClaimButtonPressed()
    {
        if (currentMission != null)
        {
            ProgressionManager.Instance.ClaimMissionReward(currentMission.missionID);
            // Non c'è bisogno di aggiornare manualmente la UI qui,
            // perché il ProgressionManager invierà un segnale che aggiornerà l'intero pannello.
        }
    }
}