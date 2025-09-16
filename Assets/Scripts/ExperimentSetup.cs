using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExperimentSetup : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField subjectIDField;
    public Slider distanceSlider;
    public TMP_Text distanceValueText;
    public Button swapButton;
    public Button setHeightButton;

    [Header("References")]
    public DataLogger dataLogger;      // for passing subject ID
    public Transform triangle;         // parent prefab of pointer/sphere/vertex
    public Transform pointer;          // pointer vertex
    public Transform sphere;           // sphere vertex
    public Transform hmdCamera;        // XR rig's Main Camera (HMD)

    [Header("Triangle Settings")]
    public float minDistance = 2f;
    public float maxDistance = 20f;

    private float baseEdgeLength;   // original prefab side length
    private Vector3 baseScale;      // original prefab scale

    private void Start()
    {
        // Save the triangle's original size (assume prefab starts at base size in scene)
        baseScale = triangle.localScale;

        // Compute original edge length from pointer → sphere distance
        if (pointer != null && sphere != null)
        {
            baseEdgeLength = Vector3.Distance(pointer.position, sphere.position);
        }
        else
        {
            baseEdgeLength = 2f; // fallback
        }

        // --- Subject ID field ---
        if (subjectIDField != null)
            subjectIDField.onEndEdit.AddListener(OnSubjectIDEntered);

        // --- Distance slider ---
        if (distanceSlider != null)
        {
            distanceSlider.minValue = minDistance;
            distanceSlider.maxValue = maxDistance;
            distanceSlider.onValueChanged.AddListener(OnDistanceChanged);

            OnDistanceChanged(distanceSlider.value);
        }

        // --- Swap button ---
        if (swapButton != null)
            swapButton.onClick.AddListener(SwapPointerAndSphere);

        // --- Set Height button ---
        if (setHeightButton != null)
            setHeightButton.onClick.AddListener(SetHeightFromHMD);
    }

    private void OnSubjectIDEntered(string newID)
    {
        if (dataLogger != null && !string.IsNullOrEmpty(newID))
        {
            dataLogger.SetSubjectID(newID);
        }
    }

    private void OnDistanceChanged(float newDistance)
    {
        if (distanceValueText != null)
            distanceValueText.text = $"{newDistance:F1} m";

        if (triangle != null && baseEdgeLength > 0f)
        {
            // Scale factor = desired edge length / original edge length
            float scaleFactor = newDistance / baseEdgeLength;
            triangle.localScale = baseScale * scaleFactor;
        }
    }

    private void SwapPointerAndSphere()
    {
        if (pointer == null || sphere == null) return;

        Vector3 tempPos = pointer.position;
        Quaternion tempRot = pointer.rotation;

        pointer.position = sphere.position;
        pointer.rotation = sphere.rotation;

        sphere.position = tempPos;
        sphere.rotation = tempRot;
    }

    private void SetHeightFromHMD()
    {
        if (hmdCamera == null || triangle == null) return;

        Vector3 trianglePos = triangle.position;
        triangle.position = new Vector3(trianglePos.x, hmdCamera.position.y, trianglePos.z);

        Debug.Log($"Triangle parent set to user height: {hmdCamera.position.y:F2} meters");
    }
}
