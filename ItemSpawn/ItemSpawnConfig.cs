using System.Collections.Generic;

namespace ItemSpawn
{
    public class ItemSpawnConfig
    {
        public IEnumerable<SpawnerEntry> ItemSpawnerEntries;
    }

    public class SpawnerEntry
    {
        public string ItemId { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string Description { get; set; }
        public IEnumerable<SpawnerEntry> Secondaries { get; set; }
    }
}
