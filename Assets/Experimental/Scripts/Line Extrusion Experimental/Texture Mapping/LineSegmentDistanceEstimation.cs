using UnityEngine;
using BabyDinoHerd.Extrusion.Line.Geometry;

namespace BabyDinoHerd.Extrusion.Line.TextureMapping.Experimental
{
    [BabyDinoHerd.Experimental]
    public static class LineSegmentDistanceEstimation 
    {
        /// <summary>
        /// Gets the signed perpendicular distance from a test point (within the extrusion distance) to the closest point on a segmentwise-defined line.
        /// </summary>
        /// <param name="point">The test point</param>
        /// <param name="originalLinePointList">The original segmentwise-defined line</param>
        /// <param name="extrusionAmountAbs">The extrusion amount absolute value</param>
        /// <param name="distanceToClosestPoint">The Euclidean distance from the test point to the closest point</param>
        internal static float GetClosestPointSignedPerpendicularDistance(Vector2 point, SegmentwiseLinePointListUV originalLinePointList, float extrusionAmountAbs, out float distanceToClosestPoint)
        {
            Vector2 closestSegmentDifference; int closestSegmentIndex; float fractionAlongClosestSegment;
            var closestPointOnSegments = SegmentedLineUtil.ClosestPointAlongSegmentwiseLine(point, originalLinePointList, extrusionAmountAbs, out fractionAlongClosestSegment, out closestSegmentDifference, out closestSegmentIndex);
            Vector2 diffClosestToPoint = point - closestPointOnSegments.Point;
            distanceToClosestPoint = diffClosestToPoint.magnitude;
            float smallestSignedPerpendicularDistance = GetClosestPointSignedPerpendicularDistance(originalLinePointList.Points.Count, closestSegmentDifference, diffClosestToPoint, distanceToClosestPoint, closestSegmentIndex, fractionAlongClosestSegment);

            return smallestSignedPerpendicularDistance;
        }

        /// <summary>
        /// Gets the signed perpendicular distance from a test point (within the extrusion distance) to the closest point on a segmentwise-defined line.
        /// </summary>
        /// <param name="numOriginalLinePoints">Number of points in the segmentwise-defined line</param>
        /// <param name="closestSegmentDifference">Start-to-end vector of the segment containing the closest point</param>
        /// <param name="diffClosestToPoint">Vector from the closest point to the test point</param>
        /// <param name="distanceToClosestPoint">Distance from the closest point to the test point</param>
        /// <param name="closestSegmentIndex">Index of the segment on the line containing the closest point</param>
        /// <param name="fractionAlongClosestSegment">Fraction along the segment of the closest point</param>
        internal static float GetClosestPointSignedPerpendicularDistance(int numOriginalLinePoints, Vector2 closestSegmentDifference, Vector2 diffClosestToPoint, float distanceToClosestPoint, int closestSegmentIndex, float fractionAlongClosestSegment)
        {
            var normal = NormalUtil.NormalFromTangent(closestSegmentDifference.normalized);
            float smallestSignedPerpendicularDistance = distanceToClosestPoint * Mathf.Sign(Vector2.Dot(normal, diffClosestToPoint));
            //NB This is needed to handle cases of fan points in between two segments, but a different approach is needed at the line endpoint fans.

            bool inLineStartFan = closestSegmentIndex == 0 && fractionAlongClosestSegment <= 0;
            bool inLineEndFan = closestSegmentIndex == numOriginalLinePoints - 2 && fractionAlongClosestSegment >= 1;
            if (inLineStartFan || inLineEndFan)
            {
                smallestSignedPerpendicularDistance = Vector2.Dot(normal, diffClosestToPoint);
            }

            return smallestSignedPerpendicularDistance;
        }
    }
}