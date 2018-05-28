using BabyDinoHerd.Extrusion.Line.Geometry;
using System.Collections.Generic;

namespace BabyDinoHerd.Extrusion.Line.Interfaces
{
    /// <summary>
    /// Can produce a line based on its line segments.
    /// </summary>
    public interface ILineSegmentation
    {
        /// <summary>
        /// Gets the list of points that define the line on a line segment by line segment basis.
        /// </summary>
        List<LinePointStandard> GetLineSegmentsPoints();
    }
}