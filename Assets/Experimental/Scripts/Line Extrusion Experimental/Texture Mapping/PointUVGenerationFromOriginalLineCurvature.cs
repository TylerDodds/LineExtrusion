using UnityEngine;
using BabyDinoHerd.Extrusion.Line.Geometry;
using System.Collections.Generic;
using System;
using BabyDinoHerd.Extrusion.Line.Curvature.Experimental;
using BabyDinoHerd.Utility.Experimental;

namespace BabyDinoHerd.Extrusion.Line.TextureMapping.Experimental
{
    /// <summary>
    /// Generates UV values for points based on their positions relative to a segmentwise-defined line using the net curvature of that line.
    /// </summary>
    [BabyDinoHerd.Experimental]
    public class PointUVGenerationFromOriginalLineCurvature
    {
        /// <summary>
        /// Gets the <see cref="UParameterAlterationFractions"/> based on the curvature of the given original line, representing how u-parameters should be shifted based on line net curvature.
        /// </summary>
        /// <param name="originalLinePointList">List of original line points</param>
        public static UParameterAlterationFractions GetUParameterAlterationFractions(SegmentwiseLinePointListUV originalLinePointList)
        {
            IList<LinePointUV> originalPoints = originalLinePointList.Points;
            float[] segmentCurvaturesAngles = GetSegmentCurvatureAngleValues(originalPoints);

            int indexOfMinCurvature = GetAnchorIndexFromMinimumCurvature(segmentCurvaturesAngles);

            float[] leftDeltaUFractions = new float[originalPoints.Count];
            float[] rightDeltaUFractions = new float[originalPoints.Count];


            float lefttDeltaUFraction = 0f;
            float rightDeltaUFraction = 0f;

            for (int i = indexOfMinCurvature + 1; i < originalPoints.Count; i++)
            {
                var prevAngle = segmentCurvaturesAngles[i - 1];
                var currAngle = segmentCurvaturesAngles[i];

                lefttDeltaUFraction += GetDeltaUFractionDifference(-prevAngle, -currAngle);
                rightDeltaUFraction += GetDeltaUFractionDifference(prevAngle, currAngle);

                leftDeltaUFractions[i] = lefttDeltaUFraction;
                rightDeltaUFractions[i] = rightDeltaUFraction;
            }

            lefttDeltaUFraction = 0f;
            rightDeltaUFraction = 0f;

            for (int i = indexOfMinCurvature - 1; i >= 0; i--)
            {
                var prevAngle = segmentCurvaturesAngles[i + 1];
                var currAngle = segmentCurvaturesAngles[i];

                lefttDeltaUFraction += GetDeltaUFractionDifference(-prevAngle, -currAngle);
                rightDeltaUFraction += GetDeltaUFractionDifference(prevAngle, currAngle);

                leftDeltaUFractions[i] = lefttDeltaUFraction;
                rightDeltaUFractions[i] = rightDeltaUFraction;
            }

            return new UParameterAlterationFractions(leftDeltaUFractions, rightDeltaUFractions);
        }

        /// <summary>
        /// Generates UV value for a triangulated points on the interior of an extruded surface.
        /// </summary>
        /// <param name="triangulatedPoint">The triangulated point</param>
        /// <param name="originalLinePointList">List of original line points</param>
        /// <param name="uParameterAlterationFractions">All u-parameter alteration fractions.</param>
        /// <param name="extrusionAmountAbs">Absolute value of the extrusion distance</param>
        public static Vector2 GenerateTriangulatedPointUv(Vector2 triangulatedPoint, SegmentwiseLinePointListUV originalLinePointList, UParameterAlterationFractions uParameterAlterationFractions, float extrusionAmountAbs)
        {
            Vector2 closestSegmentDiff; int closestSegmentIndex;
            var closestPointOnOriginalLine = SegmentedLineUtil.ClosestPointAlongSegmentwiseLine(triangulatedPoint, originalLinePointList, extrusionAmountAbs, out closestSegmentDiff, out closestSegmentIndex);
            Vector2 diffClosestToTriangulated = triangulatedPoint - closestPointOnOriginalLine.Point;
            float distanceFromOriginalLineSegment = diffClosestToTriangulated.magnitude;

            var originalSegmentStart = originalLinePointList.Points[closestSegmentIndex];
            var originalSegmentEnd = originalLinePointList.Points[closestSegmentIndex + 1];
            var originalSegmentFraction = LineSegmentUtil.ClosestFractionAlongSegment(originalSegmentStart.Point, originalSegmentEnd.Point, triangulatedPoint);
            bool rightSideOfSegment = LineSegmentUtil.PointIsOnRightSide(originalSegmentStart.Point, originalSegmentEnd.Point, triangulatedPoint);

            var closestSignedPerpendicularDistance = LineSegmentDistanceEstimation.GetClosestPointSignedPerpendicularDistance(originalLinePointList.Points.Count, closestSegmentDiff, diffClosestToTriangulated, distanceFromOriginalLineSegment, closestSegmentIndex, originalSegmentFraction);

            float uParameterAlteredFractional = GetUParameterAltered(originalLinePointList, uParameterAlterationFractions, closestSegmentIndex, closestSegmentDiff, diffClosestToTriangulated, originalSegmentFraction, rightSideOfSegment, closestSignedPerpendicularDistance);

            var vParameter = 0.5f + 0.5f * (closestSignedPerpendicularDistance / extrusionAmountAbs);

            Vector2 uv = new Vector2(uParameterAlteredFractional, vParameter);

            return uv;
        }

        /// <summary>
        /// Gets the altered u-parameter of a test point based on the closest point to the original line point list and u-parameter alteration fractions.
        /// </summary>
        /// <param name="originalLinePointList">List of original line points</param>
        /// <param name="uParameterAlterationFractions">All u-parameter alteration fractions.</param>
        /// <param name="closestSegmentIndex">The index of the original line segment of the closest point</param>
        /// <param name="closestSegmentDifference">Start-to-end vector of the segment containing the closest point</param>
        /// <param name="differenceClosestToPoint">Vector from the closest point to the test point</param>
        /// <param name="originalSegmentFraction">The fraction along the closest original line segment</param>
        /// <param name="rightSideOfSegment">Whether the test point is on the right-hande side of the original line segment.</param>
        /// <param name="closestSignedPerpendicularDistance">The signed perpendicular distance from the test point to the closest point on the segmentwise-defined line</param>
        private static float GetUParameterAltered(SegmentwiseLinePointListUV originalLinePointList, UParameterAlterationFractions uParameterAlterationFractions, int closestSegmentIndex, Vector2 closestSegmentDifference, Vector2 differenceClosestToPoint, float originalSegmentFraction, bool rightSideOfSegment, float closestSignedPerpendicularDistance)
        {
            bool inLineStartFan = closestSegmentIndex == 0 && originalSegmentFraction <= 0;
            bool inLineEndFan = closestSegmentIndex == originalLinePointList.Points.Count - 2 && originalSegmentFraction >= 1;
            float closestUnsignedPerpendicularDistance = Math.Abs(closestSignedPerpendicularDistance);

            var uParameterAlteredStart = GetUParameterAltered(originalLinePointList, uParameterAlterationFractions, closestSegmentIndex, rightSideOfSegment, closestUnsignedPerpendicularDistance);
            var uParameterAlteredEnd = GetUParameterAltered(originalLinePointList, uParameterAlterationFractions, closestSegmentIndex + 1, rightSideOfSegment, closestUnsignedPerpendicularDistance);

            float uParameter;
            if (inLineStartFan || inLineEndFan)
            {
                var uParameterAlteredEndpoint = inLineStartFan ? uParameterAlteredStart : uParameterAlteredEnd;
                float closestSegmentLength = closestSegmentDifference.magnitude;
                if (closestSegmentLength > 0)
                {
                    var closestToPointAlongSegmentDirection = Vector2.Dot(differenceClosestToPoint, closestSegmentDifference.normalized);
                    var deltaUPerDistance = (uParameterAlteredEnd - uParameterAlteredStart) / closestSegmentLength;
                    var deltaU = closestToPointAlongSegmentDirection * deltaUPerDistance;
                    uParameter = uParameterAlteredEndpoint + deltaU;
                }
                else
                {
                    //NB this is not an expected case
                    uParameter = uParameterAlteredEndpoint;
                }
            }
            else
            {
                var uParameterAlteredFractional = uParameterAlteredStart * (1f - originalSegmentFraction) + uParameterAlteredEnd * originalSegmentFraction;
                uParameter = uParameterAlteredFractional;
            }

            return uParameter;
        }

        /// <summary>
        /// Gets the altered u parameter of a test point, based on alteration fractions of original line points.
        /// </summary>
        /// <param name="originalLinePointList">List of original line points</param>
        /// <param name="uParameterAlterationFractions">All u-parameter alteration fractions.</param>
        /// <param name="index">Index of closest original line point.</param>
        /// <param name="rightSideOfSegment">Whether the test point is on the right side of the original line.</param>
        /// <param name="closestUnsignedPerpendicularDistance">The unsigned perpendicular distance from the test point to the closest point on the segmentwise-defined line</param>
        private static float GetUParameterAltered(SegmentwiseLinePointListUV originalLinePointList, UParameterAlterationFractions uParameterAlterationFractions, int index, bool rightSideOfSegment, float closestUnsignedPerpendicularDistance)
        {
            var originalUParameter = originalLinePointList.Points[index].UV.x;
            var alterationFractionAmount = uParameterAlterationFractions.GetAlterationFraction(index, rightSideOfSegment);
            return originalUParameter + alterationFractionAmount * closestUnsignedPerpendicularDistance;
        }

        /// <summary>
        /// Returns the anchoring index (from which to start curvature alteration) based on the minimum segmentwise curvature
        /// </summary>
        /// <param name="segmentCurvaturesAngles">Segmentwise curvature values obtained from a set of line points</param>
        private static int GetAnchorIndexFromMinimumCurvature(float[] segmentCurvaturesAngles)
        {
            int indexOfMinCurvature = segmentCurvaturesAngles.IndexOfMin(FloatAbsComparison, minIndexInclusive: 1, maxIndexExclusive: segmentCurvaturesAngles.Length - 1);

            //If close to the end points, just start at the end points (note that indices 0, segmentCurvaturesAngles.Length - 1 do not have usual curvature values)
            if (indexOfMinCurvature == 1)
            {
                indexOfMinCurvature = 0;
            }
            else if (indexOfMinCurvature == segmentCurvaturesAngles.Length - 2)
            {
                indexOfMinCurvature = segmentCurvaturesAngles.Length - 1;
            }

            return indexOfMinCurvature;
        }

        /// <summary>
        /// Gets the change in u parameter, as fraction of test point extrusion distance, from the previous line point to the current line point.
        /// </summary>
        /// <param name="previousPointCurvatureAngle">The curvature angle at the previous point.</param>
        /// <param name="nextPointCurvatureAngle">The curvature angle at the current point.</param>
        private static float GetDeltaUFractionDifference(float previousPointCurvatureAngle, float nextPointCurvatureAngle)
        {
            //NB Here we are not distinguishing between positive and negative angles; ie. approximate that the change in length for concave angles is also proportional to the angle.
            return 0.5f * (previousPointCurvatureAngle + nextPointCurvatureAngle);
        }

        /// <summary>
        /// Returns the comparison of the absolute values of two values.
        /// </summary>
        /// <param name="x">The first value.</param>
        /// <param name="y">The second value.</param>
        private static int FloatAbsComparison(float x, float y)
        {
            return Math.Abs(x).CompareTo(Math.Abs(y));
        }

        /// <summary>
        /// Gets an array of segmentwise curvature values from a set of line points.
        /// </summary>
        /// <param name="linePoints">The line points.</param>
        private static float[] GetSegmentCurvatureAngleValues(IList<LinePointUV> linePoints)
        {
            float[] segmentCurvatures = new float[linePoints.Count];
            segmentCurvatures[0] = 0f;//first point has no previous segment, so curvature cannot be calculated
            segmentCurvatures[segmentCurvatures.Length - 1] = 0f;//first point has no previous segment, so curvature cannot be calculated (similar for last point)
            for (int i = 1; i < segmentCurvatures.Length - 1; i++)
            {
                segmentCurvatures[i] = CurvatureDetermination.CurvatureAngle_NoIndexCheck(linePoints, i);
            }
            return segmentCurvatures;
        }

        /// <summary>
        /// Contains u-parameter alteration (as fraction of test point to closest original line point distance) amounts.
        /// </summary>
        public class UParameterAlterationFractions
        {
            /// <summary> Contains u-parameter alteration (as fraction of test point to closest original line point distance) amounts for test points on the left side of the original line. </summary>
            public float[] LeftAlterationExtrusionFraction;
            /// <summary> Contains u-parameter alteration (as fraction of test point to closest original line point distance) amounts for test points on the right side of the original line. </summary>
            public float[] RightAlterationExtrusionFraction;

            /// <summary>
            /// Gets the u-parameter alteration fraction for a test point relative to a given original line point.
            /// </summary>
            /// <param name="index">Index of the original line point.</param>
            /// <param name="rightSide">Whether the test point is on the right side of the original line.</param>
            public float GetAlterationFraction(int index, bool rightSide)
            {
                if (rightSide)
                {
                    return RightAlterationExtrusionFraction[index];
                }
                else
                {
                    return LeftAlterationExtrusionFraction[index];
                }
            }

            /// <summary>
            /// Creates a new instance of <see cref="UParameterAlterationFractions"/> based on left- and right-side alteration fractions.
            /// </summary>
            /// <param name="leftAlterationExtrusionFraction"></param>
            /// <param name="rightAlterationExtrusionFraction"></param>
            public UParameterAlterationFractions(float[] leftAlterationExtrusionFraction, float[] rightAlterationExtrusionFraction)
            {
                LeftAlterationExtrusionFraction = leftAlterationExtrusionFraction;
                RightAlterationExtrusionFraction = rightAlterationExtrusionFraction;
            }
        }
    }
}