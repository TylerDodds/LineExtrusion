using UnityEngine;
using BabyDinoHerd.Extrusion.Line.Geometry;
using BabyDinoHerd.Extrusion.Line.Configuration;

namespace BabyDinoHerd.Extrusion.Line.Triangulation.SingleContour.Experimental
{
    /// <summary>
    /// Single-contour triangulation using original line points and extruded points with their altered UVs.
    /// </summary>
    [BabyDinoHerd.Experimental]
    public class SingleContourTriangulationBetweenOriginalAndExtrudedPoints : LineToExtrudedPointTriangulationBase
    {
        /// <summary>
        /// Get an array of uv vectors from those of original line points, followed by extruded points, with their altered UVs.
        /// </summary>
        /// <param name="originalLinePointList">Original line points</param>
        /// <param name="extrudedVectorsUvAltered">Extruded points with altered uvs</param>
        /// <param name="extrusionConfiguration">The extrusion configuration parameters</param>
        protected override Vector2[] GetUVsFromLineAndExtruded(SegmentwiseLinePointListUV originalLinePointList, Vector2WithUV[] extrudedVectorsUvAltered, LineExtrusionConfiguration extrusionConfiguration)
        {
            var originalLinePoints = originalLinePointList.Points;
            var numOriginalLinePoints = originalLinePoints.Count;
            Vector2[] uvs = new Vector2[extrudedVectorsUvAltered.Length + numOriginalLinePoints];
            for (int i = 0; i < numOriginalLinePoints; i++)
            {
                uvs[i] = originalLinePoints[i].UV;
            }
            for (int i = 0; i < extrudedVectorsUvAltered.Length; i++)
            {
                uvs[i + numOriginalLinePoints] = extrudedVectorsUvAltered[i].UV;
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
                return true;
            }
        }
    }
}