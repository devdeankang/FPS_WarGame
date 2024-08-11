using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using WarGame;

public class ModificationSlot : MonoBehaviour
{
    public string SlotName;
    public Modificator[] PossibleModifications;
    public Modificator CurrentModificator;
    public Transform UIPivot;

    public UnityAction<Modificator> ModificatorAttached;
    public UnityAction<Modificator> ModificatorDetached;

    public bool isVisual = false;
    public bool isCanEmpty = true;
    public int DefaultMod = -1;

    public HexMenu.HexType ModType = HexMenu.HexType.Default;

    Modificator emptyMod;

    public int CurrentModificatorId
    {
        get
        {
            for (int i = 0; i < PossibleModifications.Length; i++)
            {
                if (PossibleModifications[i] == CurrentModificator)
                    return i;
            }

            return -1;
        }
    }

    void Start()
    {
        emptyMod = new Modificator();
        emptyMod.ModificatorName = "";
        foreach (Modificator m in PossibleModifications)
        {
            m.Detach();
        }

        if (DefaultMod == -1)
            CurrentModificator = emptyMod;
        else
            SetupMod(PossibleModifications[DefaultMod].ModId);
    }

    void OnValidate()
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer m in renderers)
        {
            m.enabled = isVisual;
        }
    }

    public void SetupMod(int id, bool withEvents = true)
    {
        if (CurrentModificator.ModId != id && !string.IsNullOrEmpty(CurrentModificator.ModificatorName))
        {
            CurrentModificator.Detach();

            if (withEvents && ModificatorDetached != null)
            {
                ModificatorDetached(CurrentModificator);
            }
            CurrentModificator = emptyMod;
        }

        foreach (Modificator m in PossibleModifications)
        {
            if (m.ModId == id && CurrentModificator != m)
            {
                CurrentModificator = m;
                m.Attach();

                if (withEvents)
                {
                    if (ModificatorAttached != null)
                    {
                        ModificatorAttached(m);
                    }
                }
            }
        }
    }
}
