namespace RPG2D
{
    public interface ITravelSceneAdapter
    {
        bool TryPlacePlayerAtSpawnPoint(string spawnPointId, out string reason);
    }
}
