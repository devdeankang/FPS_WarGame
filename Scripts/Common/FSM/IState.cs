public interface IState<T>
{
    void OperateEnter(T sender);
    void OperateUpdate(T sender);
    void OperateFixedUpdate(T sender);
    void OperateExit(T sender);
}