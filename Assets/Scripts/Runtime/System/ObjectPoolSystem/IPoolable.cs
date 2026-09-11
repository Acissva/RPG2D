namespace RPG2D
{
    /// <summary>
    /// 需要在对象池生成前后执行方法，可继承该接口
    /// </summary>
    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }
}

