# MiJuegoCartas — Cliente Unity

Esqueleto inicial de un juego de cartas multijugador por turnos, con arquitectura
por capas pensada para **compartir el Dominio con un futuro backend .NET**.

## Cómo usar este esqueleto

1. Crea (o abre) tu proyecto Unity. Recomendado: Unity 6 / 2022 LTS o superior.
2. Copia la carpeta `Assets/_Project/` dentro del `Assets/` de tu proyecto.
3. Copia `CLAUDE.md` y `.gitignore` a la raíz del proyecto.
4. Unity regenerará los `.meta` y los `.asmdef` al importar.
5. Abre **Window → General → Test Runner → EditMode → Run All** para ver los tests pasar.

## Qué incluye

```
Assets/_Project/
├── Domain/          C# puro, SIN Unity. Reglas del juego.
│   ├── Cards/       CardDefinition, CardInstance
│   ├── Match/       MatchState, PlayerState, ActionResult
│   ├── Turns/       TurnSystem (validación autoritativa)
│   └── Events/      EventBus, IDomainEvent, eventos concretos
├── Application/     Orquestación. No contiene reglas.
│   ├── Commands/    PlayCard, EndTurn, Surrender
│   └── Services/    IMatchService, LocalMatchService
├── Infrastructure/  (vacío por ahora: Network, Persistence)
├── Presentation/    UI. Ejemplo: MatchHudController
└── Tests/EditMode/  Tests NUnit que corren sin abrir escena
```

## Por qué esta estructura

El objetivo es evitar el "GameManager gigante" y el código espagueti. Cada capa
tiene una única responsabilidad y las dependencias van solo hacia abajo. Los
`.asmdef` hacen que el **compilador** impida romper las reglas: si intentas usar
`UnityEngine` en el Dominio, no compila.

## El siguiente paso: el backend .NET

Cuando arranques el servidor:

1. Crea una solución .NET aparte (fuera de la carpeta de Unity).
2. Mueve `Assets/_Project/Domain/` a un proyecto **.NET Standard 2.1** (ej. `Game.Domain.csproj`).
3. Referéncialo desde dos sitios:
   - El servidor .NET lo usa como proyecto normal.
   - Unity lo consume como código fuente (vía symlink/copy) o como **DLL** compilada,
     colocándola en `Assets/Plugins/` con un `.asmdef` que la referencie.
4. Implementa `NetworkMatchService : IMatchService` en `Infrastructure/Network/`,
   que en lugar de ejecutar el dominio localmente envía los comandos al servidor
   y publica los estados recibidos. La UI no cambia.

Así escribes las reglas **una sola vez** y corren idénticas en cliente y servidor.

## Estado actual

Funciona el flujo básico: jugar minions, terminar turno (con maná y robo), rendirse
y detección de victoria por vida. Pendiente: efectos de hechizos, combate entre
minions y la capa de red. Ver la sección "Pendiente" en `CLAUDE.md`.
