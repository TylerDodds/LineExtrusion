using UnityEngine;
using BabyDinoHerd.Extrusion.Line.Geometry;

namespace BabyDinoHerd.Extrusion.Line.TextureMapping.Experimental
{
    /// <summary>
    /// Generates UV values for points based on their positions relative to a segmentwise-defined line.
    /// </summary>
    [BabyDinoHerd.Experimental]
    public class PointUVGenerationWeightedFromOriginalLineSegments
    {
        /// <summary>
        /// Determines the UV value for a point based on its position to a segmentwise-defined line (along arclength distance, and perpendicular distance).
        /// </summary>
        /// <param name="point">The point position</param>
        /// <param name="originalLinePointList">Segmentwise-define line points</param>
        /// <param name="extrusionAmount">The extrusion amount (expect points to be within this distance from the line)</param>
        public static Vector2 EstimatePointUVFromOriginalLineSegments(Vector2 point, SegmentwiseLinePointListUV originalLinePointList, float extrusionAmount)
        {
            float extrusionAmountAbs = Mathf.Abs(extrusionAmount);

            float weightTotal = 0f;
            float uParameter = 0f;

            var originalLinePoints = originalLinePointList.Points;

            float distanceToClosetPointOnLine;
            float smallestSignedPerpendicularDistance = LineSegmentDistanceEstimation.GetClosestPointSignedPerpendicularDistance(point, originalLinePointList, extrusionAmountAbs, out distanceToClosetPointOnLine);

            int numberPoints = originalLinePoints.Count - 1;
            for (int i = 0; i < numberPoints; i++)
            {
                var segmentStart = originalLinePoints[i];
                var segmentEnd = originalLinePoints[i + 1];
                Vector2 segmentStartPoint = segmentStart.Point;
                Vector2 segmentEndPoint = segmentEnd.Point;
                Vector2 segmentDiff = segmentEndPoint - segmentStartPoint;
                Vector2 pointDiff = point - segmentStartPoint;

                if (segmentDiff.sqrMagnitude > 0f)
                {
                    Vector2 segmentDirection = segmentDiff.normalized;
                    float segmentLength = segmentDiff.magnitude;
                    var distanceAlongSegment = Vector2.Dot(pointDiff, segmentDirection);
                    float fractionUnclamped = distanceAlongSegment / segmentLength;

                    Vector2 segmentNormal = NormalUtil.NormalFromTangent(segmentDirection);
                    Vector2 pointDiffPerpendicular = pointDiff - distanceAlongSegment * segmentDirection;
                    float perpendicularDistanceSigned = pointDiffPerpendicular.magnitude * Mathf.Sign(Vector2.Dot(pointDiffPerpendicular, segmentNormal));

                    float estimatedUParameterFromSegment = fractionUnclamped * (segmentEnd.UV.x - segmentStart.UV.x) + segmentStart.UV.x;

                    float weightFromDistanceAlongSegment = GetWeightFromDistanceAlongSegment(distanceAlongSegment, segmentLength, extrusionAmountAbs);
                    float weightFromDistancePerpendicularToSegment = GetWeightFromDistancePerpendicularToSegment(perpendicularDistanceSigned, smallestSignedPerpendicularDistance, extrusionAmountAbs);

                    float currentWeight = weightFromDistanceAlongSegment * weightFromDistancePerpendicularToSegment;

                    uParameter += estimatedUParameterFromSegment * currentWeight;
                    weightTotal += currentWeight;
                }
                else
                {
                    //In this unexpected case, this "segment" will not contribute.
                }
            }

            if (weightTotal > 0f)
            {
                uParameter = uParameter / weightTotal;
            }

            var vParameter = 0.5f + 0.5f * smallestSignedPerpendicularDistance / extrusionAmountAbs;

            return new Vector2(uParameter, vParameter);
        }

        /// <summary>
        /// Determines parameter weighting based on the point's relative distance along a segment of the original line.
        /// </summary>
        /// <param name="distanceAlongSegment">The distance of the point along the segment (may be negative or larger than the segment length).</param>
        /// <param name="segmentLength">The length of the segment.</param>
        /// <param name="extrusionAmountAbs">The extrusion amount absolute value.</param>
        private static float GetWeightFromDistanceAlongSegment(float distanceAlongSegment, float segmentLength, float extrusionAmountAbs)
        {
            float weight;

            float distanceScale = extrusionAmountAbs / 4f;

            if (distanceAlongSegment < 0)
            {
                weight = IntegratedDistanceWeight(segmentLength - distanceAlongSegment, distanceScale) - IntegratedDistanceWeight(-distanceAlongSegment, distanceScale);
            }
            else if(distanceAlongSegment > segmentLength)
            {
                weight = IntegratedDistanceWeight(distanceAlongSegment, distanceScale) - IntegratedDistanceWeight(distanceAlongSegment - segmentLength, distanceScale);
            }
            else
            {
                weight = IntegratedDistanceWeight(segmentLength - distanceAlongSegment, distanceScale) + IntegratedDistanceWeight(distanceAlongSegment, distanceScale);
            }

            return weight;
        }

        /// <summary>
        /// Determines parameter weighting based on the point's perpendicular distance to a segment of the original line.
        /// </summary>
        /// <param name="distancePerpendicularSigned">The signed perpendicular distance of the point to the segment.</param>
        /// <param name="smallestSignedPerpendicularDistance">The signed perpendicular distance from the point to its closest point on the original segmentwise-defined line.</param>
        /// <param name="extrusionAmountAbs">The extrusion amount absolute value.</param>
        private static float GetWeightFromDistancePerpendicularToSegment(float distancePerpendicularSigned, float smallestSignedPerpendicularDistance, float extrusionAmountAbs)
        {
            float distanceScale = extrusionAmountAbs / 10f;

            float weight = DistanceWeight(Mathf.Abs(distancePerpendicularSigned - smallestSignedPerpendicularDistance), distanceScale);

            return weight;
        }

        /// <summary>
        /// Gets the integrated (along arclength) distance parameter weight.
        /// </summary>
        /// <param name="distanceFromWeightCenter">Distance from the weighting center position.</param>
        /// <param name="distanceScale">The distance scale.</param>
        private static float IntegratedDistanceWeight(float distanceFromWeightCenter, float distanceScale)
        {
            return IntegratedExponentialWeight(distanceFromWeightCenter, distanceScale);
        }

        /// <summary>
        /// Gets the non-integrated distance parameter weight.
        /// </summary>
        /// <param name="distanceFromWeightCenter">Distance from the weighting center position.</param>
        /// <param name="distanceScale">The distance scale.</param>
        private static float DistanceWeight(float distanceFromWeightCenter, float distanceScale)
        {
            return ExponentialWeight(distanceFromWeightCenter, distanceScale);
        }

        /// <summary>
        /// Gets the integrated (along arclength) distance parameter weight, using exponential weighting.
        /// </summary>
        /// <param name="distanceFromWeightCenter">Distance from the weighting center position.</param>
        /// <param name="distanceScale">The distance scale.</param>
        private static float IntegratedExponentialWeight(float distanceFromWeightCenter, float distanceScale)
        {
            return distanceScale * (1f - Mathf.Exp(-distanceFromWeightCenter / distanceScale));
        }

        /// <summary>
        /// Gets the non-integrated distance parameter weight, using exponential weighting.
        /// </summary>
        /// <param name="distanceFromWeightCenter">Distance from the weighting center position.</param>
        /// <param name="distanceScale">The distance scale.</param>
        private static float ExponentialWeight(float distanceFromWeightCenter, float distanceScale)
        {
            return Mathf.Exp(-distanceFromWeightCenter / distanceScale);
        }
    }
}