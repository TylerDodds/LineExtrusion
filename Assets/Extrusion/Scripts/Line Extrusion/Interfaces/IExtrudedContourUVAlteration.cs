using BabyDinoHerd.Extrusion.Line.Geometry;

namespace BabyDinoHerd.Extrusion.Line.Interfaces
{
    /// <summary>
    /// Gets UV-altered points of extruded contours.
    /// </summary>
    public interface IExtrudedContourUVAlteration
    {
        /// <summary>
        /// Returns a new list of points and uvs of an extruded contour, where the uv parameters have been altered to try to smooth out discontinuities due to extruded contour intersections.
        /// </summary>
        /// <param name="extrudedLinePoints">Points comprising the extruded contour.</param>
        /// <param name="extrusionAmount">The extrusion amount.</param>
        Vector2WithUV[] GetUvAlteredExtrudedContour(Vector2WithUV[] extrudedLinePoints, float extrusionAmount);
    }

}
