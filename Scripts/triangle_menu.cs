using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR;

public class TriangleMenu : MonoBehaviour
{
    [Header("UI References")]
    public Slider distanceSlider;
    public Button swapPositionsButton;
    public Button matchHeightButton;
    public TextMeshProUGUI distanceValueText;
    public TextMeshProUGUI heightValueText;   // 👈 New

    [Header("Triangle References")]
    public Transform pointer;
    public Transform sphere;
    public Transform vertex;
    public Transform xrOrigin;

    private float triangleDistance = 2f;
    private bool swapped = false;

    void Start()
    {
        distanceSlider.minValue = 1f;
        distanceSlider.maxValue = 50f;
        distanceSlider.value = triangleDistance;

        distanceSlider.onValueChanged.AddListener(OnDistanceChanged);
        swapPositionsButton.onClick.AddListener(SwapPointerAndSphere);
        matchHeightButton.onClick.AddListener(MatchCameraHeight);

        UpdateTriangle();
    }

    void Update()
    {
        // Continuously update the height label
        if (heightValueText != null && xrOrigin != null)
        {
            float userHeight = xrOrigin.position.y;
            heightValueText.text = $"User Height: {userHeight:F2} m";
        }
    }

    void OnDistanceChanged(float newValue)
    {
        triangleDistance = newValue;
        UpdateTriangle();
    }

    void UpdateTriangle()
    {
        float half = triangleDistance / 2f;
        Vector3 forward = xrOrigin.forward;
        Vector3 right = xrOrigin.right;

        // Positions relative to XR Origin
        pointer.position = xrOrigin.position + forward * triangleDistance + right * -half;
        sphere.position  = xrOrigin.position + forward * triangleDistance + right * half;
        vertex.position  = xrOrigin.position - forward * triangleDistance;

        // Update the distance label
        if (distanceValueText != null)
            distanceValueText.text = $"Distance: {triangleDistance:F2} m";
    }

    void SwapPointerAndSphere()
    {
        Vector3 temp = pointer.position;
        pointer.position = sphere.position;
        sphere.position = temp;
        swapped = !swapped;
        LogData("Swap");
    }

    void MatchCameraHeight()
    {
        float userHeight = xrOrigin.position.y;
        pointer.position = new Vector3(pointer.position.x, userHeight, pointer.position.z);
        sphere.position  = new Vector3(sphere.position.x, userHeight, sphere.position.z);
        vertex.position  = new Vector3(vertex.position.x, userHeight, vertex.position.z);

        LogData("MatchHeight");
    }

    void LogData(string action)
    {
        float userHeight = xrOrigin.position.y;
        string logLine = $"{System.DateTime.Now}, {action}, Distance={triangleDistance:F2}, Height={userHeight:F2}, Swapped={swapped}";
        System.IO.File.AppendAllText(Application.dataPath + "/ExperimentLogs/log.csv", logLine + "\n");
    }
}
