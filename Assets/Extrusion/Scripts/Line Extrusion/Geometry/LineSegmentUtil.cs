using UnityEngine;
using System;

namespace BabyDinoHerd.Extrusion.Line.Geometry
{
    /// <summary>
    /// Utility methods for line segments.
    /// </summary>
    public static class LineSegmentUtil
    {
        #region neighbouring-segment-intersection

        /// <summary>
        /// Get an intersection point along an extruded segment, due to a neighbouring segment, given the extrusion amount and fraction along the segment where the intersection occurs.
        /// </summary>
        /// <param name="pointVector"></param>
        /// <param name="segmentDirection">Segment direction vector</param>
        /// <param name="extrusionMagnitudeFractionAlongSegment">Fraction of extrusion distance along the segment</param>
        /// <param name="extrusionAmount">The extrusion amount</param>
        internal static Vector2 GetExtrudedSegmentNeighbouringIntersectionPoint(Vector2 pointVector, Vector2 segmentDirection, double extrusionMagnitudeFractionAlongSegment, float extrusionAmount)
        {
            var normal = NormalUtil.NormalFromTangent(segmentDirection);
            return pointVector + extrusionAmount * normal + Mathf.Abs(extrusionAmount) * segmentDirection * (float)extrusionMagnitudeFractionAlongSegment;
        }

        /// <summary>
        /// Determines if two segment tangent directions emanating from a common point intersect.
        /// This does not take into account actual segment length or extrusion distance; only fractional distances are returned.
        /// They will intersect if the segments are 'concave' with respect to the extrusion sign.
        /// Note that the extrusion fraction for second tangent is negative that of the first.
        /// </summary>
        /// <param name="previousTangent">Tangent vector of previous segment.</param>
        /// <param name="nextTangent">Tangent vector of next segment</param>
        /// <param name="extrusionSign">Sign of the extrusion direction</param>
        /// <param name="extrusionMagnitudeFractionFirstTangent">Fraction of extrusion magnitude along the first tangent direction that the intersection will occur.</param>
        internal static bool DoNeighbouringSegmentTangentsFromCommonPointIntersect(Vector2 previousTangent, Vector2 nextTangent, float extrusionSign, out double extrusionMagnitudeFractionFirstTangent)
        {
            bool ret = false;
            var cross = Vector2Util.Cross2D(nextTangent, previousTangent);
            var dot = Vector2.Dot(previousTangent, nextTangent);
            var sign = Mathf.Sign(extrusionSign);
            if (cross == 0f)
            {
                extrusionMagnitudeFractionFirstTangent = 0f;
                ret = false;
            }
            else
            {
                extrusionMagnitudeFractionFirstTangent = sign * (1f - dot) / cross;
                ret = extrusionMagnitudeFractionFirstTangent <= 0f;
            }
            return ret;
        }

        #endregion

        #region segment-intersection

        /// <summary>
        /// Determines whether two 2D line segments (each defined by start and end points) intersect.
        /// </summary>
        /// <param name="segment1Start">Segment 1 start point</param>
        /// <param name="segment1End">Segment 1 end point</param>
        /// <param name="segment2Start">Segment 2 start point</param>
        /// <param name="segment2End">Segment 2 end point</param>
        /// <param name="intersectionPoint">Intersection point, if the segments intersect</param>
        /// <param name="segmentFraction1">Fraction along first segment of the intersection, if the segments intersect</param>
        /// <param name="segmentFraction2">Fraction along second segment of the intersection, if the segments intersect</param>
        public static bool GetLineSegmentIntersection(Vector2 segment1Start, Vector2 segment1End, Vector2 segment2Start, Vector2 segment2End, out Vector2 intersectionPoint, out float segmentFraction1, out float segmentFraction2)
        {
            bool ret = false;
            intersectionPoint = Vector2.zero;
            segmentFraction1 = 0;
            segmentFraction2 = 0;
            var diff1 = segment1End - segment1Start;
            var diff2 = segment2End - segment2Start;
            var crossDiff1Diff2 = Vector2Util.Cross2D(diff1, diff2);
            var vec1to2 = segment2Start - segment1Start;
            var vec1To2CrossDiff1 = Vector2Util.Cross2D(vec1to2, diff1);
            if (crossDiff1Diff2 == 0f)
            {
                if (vec1To2CrossDiff1 == 0f)
                {
                    //collinear
                    var diff1MagSq = diff1.sqrMagnitude;
                    var diff2MagSq = diff2.sqrMagnitude;
                    var t0 = Vector2.Dot(vec1to2, diff1) / diff1MagSq;
                    var t1 = t0 + Vector2.Dot(diff1, diff2) / diff1MagSq;
                    var min = Mathf.Min(t0, t1);
                    var max = Mathf.Max(t0, t1);
                    if (min <= 1f && max >= 0f && diff1MagSq > 0f && diff2MagSq > 0f)
                    {
                        ret = true;
                        segmentFraction1 = 0.5f * (Mathf.Clamp01(min) + Mathf.Clamp01(max));
                        intersectionPoint = segment1Start + diff1 * segmentFraction1;
                        segmentFraction2 = Vector2.Dot(intersectionPoint - segment2Start, diff2);
                    }
                    else
                    {
                        //range does not intsersect [0:1]
                    }
                }
                else
                {
                    //parallel, not collinear
                    ret = false;
                }
            }
            else
            {
                var vec1To2CrossDiff2 = Vector2Util.Cross2D(vec1to2, diff2);
                segmentFraction1 = vec1To2CrossDiff2 / crossDiff1Diff2;
                segmentFraction2 = vec1To2CrossDiff1 / crossDiff1Diff2;
                if (segmentFraction1 >= 0 && segmentFraction2 >= 0 && segmentFraction1 <= 1f && segmentFraction2 <= 1f)
                {
                    ret = true;
                    intersectionPoint = segment1Start + diff1 * segmentFraction1;
                }
            }
            return ret;
        }

        #endregion

        #region point-segment distance

        /// <summary>
        /// Determines the closest point along a 2D line segment to a test point
        /// </summary>
        /// <param name="segmentStart">Segment start point</param>
        /// <param name="segmentEnd">Segment end point</param>
        /// <param name="point">Test point</param>
        /// <param name="distanceSquared">The distance squared between closest point and test point</param>
        internal static Vector2 ClosestPointOnSegment(Vector2 segmentStart, Vector2 segmentEnd, Vector2 point, out float distanceSquared)
        {
            var segmentDiff = segmentEnd - segmentStart;
            var dot = Vector2.Dot(point - segmentStart, segmentDiff);
            var segmentDiffMagSq = segmentDiff.sqrMagnitude;
            if (segmentDiffMagSq == 0f)
            {
                distanceSquared = (point - segmentStart).sqrMagnitude;
                return segmentStart;
            }
            else
            {
                float fraction = Mathf.Clamp01(dot / segmentDiff.sqrMagnitude);
                Vector2 closest = segmentStart + fraction * segmentDiff;
                distanceSquared = (closest - point).sqrMagnitude;
                return closest;
            }
        }

        /// <summary>
        /// If a point is on the right-hand side of a segment.
        /// </summary>
        /// <param name="segmentStart">The segment start point.</param>
        /// <param name="segmentEnd">The segment end point.</param>
        /// <param name="point">The test point.</param>
        internal static bool PointIsOnRightSide(Vector2 segmentStart, Vector2 segmentEnd, Vector2 point)
        {
            var segmentDiff = segmentEnd - segmentStart;
            var pointDiff = point - segmentStart;
            var cross = Vector2Util.Cross2D(pointDiff, segmentDiff);
            return cross >= 0f;
        }

        /// <summary>
        /// Determines the fraction along a 2D line segment of the closest point to a test point
        /// </summary>
        /// <param name="segmentStart">Segment start point</param>
        /// <param name="segmentEnd">Segment end point</param>
        /// <param name="point">Test point</param>
        internal static float ClosestFractionAlongSegment(Vector2 segmentStart, Vector2 segmentEnd, Vector2 point)
        {
            float distanceSquared;
            return ClosestFractionAlongSegment(segmentStart, segmentEnd, point, out distanceSquared);
        }

        /// <summary>
        /// Determines the fraction along a 2D line segment of the closest point to a test point
        /// </summary>
        /// <param name="segmentStart">Segment start point</param>
        /// <param name="segmentEnd">Segment end point</param>
        /// <param name="point">Test point</param>
        /// <param name="distanceSquared">The distance squared between closest point and test point</param>
        internal static float ClosestFractionAlongSegment(Vector2 segmentStart, Vector2 segmentEnd, Vector2 point, out float distanceSquared)
        {
            var segmentDiff = segmentEnd - segmentStart;
            var dot = Vector2.Dot(point - segmentStart, segmentDiff);
            var segmentDiffMagSq = segmentDiff.sqrMagnitude;
            if (segmentDiffMagSq == 0f)
            {
                distanceSquared = (point - segmentStart).sqrMagnitude;
                return 0f;
            }
            else
            {
                float fraction = Mathf.Clamp01(dot / segmentDiff.sqrMagnitude);
                distanceSquared = (segmentStart + fraction * segmentDiff - point).sqrMagnitude;
                return fraction;
            }
        }

        /// <summary>
        /// Determines if a point is within a certain distance of a 2D line segment defined by a start and end point.
        /// </summary>
        /// <param name="segmentStart">Segment start point</param>
        /// <param name="segmentEnd">Segment end point</param>
        /// <param name="point">Test point</param>
        /// <param name="distanceSquared">The test distance squared</param>
        internal static bool IsPointWithinDistanceOfSegment(Vector2 segmentStart, Vector2 segmentEnd, Vector2 point, float distanceSquared)
        {
            var segmentDiff = segmentEnd - segmentStart;
            var dot = Vector2.Dot(point - segmentStart, segmentDiff);
            var segmentDiffMagSq = segmentDiff.sqrMagnitude;
            if (segmentDiffMagSq == 0f)
            {
                return (point - segmentStart).sqrMagnitude < distanceSquared;
            }
            else
            {
                float fraction = Mathf.Clamp01(dot / segmentDiff.sqrMagnitude);
                return (segmentStart + fraction * segmentDiff - point).sqrMagnitude < distanceSquared;
            }
        }

        /// <summary> Returns closest point, distances, and handedness information about the closest point on a 2D segment to a test point. </summary>
        /// <param name="segmentStart">Segment start point</param>
        /// <param name="segmentEnd">Segment end point</param>
        /// <param name="point">Test point</param>
        /// <param name="closestPoint"> The closest point on the segment to the test point </param>
        /// <param name="onRightSide">Whether the test point is on the right-hand side of the segment </param>
        /// <param name="segmentFraction">Fraction along the segment of the closest point to the test point</param>
        /// <param name="distance">Distance from the closest point to the test point</param>
        /// <param name="perpendicularDistance">Perpendicular distance from the closest point to the test point</param>
        /// <param name="tangentialDistance">Tangential distance from the closest point to the test point</param>
        internal static void GetClosestPointToSegmentInformation(Vector2 segmentStart, Vector2 segmentEnd, Vector2 point, out Vector2 closestPoint, out bool onRightSide, out float segmentFraction, out float distance, out float perpendicularDistance, out float tangentialDistance)
        {
            var segmentDiff = segmentEnd - segmentStart;
            Vector2 pointDiff = point - segmentStart;
            var dot = Vector2.Dot(pointDiff, segmentDiff);
            var segmentDiffMagSq = segmentDiff.sqrMagnitude;
            if (segmentDiffMagSq == 0f)
            {
                onRightSide = true;
                segmentFraction = 0f;
                closestPoint = segmentStart;
                distance = pointDiff.magnitude;
                perpendicularDistance = distance;
                tangentialDistance = distance;
            }
            else
            {
                var segmentMagnitude = segmentDiff.magnitude;
                var segmentDirection = segmentDiff / segmentMagnitude;
                segmentFraction = Mathf.Clamp01(dot / (segmentMagnitude * segmentMagnitude));
                closestPoint = (segmentStart + segmentFraction * segmentDiff);

                var closestPointDiff = point - closestPoint;
                distance = closestPointDiff.magnitude;

                var tangentialSignedDistance = Vector2.Dot(segmentDirection, closestPointDiff);
                var perpendicularVector = closestPointDiff - tangentialSignedDistance * segmentDirection;

                tangentialDistance = Mathf.Abs(tangentialSignedDistance);
                perpendicularDistance = perpendicularVector.magnitude;

                var cross = Vector2Util.Cross2D(pointDiff, segmentDiff);
                onRightSide = cross >= 0f;
            }
        }


        /// <summary>
        /// Gets the distance from a point to a 2D line segment defined by a start and end point, along with the closest point along the segment.
        /// </summary>
        /// <param name="segmentStart">Segment start point</param>
        /// <param name="segmentEnd">Segment end point</param>
        /// <param name="point">Test point</param>        
        private static PointAndDistance ClosestPointOnSegment(Vector2 segmentStart, Vector2 segmentEnd, Vector2 point)
        {
            var segmentDiff = segmentEnd - segmentStart;
            var dot = Vector2.Dot(point - segmentStart, segmentDiff);
            var segmentDiffMagSq = segmentDiff.magnitude;
            if (segmentDiffMagSq == 0f)
            {
                return new PointAndDistance(segmentStart, (point - segmentStart).magnitude);
            }
            else
            {
                float fraction = Mathf.Clamp01(dot / segmentDiff.sqrMagnitude);
                var pointOnSegment = fraction <= 0 ? segmentStart : fraction >= 1 ? segmentEnd : segmentStart + fraction * segmentDiff;
                return new PointAndDistance(pointOnSegment, (point - pointOnSegment).magnitude);
            }
        }

        #endregion

        #region segment distances

        /// <summary>
        /// Returns the signed distance (relative to the first segment) to the closest point on the second line segment.
        /// </summary>
        /// <param name="segment1Start">Segment 1 start point</param>
        /// <param name="segment1End">Segment 1 end point</param>
        /// <param name="segment2Start">Segment 2 start point</param>
        /// <param name="segment2End">Segment 2 end point</param>
        internal static float SignedDistanceFromSegmentToClosestPointOnOtherSegment(Vector2 segment1Start, Vector2 segment1End, Vector2 segment2Start, Vector2 segment2End)
        {
            var closestPoints = ClosestPointsBetweenSegments(segment1Start, segment1End, segment2Start, segment2End);
            var cross = Vector2Util.Cross2D(closestPoints.SecondPoint - segment1Start, segment1End - segment1Start);
            return -Math.Sign(cross) * closestPoints.Distance;//- sign is to match with normal direction in Normal Util
        }

        /// <summary>
        /// Gets the distance between two 2D line segments each defined by a start and end point, as well as the points on each segment defining this closest distance.
        /// </summary>
        /// <param name="segment1Start">Segment 1 start point</param>
        /// <param name="segment1End">Segment 1 end point</param>
        /// <param name="segment2Start">Segment 2 start point</param>
        /// <param name="segment2End">Segment 2 end point</param>
        private static PointsAndDistance ClosestPointsBetweenSegments(Vector2 segment1Start, Vector2 segment1End, Vector2 segment2Start, Vector2 segment2End)
        {
            var closestPoint1 = ClosestPointOnSegment(segment1Start, segment1End, segment2Start);
            var closestPoint2 = ClosestPointOnSegment(segment1Start, segment1End, segment2End);
            var closestPoint3 = ClosestPointOnSegment(segment2Start, segment2End, segment1Start);
            var closestPoint4 = ClosestPointOnSegment(segment2Start, segment2End, segment1End);
            var closerOf1Or2 = closestPoint1.Distance <= closestPoint2.Distance ? new PointsAndDistance(closestPoint1, segment2Start) : new PointsAndDistance(closestPoint2, segment2End);
            var closerOf3Or4 = closestPoint3.Distance <= closestPoint4.Distance ? new PointsAndDistance(closestPoint3, segment1Start) : new PointsAndDistance(closestPoint4, segment1End);
            var closest = closerOf1Or2.Distance <= closerOf3Or4.Distance ? closerOf1Or2 : closerOf3Or4;
            return closest;
        }

        /// <summary>
        /// Gets the distance between two 2D line segments each defined by a start and end point.
        /// </summary>
        /// <param name="segment1Start">Segment 1 start point</param>
        /// <param name="segment1End">Segment 1 end point</param>
        /// <param name="segment2Start">Segment 2 start point</param>
        /// <param name="segment2End">Segment 2 end point</param>
        internal static float DistanceBetweenSegments(Vector2 segment1Start, Vector2 segment1End, Vector2 segment2Start, Vector2 segment2End)
        {
            var distance1 = DistanceToSegment(segment1Start, segment1End, segment2Start);
            var distance2 = DistanceToSegment(segment1Start, segment1End, segment2End);
            var distance3 = DistanceToSegment(segment2Start, segment2End, segment1Start);
            var distance4 = DistanceToSegment(segment2Start, segment2End, segment1End);
            return Math.Min(distance1, Math.Min(distance2, Math.Min(distance3, distance4)));
        }

        /// <summary>
        /// Gets the distance from a point to a 2D line segment defined by a start and end point.
        /// </summary>
        /// <param name="segmentStart">Segment start point</param>
        /// <param name="segmentEnd">Segment end point</param>
        /// <param name="point">Test point</param>        
        internal static float DistanceToSegment(Vector2 segmentStart, Vector2 segmentEnd, Vector2 point)
        {
            var segmentDiff = segmentEnd - segmentStart;
            var dot = Vector2.Dot(point - segmentStart, segmentDiff);
            var segmentDiffMagSq = segmentDiff.magnitude;
            if (segmentDiffMagSq == 0f)
            {
                return (point - segmentStart).magnitude;
            }
            else
            {
                float fraction = Mathf.Clamp01(dot / segmentDiff.sqrMagnitude);
                var pointOnSegment = fraction <= 0 ? segmentStart : fraction >= 1 ? segmentEnd : segmentStart + fraction * segmentDiff;
                return (pointOnSegment - point).magnitude;
            }
        }

        private struct PointsAndDistance
        {
            public Vector2 FirstPoint;
            public Vector2 SecondPoint;
            public float Distance;

            public PointsAndDistance(Vector2 firstPoint, Vector2 secondPoint, float distance)
            {
                FirstPoint = firstPoint;
                SecondPoint = secondPoint;
                Distance = distance;
            }

            public PointsAndDistance(PointAndDistance firstPointAndDistance, Vector2 secondPoint)
            {
                FirstPoint = firstPointAndDistance.Point;
                SecondPoint = secondPoint;
                Distance = firstPointAndDistance.Distance;
            }
        }

        internal struct PointAndDistance
        {
            public Vector2 Point;
            public float Distance;
            public PointAndDistance(Vector2 point, float distance)
            {
                Point = point;
                Distance = distance;
            }
        }

        #endregion
    }
}