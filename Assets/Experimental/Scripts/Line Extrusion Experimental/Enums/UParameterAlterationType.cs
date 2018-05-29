namespace BabyDinoHerd.Extrusion.Line.Enums.Experimental
{
    /// <summary>
    /// Describes the method for determining how u parameters of contours are altered from their original values.
    /// </summary>
    [BabyDinoHerd.Experimental]
    public enum UParameterAlterationType
    {
        Default,
        None,
        CurvatureLocalMaxima,
        CurvatureRiseAndFall,
        CurvatureLargeValueMidpoints,
        CurvatureInflectionPoints,
        SpringApproximation,
        Convolution,
    }
}