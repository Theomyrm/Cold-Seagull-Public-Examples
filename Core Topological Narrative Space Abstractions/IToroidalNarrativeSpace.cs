using System;
using SeagullEngine.Math.Topology;

namespace SeagullEngine.Math.NarrativeSpaces
{
    /// <summary>
    /// Specialized narrative space interface for toroidal topology integration.
    /// Extends the base narrative space with toroidal-specific operations
    /// and direct access to the underlying topological engine.
    /// 
    /// Mathematical Foundation:
    /// - Dual distance calculations: base topology (n-dimensional) and carousel (extended)
    /// - Bidirectional mapping between carousel positions and personality coordinates
    /// - Gradient-aware positioning within personality arcs
    /// - Toroidal displacement operations with wraparound behavior
    /// </summary>
    /// <typeparam name="TPersonality">Personality enum type for topological mapping</typeparam>
    public interface IToroidalNarrativeSpace<TPersonality> : INarrativeSpace<TPersonality>
        where TPersonality : struct, IConvertible
    {
        /// <summary>
        /// Access to the underlying toroidal topological space.
        /// Provides direct access to mathematical operations and personality calculations.
        /// </summary>
        ITopologicalSpace<TPersonality> TopologyEngine { get; }

        /// <summary>
        /// Map a personality and gradient position to a carousel spatial index.
        /// Combines personality arc positioning with fine-grained gradient placement.
        /// </summary>
        /// <param name="personality">Base personality for arc selection</param>
        /// <param name="gradientPosition">Position within personality arc [0.0, 1.0]</param>
        /// <returns>Spatial index in carousel coordinates</returns>
        int MapToCarousel(TPersonality personality, float gradientPosition);

        /// <summary>
        /// Map a carousel spatial index back to personality and gradient components.
        /// Reverse operation for extracting personality context from spatial position.
        /// </summary>
        /// <param name="carouselPosition">Spatial index in carousel coordinates</param>
        /// <returns>Tuple containing (personality, gradient) components</returns>
        (TPersonality personality, float gradient) MapFromCarousel(int carouselPosition);

        /// <summary>
        /// Calculate distance using base topological space (n-dimensional).
        /// Provides precise personality relationship calculations.
        /// </summary>
        /// <param name="personality1">First personality</param>
        /// <param name="personality2">Second personality</param>
        /// <returns>Topological distance between personalities</returns>
        float CalculateTopologicalDistance(TPersonality personality1, TPersonality personality2);

        /// <summary>
        /// Calculate extended distance in carousel space with gradient awareness.
        /// Combines topological distance with fine-grained spatial positioning.
        /// </summary>
        /// <param name="position1">First carousel position</param>
        /// <param name="position2">Second carousel position</param>
        /// <returns>Extended distance including gradient components</returns>
        float CalculateCarouselDistance(int position1, int position2);

        /// <summary>
        /// Calculate centroid using base topological operations.
        /// Provides mathematically precise personality-based centroids.
        /// </summary>
        /// <param name="personalityWeights">Weighted personality distribution</param>
        /// <returns>Complex centroid in topological space</returns>
        ComplexCentroid CalculateTopologicalCentroid(System.Collections.Generic.Dictionary<TPersonality, float> personalityWeights);

        /// <summary>
        /// Perform intense spatial jump with controlled parameters.
        /// Enables dramatic narrative transitions while maintaining topological constraints.
        /// </summary>
        /// <param name="currentPosition">Starting carousel position</param>
        /// <param name="intensity">Jump intensity factor [0.0, 1.0]</param>
        /// <param name="jumpFactor">Multiplication factor for jump distance</param>
        /// <returns>Target carousel position after intense jump</returns>
        int CalculateIntenseJump(int currentPosition, float intensity, int jumpFactor);

        /// <summary>
        /// Check if a spatial position requires dynamic compression.
        /// Determines when multiple elements should be represented as centroids.
        /// </summary>
        /// <param name="position">Spatial position to analyze</param>
        /// <returns>True if position exceeds compression threshold</returns>
        bool RequiresCompression(int position);

        /// <summary>
        /// Apply dynamic compression to a crowded spatial position.
        /// Replaces multiple elements with representative centroids for performance.
        /// </summary>
        /// <param name="position">Spatial position to compress</param>
        void CompressPosition(int position);

        /// <summary>
        /// Decompress a previously compressed spatial position.
        /// Restores individual elements from centroid representation.
        /// </summary>
        /// <param name="position">Spatial position to decompress</param>
        void DecompressPosition(int position);
    }
}
