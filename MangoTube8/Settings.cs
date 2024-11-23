using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Diagnostics;

namespace MangoTube8
{
    public static class Settings
    {
        public const string InnerTubeApiKey = "AIzaSyAO_FJ2SlqU8Q4STEHLGCilw_Y9_11qcW8";

        private const string SeedVideoIdsKey = "SeedVideoIds";
        private const string VideoQualityKey = "VideoQuality";

        public static List<string> Qualities { get; } = new List<string> { "Medium (recommended)", "HD" };
        private static List<string> seedVideoIds;

        private static readonly List<string> DefaultSeedVideoIds = new List<string>
        {
            "4NxiAMLjKWs",
            "vfbmLQmMPl4",
            "Yb8g1dtEuUA",
            "1HdibCYa9FI",
            "AUJFAx9rsVc"
        };

        public static string VideoQuality
        {
            get
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                string quality = settings.Contains(VideoQualityKey)
                    ? settings[VideoQualityKey] as string
                    : Qualities[0];

                Debug.WriteLine("Retrieved video quality: " + quality);

                return quality;
            }
            set
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;

                Debug.WriteLine("Setting video quality: " + value);

                settings[VideoQualityKey] = value;
                settings.Save();
            }
        }

        public static IReadOnlyList<string> SeedVideoIds
        {
            get
            {
                if (seedVideoIds == null)
                {
                    LoadSeedVideoIds();
                }
                return seedVideoIds.AsReadOnly();
            }
        }

        public static void AddSeedVideoId(string videoId)
        {
            if (seedVideoIds == null)
            {
                LoadSeedVideoIds();
            }

            if (seedVideoIds.Count >= 5)
            {
                seedVideoIds.RemoveAt(0);
            }

            seedVideoIds.Add(videoId);
            SaveSeedVideoIds();
        }

        public static void RemoveSeedVideoId(string videoId)
        {
            if (seedVideoIds == null)
            {
                LoadSeedVideoIds();
            }

            if (seedVideoIds.Contains(videoId))
            {
                seedVideoIds.Remove(videoId);
                SaveSeedVideoIds();
            }
        }

        public static void LoadSeedVideoIds()
        {
            seedVideoIds = new List<string>();

            try
            {
                var storage = IsolatedStorageFile.GetUserStoreForApplication();
                if (storage.FileExists(SeedVideoIdsKey))
                {
                    using (var fileStream = storage.OpenFile(SeedVideoIdsKey, System.IO.FileMode.Open))
                    using (var reader = new System.IO.StreamReader(fileStream))
                    {
                        string seedData = reader.ReadToEnd();
                        seedVideoIds = string.IsNullOrEmpty(seedData)
                            ? new List<string>(DefaultSeedVideoIds)
                            : seedData.Split(',').ToList();
                    }
                }
                else
                {
                    seedVideoIds = new List<string>(DefaultSeedVideoIds);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error loading seed video IDs: " + ex.Message);
                seedVideoIds = new List<string>(DefaultSeedVideoIds);
            }
        }

        private static void SaveSeedVideoIds()
        {
            try
            {
                var storage = IsolatedStorageFile.GetUserStoreForApplication();
                using (var fileStream = storage.OpenFile(SeedVideoIdsKey, System.IO.FileMode.Create))
                using (var writer = new System.IO.StreamWriter(fileStream))
                {
                    string seedData = string.Join(",", seedVideoIds);
                    writer.Write(seedData);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error saving seed video IDs: " + ex.Message);
            }
        }

        public static string GetRandomSeedVideoId()
        {
            if (seedVideoIds == null || seedVideoIds.Count == 0)
            {
                return null;
            }

            Random random = new Random();
            int randomIndex = random.Next(seedVideoIds.Count);
            return seedVideoIds[randomIndex];
        }
    }
}