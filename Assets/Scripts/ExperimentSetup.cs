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

    [Header("Trial Counter UI")]
    public TMP_Text trialCounterText;


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
        {
            startButton.onClick.RemoveListener(StartExperiment);
            startButton.onClick.AddListener(StartExperiment);
        }

        if (trialManager != null)
        {
            trialManager.OnTrialCounterChanged -= HandleTrialCounterChanged;
            trialManager.OnTrialCounterChanged += HandleTrialCounterChanged;
        }
    }
    private void OnDestroy()
    {
        if (trialManager != null)
            trialManager.OnTrialCounterChanged -= HandleTrialCounterChanged;   
    }

    private void HandleTrialCounterChanged(bool inPractice, int current, int total)
    {
        if (trialCounterText == null) return;

        if (total <= 0)
        {
            trialCounterText.text = inPractice ? "Practice -/-" : "Trial -/-";
            return;
        }

        trialCounterText.text = inPractice
            ? $"Practice {current}/{total}"
            : $"Trial {current}/{total}";
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

        float height = hmdCamera.position.y;

        // Triangle centered at origin, at current HMD height
        trianglePrefab.position = new Vector3(0f, height, 0f);

        float newYaw = 180f;
        float currentYaw = hmdCamera.eulerAngles.y;
        float yawDelta = Mathf.DeltaAngle(currentYaw, newYaw);

        xrOrigin.RotateAround(hmdCamera.position, Vector3.up, yawDelta);
        // Move rig so the HMD ends up at (0, height, 0) without changing rotation
        Vector3 newHmdPos = new Vector3(0f, height, 0f);
        Vector3 delta = newHmdPos - hmdCamera.position;
        xrOrigin.position += delta;

        Debug.Log($"[ExperimentSetup] Recentered HMD position to {newHmdPos}. Actual HMD pos: {hmdCamera.position}");
    }

    private void StartExperiment()
    {
        if (experimentManager == null || trialManager == null)
        {
            Debug.LogError("[ExperimentSetup] Missing references to ExperimentManager or TrialManager");
            return;
        }

        if (startButton != null)
        {
            startButton.interactable = false;
            startButton.onClick.RemoveListener(StartExperiment);
        }

        trialManager.StartExperiment();

        Debug.Log("[ExperimentSetup] Experiment started.");

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
