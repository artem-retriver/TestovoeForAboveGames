using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class ImageGallery : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private string baseUrl = "https://data.ikppbb.com/test-task-unity-data/pics/";
    [SerializeField] private int totalImages = 66;
    [SerializeField] private int preloadRows = 2;

    [Header("UI")]
    [SerializeField] private RectTransform content;
    [SerializeField] private GridLayoutGroup grid;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GameObject imagePrefab;

    [Header("Popup Objects")] 
    [SerializeField] private GameObject basePopup;
    [SerializeField] private Image basePopupImage;
    [SerializeField] private GameObject premiumPopup;

    private readonly List<ImageItem> _items = new();
    private readonly Dictionary<int, Sprite> _cache = new();
    
    private RectTransform _viewport;
    private bool _initialized;

    #region Unity

    private void Awake()
    {
        _viewport = scrollRect.viewport;
        ConfigureGrid();
    }

    private void Start()
    {
        BuildItems();

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        Canvas.ForceUpdateCanvases();

        scrollRect.onValueChanged.AddListener(_ => TryLoadVisible());
        _initialized = true;

        TryLoadVisible();
    }


    private void OnDestroy()
    {
        scrollRect.onValueChanged.RemoveAllListeners();

        foreach (var sprite in _cache.Values)
        {
            if (sprite != null)
            {
                Destroy(sprite.texture);
                Destroy(sprite);
            }
        }
    }

    private void OnRectTransformDimensionsChange()
    {
        if (!_initialized) return;

        ConfigureGrid();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        TryLoadVisible();
    }

    #endregion

    #region Setup

    private void ConfigureGrid()
    {
        bool isTablet = IsTablet();

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = isTablet ? 3 : 2;

        float spacing = 60f;
        float padding = 50f;

        grid.spacing = Vector2.one * spacing;
        grid.padding = new RectOffset((int)padding, (int)padding, (int)padding, (int)padding);
        grid.cellSize = isTablet ? new Vector2(650, 650) : new Vector2(600, 600);
    }

    private bool IsTablet()
    {
        float width = Screen.width; 
        float height = Screen.height; 
        float aspectRatio = Mathf.Max(width, height) / Mathf.Min(width, height); 
        
        return aspectRatio < 1.45f;
    }

    private void BuildItems()
    {
        for (int i = 1; i <= totalImages; i++)
        {
            GameObject root = Instantiate(imagePrefab, content);
            ContentController contentController = root.GetComponent<ContentController>();
            root.name = $"Image_{i}";
            
            Image mainImage = contentController.GetMainImage();
            //Image premiumBadge = null;

            //foreach (var img in root.GetComponentsInChildren<Image>(true))
            //{
            //    if (img.name == "MainImage")
            //        mainImage = img;
            //    else if (img.name == "PremiumBadge")
            //        premiumBadge = img;
            //}

            if (mainImage == null)
            {
                Destroy(root);
                continue;
            }

            //if (contentController != null)
            //{
            //    premiumBadge.gameObject.SetActive(i % 4 == 0);
            //}
            
            contentController.ChangeContentStatus(i % 4 == 0);

            mainImage.sprite = null;
            mainImage.color = new Color(0.9f, 0.9f, 0.9f, 1f);

            _items.Add(new ImageItem
            {
                index = i,
                content = contentController,
                image = mainImage,
                rect = root.GetComponent<RectTransform>()
            });
        }
    }
    
    public void SetFilter(TabType filter)
    {
        foreach (var item in _items)
        {
            bool visible = filter switch
            {
                TabType.Odd  => item.index % 2 == 1,
                TabType.Even => item.index % 2 == 0,
                _            => true
            };

            item.rect.gameObject.SetActive(visible);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        TryLoadVisible();
    }

    #endregion

    #region Loading

    private void TryLoadVisible()
    {
        if (!_initialized)
            return;

        if (!isActiveAndEnabled)
            return;

        foreach (var item in _items)
        {
            if (item.IsLoaded || item.IsLoading)
                continue;

            bool visible = IsVisibleWithPreload(item.rect);

            if (visible)
                StartCoroutine(LoadImage(item));
        }
    }


    private bool IsVisibleWithPreload(RectTransform item)
    {
        _viewport.GetWorldCorners(_viewportCorners);
        item.GetWorldCorners(_itemCorners);

        float viewportTop = _viewportCorners[1].y;
        float viewportBottom = _viewportCorners[0].y;

        float itemTop = _itemCorners[1].y;
        float itemBottom = _itemCorners[0].y;

        float preload = grid.cellSize.y * preloadRows;

        return itemBottom < viewportTop + preload &&
               itemTop > viewportBottom - preload;
    }


    private IEnumerator LoadImage(ImageItem item)
    {
        item.IsLoading = true;

        if (_cache.TryGetValue(item.index, out var cached))
        {
            Apply(item, cached);
            yield break;
        }

        using UnityWebRequest req =
            UnityWebRequestTexture.GetTexture($"{baseUrl}{item.index}.jpg");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            item.IsLoading = false;
            yield break;
        }

        Texture2D tex = DownloadHandlerTexture.GetContent(req);
        Sprite sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            Vector2.one * 0.5f
        );

        _cache[item.index] = sprite;
        Apply(item, sprite);
    }

    private void Apply(ImageItem item, Sprite sprite)
    {
        item.image.sprite = sprite;
        item.image.color = Color.white;
        item.IsLoaded = true;
        item.IsLoading = false;
        item.content.ChangeIsReady(true);
        item.content.GetImageGallery(this);
    }

    #endregion

    #region Helpers

    private static readonly Vector3[] _viewportCorners = new Vector3[4];
    private static readonly Vector3[] _itemCorners = new Vector3[4];

    private class ImageItem
    {
        public int index;
        public ContentController content;
        public Image image;
        public RectTransform rect;
        public bool IsLoaded;
        public bool IsLoading;
    }

    #endregion

    #region Popup

    public void ShowBasePopup(Image mainImage)
    {
        basePopupImage.sprite = mainImage.sprite;
        basePopup.SetActive(true);
        
        FitToScreenPreserveAspect(basePopupImage);
        
        StartCoroutine(ScaleAnimation(basePopup.transform, Vector3.one, 0.1f));
    }
    
    private void FitToScreenPreserveAspect(Image image)
    {
        if (image.sprite == null) 
            return;
        
        Canvas canvas = image.canvas;
        
        if (canvas == null) 
            return;
    
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        RectTransform imageRect = image.GetComponent<RectTransform>();
        
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;
        
        float spriteWidth = imageRect.sizeDelta.x;
        float spriteHeight = imageRect.sizeDelta.y;
        
        float widthRatio = canvasWidth / spriteWidth;
        float heightRatio = canvasHeight / spriteHeight;
        
        float scale = Mathf.Min(widthRatio, heightRatio);
        imageRect.localScale = new Vector3(scale, scale, 1f);
    }

    public void HideBasePopup()
    {
        StartCoroutine(ScaleAnimation(basePopup.transform, Vector3.zero, 0.1f, () =>
        {
            basePopupImage.sprite = null;
            basePopup.SetActive(false);
        }));
    }

    public void ShowPremiumPopup()
    {
        premiumPopup.SetActive(true);
        StartCoroutine(ScaleAnimation(premiumPopup.transform, Vector3.one, 0.1f));
    }

    public void HidePremiumPopup()
    {
        StartCoroutine(ScaleAnimation(premiumPopup.transform, Vector3.zero, 0.1f, () =>
        {
            premiumPopup.SetActive(false);
        }));
    }
    
    private static IEnumerator ScaleAnimation(Transform popupTransform, Vector3 targetScale, float duration, Action onComplete = null)
    {
        if (popupTransform == null)
            yield break;
    
        Vector3 startScale = popupTransform.localScale;
        float time = 0f;
    
        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = time / duration;
        
            popupTransform.localScale = Vector3.Lerp(startScale, targetScale, progress);
        
            yield return null;
        }
    
        popupTransform.localScale = targetScale;
        onComplete?.Invoke();
    }

    #endregion
}
