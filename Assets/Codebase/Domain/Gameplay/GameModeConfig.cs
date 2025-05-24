using System;

namespace Codebase.Domain.Gameplay
{
    [Serializable]
    public class GameModeConfig
    {
        public int MafiaCount;
        public int PoliceCount;
        public int CivilianCount;

        public int TotalCount => MafiaCount + PoliceCount + CivilianCount;
    }
}