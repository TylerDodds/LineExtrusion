using System.Collections.Generic;

namespace BabyDinoHerd.Extrusion.Line.Geometry
{
    /// <summary>
    /// A collection of <see cref="ChunkBetweenIntersections"/>.
    /// </summary>
    public class ChunkBetweenIntersectionsCollection
    {
        /// <summary>
        /// The chunks that comprise this collection.
        /// </summary>
        public List<ChunkBetweenIntersections> Chunks;

        /// <summary>
        /// The total number of points in this collection.
        /// </summary>
        public int TotalNumberOfPoints { get { return GetTotalNumberOfPoints(); } }

        /// <summary>
        /// Gets the total number of points in this collection.
        /// </summary>
        private int GetTotalNumberOfPoints()
        {
            int num = 0;
            for (int i = 0; i < Chunks.Count; i++)
            {
                num++;//For start point of chunk
                num += Chunks[i].ExtrudedPoints.Count;
            }
            num++;//For end point after all chunks
            return num;
        }

        /// <summary>
        /// Creates a new instance of <see cref="ChunkBetweenIntersectionsCollection"/> based on a list of <see cref="ChunkBetweenIntersections"/>.
        /// </summary>
        /// <param name="chunksBetweenIntersection">The chunks that comprise this collection.</param>
        public ChunkBetweenIntersectionsCollection(List<ChunkBetweenIntersections> chunksBetweenIntersection)
        {
            Chunks = chunksBetweenIntersection;
        }

        public override string ToString()
        {
            return string.Format("{0} Chunks With {1} Points", Chunks.Count, TotalNumberOfPoints);
        }
    }
}