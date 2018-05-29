namespace BabyDinoHerd.Extrusion.Line.Curvature.Experimental
{
    /// <summary>
    /// Information for determining how uv stretching should be performed.
    /// </summary>
    [BabyDinoHerd.Experimental]
    public struct UvStretchingIndexesInformation
    {
        /// <summary>
        /// Start index of the point determining the chunk of points that should be stretched.
        /// </summary>
        public readonly int StartIndex;

        /// <summary>
        /// End index of the point determining the chunk of points that should be stretched.
        /// </summary>
        public readonly int NextIndex;

        /// <summary>
        /// If half of the previous line segment from the start point should be included in the stretching.
        /// </summary>
        public readonly bool IncludeStartHalfSegment;

        /// <summary>
        /// If half of the next line segment from the end point should be included in the stretching.
        /// </summary>
        public readonly bool IncludeEndHalfSegment;

        /// <summary>
        /// Creates a new instance of <see cref="UvStretchingIndexesInformation"/> based on its public fields.
        /// </summary>
        /// <param name="startIndex">Start index of the point determining the chunk of points that should be stretched.</param>
        /// <param name="nextIndex">End index of the point determining the chunk of points that should be stretched.</param>
        /// <param name="includeStartHalfSegment">If half of the previous line segment from the start point should be included in the stretching.</param>
        /// <param name="includeEndHalfSegment">If half of the next line segment from the end point should be included in the stretching.</param>
        public UvStretchingIndexesInformation(int startIndex, int nextIndex, bool includeStartHalfSegment, bool includeEndHalfSegment)
        {
            StartIndex = startIndex;
            NextIndex = nextIndex;
            IncludeStartHalfSegment = includeStartHalfSegment;
            IncludeEndHalfSegment = includeEndHalfSegment;
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3}", StartIndex, NextIndex, IncludeStartHalfSegment, IncludeEndHalfSegment);
        }
    }
}