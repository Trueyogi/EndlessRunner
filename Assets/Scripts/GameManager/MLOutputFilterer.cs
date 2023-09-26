using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MLOutputFilterer : MonoBehaviour
{
    private static int pastFrameCount;
    private static Queue<Vector3>[] pastWristPoints = new Queue<Vector3>[2];
    private static Vector3[] avgWristPoint = new Vector3[2];
    private static KalmanFilter[] wristKalmanFilter = new KalmanFilter[2];

    public static void Initialize()
    {
        pastFrameCount = 3;
        for (int i = 0; i < pastWristPoints.Length; i++)
        {
            pastWristPoints[i] = new Queue<Vector3>();
        }

        for (int i = 0; i < wristKalmanFilter.Length; i++)
        {
            wristKalmanFilter[i] = new KalmanFilter();
        }
    }

    public static Vector3 ScaleHandOnEveryDistance(Vector3 shoulder, Vector3 hip, Vector3 wrist, Vector3 center, Vector3 radius)
    {
        var vectorToWrist = shoulder - wrist;
        var angle = Vector3.SignedAngle(radius, vectorToWrist, Vector3.forward);
        var x = radius.magnitude * Mathf.Cos(angle * Mathf.Deg2Rad) + center.x;
        var y = radius.magnitude * Mathf.Sin(angle * Mathf.Deg2Rad) + center.y;
        var wristPoint = new Vector3(x, y, wrist.z);
                
        var maxArmLength = (shoulder - hip).magnitude;
        var armLength = vectorToWrist.magnitude;
        var scaler = armLength / (maxArmLength * 1.3f);
        wristPoint = Vector3.Lerp(center, wristPoint, scaler);

        return wristPoint;
    }

    public static Vector3 RestrictHandMovement(Vector3 wristPoint, float leftRestrictive, float rightRestrictive, float upperRestrictive, float bottomRestrictive)
    {
        wristPoint.x = Mathf.Clamp(wristPoint.x, leftRestrictive, rightRestrictive);
        wristPoint.y = Mathf.Clamp(wristPoint.y, bottomRestrictive, upperRestrictive);
        //wristPoint.z = GoalController.instance.topRight.position.z;
        
        return wristPoint;
    }

    public static Quaternion CalculateHandRotation(Vector3 wrist, Vector3 index)
    {
        var vectorToIndex = index - wrist;
        float targetAngle = -Mathf.Atan2(vectorToIndex.x, vectorToIndex.y) * Mathf.Rad2Deg;
        targetAngle = Mathf.Clamp(targetAngle, -90, 90);
        return Quaternion.Euler(0f, 0f,targetAngle);
    }

    public static Vector3 SmoothHandMovementKalman(int hand, Vector3 wrist)
    {
        var lastFrameWristPos = avgWristPoint[hand];
        var wristPointsList = new List<Vector3>(pastWristPoints[hand]);
        var smoothedWristPos = wristKalmanFilter[hand].Update(wristPointsList);
        avgWristPoint[hand] = smoothedWristPos;
        if (pastWristPoints[hand].Count == pastFrameCount)
        {
            pastWristPoints[hand].Dequeue();
        }
        pastWristPoints[hand].Enqueue(wrist);
        
        var interpolant = 50 * Time.deltaTime;
        smoothedWristPos = Vector3.Lerp(lastFrameWristPos, smoothedWristPos, interpolant);
        return smoothedWristPos;
    }
    
    public static Vector3 SmoothHandMovementMovingAvg(int hand, Vector3 wrist)
    {
        var lastFrameWristPos = avgWristPoint[hand];
        if (pastWristPoints[hand].Count == pastFrameCount)
        {
            var oldestPos = pastWristPoints[hand].Dequeue();
            avgWristPoint[hand] -= oldestPos / pastFrameCount;
        }

        pastWristPoints[hand].Enqueue(wrist);
        avgWristPoint[hand] += wrist / pastFrameCount;
        
        var interpolant = 50 * Time.deltaTime;
        var smoothedWristPos = Vector3.Lerp(lastFrameWristPos, avgWristPoint[hand], interpolant);

        return smoothedWristPos;
    }
}
