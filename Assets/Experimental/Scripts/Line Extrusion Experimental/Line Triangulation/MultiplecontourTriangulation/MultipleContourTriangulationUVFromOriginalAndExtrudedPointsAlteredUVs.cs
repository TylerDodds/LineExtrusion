using UnityEngine;
using BabyDinoHerd.Extrusion.Line.Geometry;
using BabyDinoHerd.Extrusion.Line.Configuration;
using BabyDinoHerd.Extrusion.Line.Extrusion;
using System;
using System.Collections.Generic;
using BabyDinoHerd.Extrusion.Line.TextureMapping.Experimental;

namespace BabyDinoHerd.Extrusion.Line.Triangulation.MultipleContour.Experimental
{
    /// <summary>
    /// Triangulates an outer extruded contour with inner hole contours, with uvs generated from closest original line and extruded points with altered uvs.
    /// </summary>
    public class MultipleContourTriangulationUVFromOriginalAndExtrudedPointsAlteredUVs : MultipleContourTriangulationBase
    {
        /// <summary> 
        /// Whether triangulation uses uv-altered extruded contour uvs.
        /// </summary>
        [BabyDinoHerd.Experimental]
        public override bool HasUvAlteration
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Generates UV values for triangulated points on the interior of an extruded surface, from closest original line and extruded points with altered uvs.
        /// </summary>
        /// <param name="triangulatedPoints">The triangulated points</param>
        /// <param name="lineExtrusionResults">Line extrusion resulting contours.</param>
        /// <param name="originalLinePointsList">Original line points</param>
        /// <param name="extrusionConfiguration">The extrusion configuration parameters</param>
        protected override Vector2[] GenerateTriangulatedPointUVs(Vector3[] triangulatedPoints, LineExtrusionResults lineExtrusionResults, SegmentwiseLinePointListUV originalLinePointsList, LineExtrusionConfiguration extrusionConfiguration)
        {
            Vector2[] uvs = new Vector2[triangulatedPoints.Length];
            float extrusionAmountAbs = Math.Abs(extrusionConfiguration.ExtrusionAmount);

            var extrudedContoursCurvatureAlteredPointsLists = GetExtrudedContoursCurvatureAlteredPointsLists(lineExtrusionResults);

            for (int i = 0; i < uvs.Length; i++)
            {
                var triangulatedPoint = triangulatedPoints[i];
                uvs[i] = GenerateTriangulatedPointUvBasedOnExtrudedContoursWithAlteredUVs(triangulatedPoint, lineExtrusionResults, originalLinePointsList, extrudedContoursCurvatureAlteredPointsLists, extrusionAmountAbs);
            }

            return uvs;
        }

        /// <summary>
        /// If original line points should be included in triangulation.
        /// </summary>
        protected override bool IncludeOriginalLinePoints
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// If removed extruded contours should be included in triangulation.
        /// </summary>
        protected override bool IncludeRemovedContours
        {
            get
            {
                return false;
            }
        }

        #region curvature alteration

        /// <summary>
        /// Get a list of segmentwise-defined lines from each extruded contour, where u-parameters have been altered based on curvature.
        /// </summary>
        /// <param name="lineExtrusionResults">Line extrusion results</param>
        /// 
        /// 
        private static List<SegmentwiseVector2WithUVList> GetExtrudedContoursCurvatureAlteredPointsLists(LineExtrusionResults lineExtrusionResults)
        {
            List<SegmentwiseVector2WithUVList> res = new List<SegmentwiseVector2WithUVList>();
            for (int i = 0; i < lineExtrusionResults.ContoursWithAlteredUParameters.Count; i++)
            {
                res.Add(new SegmentwiseVector2WithUVList(lineExtrusionResults.ContoursWithAlteredUParameters[i]));
            }
            return res;
        }

        #endregion

        /// <summary>
        /// Generates UV value for a triangulated points on the interior of an extruded surface, based on the original line and curvature-altered extruded contours with uv parameters altered based on curvature.
        /// </summary>
        /// <param name="triangulatedPoint">The triangulated point</param>
        /// <param name="lineExtrusionResults">Line extrusion resulting contours.</param>
        /// <param name="originalLinePointList">List of original line points</param>
        /// <param name="extrudedContoursCurvatureAlteredPointsLists">Extruded contours that form boundaries of triangulated mesh, with uv parameters altered based on curvature.</param>
        /// <param name="extrusionAmountAbs">Absolute value of the extrusion distance</param>
        private static Vector2 GenerateTriangulatedPointUvBasedOnExtrudedContoursWithAlteredUVs(Vector2 triangulatedPoint, LineExtrusionResults lineExtrusionResults, SegmentwiseLinePointListUV originalLinePointList, List<SegmentwiseVector2WithUVList> extrudedContoursCurvatureAlteredPointsLists, float extrusionAmountAbs)
        {
            Vector2 closestSegmentDiff; int closestSegmentIndex;
            var closestPointOnOriginalLine = SegmentedLineUtil.ClosestPointAlongSegmentwiseLine(triangulatedPoint, originalLinePointList, extrusionAmountAbs, out closestSegmentDiff, out closestSegmentIndex);
            var segmentDirectionOrig = closestSegmentDiff.normalized;

            Vector2 closestSegmentDiffOnExtrudedContours;
            var closestPointOnExtrudedContours = GetClosestPointOnContours(triangulatedPoint, extrudedContoursCurvatureAlteredPointsLists, extrusionAmountAbs, out closestSegmentDiffOnExtrudedContours);
            var segmentDirectionExtruded = closestSegmentDiffOnExtrudedContours.normalized;


            float intersectionFractionAlongClosestExtrudedToTriangulatedPointSegment; Vector2WithUV intersectionVectorAlongClosestExtrudedToTriangulatedPointSegment;
            bool hasIntersection = IsAnyIntersectionWithRemovedChunks(closestPointOnExtrudedContours.Vector, triangulatedPoint, lineExtrusionResults, out intersectionFractionAlongClosestExtrudedToTriangulatedPointSegment, out intersectionVectorAlongClosestExtrudedToTriangulatedPointSegment);

            Vector2 uv = Vector2.zero;
            if (hasIntersection)
            {
                uv = GetTriangulatedPointUvWithRemovedIntersection(triangulatedPoint, closestPointOnOriginalLine, segmentDirectionOrig, closestPointOnExtrudedContours, segmentDirectionExtruded, intersectionFractionAlongClosestExtrudedToTriangulatedPointSegment, intersectionVectorAlongClosestExtrudedToTriangulatedPointSegment, extrusionAmountAbs);
            }
            else
            {
                uv = PointUVGenerationBetweenOriginalAndExtrudedPoints.GetTriangulatedPointUvFromClosestOriginalAndExtrudedSegments(triangulatedPoint, closestPointOnOriginalLine, segmentDirectionOrig, closestPointOnExtrudedContours, segmentDirectionExtruded, extrusionAmountAbs);
            }

            return uv;
        }

        /// <summary>
        /// Gets the uv parameters of a triangulated point when the segment between it and the closest point on the original line does intersect a removed extruded segments.
        /// </summary>
        /// <param name="triangulatedPoint">The triangulated point.</param>
        /// <param name="closestPointOnOriginalLine">The closest point on the original line to the triangulated point.</param>
        /// <param name="closestOriginalLineSegmentDirection">The direction of segment containing the closest point on the original line.</param>
        /// <param name="closestPointOnExtrudedContours">The closest point on the extruded contours to the triangulated point.</param>
        /// <param name="closestExtrudedContoursSegmentDirection">The direction of segment containing the closest point on the extruded contours.</param>
        /// <param name="intersectionFractionAlongOrigPointToTriangulatedPointSegment">Fraction along closest point on original line to triangulated point segment where intersection occurs</param>
        /// <param name="intersectionVectorAlongOrigPointToTriangulatedPointSegment">Intersection point between closest point on original line to triangulated point segment and removed extruded segments.</param>
        /// <param name="extrusionAmountAbs">Absolute value of the extrusion distance.</param>
        private static Vector2 GetTriangulatedPointUvWithRemovedIntersection(Vector2 triangulatedPoint, LinePointUV closestPointOnOriginalLine, Vector2 closestOriginalLineSegmentDirection, Vector2WithUV closestPointOnExtrudedContours, Vector2 closestExtrudedContoursSegmentDirection, float intersectionFractionAlongOrigPointToTriangulatedPointSegment, Vector2WithUV intersectionVectorAlongOrigPointToTriangulatedPointSegment, float extrusionAmountAbs)
        {
            Vector2 uvWithoutRemovedIntersection = PointUVGenerationBetweenOriginalAndExtrudedPoints.GetTriangulatedPointUvFromClosestOriginalAndExtrudedSegments(triangulatedPoint, closestPointOnOriginalLine, closestOriginalLineSegmentDirection, closestPointOnExtrudedContours, closestExtrudedContoursSegmentDirection, extrusionAmountAbs);
            //TODO Additional work is needed to determine how UVs should behave when inside a portion of the contour belonging to removed extruded segments. This is likely an aesthetic choice.
            return new Vector2(uvWithoutRemovedIntersection.x, uvWithoutRemovedIntersection.y);
        }

        #region relationship to extruded contours

        /// <summary>
        /// Gets the closest point to <paramref name="triangulatedPoint"/> on all extruded contours (<paramref name="extrudedContoursCurvatureAlteredPointsLists"/>).
        /// </summary>
        /// <param name="triangulatedPoint">The triangulated point</param>
        /// <param name="extrudedContoursCurvatureAlteredPointsLists">Extruded contours that form boundaries of triangulated mesh, with uv parameters.</param>
        /// <param name="extrusionAmountAbs">Absolute value of the extrusion distance</param>
        /// <param name="closestSegmentDifference">Start-to-end vector of the segment containing the closest point</param>
        private static Vector2WithUV GetClosestPointOnContours(Vector2 triangulatedPoint, List<SegmentwiseVector2WithUVList> extrudedContoursCurvatureAlteredPointsLists, float extrusionAmountAbs, out Vector2 closestSegmentDiff)
        {
            float minDistanceSquared = float.MaxValue;
            Vector2WithUV fullyClosestPoint = new Vector2WithUV();
            closestSegmentDiff = new Vector2();
            for (int i = 0; i < extrudedContoursCurvatureAlteredPointsLists.Count; i++)
            {
                var contour = extrudedContoursCurvatureAlteredPointsLists[i];
                Vector2 closestSegmentDiffOnContour; int closestSegmentIndexOnContour;
                var closestPointOnThisContour = SegmentedLineUtil.ClosestPointAlongSegmentwiseLine(triangulatedPoint, contour, extrusionAmountAbs, out closestSegmentDiffOnContour, out closestSegmentIndexOnContour);
                var distanceSquared = (closestPointOnThisContour.Vector - triangulatedPoint).sqrMagnitude;
                if (distanceSquared < minDistanceSquared)
                {
                    minDistanceSquared = distanceSquared;
                    closestSegmentDiff = closestSegmentDiffOnContour;
                    fullyClosestPoint = closestPointOnThisContour;
                }
            }
            return fullyClosestPoint;
        }

        /// <summary>
        /// Determines if there is any intersection between a line segment and any extruded segments that are too close to the original line.
        /// </summary>
        /// <param name="segmentStart">Segment start point.</param>
        /// <param name="segmentEnd">Segment end point.</param>
        /// <param name="lineExtrusionResults">Line extrusion resulting contours.</param>
        /// <param name="firstSegmentFractionAtMin">Fraction along segment of the intersection, at the smallest distance, if there is any intersection.</param>
        /// <param name="intersectionVectorAtMin">Intersection point at minimum distance to the segment, if any exists.</param>
        private static bool IsAnyIntersectionWithRemovedChunks(Vector2 segmentStart, Vector2 segmentEnd, LineExtrusionResults lineExtrusionResults, out float firstSegmentFractionAtMin, out Vector2WithUV intersectionVectorAtMin)
        {
            firstSegmentFractionAtMin = float.MaxValue;
            intersectionVectorAtMin = new Vector2WithUV();

            bool anyIntersection = false;
            for (int removedChunkIndex = 0; removedChunkIndex < lineExtrusionResults.RemovedContours.Count; removedChunkIndex++)
            {
                var chunk = lineExtrusionResults.RemovedContours[removedChunkIndex];

                for (int pointIndex = 0; pointIndex < chunk.Length - 1; pointIndex++)
                {
                    Vector2 intersectionVector; float firstSegmentFraction, secondSegmentFraction;
                    Vector2WithUV chunkSegmentStart = chunk[pointIndex];
                    Vector2WithUV chunkSegmentEnd = chunk[pointIndex + 1];
                    bool isCurrentIntersection = LineSegmentUtil.GetLineSegmentIntersection(segmentStart, segmentEnd, chunkSegmentStart.Vector, chunkSegmentEnd.Vector, out intersectionVector, out firstSegmentFraction, out secondSegmentFraction);
                    if (isCurrentIntersection && firstSegmentFraction < firstSegmentFractionAtMin)
                    {
                        anyIntersection = true;
                        firstSegmentFractionAtMin = firstSegmentFraction;
                        intersectionVectorAtMin = chunkSegmentStart.AverageWith(chunkSegmentEnd, secondSegmentFraction);
                    }
                }
            }

            return anyIntersection;
        }

        #endregion
    }
}