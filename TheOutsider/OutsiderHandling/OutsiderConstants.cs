namespace TheOutsider
{
    public sealed class OutsiderConstants
    {
        public const NomaiRemoteCameraPlatform.ID DBNomaiWarpID = (NomaiRemoteCameraPlatform.ID)100;
        public const DreamArrivalPoint.Location DBDreamWarpID = (DreamArrivalPoint.Location)450;

        public const string SeedPPH = "InnerWarp_Index21";
        public const string LogPPH = "DB_PROJECTION_POOL_HOUSE";
        public const string LogPowerStation = "POWER_STATION";

        public const string ComputerParentPowerStation = "Station";
        public const string ComputerParentObservatory = "Observatory";

        public const string PowerStation = "PowerStation";
        public const string FriendMusic = "MUSIC_FRIEND";
    }
    public sealed class OutsiderPath
    {
        public const string SecretRoomDoor = "SectorDB_SouthPole/SectorDB_FriendsHouse/SecretRoomDoor";
    }

    public sealed class OutsiderSector
    {
        public const string PowerStation = "SectorDB_PowerStation";
        public const string BrambleDimension = "SectorDB_BrambleDimension";
        
        public const string Observatory = "SectorDB_Observatory";
        public const string SouthPole = "SectorDB_SouthPole";
        public const string StudyTower = "SectorDB_StudyTower";
        public const string HuntingBlind = "SectorDB_HuntingBlind";
        public const string HuntingBlindCloser = "SectorDB_CloseHuntingBlind";

        public const string ShuttleCrusher = "SectorDB_ShuttleCrusher";

        public const string EyeShack = "SectorDB_EyeShack";
        public const string ArtHouse = "SectorDB_ArtHouse";
        public const string FriendsHouse = "SectorDB_FriendsHouse";
        public const string DaturaHouse = "SectorDB_DaturaHouse";

        public const string StarterHouse = "StarterHouse";
        public const string StudyTowerRoot = "StudyTowerRoot";
    }

    public sealed class OutsiderLightSwitch
    {
        public const string PowerStation = "LS_PowerStation";
        public const string BrambleDimension = "LS_PPH";

        public const string Observatory = "LS_Observatory";
        public const string SouthPole = "LS_SouthPole";
        public const string HuntingBlind = "LS_HuntingBlind";
        public const string ShuttleCrusher = "LS_ShuttleCrusher";
    }
    public sealed class OutsiderAsset
    {
        public const string PowerStationComputer = "PowerStation_z_Computer.bytes";
        public const string ObservatoryComputer = "Observatory_Computer.bytes";
        public const string FeldsparRecording1 = "FeldsparRecording1.bytes";

        public const string FeldsparDialogue = "Feldspar_NEW.bytes";
        public const string SlateDialogue = "Slate_NEW.bytes";

        public const string DBAtmoCookie = "AmbientLight_DB_Exterior_New.png";

        public const string OcclusionLightBasic = "OcclusionLight_Basic.png";
        public const string OcclusionLightST = "OcclusionLight_StudyTower.png";
    }

    public sealed class OutsiderMatName
    {
        public const string LightWood = "Structure_HEA_WoodBeams_mat";
        public const string DarkWood = "Structure_HEA_DarkWoodBeams_mat 1";

    }
}