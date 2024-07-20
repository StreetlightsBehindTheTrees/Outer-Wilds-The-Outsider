// Downloaded from https://github.com/Outer-Wilds-New-Horizons/new-horizons/blob/cb08f4710d184b778bb4d92e995ba264186744bb/NewHorizons/External/NewHorizonsData.cs and edited.

using System;
using System.Collections.Generic;
using TheOutsider;

namespace TheOutsiderFixes
{
    /// <summary>
    /// Copied from New Horizons to fix a bug where having just revealed ship logs would brick your save file after uninstalling
    /// </summary>
    public static class OutsiderSaveData
    {
        private static OutsiderSaveFile _saveFile;
        private static OutsiderProfile _activeProfile;
        private static string _activeProfileName;
        private static readonly string FileName = "save.json";

        private static object _lock = new();

        public static string GetProfileName() => StandaloneProfileManager.SharedInstance?.currentProfile?.profileName;

        public static void Load()
        {
            lock (_lock)
            {
                _activeProfileName = GetProfileName();
                if (_activeProfileName == null)
                {
                    Log.Warning("Couldn't find active profile, are you on Gamepass?");
                    _activeProfileName = "XboxGamepassDefaultProfile";
                }

                try
                {
                    _saveFile = ModMain.Instance.ModHelper.Storage.Load<OutsiderSaveFile>(FileName, false);
                    if (!_saveFile.Profiles.ContainsKey(_activeProfileName))
                        _saveFile.Profiles.Add(_activeProfileName, new OutsiderProfile());
                    _activeProfile = _saveFile.Profiles[_activeProfileName];
                    Log.Print($"Loaded save data for {_activeProfileName}");
                }
                catch (Exception)
                {
                    try
                    {
                        Log.Print($"Couldn't load save data from {FileName}, creating a new file");
                        _saveFile = new OutsiderSaveFile();
                        _saveFile.Profiles.Add(_activeProfileName, new OutsiderProfile());
                        _activeProfile = _saveFile.Profiles[_activeProfileName];
                        ModMain.Instance.ModHelper.Storage.Save(_saveFile, FileName);
                        Log.Print($"Loaded save data for {_activeProfileName}");
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Couldn't create save data:\n{e}");
                    }
                }
            }
        }

        public static void Save()
        {
            if (_saveFile == null) return;

            // Threads exist
            lock (_lock)
            {
                try
                {
                    ModMain.Instance.ModHelper.Storage.Save(_saveFile, FileName);
                }
                catch (Exception ex)
                {
                    Log.Error($"Couldn't save data:\n{ex}");
                }
            }
        }

        public static void Reset()
        {
            if (_saveFile == null || _activeProfile == null) Load();
            Log.Print($"Resetting save data for {_activeProfileName}");
            _activeProfile = new OutsiderProfile();
            _saveFile.Profiles[_activeProfileName] = _activeProfile;

            Save();
        }

        private class OutsiderSaveFile
        {
            public OutsiderSaveFile()
            {
                Profiles = new Dictionary<string, OutsiderProfile>();
            }

            public Dictionary<string, OutsiderProfile> Profiles { get; }
        }

        private class OutsiderProfile
        {
            public OutsiderProfile()
            {
                NewlyRevealedFactIDs = new List<string>();
            }

            public List<string> NewlyRevealedFactIDs { get; }
        }

        public static void AddNewlyRevealedFactID(string id)
        {
            _activeProfile?.NewlyRevealedFactIDs.Add(id);
            Save();
        }

        public static List<string> GetNewlyRevealedFactIDs()
        {
            return _activeProfile?.NewlyRevealedFactIDs;
        }

        public static void ClearNewlyRevealedFactIDs()
        {
            _activeProfile?.NewlyRevealedFactIDs.Clear();
            Save();
        }
    }
}
