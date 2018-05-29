using UnityEngine;
using BabyDinoHerd.Extrusion.Line.Geometry;
using BabyDinoHerd.Extrusion.Line.Configuration;
using BabyDinoHerd.Extrusion.Line.Extrusion;
using BabyDinoHerd.Extrusion.Line.TextureMapping.Experimental;

namespace BabyDinoHerd.Extrusion.Line.Triangulation.MultipleContour.Experimental
{
    /// <summary>
    /// Triangulates an outer extruded contour with inner hole contours, basing uvs on net original line curvature.
    /// </summary>
    public class MultipleContourTriangulationUVFromOriginalLineCurvature : MultipleContourTriangulationBase
    {
        /// <summary> 
        /// Whether triangulation uses uv-altered extruded contour uvs.
        /// </summary>
        [BabyDinoHerd.Experimental]
        public override bool HasUvAlteration
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Generates UV values for triangulated points on the interior of an extruded surface, based on net original line curvature.
        /// </summary>
        /// <param name="triangulatedPoints">The triangulated points</param>
        /// <param name="lineExtrusionResults">Line extrusion resulting contours.</param>
        /// <param name="originalLinePointsList">Original line points</param>
        /// <param name="extrusionConfiguration">The extrusion configuration parameters</param>
        protected override Vector2[] GenerateTriangulatedPointUVs(Vector3[] triangulatedPoints, LineExtrusionResults lineExtrusionResults, SegmentwiseLinePointListUV originalLinePointsList, LineExtrusionConfiguration extrusionConfiguration)
        {
            Vector2[] uvs = new Vector2[triangulatedPoints.Length];

            var uParameterAlterationFractions = PointUVGenerationFromOriginalLineCurvature.GetUParameterAlterationFractions(originalLinePointsList);
            float extrusionAmountAbs = Mathf.Abs(extrusionConfiguration.ExtrusionAmount);

            for (int i = 0; i < uvs.Length; i++)
            {
                var triangulatedPoint = triangulatedPoints[i];
                uvs[i] = PointUVGenerationFromOriginalLineCurvature.GenerateTriangulatedPointUv(triangulatedPoint, originalLinePointsList, uParameterAlterationFractions, extrusionAmountAbs);
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
    }
}