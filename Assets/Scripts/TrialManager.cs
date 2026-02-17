using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TrialManager : MonoBehaviour
{
    [Header("References")]
    public DataLogger dataLogger;
    public ExperimentManager experimentManager;
    public Transform trianglePrefab; // parent ( to find barycenter of the triangle)
    public Transform pointer;
    public Transform sphere;
    public Transform vertex;
    public Transform hmdCamera;

    [Header("Input")]
    public InputActionProperty triggerAction;

    //[Header("Visual Angle Scaling")]
    //[Range(1f, 60f)]
    //public float visualAngleDegrees = 10f;
    //public float baseSphereSize = 0.5f;      // diameter of sphere at localScale = 1 (prefab actual size is 0.5m)
    //public float basePointerSize = 0.5f;        // same as above but witht the cube

    private List<Trial> trials;
    private int currentTrialIndex = -1;
    private bool trialCompleted = false;

    private void Update()
    {
        if (triggerAction != null && triggerAction.action != null && triggerAction.action.WasPressedThisFrame())
        {
            if (trialCompleted)
            {
                NextTrial();
            }
            else
            {
                if (trialCompleted)
                {
                    NextTrial();
                }
                else
                {
                    if (dataLogger != null)
                        dataLogger.FinalizeTrialAndWrite();  // this will call MarkTrialAsCompleted() on success
                    else
                        Debug.LogWarning("[TrialManager] DataLogger not assigned.");
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
        // ScaleToVisualAngle(); 

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

        // Center is the barycenter (trianglePrefab)
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
    //private void ScaleToVisualAngle()
    //{
    //    if (hmdCamera == null || pointer == null || sphere == null || vertex == null)
    //    {
    //        Debug.LogWarning("[TrialManager] Missing references for scaling.");
    //        return;
    //    }
    //    float thetaRad = visualAngleDegrees * Mathf.Deg2Rad * 0.5f; // half angle in radians

    //    float dPointer = Vector3.Distance(pointer.position, hmdCamera.position);
    //    float dSphere = Vector3.Distance(sphere.position, hmdCamera.position);
    //    float dVertex = Vector3.Distance(vertex.position, hmdCamera.position);

    //    float scalePointer = 2f * dPointer * Mathf.Tan(thetaRad) / basePointerSize;
    //    float scaleSphere = 2f * dSphere * Mathf.Tan(thetaRad) / baseSphereSize;
    //    float scaleVertex = 2f * dVertex * Mathf.Tan(thetaRad) / baseSphereSize;

    //    pointer.localScale = Vector3.one * scalePointer;
    //    sphere.localScale = Vector3.one * scaleSphere;
    //    vertex.localScale = Vector3.one * scaleVertex;
    //}
}
