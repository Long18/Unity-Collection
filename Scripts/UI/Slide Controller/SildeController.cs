using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx; // Install package UniRx
using System;

[Serializable]
class ImageSpriteList
{
    public string name;
    public List<Sprite> imageSprite;
}

public class SlideController : MonoBehaviour
{

    [SerializeField] private Image _slideImage = null;
    [SerializeField] private ButtonHover _backBtn = null;
    [SerializeField] private ButtonHover _nextBtn = null;

    [Header("Image")]
    [SerializeField] private ImageSpriteList[] _slideSprites;

    [Header("dotPage")]
    [SerializeField] private PageDotController _dotPage;

    private int _slideTypeIndex = 0;
    private int _slideIndex = 0;

    private ISubject<Unit> _onFinished = new Subject<Unit>();
    public IObservable<Unit> OnFinished => _onFinished;

    private ISubject<Unit> _onBacked = new Subject<Unit>();
    public IObservable<Unit> OnBacked => _onBacked;

    bool hidden;

    #region Unity Methods

    void Start()
    {

        if (hidden == false)
        {
            hidden = true;
            gameObject.SetActive(false);
        }

        _backBtn.OnClickButton
            .ThrottleFirst(TimeSpan.FromSeconds(0.05f))
            .Subscribe(_ => Back())
            .AddTo(this);

        _nextBtn.OnClickButton
            .ThrottleFirst(TimeSpan.FromSeconds(0.05f))
            .Subscribe(_ => Next())
            .AddTo(this);
    }

    #endregion

    #region Class

    public void ShowSlides(int slideTypeIndex)
    {
        _slideTypeIndex = slideTypeIndex;
        _slideIndex = 0;
        _slideImage.sprite = GetSlides()[_slideIndex];
        gameObject.SetActive(true);

        //dotPage Initial setting
        _dotPage.PageDotInit(GetSlides().Count);

    }

    private void Next()
    {
        if (GetSlides().Count > _slideIndex) _slideIndex++;

        if (_slideIndex >= GetSlides().Count)
        {
            _onFinished.OnNext(Unit.Default);
        }
        else
        {
            _slideImage.sprite = GetSlides()[_slideIndex];

            //dotPage Display of
            _dotPage.SetPage(_slideIndex);
        }
    }

    private void Back()
    {
        _slideIndex--;
        if (_slideIndex < 0)
        {
            gameObject.SetActive(false);
            _onBacked.OnNext(Unit.Default);
            BackUI();
        }
        else
        {
            _slideImage.sprite = GetSlides()[_slideIndex];

            //dotPage Display of
            _dotPage.SetPage(_slideIndex);
        }
    }

    private List<Sprite> GetSlides()
    {
        return _slideSprites[_slideTypeIndex].imageSprite;
    }
    #endregion


}
