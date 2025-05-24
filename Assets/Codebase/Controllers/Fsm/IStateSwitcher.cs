using Codebase.Controllers.Fsm.States;

namespace Codebase.Controllers.Fsm
{
    public interface IStateSwitcher
    {
        public void Switch(IState state);
    }
}