



float GetAngle(Vector3 a, Vector3 b, Vector3 c)
{
    Vector3 ab = a - b;
    Vector3 cb = c - b;
    return Vector3.Angle(ab, cb); // returns angle at point B in degrees
}


float angleA = GetAngle(pointB, pointA, pointC);
float angleB = GetAngle(pointA, pointB, pointC);
float angleC = GetAngle(pointA, pointC, pointB);
float angleSum = angleA + angleB + angleC;


string logLine = $"{Time.time},{angleA},{angleB},{angleC},{angleSum}";
File.AppendAllText("experiment_log.csv", logLine + "\n");
