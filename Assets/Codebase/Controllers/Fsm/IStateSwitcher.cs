using Codebase.Controllers.Fsm.States;

namespace Codebase.Controllers.Fsm
{
    public interface IStateSwitcher
    {
        void Switch(IState state);
        void Switch<T>() where T : class, IState;
    }
}