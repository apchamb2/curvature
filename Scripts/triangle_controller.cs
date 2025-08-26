using System.IO;
using UnityEngine;

public class TriangleController : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public Transform pointC;

    public Transform xrCamera; // Assign the Main Camera (HMD)
    public string logFileName = "angle_log.csv";

    public float triangleDepth = 2.0f;  // Distance in front of player (Z)
    public float triangleWidth = 0.5f;  // Horizontal spacing

    void Start()
    {
        if (xrCamera == null)
        {
            xrCamera = Camera.main.transform; // fallback
        }

        // Set triangle at user’s eye height
        float eyeY = xrCamera.position.y;
        Vector3 origin = xrCamera.position + xrCamera.forward * triangleDepth;

        // Position points relative to eye level and forward direction
        Vector3 center = new Vector3(origin.x, eyeY, origin.z);
        pointA.position = center + Vector3.left * triangleWidth;
        pointB.position = center + Vector3.right * triangleWidth;
        pointC.position = center + Vector3.up * 0.3f;  // Adjustable point slightly above

        // Create log file with header
        string path = Path.Combine(Application.persistentDataPath, logFileName);
        if (!File.Exists(path))
        {
            File.AppendAllText(path, "Time,AngleA,AngleB,AngleC,Sum,A.x,A.y,A.z,B.x,B.y,B.z,C.x,C.y,C.z\n");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LogAngles();
        }
    }

    void LogAngles()
    {
        Vector3 A = pointA.position;
        Vector3 B = pointB.position;
        Vector3 C = pointC.position;

        float angleA = GetAngle(B, A, C);
        float angleB = GetAngle(A, B, C);
        float angleC = GetAngle(A, C, B);
        float angleSum = angleA + angleB + angleC;

        string line = $"{Time.time:F3},{angleA:F2},{angleB:F2},{angleC:F2},{angleSum:F2}," +
                      $"{A.x:F3},{A.y:F3},{A.z:F3}," +
                      $"{B.x:F3},{B.y:F3},{B.z:F3}," +
                      $"{C.x:F3},{C.y:F3},{C.z:F3}";

        string path = Path.Combine(Application.persistentDataPath, logFileName);
        File.AppendAllText(path, line + "\n");

        Debug.Log("Angles logged:\n" + line);
    }

    float GetAngle(Vector3 from, Vector3 vertex, Vector3 to)
    {
        Vector3 dir1 = from - vertex;
        Vector3 dir2 = to - vertex;
        return Vector3.Angle(dir1, dir2); // in degrees
    }
}
