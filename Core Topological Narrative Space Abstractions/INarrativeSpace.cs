using System;
using System.Collections.Generic;
using SeagullEngine.Math.Topology;

namespace SeagullEngine.Math.NarrativeSpaces
{
    /// <summary>
    /// Generic interface for narrative spaces that provide topological frameworks
    /// for positioning and querying narrative content. The space handles spatial
    /// relationships while leaving semantic interpretation to implementations.
    /// 
    /// Mathematical Foundation:
    /// - Spatial positions mapped to personality topology
    /// - O(1) queries through direct indexing
    /// - Dynamic centroid calculations for population analysis
    /// - Influence propagation through spatial proximity
    /// - Social arc support for non-contiguous personality regions
    /// </summary>
    /// <typeparam name="TPersonality">Personality enum type for topological mapping</typeparam>
    public interface INarrativeSpace<TPersonality>
        where TPersonality : struct, IConvertible
    {
        /// <summary>
        /// Total capacity of the narrative carousel.
        /// Defines the maximum number of spatial positions available.
        /// This value is configurable and should not be hardcoded.
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// Number of personality dimensions in the underlying topological space.
        /// Extracted dynamically from the personality enum type.
        /// </summary>
        int TotalPersonalities { get; }

        /// <summary>
        /// Division factor for mapping personality arcs within the carousel.
        /// Determines how the carousel space is partitioned among personalities.
        /// </summary>
        int DivisionFactor { get; }

        /// <summary>
        /// Get the start and end indices for a specific personality's arc in the carousel.
        /// Each personality occupies a contiguous range of spatial positions.
        /// </summary>
        /// <param name="personality">Personality to query</param>
        /// <returns>Tuple containing (startIndex, endIndex) for the personality arc</returns>
        (int startIndex, int endIndex) GetPersonalityArc(TPersonality personality);

        /// <summary>
        /// Calculate the span size of a personality's arc in the carousel.
        /// Represents the number of spatial positions assigned to the personality.
        /// </summary>
        /// <param name="personality">Personality to analyze</param>
        /// <returns>Number of spatial positions in the arc</returns>
        float GetArcSpan(TPersonality personality);

        /// <summary>
        /// Determine the dominant personality for a spatial interval.
        /// Analyzes which personality has the strongest presence across the range.
        /// </summary>
        /// <param name="startPos">Start of the interval (inclusive)</param>
        /// <param name="endPos">End of the interval (inclusive)</param>
        /// <returns>Dominant personality across the specified interval</returns>
        TPersonality GetDominantPersonality(int startPos, int endPos);

        /// <summary>
        /// Check if any units or agents occupy positions within a spatial interval.
        /// Used for collision detection and spatial availability queries.
        /// </summary>
        /// <param name="startPos">Start of the interval (inclusive)</param>
        /// <param name="endPos">End of the interval (inclusive)</param>
        /// <returns>True if interval contains any occupants</returns>
        bool HasOccupantsInInterval(int startPos, int endPos);

        /// <summary>
        /// Retrieve all narrative units within a spatial interval.
        /// Multiple units can occupy the same position for narrative variety.
        /// </summary>
        /// <param name="startPos">Start of the interval (inclusive)</param>
        /// <param name="endPos">End of the interval (inclusive)</param>
        /// <returns>Enumerable of units within the interval</returns>
        IEnumerable<INarrativeUnit> GetUnitsInInterval(int startPos, int endPos);

        /// <summary>
        /// Retrieve all agents within a spatial interval.
        /// Multiple agents can coexist at the same position.
        /// </summary>
        /// <param name="startPos">Start of the interval (inclusive)</param>
        /// <param name="endPos">End of the interval (inclusive)</param>
        /// <returns>Enumerable of agents within the interval</returns>
        IEnumerable<INarrativeAgent<TPersonality>> GetAgentsInInterval(int startPos, int endPos);

        /// <summary>
        /// Search for narrative units in a specific direction from a starting position.
        /// Enables directional queries along the personality spectrum.
        /// </summary>
        /// <param name="startPos">Starting position for search</param>
        /// <param name="direction">Search direction: +1 for forward, -1 for backward</param>
        /// <param name="maxDistance">Maximum search distance</param>
        /// <returns>Enumerable of units found in the specified direction</returns>
        IEnumerable<INarrativeUnit> FindUnitsInDirection(int startPos, int direction, int maxDistance);

        /// <summary>
        /// Get social arcs defined for a specific social identity.
        /// Each social group can have multiple non-contiguous arcs representing
        /// compatible personality ranges that may be topologically distant.
        /// 
        /// Mathematical Foundation:
        /// Social Region Ψr = ⋃_{k=1}^{m} [α_{r,k}, β_{r,k}] mod 2π
        /// where α_{r,k} and β_{r,k} are arc boundaries in carousel coordinates.
        /// </summary>
        /// <param name="socialIdentity">Social identity to query</param>
        /// <returns>Collection of arc ranges [(start, end)] in carousel coordinates</returns>
        IEnumerable<(int startPos, int endPos)> GetSocialArcs(object socialIdentity);

        /// <summary>
        /// Check if a position falls within any social arc.
        /// Used for social compatibility determination and membership queries.
        /// </summary>
        /// <param name="position">Spatial position to check</param>
        /// <param name="socialIdentity">Social identity to test</param>
        /// <returns>True if position is within social influence arcs</returns>
        bool IsInSocialTerritory(int position, object socialIdentity);

        /// <summary>
        /// Calculate centroid of all passive agents in the space.
        /// Passive agents provide spatial presence without active behavior.
        /// </summary>
        /// <returns>Complex centroid representing passive agent distribution</returns>
        ComplexCentroid GetPassiveAgentsCentroid();

        /// <summary>
        /// Calculate centroid of all active agents in the space.
        /// Active agents participate in dynamic narrative processes.
        /// </summary>
        /// <returns>Complex centroid representing active agent distribution</returns>
        ComplexCentroid GetActiveAgentsCentroid();

        /// <summary>
        /// Calculate social centroid using both functional membership and predefined arcs.
        /// Combines current agent positions with social arc definitions.
        /// Supports dual calculation: functional (current members) and territorial (arcs).
        /// </summary>
        /// <param name="socialIdentity">Social identity to analyze</param>
        /// <param name="includeArcs">Whether to include predefined social arcs in calculation</param>
        /// <returns>Complex centroid representing social influence distribution</returns>
        ComplexCentroid CalculateSocialCentroid(object socialIdentity, bool includeArcs = true);

        /// <summary>
        /// Calculate overall centroid including all agents and influences.
        /// Provides comprehensive view of spatial population distribution.
        /// </summary>
        /// <returns>Complex centroid representing total population distribution</returns>
        ComplexCentroid GetGeneralCentroid();

        /// <summary>
        /// Find the next narrative unit in spatial proximity to a position.
        /// Implements O(1) spatial search using direct indexing.
        /// </summary>
        /// <param name="currentPosition">Starting position for search</param>
        /// <param name="searchRadius">Maximum search distance from current position</param>
        /// <returns>Nearest narrative unit, or null if none found within radius</returns>
        INarrativeUnit FindNextUnit(int currentPosition, int searchRadius = 1);

        /// <summary>
        /// Search for narrative units within a specified range around a position.
        /// Returns multiple candidates for selection variety.
        /// </summary>
        /// <param name="position">Center position for search</param>
        /// <param name="minRange">Minimum search distance from center</param>
        /// <param name="maxRange">Maximum search distance from center</param>
        /// <returns>Enumerable of units within the specified range</returns>
        IEnumerable<INarrativeUnit> SearchUnitsInRange(int position, int minRange, int maxRange);

        /// <summary>
        /// Calculate population shift based on centroid analysis.
        /// Determines optimal next position based on agent distribution patterns.
        /// </summary>
        /// <param name="centroid">Reference centroid for calculation</param>
        /// <returns>Suggested spatial position for population-based movement</returns>
        int CalculatePopulationShift(ComplexCentroid centroid);

        /// <summary>
        /// Register a narrative unit at a specific spatial position.
        /// Adds the unit to the space's internal tracking structures.
        /// Position is extracted from unit.SpatialId automatically.
        /// </summary>
        /// <param name="unit">Narrative unit to register</param>
        void RegisterUnit(INarrativeUnit unit);

        /// <summary>
        /// Register an agent at a specific spatial position.
        /// Adds the agent to the space's tracking and influence systems.
        /// Position is extracted from agent.CurrentPosition automatically.
        /// </summary>
        /// <param name="agent">Agent to register</param>
        void RegisterAgent(INarrativeAgent<TPersonality> agent);

        /// <summary>
        /// Move an agent to a new spatial position.
        /// Updates tracking structures and triggers position change events.
        /// </summary>
        /// <param name="agent">Agent to move</param>
        /// <param name="newPosition">Target spatial position</param>
        void MoveAgent(INarrativeAgent<TPersonality> agent, int newPosition);

        /// <summary>
        /// Apply population-wide influence that affects multiple agents.
        /// Used for global events and systemic narrative changes.
        /// Influence is attenuated based on distance from population centroid.
        /// </summary>
        /// <param name="influence">Influence vector to apply to population</param>
        void ApplyPopulationInfluence(InfluenceVector influence);
    }
}
