using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KSerialization;

namespace DontUseMutatedPlants
{
    [SerializationConfig(MemberSerialization.OptIn)]
    internal class AllowUseMutationsComp : KMonoBehaviour, ISaveLoadable
    {
        [Serialize]
        public bool AllowUsageOfMutations;

#pragma warning disable CS0169
        [MyCmpAdd]
        private AllowUserMutationsButton allowUserMutationsButton;
    }
}
