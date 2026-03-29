using ChilledLeves.Resources;
using System;
using System.Collections.Generic;
namespace ChilledLeves.Utilities;

public static partial class Utils
{
    private static readonly Random _random = new Random();

    public static Vector3 GetRandomFlightPosition(this GatheringNode nodeLocation, Vector3 playerPosition, float sectionSize = 15f)
    {
        Vector3 nodePos = nodeLocation.Position;
        float angleToPlayer = CalculateAngleToPlayer(nodePos, playerPosition);

        var (sectionMin, sectionMax) = GetNearestSection(
            nodeLocation.Flight_FanInfo.Fan_StartAngle,
            nodeLocation.Flight_FanInfo.Fan_EndAngle,
            angleToPlayer,
            sectionSize);

        float selectedAngle = RandomAngleInRange(sectionMin, sectionMax);
        float selectedDistance = _random.NextFloat(nodeLocation.Flight_FanInfo.Fan_DistanceMin, nodeLocation.Flight_FanInfo.Fan_DistanceMax);

        return CalculateFanPosition(nodePos, selectedAngle, selectedDistance, nodeLocation.Flight_FanInfo.Fan_Height);
    }

    public static Vector3 GetRandomGatherPosition(this GatheringNode nodeLocation, Vector3 playerPosition, float sectionSize = 15f)
    {
        Vector3 nodePos = nodeLocation.Position;
        float angleToPlayer = CalculateAngleToPlayer(nodePos, playerPosition);

        var (sectionMin, sectionMax) = GetNearestSection(
            nodeLocation.Gathering_FanInfo.Fan_StartAngle,
            nodeLocation.Gathering_FanInfo.Fan_EndAngle,
            angleToPlayer,
            sectionSize);

        float selectedAngle = RandomAngleInRange(sectionMin, sectionMax);
        float selectedDistance = _random.NextFloat(nodeLocation.Gathering_FanInfo.Fan_DistanceMin, nodeLocation.Gathering_FanInfo.Fan_DistanceMax);

        return CalculateFanPosition(nodePos, selectedAngle, selectedDistance, nodeLocation.Gathering_FanInfo.Fan_Height);
    }

    #region Helper Methods

    private static float CalculateAngleToPlayer(Vector3 nodePos, Vector3 playerPos)
    {
        Vector3 direction = playerPos - nodePos;
        float angle = MathF.Atan2(direction.X, direction.Z) * (180f / MathF.PI);
        angle = 180f - angle;

        if (angle < 0f)
            angle += 360f;
        else if (angle >= 360f)
            angle -= 360f;

        return angle;
    }

    private static float NormalizeAngle(float angle)
    {
        angle = angle % 360f;
        if (angle < 0f)
            angle += 360f;
        return angle;
    }

    private static float GetRangeSpan(float min, float max)
    {
        float diff = MathF.Abs(max - min);
        if (MathF.Abs(diff - 360f) < 0.01f)
            return 360f;

        min = NormalizeAngle(min);
        max = NormalizeAngle(max);

        float span = max - min;
        if (span < 0)
            span += 360f;

        return span;
    }

    private static (float sectionMin, float sectionMax) GetNearestSection(float allowedMin, float allowedMax, float targetAngle, float sectionSize)
    {
        float rangeSpan = GetRangeSpan(allowedMin, allowedMax);

        if (rangeSpan >= 359.9f)
        {
            float halfSection = sectionSize / 2f;
            return (targetAngle - halfSection, targetAngle + halfSection);
        }

        allowedMin = NormalizeAngle(allowedMin);
        allowedMax = NormalizeAngle(allowedMax);
        targetAngle = NormalizeAngle(targetAngle);

        float closestPointInRange;

        if (IsAngleInRange(targetAngle, allowedMin, allowedMax))
        {
            closestPointInRange = targetAngle;
        }
        else
        {
            float distToMin = GetAngularDistance(targetAngle, allowedMin);
            float distToMax = GetAngularDistance(targetAngle, allowedMax);
            closestPointInRange = distToMin < distToMax ? allowedMin : allowedMax;
        }

        float halfSectionSize = sectionSize / 2f;
        float finalSectionMin = closestPointInRange - halfSectionSize;
        float finalSectionMax = closestPointInRange + halfSectionSize;

        finalSectionMin = ClampAngleToRange(finalSectionMin, allowedMin, allowedMax, true);
        finalSectionMax = ClampAngleToRange(finalSectionMax, allowedMin, allowedMax, false);

        return (finalSectionMin, finalSectionMax);
    }

    private static float ClampAngleToRange(float angle, float allowedMin, float allowedMax, bool preferMin)
    {
        angle = NormalizeAngle(angle);

        if (IsAngleInRange(angle, allowedMin, allowedMax))
            return angle;

        float distToMin = GetAngularDistance(angle, allowedMin);
        float distToMax = GetAngularDistance(angle, allowedMax);

        if (MathF.Abs(distToMin - distToMax) < 0.01f)
            return preferMin ? allowedMin : allowedMax;

        return distToMin < distToMax ? allowedMin : allowedMax;
    }

    private static float GetAngularDistance(float angle1, float angle2)
    {
        angle1 = NormalizeAngle(angle1);
        angle2 = NormalizeAngle(angle2);

        float diff = angle2 - angle1;

        while (diff > 180f) diff -= 360f;
        while (diff < -180f) diff += 360f;

        return MathF.Abs(diff);
    }

    private static bool IsAngleInRange(float angle, float min, float max)
    {
        angle = NormalizeAngle(angle);
        min = NormalizeAngle(min);
        max = NormalizeAngle(max);

        float rangeSpan = max - min;
        if (rangeSpan < 0)
            rangeSpan += 360f;

        if (rangeSpan >= 360f)
            return true;

        if (min <= max)
        {
            return angle >= min && angle <= max;
        }
        else
        {
            return angle >= min || angle <= max;
        }
    }

    private static float RandomAngleInRange(float min, float max)
    {
        min = NormalizeAngle(min);
        max = NormalizeAngle(max);

        if (min <= max)
        {
            return _random.NextFloat(min, max);
        }
        else
        {
            float rangeSize = (360f - min) + max;
            float randomValue = _random.NextFloat(0, rangeSize);
            float result = min + randomValue;
            return NormalizeAngle(result);
        }
    }

    private static Vector3 CalculateFanPosition(Vector3 center, float angleDegrees, float distance, float heightOffset)
    {
        float standardAngle = 180f - angleDegrees;
        float angleRadians = standardAngle * (MathF.PI / 180f);

        return new Vector3(
            center.X + distance * MathF.Sin(angleRadians),
            center.Y + heightOffset,
            center.Z + distance * MathF.Cos(angleRadians)
        );
    }

    private static Vector3 GetNearestWalkablePosition(List<Vector3> walkablePositions, Vector3 playerPosition)
    {
        Vector3 nearest = walkablePositions[0];
        float minDistSq = Vector3.DistanceSquared(nearest, playerPosition);

        for (int i = 1; i < walkablePositions.Count; i++)
        {
            Vector3 pos = walkablePositions[i];
            float distSq = Vector3.DistanceSquared(pos, playerPosition);

            if (distSq < minDistSq)
            {
                minDistSq = distSq;
                nearest = pos;
            }
        }

        return nearest;
    }

    private static float NextFloat(this Random random, float min, float max)
    {
        return min + (float)random.NextDouble() * (max - min);
    }

    #endregion
}
