namespace BabyDinoHerd.Extrusion.Line.Enums.Experimental
{
    /// <summary>
    /// Describes the method for determining triangulation of multiple contours and how uvs are generated.
    /// </summary>
    [BabyDinoHerd.Experimental]
    public enum MultipleContourTriangulationType
    {
        Default,
        NoUVs,
        OriginalAndExtrudedPoints,
        OriginalAndExtrudedPointsAnglewiseWeighted,
        OriginalLineCurvature,
        OriginalLineWeightedSegments,
    }
}