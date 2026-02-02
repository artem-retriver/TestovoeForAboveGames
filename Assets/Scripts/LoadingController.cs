using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Random = UnityEngine.Random;

public class LoadingController : MonoBehaviour
{
    [Header("Main options:")]
    [SerializeField] private Image[] hearts;
    [SerializeField] private Color emptyColor = Color.white;
    [SerializeField] private Color fullColor = Color.red;
    [SerializeField] private float fillDuration = 4f;
    [SerializeField] private float delayBetweenHearts = 0.5f;
    
    [Header("Visual animation options:")]
    [SerializeField] private float pulseIntensity = 0.15f;
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private float floatAmplitude = 10f;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private bool randomFloatOffset = true;
    
    [Header("Canvas group:")]
    [SerializeField] private CanvasGroup mainMenuGroup;
    
    private Coroutine[] _pulseCoroutines;
    private Vector3[] _originalPositions;
    private Vector3[] _originalScales;
    private float[] _floatOffsets;
    private bool _isAnimating;

    public LoadingController(Coroutine[] pulseCoroutines)
    {
        _pulseCoroutines = pulseCoroutines;
    }

    private void Start()
    {
        InitializeHearts();
        StartAnimation();
    }

    private void InitializeHearts()
    {
        _pulseCoroutines = new Coroutine[hearts.Length];
        _originalPositions = new Vector3[hearts.Length];
        _originalScales = new Vector3[hearts.Length];
        _floatOffsets = new float[hearts.Length];
        
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
            {
                hearts[i].color = emptyColor;
                _originalPositions[i] = hearts[i].transform.localPosition;
                _originalScales[i] = hearts[i].transform.localScale;
                _floatOffsets[i] = randomFloatOffset ? Random.Range(0f, Mathf.PI * 2f) : 0f;
            }
        }
    }

    private void Update()
    {
        if (_isAnimating)
        {
            UpdateFloatAnimation();
        }
    }

    private void UpdateFloatAnimation()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
            {
                float floatValue = Mathf.Sin(Time.time * floatSpeed + _floatOffsets[i]) * floatAmplitude;
                Vector3 newPosition = _originalPositions[i];
                newPosition.y += floatValue;
                hearts[i].transform.localPosition = newPosition;
            }
        }
    }

    private void StartAnimation()
    {
        if (_isAnimating)
        {
            return;
        }
        
        _isAnimating = true;
        
        StopAllCoroutines();
        StartCoroutine(AnimateHeartsSequence(() =>
        {
            StartCoroutine(FadeInCanvasGroup(mainMenuGroup, 0.5f));
        }));
    }

    private IEnumerator AnimateHeartsSequence(Action onComplete = null)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
            {
                _pulseCoroutines[i] = StartCoroutine(PulseHeartContinuously(i));
                
                yield return StartCoroutine(FillHeartColor(i));
                
                if (i < hearts.Length - 1)
                {
                    yield return new WaitForSeconds(delayBetweenHearts);
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
        
        onComplete?.Invoke();
        _isAnimating = false;
    }
    
    private IEnumerator FadeInCanvasGroup(CanvasGroup group, float duration)
    {
        group.alpha = 0f;
        float elapsed = 0f;
    
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            group.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }
    
        group.alpha = 1f;
    }

    private IEnumerator FillHeartColor(int heartIndex)
    {
        Image heart = hearts[heartIndex];
        float elapsedTime = 0f;
        
        while (elapsedTime < fillDuration)
        {
            float progress = elapsedTime / fillDuration;
            heart.color = Color.Lerp(emptyColor, fullColor, progress);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        heart.color = fullColor;
    }

    private IEnumerator PulseHeartContinuously(int heartIndex)
    {
        Image heart = hearts[heartIndex];
        float pulseTimer = 0f;
        
        while (true)
        {
            float pulseValue = Mathf.Sin(pulseTimer * pulseSpeed) * pulseIntensity;
            float pulseScale = 1f + pulseValue;
            
            heart.transform.localScale = _originalScales[heartIndex] * pulseScale;
            
            pulseTimer += Time.deltaTime;
            yield return null;
        }
    }
}