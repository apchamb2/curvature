using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExperimentManager : MonoBehaviour
{
    [Header("References")]
    public TrialManager trialManager;
    public Transform pointer;
    public string subjectID = "P001";

    [Header("Distances (meters)")]
    public List<float> distances = new List<float> { 2f, 4f, 6f, 8f, 10f, 15f, 20f };

    [Header("Repetitions per (distance, side) cell")]
    [Min(1)] public int repsPerCell = 5;

    [Header("End of Experiment UI")]
    public EndOfExperimentPopup endPopup;    
    public Behaviour[] disableOnEnd;

    private List<Trial> trials;
    private System.Random rng;

    private void OnEnable()
    {
        if (trialManager != null)
            trialManager.OnExperimentFinished += HandleExperimentFinished;
    }

    private void OnDisable()
    {
        if (trialManager != null)
            trialManager.OnExperimentFinished -= HandleExperimentFinished;
    }

    void Start()
    {
        GenerateTrials();
        ShuffleTrials();
        if (trialManager == null)
        {
            Debug.LogError("[ExperimentManager] TrialManager not assigned.");
            return;
        }
        trialManager.InitializeTrials(trials);

        Debug.Log($"[ExperimentManager] Started {trials.Count} randomized trials");
    }

    private void HandleExperimentFinished()
    {
        if (endPopup != null)
            endPopup.Show();

        else
            Debug.LogWarning("[ExperimentManager] End popup not assigned.");

        if (disableOnEnd != null)
        {
            foreach (var b in disableOnEnd)
            {
                if (b != null) b.enabled = false;
            }
        }
    }

    private void GenerateTrials()
    {
        trials = new List<Trial>();
        if (distances == null || distances.Count == 0)
        {
            Debug.LogError("[ExperimentManager] Distances list is empty!");
            return;
        }
        int conditionID = 1;

        foreach (float d in distances)
        {
            foreach (string side in new[] { "Left", "Right" })
            {
                for (int r = 1; r <= repsPerCell; r++)
                {
                    trials.Add(new Trial
                    {
                        conditionID = conditionID++, 
                        distance = d,
                        targetSide = side
                    });
                }
            }
        }
    }

    private void ShuffleTrials()
    {
        rng = new System.Random();

        for (int i = trials.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (trials[i], trials[j]) = (trials[j], trials[i]);
        }

        for (int i = 0; i < trials.Count; i++)
        {
            trials[i].trialNumber = i + 1;
        }
    }

    public void ApplyPointerRandomization(Trial trial)
    {
        if (pointer == null || trialManager == null || trialManager.sphere == null)
        {
            Debug.LogWarning("[ExperimentManager] Missing pointer or sphere reference!");
            return;
        }
        if (rng == null) rng = new System.Random();

        const float exclusion = 30f;

        // Reference: direction from pointer -> sphere 
        Vector3 toSphere = (trialManager.sphere.position - pointer.position);
        toSphere.y = 0f;
        toSphere.Normalize();

        float yaw;
        do
        {
            yaw = (float)rng.NextDouble() * 360f;

            // Aim direction for that yaw (experiment uses pointer.right as the aim axis)
            Vector3 aim = Quaternion.Euler(0f, yaw, 0f) * Vector3.right;
            aim.y = 0f;
            aim.Normalize();

            // Reject if too close to pointing at the sphere
            if (Vector3.Angle(aim, toSphere) >= exclusion)
                break;

        } while (true);

        pointer.rotation = Quaternion.Euler(0f, yaw, 0f);

        // Store *relative* start offset (signed), not world yaw
        Vector3 upAxis = -Vector3.up; 
        Vector3 finalAim = pointer.right; finalAim.y = 0; finalAim.Normalize();
        trial.startAngle = Vector3.SignedAngle(toSphere, finalAim, upAxis);
    }
}
