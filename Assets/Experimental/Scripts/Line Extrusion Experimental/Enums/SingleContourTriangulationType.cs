namespace BabyDinoHerd.Extrusion.Line.Enums.Experimental
{
    /// <summary>
    /// Describes the method for determining how a single contour obtained from extrusion is triangulated and uvs are generated.
    /// </summary>
    [BabyDinoHerd.Experimental]
    public enum SingleContourTriangulationType
    {
        Default,
        OriginalLineAndAlteredExtrudedParameters,
        ConnectedSegmentsOriginalLineAndAlteredExtrudedParameters,
        OriginalLineWeightedSegments,
        OriginalLineCurvature,
    }
}