using UnityEngine;

public class EnvironmentLoader : MonoBehaviour
{
    public GameObject environmentPrefab;

    void Start()
    {
        // Instantiate at world origin
        Instantiate(environmentPrefab, Vector3.zero, Quaternion.identity);
    }
}
