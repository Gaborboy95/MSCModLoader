﻿//Core class (placeholder for core assets)
namespace MSCLoader
{
#pragma warning disable CS1591
    public class ModCore : Mod
    {
        public override string ID => "MSCLoader_Core"; 
        public override string Name => "MSCLoader"; 
        public override string Author => "Piotrulos";
        public override string Version => ModLoader.Version;
        public override bool UseAssetsFolder => true;
    }
#pragma warning restore CS1591
}
