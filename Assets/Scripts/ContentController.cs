using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ContentController : MonoBehaviour, IPointerClickHandler
{
    [Header("Main objects:")]
    [SerializeField] private Image mainImage;
    [SerializeField] private Image premiumImage;

    private ImageGallery _imageGallery;
    private GameObject _popup;
    private bool _isPremium, _isReady;

    public Image GetMainImage()
    {
        return mainImage;
    }

    public void GetImageGallery(ImageGallery imageGallery)
    {
        _imageGallery = imageGallery;
    }

    public void ChangeIsReady(bool isReady)
    {
        _isReady = isReady;
    }

    public void ChangeContentStatus(bool active)
    {
        _isPremium = active;

        if (premiumImage != null)
        {
            premiumImage.enabled = active;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_isReady)
        {
            SoundController.Instance.PlayImageNotReady();
            return;
        }

        if (_isPremium)
        {
            _imageGallery.ShowPremiumPopup();
        }
        else
        {
            _imageGallery.ShowBasePopup(mainImage);
        }
        
        SoundController.Instance.PlayImageClick();
    }
}