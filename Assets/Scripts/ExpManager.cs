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
    public int minDistance = 2;
    public int maxDistance = 20;
    public int step = 2;

    [Header("Repetitions per (distance, side) cell")]
    [Min(1)] public int repsPerCell = 3;

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
        var distances = Enumerable.Range(0, ((maxDistance - minDistance) / step) + 1)
                                  .Select(i => minDistance + i * step)
                                  .Select(d => (float)d);

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
            Debug.LogWarning("[ExperimentManager] Pointer not assigned!");
            return;
        }

        float randomAngle = (float)rng.NextDouble() * 360f;
        pointer.rotation = Quaternion.Euler(0f, randomAngle, 0f);

        trial.startAngle = randomAngle;
    }
}
