namespace Codebase.Controllers.Fsm.States
{
    public interface IState
    {
        void Initialize(IStateSwitcher stateSwitcher);
        void Enter();
        void Exit();
        void Update(float deltaTime);
    }
}