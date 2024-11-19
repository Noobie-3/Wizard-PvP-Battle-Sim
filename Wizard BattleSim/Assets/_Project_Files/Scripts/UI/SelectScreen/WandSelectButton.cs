using UnityEngine;
using UnityEngine.UI;

public class WandSelectButton : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject disabledOverlay;
    [SerializeField] private Button button;

    private WandSelectDisplay wandSelect;

    public Wand wand { get; private set; }
    public bool IsDisabled { get; private set; }

    public void SetWand(WandSelectDisplay WandSelect, Wand Wand)
    {
        iconImage.sprite = Wand.Icon;

        this.wandSelect = WandSelect;

        wand = Wand;
    }

    public void SelectWand()
    {
        wandSelect.SelectWand(wand);
    }

    public void SetDisabled()
    {
        IsDisabled = true;
        disabledOverlay.SetActive(true);
        button.interactable = false;
    }
}

