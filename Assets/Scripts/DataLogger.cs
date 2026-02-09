using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using System; 
using UnityEngine.XR;          // for haptics
using UnityEngine.UI;  

public class DataLogger : MonoBehaviour
{
    [Header("References")]
    public Transform pointer;    
    public Transform sphere;
    public Transform triangleCenter;
    public TrialManager trialManager;
    public Transform hmdCamera;

    [Header("Button Input")]
    public InputActionProperty aButtonAction; // subject presses A to record angle

    [Header("Haptics (Right Controller)")]
    [Range(0f, 1f)] public float hapticAmplitude = 0.6f;
    public float hapticDuration = 0.08f;

    private void TriggerRightHaptics(float amplitude, float duration)
    {
        var device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        if (!device.isValid) return;

        if (device.TryGetHapticCapabilities(out var caps) && caps.supportsImpulse)
        {
            device.SendHapticImpulse(0u, amplitude, duration);
        }
    }

    private string subjectID = "default_subject";
    private string filePath;
    private Trial currentTrial;

    private void Start()
    {
        if (trialManager == null)
            trialManager = FindFirstObjectByType<TrialManager>();

        if (aButtonAction.action != null)
            aButtonAction.action.Enable();
    }

    private void Update()
    {
        if (aButtonAction.action != null && aButtonAction.action.WasPressedThisFrame())
            LogCurrentAngle();
    }

    /// Called from ExperimentSetup when subject ID is entered
    public void SetSubjectID(string id)
    {
        subjectID = id;
        SetupFile(subjectID);
    }

    /// Called from TrialManager when a new trial starts
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

        string header = "Time,TrialNumber,ConditionID,Distance,Sphere Side,StartAngle,MeasuredAngle,HMDHeight";
        File.WriteAllText(filePath, header + "\n");

        Debug.Log($"[DataLogger] Logging to: {filePath}");
    }

    private string GetLogFolder()  // creates data folder on desktop
    {
        return Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "ExperimentData");
    }

    private void LogCurrentAngle()
    {
        if (pointer == null || sphere == null || currentTrial == null || triangleCenter == null)
        {
            Debug.LogWarning("[DataLogger] Missing references or trial info.");
            return;
        }
        
        Vector3 referenceDir = (sphere.position - pointer.position).normalized;  // 0° reference: line from pointer → sphere
        Vector3 measuredDir = pointer.right.normalized;  // using pointer.forward gives incorrect measures by 90°, pointer position visually points to the local 'right' side (prefab issues again)
        Vector3 upAxis = -Vector3.up;       //toward participant = negative, away = positive
        float angle = Vector3.SignedAngle(referenceDir, measuredDir, upAxis);

        // invert sign for left-side trials for positive angle toward participant
        if (currentTrial.targetSide == "Right")
            angle *= -1f;

        // HMD height logging
        float hmdHeight = -1f;
        if (hmdCamera != null)
            hmdHeight = hmdCamera.position.y;
        else
            Debug.LogWarning("[DataLogger] hmdCamera not assigned");

        string line = $"{Time.time:F2},{currentTrial.trialNumber},{currentTrial.conditionID},{currentTrial.distance},{currentTrial.targetSide},{currentTrial.startAngle:F2},{angle:F2},{hmdHeight:F3}";
        File.AppendAllText(filePath, line + "\n");

        Debug.Log($"[DataLogger] Trial {currentTrial.trialNumber} | Sphere Side {currentTrial.targetSide} | Angle {angle:F2}° | HMD Y: {hmdHeight:F3}m");
        trialManager?.MarkTrialAsCompleted();
    }
}
