# Proyecto: RacingCards — juego de carreras con cartas (Unity)

Juego **competitivo de carreras automáticas top-down** con cartas, para mobile
(Android/iOS). El jugador construye su vehículo con cartas (vehículo + modificadores)
y la carrera se resuelve por una **simulación determinista**. El objetivo a medio
plazo es multijugador con **Photon Quantum** (Fase 4), por eso el núcleo de
simulación está aislado de Unity.

> Nota: este proyecto nació como un card-battler tipo Hearthstone. Ese scaffolding
> (`Game.Domain/Application/...`, menús de duelo, `PracticeMatch`) se considera
> **LEGACY** y está pendiente de retirar. La mecánica principal hoy es **RacingCards**.

## Módulo principal: `Assets/_Project/RacingCards/`

Assembly propio (`RacingCards.asmdef`, namespaces `RacingCards.*`), aislado del
resto. Estructura interna por responsabilidades:

```
RacingCards/
├── Data/         ← ScriptableObjects (datos): VehicleCard, ModifierCard,
│                   ConsumableCard, TerrainConfig, TrackData. Dependen de Unity.
├── Core/         ← lógica pura y determinista: VehicleStats, StatResolver
│                   (fórmula de afinidad de terreno). Sin estado, sin Random/Time.
├── Simulation/   ← simulación de carrera: RaceSimulation, RacerState.
│                   Avanza en pasos FIJOS (Step(dt)); el dt llega por parámetro.
├── Race/         ← capa Unity (vista/orquestación): RaceManager, TopDownCamera.
│                   MonoBehaviours. SOLO leen la simulación para mover visuals.
├── Content/      ← assets de ejemplo (TC_City, TR_City01, VC_Player + bots…).
└── Scenes/       ← Race.unity (escena jugable de carrera).
```

## Reglas de oro (determinismo para Quantum)

1. **La simulación no depende de Unity para su lógica.** `StatResolver`,
   `RacerState` y `RaceSimulation` no usan `MonoBehaviour` ni `Time.deltaTime`
   interno. El `deltaTime` se recibe como parámetro y el avance es por **tick fijo**.
2. **La vista nunca modifica el estado.** `RaceManager`/`TopDownCamera` solo LEEN
   `RaceSimulation` para renderizar. Nunca al revés.
3. **Toda la lógica numérica vive en `Core/` y `Simulation/`.** En Fase 4 los
   `float`/`Vector3`/`Mathf` se sustituirán por punto fijo (`FP`/`FPVector3`/`FPMath`)
   de Quantum. La lógica no cambia; solo los tipos. Por eso está aislada.
4. **Datos en ScriptableObjects.** Las cartas y pistas son assets en `Content/`,
   creables desde `Assets > Create > RacingCards`.

## Flujo actual (Fase 1, local)

- **Menús** (UI Toolkit, `Game.Presentation/UI`): `MainMenu → ModeSelect → "Carrera
  rápida"` carga la escena `Race` (`SceneManager.LoadScene("Race")`).
- **Escena `Race`**: un `RaceManager` con `TerrainConfig` + `VehicleCard`s resuelve
  los stats por terreno (`StatResolver`), crea la `RaceSimulation` con 1 jugador +
  4 bots y avanza a tick fijo. Al terminar imprime el ranking.
- Sin modelos 3D aún: los corredores se ven como cápsulas (`racerVisualFallback`).

## Tests

EditMode (NUnit, sin entrar a Play):
- `Assets/_Project/RacingCards/Tests/` (`RacingCards.Tests.asmdef`) — afinidad de
  terreno, determinismo del resolver, la carrera en recta la gana el más rápido.
- Correr: `Window > General > Test Runner > EditMode > Run All`.

Al añadir lógica de simulación/stats, añade tests aquí.

## Roadmap (plan MVP por fases)

- **Fase 1 (actual):** core de carrera automática local. ✅ integrado.
- **Fase 2:** consumibles (Nitro/Trampa) — ya hay ganchos en `RacerState`
  (`ApplyTemporarySpeedEffect`).
- **Fase 3:** modelos 3D low-poly y pistas reales (`trackPrefab`, `model3D`).
- **Fase 4:** multijugador determinista con **Photon Quantum** (migrar a punto fijo).

## Sistema de niveles (escenas aditivas)

Assembly `Game.Levels` (`Assets/_Project/Levels/`). Un **nivel** es un
`LevelDefinition` (ScriptableObject) que agrupa varias escenas que se cargan de
forma **aditiva** al arrancar, montando el nivel en la jerarquía. Estructura
estándar de cada nivel (una escena por responsabilidad):

- **playerScene** → toda la lógica del jugador (controladores, cámara, HUD, gameplay).
- **environmentScene** → el escenario de interacción (entorno, props, pista).
- **effectsLightingScene** → efectos e iluminación (queda como escena activa).

Piezas:
- `SceneReference` — referencia a escena asignable en el inspector (SceneAsset) que
  se "hornea" a nombre/ruta para runtime.
- `LevelDefinition` — el SO del nivel (`Assets > Create > Game > Level Definition`).
- `LevelLoader` — MonoBehaviour en una escena **Boot** mínima; al iniciar carga las
  escenas del nivel aditivamente y fija la escena activa.

Reglas:
- Las escenas de cada nivel viven en `Assets/_Project/Levels/<NombreNivel>/`.
- Toda escena referenciada debe estar en **Build Settings** para cargarse por nombre.
- La escena `Boot` (con el `LevelLoader`) es el punto de entrada del juego.

## Convenciones

- Cada módulo/área = su propio `.asmdef`. Mantener dependencias unidireccionales.
- Carpetas temporales/escena de prueba bajo el módulo, no en la raíz de `Assets/`.

## Legacy pendiente de decisión

`Game.Domain`, `Game.Application` (Match/Cards/Turns, `PracticeMatch`,
`LocalMatchService`) y el HUD de cartas (`MatchHudScreen`) son del antiguo
card-battler. No se usan en el flujo de carreras. Decidir si se eliminan o se
archivan. Los menús (`UIManager`, pantallas UI Toolkit) SÍ se reutilizan.
