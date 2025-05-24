using System.Collections.Generic;
using Codebase.Controllers.Fsm.States;

namespace Codebase.Controllers.Fsm
{
    public class BotFsm : FsmBase
    {
        public BotFsm(List<IState> states) : base(states)
        {
        }
    }
}