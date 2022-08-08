using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using UniRx;
using System;

/// <summary>  
/// This class is used to control the button behavior.
/// </summary>
public class ButtonHover : MonoBehaviour,
IPointerEnterHandler,
IPointerExitHandler,
    IPointerClickHandler,
    IPointerDownHandler,
    IPointerUpHandler
{

    #region Variables

    [Header("MainImage")]
    [SerializeField] private Image _buttonMain;
    [Header("AdditiveImage")]
    [SerializeField] private Image _buttonAdditive;
    [SerializeField] private Color _additiveColor;
    [Header("ButtonSprite")]
    [SerializeField] private Sprite _activeSprite;
    [SerializeField] private Sprite _deactiveSprite;
    [Header("Outline")]
    [SerializeField] private float _outlinePixel;
    [SerializeField] private Image _buttonOutline;
    [SerializeField] private Color _outlineColor;
    [Header("Sequence")]
    [SerializeField] private float _scale;
    [SerializeField] private float _duration;
    [SerializeField] private Ease _hoberEase;
    private Sequence _hover_seq;
    private Sequence _exit_seq;
    private Sequence _click_seq;
    private bool _isPointerDowned = false;

    private Subject<Unit> _subject = new Subject<Unit>();
    public IObservable<Unit> OnClickButton => _subject;

    private bool _isInteractable = true;
    public bool interactable
    {
        get { return _isInteractable; }
        set
        {
            _isInteractable = value;
            if (_activeSprite != null && _deactiveSprite != null)
            {
                _buttonMain.sprite = value ? _activeSprite : _deactiveSprite;
            }
        }
    }


    #endregion

    private void Awake()
    {
        ResetButton();
    }

    #region Class
    private void ResetButton()
    {
        _buttonOutline.rectTransform.sizeDelta = new Vector2(_buttonMain.rectTransform.sizeDelta.x + _outlinePixel * 2, _buttonMain.rectTransform.sizeDelta.y + _outlinePixel * 2);
        _buttonOutline.enabled = false;
        _buttonOutline.color = new Color(_outlineColor.r, _outlineColor.g, _outlineColor.b, 0f);
        _buttonAdditive.color = new Color(1f, 1f, 1f, 0f);
    }
    /// <summary>
    /// When holding the mouse
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!interactable) return;
        if (_hover_seq != null) _hover_seq.Kill();

        _buttonOutline.enabled = true;

        if (_isPointerDowned)
        {
            _buttonAdditive.color = _additiveColor;
            _hover_seq = DOTween.Sequence();
            _hover_seq.Append(_buttonOutline.rectTransform.DOScale(Vector2.one * _scale, _duration).SetEase(_hoberEase));
        }
        else
        {
            _hover_seq = DOTween.Sequence();
            _hover_seq.SetLink(gameObject);
            _hover_seq
                .Append(_buttonOutline.rectTransform.DOScale(Vector2.one * _scale, _duration).SetEase(_hoberEase))
                .Join(_buttonOutline.DOFade(1f, _duration).SetEase(_hoberEase))
                .Join(_buttonAdditive.DOFade(0f, _duration).SetEase(_hoberEase));
        }
    }

    /// <summary>
    /// When the mouse you are holding away
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!interactable) return;
        _buttonOutline.enabled = false;

        _exit_seq?.Kill();

        if (_isPointerDowned)
        {
            _exit_seq = DOTween.Sequence();
            _exit_seq.SetLink(gameObject);
            _exit_seq
                .Append(_buttonOutline.rectTransform.DOScale(Vector2.one, _duration).SetEase(_hoberEase))
                .Join(_buttonOutline.DOFade(1f, _duration).SetEase(_hoberEase))
                .Join(_buttonAdditive.DOFade(0f, _duration).SetEase(_hoberEase));
        }
        else
        {
            _exit_seq = DOTween.Sequence();
            _exit_seq.SetLink(gameObject);
            _exit_seq
                .Append(_buttonOutline.rectTransform.DOScale(Vector2.one, _duration).SetEase(_hoberEase))
                .Join(_buttonOutline.DOFade(0f, _duration).SetEase(_hoberEase));
        }
    }

    /// <summary>
    /// When clicked
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!interactable) return;

        _subject.OnNext(Unit.Default);

        if (gameObject.activeInHierarchy)
        {
            OnPointerEnter(eventData);
        }
        else
        {
            ResetButton();
        }
    }

    /// <summary>
    /// When pressing the mouse
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!interactable) return;
        _isPointerDowned = true;
        ClickAnimation();
    }

    /// <summary>
    /// When the mouse is released
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!interactable) return;
        _isPointerDowned = false;
    }

    private void ClickAnimation()
    {
        if (!interactable) return;
        _buttonAdditive.color = _additiveColor;
        if (_click_seq != null) _click_seq.Kill();
        _click_seq = DOTween.Sequence();
        _click_seq.Append(_buttonOutline.rectTransform.DOScale(Vector2.one, _duration).SetEase(_hoberEase));
    }

    public Button GetButton()
    {
        return this._buttonMain.GetComponent<Button>();
    }
    #endregion
}
