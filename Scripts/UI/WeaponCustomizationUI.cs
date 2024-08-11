using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace WarGame
{
    public class WeaponCustomizationUI : MonoBehaviour
    {
        public static WeaponCustomizationUI Instance;
        public RectTransform RootExample;
        private Camera targetCamera;
        List<ModificationSlotUI> createdUISlots = new List<ModificationSlotUI>();
        WeaponCustomization currentCustomization;

        public Camera TargetCamera
        {
            get
            {
                if (targetCamera == null)
                    targetCamera = FindObjectOfType<PlayerController>().playerCamera;

                return targetCamera;
            }

            set
            {
                targetCamera = value;
            }
        }

        void Awake()
        {
            Instance = this;

            if(FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                throw new System.Exception("Event system is missing?");
            }
        }

        public void SetCustomizations(WeaponCustomization customization, List<ModificationSlot> slots)
        {
            currentCustomization = customization;
            List<ModificationSlotUI> dontDestroySlots = new List<ModificationSlotUI>();
            foreach (ModificationSlot slot in slots)
            {
                foreach (ModificationSlotUI uiSlot in createdUISlots)
                {
                    if (uiSlot.Slot.Equals(slot))
                    {
                        dontDestroySlots.Add(uiSlot);
                    }
                }
            }

            foreach (ModificationSlotUI slotUI in createdUISlots)
            {
                if (!dontDestroySlots.Contains(slotUI))
                {
                    Destroy(slotUI.Root.gameObject);
                }
            }

            createdUISlots.Clear();
            createdUISlots.AddRange(dontDestroySlots);

            foreach (ModificationSlot slot in slots)
            {
                if (!slot.gameObject.activeInHierarchy)
                    continue;

                bool contains = false;
                foreach (ModificationSlotUI slotUI in createdUISlots)
                {
                    if (slotUI.Slot.Equals(slot))
                    {
                        contains = true;
                        break;
                    }
                }
                if (contains)
                    continue;

                ModificationSlotUI uiSlot = new ModificationSlotUI(slot, RootExample, TargetCamera);

                var slotInstance = slot;
                uiSlot.ModSelected += (m) => Selected(slotInstance, m);
                uiSlot.ModRemoved += () => Removed(slotInstance);

                createdUISlots.Add(uiSlot);
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void Update()
        {
            if (createdUISlots != null)
            {
                foreach (ModificationSlotUI slotUI in createdUISlots)
                {
                    slotUI.Update();
                }
            }
        }

        public void Close()
        {
            foreach (ModificationSlotUI slotUI in createdUISlots)
            {
                Destroy(slotUI.Root.gameObject);
            }
            createdUISlots.Clear();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Selected(ModificationSlot slot, Modificator mod)
        {
            currentCustomization.ApplyMod(slot, mod);
        }

        void Removed(ModificationSlot slot)
        {
            currentCustomization.ClearMod(slot);
        }

        [System.Serializable]
        public class ModificationSlotUI
        {
            public ModificationSlot Slot;
            public RectTransform Root;
            public HexMenu Holder;
            public UnityAction<Modificator> ModSelected;
            public UnityAction ModRemoved;

            Camera targetCamera;

            public ModificationSlotUI(ModificationSlot slot, RectTransform rootExample, Camera transformationCamera)
            {
                Slot = slot;
                Root = Instantiate(rootExample, rootExample.parent);
                Root.gameObject.SetActive(true);
                Holder = Root.Find("HexMenu").GetComponent<HexMenu>();

                Holder.SetTitle(slot.SlotName);
                Holder.SetType(slot.ModType);
                Holder.ClearOptions();
                List<string> options = new List<string>();
                if (Slot.isCanEmpty)
                    Holder.AddOption(null, CheckId);

                foreach (Modificator mod in slot.PossibleModifications)
                {
                    Holder.AddOption(mod.ModIcon, CheckId);
                }

                if (Slot.isCanEmpty)
                    Holder.SetActiveOption(slot.CurrentModificatorId + 1);
                else
                    Holder.SetActiveOption(slot.CurrentModificatorId);

                targetCamera = transformationCamera;
            }

            void CheckId(int dropdownId)
            {
                if (Slot.isCanEmpty)
                {
                    if (dropdownId == 0)
                    {
                        RemoveMod();
                    }
                    else
                    {
                        Selected(dropdownId - 1);
                    }
                }
                else
                {
                    Selected(dropdownId);
                }
                Holder.SetActiveOption(dropdownId);
            }

            void RemoveMod()
            {
                if (ModRemoved != null)
                {
                    ModRemoved();
                }
            }

            void Selected(int modId)
            {
                if (ModSelected != null)
                {
                    ModSelected(Slot.PossibleModifications[modId]);
                }
            }

            public void Update()
            {
                Vector2 screenPosition = targetCamera.WorldToScreenPoint(Slot.UIPivot.position);

                Root.position = screenPosition;
            }
        }
    }
}