using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PageDotController : MonoBehaviour
{

    #region Variables

    [SerializeField] private Image _dot;
    [SerializeField] private Sprite _onSprite;
    [SerializeField] private Sprite _offSprite;
    [SerializeField] private RectTransform _dotSpawn;
    [SerializeField] private List<Image> _pages;
    [SerializeField] private float _dotWidth = 30f;

    #endregion

    #region Class
    /// <summary>
    /// Produced a point to match the number of pages, basically the most colored state
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="startPage"></param>
    public void PageDotInit(int pageNumber, int startPage = 0)
    {
        foreach (var nDot in _pages)
        {
            Destroy(nDot.gameObject);
        }
        _pages.Clear();

        var spawnWidth = pageNumber * _dotWidth;
        _dotSpawn.sizeDelta = new Vector2(spawnWidth, 100f);
        for (var i = 0; i < pageNumber; i++)
        {
            var nDot = Instantiate(_dot, _dotSpawn);
            nDot.gameObject.SetActive(true);
            nDot.name = "page_" + i;
            _pages.Add(nDot);
            nDot.sprite = _offSprite;
            if (i == startPage)
            {
                nDot.sprite = _onSprite;
            }
        }
    }

    /// <summary>
    /// Only the specified page INDEX is colored
    /// </summary>
    /// <param name="setPage"></param>
    public void SetPage(int setPage)
    {
        foreach (Image page in _pages)
        {
            page.sprite = _offSprite;
        }
        _pages[setPage].sprite = _onSprite;

    }
    #endregion
}
