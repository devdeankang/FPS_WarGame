using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HexMenu : MonoBehaviour
{
    public Button[] HexButtons;
    public Sprite DefaultCenter;
    public Sprite ColorCenter;
    public Image CenterUI;
    public Sprite DefaultButton;
    public Sprite SelectedButton;
    public Sprite NoneIcon;
    public Text Title;

    public enum HexType
    {
        Default,
        Color
    }

    ZonePointerNotifier zoneNotifier;
    Animator menuAnimator;

    const int maxSlots = 6;

    int currentSlot = 0;
    
	void Start ()
    {
        foreach (Button b in HexButtons)
        {
            b.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
        }

        zoneNotifier = GetComponent<ZonePointerNotifier>();
        menuAnimator = GetComponent<Animator>();

        zoneNotifier.EnterEvent.AddListener(InZone);
        zoneNotifier.ExitEvent.AddListener(OutZone);
    }

    void InZone()
    {
        menuAnimator.Play("HexMenuShow");
    }

    void OutZone()
    {
        menuAnimator.Play("HexMenuHide");
    }

    public void SetTitle(string title)
    {
        title = title.ToUpper();
        Title.text = title;
    }

    public void SetType(HexType ht)
    {
        if(ht == HexType.Default)
        {
            CenterUI.sprite = DefaultCenter;
        } else if(ht == HexType.Color)
        {
            CenterUI.sprite = ColorCenter;
        }
    }
	
    public void AddOption(Sprite icon, UnityAction<int> callback)
    {
        if (currentSlot == maxSlots)
        {
            throw new System.Exception("Max slots " + name);
        }

        if (icon == null)
            icon = NoneIcon;

        HexButtons[currentSlot].gameObject.SetActive(true);
        HexButtons[currentSlot].onClick.RemoveAllListeners();
        var slotIndex = currentSlot;
        HexButtons[currentSlot].onClick.AddListener(() => callback(slotIndex));
        Image iconImage = HexButtons[currentSlot].transform.GetChild(0).GetComponent<Image>();
        iconImage.sprite = icon;
        iconImage.SetNativeSize();
        currentSlot++;
    }

    public void SetActiveOption(int optionIndex)
    {
        foreach (Button b in HexButtons)
        {
            b.GetComponent<Image>().sprite = DefaultButton;
        }
        HexButtons[optionIndex].GetComponent<Image>().sprite = SelectedButton;
    }

    public void ClearOptions()
    {
        foreach(Button b in HexButtons)
        {
            b.gameObject.SetActive(false);
        }
    }
}
