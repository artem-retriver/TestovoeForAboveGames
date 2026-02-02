using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class BannerCarousel : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [Header("Banner Settings")]
    [SerializeField] private RectTransform[] banners;
    [SerializeField] private float spacing = 20f;
    [SerializeField] private float autoScrollInterval = 5f;
    [SerializeField] private float swipeThreshold = 0.2f;
    [SerializeField] private float transitionSpeed = 10f;
    
    [Header("Visuals")]
    [SerializeField] private Image[] dots;
    [SerializeField] private Color activeDotColor = Color.white;
    [SerializeField] private Color inactiveDotColor = new (1, 1, 1, 0.5f);
    
    
    private Coroutine _autoScrollCoroutine;
    private RectTransform _container;
    private Vector2[] _bannerPositions;
    private int _currentIndex;
    private bool _isTransitioning;
    
    private void Awake()
    {
        _container = GetComponent<RectTransform>();
        InitializeBanners();
    }
    
    private void Start()
    {
        StartAutoScroll();
        UpdateDots();
    }
    
    private void InitializeBanners()
    {
        _bannerPositions = new Vector2[banners.Length];
        
        float containerWidth = _container.rect.width;
        
        for (int i = 0; i < banners.Length; i++)
        {
            float xPos = (containerWidth + spacing) * i;
            banners[i].anchoredPosition = new Vector2(xPos, 0);
            _bannerPositions[i] = banners[i].anchoredPosition;
        }
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
        while (true)
        {
            yield return new WaitForSeconds(autoScrollInterval);
            
            if (!_isTransitioning)
            {
                GoToNextBanner();
            }
        }
    }
    
    private void GoToNextBanner()
    {
        if (_isTransitioning)
        {
            return;
        }
        
        int nextIndex = (_currentIndex + 1) % banners.Length;
        StartCoroutine(TransitionToBanner(nextIndex));
    }
    
    private void GoToPreviousBanner()
    {
        if (_isTransitioning)
        {
            return;
        }
        
        int prevIndex = (_currentIndex - 1 + banners.Length) % banners.Length;
        StartCoroutine(TransitionToBanner(prevIndex));
    }
    
    private IEnumerator TransitionToBanner(int targetIndex)
    {
        _isTransitioning = true;
        
        Vector2 startOffset = banners[0].anchoredPosition;
        Vector2 targetOffset = new Vector2(-_bannerPositions[targetIndex].x, 0);
        
        float elapsedTime = 0f;
        
        while (elapsedTime < 1f)
        {
            float t = elapsedTime / 0.3f;
            
            Vector2 newOffset = Vector2.Lerp(startOffset, targetOffset, Mathf.SmoothStep(0, 1, t));
            
            for (int i = 0; i < banners.Length; i++)
            {
                banners[i].anchoredPosition = _bannerPositions[i] + newOffset;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        for (int i = 0; i < banners.Length; i++)
        {
            banners[i].anchoredPosition = _bannerPositions[i] + targetOffset;
        }
        
        _currentIndex = targetIndex;
        UpdateDots();
        
        if (_autoScrollCoroutine != null)
        {
            StopCoroutine(_autoScrollCoroutine);
        }
        _autoScrollCoroutine = StartCoroutine(AutoScrollRoutine());
        
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
    
    public void OnDrag(PointerEventData eventData)
    {
        if (_isTransitioning)
        {
            return;
        }
        
        float dragDelta = eventData.delta.x / Screen.width;
        
        foreach (var banner in banners)
        {
            banner.anchoredPosition += new Vector2(dragDelta * _container.rect.width, 0);
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isTransitioning) return;
        
        float swipeValue = (eventData.position.x - eventData.pressPosition.x) / Screen.width;
        
        if (Mathf.Abs(swipeValue) >= swipeThreshold)
        {
            if (swipeValue > 0)
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
            StartCoroutine(TransitionToBanner(_currentIndex));
        }
    }
    
    private void OnDestroy()
    {
        if (_autoScrollCoroutine != null)
        {
            StopCoroutine(_autoScrollCoroutine);
        }
    }
}