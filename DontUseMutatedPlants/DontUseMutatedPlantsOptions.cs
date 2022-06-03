using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;

namespace DontUseMutatedPlants
{
    [ModInfo("https://github.com/kat0r/oni_mods", "preview.png")]
    [JsonObject(MemberSerialization.OptIn)]
    [RestartRequired]
    internal class DontUseMutatedPlantsOptions : SingletonOptions<DontUseMutatedPlantsOptions>
    {
        [Option("Add to Conveyor Loader", "Also adds this feature to Conveyor Loaders")]
        [JsonProperty]
        public bool AddToConveyorLoader { get; set; }

        public DontUseMutatedPlantsOptions()
        {
            AddToConveyorLoader = false;
        }

        public override string ToString()
        {
            return $"DontUseMutatedPlantsOptions[AddToConveyorLoader={AddToConveyorLoader}]";
        }
    }
}
