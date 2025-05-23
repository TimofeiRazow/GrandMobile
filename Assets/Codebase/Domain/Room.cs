using System;
using System.Collections.Generic;

namespace Codebase.Domain
{
    public class Room
    {
        private readonly List<PlayerProfile> _playerProfiles = new();

        public Action<IReadOnlyList<PlayerProfile>> PlayersChanged;

        public IReadOnlyList<PlayerProfile> PlayerProfiles => _playerProfiles;

        public void AddPlayer(PlayerProfile player)
        {
            if (_playerProfiles.Contains(player))
                throw new ArgumentException(nameof(player));

            _playerProfiles.Add(player);
        }

        public void RemovePlayer(PlayerProfile player)
        {
            if (_playerProfiles.Contains(player) == false)
                throw new ArgumentException(nameof(player));

            _playerProfiles.Remove(player);
        }

        private void NotifyPlayersChanged() => 
            PlayersChanged?.Invoke(PlayerProfiles);
    }
}