using UnityEngine;
using System.Collections.Generic;
using BabyDinoHerd.Extrusion.Line.Geometry;
using System;

namespace BabyDinoHerd.Extrusion.Line.Extrusion
{
    /// <summary>
    /// Class for removing <see cref="ChunkBetweenIntersections"/> that are too close to a line defined segmentwise (<see cref="SegmentwiseLinePointListUV"/>).
    /// </summary>
    public class SegmentedExtrudedChunkRemoval
    {
        /// <summary>
        /// Remove chunks that are too close to a line, from a list of all extruded chunks between intersections.
        /// Returns the removed chunks.
        /// </summary>
        /// <param name="extrudedChunksBetweenIntersections">Extruded chunks between extruded line intersection points. Chunks too close are removed from this list.</param>
        /// <param name="linePoints">The line against which chunks are compared by distance.</param>
        /// <param name="extrusionAmount">The extrusion amount.</param>
        /// <param name="intersectionPoints">The intersection points.</param>
        /// <returns>The removed chunks.</returns>
        internal static List<ChunkBetweenIntersections> RemoveChunksThatAreTooClose(List<ChunkBetweenIntersections> extrudedChunksBetweenIntersections, SegmentwiseLinePointListUV linePoints, float extrusionAmount, List<IntersectionPoint> intersectionPoints)
        {
            List<ChunkBetweenIntersections> removed = new List<ChunkBetweenIntersections>();
            for (int i = extrudedChunksBetweenIntersections.Count - 1; i >= 0; i--)
            {
                var thisChunk = extrudedChunksBetweenIntersections[i];
                var extractedIsTooClose = IsExtractedContinuousChunkooClose_DeterminedFromFurthestParameter(thisChunk, linePoints, intersectionPoints, extrusionAmount);
                if (extractedIsTooClose)
                {
                    removed.Add(thisChunk);
                    extrudedChunksBetweenIntersections.RemoveAt(i);
                }
            }

            removed.Reverse();
            return removed;
        }

        /// <summary>
        /// Determine if a chunk of extruded points is too close to a line of points, determined by comparing the extruded point whose parameter is furthest from any intersection points.
        /// </summary>
        /// <param name="extrudedContinuousChunk">Chunk of extruded points between two intersection endpoints.</param>
        /// <param name="linePoints">Points on the line from which to determine distance.</param>
        /// <param name="intersectionPoints">All intersection points between extruded segments.</param>
        /// <param name="extrusionAmount">The extrusion amount.</param>
        private static bool IsExtractedContinuousChunkooClose_DeterminedFromFurthestParameter(ChunkBetweenIntersections extrudedContinuousChunk, SegmentwiseLinePointListUV linePoints, IList<IntersectionPoint> intersectionPoints, float extrusionAmount)
        {
            bool tooClose;
            if (extrudedContinuousChunk.ExtrudedPoints.Count > 0)
            {
                var pointToCheck = PointOfLargestParameterDifference(extrudedContinuousChunk.ExtrudedPoints, intersectionPoints);
                tooClose = SegmentedLineUtil.IsCloserThanExtrusionDistance_Segmentwise(pointToCheck, linePoints, extrusionAmount);
            }
            else
            {
                tooClose = IsIntersectionPointChunkSegmentCloserThanExtrusionDistance(extrudedContinuousChunk.StartIntersection, extrudedContinuousChunk.EndIntersection, linePoints, extrusionAmount);
            }
            return tooClose;
        }

        #region point check

        /// <summary>
        /// Returns the extruded point which has the largest distance parameter-wise to the parameters of a set of intersection points.
        /// </summary>
        /// <param name="extrudedPoints">Extruded points</param>
        /// <param name="intersectionPoints">Intersection points</param>
        private static ExtrudedPointUV PointOfLargestParameterDifference(IList<ExtrudedPointUV> extrudedPoints, IList<IntersectionPoint> intersectionPoints)
        {
            float maxDist = float.NegativeInfinity;
            ExtrudedPointUV point = new ExtrudedPointUV();
            for (int i = 0; i < extrudedPoints.Count; i++)
            {
                var currentChunkPoint = extrudedPoints[i];
                var dist = SmallestParameterDifference(currentChunkPoint, intersectionPoints);
                if (dist > maxDist)
                {
                    maxDist = dist;
                    point = currentChunkPoint;
                }
            }
            return point;
        }


        /// <summary>
        /// Gets the smallest difference between parameter value of the given extruded point and the parameters of intersection points.
        /// </summary>
        /// <param name="point">The extruded point</param>
        /// <param name="intersectionPoints">The intersection points</param>
        private static float SmallestParameterDifference(ExtrudedPointUV point, IList<IntersectionPoint> intersectionPoints)
        {
            float minDist = float.PositiveInfinity;
            var parameter = point.Parameter;
            for (int i = 0; i < intersectionPoints.Count; i++)
            {
                var intersectionParameter = intersectionPoints[i].Parameter;
                var dist = Mathf.Abs(parameter - intersectionParameter);
                if (dist < minDist)
                {
                    minDist = dist;
                }
            }
            return minDist;
        }

        #endregion

        #region chunk-line distance check

        /// <summary>
        /// Determines if a chunk between intersection points that contains no other extruded points (ie. the chunk is a single line segment of beginning and ending with two intersection points) is closer than the extrusion distance to the original line.
        /// </summary>
        /// <param name="intersectionSegmentStart">Intersection point comprising the start of the segment.</param>
        /// <param name="intersectionSegmentEnd">Intersection point comprising the end of the segment.</param>
        /// <param name="originalLinePointList">List of original line points</param>
        /// <param name="extrusionAmount">The extrusion amount.</param>
        private static bool IsIntersectionPointChunkSegmentCloserThanExtrusionDistance(IntersectionPoint point, IntersectionPoint extrudedOther, SegmentwiseLinePointListUV originalLinePointList, float extrusionAmount)
        {
            return IsIntersectionPointChunkSegmentCloserThanExtrusionDistance_OriginalLineSegmentwise(point, extrudedOther, originalLinePointList, extrusionAmount);
        }

        /// <summary>
        /// Determines if a chunk between intersection points that contains no other extruded points (ie. the chunk is a single line segment of beginning and ending with two intersection points) is closer than the extrusion distance to the original line.
        /// Calculates based on the original line segmentwise.
        /// </summary>
        /// <param name="intersectionSegmentStart">Intersection point comprising the start of the segment.</param>
        /// <param name="intersectionSegmentEnd">Intersection point comprising the end of the segment.</param>
        /// <param name="originalLinePointList">List of original line points</param>
        /// <param name="extrusionAmount">The extrusion amount.</param>
        private static bool IsIntersectionPointChunkSegmentCloserThanExtrusionDistance_OriginalLineSegmentwise(IntersectionPoint intersectionSegmentStart, IntersectionPoint intersectionSegmentEnd, SegmentwiseLinePointListUV originalLinePointList, float extrusionAmount)
        {
            var linePoints = originalLinePointList.Points;
            var linePointsMaximumDelta = originalLinePointList.MaxSegmentDistance;

            var numSegments = linePoints.Count - 1;

            var segmentIndex1 = Math.Max(0, Math.Min(numSegments - 1, intersectionSegmentStart.FirstPointSegmentIndex));
            var segmentIndex2 = Math.Max(0, Math.Min(numSegments - 1, intersectionSegmentStart.FirstPointSegmentIndex2));
            var segmentIndex3 = Math.Max(0, Math.Min(numSegments - 1, intersectionSegmentStart.SecondPointSegmentIndex));
            var segmentIndex4 = Math.Max(0, Math.Min(numSegments - 1, intersectionSegmentStart.SecondPointSegmentIndex2));
            var segmentIndex5 = Math.Max(0, Math.Min(numSegments - 1, intersectionSegmentEnd.FirstPointSegmentIndex));
            var segmentIndex6 = Math.Max(0, Math.Min(numSegments - 1, intersectionSegmentEnd.FirstPointSegmentIndex2));
            var segmentIndex7 = Math.Max(0, Math.Min(numSegments - 1, intersectionSegmentEnd.SecondPointSegmentIndex));
            var segmentIndex8 = Math.Max(0, Math.Min(numSegments - 1, intersectionSegmentEnd.SecondPointSegmentIndex2));
            var distanceExtruded1 = LineSegmentUtil.DistanceToSegment(linePoints[segmentIndex1].Point, linePoints[segmentIndex1 + 1].Point, intersectionSegmentStart.Point);
            var distanceExtruded2 = LineSegmentUtil.DistanceToSegment(linePoints[segmentIndex2].Point, linePoints[segmentIndex2 + 1].Point, intersectionSegmentStart.Point);
            var distanceExtruded3 = LineSegmentUtil.DistanceToSegment(linePoints[segmentIndex3].Point, linePoints[segmentIndex3 + 1].Point, intersectionSegmentStart.Point);
            var distanceExtruded4 = LineSegmentUtil.DistanceToSegment(linePoints[segmentIndex4].Point, linePoints[segmentIndex4 + 1].Point, intersectionSegmentStart.Point);
            var distanceExtruded5 = LineSegmentUtil.DistanceToSegment(linePoints[segmentIndex5].Point, linePoints[segmentIndex5 + 1].Point, intersectionSegmentEnd.Point);
            var distanceExtruded6 = LineSegmentUtil.DistanceToSegment(linePoints[segmentIndex6].Point, linePoints[segmentIndex6 + 1].Point, intersectionSegmentEnd.Point);
            var distanceExtruded7 = LineSegmentUtil.DistanceToSegment(linePoints[segmentIndex7].Point, linePoints[segmentIndex7 + 1].Point, intersectionSegmentEnd.Point);
            var distanceExtruded8 = LineSegmentUtil.DistanceToSegment(linePoints[segmentIndex8].Point, linePoints[segmentIndex8 + 1].Point, intersectionSegmentEnd.Point);
            bool isCloser = false;
            var extrusionAmountAbs = Mathf.Abs(extrusionAmount);

            int numSteps = 1;//We'll look every numSteps points to see what's closest, moving more steps when possible.

            for (int i = 0; i < numSegments; i += numSteps)
            {
                var distanceBetweenSegments = LineSegmentUtil.DistanceBetweenSegments(intersectionSegmentStart.Point, intersectionSegmentEnd.Point, linePoints[i].Point, linePoints[i + 1].Point);
                bool useDefaultExtrusionAmount = true;
                var comparisonDistance = float.PositiveInfinity;
                if (i == segmentIndex1) { comparisonDistance = Math.Min(comparisonDistance, distanceExtruded1); useDefaultExtrusionAmount = false; }
                if (i == segmentIndex2) { comparisonDistance = Math.Min(comparisonDistance, distanceExtruded2); useDefaultExtrusionAmount = false; }
                if (i == segmentIndex3) { comparisonDistance = Math.Min(comparisonDistance, distanceExtruded3); useDefaultExtrusionAmount = false; }
                if (i == segmentIndex4) { comparisonDistance = Math.Min(comparisonDistance, distanceExtruded4); useDefaultExtrusionAmount = false; }
                if (i == segmentIndex5) { comparisonDistance = Math.Min(comparisonDistance, distanceExtruded5); useDefaultExtrusionAmount = false; }
                if (i == segmentIndex6) { comparisonDistance = Math.Min(comparisonDistance, distanceExtruded6); useDefaultExtrusionAmount = false; }
                if (i == segmentIndex7) { comparisonDistance = Math.Min(comparisonDistance, distanceExtruded7); useDefaultExtrusionAmount = false; }
                if (i == segmentIndex8) { comparisonDistance = Math.Min(comparisonDistance, distanceExtruded8); useDefaultExtrusionAmount = false; }
                if (useDefaultExtrusionAmount) { comparisonDistance = extrusionAmountAbs; }

                isCloser = distanceBetweenSegments < comparisonDistance;

                if (!useDefaultExtrusionAmount)
                {
                    if ((i == segmentIndex1 || i == segmentIndex2) && segmentIndex1 == segmentIndex3 && segmentIndex2 == segmentIndex4
                     || (i == segmentIndex5 || i == segmentIndex6) && segmentIndex5 == segmentIndex7 && segmentIndex6 == segmentIndex8)
                    {
                        isCloser = false;
                    }
                }

                if (isCloser)
                {
                    break;
                }
                else
                {
                    var distanceDiff = distanceBetweenSegments - extrusionAmountAbs;
                    numSteps = Mathf.FloorToInt(Mathf.Max(1f, distanceDiff / linePointsMaximumDelta));
                }
            }

            return isCloser;
        }

        #endregion
    }
}