using UnityEngine;
using SeagullEngine.DSL.Core;

namespace SeagullEngine.DSL.SeagullNoLonger.Core
{
    /// <summary>
    /// Unified DSL execution context for SeagullNoLonger DSL system.
    /// 
    /// Design Philosophy:
    /// - Composes core capability interfaces (IDSLAnimationContext, IDSLAudioContext)
    /// - Adds minimal DSL-specific properties (speaker, listener, personality)
    /// - Zero redundancy with core DSL capabilities
    /// - Extension point for future template resolution (ITemplateResolver integration)
    /// 
    /// Usage:
    /// - Commands use this interface for execution callbacks
    /// - Resolvers use this for personality-driven word selection
    /// - Effect applicators use this for applying game effects
    /// - Future: Template resolvers will use this for hierarchical template resolution
    /// 
    /// Integration:
    /// - Inherits IDSLAnimationContext for animation triggers
    /// - Inherits IDSLAudioContext for sound/music playback
    /// - Adds SpeakerId/ListenerId for narrative context
    /// - Adds PersonalityVector/TemperamentIndex for coherence
    /// - Provides ApplyEffect for game effect integration
    /// </summary>
    public interface IDSLContext : IDSLAnimationContext, IDSLAudioContext
    {
        #region Narrative Identity

        /// <summary>Identifier of the speaking entity (character, NPC, agent)</summary>
        string SpeakerId { get; }

        /// <summary>Identifier of the listening/target entity</summary>
        string ListenerId { get; }

        #endregion

        #region Personality & Coherence

        /// <summary>
        /// 4D personality vector for semantic coherence.
        /// Used by word pool resolvers to filter semantically compatible words.
        /// </summary>
        Vector4 PersonalityVector { get; }

        /// <summary>
        /// Temperament index for word pool filtering [0, n-1].
        /// Maps to personality configurations (e.g., Calm, Aggressive, Playful).
        /// </summary>
        int TemperamentIndex { get; }

        #endregion

        #region Game Effects

        /// <summary>
        /// Apply a game effect to the target entity.
        /// Effects are accumulated and applied based on game-specific logic.
        /// </summary>
        /// <param name="effectName">Effect identifier (e.g., "damage", "morale", "relationship")</param>
        /// <param name="magnitude">Effect magnitude/strength</param>
        void ApplyEffect(string effectName, float magnitude);

        #endregion

        #region Bubble Presentation Control

        /// <summary>
        /// Request a wait before showing bubble text.
        /// Used by WaitCommand to create dramatic pauses.
        /// </summary>
        /// <param name="durationInSeconds">Wait duration in seconds</param>
        void RequestWait(float durationInSeconds);

        /// <summary>
        /// Enable typewriter effect for bubble text.
        /// Used by TypewriterCommand to activate character-by-character rendering.
        /// </summary>
        void EnableTypewriter();

        /// <summary>
        /// Disable typewriter effect for bubble text.
        /// Used by TypewriterCommand to return to instant text display.
        /// </summary>
        void DisableTypewriter();

        /// <summary>
        /// Set typewriter speed in characters per second.
        /// Used by TypewriterCommand to control text rendering speed.
        /// </summary>
        /// <param name="charsPerSecond">Characters per second</param>
        void SetTypewriterSpeed(float charsPerSecond);

        /// <summary>
        /// Request a pause during typewriter effect.
        /// Used by PauseCommand to create mid-sentence pauses.
        /// </summary>
        /// <param name="durationInSeconds">Pause duration in seconds</param>
        void RequestTypewriterPause(float durationInSeconds);

        #endregion

        #region Future Extension Points

        // Reserved for ITemplateResolver integration:
        // - string ResolveTemplate(string template)
        // - string ResolvePlaceholder(string placeholder)
        // - List<string> GetCompatibleWords(string domain)

        #endregion
    }
}
