using System;
using System.Collections.Generic;

namespace Codebase.Domain
{
    public class RoomList
    {
        private readonly List<Room> _rooms = new();

        public Action<IReadOnlyList<Room>> RoomsChanged;

        public IReadOnlyList<Room> Rooms => _rooms;

        public void AddRoom(Room room)
        {
            if (_rooms.Contains(room))
                throw new ArgumentException(nameof(room));

            _rooms.Add(room);

            NotifyRoomsChanged();
        }

        public void RemoveRoom(Room room)
        {
            if (_rooms.Contains(room) == false)
                throw new ArgumentException(nameof(room));

            _rooms.Remove(room);
        }

        private void NotifyRoomsChanged() =>
            RoomsChanged?.Invoke(Rooms);
    }
}