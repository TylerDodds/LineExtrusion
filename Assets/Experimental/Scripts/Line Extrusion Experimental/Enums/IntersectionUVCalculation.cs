namespace BabyDinoHerd.Extrusion.Line.Enums.Experimental
{
    /// <summary>
    /// Describes the method for determining how UV parameters of extruded intersection points are calculated.
    /// </summary>
    [BabyDinoHerd.Experimental]
    public enum IntersectionUVCalculation
    {
        Default,
        KeepStartPointUV,
        KeepEndPointUV,
        AveragedFromArcdistance,
        AveragedEqually,
    }
}