using UnityEngine;
using BabyDinoHerd.Extrusion.Line.Geometry;
using BabyDinoHerd.Extrusion.Line.Configuration;
using BabyDinoHerd.Extrusion.Line.TextureMapping.Experimental;

namespace BabyDinoHerd.Extrusion.Line.Triangulation.SingleContour.Experimental
{
    /// <summary>
    /// Single-contour triangulation using original line points and extruded points with altered UVs based on net original line curvature.
    /// </summary>
    [BabyDinoHerd.Experimental]
    public class SingleContourTriangulationUVFromOriginalLineCurvature : LineToExtrudedPointTriangulationBase
    {
        /// <summary>
        /// Get an array of uv vectors from those of original line points, followed by extruded points, weighted based on net original line curvature.
        /// </summary>
        /// <param name="originalLinePointList">Original line points</param>
        /// <param name="extrudedVectorsUvAltered">Extruded points with altered uvs</param>
        /// <param name="extrusionConfiguration">The extrusion configuration parameters</param>
        protected override Vector2[] GetUVsFromLineAndExtruded(SegmentwiseLinePointListUV originalLinePointList, Vector2WithUV[] extrudedVectorsUvAltered, LineExtrusionConfiguration extrusionConfiguration)
        {
            var originalLinePoints = originalLinePointList.Points;
            var numOriginalLinePoints = originalLinePoints.Count;
            Vector2[] uvs = new Vector2[extrudedVectorsUvAltered.Length + numOriginalLinePoints];
            var uParameterAlterationFractions = PointUVGenerationFromOriginalLineCurvature.GetUParameterAlterationFractions(originalLinePointList);

            float extrusionAmountAbs = Mathf.Abs(extrusionConfiguration.ExtrusionAmount);
            for (int i = 0; i < numOriginalLinePoints; i++)
            {
                uvs[i] = PointUVGenerationFromOriginalLineCurvature.GenerateTriangulatedPointUv(originalLinePoints[i].Point, originalLinePointList, uParameterAlterationFractions, extrusionAmountAbs);
            }
            for (int i = 0; i < extrudedVectorsUvAltered.Length; i++)
            {
                uvs[i + numOriginalLinePoints] = PointUVGenerationFromOriginalLineCurvature.GenerateTriangulatedPointUv(extrudedVectorsUvAltered[i].Vector, originalLinePointList, uParameterAlterationFractions, extrusionAmountAbs);
            }

            return uvs;
        }

        /// <summary> 
        /// Whether triangulation uses uv-altered extruded contour uvs.
        /// </summary>
        public override bool HasUvAlteration
        {
            get
            {
                return false;
            }
        }
    }
}