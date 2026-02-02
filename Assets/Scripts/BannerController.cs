using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class BannerCarousel : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [Header("Banner Settings")]
    [SerializeField] private Transform bannersContainer;
    [SerializeField] private RectTransform[] banners;
    [SerializeField] private float spacing = 20f;
    [SerializeField] private float autoScrollInterval = 5f;
    [SerializeField] private float swipeThreshold = 0.2f;
    [SerializeField] private float transitionDuration = 0.3f;
    
    [Header("Visuals")]
    [SerializeField] private Image[] dots;
    [SerializeField] private Color activeDotColor = Color.white;
    [SerializeField] private Color inactiveDotColor = new Color(1, 1, 1, 0.5f);
    
    private GridLayoutGroup  _gridLayoutGroup;
    private Coroutine _autoScrollCoroutine;
    private RectTransform _container;
    private float _containerWidth;
    private Vector3[] _bannerOriginalPositions;
    private float[] _bannerTargetXPositions;
    private float[] _bannerTargetYPositions;
    private int _currentIndex;
    private bool _isTransitioning;
    private float _currentOffsetX;
    private bool _isDragging;
    
    private void Awake()
    {
        _gridLayoutGroup = bannersContainer.GetComponent<GridLayoutGroup>();
        _container = GetComponent<RectTransform>();

        if (bannersContainer == null && banners.Length > 0)
        {
            bannersContainer = banners[0].parent;
        }
        
        InitializeBanners();
    }

    private void InitializeBanners()
    {
        _containerWidth = _container.rect.width;
        _bannerOriginalPositions = new Vector3[banners.Length];
        
        for (int i = 0; i < banners.Length; i++)
        {
            _bannerOriginalPositions[i] = banners[i].position;
        }
        
        _bannerTargetXPositions = new float[banners.Length];
        
        for (int i = 0; i < banners.Length; i++)
        {
            _bannerTargetXPositions[i] = i * (_containerWidth + spacing);
        }
        
        ResetBannerPositions();
    }
    
    private void ResetBannerPositions()
    {
        for (int i = 0; i < banners.Length; i++)
        {
            float targetX = _bannerTargetXPositions[i] + _currentOffsetX;
            
            Vector3 worldPos = _bannerOriginalPositions[i];
            worldPos.x = bannersContainer.position.x + targetX;
            worldPos.y = bannersContainer.position.y;
            banners[i].position = worldPos;
        }
    }
    
    private void Start()
    {
        StartAutoScroll();
        UpdateDots();
        UpdateGridLayoutGroup();
    }
    
    private void StartAutoScroll()
    {
        if (_autoScrollCoroutine != null)
        {
            StopCoroutine(_autoScrollCoroutine);
        }
        
        _autoScrollCoroutine = StartCoroutine(AutoScrollRoutine());
    }
    
    private IEnumerator AutoScrollRoutine()
    {
        yield return new WaitForSeconds(5f);
        
        while (true)
        {
            yield return new WaitForSeconds(autoScrollInterval);
            
            if (!_isTransitioning && !_isDragging)
            {
                GoToNextBanner();
            }
        }
    }
    
    private void GoToNextBanner()
    {
        if (_isTransitioning || banners.Length <= 1)
        {
            return;
        }
        
        int nextIndex = (_currentIndex + 1) % banners.Length;
        StartCoroutine(TransitionToBanner(nextIndex));
    }
    
    private void GoToPreviousBanner()
    {
        if (_isTransitioning || banners.Length <= 1)
        {
            return;
        }
        
        int prevIndex = (_currentIndex - 1 + banners.Length) % banners.Length;
        StartCoroutine(TransitionToBanner(prevIndex));
    }
    
    private IEnumerator TransitionToBanner(int targetIndex)
    {
        _isTransitioning = true;
        
        float startOffset = _currentOffsetX;
        float targetOffset = -_bannerTargetXPositions[targetIndex];
        
        float elapsedTime = 0f;
        
        while (elapsedTime < transitionDuration)
        {
            float t = Mathf.SmoothStep(0, 1, elapsedTime / transitionDuration);
            _currentOffsetX = Mathf.Lerp(startOffset, targetOffset, t);
            ResetBannerPositions();
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        _currentOffsetX = targetOffset;
        ResetBannerPositions();
        _currentIndex = targetIndex;
        UpdateDots();
        
        _isTransitioning = false;
    }
    
    private void UpdateDots()
    {
        if (dots == null || dots.Length == 0)
        {
            return;
        }
        
        for (int i = 0; i < dots.Length; i++)
        {
            dots[i].color = (i == _currentIndex) ? activeDotColor : inactiveDotColor;
        }
    }

    private void UpdateGridLayoutGroup()
    {
        _gridLayoutGroup.cellSize = new Vector2(_container.rect.width, _container.rect.height);
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (_isTransitioning)
        {
            return;
        }
        
        _isDragging = true;
        
        if (_autoScrollCoroutine != null)
        {
            StopCoroutine(_autoScrollCoroutine);
        }
        
        _currentOffsetX += eventData.delta.x;
        ResetBannerPositions();
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isTransitioning)
        {
            return;
        }
        
        _isDragging = false;
        
        float swipeDistance = eventData.position.x - eventData.pressPosition.x;
        
        if (Mathf.Abs(swipeDistance) > swipeThreshold * Screen.width)
        {
            if (swipeDistance > 0)
            {
                GoToPreviousBanner();
            }
            else
            {
                GoToNextBanner();
            }
        }
        else
        {
            StartCoroutine(SnapToNearestBanner());
        }
        
        StartAutoScroll();
    }
    
    private IEnumerator SnapToNearestBanner()
    {
        _isTransitioning = true;
        
        float minDistance = float.MaxValue;
        int nearestIndex = _currentIndex;
        
        for (int i = 0; i < banners.Length; i++)
        {
            float targetOffset = -_bannerTargetXPositions[i];
            float distance = Mathf.Abs(_currentOffsetX - targetOffset);
            
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = i;
            }
        }
        
        yield return TransitionToBanner(nearestIndex);
    }
    
    private void OnDestroy()
    {
        if (_autoScrollCoroutine != null)
        {
            StopCoroutine(_autoScrollCoroutine);
        }
    }
}