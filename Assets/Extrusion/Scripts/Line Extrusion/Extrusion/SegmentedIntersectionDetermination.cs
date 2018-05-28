using UnityEngine;
using BabyDinoHerd.Extrusion.Line.Geometry;
using System.Collections.Generic;

namespace BabyDinoHerd.Extrusion.Line.Extrusion
{
    public class SegmentedIntersectionDetermination
    {
        /// <summary>
        /// Determine points where an extruded line consisting of segments intersect, as well as the neighbouring pairs of those intersection points that serve as endpoints for chunks of extruded points that lie between intersections.
        /// </summary>
        /// <param name="extrudedPointList">The extruded points.</param>
        /// <param name="intersectionPoints">The intersection points.</param>
        /// <param name="chunkIntersectionEndpoints">Pairs of neighbouring intersection points, acting as chunk endpoints.</param>
        internal static void FindIntersectionPointsAndChunkEndpoints(SegmentwiseExtrudedPointListUV extrudedPointList, out List<IntersectionPoint> intersectionPoints, out List<IntersectionPointPair> chunkIntersectionEndpoints)
        {
            intersectionPoints = DetermineIntersectionPoints(extrudedPointList);

            var extrudedPoints = extrudedPointList.Points;
            chunkIntersectionEndpoints = new List<IntersectionPointPair>();
            var firstExtrudedPoint = extrudedPoints[0];
            var lastExtrudedPoint = extrudedPoints[extrudedPoints.Count - 1];
            var firstExtrudedPointAsIntersection = new IntersectionPoint(firstExtrudedPoint, firstExtrudedPoint, firstExtrudedPoint);
            var lastExtrudedPointAsIntersection = new IntersectionPoint(lastExtrudedPoint, lastExtrudedPoint, lastExtrudedPoint);
            if (intersectionPoints.Count > 0)
            {
                chunkIntersectionEndpoints.Add(new IntersectionPointPair(firstExtrudedPointAsIntersection, intersectionPoints[0]));
                for (int i = 0; i < intersectionPoints.Count - 1; i++)
                {
                    chunkIntersectionEndpoints.Add(new IntersectionPointPair(intersectionPoints[i], intersectionPoints[i + 1]));
                }
                chunkIntersectionEndpoints.Add(new IntersectionPointPair(intersectionPoints[intersectionPoints.Count - 1], lastExtrudedPointAsIntersection));
            }
            else
            {
                chunkIntersectionEndpoints.Add(new IntersectionPointPair(firstExtrudedPointAsIntersection, lastExtrudedPointAsIntersection));
            }
            intersectionPoints.Insert(0, firstExtrudedPointAsIntersection);
            intersectionPoints.Add(lastExtrudedPointAsIntersection);
        }

        /// <summary>
        /// Return a list of points where an extruded line consisting of segments intersect.
        /// </summary>
        /// <param name="extrudedPointList">The extruded points.</param>
        private static List<IntersectionPoint> DetermineIntersectionPoints(SegmentwiseExtrudedPointListUV extrudedPoints)
        {
            bool doExtrudedPointsLoop = true;
            List<IntersectionPoint> intersectionPoints = ExtractIntersectionsPoints(extrudedPoints, doExtrudedPointsLoop);
            intersectionPoints.Sort(CompareIntersectionPointParameter);
            return intersectionPoints;
        }

        /// <summary>
        /// Comparison function between two intersection points, comparing their parameter values.
        /// </summary>
        /// <param name="firstIntersectionPoint">First intersection point</param>
        /// <param name="secondItersectionPoint">Second intersection point</param>
        /// <returns></returns>
        private static int CompareIntersectionPointParameter(IntersectionPoint firstIntersectionPoint, IntersectionPoint secondItersectionPoint)
        {
            return firstIntersectionPoint.Parameter.CompareTo(secondItersectionPoint.Parameter);
        }

        #region intersections

        /// <summary>
        /// Gets a list of intersections of extruded points.
        /// </summary>
        /// <param name="extrudedPointList">List of extruded points.</param>
        /// <param name="doExtrudedPointsLoop">If the extruded points form a continuous loop.</param>
        private static List<IntersectionPoint> ExtractIntersectionsPoints(SegmentwiseExtrudedPointListUV extrudedPointList, bool doExtrudedPointsLoop)
        {
            var intersectionPoints = new List<IntersectionPoint>();
            var intersections = new List<Intersection>();
            var extrudedPoints = extrudedPointList.Points;
            for (int i = 0; i < extrudedPoints.Count - 1; i++)
            {
                bool isFurtherIntersection = AddFurtherSegmentIntersection(extrudedPointList, i, intersections);
                if (isFurtherIntersection)
                {
                    for (int index = 0; index < intersections.Count; index++)
                    {
                        var intersection = intersections[index];
                        var intersectionIndex = intersection.SecondSegmentIndex;
                        var segmentFraction1 = intersection.FirstSegmentFraction;
                        var segmentFraction2 = intersection.SecondSegmentFraction;
                        if (!doExtrudedPointsLoop || i > 0 || intersectionIndex + 1 < extrudedPoints.Count - 1)
                        {
                            var firstSegmentPoint1 = extrudedPoints[i];
                            var firstSegmentPoint2 = extrudedPoints[i + 1];
                            var secondSegmentPoint1 = extrudedPoints[intersectionIndex];
                            var secondSegmentPoint2 = extrudedPoints[intersectionIndex + 1];
                            var firstIntersection = AverageExtrudedPointsToIntersection(firstSegmentPoint1, firstSegmentPoint2, segmentFraction1, secondSegmentPoint1.LinePointSegmentIndex, secondSegmentPoint2.LinePointSegmentIndex2);
                            var secondIntersection = AverageExtrudedPointsToIntersection(secondSegmentPoint1, secondSegmentPoint2, segmentFraction2, firstSegmentPoint1.LinePointSegmentIndex, firstSegmentPoint2.LinePointSegmentIndex2);

                            //Average the positions of each points, and assign this position to both points.
                            var averagedIntersectionVector = 0.5f * (firstIntersection.Point + secondIntersection.Point);
                            firstIntersection.Point = averagedIntersectionVector;
                            secondIntersection.Point = averagedIntersectionVector;

                            intersectionPoints.Add(firstIntersection);
                            intersectionPoints.Add(secondIntersection);
                        }
                    }
                }
            }
            return intersectionPoints;
        }

        /// <summary>
        /// Average extruded points of a segment to an intersection point based on fraction of how far along the segment the intersection happened. 
        /// Averages UVs directly.
        /// </summary>
        /// <param name="first">First point</param>
        /// <param name="second">Second point</param>
        /// <param name="fractionOfSecond">From of the section point to be used in the average</param>
        /// <param name="otherOriginalLineIndex1">Additional original line index to be referred to by the intersection point.</param>
        /// <param name="otherOriginalLineIndex2">Additional original line index to be referred to by the intersection point.</param>
        private static IntersectionPoint AverageExtrudedPointsToIntersection(ExtrudedPointUV first, ExtrudedPointUV second, float fractionOfSecond, int otherOriginalLineIndex1, int otherOriginalLineIndex2)
        {
            var averagedLinePoint = new LinePointUV(first).AverageWith(new LinePointUV(second), fractionOfSecond);
            var extrudedPoint = new ExtrudedPointUV(averagedLinePoint, first.LinePointSegmentIndex, second.LinePointSegmentIndex2);
            var extrudedPointOtherIndices = extrudedPoint;
            extrudedPointOtherIndices.LinePointSegmentIndex = otherOriginalLineIndex1;
            extrudedPointOtherIndices.LinePointSegmentIndex2 = otherOriginalLineIndex2;
            return new IntersectionPoint(extrudedPoint, extrudedPoint, extrudedPointOtherIndices);
        }

        /// <summary>
        /// Returns if an intersection with a further segment exists, and has been added to the list of intersections.
        /// </summary>
        /// <param name="extrudedPointList">List of extruded points.</param>
        /// <param name="startExtrudedIndex">Index of extruded points at which to begin looking for intersections later in the list.</param>
        /// <param name="intersections">List of all intersections</param>
        private static bool AddFurtherSegmentIntersection(SegmentwiseExtrudedPointListUV extrudedPointList, int startExtrudedIndex, List<Intersection> intersections)
        {
            intersections.Clear();
            bool ret = false;
            int numSteps = 1;
            var extrudedMaxSegmentDistance = extrudedPointList.MaxSegmentDistance;
            var extrudedPoints = extrudedPointList.Points;
            for (int j = startExtrudedIndex + 2; j < extrudedPoints.Count - 1; j += numSteps)
            {
                Vector2 intersectionVector; float firstSegmentFraction, secondSegmentFraction;
                bool isCurrentIntersection = LineSegmentUtil.GetLineSegmentIntersection(extrudedPoints[startExtrudedIndex].Point, extrudedPoints[startExtrudedIndex + 1].Point, extrudedPoints[j].Point, extrudedPoints[j + 1].Point, out intersectionVector, out firstSegmentFraction, out secondSegmentFraction);
                if (isCurrentIntersection)
                {
                    ret = true;
                    int secondIntersectionIndex = j;
                    intersections.Add(new Intersection(startExtrudedIndex, secondIntersectionIndex, firstSegmentFraction, secondSegmentFraction));
                    //Do not break; there can be more than one intersection further from this startExtrudedIndex
                }
                var distanceDiff = (extrudedPoints[j].Point - extrudedPoints[startExtrudedIndex].Point).magnitude;
                numSteps = Mathf.FloorToInt(Mathf.Max(1f, distanceDiff / extrudedMaxSegmentDistance));
            }
            return ret;
        }

        #endregion
    }
}
