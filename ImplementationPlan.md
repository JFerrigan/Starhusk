# Starhusk Implementation Plan

This file is the working implementation tracker for Starhusk. We will mark each checkbox as complete only after the feature is implemented and verified in Unity. Unchecked items are not done.

Source design: `HighLeveldesign.md`

Current prototype foundation:

- Player ship is bootstrapped at runtime.
- Asteroids-like 2D movement exists in `Assets/Scripts/PlayerMovement.cs`.
- Procedural star system generation exists in `Assets/Scripts/StarSystemGenerator.cs`.
- Finite resource deposits exist in `Assets/Scripts/ResourceDeposit.cs`.
- Manual asteroid interaction exists in `Assets/Scripts/MineableAsteroid.cs`.
- Scanner reveal exists in `Assets/Scripts/PlayerScanner.cs`.
- Basic resource HUD exists in `Assets/Scripts/FoundationHud.cs`.
- EditMode generator tests exist in `Assets/Tests/EditMode/StarSystemGeneratorTests.cs`.

## Phase 0: Project Organization

- [ ] Rename or standardize folders to match Unity conventions: `Assets/Prefabs`, `Assets/Scripts/Core`, `Assets/Scripts/Player`, `Assets/Scripts/WorldGeneration`, `Assets/Scripts/Resources`, `Assets/Scripts/Structures`, `Assets/Scripts/Logistics`, `Assets/Scripts/Energy`, `Assets/Scripts/UI`, and `Assets/Scripts/Tests` if needed.
- [ ] Move current scripts into matching folders without changing behavior.
- [ ] Rename `PreFab` to `Prefabs` and update references.
- [ ] Create `Assets/ScriptableObjects/Resources`, `Structures`, `Planets`, `Doctrines`, `Dialogue`, and `Recipes`.
- [ ] Add an implementation note in this file whenever a task is intentionally deferred or replaced.

Implementation details:

- Keep runtime bootstrap support so scenes stay easy to test.
- Prefer ScriptableObjects for resource, structure, recipe, and doctrine definitions.
- Keep procedural generation deterministic and covered by EditMode tests.

## Phase 1: Movement, Mining, and Prototype Feel

- [ ] Replace proximity `E` mining with an aimed mining laser tool.
- [ ] Add `PlayerToolController` with selected tools: `Mine`, `Scan`, `Build`, and later `Interact`.
- [ ] Add a mining laser line/VFX using `LineRenderer` or a simple generated sprite beam.
- [ ] Mining laser should raycast or overlap in front of the ship and mine `ResourceDeposit`.
- [ ] Add mining range, mining tick interval, and per-tick amount fields.
- [ ] Add visual and audio feedback hooks for mined, failed, and depleted states.
- [ ] Add ship integrity and energy fields, even if damage is not active yet.
- [ ] Add camera smoothing and zoom limits if `CameraFollow` does not already cover them.
- [ ] Update HUD to show selected tool, ship integrity, cargo summary, scanner state, and nearby prompt.
- [ ] Add PlayMode or EditMode tests for deposit mining behavior.

Implementation details:

- New files: `Assets/Scripts/Player/PlayerToolController.cs`, `Assets/Scripts/Player/MiningLaser.cs`.
- Update `MineableAsteroid` into a compatibility component or remove it after laser mining is stable.
- `ResourceDeposit.Mine` should stay the core resource removal API.
- Avoid hard-coding only ore. Mining must support every `ResourceType`.

Success criteria:

- [ ] The player can fly, aim, mine a visible deposit at range, collect resources, and see deposit depletion.

## Phase 2: Procedural System and Exploration

- [ ] Expand `StarType` to include `Dying`, `Binary`, and `NeutronRemnant`.
- [ ] Expand `CelestialBodyType` to include `CarbonWorld`, `VolcanicPlanet`, `GasGiant`, `CrystalWorld`, `DeadPlanet`, and `HiveOccupiedPlanet`.
- [ ] Expand `ResourceType` with raw, refined, Dyson, stellar, alien, and data resources.
- [ ] Add `ResourceDefinition` ScriptableObjects for display names, category, rarity, finite flag, and icon.
- [ ] Add `PlanetDefinition` ScriptableObjects for possible resources, hazard level, scan difficulty, build slots, and depletion visuals.
- [ ] Refactor `StarSystemGenerator` to choose planets from definitions instead of a small enum switch.
- [ ] Add hazard zone generation data, starting with radiation and heat zones.
- [ ] Add anomaly and wreck generation data.
- [ ] Add fog-of-war coverage data instead of only per-object discovery.
- [ ] Add persistent discovered state for bodies, deposits, anomalies, and structures.
- [ ] Add scanner rewards: `SurveyData` for first scan, `AnomalyData` for anomaly scan.
- [ ] Add generator tests for star type rules, planet resource rules, hazards, and deterministic anomalies.

Implementation details:

- New files: `Assets/Scripts/WorldGeneration/SystemSeed.cs`, `GeneratedSystemState.cs`, `HazardZone.cs`, `AnomalySite.cs`.
- `StarSystemLayout` should remain pure data so tests do not require scene objects.
- Generated runtime objects should read from layout data and attach gameplay components afterward.

Success criteria:

- [ ] A new seed produces a larger system with star type, planets, deposits, hazards, anomalies, fog, and deterministic layout tests.

## Phase 3: Minimap and System Map

- [x] Implement a minimap panel showing player, star, discovered planets, discovered asteroids, and fog.
- [ ] Add layer toggles: resources, logistics, energy, maintenance, territory, alien, hazards, and exploration.
- [x] Add full system map toggle.
- [ ] Add selectable map markers for deposits, planets, structures, anomalies, and planned build sites.
- [ ] Add waypoint/beacon placement from the map.
- [x] Add map icon data per object type.
- [ ] Add alerts for low resources, depleted deposits, blocked routes, unpowered structures, and damaged structures.

Implementation details:

- New files: `Assets/Scripts/UI/MinimapController.cs`, `SystemMapController.cs`, `MapMarker.cs`, `MapLayerToggle.cs`, `AlertManager.cs`.
- Keep UI data separate from world objects through marker registration.
- Use discovered state to hide undiscovered objects.

Success criteria:

- [ ] The player can navigate the generated system using minimap and full map information without relying on scene hierarchy.

## Phase 4: Structures and Building

- [ ] Add `StructureDefinition` ScriptableObjects with ID, name, sprite, build cost, placement type, energy use, maintenance profile, storage, inputs, outputs, and required unlocks.
- [ ] Add placement modes: free-space, orbital, surface, star-orbit, route node, territory beacon, and Dyson orbit.
- [ ] Implement `BuildController` for ghost preview, valid/invalid placement, rotate/cancel/confirm.
- [ ] Implement build costs from player inventory or nearby storage.
- [ ] Allow blueprints to be placed without all resources.
- [ ] Add construction progress state: planned, waiting for resources, building, active, disabled, broken.
- [ ] Implement initial structures: storage hub, basic miner, refinery, solar collector, energy relay, maintenance depot.
- [ ] Add structure selection panel with status, inventory, power, wear, recipe, and route connections.
- [ ] Add tests for build cost validation and structure state transitions.

Implementation details:

- New files: `Assets/Scripts/Structures/StructureDefinition.cs`, `StructureInstance.cs`, `BuildController.cs`, `ConstructionSite.cs`, `PlacementValidator.cs`.
- `StructureInstance` should own runtime state and reference a definition.
- Start with simple rectangular or circular placement validation; improve attachment slots later.

Success criteria:

- [ ] The player can place a storage hub, miner, refinery, and solar collector, with costs and construction state visible.

## Phase 5: Inventory, Storage, and Recipes

- [ ] Replace player-only `ResourceInventory` assumptions with reusable inventory/storage components.
- [ ] Add capacity limits and per-resource stack rules.
- [ ] Add `RecipeDefinition` ScriptableObjects for input resources, output resources, duration, energy use, and required structure type.
- [ ] Implement refinery recipes: ore to alloy, silicate to lensglass, copper plus silicate to circuitry, ice to coolant.
- [ ] Implement spare parts recipe from alloy and circuitry.
- [ ] Add storage transfer APIs for player, stations, miners, refineries, and drones.
- [ ] Add resource reservation for construction sites.
- [ ] Add tests for inventory add/remove/transfer, capacity, and recipes.

Implementation details:

- New files: `Assets/Scripts/Resources/ResourceStorage.cs`, `ResourceTransaction.cs`, `RecipeDefinition.cs`, `RecipeProcessor.cs`.
- Keep `ResourceInventory` as a thin player wrapper or migrate it to inherit/compose `ResourceStorage`.

Success criteria:

- [ ] Resources can move between player and structures, and refineries can produce refined resources over time.

## Phase 6: Logistics Routes

- [ ] Add cargo drone prefab/placeholder sprite.
- [ ] Add `LogisticsManager` to own routes and active drones.
- [ ] Add `RouteDefinition` runtime data: source, destination, resource type, desired amount, priority, allowed drones, territory permission, maintenance required, auto-reroute, and emergency mode.
- [ ] Add route creation UI: select source, select destination, choose resource, set priority.
- [ ] Add drone behavior: load, travel, unload, return or continue loop.
- [ ] Add storage limits and route stalled states.
- [ ] Add route visualization lines in-world and on minimap.
- [ ] Add route alerts: no source resource, destination full, blocked path, no power, no drone.
- [ ] Add tests for route state and resource transfer.

Implementation details:

- New files: `Assets/Scripts/Logistics/LogisticsManager.cs`, `LogisticsRoute.cs`, `CargoDrone.cs`, `RouteLineRenderer.cs`.
- Start with straight-line travel. Add pathing and hazards later.
- Use route state machines so UI can explain why a route is idle.

Success criteria:

- [ ] Resources move automatically from miner to storage to refinery to storage through visible cargo drone routes.

## Phase 7: Energy Grid

- [ ] Add `EnergyGridManager` for production, consumption, storage, priority, and shortages.
- [ ] Add `EnergyProducer`, `EnergyConsumer`, `EnergyStorage`, and `EnergyRelay` components.
- [ ] Make solar collectors produce power based on star type and distance.
- [ ] Make miners, refineries, drones, maintenance depots, and relays consume power.
- [ ] Add shutdown behavior for unpowered structures.
- [ ] Add energy overlay and HUD summary.
- [ ] Add overdraw and blackout alert states.
- [ ] Add tests for power balance, priority shutdown, and collector output.

Implementation details:

- New files: `Assets/Scripts/Energy/EnergyGridManager.cs`, `EnergyProducer.cs`, `EnergyConsumer.cs`, `EnergyNode.cs`.
- Start with one system-wide grid, then split into relay-connected grids later.
- Structure active state should depend on construction state, power state, and broken state.

Success criteria:

- [ ] Structures require power, solar collectors increase available power, and shortages visibly disable low-priority structures.

## Phase 8: Depletion and World Consequences

- [ ] Add depletion percentage events to `ResourceDeposit`.
- [ ] Add visual states for full, low, nearly depleted, and depleted deposits.
- [ ] Add planet depletion/scarring state when surface extractors drain major resources.
- [ ] Add depletion alerts at configurable thresholds.
- [ ] Add reclamation action for depleted or obsolete structures.
- [ ] Add system summary: total remaining raw resources, depleted deposits, active extraction rate.
- [ ] Persist depletion state in save data.
- [ ] Add tests for depletion thresholds and depleted-state persistence data.

Implementation details:

- New files: `Assets/Scripts/Resources/DepletionState.cs`, `Assets/Scripts/WorldGeneration/SystemDepletionSummary.cs`.
- Keep visual changes simple first: color tint, sprite swap, label, or particle hook.

Success criteria:

- [ ] The player sees deposits and planets become husks as finite resources are consumed.

## Phase 9: Maintenance and Failure

- [ ] Add wear values to structures.
- [ ] Add wear sources: time, heat, radiation, heavy use, overclocking, poor materials, sabotage placeholder, and lack of spare parts.
- [ ] Add structure states: healthy, worn, critical, broken, quarantined.
- [ ] Add manual repair interaction from ship.
- [ ] Add spare parts consumption for repair.
- [ ] Add repair drone depot behavior.
- [ ] Add maintenance route type for spare parts and repair drones.
- [ ] Add failure consequences: reduced output, disabled recipe, broken route, energy leak, or shutdown.
- [ ] Add maintenance overlay and alerts.
- [ ] Add tests for wear progression, repair, and breakdown state.

Implementation details:

- New files: `Assets/Scripts/Maintenance/MaintenanceProfile.cs`, `WearComponent.cs`, `RepairAction.cs`, `RepairDroneDepot.cs`, `MaintenanceManager.cs`.
- Failures should be predictable and preceded by alerts.
- Start with deterministic wear per second; add random incidents later only if readable.

Success criteria:

- [ ] Structures degrade, warn the player before failure, break if ignored, and can be repaired manually or by depot.

## Phase 10: Dyson Progression

- [ ] Add `DysonManager` to track stages, components, energy output, heat, and maintenance demand.
- [ ] Implement stage 1: basic solar collectors.
- [ ] Implement stage 2: solar sail launcher with temporary decaying sails.
- [ ] Implement stage 3: Dyson swarm visual orbiting the star.
- [ ] Implement stage 4: mirror arrays and energy beam routing.
- [ ] Add Dyson resources: mirror film, frame alloy, energy lenses, thermal sinks, orbital stabilizers, plasma condensate.
- [ ] Add production recipes for Dyson resources.
- [ ] Add heat buildup and cooling with thermal sinks.
- [ ] Add Dyson UI panel with stage progress, output, heat, missing resources, and maintenance warnings.
- [ ] Add solar flare event for star pulse capture.
- [ ] Add tests for stage progression, energy output, heat, and sail decay.

Implementation details:

- New files: `Assets/Scripts/Dyson/DysonManager.cs`, `DysonStageDefinition.cs`, `DysonComponent.cs`, `SolarSail.cs`, `DysonHeatModel.cs`.
- Keep Dyson visuals data-driven: component count controls orbit sprites/arcs around the star.

Success criteria:

- [ ] Building Dyson infrastructure visibly increases system power while adding heat and maintenance pressure.

## Phase 11: Stellar Engine

- [ ] Add `StellarEngineManager` for research state, component construction, charging, destination choice, ignition, and transition.
- [ ] Add stellar engine structures: anchors, focusing array, cooling systems, stabilizer ring, engine core.
- [ ] Add stellar engine resources: exotic matter, neutronium shards, gravity lattice, engine core.
- [ ] Add requirements: Dyson output threshold, cooling threshold, anchor alignment, fuel infrastructure, logistics stability.
- [ ] Add charging sequence with interruptible progress and alert states.
- [ ] Add simple destination selection UI.
- [ ] Generate a new system after ignition while preserving prior system state for future return.
- [ ] Add tests for requirement validation and transition data.

Implementation details:

- New files: `Assets/Scripts/StellarEngine/StellarEngineManager.cs`, `StellarEngineProject.cs`, `DestinationStarData.cs`.
- The first version can use a small list of generated destination seeds before the full galaxy map exists.

Success criteria:

- [ ] The player can complete a system-scale stellar engine objective and move to another generated star system.

## Phase 12: Doctrine Web

- [ ] Add `DoctrineNodeDefinition` ScriptableObjects with ID, name, branch, description, unlock behavior, prerequisites, mutual exclusions, cost, and icon.
- [ ] Add `DoctrineManager` for available, unlocked, committed, and locked states.
- [ ] Add Doctrine Web UI with branches: Pilot, Industry, Dyson, Maintenance, Exploration, Diplomacy, Warfare, Hive Integration, Stellar Engineering, Galactic Architecture.
- [ ] Enforce the rule that every node unlocks a new action, command, structure, route type, diplomatic option, combat behavior, maintenance behavior, exploration behavior, or interaction.
- [ ] Add early Pilot nodes: Anchored Drift, Tether Tow, Emergency Dock, Survey Pulse, Hazard Skim.
- [ ] Add early Industry nodes: Mass Driver Routes, Mobile Refinery, Priority Routing, Resource Locking, Route Contracts.
- [ ] Add early Maintenance nodes: Repair Drone Depots, Failure Quarantine, Cannibalize Structure, Maintenance Routes, Patch Foam.
- [ ] Add early Exploration nodes: Probe Launcher, Breadcrumb Beacons, Anomaly Sampling, Gravimetric Mapping.
- [ ] Add committal gates: Industrial Mandate vs Stewardship Protocol, Accord Path vs Total Claim Doctrine, Solar Dominion vs Silent Star.
- [ ] Add tests for prerequisites, mutual exclusions, and unlock effects.

Implementation details:

- New files: `Assets/Scripts/Doctrine/DoctrineManager.cs`, `DoctrineNodeDefinition.cs`, `DoctrineUnlockHandler.cs`.
- Unlock effects should be behavior flags or commands consumed by the relevant system, not passive percentage stats.

Success criteria:

- [ ] The player spends research/data resources on behavior-changing unlocks, including mutually exclusive doctrine choices.

## Phase 13: Save and Load

- [ ] Add `SaveLoadManager`.
- [ ] Save current system seed, generated system state, discovered objects, deposits, structures, inventories, routes, energy grid state, maintenance state, Dyson state, doctrine state, alerts, and player position.
- [ ] Save multiple visited systems.
- [ ] Add versioned save schema.
- [ ] Add manual save/load UI and autosave hook.
- [ ] Add tests for serializing and restoring core data models.

Implementation details:

- New files: `Assets/Scripts/SaveLoad/SaveLoadManager.cs`, `GameSaveData.cs`, `SystemSaveData.cs`.
- Prefer serializing plain data objects rather than scene object references.

Success criteria:

- [ ] The player can leave and reload a game without losing system progress, structures, routes, resources, or depletion.

## Phase 14: Hive First Contact

- [ ] Add hive entity definitions for worker, surveyor, soldier, speaker station, cargo pod, repair swarm, miner, refinery, territory marker, Dyson collector, brood station, and queen/core chamber.
- [ ] Add alien harvested system generation template.
- [ ] Add first-contact trigger after the player reaches another system or detects the unknown signal.
- [ ] Add first-contact worker encounter.
- [ ] Add dialogue UI with choices and consequences.
- [ ] Implement the first-contact choices: peace, questions, star-harvesting question, take me to your leader, territorial claim, apology.
- [ ] Add Speaker encounter and initial relationship choice.
- [ ] Add relationship state: unknown, contact, accord, tension, war.
- [ ] Add tests for dialogue requirements and consequence application.

Implementation details:

- New files: `Assets/Scripts/Aliens/HiveFactionManager.cs`, `HiveEntity.cs`, `Assets/Scripts/Dialogue/DialogueDefinition.cs`, `DialogueController.cs`.
- First version can use placeholder sprites and scripted encounters.

Success criteria:

- [ ] The player encounters a hive worker, reaches a Speaker, and chooses the first friendship or conflict direction.

## Phase 15: Territory, Friendship, and Conflict

- [ ] Add `TerritoryManager` with zones: player, hive, neutral, shared, contested, protected brood, demilitarized, and war.
- [ ] Add territory overlays and minimap borders.
- [ ] Add claims from beacons, structures, negotiated borders, and military occupation.
- [ ] Add trust/tension values.
- [ ] Add friendship mechanics: trade routes, shared maps, shared claim negotiation, joint construction, worker aid, and restrictions on aggression.
- [ ] Add conflict mechanics: raids, sabotage, blockades, route interdiction, station capture, forced salvage, and occupation grid.
- [ ] Add hive expansion AI that surveys, claims, mines, builds routes, repairs, depletes resources, and constructs Dyson infrastructure.
- [ ] Add diplomatic restrictions and consequences for mining claimed planets or attacking infrastructure.
- [ ] Add tests for territory permissions, trust changes, war state, and capture/salvage outcomes.

Implementation details:

- New files: `Assets/Scripts/Territory/TerritoryManager.cs`, `TerritoryZone.cs`, `ClaimSource.cs`, `Assets/Scripts/Aliens/HiveExpansionAI.cs`, `HiveDiplomacyState.cs`.
- Make hive behavior systemic enough to feel like another automation network, but start with simple scheduled actions.

Success criteria:

- [ ] Friendship constrains expansion but enables cooperation; conflict allows seizure but creates active danger.

## Phase 16: Interstellar and Galactic Layer

- [ ] Add galaxy map with nearby stars, star type, rough resources, alien presence likelihood, hazard level, distance, energy cost, known claims, trade routes, and war fronts.
- [ ] Add interstellar route planning.
- [ ] Add multi-system state management.
- [ ] Add interstellar logistics route type.
- [ ] Add system roles: resource hub, energy hub, maintenance depot, military outpost, joint project zone, depleted husk.
- [ ] Add galactic Dyson network projects.
- [ ] Add endgame project requirements and progress UI.
- [ ] Add tests for galaxy generation and multi-system persistence.

Implementation details:

- New files: `Assets/Scripts/Galaxy/GalaxyMapManager.cs`, `StarNodeData.cs`, `InterstellarRoute.cs`, `GalacticProjectManager.cs`.
- This phase should wait until one-system automation, Dyson, and stellar engine loops are playable.

Success criteria:

- [ ] The game expands from one-system automation into multi-system planning and galactic megastructure construction.

## Vertical Slice Target

This is the first complete playable slice we should aim for before expanding scope:

- [ ] Start near a generated star.
- [ ] Fly with satisfying 2D ship controls.
- [ ] Scan and reveal nearby bodies.
- [ ] Mine asteroids manually with the mining laser.
- [ ] Build a storage hub.
- [ ] Build a miner on a deposit.
- [ ] Build a refinery.
- [ ] Create a cargo drone route.
- [ ] Build solar collectors.
- [ ] Power structures through the energy grid.
- [ ] See a deposit run low and deplete.
- [ ] Build a maintenance depot.
- [ ] Repair a failing collector.
- [ ] Build a simple stellar engine component.
- [ ] Detect an unknown signal from another system.

## MVP Definition

- [x] 2D ship movement.
- [x] Procedural small star system.
- [ ] Minimap and fog-of-war.
- [ ] Manual asteroid mining.
- [ ] Finite ore, ice, silicate, and copper deposits.
- [ ] Basic structures.
- [ ] Basic logistics drones.
- [ ] Basic solar/Dyson collectors.
- [ ] Energy grid.
- [ ] Simple maintenance wear.
- [ ] Spare parts repair.
- [ ] First Dyson milestone.
- [ ] First stellar engine prototype goal.

MVP exclusions:

- [ ] Full alien diplomacy.
- [ ] Full galaxy map.
- [ ] Advanced Doctrine Web.
- [ ] War systems.
- [ ] Hive integration.
- [ ] Many star types.
- [ ] Complex combat.
- [ ] Full endgame.

## Testing Checklist

- [ ] EditMode tests cover deterministic generation.
- [ ] EditMode tests cover resource deposits, storage, transfers, recipes, and depletion.
- [ ] EditMode tests cover build validation and structure state.
- [ ] EditMode tests cover logistics route state.
- [ ] EditMode tests cover energy grid balance.
- [ ] EditMode tests cover maintenance wear and repair.
- [ ] EditMode tests cover doctrine prerequisites and mutual exclusions.
- [ ] PlayMode smoke test covers bootstrap scene startup.
- [ ] PlayMode smoke test covers fly, scan, mine, build, route, power, repair loop.

## Tracking Rules

- [ ] A checkbox is only checked after implementation is in the project.
- [ ] A checkbox is only checked after the feature has been manually verified or covered by an automated test.
- [ ] If a task changes scope, edit the task text before checking it.
- [ ] If a task is split, leave the parent unchecked until all child work is done.
- [ ] This file is the source of truth for what is done and not done.
