using System.Collections.Generic;
using Codebase.Controllers.Fsm.States;

namespace Codebase.Controllers.Fsm
{
    public class PlayerFsm : FsmBase
    {
        public PlayerFsm(List<IState> states) : base(states)
        {
        }
    }
}