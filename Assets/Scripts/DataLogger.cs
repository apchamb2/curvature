using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using System; 

public class DataLogger : MonoBehaviour
{
    [Header("References")]
    public Transform pointer;    
    public Transform sphere;    

    [Header("Button Input")]
    public InputActionProperty aButtonAction; // subject presses A to record angle

    private string subjectID = "default_subject";
    private string filePath;
    private Trial currentTrial;

    private void Start()
    {
        SetupFile(subjectID);
    }

    private void Update()
    {
        if (aButtonAction != null && aButtonAction.action != null)
        {
            if (aButtonAction.action.WasPressedThisFrame())
            {
                LogCurrentAngle();
            }
        }
    }

    /// <summary>
    /// Called from ExperimentSetup when subject ID is entered
    /// </summary>
    public void SetSubjectID(string id)
    {
        subjectID = id;
        SetupFile(subjectID);
    }

    /// <summary>
    /// Called from TrialManager when a new trial starts
    /// </summary>
    public void SetCurrentTrial(Trial trial)
    {
        currentTrial = trial;
    }

    private void SetupFile(string id)
    {
        string folderPath = GetLogFolder();

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string fileName = $"{id}_{timestamp}.csv";

        filePath = Path.Combine(folderPath, fileName);

        string header = "Time,TrialNumber,Distance,Side,Angle";
        File.WriteAllText(filePath, header + "\n");

        Debug.Log($"[DataLogger] Logging to: {filePath}");
    }

    private string GetLogFolder()  // creates data folder on desktop or persistent path for Quest
    {
#if UNITY_ANDROID && !UNITY_EDITOR
            // Safe path for Quest
            return Path.Combine(Application.persistentDataPath, "Data");
#else
        return Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "ExperimentData");
#endif
    }

    private void LogCurrentAngle() // calculates angle and appends to CSV
    {
        if (pointer == null || sphere == null || currentTrial == null)
        {
            Debug.LogWarning("[DataLogger] Missing references or trial info.");
            return;
        }

        float angle = Vector3.SignedAngle(
            pointer.forward,
            (sphere.position - pointer.position).normalized,
            Vector3.up);

        string line = $"{Time.time:F2},{currentTrial.trialNumber},{currentTrial.distance},{currentTrial.targetSide},{angle:F2}";
        File.AppendAllText(filePath, line + "\n");

        Debug.Log($"[DataLogger] Recorded Trial {currentTrial.trialNumber} | Distance: {currentTrial.distance} | Side: {currentTrial.targetSide} | Angle: {angle:F2}");
    }
}
