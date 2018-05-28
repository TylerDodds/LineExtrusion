using BabyDinoHerd.Extrusion.Line.Geometry;
using UnityEngine;
using BabyDinoHerd.Extrusion.Line.Extrusion;
using BabyDinoHerd.Extrusion.Line.Configuration;

namespace BabyDinoHerd.Extrusion.Line.Triangulation
{
    /// <summary>
    /// Triangulates extruded contours.
    /// </summary>
    public class ExtrusionTriangulator2D
    {
        /// <summary>
        /// Triangulate a polygon based on line extrusion results and original line points from which extrusion occured.
        /// </summary>
        /// <param name="lineExtrusionResults">Results of line extrusion.</param>
        /// <param name="originalLinePointList">Original line points</param>
        /// <param name="extrusionConfiguration">The extrusion configuration parameters</param>
        public static Mesh TriangulatePolygon(LineExtrusionResults lineExtrusionResults, SegmentwiseLinePointListUV originalLinePointList, LineExtrusionConfiguration extrusionConfiguration)
        {
            PolygonHelper.OrderContourByAreaDescending(lineExtrusionResults.Contours);
            PolygonHelper.OrderContourByAreaDescending(lineExtrusionResults.ContoursWithAlteredUParameters);
            var outerThenAllInnerPointsListWithAlteredUVs = lineExtrusionResults.ContoursWithAlteredUParameters;

            Mesh mesh;
            if (outerThenAllInnerPointsListWithAlteredUVs.Count == 1)
            {
                mesh = extrusionConfiguration.GetSingleContourTriangulation().TriangulateClosedContour(lineExtrusionResults, originalLinePointList, extrusionConfiguration);
            }
            else if (outerThenAllInnerPointsListWithAlteredUVs.Count >= 1)
            {
                mesh = extrusionConfiguration.GetMultipleContourTriangulation().TriangulatePolygonWithHoleContours(lineExtrusionResults, originalLinePointList, extrusionConfiguration);
            }
            else
            {
                mesh = new Mesh();
            }
            return mesh;
        }        
    }
}
