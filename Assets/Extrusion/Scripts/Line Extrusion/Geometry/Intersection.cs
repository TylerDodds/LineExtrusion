namespace BabyDinoHerd.Extrusion.Line.Geometry
{
    /// <summary>
    /// Minimal information about intersection between two line segments.
    /// </summary>
    struct Intersection
    {
        /// <summary>
        /// The index of the start point of the first line segment involved in the intersection.
        /// </summary>
        public int FirstSegmentIndex;

        /// <summary>
        /// The index of the start point of the second line segment involved in the intersection.
        /// </summary>
        public int SecondSegmentIndex;

        /// <summary>
        /// The fraction along the first segment where the intersection occurs.
        /// </summary>
        public float FirstSegmentFraction;

        /// <summary>
        /// The fraction along the second segment where the intersection occurs.
        /// </summary>
        public float SecondSegmentFraction;

        /// <summary>
        /// Creates an instance of <see cref="Intersection"/> with values for its public fields.
        /// </summary>
        /// <param name="firstSegmentIndex">The index of the start point of the first line segment involved in the intersection.</param>
        /// <param name="secondSegmentIndex">The index of the start point of the second line segment involved in the intersection.</param>
        /// <param name="firstSegmentFraction">The fraction along the first segment where the intersection occurs.</param>
        /// <param name="secondSegmentFraction">The fraction along the second segment where the intersection occurs.</param>
        public Intersection(int firstSegmentIndex, int secondSegmentIndex, float firstSegmentFraction, float secondSegmentFraction)
        {
            FirstSegmentIndex= firstSegmentIndex;
            SecondSegmentIndex = secondSegmentIndex;
            FirstSegmentFraction = firstSegmentFraction;
            SecondSegmentFraction = secondSegmentFraction;
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", SecondSegmentIndex, FirstSegmentFraction, SecondSegmentFraction);
        }
    }
}