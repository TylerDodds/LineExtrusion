namespace BabyDinoHerd.Extrusion.Line.Geometry.ChunkConnection
{
    /// <summary>
    /// Extruded chunk conneector that averages intersection points equally.
    /// </summary>
    public class ExtrudedChunkContourConnector_AveragedEqually : ExtrudedChunkContourConnnectorBase
    {
        /// <summary>
        /// Determines the intersection point calculated from the one at the start of a given chunk with the intersection point at the end of the previous chunk.
        /// </summary>
        /// <param name="currentChunkToAdd">The current chunk of points under consideration</param>
        /// <param name="intersectionPointAtStart">The intersection point at the start of the current chunk</param>
        /// <param name="previousChunk">The previous chunk of points</param>
        protected override Vector2WithUV DetermineChunkStartIntersectionPointWithRecalculatedUVs(ChunkBetweenIntersections currentChunkToAdd, Vector2WithUV intersectionPointAtStart, ChunkBetweenIntersections previousChunk)
        {
            return AverageIntersectionPointsEqually(currentChunkToAdd, intersectionPointAtStart, previousChunk);
        }

        /// <summary>
        /// Averages the intersection point at the start of this chunk with the intersection point at the end of the previous chunk, weighted equally.
        /// </summary>
        /// <param name="currentChunkToAdd">The current chunk of points under consideration</param>
        /// <param name="intersectionPointAtStart">The intersection point at the start of the current chunk</param>
        /// <param name="previousChunk">The previous chunk of points</param>
        private static Vector2WithUV AverageIntersectionPointsEqually(ChunkBetweenIntersections currentChunkToAdd, Vector2WithUV intersectionPointAtStart, ChunkBetweenIntersections previousChunk)
        {
            var intersectionPointAtPreviousEnd = new Vector2WithUV(previousChunk.EndIntersection);
            var averagedIntersectionPointAtStart = intersectionPointAtStart.AverageWith(intersectionPointAtPreviousEnd, 0.5f);
            return averagedIntersectionPointAtStart;
        }
    }
}