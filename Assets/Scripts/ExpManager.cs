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

    private List<Trial> trials;
    private System.Random rng;

    void Start()
    {
        GenerateTrials();
        ShuffleTrials();

        trialManager.InitializeTrials(trials);

        Debug.Log($"[ExperimentManager] Started {trials.Count} randomized trials");
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
        if (pointer == null)
        {
            Debug.LogWarning("[ExperimentManager] Pointer not assigned");
            return;
        }
        const float exclusion = 30f; // Exclude angles within ±30° of the target side
        
        // Compute zero line (pointer -> sphere direction)
        Vector3 toSphere = (trialManager.sphere.position - pointer.position).normalized;
        toSphere.y = 0f;
        toSphere.Normalize();

        // Convert to yaw angle
        float zeroYaw = Mathf.Atan2(toSphere.x, toSphere.z) * Mathf.Rad2Deg;
        float randomYaw;

        do
        {
            randomYaw = (float)rng.NextDouble() * 360f;
        }
        while (Mathf.Abs(Mathf.DeltaAngle(randomYaw, zeroYaw)) < exclusion);

        pointer.rotation = Quaternion.Euler(0f, randomYaw, 0f);
        trial.startAngle = randomYaw;
    }
}
