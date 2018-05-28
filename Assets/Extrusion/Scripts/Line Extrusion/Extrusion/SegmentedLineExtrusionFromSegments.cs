using BabyDinoHerd.Extrusion.Line.Geometry;
using BabyDinoHerd.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace BabyDinoHerd.Extrusion.Line.Extrusion
{
    public class SegmentedLineExtrusionFromSegments
    {
        /// <summary>
        /// The minimum angle between points in case of extruding between two convex segments, in degrees.
        /// </summary>
        private static float _convexExtrusionMinimumAngleDegrees = 5.0f;
        /// <summary>
        /// The minimum angle between points in case of extruding between two convex segments, in radians.
        /// </summary>
        private static float _convexExtrusionMinimumAngleRadians = _convexExtrusionMinimumAngleDegrees * Mathf.Deg2Rad;
        /// <summary> If the start point of the full segmentwise extrusion should be repeated at the end with appropriately-shifted parameter. </summary>
        private static bool _repeatAllSidesStartPointAtEnd = true;

        /// <summary>
        /// Gets the extruded subspace of points of a line on both sides and both ends.
        /// </summary>
        /// <param name="originalLineSegments">List of line points with segments connecting to the next point in the line.</param>
        /// <param name="extrusionAmount">The extrusion amount.</param>
        public static List<ExtrudedPointUV> GetExtrudedSubspaceAllSides(List<LinePointUV> originalLinePoints, float extrusionAmount)
        {
            var originalLineSegments = GeometryConversion.ConvertLinePointsToForwardLineSegments(originalLinePoints);
            var reversedLineSegments = GetReversedSegmentsAndReversedParameters(originalLineSegments, originalLinePoints[originalLinePoints.Count - 1]);

            var extrudedLineCapForwardParameterShift = 0f;
            var extrudedLineCapForward = GetExtrudedLineCap(originalLineSegments, extrusionAmount, extrudedLineCapForwardParameterShift, false);

            const float localParameterShift = 0.1f;

            var lineForwardParameterShift = extrudedLineCapForward[extrudedLineCapForward.Count - 1].Parameter + localParameterShift;
            var extrudedSubspaceForward = GetExtrudedSubspaceAndUvFromSegmentIntersections(originalLineSegments, originalLinePoints[originalLinePoints.Count - 1], extrusionAmount, lineForwardParameterShift, false);

            var extrudedLineCapBackwardParameterShift = extrudedSubspaceForward[extrudedSubspaceForward.Count - 1].Parameter + localParameterShift;
            var extrudedLineCapBackward = GetExtrudedLineCap(reversedLineSegments, extrusionAmount, extrudedLineCapBackwardParameterShift, true);

            var lineBackwardParameterShift = extrudedLineCapBackward[extrudedLineCapBackward.Count - 1].Parameter + originalLinePoints[0].Parameter + localParameterShift;
            var reversedLastPoint = originalLinePoints[0];
            reversedLastPoint.Parameter = originalLinePoints[originalLinePoints.Count - 1].Parameter;
            var extrudedSubspaceBackward = GetExtrudedSubspaceAndUvFromSegmentIntersections(reversedLineSegments, reversedLastPoint, extrusionAmount, lineBackwardParameterShift, true);

            var extrudedSubspaceTotal = new List<ExtrudedPointUV>();
            extrudedSubspaceTotal.AddRange(extrudedLineCapForward);
            extrudedSubspaceTotal.AddRange(extrudedSubspaceForward);
            extrudedSubspaceTotal.AddRange(extrudedLineCapBackward);
            extrudedSubspaceTotal.AddRange(extrudedSubspaceBackward);

            if (_repeatAllSidesStartPointAtEnd)
            {
                var endPointWithShiftedParameter = extrudedLineCapForward[0];
                endPointWithShiftedParameter.Parameter = extrudedSubspaceBackward[extrudedSubspaceBackward.Count - 1].Parameter + localParameterShift;
                extrudedSubspaceTotal.Add(endPointWithShiftedParameter);
            }

            return extrudedSubspaceTotal;
        }

        /// <summary>
        /// Gets a set of line point and segments in reverse direction, with reversed parameters.
        /// </summary>
        /// <param name="originalLineSegments">List of line points with segments connecting to the next point in the line.</param>
        /// <param name="lastLinePoint">The final line point.</param>
        private static List<LinePointUVAndSegment> GetReversedSegmentsAndReversedParameters(List<LinePointUVAndSegment> originalLineSegments, LinePointUV lastLinePoint)
        {
            List<LinePointUVAndSegment> reversedSegments = new List<LinePointUVAndSegment>();
            RangeF originalSegmentsParameterRange = new RangeF(originalLineSegments[0].Parameter, lastLinePoint.Parameter);
            var originalSegmentsParameterRangeMaxPlusMin = originalSegmentsParameterRange.Min + originalSegmentsParameterRange.Max;

            var endIndex = originalLineSegments.Count - 1;
            for (int i = endIndex; i >= 0; i--)
            {
                var originPoint = i == endIndex ? lastLinePoint : originalLineSegments[i + 1].LinePoint;
                var currentSegmentForward = originalLineSegments[i];
                originPoint.Parameter = originalSegmentsParameterRangeMaxPlusMin - originPoint.Parameter;
                var segment = new LinePointUVAndSegment(originPoint, currentSegmentForward.Point, -currentSegmentForward.SegmentTangent, -currentSegmentForward.SegmentNormal, currentSegmentForward.SegmentLength);
                reversedSegments.Add(segment);
            }
            return reversedSegments;
        }

        /// <summary>
        /// Gets the extruded subspace of points with v parameter set appropriately to direction of hte line compared to extrusion.
        /// </summary>
        /// <param name="originalLineSegments">List of line points with segments connecting to the next point in the line.</param>
        /// <param name="lastLinePoint">The final line point.</param>
        /// <param name="extrusionAmount">The extrusion amount.</param>
        /// <param name="parameterShift">The amount to shift the original line point parameters by when constructing the extruded points.</param>
        /// <param name="isReversed">Whether line point indices associated with extruded points should be reversed.</param>
        private static List<ExtrudedPointUV> GetExtrudedSubspaceAndUvFromSegmentIntersections(List<LinePointUVAndSegment> originalLineSegments, LinePointUV lastLinePoint, float extrusionAmount, float parameterShift, bool isReversed)
        {
            var extrudedSubspace = GetExtrudedSubspace_Points_SegmentIntersections(originalLineSegments, lastLinePoint, extrusionAmount, parameterShift, isReversed, includeLast: true);
            float uv_vparameter = isReversed ? 0f : 1f;
            for (int i = 0; i < extrudedSubspace.Count; i++)
            {
                ExtrudedPointUV extrudedPointUV = extrudedSubspace[i];
                extrudedPointUV.UV = new Vector2(extrudedSubspace[i].UV.x, uv_vparameter);
                extrudedSubspace[i] = extrudedPointUV;
            }
            return extrudedSubspace;
        }

        /// <summary>
        /// Gets an extruded semicircular line cap at the start of a set of line points.
        /// </summary>
        /// <param name="linePointsAndSegments">List of line points with segments connecting to the next point in the line.</param>
        /// <param name="extrusionAmount">The extrusion amount.</param>
        /// <param name="parameterShift">The amount to shift the original line point parameters by when constructing the extruded points.</param>
        /// <param name="lineIsReversed">Whether the line segments have been reversed.</param>
        private static List<ExtrudedPointUV> GetExtrudedLineCap(List<LinePointUVAndSegment> linePointsAndSegments, float extrusionAmount, float parameterShift, bool lineIsReversed)
        {
            List<ExtrudedPointUV> lineCapPoints = new List<ExtrudedPointUV>();
            var startSegment = linePointsAndSegments[0];
            var normal = startSegment.SegmentNormal;
            var minusTangent = -startSegment.SegmentTangent;

            var normalDeltaUV = Vector2.up * (lineIsReversed ? -1f : 1f);
            var tangentDeltaUV = -Vector2.right * (lineIsReversed ? -1f : 1f);

            var comparisonDistance = startSegment.SegmentLength;
            var halfCircleDistance = Mathf.PI * extrusionAmount;
            int numPoints = Mathf.CeilToInt(halfCircleDistance / comparisonDistance);
            int extrudedPointIndex = lineIsReversed ? linePointsAndSegments.Count : 0;
            if (numPoints < 5)
            {
                numPoints = 5;
            }
            else if(numPoints % 2 == 0)
            {
                //Make a more symmetrical cap.
                numPoints++;
            }

            for (int i = 0; i < numPoints; i++)
            {
                float fractionOrig = (i + 1) / (float)(numPoints + 1);
                float fraction = 1f - fractionOrig;
                var avgNormal = fraction <= 0.5f ? normal.AverageDirectionVector(minusTangent, fraction * 2f) : minusTangent.AverageDirectionVector(-normal, (fraction - 0.5f) * 2f);
                var avgDeltaUVDir = fraction <= 0.5f ? normalDeltaUV.AverageDirectionVector(tangentDeltaUV, fraction * 2f) : tangentDeltaUV.AverageDirectionVector(-normalDeltaUV, (fraction - 0.5f) * 2f);
                var deltaUV = new Vector2(avgDeltaUVDir.x * extrusionAmount, avgDeltaUVDir.y * 0.5f);
                var extrudedVector = startSegment.Point + avgNormal * extrusionAmount;
                var newLinePointInFan = new LinePointUV(fractionOrig + parameterShift, extrudedVector, startSegment.UV + deltaUV);
                lineCapPoints.Add(new ExtrudedPointUV(newLinePointInFan, extrudedPointIndex, extrudedPointIndex));
            }
            return lineCapPoints;
        }

        /// <summary>
        /// Delegate for returning a shifted and/or scaled version of an original line point parameter.
        /// </summary>
        /// <param name="parameter">Original parameter.</param>
        private delegate float GetShiftedScaledParameter(float parameter);

        /// <summary>
        /// Gets the points corresponding to the extruded subspace due to extruded segment-by-segment intersection.
        /// </summary>
        /// <param name="linePointsAndForwardSegments">List of line points with segments connecting to the next point in the line.</param>
        /// <param name="lastLinePoint">The final line point.</param>
        /// <param name="extrusionAmount">The extrusion amount.</param>
        /// <param name="parameterShift">The amount to shift the original line point parameters by when constructing the extruded points.</param>
        /// <param name="reverseIndices">Whether line point indices associated with extruded points should be reversed.</param>
        /// <param name="includeLast">If final segment should be included in the extrusion.</param>
        private static List<ExtrudedPointUV> GetExtrudedSubspace_Points_SegmentIntersections(List<LinePointUVAndSegment> linePointsAndForwardSegments, LinePointUV lastLinePoint, float extrusionAmount, float parameterShift, bool reverseIndices, bool includeLast)
        {
            var extrudedLinePoints = new List<ExtrudedPointUV>();

            GetShiftedScaledParameter findParameter = (originalPointParameter) => parameterShift + originalPointParameter;

            var numSegments = linePointsAndForwardSegments.Count;
            if (numSegments >= 2)
            {
                int endCount = includeLast ? numSegments : numSegments - 1;
                for (int i = 0; i <= endCount; i++)
                {
                    if (i == 0)
                    {
                        int lineSegmentIndex = 0;
                        var linePointAndSegment = linePointsAndForwardSegments[lineSegmentIndex];
                        int extrudedPointIndexOnOriginalLine = reverseIndices ? numSegments : 0;

                        var extrudedStartPoint = new ExtrudedPointUV(linePointAndSegment.LinePoint, findParameter(linePointAndSegment.Parameter), linePointAndSegment.SegmentNormal, extrusionAmount, extrudedPointIndexOnOriginalLine);
                        extrudedLinePoints.Add(extrudedStartPoint);
                    }
                    else if (i == numSegments)
                    {
                        int lineSegmentIndex = numSegments - 1;
                        var lastLinePointAndSegment = linePointsAndForwardSegments[lineSegmentIndex];
                        int extrudedPointIndexOnOriginalLine = reverseIndices ? 0 : numSegments;

                        var extrudedEndPoint = new ExtrudedPointUV(lastLinePoint, findParameter(lastLinePoint.Parameter), lastLinePointAndSegment.SegmentNormal, extrusionAmount, extrudedPointIndexOnOriginalLine);
                        extrudedLinePoints.Add(extrudedEndPoint);
                    }
                    else
                    {
                        var indexToAssign = reverseIndices ? numSegments - i : i;
                        var linePointAndSegment = linePointsAndForwardSegments[i];
                        var nextLinePoint = i + 1 < numSegments ? linePointsAndForwardSegments[i + 1].LinePoint : lastLinePoint;
                        AddPointExtrusionFromPreviousNextSegments(linePointAndSegment, linePointsAndForwardSegments[i - 1], nextLinePoint, indexToAssign, extrusionAmount, extrudedLinePoints, findParameter);
                    }
                }
            }
            return extrudedLinePoints;
        }

        /// <summary>
        /// Add extruded point(s) based on two neighbouring line segments.
        /// </summary>
        /// <param name="linePointAndForwardSegment">A line point and its corresponding segment.</param>
        /// <param name="previousLinePointAndForwardSegment">The previous line point and its corresponding segment.</param>
        /// <param name="nextLinePoint">The next line point (since its parameter, uv etc will be needed).</param>
        /// <param name="lineSegmentIndexToAssign">Line segment index to assign the extruded point coming from.</param>
        /// <param name="extrusionAmount">The extrusion amount.</param>
        /// <param name="extrudedLinePoints">The list of all extruded line points.</param>
        /// <param name="findParameter">Delegate for determining altered point parameter.</param>
        private static void AddPointExtrusionFromPreviousNextSegments(LinePointUVAndSegment linePointAndForwardSegment, LinePointUVAndSegment previousLinePointAndForwardSegment, LinePointUV nextLinePoint, int lineSegmentIndexToAssign, float extrusionAmount, List<ExtrudedPointUV> extrudedLinePoints, GetShiftedScaledParameter findParameter)
        {
            double extrusionMagnitudeFractionFirstTangent;
            bool segmentIntersection = LineSegmentUtil.DoNeighbouringSegmentTangentsFromCommonPointIntersect(previousLinePointAndForwardSegment.SegmentTangent, linePointAndForwardSegment.SegmentTangent, Mathf.Sign(extrusionAmount), out extrusionMagnitudeFractionFirstTangent);
            if (segmentIntersection)
            {
                //Concave section; expect intersection of neighbouring line segments around this point
                var extrusionDistanceAlongTangents = Mathf.Abs((float)extrusionMagnitudeFractionFirstTangent * extrusionAmount);
                bool intersectionOccursOutOfSegment = extrusionDistanceAlongTangents >= Mathf.Min(previousLinePointAndForwardSegment.SegmentLength, linePointAndForwardSegment.SegmentLength);
                if (intersectionOccursOutOfSegment)
                {
                    AttemptAddNaiveExtrudedPointsWhenTheirExtrudedSegmentsDoNotIntersect(linePointAndForwardSegment, previousLinePointAndForwardSegment, lineSegmentIndexToAssign, extrusionAmount, extrudedLinePoints, findParameter, extrusionDistanceAlongTangents);
                }
                else
                {
                    var intersection = LineSegmentUtil.GetExtrudedSegmentNeighbouringIntersectionPoint(linePointAndForwardSegment.Point, previousLinePointAndForwardSegment.SegmentTangent, extrusionMagnitudeFractionFirstTangent, extrusionAmount);
                    var linePointOfThisIntersection = new LinePointUV(findParameter(linePointAndForwardSegment.Parameter), intersection, linePointAndForwardSegment.UV);
                    extrudedLinePoints.Add(new ExtrudedPointUV(linePointOfThisIntersection, lineSegmentIndexToAssign, lineSegmentIndexToAssign - 1));
                }
            }
            else
            {
                //Convex section, expect a circular fan of points around this point.
                RangeF rangeFromPreviousPoints = new RangeF(findParameter(previousLinePointAndForwardSegment.Parameter), findParameter(linePointAndForwardSegment.Parameter));
                RangeF rangeFromNextPoints = new RangeF(findParameter(linePointAndForwardSegment.Parameter), findParameter(nextLinePoint.Parameter));
                RangeF parameterRangeToUse = new RangeF(rangeFromPreviousPoints.FromFractionUnclamped(0.6f), rangeFromNextPoints.FromFractionUnclamped(0.4f));

                var extrudedFromPreviousNormal = linePointAndForwardSegment.Point + extrusionAmount * previousLinePointAndForwardSegment.SegmentNormal;
                var extrudedFromNextNormal = linePointAndForwardSegment.Point + extrusionAmount * linePointAndForwardSegment.SegmentNormal;
                var diffBetweenExtrusion = (extrudedFromNextNormal - extrudedFromPreviousNormal).magnitude;
                var minNeighbouringSegmentDistance = Mathf.Min(previousLinePointAndForwardSegment.SegmentLength, linePointAndForwardSegment.SegmentLength);
                float absExtrusionAmount = Mathf.Abs(extrusionAmount);
                var comparisonDistance = Mathf.Min(minNeighbouringSegmentDistance, absExtrusionAmount);

                var angleBetweenSegmentNormals = Mathf.Acos(Mathf.Clamp(Vector3.Dot(previousLinePointAndForwardSegment.SegmentNormal, linePointAndForwardSegment.SegmentNormal), -1f, 1f));

                int numPointsFromDistance = Mathf.CeilToInt(diffBetweenExtrusion / comparisonDistance);
                if (numPointsFromDistance <= 2) { numPointsFromDistance++; }//NB Want at least the end-points, and if distance is large enough to need 2 naturally, 3 seems to looks better.
                int numPointsFromAngle = Mathf.CeilToInt(angleBetweenSegmentNormals / _convexExtrusionMinimumAngleRadians);
                int numPoints = Mathf.Max(numPointsFromDistance, numPointsFromAngle);

                for (int i = 0; i < numPoints; i++)
                {
                    float fraction = numPoints == 1 ? 0.5f : i / (numPoints - 1.0f);
                    var averageTangent = previousLinePointAndForwardSegment.SegmentTangent.AverageDirectionVector(linePointAndForwardSegment.SegmentTangent, fraction);
                    var averageNormal = NormalUtil.NormalFromTangent(averageTangent);
                    var extrudedVector = linePointAndForwardSegment.Point + averageNormal * extrusionAmount;
                    var parameter = parameterRangeToUse.FromFractionUnclamped(fraction);

                    Vector2 newUV = GetUVParameterOfFanPoint(linePointAndForwardSegment, previousLinePointAndForwardSegment, nextLinePoint, absExtrusionAmount, angleBetweenSegmentNormals, fraction);

                    var newLinePointInFan = new LinePointUV(parameter, extrudedVector, newUV);
                    extrudedLinePoints.Add(new ExtrudedPointUV(newLinePointInFan, lineSegmentIndexToAssign, lineSegmentIndexToAssign - 1));
                }
            }
        }

        /// <summary>
        /// Determine the UV parameter of a fan point.
        /// </summary>
        /// <param name="linePointAndForwardSegment">A line point and its corresponding segment.</param>
        /// <param name="previousLinePointAndForwardSegment">The previous line point and its corresponding segment.</param>
        /// <param name="nextLinePoint">The next line point (since its parameter, uv etc will be needed).</param>
        /// <param name="absExtrusionAmount">Absolute value of the extrusion amount.</param>
        /// <param name="angleBetweenSegmentNormals">Angle between segment normals of <paramref name="previousLinePointAndForwardSegment"/> and <paramref name="linePointAndForwardSegment"/>.</param>
        /// <param name="fraction">Fraction along the fan.</param>
        private static Vector2 GetUVParameterOfFanPoint(LinePointUVAndSegment linePointAndForwardSegment, LinePointUVAndSegment previousLinePointAndForwardSegment, LinePointUV nextLinePoint, float absExtrusionAmount, float angleBetweenSegmentNormals, float fraction)
        {
            var currentPointUv = linePointAndForwardSegment.UV;
            bool isInPreviousHalfOfArc = fraction < 0.5f;
            var fractionFromNeighbour = isInPreviousHalfOfArc ? fraction : 1f - fraction;
            var fanArcLength = angleBetweenSegmentNormals * absExtrusionAmount;

            var neighbouringSegmentHalfLength = 0.5f * (isInPreviousHalfOfArc ? previousLinePointAndForwardSegment.SegmentLength : linePointAndForwardSegment.SegmentLength);

            var distance = neighbouringSegmentHalfLength + fractionFromNeighbour * fanArcLength;
            var distanceToHalfFan = neighbouringSegmentHalfLength + 0.5f * fanArcLength;
            var uvFractionFromNeighbouringHalfPoint = distance / distanceToHalfFan;

            var neighbouringPoint = isInPreviousHalfOfArc ? previousLinePointAndForwardSegment.LinePoint : nextLinePoint;
            var averageNeighbourAndCurrentPoint = neighbouringPoint.AverageWith(linePointAndForwardSegment.LinePoint, 0.5f);
            var averageNeighbourAndCurrentPointUv = averageNeighbourAndCurrentPoint.UV;
            var newUV = averageNeighbourAndCurrentPointUv * (1f - uvFractionFromNeighbouringHalfPoint) + currentPointUv * uvFractionFromNeighbouringHalfPoint;

            return newUV;
        }

        /// <summary>
        /// Atempt to add naively-extruded points when the extrusion from two neighbouring segments would intersect outside of those extruded segments.
        /// </summary>
        /// <param name="linePointAndForwardSegment">First segment.</param>
        /// <param name="previousLinePointAndForwardSegment">Previous segment.</param>
        /// <param name="lineSegmentIndexToAssign">Line segment index to assign the extruded point coming from.</param>
        /// <param name="extrusionAmount">The extrusion amount.</param>
        /// <param name="extrudedLinePoints">The list of all extruded line points.</param>
        /// <param name="findParameter">Delegate for determining altered point parameter.</param>
        /// <param name="extrusionIntersectionDistanceAlongTangents">The extrusion intersection distance along segment tangent directions.</param>
        private static void AttemptAddNaiveExtrudedPointsWhenTheirExtrudedSegmentsDoNotIntersect(LinePointUVAndSegment linePointAndForwardSegment, LinePointUVAndSegment previousLinePointAndForwardSegment, int lineSegmentIndexToAssign, float extrusionAmount, List<ExtrudedPointUV> extrudedLinePoints, GetShiftedScaledParameter findParameter, float extrusionIntersectionDistanceAlongTangents)
        {
            bool intersectionOccursOutOfBothSegments = extrusionIntersectionDistanceAlongTangents >= Mathf.Max(previousLinePointAndForwardSegment.SegmentLength, linePointAndForwardSegment.SegmentLength);
            if (intersectionOccursOutOfBothSegments)
            {
                //Nothing to do here, since any additional points will not contribute to any chunks that are not too close to the original line.
            }
            else
            {
                var extrudedFromCenterPointPreviousSegmentNormal = new ExtrudedPointUV(linePointAndForwardSegment.LinePoint, findParameter(linePointAndForwardSegment.Parameter), previousLinePointAndForwardSegment.SegmentNormal, extrusionAmount, lineSegmentIndexToAssign - 1);//Previous segment's index
                extrudedLinePoints.Add(extrudedFromCenterPointPreviousSegmentNormal);

                var extrudedFromCenterPointCurrentSegmentNormal = new ExtrudedPointUV(linePointAndForwardSegment.LinePoint, findParameter(linePointAndForwardSegment.Parameter), linePointAndForwardSegment.SegmentNormal, extrusionAmount, lineSegmentIndexToAssign);//Current segment's index
                extrudedLinePoints.Add(extrudedFromCenterPointCurrentSegmentNormal);
                //NB Here we are just adding the 'naive' points without computing the intersection, and relying on the overall intersection algorithm to take care of the rest.
            }
        }
    }
}