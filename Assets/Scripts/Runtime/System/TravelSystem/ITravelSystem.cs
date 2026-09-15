using QFramework;

namespace RPG2D
{
    public interface ITravelSystem : ISystem
    {
        bool IsTraveling { get; }

        /// <returns>返回 true，只表示请求已被接受，不代表玩家已经到达</returns>
        bool Travel(TravelRequest request);
    }
}
