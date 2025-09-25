using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ExperimentSetup : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField subjectIDField;
    public Button setHeightButton;
    public Button startButton;

    [Header("Trial Inputs")]
    public TMP_InputField[] distanceFields = new TMP_InputField[5];
    public TMP_Dropdown[] sideDropdowns = new TMP_Dropdown[5];

    [Header("References")]
    public DataLogger dataLogger;
    public TrialManager trialManager;
    public Transform trianglePrefab;
    public Transform hmdCamera;

    public void Start()
    {
        if (subjectIDField != null)
            subjectIDField.onEndEdit.AddListener(OnSubjectIDEntered);

        if (setHeightButton != null)
            setHeightButton.onClick.AddListener(SetHeightFromHMD);

        if (startButton != null)
            startButton.onClick.AddListener(StartExperiment);

        // Make sure dropdowns have Left/Right options
        foreach (var dd in sideDropdowns)
        {
            if (dd != null && dd.options.Count == 0)
            {
                dd.options.Add(new TMP_Dropdown.OptionData("Left"));
                dd.options.Add(new TMP_Dropdown.OptionData("Right"));
            }
        }
    }

    private void OnSubjectIDEntered(string newID)
    {
        if (dataLogger != null && !string.IsNullOrEmpty(newID))
            dataLogger.SetSubjectID(newID);
    }

    public void SetHeightFromHMD()
    {
        if (hmdCamera == null || trianglePrefab == null) return;

        Vector3 pos = trianglePrefab.position;
        trianglePrefab.position = new Vector3(pos.x, hmdCamera.position.y, pos.z);

        Debug.Log($"[ExperimentSetup] Triangle set to HMD height: {hmdCamera.position.y:F2} m");
    }

    private void StartExperiment()
    {
        List<Trial> trials = new List<Trial>();

        for (int i = 0; i < 5; i++)
        {
            float distance = 5f;
            string side = "Right";

            if (distanceFields[i] != null)
                float.TryParse(distanceFields[i].text, out distance);

            if (sideDropdowns[i] != null)
                side = sideDropdowns[i].options[sideDropdowns[i].value].text;

            trials.Add(new Trial
            {
                trialNumber = i + 1,
                distance = distance,
                targetSide = side
            });
        }

        trialManager.InitializeTrials(trials);
        trialManager.StartExperiment();

        Debug.Log("[ExperimentSetup] Started experiment with 5 trials.");
    }
}
