using System;
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

    [Header("Practice")]
    public bool enablePractice = true;
    [Min(0)] public int practiceTrialCount = 3;
    public List<float> practiceDistances = new List<float> { 6f, 10f, 4f }; 
    public bool randomizePracticeSide = true;

    public event Action<bool, int, int> OnTrialCounterChanged;
    public event Action OnExperimentFinished;
    
    public int TotalRealTrials => trials != null ? trials.Count : 0;
    public int TotalPracticeTrials => (enablePractice && practiceTrialCount > 0) ? practiceTrialCount : 0;
    
    private List<Trial> trials;
    private int currentTrialIndex = -1;
    private bool trialCompleted = false;
    private bool hasStarted = false;
    // practice state
    private bool inPractice = false;
    private int practiceIndex = 0;

    private bool experimentFinished = false;

    private void Start()
    {
        // Start with practice phase
        inPractice = enablePractice && practiceTrialCount > 0;
        practiceIndex = 0;
    }


    private void Update()
    {
        if (experimentFinished) return;
        if (triggerAction == null || triggerAction.action == null) return;

        if (!triggerAction.action.WasPressedThisFrame()) return;

        if (dataLogger == null)
        {
            Debug.LogWarning("[TrialManager] DataLogger not assigned.");
            return;
        }

        if (!trialCompleted)
        {
            dataLogger.FinalizeTrialAndWrite();
            return; 
        }

        NextTrial();
        
    }

    public void InitializeTrials(List<Trial> trialList)
    {
        trials = trialList;
        currentTrialIndex = -1;
    }
    
    public void StartExperiment()
    {
        if (hasStarted && !experimentFinished)
        {
            Debug.LogWarning("[TrialManager] StartExperiment() ignored: experiment already running.");
            return;
        }

        hasStarted = true;
        experimentFinished = false;
        trialCompleted = false;

        inPractice = enablePractice && practiceTrialCount > 0;
        practiceIndex = 0;

        currentTrialIndex = -1;

        NextTrial(); // starts practice or real trials and fires counter updates
    }

    private void NextTrial()
    {
      // PRACTICE
        if (inPractice)
        {
            if (practiceIndex >= practiceTrialCount)
            {
                // switch to real trials
                inPractice = false;
                currentTrialIndex = -1;
                Debug.Log("[TrialManager] Practice finished. Starting real experiment.");
                NextTrial();
                return;
            }

            Trial practiceTrial = MakePracticeTrial(practiceIndex);

            // Apply same environment logic
            ApplyTrialSettings(practiceTrial);

            // Same pointer randomization logic
            if (experimentManager != null)
                experimentManager.ApplyPointerRandomization(practiceTrial);

            // Tell DataLogger this is practice (skip CSV)
            if (dataLogger != null)
            {
                dataLogger.practiceMode = true;
                dataLogger.SetCurrentTrial(practiceTrial);
            }

            trialCompleted = false;

            Debug.Log($"[TrialManager] PRACTICE {practiceIndex + 1}/{practiceTrialCount} | Distance: {practiceTrial.distance} | Side: {practiceTrial.targetSide}");
            OnTrialCounterChanged?.Invoke(true, practiceIndex + 1, practiceTrialCount);
            practiceIndex++;
            return;
        }

        // REAL Trials
        currentTrialIndex++;

        if (trials == null || currentTrialIndex >= trials.Count)
        {
            FinishExperiment();
            return;
        }

        Trial trial = trials[currentTrialIndex];
        ApplyTrialSettings(trial);

        if (experimentManager != null)
            experimentManager.ApplyPointerRandomization(trial);

        if (dataLogger != null)
        {
            dataLogger.practiceMode = false; 
            dataLogger.SetCurrentTrial(trial);
        }
        trialCompleted = false;

        Debug.Log($"[TrialManager] Trial {trial.trialNumber} (Condition {trial.conditionID}) | Distance: {trial.distance} | Side: {trial.targetSide}");
        OnTrialCounterChanged?.Invoke(false, currentTrialIndex + 1, trials.Count);
    }

    public void MarkTrialAsCompleted()
    {
        trialCompleted = true;
        Debug.Log("[TrialManager] Data saved");

        if (trials == null || trials.Count == 0)
        {
            if (inPractice && !experimentFinished)
                NextTrial();
            return;
        }

        if (!inPractice && currentTrialIndex >= trials.Count - 1)
        {
            FinishExperiment();
            return;
        }

        if (!experimentFinished)
            NextTrial();
    }

    private Trial MakePracticeTrial(int index)
    {
        float d;

        if (practiceDistances == null || practiceDistances.Count == 0)
            d = 6f;
        else if (practiceDistances.Count == 1)
            d = practiceDistances[0];
        else
            d = practiceDistances[Mathf.Clamp(index, 0, practiceDistances.Count - 1)];

        string side;
        if (randomizePracticeSide)
            side = (UnityEngine.Random.value < 0.5f) ? "Left" : "Right";
        else
            side = "Left";

        return new Trial
        {
            conditionID = 0,
            trialNumber = index + 1,
            distance = d,
            targetSide = side,
            startAngle = 0f
        };
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

    private void FinishExperiment()
    {
        if (experimentFinished) return;

        experimentFinished = true;
        hasStarted = false;

        Debug.Log("[TrialManager] Experiment has ended");
        OnExperimentFinished?.Invoke();
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
