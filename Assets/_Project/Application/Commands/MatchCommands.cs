using System;

namespace Game.Application.Commands
{
    public readonly struct PlayCardCommand : ICommand
    {
        public readonly string PlayerId;
        public readonly Guid CardInstanceId;

        public PlayCardCommand(string playerId, Guid cardInstanceId)
        {
            PlayerId = playerId;
            CardInstanceId = cardInstanceId;
        }
    }

    public readonly struct EndTurnCommand : ICommand
    {
        public readonly string PlayerId;

        public EndTurnCommand(string playerId)
        {
            PlayerId = playerId;
        }
    }

    public readonly struct SurrenderCommand : ICommand
    {
        public readonly string PlayerId;

        public SurrenderCommand(string playerId)
        {
            PlayerId = playerId;
        }
    }
}
