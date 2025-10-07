using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;

public class ExperimentSetup : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField subjectIDField;
    public Button setHeightButton;
    public Button startButton;
    public CanvasGroup menuCanvasGroup; 

    [Header("References")]
    public DataLogger dataLogger;
    public TrialManager trialManager;
    public ExperimentManager experimentManager;
    public Transform trianglePrefab;
    public Transform hmdCamera;
    public Transform xrOrigin;

    private void Start()
    {
        if (subjectIDField != null)
            subjectIDField.onEndEdit.AddListener(OnSubjectIDEntered);

        if (setHeightButton != null)
            setHeightButton.onClick.AddListener(SetHeightFromHMD);

        if (startButton != null)
            startButton.onClick.AddListener(StartExperiment);
    }

    private void OnSubjectIDEntered(string newID)
    {
        if (!string.IsNullOrEmpty(newID))
        {
            if (dataLogger != null)
                dataLogger.SetSubjectID(newID);

            if (experimentManager != null)
                experimentManager.subjectID = newID;
        }
    }

    public void SetHeightFromHMD()
    {
        if (hmdCamera == null || trianglePrefab == null || xrOrigin == null)
        {
            Debug.LogWarning("[ExperimentSetup] Missing HMD or triangle reference (SetHeight).");
            return;
        }

        trianglePrefab.position = new Vector3(0f, hmdCamera.position.y, 0f);

        xrOrigin.position = new Vector3(0f, xrOrigin.position.y, 0f);

        Debug.Log($"[ExperimentSetup] Triangle set to HMD height: {hmdCamera.position.y:F2} m");
    }

    private void StartExperiment()
    {
        if (experimentManager == null || trialManager == null)
        {
            Debug.LogError("[ExperimentSetup] Missing references to ExperimentManager or TrialManager");
            return;
        }

        trialManager.StartExperiment();

        Debug.Log("[ExperimentSetup] Experiment started with randomized 60 trials.");

        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.alpha = 0;
            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false;
        }
    }

    public void ShowMenu(bool show)
    {
        if (menuCanvasGroup == null) return;
        menuCanvasGroup.alpha = show ? 1 : 0;
        menuCanvasGroup.interactable = show;
        menuCanvasGroup.blocksRaycasts = show;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            bool show = menuCanvasGroup != null && menuCanvasGroup.alpha < 0.5f;
            ShowMenu(show);
        }
    }

}
