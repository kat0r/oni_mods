using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSerialization;
using PeterHan.PLib.Detours;
using UnityEngine;
using System.Runtime.Serialization;

namespace ImprovedFilteredStorage
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class ImprovedTreeFilterable : KMonoBehaviour, ISaveLoadable
    {
        private TreeFilterable treeFilterable;
        public IUserControlledCapacity userControlledCapacity;

#pragma warning disable CS0169
        [MyCmpAdd]
        private ImprovedTreeFilterableActivateToggleButton activateToggleButton;

        [Serialize]
        public bool Enabled;

        private Dictionary<Tag, float> acceptedElements = new Dictionary<Tag, float>();

        [Serialize]
        public List<KeyValuePair<Tag, float>> acceptedElements_serialized
        {
            get
            {
                return acceptedElements.ToList();
            }
            set
            {
                acceptedElements.Clear();
                if (value != null)
                    foreach (var element in value)
                        acceptedElements.Add(element.Key, element.Value);
            }
        }

        public event EventHandler OnUpdateFilters;
        private static readonly EventSystem.IntraObjectHandler<ImprovedTreeFilterable> OnCopySettingsDelegate = new EventSystem.IntraObjectHandler<ImprovedTreeFilterable>((System.Action<ImprovedTreeFilterable, object>)((component, data) => component.OnCopySettings(data)));
        private static readonly IDetouredField<FilteredStorage, IUserControlledCapacity> IUSERCONTROLLEDCAPACITY = PDetours.DetourField<FilteredStorage, IUserControlledCapacity>("capacityControl");

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            this.Subscribe<ImprovedTreeFilterable>((int)GameHashes.CopySettings, ImprovedTreeFilterable.OnCopySettingsDelegate);
            treeFilterable = GetComponent<TreeFilterable>();
            userControlledCapacity = GetComponent(typeof(IUserControlledCapacity)) as IUserControlledCapacity;

            if (userControlledCapacity == null)
            {
                var storageLocker = treeFilterable.gameObject.GetComponent<StorageLocker>();
                if (storageLocker != null)
                {
                    var filteredStorage = FILTEREDSTORAGE.Get(storageLocker);
                    if (filteredStorage != null)
                    {
                        userControlledCapacity = IUSERCONTROLLEDCAPACITY.Get(filteredStorage);
                    }
                }
            }
            //Debug.Log($"userControlledCapacity :{userControlledCapacity == null} on {gameObject}");
        }
 
        public void RemoveIncorrectAcceptedTags()
        {
            if (!Enabled)
                return;

            Storage storage = GetComponent<Storage>();
            float storageMaxCap = userControlledCapacity != null ? userControlledCapacity.MaxCapacity : 20000f;

            foreach (var tag in storage.GetAllTagsInStorage())
            {
                if (acceptedElements.ContainsKey(tag))
                {
                    var overflow = storage.GetAmountAvailable(tag) - acceptedElements[tag];
                    if (storage.MassStored() > storageMaxCap) // someone put more in storage than ever should be in there
                        overflow = Mathf.Max(overflow, storage.MassStored() - storageMaxCap); // drop as much as we can, DropSome should check

                    if (overflow > 0)
                        storage.DropSome(tag, overflow);
                }
                else
                {
                    storage.Drop(tag);
                }
            }
        }

        private static readonly IDetouredField<StorageLocker, FilteredStorage> FILTEREDSTORAGE = PDetours.DetourField<StorageLocker, FilteredStorage>("filteredStorage");
        public void AddTagToFilter(Tag t, float amount)
        {
            acceptedElements[t] = amount;
            UpdateCapacityControl();
        }

        public IDictionary<Tag, float> GetAcceptedElements()
        {
            return acceptedElements;
        }

        protected override void OnCleanUp()
        {
            base.OnCleanUp();
        }

        private void OnCopySettings(object data)
        {
            ImprovedTreeFilterable component = ((GameObject)data).GetComponent<ImprovedTreeFilterable>();
            if (component == null)
                return;

            this.Enabled = component.Enabled;
            this.acceptedElements = component.acceptedElements;
            this.UpdateFilters(acceptedElements.Keys.ToList());
        }

        public void UpdateFilters(IList<Tag> filters)
        {
            acceptedElements = filters.ToDictionary(key => key, key => acceptedElements.ContainsKey(key) ? acceptedElements[key] : 0);
            if (!Enabled)
                return;
            
            RemoveIncorrectAcceptedTags();
            UpdateCapacityControl();
            
            OnUpdateFilters?.Invoke(this, null);       
        }

        private static readonly IDetouredField<FilteredStorage, FetchList2> FETCHLIST = PDetours.DetourField<FilteredStorage, FetchList2>("fetchList");
        private static readonly IDetouredField<FilteredStorage, Storage> STORAGE = PDetours.DetourField<FilteredStorage, Storage>("storage");
        private static readonly IDetouredField<FilteredStorage, ChoreType> CHORETYPE = PDetours.DetourField<FilteredStorage, ChoreType>("choreType");
        private static readonly IDetouredField<FilteredStorage, Tag[]> REQUIREDTAGS = PDetours.DetourField<FilteredStorage, Tag[]>("requiredTags");
        private static readonly IDetouredField<FilteredStorage, Tag[]> FORBIDDENTAGS = PDetours.DetourField<FilteredStorage, Tag[]>("forbiddenTags");
        public static readonly DetouredMethod<System.Action<FilteredStorage>> ONFETCHCOMPLETE = typeof(FilteredStorage).DetourLazy<System.Action<FilteredStorage>>("OnFetchComplete");

        public void GenerateFetchList(FilteredStorage __instance)
        {
            if (FETCHLIST.Get(__instance) != null)
            {
                FETCHLIST.Get(__instance).Cancel("");
                FETCHLIST.Set(__instance, (FetchList2)null);
            }

            RemoveIncorrectAcceptedTags();

            FETCHLIST.Set(__instance, new FetchList2(STORAGE.Get(__instance), CHORETYPE.Get(__instance)));
            FETCHLIST.Get(__instance).ShowStatusItem = false;

            float storageLeft = userControlledCapacity != null ? userControlledCapacity.MaxCapacity : 20000f;
            //Debug.Log($"storageLeft: {storageLeft}");

            foreach (var tag in GetAcceptedElements())
            {
                if (storageLeft <= 0)
                    break;

                float amountMissing = tag.Value - STORAGE.Get(__instance).GetAmountAvailable(tag.Key);
                if (amountMissing <= 0)
                    continue;

                if (amountMissing > storageLeft)
                    amountMissing = storageLeft;

                storageLeft -= amountMissing;
                //Debug.Log($"add: {tag.Key}, {amountMissing}");

                FETCHLIST.Get(__instance).Add(new[] { tag.Key }, REQUIREDTAGS.Get(__instance), FORBIDDENTAGS.Get(__instance), amountMissing, FetchOrder2.OperationalRequirement.Functional);
            }
            FETCHLIST.Get(__instance).Submit(new System.Action(() => ONFETCHCOMPLETE.Invoke(__instance)), false);
        }

        // to update the little info on the right "x of y stored"
        private void UpdateCapacityControl()
        {
            if (userControlledCapacity != null && Enabled)
            {
                var sum = Mathf.Clamp(acceptedElements.Values.Sum(), 0, userControlledCapacity.MaxCapacity);
                userControlledCapacity.UserMaxCapacity = sum;
            }
        }
    }
}
