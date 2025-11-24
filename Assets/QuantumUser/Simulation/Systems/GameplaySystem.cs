namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class GameplaySystem : SystemSignalsOnly, ISignalOnPlayerAdded, ISignalOnPlayerRemoved
    {
        void ISignalOnPlayerAdded.OnPlayerAdded(Frame frame, PlayerRef playerRef, bool firstTime)
        {
            Log.Info($"Player added: {playerRef}");
            RespawnPlayer(frame, playerRef);
        }

        void ISignalOnPlayerRemoved.OnPlayerRemoved(Frame frame, PlayerRef playerRef)
        {
            foreach (var pair in frame.GetComponentIterator<PlayerLink>())
            {
                if (pair.Component.PlayerRef != playerRef)
                    continue;

                frame.Destroy(pair.Entity);
            }
        }

        /// <summary>
        /// Spawns a player at a random position
        /// </summary>
        private void RespawnPlayer(Frame frame, PlayerRef playerRef)
        {
            var runtimePlayer = frame.GetPlayerData(playerRef);
            var playerEntity = frame.Create(runtimePlayer.PlayerAvatar);

            frame.AddOrGet<PlayerLink>(playerEntity, out var playerLink);
            playerLink->PlayerRef = playerRef;
            frame.Events.PlayerLinked(playerRef, playerEntity);
        }
    } 
}