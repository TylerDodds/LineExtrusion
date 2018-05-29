using System.Collections.Generic;
using UnityEngine;
using System;
using BabyDinoHerd.Extrusion.Line.Geometry;

namespace BabyDinoHerd.Extrusion.Line.Curvature.Experimental
{
    /// <summary>
    /// Determines different curvature measures or curvature analogues for a line at a particular point.
    /// </summary>
    [BabyDinoHerd.Experimental]
    public class CurvatureDetermination
    {
        /// <summary>
        /// Determines the the average U parater of the UV coordinates of a segment of <see cref="Vector2WithUV"/>.
        /// </summary>
        /// <param name="linePoints">The line points</param>
        /// <param name="index">The index of the start point in the segment.</param>
        public static float GetSegmentAverageUParameter(IList<Vector2WithUV> linePoints, int index)
        {
            var prev = linePoints[index];
            var next = linePoints[index + 1];
            return prev.AverageWith(next, 0.5f).UV.x;
        }

        /// <summary>
        /// Get the sign of the curvature at a point on a line, defined by the two segments it is connected to.
        /// </summary>
        /// <param name="linePoints">The line points</param>
        /// <param name="startingIndex">The index of the point</param>
        public static int CurvatureSign_NoIndexCheck(IList<Vector2WithUV> linePoints, int startingIndex)
        {
            var pPrev = linePoints[startingIndex - 1];
            var pCurr = linePoints[startingIndex + 0];
            var pNext = linePoints[startingIndex + 1];

            var diffPrev = pCurr.Vector - pPrev.Vector;
            var diffNext = pNext.Vector - pCurr.Vector;
            var unnormalizedCross2D = Vector2Util.Cross2D(diffPrev, diffNext);//These differences are unscaled tangent vectors.

            return Math.Sign(unnormalizedCross2D);
        }

        /// <summary>
        /// Get the curvature angle at a point on a line, defined by the two segments it is connected to.
        /// </summary>
        /// <param name="linePoints">The line points</param>
        /// <param name="startingIndex">The index of the point</param>
        public static float CurvatureAngle_NoIndexCheck(IList<Vector2WithUV> linePoints, int middlePointIndex)
        {
            var pPrev = linePoints[middlePointIndex - 1].Vector;
            var pCurr = linePoints[middlePointIndex + 0].Vector;
            var pNext = linePoints[middlePointIndex + 1].Vector;

            return CurvatureAngleFromPoints(pPrev, pCurr, pNext);
        }

        /// <summary>
        /// Get the curvature angle at a point on a line, defined by the two segments it is connected to.
        /// </summary>
        /// <param name="linePoints">The line points</param>
        /// <param name="startingIndex">The index of the point</param>
        public static float CurvatureAngle_NoIndexCheck(IList<LinePointUV> linePoints, int middlePointIndex)
        {
            var pPrev = linePoints[middlePointIndex - 1].Point;
            var pCurr = linePoints[middlePointIndex + 0].Point;
            var pNext = linePoints[middlePointIndex + 1].Point;

            return CurvatureAngleFromPoints(pPrev, pCurr, pNext);
        }

        /// <summary>
        /// Get the curvature angle defined by two segments of a start, middle, and end point.
        /// </summary>
        /// <param name="startPoint">The start point of the first segment.</param>
        /// <param name="middlePoint">The mid-point common to both segments.</param>
        /// <param name="endPoint">The end point of the second segment.</param>
        private static float CurvatureAngleFromPoints(Vector2 startPoint, Vector2 middlePoint, Vector2 endPoint)
        {
            var diffPrev = middlePoint - startPoint;
            var diffNext = endPoint - middlePoint;

            Vector2 diffPrevNormalized = Normalized(diffPrev);
            Vector2 diffNextNormalized = Normalized(diffNext);

            var cross2D = Vector2Util.Cross2D(diffPrevNormalized, diffNextNormalized);//When normalized, THESE DIFFERENCES are tangent vectors.

            var angleDiff = Mathf.Asin(Mathf.Clamp(cross2D, -1, 1));

            return angleDiff;
        }

        /// <summary>
        /// Normalize a vector, performing double-precision calculation in case built-in <see cref="Vector2.normalized"/> returns zero.
        /// </summary>
        /// <param name="vector">The vector to normalize.</param>
        private static Vector2 Normalized(Vector2 vector)
        {
            var normalized = vector.normalized;
            if(normalized.sqrMagnitude == 0)
            {
                double mag = vector.magnitude;
                double x = vector.x / mag;
                double y = vector.y / mag;
                normalized = new Vector2((float)x, (float)y);
            }
            return normalized;
        }
    }
}