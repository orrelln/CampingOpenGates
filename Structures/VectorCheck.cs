using System.Collections.Generic;
using UnityEngine;

using Exiled.API.Features;

public class VectorCheck
{
    public static bool IsWithinRange(Vector3 e, Vector3 i, Vector3 v, bool invert = false)
    {
        return (Mathf.Abs(e.x - i.x) > Mathf.Abs(e.z - i.z)) ^ invert
            ? i.x > e.x ? v.x >= e.x : v.x <= e.x
            : i.z > e.z ? v.z >= e.z : v.z <= e.z;
    }

    public static bool IsWithinRangeWithBoundaries(Vector3 e, Vector3 i, Vector3 v, float b1, float b2, bool invert = false)
    {
        return (Mathf.Abs(e.x - i.x) > Mathf.Abs(e.z - i.z)) ^ invert
            ? (i.x > e.x ? v.x >= e.x : v.x <= e.x) && (v.z >= i.z + b1 && v.z <= i.z + b2)
            : (i.z > e.z ? v.z >= e.z : v.z <= e.z) && (v.x >= i.x + b1 && v.x <= i.x + b2);
    }

    public static bool IsPastInternal(Vector3 e, Vector3 i, Vector3 v, bool invert = false)
    {
        return (Mathf.Abs(e.x - i.x) > Mathf.Abs(e.z - i.z)) ^ invert
            ? i.x > e.x ? v.x > i.x : v.x < i.x
            : i.z > e.z ? v.z > i.z : v.z < i.z;
    }

    public static bool IsPastInternalWithRoom(Vector3 e, Vector3 i, Vector3 v, float offset)
    {
        bool isDoorsAlignedAlongX = Mathf.Abs(e.x - i.x) > Mathf.Abs(e.z - i.z);
        bool pastDoors = isDoorsAlignedAlongX
            ? i.x > e.x ? v.x > i.x : v.x < i.x
            : i.z > e.z ? v.z > i.z : v.z < i.z;

        bool inRoomComplex = isDoorsAlignedAlongX
           ? i.x > e.x ? v.z < i.z - offset : v.z > i.z + offset
           : i.z > e.z ? v.x > i.x + offset : v.x < i.x - offset;

        return pastDoors || inRoomComplex;
    }

    public static bool IsPastInternalWithBoundaries(Vector3 e, Vector3 i, Vector3 v, float b1, float b2, bool invert = false)
    {
        return (Mathf.Abs(e.x - i.x) > Mathf.Abs(e.z - i.z)) ^ invert
        ? (i.x > e.x ? v.x > i.x : v.x < i.x) && (v.z >= i.z + b1 && v.z <= i.z + b2)
        : (i.z > e.z ? v.z > i.z : v.z < i.z) && (v.x >= i.x + b1 && v.x <= i.x + b2);
    }

    public static Vector3 SelectMostDissimilarVector(List<Vector3> vectors)
    {
        Vector3 mostDissimilar = vectors[0];
        float greatestMinDifference = 0;

        foreach (Vector3 v1 in vectors)
        {
            float minDifference = float.MaxValue;
            foreach (Vector3 v2 in vectors)
            {
                if (v1 != v2)
                {
                    float differenceX = Mathf.Abs(v1.x - v2.x);
                    float differenceZ = Mathf.Abs(v1.z - v2.z);
                    
                    minDifference = Mathf.Min(minDifference, Mathf.Min(differenceX, differenceZ));
                }
            }
            if (minDifference > greatestMinDifference)
            {
                greatestMinDifference = minDifference;
                mostDissimilar = v1;
            }
        }

        return mostDissimilar;
    }

    public static Vector3 SelectMostSimilarVector(Vector3 v1, List<Vector3> vectors)
    {
        Vector3 mostSimilar = vectors[0];
        float minDifference = float.MaxValue;
        foreach (Vector3 v2 in vectors)
        {
            float differenceX = Mathf.Abs(v1.x - v2.x);
            float differenceZ = Mathf.Abs(v1.z - v2.z);

            float difference = Mathf.Min(minDifference, Mathf.Min(differenceX, differenceZ));
            if (minDifference > difference)
            {
                minDifference = difference;
                mostSimilar = v2;
            }
        }

        return mostSimilar;
    }
}

