using System;
using System.IO;
using System.Linq;
using FistVR;
using Kolibri.Lib;
using UnityEngine;
using Valve.Newtonsoft.Json;

namespace ItemSpawn
{
    public class InjectionMethods
    {
        /// <summary>
        /// This method is injected into the bottom of MainMenuScreen's Awake method
        /// </summary>
        [InjectMethod(typeof(MainMenuScreen), "Awake", MethodInjectionInfo.MethodInjectionLocation.Bottom)]
        
        public static void AddSpawnerEntries()
        {
            //Find config file
            var configFile = new FileInfo(@"Mods\ItemSpawn\ItemSpawnerConfig.json");
            if (!configFile.Exists)
            {
                var e = new Exception("Config file not found");
                Logger.Log(e);
                throw e;
            }

            try
            {
                //Load config file
                var ItemSpawnerConfig = JsonConvert.DeserializeObject<ItemSpawnConfig>(File.ReadAllText(configFile.FullName));

                //Add the items from the config to the game
                foreach (var entry in ItemSpawnerConfig.ItemSpawnerEntries)
                {
                    var itemSpawnId = GetItemSpawnerId(entry);
                    if (itemSpawnId == null)
                    {
                    }
                    IM.SCD[itemSpawnId.SubCategory].Add(itemSpawnId);
                }
            }
            catch (Exception e)
            {
                Logger.Log(e);
                Application.Quit();
            }

            Logger.Log($"Total Memory: {GC.GetTotalMemory(true)}");
        }

        /// <summary>
        /// Creates an ItemSpawnerId object for a given SpawnerEntry 
        /// </summary>
        /// <param name="entry">The entry to get an ItemSpawnerId for</param>
        /// <returns>An ItemSpawnerId object that can be inserted into the IM's menu</returns>
        private static ItemSpawnerID GetItemSpawnerId(SpawnerEntry entry)
        {
            //Parameter validation
            var itemEntry = entry ?? throw new ArgumentNullException(nameof(entry));
            var itemId = itemEntry.ItemId ?? throw new ArgumentNullException(nameof(itemEntry.ItemId));
            var catString = itemEntry.Category ?? throw new ArgumentNullException(nameof(itemEntry.Category));
            var subcatString = itemEntry.SubCategory ?? throw new ArgumentNullException(nameof(itemEntry.SubCategory));

            if(string.IsNullOrEmpty(itemId))
                throw new Exception($"Entry's ItemId is null");

            if (string.IsNullOrEmpty(catString))
                throw new Exception($"Entry's Category is null");

            if (string.IsNullOrEmpty(subcatString))
                throw new Exception($"Entry's SubCategory is null");

            Logger.Log($"Getting {itemId}");
            var item = ResolveFVRObjectByItemId(itemId);

            if (item == null)
                throw new Exception($"Could not find a resource with ID {itemEntry.ItemId}");

            //Create category and subcategory variables
            ItemSpawnerID.EItemCategory cat;
            ItemSpawnerID.ESubCategory subcat;
            
            //Try to parse out values from the entry
            try
            {
                cat = (ItemSpawnerID.EItemCategory) Enum.Parse(typeof(ItemSpawnerID.EItemCategory), catString);
            }
            catch (Exception e)
            {
                throw new Exception($"Could not parse category from {entry.Category}", e);
            }

            try
            {
                subcat = (ItemSpawnerID.ESubCategory)Enum.Parse(typeof(ItemSpawnerID.ESubCategory), subcatString);
            }
            catch (Exception e)
            {
                throw new Exception($"Could not parse subcategory from {entry.SubCategory}", e);
            }

            //Create ItemSpawnerId object
            var newItemSpawnerId = new ItemSpawnerID
            {
                Category = cat,
                SubCategory = subcat,
                Description = entry.Description,
                DisplayName = item.DisplayName,
                hideFlags = HideFlags.None,
                Infographic = null, //TODO add support for this
                IsDisplayedInMainEntry = true,
                MainObject = item,
                ItemID = item.ItemID,
                IsReward = false,
                IsUnlockedByDefault = true,
                Sprite = null, //TODO add support for this
                //Recursivly retreive entries for subitems
                //TODO this might break if 2 or more guns have the same item in their secondaries
                Secondaries = entry.Secondaries != null && entry.Secondaries.Any() ? entry.Secondaries.Select(GetItemSpawnerId).ToArray() : null
            };

            try
            {
                //Add secondaries to the created item's secondaries that link back to it
                if (newItemSpawnerId.Secondaries != null && newItemSpawnerId.Secondaries.Any())
                {
                    foreach (var itemSpawnerId in newItemSpawnerId.Secondaries)
                    {
                        if (itemSpawnerId == null)
                            throw new Exception($"ItemSpawnerId is null for one of {newItemSpawnerId.DisplayName}'s secondaries");
                        var sec = itemSpawnerId.Secondaries?.ToList();
                        sec?.Add(newItemSpawnerId);
                        itemSpawnerId.Secondaries = sec?.ToArray();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log($"Failed to set secondaries");
                Logger.Log(e);
                Application.Quit();
            }
            return newItemSpawnerId;
        }

        public static FVRObject ResolveFVRObjectByItemId(string ItemId) => Resources.FindObjectsOfTypeAll<FVRObject>().SingleOrDefault(o => o.ItemID == ItemId);
    }
}
