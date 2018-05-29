namespace BabyDinoHerd.Extrusion.Line.Geometry.ChunkConnection.Experimental
{
    /// <summary>
    /// Extruded chunk conneector that takes the intersection point at the start of any given chunk, not averaging with the intersection point at the end of the previous chunk.
    /// </summary>
    [BabyDinoHerd.Experimental]
    public class ExtrudedChunkContourConnector_KeepStart : ExtrudedChunkContourConnnectorBase
    {
        /// <summary>
        /// Determines the intersection point calculated from the one at the start of a given chunk with the intersection point at the end of the previous chunk.
        /// </summary>
        /// <param name="currentChunkToAdd">The current chunk of points under consideration</param>
        /// <param name="intersectionPointAtStart">The intersection point at the start of the current chunk</param>
        /// <param name="previousChunk">The previous chunk of points</param>
        protected override Vector2WithUV DetermineChunkStartIntersectionPointWithRecalculatedUVs(ChunkBetweenIntersections currentChunkToAdd, Vector2WithUV intersectionPointAtStart, ChunkBetweenIntersections previousChunk)
        {
            return KeepIntersectionPointAtStart(currentChunkToAdd, intersectionPointAtStart, previousChunk);
        }

        /// <summary>
        /// Returns the intersection point at the start of this chunk, ignoring the intersection point at the end of the previous chunk.
        /// </summary>
        /// <param name="currentChunkToAdd">The current chunk of points under consideration</param>
        /// <param name="intersectionPointAtStart">The intersection point at the start of the current chunk</param>
        /// <param name="previousChunk">The previous chunk of points</param>
        private static Vector2WithUV KeepIntersectionPointAtStart(ChunkBetweenIntersections currentChunkToAdd, Vector2WithUV intersectionPointAtStart, ChunkBetweenIntersections previousChunk)
        {
            return intersectionPointAtStart;
        }
    }
}