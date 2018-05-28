namespace BabyDinoHerd.Extrusion.Line.Geometry
{
    /// <summary>
    /// A pair of <see cref="IntersectionPoint"/> usually encapsulating a distance range along a line.
    /// </summary>
    struct IntersectionPointPair
    {
        /// <summary>
        /// The start intersection point.
        /// </summary>
        public IntersectionPoint Start;

        /// <summary>
        /// The end intersection point.
        /// </summary>
        public IntersectionPoint End;

        /// <summary>
        /// Creates an instance of <see cref="IntersectionPointPair"/> with values for its public fields.
        /// </summary>
        /// <param name="start">The start intersection point.</param>
        /// <param name="end">The end intersection point.</param>
        public IntersectionPointPair(IntersectionPoint start, IntersectionPoint end)
        {
            Start = start;
            End = end;
        }

        public override string ToString()
        {
            return string.Format("{0} :: {1}", Start, End);
        }
    }
}