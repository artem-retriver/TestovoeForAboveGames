using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    [Header("Main Objects:")]
    [SerializeField] private Button[] tabButtons;
    [SerializeField] private ImageGallery gallery;

    [Header("Main options:")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);

    private TabType _currentTab = TabType.All;

    private void Awake()
    {
        if (gallery == null)
        {
            Debug.LogError("ImageGallery is not assigned", this);
            enabled = false;
        }
    }

    private void Start()
    {
        for (int i = 0; i < tabButtons.Length; i++)
        {
            if (tabButtons[i] == null)
                continue;

            TabType tab = (TabType)i;
            tabButtons[i].onClick.AddListener(() =>
            {
                SoundController.Instance.PlayTabClick();
                SetActiveTab(tab);
            });
        }

        SetActiveTab(TabType.All);
    }

    private void SetActiveTab(TabType tab)
    {
        if (_currentTab == tab)
            return;

        _currentTab = tab;

        UpdateVisuals();
        gallery.SetFilter(tab);
    }

    private void UpdateVisuals()
    {
        for (int i = 0; i < tabButtons.Length; i++)
        {
            Button button = tabButtons[i];
            if (button == null)
                continue;

            bool isActive = (TabType)i == _currentTab;
            Color color = isActive ? activeColor : inactiveColor;

            if (button.image != null)
                button.image.color = color;

            var text = button.GetComponentInChildren<Text>(true);
            if (text != null)
                text.color = color;
        }
    }
}

public enum TabType
{
    All,
    Odd,
    Even
}