using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;

public class DataLogger : MonoBehaviour
{
    [Header("References")]
    public Transform pointer;  // Pointer vertex
    public Transform sphere;   // Sphere vertex

    [Header("Button Input")]
    public InputActionProperty aButtonAction; // XRI RightHand Interaction / Activate

    private string subjectID = "DefaultSubject";
    private string filePath;

    void Start()
    {
        // Create file immediately if subjectID already set
        if (!string.IsNullOrEmpty(subjectID))
            CreateFile();
    }

    public void SetSubjectID(string newID)
    {
        if (string.IsNullOrEmpty(newID)) return;

        subjectID = newID;
        CreateFile();
    }

    private void CreateFile()
    {
        string fileName = subjectID + ".csv";
        filePath = Path.Combine(Application.persistentDataPath, fileName);

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "Timestamp,Angle\n");
        }

        Debug.Log($"[DataLogger] Logging to: {filePath}");
    }

    void Update()
    {
        if (aButtonAction == null || aButtonAction.action == null) return;
        if (pointer == null || sphere == null) return;

        if (aButtonAction.action.WasPressedThisFrame())
        {
            // Pointer forward direction
            Vector3 pointerForward = pointer.forward;

            // Direction from pointer to sphere
            Vector3 legDirection = (sphere.position - pointer.position).normalized;

            // Compute signed angle in the horizontal plane (Y axis up)
            float angle = Vector3.SignedAngle(pointerForward, legDirection, Vector3.up);

            // Append to CSV
            string line = $"{System.DateTime.Now:O},{angle:F2}\n";
            File.AppendAllText(filePath, line);

            Debug.Log($"[DataLogger] Saved angle {angle:F2}° to {filePath}");
        }
    }
}
