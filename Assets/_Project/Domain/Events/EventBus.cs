using System;
using System.Collections.Generic;

namespace Game.Domain.Events
{
    /// <summary>
    /// Bus de eventos del dominio. Permite que los sistemas (combate, maná,
    /// victoria, etc.) reaccionen a lo que pasa SIN conocerse entre sí.
    ///
    /// Esta es la pieza que evita el código espagueti: en lugar de que
    /// CardSystem llame a BoardSystem que llama a AnimationSystem...,
    /// cada sistema publica/escucha eventos y nadie depende de la
    /// implementación de los demás.
    ///
    /// Implementación deliberadamente simple y SIN Unity, para poder
    /// reutilizarla idéntica en el servidor .NET.
    /// </summary>
    public sealed class EventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IDomainEvent
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            var type = typeof(TEvent);
            if (!_handlers.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                _handlers[type] = list;
            }
            list.Add(handler);
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IDomainEvent
        {
            if (_handlers.TryGetValue(typeof(TEvent), out var list))
                list.Remove(handler);
        }

        public void Publish<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent
        {
            if (_handlers.TryGetValue(typeof(TEvent), out var list))
            {
                // Copia defensiva: un handler podría modificar la suscripción.
                foreach (var handler in list.ToArray())
                    ((Action<TEvent>)handler).Invoke(domainEvent);
            }
        }
    }
}
