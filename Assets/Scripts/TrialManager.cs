using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TrialManager : MonoBehaviour
{
    [Header("References")]
    public DataLogger dataLogger;
    public Transform trianglePrefab;
    public Transform pointer;
    public Transform sphere;

    [Header("Input")]
    public InputActionProperty triggerAction; // bound to Trigger button

    private List<Trial> trials;
    private int currentTrialIndex = -1;

    private void Update()
    {
        if (triggerAction != null && triggerAction.action != null)
        {
            if (triggerAction.action.WasPressedThisFrame())
            {
                NextTrial();
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
            Debug.Log("[TrialManager] Experiment finished!");
            return;
        }

        Trial trial = trials[currentTrialIndex];
        ApplyTrialSettings(trial);

        if (dataLogger != null)
        {
            dataLogger.SetCurrentTrial(trial);
        }

        Debug.Log($"[TrialManager] Started Trial {trial.trialNumber} " +
                  $"| Distance: {trial.distance} | Side: {trial.targetSide}");
    }

    private void ApplyTrialSettings(Trial trial)
    {
        // Scale the triangle by distance
        float scale = trial.distance / 2f; // adjust if your prefab assumes 2m base
        trianglePrefab.localScale = new Vector3(scale, scale, scale);

        // Place sphere on correct side (left/right relative to pointer)
        Vector3 pointerPos = pointer.position;
        Vector3 offset = pointer.right * trial.distance;

        if (trial.targetSide == "Left")
            sphere.position = pointerPos - offset;
        else
            sphere.position = pointerPos + offset;
    }
}
