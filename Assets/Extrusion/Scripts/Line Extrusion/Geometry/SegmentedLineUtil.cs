using UnityEngine;
using System;

namespace BabyDinoHerd.Extrusion.Line.Geometry
{
    /// <summary>
    /// Utility methods for lines defined segmentwise.
    /// </summary>
    public class SegmentedLineUtil
    {
        #region point-to-line-distance

        /// <summary>
        /// Determines if an extruded point is closer to a line (on a segment-by-segment basis) than the extrusion amount.
        /// </summary>
        /// <param name="extrudedPoint">Extruded test point</param>
        /// <param name="linePointList">List of points defining the line</param>
        /// <param name="extrusionAmount">The extrusion amount</param>
        internal static bool IsCloserThanExtrusionDistance_Segmentwise(ExtrudedPointUV extrudedPoint, SegmentwiseLinePointListUV linePointList, float extrusionAmount)
        {
            var linePoints = linePointList.Points;
            var linePointsMaximumDelta = linePointList.MaxSegmentDistance;

            var numSegments = linePoints.Count - 1;

            Vector2 point = extrudedPoint.Point;
            var segmentIndex = Math.Max(0, Math.Min(numSegments - 1, extrudedPoint.LinePointSegmentIndex));
            var segmentIndex2 = Math.Max(0, Math.Min(numSegments - 1, extrudedPoint.LinePointSegmentIndex2));
            bool isCloser = false;
            var extrusionAmountAbs = Mathf.Abs(extrusionAmount);
            var extrusionAmountSquared = extrusionAmount * extrusionAmount;

            //We'll look every *numSteps* points to see what's closest. We will increase it if the line point is much further away than the extrusion distance.
            int numSteps = 1;

            for (int i = 0; i < numSegments; i += numSteps)
            {
                if (i != segmentIndex && i != segmentIndex2)
                {
                    isCloser = LineSegmentUtil.IsPointWithinDistanceOfSegment(linePoints[i].Point, linePoints[i + 1].Point, point, extrusionAmountSquared);
                }
                if (isCloser)
                {
                    break;
                }
                else
                {
                    var distanceDiff = (linePoints[i].Point - point).magnitude - extrusionAmountAbs;
                    numSteps = Mathf.FloorToInt(Mathf.Max(1f, distanceDiff / linePointsMaximumDelta));
                }
            }

            return isCloser;
        }

        #endregion

        #region closest point

        /// <summary>
        /// Determine the segment index of the closest point on a segmentwise-defined line to a test point
        /// </summary>
        /// <param name="point">The test point</param>
        /// <param name="linePointList">List of points defining the line</param>
        /// <param name="extrusionAmountAbs">Absolute value of the extrusion distance</param>
        internal static int ClosestIndexAlongSegmentwiseLine(Vector2 point, SegmentwiseLinePointListUV linePointList, float extrusionAmountAbs)
        {
            float fractionAlongClosestSegment;
            Vector2 closestSegmentDifference;
            int closestSegmentIndex;
            ClosestPointAlongSegmentwiseLine(point, linePointList, extrusionAmountAbs, GetVector, GetAverage, out fractionAlongClosestSegment, out closestSegmentDifference, out closestSegmentIndex);
            return closestSegmentIndex;
        }

        /// <summary>
        /// Determine the closest point on a segmentwise-defined line to a test point
        /// </summary>
        /// <param name="point">The test point</param>
        /// <param name="linePointList">List of points defining the line</param>
        /// <param name="extrusionAmountAbs">Absolute value of the extrusion distance</param>
        /// <param name="closestSegmentDifference">Start-to-end vector of the segment containing the closest point</param>
        /// <param name="closestSegmentIndex">Index of the closest segment.</param>
        internal static Vector2WithUV ClosestPointAlongSegmentwiseLine(Vector2 point, SegmentwiseVector2WithUVList linePointList, float extrusionAmountAbs, out Vector2 closestSegmentDifference, out int closestSegmentIndex)
        {
            float fractionAlongClosestSegment;
            return ClosestPointAlongSegmentwiseLine(point, linePointList, extrusionAmountAbs, GetVector, GetAverage, out fractionAlongClosestSegment, out closestSegmentDifference, out closestSegmentIndex);
        }

        /// <summary> Gets the average of two <see cref="Vector2WithUV"/>.</summary>
        /// <param name="firstVector">The first vector</param>
        /// <param name="secondVector">The second vector</param>
        /// <param name="fractionOfSecond"> The weight of the second vector in the average</param>
        private static Vector2WithUV GetAverage(Vector2WithUV firstVector, Vector2WithUV secondVector, float fractionOfSecond)
        {
            return firstVector.AverageWith(secondVector, fractionOfSecond);
        }

        /// <summary> Gets the position of a <see cref="Vector2WithUV"/>.</summary>
        /// <param name="vector2WithUV">The <see cref="Vector2WithUV"/>.</param>
        private static Vector2 GetVector(Vector2WithUV vector2WithUV)
        {
            return vector2WithUV.Vector;
        }

        /// <summary>
        /// Determine the closest point on a segmentwise-defined line to a test point
        /// </summary>
        /// <param name="point">The test point</param>
        /// <param name="linePointList">List of points defining the line</param>
        /// <param name="extrusionAmountAbs">Absolute value of the extrusion distance</param>
        /// <param name="closestSegmentDifference">Start-to-end vector of the segment containing the closest point</param>
        /// <param name="closestSegmentIndex">Index of the closest segment.</param>
        internal static Vector2WithUV ClosestPointAlongSegmentwiseLine(Vector2 point, SegmentwiseExtrudedPointListUV linePointList, float extrusionAmountAbs, out Vector2 closestSegmentDifference, out int closestSegmentIndex)
        {
            float fractionAlongClosestSegment;
            var closestExtrudedPoint = ClosestPointAlongSegmentwiseLine(point, linePointList, extrusionAmountAbs, GetVector, GetAverage, out fractionAlongClosestSegment, out closestSegmentDifference, out closestSegmentIndex);
            return new Vector2WithUV(closestExtrudedPoint);
        }

        /// <summary> Gets the average of two <see cref="ExtrudedPointUV"/>, not really caring about the line point segment indexes for the purposes of ClosestPointAlongSegmentwiseLine.</summary>
        /// <param name="firstVector">The first vector</param>
        /// <param name="secondVector">The second vector</param>
        /// <param name="fractionOfSecond"> The weight of the second vector in the average</param>
        private static ExtrudedPointUV GetAverage(ExtrudedPointUV firstVector, ExtrudedPointUV secondVector, float fractionOfSecond)
        {
            var averagedLinePoint = new LinePointUV(firstVector).AverageWith(new LinePointUV(secondVector), fractionOfSecond);
            return new ExtrudedPointUV(averagedLinePoint, firstVector.LinePointSegmentIndex, firstVector.LinePointSegmentIndex2);
        }

        /// <summary> Gets the position of a <see cref="ExtrudedPointUV"/>.</summary>
        /// <param name="vector2WithUV">The <see cref="ExtrudedPointUV"/>.</param>
        private static Vector2 GetVector(ExtrudedPointUV vector2WithUV)
        {
            return vector2WithUV.Point;
        }

        /// <summary>
        /// Determine the closest point on a segmentwise-defined line to a test point
        /// </summary>
        /// <param name="point">The test point</param>
        /// <param name="linePointList">List of points defining the line</param>
        /// <param name="extrusionAmountAbs">Absolute value of the extrusion distance</param>
        /// <param name="fractionAlongClosestSegment">Fraction along segment containing the closest point.</param>
        /// <param name="closestSegmentDifference">Start-to-end vector of the segment containing the closest point</param>
        /// <param name="closestSegmentIndex">Index of the closest segment.</param>
        internal static LinePointUV ClosestPointAlongSegmentwiseLine(Vector2 point, SegmentwiseLinePointListUV linePointList, float extrusionAmountAbs, out float fractionAlongClosestSegment, out Vector2 closestSegmentDifference, out int closestSegmentIndex)
        {
            return ClosestPointAlongSegmentwiseLine(point, linePointList, extrusionAmountAbs, GetVector, GetAverage, out fractionAlongClosestSegment, out closestSegmentDifference, out closestSegmentIndex);
        }

        /// <summary>
        /// Determine the closest point on a segmentwise-defined line to a test point
        /// </summary>
        /// <param name="point">The test point</param>
        /// <param name="linePointList">List of points defining the line</param>
        /// <param name="extrusionAmountAbs">Absolute value of the extrusion distance</param>
        /// <param name="closestSegmentDifference">Start-to-end vector of the segment containing the closest point</param>
        /// <param name="closestSegmentIndex">Index of the closest segment.</param>
        internal static LinePointUV ClosestPointAlongSegmentwiseLine(Vector2 point, SegmentwiseLinePointListUV linePointList, float extrusionAmountAbs, out Vector2 closestSegmentDifference, out int closestSegmentIndex)
        {
            float fractionAlongClosestSegment;
            return ClosestPointAlongSegmentwiseLine(point, linePointList, extrusionAmountAbs, GetVector, GetAverage, out fractionAlongClosestSegment, out closestSegmentDifference, out closestSegmentIndex);
        }

        /// <summary> Gets the average of two <see cref="LinePointUV"/>.</summary>
        /// <param name="firstVector">The first vector</param>
        /// <param name="secondVector">The second vector</param>
        /// <param name="fractionOfSecond"> The weight of the second vector in the average</param>
        private static LinePointUV GetAverage(LinePointUV firstVector, LinePointUV secondVector, float fractionOfSecond)
        {
            return firstVector.AverageWith(secondVector, fractionOfSecond);
        }

        /// <summary> Gets the position of a <see cref="LinePointUV"/>.</summary>
        /// <param name="linePointUv">The <see cref="LinePointUV"/>.</param>
        private static Vector2 GetVector(LinePointUV linePointUv)
        {
            return linePointUv.Point;
        }

        /// <summary>
        /// Determine the closest point on a segmentwise-defined line to a test point
        /// </summary>
        /// <param name="point">The test point</param>
        /// <param name="linePointList">List of points defining the line</param>
        /// <param name="extrusionAmountAbs">Absolute value of the extrusion distance</param>
        /// <param name="closestSegmentDifference">Start-to-end vector of the segment containing the closest point</param>
        /// <param name="getPoint">Deleate to return the position <see cref="Vector2"/> from the point type <typeparamref name="T"/></param>
        /// <param name="average">Delegate to average two points of type <typeparamref name="T"/></param>
        private static T ClosestPointAlongSegmentwiseLine<T>(Vector2 point, SegmentwisePointList<T> linePointList, float extrusionAmountAbs, Func<T, Vector2> getPoint, Func<T, T, float, T> average, out float fractionAlongClosestSegment, out Vector2 closestSegmentDifference, out int closestSegmentIndex) where T : struct
        {
            var linePoints = linePointList.Points;
            var linePointsMaximumDelta = linePointList.MaxSegmentDistance;
            var numSegments = linePoints.Count - 1;

            float minDistanceSquared = float.MaxValue;
            T closestPoint = new T();
            closestSegmentDifference = Vector2.zero;
            closestSegmentIndex = 0;
            fractionAlongClosestSegment = 0f;

            //We'll look every *numSteps* points to see what's closest. We will increase it if the line point is much further away than the extrusion distance.
            int numSteps = 1;

            for (int i = 0; i < numSegments; i += numSteps)
            {
                float distanceSquaredToSegment;

                var segmentStart = linePoints[i];
                var segmentEnd = linePoints[i + 1];
                Vector2 startVector = getPoint(segmentStart);
                Vector2 endVector = getPoint(segmentEnd);
                var currentClosestFractionToSegment = LineSegmentUtil.ClosestFractionAlongSegment(startVector, endVector, point, out distanceSquaredToSegment);

                if (distanceSquaredToSegment < minDistanceSquared)
                {
                    minDistanceSquared = distanceSquaredToSegment;
                    closestPoint = average(segmentStart, segmentEnd, currentClosestFractionToSegment);
                    fractionAlongClosestSegment = currentClosestFractionToSegment;
                    closestSegmentDifference = endVector - startVector;
                    closestSegmentIndex = i;
                }

                var distanceDiff = Math.Max(Math.Sqrt(distanceSquaredToSegment) - extrusionAmountAbs, 0);
                numSteps = Mathf.FloorToInt(Mathf.Max(1f, (float)distanceDiff / linePointsMaximumDelta));
            }

            return closestPoint;
        }

        #endregion

    }
}