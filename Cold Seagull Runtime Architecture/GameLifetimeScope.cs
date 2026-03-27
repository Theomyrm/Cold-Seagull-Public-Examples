using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using ColdSeagull.Audio;
using ColdSeagull.Core;
using ColdSeagull.Core.Encounter;
using ColdSeagull.CameraSystem;
using ColdSeagull.Narrative;
using ColdSeagull.Cards.Scripting;
using ColdSeagull.Cards.Scripting.TagCompletion;
using ColdSeagull.Cards.Scripting.Commands;
using ColdSeagull.DSL.Effects;
using ColdSeagull.Encounter.NpcBehavior;
using ColdSeagull.Encounter.Passives;
using ColdSeagull.Encounter.StatModifiers;
using ColdSeagull.Encounter.Turns;
using ColdSeagull.Playground;
using ColdSeagull.WordPoolSystem.Implementation;
using ColdSeagull.Core.Enums;
using ColdSeagull.Core.Interfaces;
using ColdSeagull.Core.Movement;
using ColdSeagull.Core.Zones;
using ColdSeagull.SemanticSystem;
using ColdSeagull.DSLIntegration.Grammar;
using ColdSeagull.DSLIntegration.Runtime;
using ColdSeagull.Injection.Scripting;
using SeagullEngine.DSL.Camera.Core;
using SeagullEngine.DSL.Sfx.Core;
using SeagullEngine.DSL.WordPool.Core;
using SeagullEngine.DSL.SeagullNoLonger.Core;
using SeagullEngine.DSL.SeagullNoLonger.Implementation;
using SeagullEngine.DSL.SeagullNoLonger.Grammar;
using SeagullEngine.Math.Topology;
using SeagullEngine.SemanticSystem.Core;
using SeagullEngine.Configuration;
using ColdSeagull.DSL.Context;
using ColdSeagull.Core.Utils;
using ColdSeagull.UI.Cards;
using ColdSeagull.UI.Encounter;
using SeagullEngine.SemanticSystem.Data;

namespace ColdSeagull.Infrastructure.DI
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameCoordinator gameCoordinatorPrefab;
        [SerializeField] private SemanticAtlas semanticAtlas;
        [SerializeField] private EngineConfiguration engineConfig;
        [SerializeField] private CardRegistryAsset cardRegistry;
        [SerializeField] private NpcBehaviorCatalog npcBehaviorCatalog;

        [Header("Passive Abilities")]
        [Tooltip("Catalog of all passive abilities. Leave empty to disable passive system.")]
        [SerializeField] private PassiveCatalog passiveCatalog;

        [Header("Injection System")]
        [Tooltip("Optional. Defines which SnLInjectorAsset scripts run when the player agent registers. " +
                 "Leave empty to disable world-state injection without errors.")]
        [SerializeField] private SnLInjectorConfig injectorConfig;

        protected override void Configure(IContainerBuilder builder)
        {
            RegisterCoreGameSystems(builder);
            RegisterInventorySystem(builder);
            RegisterSemanticSystem(builder);
            RegisterWordPoolSystems(builder);
            RegisterGrammarSystem(builder);
            RegisterGrammarStrategies(builder);
            RegisterSeagullNoLongerDSL(builder);
            RegisterNarrativeSystem(builder);
            RegisterPassiveSystem(builder);
            RegisterMovementSystems(builder);
            RegisterInjectionSystem(builder);
        }

        /// <summary>
        /// Forces eager resolution of services that subscribe to events at construction time.
        /// Runs after Awake (where the container is built) and before any gameplay MonoBehaviour Start.
        /// GameLifetimeScope inherits DefaultExecutionOrder(-5000) from LifetimeScope, so this
        /// Start() fires before all other Start() methods in the scene.
        /// </summary>
        private void Start()
        {
            Container.Resolve<NarrativeMovementController>();
            Container.Resolve<ISnLInjectorExecutor>();

            if (passiveCatalog != null)
                Container.Resolve<IPassiveRegistry>();

            // Eagerly wire StatModifierRegistry so it subscribes to round/encounter events
            // before the first encounter starts.
            Container.Resolve<IStatModifierRegistry>();
        }

        private void RegisterCoreGameSystems(IContainerBuilder builder)
        {
            builder.RegisterComponentInNewPrefab(gameCoordinatorPrefab, Lifetime.Singleton)
                .DontDestroyOnLoad()
                .AsImplementedInterfaces()
                .AsSelf();
        }

        private void RegisterInventorySystem(IContainerBuilder builder)
        {
            if (cardRegistry == null)
            {
                Debug.LogWarning("[GameLifetimeScope] CardRegistryAsset not assigned — inventory system will have no card lookup");
            }
            else
            {
                builder.RegisterInstance<ICardRegistry>(cardRegistry);
            }

            builder.Register<IInventoryService>(_ => new InventoryService(), Lifetime.Singleton);
            Debug.Log("[GameLifetimeScope] Inventory System registered successfully");
        }

        private void RegisterSemanticSystem(IContainerBuilder builder)
        {
            if (semanticAtlas == null)
            {
                Debug.LogWarning("[GameLifetimeScope] SemanticAtlas not assigned - semantic system disabled");
                return;
            }

            semanticAtlas.Initialize();
            builder.RegisterInstance<ISemanticAtlas>(semanticAtlas);

            builder.Register<ISemanticCoherenceEngine>(resolver =>
            {
                var atlas = resolver.Resolve<ISemanticAtlas>();
                return new SeagullEngine.SemanticSystem.Implementation.SemanticCoherenceEngine(
                    atlas,
                    globalCoherenceThreshold: 0.7f,
                    enableDebugLogs: true,
                    showCoherenceScores: false
                );
            }, Lifetime.Singleton);

            builder.Register<ISemanticProfileManager>(resolver =>
            {
                return new ColdSeagullSemanticProfileManager();
            }, Lifetime.Singleton);

            Debug.Log("[GameLifetimeScope] Semantic System registered successfully");
        }

        private void RegisterGrammarSystem(IContainerBuilder builder)
        {
            var grammarRegistry = Resources.Load<GrammarProfileRegistry>("Grammar/GrammarProfileRegistry");

            if (grammarRegistry == null)
            {
                Debug.LogError("[GameLifetimeScope] GrammarProfileRegistry not found at 'Resources/Grammar/GrammarProfileRegistry'. Grammar system disabled!");
                return;
            }

            builder.RegisterInstance(grammarRegistry);

            builder.Register<ColdSeagullGrammarService>(container =>
            {
                var registry = container.Resolve<GrammarProfileRegistry>();
                var profileDict = registry.GetProfileDictionary();

                return new ColdSeagullGrammarService(
                    profileRegistry: profileDict,
                    seed: 0,
                    deterministicMode: false,
                    verboseLogging: false
                );
            }, Lifetime.Singleton);

            Debug.Log($"[GameLifetimeScope] Grammar System registered successfully with {grammarRegistry.GetProfileDictionary().Count} profiles");
        }

        private void RegisterGrammarStrategies(IContainerBuilder builder)
        {
            builder.Register<Dictionary<GrammarClass, IGrammarStrategy>>(container =>
            {
                return new Dictionary<GrammarClass, IGrammarStrategy>
                {
                    [GrammarClass.Verb]      = new VerbGrammarStrategy(),
                    [GrammarClass.Noun]      = new NounGrammarStrategy(),
                    [GrammarClass.Adjective] = new AdjectiveGrammarStrategy(),
                    [GrammarClass.Adverb]    = new AdverbGrammarStrategy()
                };
            }, Lifetime.Singleton);

            builder.Register<IGrammarInference>(container =>
            {
                var registry = container.Resolve<GrammarProfileRegistry>();
                var profileDict = registry.GetProfileDictionary();

                return new UnifiedGrammarInference(
                    profileRegistry: profileDict,
                    seed: 0,
                    deterministicMode: false,
                    verboseLogging: false
                );
            }, Lifetime.Singleton);

            builder.Register<IGrammarFormSelector>(container =>
            {
                var strategies = container.Resolve<Dictionary<GrammarClass, IGrammarStrategy>>();

                return new UnifiedFormSelector(
                    strategyRegistry: strategies,
                    verboseLogging: false
                );
            }, Lifetime.Singleton);

            Debug.Log("[GameLifetimeScope] Grammar Strategies and Form Selector registered successfully");
        }

        private void RegisterWordPoolSystems(IContainerBuilder builder)
        {
            builder.Register<IWordPoolManager>(container =>
            {
                return new ColdSeagullWordPoolManager("WordPools/ColdSeagull/");
            }, Lifetime.Singleton);

            builder.Register<IPersonalCoherence>(container =>
            {
                return new SeagullPersonalCoherence(
                    coherenceThreshold: 0.75f,
                    distanceFunction: SeagullPersonalCoherence.DistanceFunction.Euclidean,
                    enableAdaptiveThreshold: false
                );
            }, Lifetime.Singleton);

            builder.Register<IFacetStrategy>(container =>
            {
                return new SeagullFacetStrategy(config: null);
            }, Lifetime.Singleton);

            builder.Register<WordPoolService>(container =>
            {
                var wordPoolManager   = container.Resolve<IWordPoolManager>();
                var personalCoherence = container.Resolve<IPersonalCoherence>();
                var facetStrategy     = container.Resolve<IFacetStrategy>();

                return new WordPoolService(
                    wordPoolManager,
                    personalCoherence,
                    facetStrategy,
                    enableFallbackBehavior: true
                );
            }, Lifetime.Singleton);
        }

        private void RegisterSeagullNoLongerDSL(IContainerBuilder builder)
        {
            builder.Register<ITopologicalSpace<int>>(container =>
            {
                int temperamentCount = ColdSeagullEnumExtensions.GetTemperamentCount();
                return new ToroidalSpace<int>(temperamentCount);
            }, Lifetime.Singleton);

            builder.Register<EnvironmentalSnapshot>(container =>
            {
                return new EnvironmentalSnapshot();
            }, Lifetime.Singleton);

            builder.Register<IDSLParser>(container =>
            {
                return new SeagullNoLongerParser();
            }, Lifetime.Singleton);

            builder.Register<ICameraCommandResolver>(container =>
            {
                var wordPoolManager = container.Resolve<IWordPoolManager>();
                return new CameraCommandResolver(wordPoolManager);
            }, Lifetime.Singleton);

            builder.Register<ISfxCommandResolver>(container =>
            {
                var wordPoolManager = container.Resolve<IWordPoolManager>();
                return new SfxCommandResolver(wordPoolManager);
            }, Lifetime.Singleton);

            // IHandInjectionService is implemented by EncounterManager (MonoBehaviour found at
            // runtime). We register it as a lazy factory so it is available when GameCommandFactory
            // is built, but doesn't require EncounterManager to already exist in the scene
            // at container build time.
            builder.Register<IHandInjectionService>(container =>
            {
                var em = FindFirstObjectByType<EncounterManager>();
                if (em == null)
                    Debug.LogWarning("[GameLifetimeScope] EncounterManager not found — hand injection commands will be no-ops.");
                return em as IHandInjectionService;
            }, Lifetime.Singleton);

            builder.Register<ICommandFactory>(container =>
            {
                ICameraCommandResolver cameraResolver = null;
                ISfxCommandResolver    sfxResolver    = null;

                try { cameraResolver = container.Resolve<ICameraCommandResolver>(); }
                catch { }

                try { sfxResolver = container.Resolve<ISfxCommandResolver>(); }
                catch { }

                var engineFactory    = new SeagullCommandFactory(cameraResolver, sfxResolver);
                var cardReg          = container.Resolve<ICardRegistry>();
                var inventoryService = container.Resolve<IInventoryService>();

                IHandInjectionService handInjection = null;
                try { handInjection = container.Resolve<IHandInjectionService>(); } catch { }

                IStatModifierRegistry modifierReg = null;
                try { modifierReg = container.Resolve<IStatModifierRegistry>(); } catch { }

                return new GameCommandFactory(engineFactory, cardReg, inventoryService, handInjection, modifierReg);
            }, Lifetime.Singleton);

            // StatModifierRegistry needs IEncounterTurnSystem which is registered in
            // RegisterNarrativeSystem. VContainer resolves lazily —
            // what matters is that both are registered before the container is built.
            builder.Register<IStatModifierRegistry>(container =>
                new StatModifierRegistry(
                    container.Resolve<IEncounterTurnSystem>(),
                    container.Resolve<IGameCoordinator>()),
                Lifetime.Singleton);

            builder.Register<IEffectApplicator>(container =>
            {
                IStatModifierRegistry statMods = null;
                try { statMods = container.Resolve<IStatModifierRegistry>(); } catch { }

                return new ColdSeagullEffectApplicator(
                    container.Resolve<IGameCoordinator>(),
                    statMods);
            }, Lifetime.Singleton);

            builder.Register<IDSLResolver>(container =>
            {
                var wordPoolManager   = container.Resolve<IWordPoolManager>();
                var personalCoherence = container.Resolve<IPersonalCoherence>();
                var commandFactory    = container.Resolve<ICommandFactory>();
                var topologicalSpace  = container.Resolve<ITopologicalSpace<int>>();
                var grammarInference  = container.Resolve<IGrammarInference>();
                var formSelector      = container.Resolve<IGrammarFormSelector>();

                ISemanticCoherenceEngine coherenceEngine = null;
                ISemanticProfileManager  profileManager  = null;
                ISemanticAtlas           atlas           = null;

                try
                {
                    coherenceEngine = container.Resolve<ISemanticCoherenceEngine>();
                    profileManager  = container.Resolve<ISemanticProfileManager>();
                    atlas           = container.Resolve<ISemanticAtlas>();
                }
                catch
                {
                    Debug.LogWarning("[GameLifetimeScope] Semantic services not available for resolver - Layer 4 disabled");
                }

                return new GrammarIntegratedResolver(
                    wordPoolManager:         wordPoolManager,
                    personalCoherence:       personalCoherence,
                    commandFactory:          commandFactory,
                    grammarInference:        grammarInference,
                    formSelector:            formSelector,
                    topologicalSpace:        topologicalSpace,
                    semanticCoherenceEngine: coherenceEngine,
                    semanticProfileManager:  profileManager,
                    semanticAtlas:           atlas,
                    verboseGrammarLogging:   false,
                    wordSelectionConfig:     engineConfig != null ? engineConfig.wordSelectionConfig : null
                );
            }, Lifetime.Singleton);

            builder.Register<IDSLExecutor>(container =>
            {
                var resolver         = container.Resolve<IDSLResolver>();
                var effectApplicator = container.Resolve<IEffectApplicator>();

                return new SeagullNoLongerExecutor(resolver, effectApplicator);
            }, Lifetime.Singleton);
        }

        private void RegisterNarrativeSystem(IContainerBuilder builder)
        {
            builder.Register<NarrativeExecutor>(container =>
            {
                var resolver        = container.Resolve<IDSLResolver>();
                var gameCoordinator = container.Resolve<IGameCoordinator>();
                return new NarrativeExecutor(resolver, gameCoordinator);
            }, Lifetime.Singleton);

            builder.RegisterComponentInHierarchy<NarrativeBrain>()
                .AsImplementedInterfaces()
                .AsSelf();

            builder.Register<ITagCompletionBus, TagCompletionBus>(Lifetime.Singleton);

            builder.Register<ICardScriptExecutor>(container =>
            {
                var narrativeExecutor = container.Resolve<NarrativeExecutor>();
                var narrativeBrain    = container.Resolve<INarrativeBrain>();
                var gameCoordinator   = container.Resolve<IGameCoordinator>();
                var effectApplicator  = container.Resolve<IEffectApplicator>();
                var sharedEnvironment = container.Resolve<EnvironmentalSnapshot>();
                var tagCompletionBus  = container.Resolve<ITagCompletionBus>();

                return new CardScriptExecutor(narrativeExecutor, narrativeBrain, gameCoordinator, effectApplicator, sharedEnvironment, tagCompletionBus);
            }, Lifetime.Singleton);

            builder.Register<IPendingConditionEvaluator>(container =>
            {
                var scriptExecutor    = container.Resolve<ICardScriptExecutor>();
                var narrativeExecutor = container.Resolve<NarrativeExecutor>();
                var effectApplicator  = container.Resolve<IEffectApplicator>();
                return new PendingConditionEvaluator(scriptExecutor, narrativeExecutor, effectApplicator);
            }, Lifetime.Singleton);

            builder.Register<ITurnOrderResolver>(_ =>
                new PersonalityVectorTurnOrderResolver(),
                Lifetime.Singleton);

            builder.Register<IEncounterTurnSystem>(container =>
                new EncounterTurnSystem(
                    container.Resolve<ITurnOrderResolver>(),
                    container.Resolve<IGameCoordinator>(),
                    container.Resolve<EnvironmentalSnapshot>()),
                Lifetime.Singleton);

            builder.Register<INpcBehaviorRegistry>(container =>
            {
                var coordinator = container.Resolve<IGameCoordinator>();

                if (npcBehaviorCatalog == null)
                    Debug.LogWarning("[GameLifetimeScope] NpcBehaviorCatalog not assigned — NPC behavior assignment will be skipped.");

                return new NpcBehaviorRegistry(npcBehaviorCatalog, coordinator);
            }, Lifetime.Singleton);

            builder.Register<INpcRelationshipRegistry>(container =>
                new NpcRelationshipRegistry(container.Resolve<IGameCoordinator>()),
                Lifetime.Singleton);

            builder.Register<ITargetSelector>(container =>
                new TargetSelector(container.Resolve<IGameCoordinator>()),
                Lifetime.Singleton);

            builder.Register<INpcTurnExecutor>(container =>
                new NpcTurnExecutor(
                    container.Resolve<ICardScriptExecutor>(),
                    container.Resolve<INpcBehaviorRegistry>(),
                    container.Resolve<ITargetSelector>(),
                    container.Resolve<INpcRelationshipRegistry>(),
                    container.Resolve<IGameCoordinator>()),
                Lifetime.Singleton);

            Debug.Log("[GameLifetimeScope] Narrative System registered successfully");
        }

        private void RegisterPassiveSystem(IContainerBuilder builder)
        {
            if (passiveCatalog == null)
            {
                Debug.LogWarning("[GameLifetimeScope] PassiveCatalog not assigned — passive system disabled.");
                return;
            }

            builder.RegisterInstance(passiveCatalog);

            builder.Register<IPassiveRegistry>(container =>
                new PassiveRegistry(
                    container.Resolve<PassiveCatalog>(),
                    container.Resolve<IGameCoordinator>(),
                    container.Resolve<IEncounterTurnSystem>(),
                    container.Resolve<ICardScriptExecutor>(),
                    container.Resolve<INpcRelationshipRegistry>()),
                Lifetime.Singleton);

            Debug.Log("[GameLifetimeScope] Passive System registered successfully");
        }

        private void RegisterMovementSystems(IContainerBuilder builder)
        {
            builder.Register<ActionSequencer>(Lifetime.Singleton);

            builder.Register<IZoneService>(_ =>
                new ZoneServiceAdapter(),
                Lifetime.Singleton);

            builder.Register<IAgentPositionService>(container =>
            {
                var zoneService = container.Resolve<IZoneService>();
                return new AgentPositionService(zoneService);
            }, Lifetime.Singleton);

            builder.Register<NarrativeMovementController>(Lifetime.Singleton);

            builder.RegisterComponentInHierarchy<ColdSeagullPlaygroundController>();

            builder.RegisterComponentInHierarchy<EncounterStarter>()
                   .AsImplementedInterfaces()
                   .AsSelf();

            builder.RegisterBuildCallback(container =>
            {
                var player = FindFirstObjectByType<DialogueSequencePlayer>();
                if (player != null)
                    container.Inject(player);
                else
                    Debug.LogWarning("[GameLifetimeScope] DialogueSequencePlayer not found in scene — skipping injection.");

                var encounterManager = FindFirstObjectByType<EncounterManager>();
                if (encounterManager != null)
                    container.Inject(encounterManager);
                else
                    Debug.LogWarning("[GameLifetimeScope] EncounterManager not found in scene — skipping injection.");

                var encounterUI = FindFirstObjectByType<EncounterUIController>();
                if (encounterUI != null)
                    container.Inject(encounterUI);
                else
                    Debug.LogWarning("[GameLifetimeScope] EncounterUIController not found in scene — skipping injection.");

                var encounterCanvas = FindFirstObjectByType<EncounterCanvas>();
                if (encounterCanvas != null)
                    container.Inject(encounterCanvas);
                else
                    Debug.LogWarning("[GameLifetimeScope] EncounterCanvas not found in scene — skipping injection.");

                // TurnIndicatorController — optional visual component for turn feedback.
                // Safe to omit from scene (no warning if absent).
                var turnIndicator = FindFirstObjectByType<TurnIndicatorController>();
                if (turnIndicator != null)
                    container.Inject(turnIndicator);
            });

            Debug.Log("[GameLifetimeScope] Movement Systems registered successfully");
        }

        private void RegisterInjectionSystem(IContainerBuilder builder)
        {
            if (injectorConfig != null)
                builder.RegisterInstance(injectorConfig);

            builder.Register<ISnLInjectorExecutor>(container =>
            {
                var narrativeExecutor = container.Resolve<NarrativeExecutor>();
                var gameCoordinator   = container.Resolve<IGameCoordinator>();
                var effectApplicator  = container.Resolve<IEffectApplicator>();
                var sharedEnvironment = container.Resolve<EnvironmentalSnapshot>();

                SnLInjectorConfig config = null;
                try { config = container.Resolve<SnLInjectorConfig>(); } catch { }

                return new SnLInjectorExecutor(
                    narrativeExecutor,
                    gameCoordinator,
                    effectApplicator,
                    sharedEnvironment,
                    config);
            }, Lifetime.Singleton);

            Debug.Log("[GameLifetimeScope] Injection System registered successfully");
        }
    }
}
