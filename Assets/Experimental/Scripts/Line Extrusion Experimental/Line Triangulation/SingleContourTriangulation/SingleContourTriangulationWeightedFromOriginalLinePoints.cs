using UnityEngine;
using BabyDinoHerd.Extrusion.Line.Geometry;
using BabyDinoHerd.Extrusion.Line.Configuration;
using BabyDinoHerd.Extrusion.Line.TextureMapping.Experimental;

namespace BabyDinoHerd.Extrusion.Line.Triangulation.SingleContour.Experimental
{
    /// <summary>
    /// Single-contour triangulation using original line points and extruded points with altered UVs based on distance to original line segments.
    /// </summary>
    [BabyDinoHerd.Experimental]
    public class SingleContourTriangulationWeightedFromOriginalLinePoints : LineToExtrudedPointTriangulationBase
    {
        /// <summary>
        /// Get an array of uv vectors from those of original line points, followed by extruded points, weighted based on distance to original line segments.
        /// </summary>
        /// <param name="originalLinePointList">Original line points</param>
        /// <param name="extrudedVectorsUvAltered">Extruded points with altered uvs</param>
        /// <param name="extrusionConfiguration">The extrusion configuration parameters</param>
        protected override Vector2[] GetUVsFromLineAndExtruded(SegmentwiseLinePointListUV originalLinePointList, Vector2WithUV[] extrudedVectorsUvAltered, LineExtrusionConfiguration extrusionConfiguration)
        {
            var originalLinePoints = originalLinePointList.Points;
            var numOriginalLinePoints = originalLinePoints.Count;
            Vector2[] uvs = new Vector2[extrudedVectorsUvAltered.Length + numOriginalLinePoints];

            float extrusionAmount = extrusionConfiguration.ExtrusionAmount;
            for (int i = 0; i < numOriginalLinePoints; i++)
            {
                uvs[i] = PointUVGenerationWeightedFromOriginalLineSegments.EstimatePointUVFromOriginalLineSegments(originalLinePoints[i].Point, originalLinePointList, extrusionAmount);
            }
            for (int i = 0; i < extrudedVectorsUvAltered.Length; i++)
            {
                uvs[i + numOriginalLinePoints] = PointUVGenerationWeightedFromOriginalLineSegments.EstimatePointUVFromOriginalLineSegments(extrudedVectorsUvAltered[i].Vector, originalLinePointList, extrusionAmount);
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