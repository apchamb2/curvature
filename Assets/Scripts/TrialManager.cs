using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TrialManager : MonoBehaviour
{
    [Header("References")]
    public DataLogger dataLogger;
    public ExperimentManager experimentManager;
    public Transform trianglePrefab; // parent (barycenter of the triangle)
    public Transform pointer;
    public Transform sphere;
    public Transform vertex;         // third point of triangle

    [Header("Input")]
    public InputActionProperty triggerAction; // bound to Trigger button

    private List<Trial> trials;
    private int currentTrialIndex = -1;
    private bool trialCompleted = false;

    private void Update()
    {
        if (triggerAction != null && triggerAction.action != null)
        {
            if (triggerAction.action.WasPressedThisFrame())
            {
                if (trialCompleted)
                {
                    NextTrial();
                }
                else
                {
                    Debug.Log("[TrialManager] Trial not logged yet (have user press A first).");
                }
            }
        }
    }

    public void InitializeTrials(List<Trial> trialList)
    {
        trials = trialList;
        currentTrialIndex = -1;
    }

    public void StartExperiment()
    {
        currentTrialIndex = -1;
        NextTrial();
    }

    private void NextTrial()
    {
        currentTrialIndex++;

        if (trials == null || currentTrialIndex >= trials.Count)
        {
            Debug.Log("[TrialManager] Experiment has ended");
            return;
        }

        Trial trial = trials[currentTrialIndex];
        ApplyTrialSettings(trial);

        if (experimentManager != null)
        {
            experimentManager.ApplyPointerRandomization(trial);
        }
        // Tells DataLogger which trial is active
        if (dataLogger != null)
        {
            dataLogger.SetCurrentTrial(trial);
        }
        trialCompleted = false;

        Debug.Log($"[TrialManager] Trial {trial.trialNumber} (Condition {trial.conditionID}) | Distance: {trial.distance} | Side: {trial.targetSide}");
    }

    public void MarkTrialAsCompleted()
    {
        trialCompleted = true;
        Debug.Log("[TrialManager] Data saved on button press");
    }

    private void ApplyTrialSettings(Trial trial)
    {
        float d = trial.distance;

        // Center is the barycenter (trianglePrefab position)
        Vector3 center = trianglePrefab.position;

        // Equilateral triangle layout (on XZ plane, pointing forward)
        // Distance from barycenter to each vertex = d / √3
        Vector3 forward = Vector3.forward * d / Mathf.Sqrt(3);
        Vector3 left = Quaternion.Euler(0, -120, 0) * forward;
        Vector3 right = Quaternion.Euler(0, 120, 0) * forward;

        if (trial.targetSide == "Left")
        {
            pointer.position = center + right;
            sphere.position = center + left;
        }
        else // Right
        {
            pointer.position = center + left;
            sphere.position = center + right;
        }

        // Vertex is always forward
        if (vertex != null)
        {
            vertex.position = center + forward;
        }
    }
}
