using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using System.IO;

public class TriangleMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider distanceSlider; // slider for triangle side length
    public Button swapPositionsButton;
    public Button matchHeightButton;

    [Header("Scene References")]
    public Transform pointer;
    public Transform sphere;
    public Transform vertex;
    public Transform xrOrigin;

    private float sideLength = 2.0f;
    private float headHeightOffset = 0f;
    private string logFilePath;

    void Start()
    {
        //  UI events
        distanceSlider.onValueChanged.AddListener(OnDistanceSliderChanged);
        swapPositionsButton.onClick.AddListener(SwapPositions);
        matchHeightButton.onClick.AddListener(UpdateHeadHeight);

        // not needed?
        distanceSlider.minValue = 1f;
        distanceSlider.maxValue = 50f;
        distanceSlider.value = sideLength;

        // Initialize log file
        string folderPath = Path.Combine(Application.dataPath, "ExperimentLogs");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        logFilePath = Path.Combine(folderPath, "experiment_log.csv");

        if (!File.Exists(logFilePath))
        {
            File.WriteAllText(logFilePath,
                "Timestamp,Action,SideLength,HeadHeight,PointerX,PointerY,PointerZ,SphereX,SphereY,SphereZ,VertexX,VertexY,VertexZ\n");
        }

        UpdateHeadHeight();
        UpdateTriangle();
        LogTrial("Start");
    }

    void OnDistanceSliderChanged(float value)
    {
        sideLength = value;
        UpdateTriangle();
        LogTrial("SetDistance");
    }

    void SwapPositions()
    {
        Vector3 temp = pointer.position;
        pointer.position = sphere.position;
        sphere.position = temp;
        LogTrial("SwapPositions");
    }

    void UpdateHeadHeight()
    {
        InputDevice headDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);

        if (headDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 headPos))
        {
            headHeightOffset = xrOrigin.position.y + headPos.y;
        }
        else
        {
            Debug.LogWarning("Could not read HMD position from OpenXR.");
        }

        UpdateTriangle();
        LogTrial("UpdateHeadHeight");
    }

    void UpdateTriangle()
    {
        Vector3 originPos = xrOrigin.position;
        float halfBase = sideLength / 2f;
        float height = Mathf.Sqrt(3) / 2f * sideLength;

        pointer.position = new Vector3(originPos.x - halfBase, headHeightOffset, originPos.z + height / 2f);
        sphere.position = new Vector3(originPos.x + halfBase, headHeightOffset, originPos.z + height / 2f);
        vertex.position = new Vector3(originPos.x, headHeightOffset, originPos.z - height / 2f);
    }

    void LogTrial(string action)
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

        string logEntry = string.Format(
            "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}\n",
            timestamp,
            action,
            sideLength,
            headHeightOffset,
            pointer.position.x, pointer.position.y, pointer.position.z,
            sphere.position.x, sphere.position.y, sphere.position.z,
            vertex.position.x, vertex.position.y, vertex.position.z
        );

        File.AppendAllText(logFilePath, logEntry);
        Debug.Log($"[{action}] Logged trial at {timestamp}");
    }
}
