using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Baccarat
{
    /// <summary>  
    /// This class is used to customize the layout of UI.
    /// </summary>
    public class FlexibleGridLayout : LayoutGroup
    {

        #region Variables

        public enum Alignment
        {
            Horizontal,
            Vertical
        }

        public enum FitType
        {
            Uniform,
            Width,
            Height,
            FixedRows,
            FixedColumns,
            FixedBoth
        }
        [Space]
        public Alignment alignment;
        [Space]
        public FitType fitType;
        [Space, Min(1)]
        public int rows, columns;
        [Space, Min(0)]
        public Vector2 spacing, cellSize;
        [Space]
        public bool fitX, fitY;
        public bool NudgeLastItemsOver;


        #endregion

        #region Unity Methods


        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            if (fitType == FitType.Width || fitType == FitType.Height || fitType == FitType.Uniform)
            {
                fitX = true;
                fitY = true;

                float sqrRt = Mathf.Sqrt(transform.childCount);
                rows = Mathf.CeilToInt(sqrRt);
                columns = Mathf.CeilToInt(sqrRt);
            }


            if (fitType == FitType.Width || fitType == FitType.FixedColumns) rows = Mathf.CeilToInt(transform.childCount / (float)columns);
            if (fitType == FitType.Height || fitType == FitType.FixedRows) columns = Mathf.CeilToInt(transform.childCount / (float)rows);

            float parentWidth = rectTransform.rect.width;
            float parentHeight = rectTransform.rect.height;

            float cellWidth = (parentWidth / (float)columns) - ((spacing.x / (float)columns) * ((float)columns - 1)) - (padding.left / (float)columns) - (padding.right / (float)columns);
            float cellHeight = parentHeight / (float)rows - ((spacing.y / (float)rows) * ((float)rows - 1)) - (padding.top / (float)rows) - (padding.bottom / (float)rows);

            cellSize.x = fitX ? cellWidth : cellSize.x;
            cellSize.y = fitY ? cellHeight : cellSize.y;

            int columnCount = 0;
            int rowCount = 0;

            for (int i = 0; i < rectChildren.Count; i++)
            {
                rowCount = i / columns;
                columnCount = i % columns;

                var item = rectChildren[i];

                var xPos = (cellSize.x * columnCount) + (spacing.x * columnCount) + padding.left;
                var yPos = (cellSize.y * rowCount) + (spacing.y * rowCount) + padding.top;

                SetChildAlongAxis(item, 0, xPos, cellSize.x);
                SetChildAlongAxis(item, 1, yPos, cellSize.y);
            }
        }

        public override void CalculateLayoutInputVertical()
        {
            base.CalculateLayoutInputHorizontal();
            float sqrRt;
            float cellWidth;
            float cellHeight;
            int columnCount = 0;
            int rowCount = 0;

            switch (fitType)
            {
                case FitType.Uniform:
                default:
                    fitX = fitY = true;
                    sqrRt = Mathf.Sqrt(transform.childCount);
                    rows = Mathf.CeilToInt(sqrRt);
                    columns = Mathf.CeilToInt(sqrRt);
                    rows = Mathf.CeilToInt(transform.childCount / (float)columns);
                    columns = Mathf.CeilToInt(transform.childCount / (float)rows);
                    break;
                case FitType.Width:
                    fitX = fitY = true;
                    sqrRt = Mathf.Sqrt(transform.childCount);
                    rows = Mathf.CeilToInt(sqrRt);
                    columns = Mathf.CeilToInt(sqrRt);
                    rows = Mathf.CeilToInt(transform.childCount / (float)columns);
                    break;
                case FitType.Height:
                    fitX = fitY = true;
                    sqrRt = Mathf.Sqrt(transform.childCount);
                    rows = Mathf.CeilToInt(sqrRt);
                    columns = Mathf.CeilToInt(sqrRt);
                    columns = Mathf.CeilToInt(transform.childCount / (float)rows);
                    break;
                case FitType.FixedRows:
                    // fitX = fitY = false;
                    columns = Mathf.CeilToInt(transform.childCount / (float)rows);
                    break;
                case FitType.FixedColumns:
                    // fitX = fitY = false;
                    rows = Mathf.CeilToInt(transform.childCount / (float)columns);
                    break;
                case FitType.FixedBoth:
                    // fitX = fitY = false;
                    break;
            }


            switch (alignment)
            {
                case Alignment.Horizontal:
                    cellWidth = (this.rectTransform.rect.width / (float)columns) - ((spacing.x / (float)columns) * (columns - 1)) - (padding.left / (float)columns) - (padding.right / (float)columns);
                    cellHeight = (this.rectTransform.rect.height / (float)rows) - ((spacing.y / (float)rows) * (rows - 1)) - (padding.top / (float)rows) - (padding.bottom / (float)rows);
                    break;
                case Alignment.Vertical:
                default:
                    cellHeight = (this.rectTransform.rect.width / (float)columns) - ((spacing.x / (float)columns) * (columns - 1)) - (padding.left / (float)columns) - (padding.right / (float)columns);
                    cellWidth = (this.rectTransform.rect.height / (float)rows) - ((spacing.y / (float)rows) * (rows - 1)) - (padding.top / (float)rows) - (padding.bottom / (float)rows);
                    break;
            }

            cellSize.x = fitX ? (cellWidth <= 0 ? cellSize.x : cellWidth) : cellSize.x;
            cellSize.y = fitY ? (cellHeight <= 0 ? cellSize.y : cellHeight) : cellSize.y;


            for (int i = 0; i < rectChildren.Count; i++)
            {
                var item = rectChildren[i];
                float xPos;
                float yPos;
                float xLastItemOffset = 0;

                switch (alignment)
                {
                    case Alignment.Horizontal:
                        rowCount = i / columns;
                        columnCount = i % columns;
                        if (NudgeLastItemsOver && rowCount == (rectChildren.Count / columns)) { xLastItemOffset = (cellSize.x + padding.left) / 2; }
                        break;
                    case Alignment.Vertical:
                    default:
                        rowCount = i / rows;
                        columnCount = i % rows;
                        if (NudgeLastItemsOver && rowCount == (rectChildren.Count / rows)) { xLastItemOffset = (cellSize.x + padding.left) / 2; }
                        break;
                }

                xPos = (cellSize.x * columnCount) + (spacing.x * columnCount) + padding.left + xLastItemOffset;
                yPos = (cellSize.y * rowCount) + (spacing.y * rowCount) + padding.top;

                switch (m_ChildAlignment)
                {
                    case TextAnchor.UpperLeft:
                    default:
                        //No need to change xPos;
                        //No need to change yPos;
                        break;
                    case TextAnchor.UpperCenter:
                        xPos += (0.5f * (this.gameObject.GetComponent<RectTransform>().sizeDelta.x + (spacing.x + padding.left + padding.left) - (columns * (cellSize.x + spacing.x + padding.left)))); //Center xPos
                        //No need to change yPos;
                        break;
                    case TextAnchor.UpperRight:
                        xPos = -xPos + this.gameObject.GetComponent<RectTransform>().sizeDelta.x - cellSize.x; //Flip xPos to go bottom-up
                        //No need to change yPos;
                        break;
                    case TextAnchor.MiddleLeft:
                        //No need to change xPos;
                        yPos += (0.5f * (this.gameObject.GetComponent<RectTransform>().sizeDelta.y + (spacing.y + padding.top + padding.top) - (rows * (cellSize.y + spacing.y + padding.top)))); //Center yPos
                        break;
                    case TextAnchor.MiddleCenter:
                        xPos += (0.5f * (this.gameObject.GetComponent<RectTransform>().sizeDelta.x + (spacing.x + padding.left + padding.left) - (columns * (cellSize.x + spacing.x + padding.left)))); //Center xPos
                        yPos += (0.5f * (this.gameObject.GetComponent<RectTransform>().sizeDelta.y + (spacing.y + padding.top + padding.top) - (rows * (cellSize.y + spacing.y + padding.top)))); //Center yPos
                        break;
                    case TextAnchor.MiddleRight:
                        xPos = -xPos + this.gameObject.GetComponent<RectTransform>().sizeDelta.x - cellSize.x; //Flip xPos to go bottom-up
                        yPos += (0.5f * (this.gameObject.GetComponent<RectTransform>().sizeDelta.y + (spacing.y + padding.top + padding.top) - (rows * (cellSize.y + spacing.y + padding.top)))); //Center yPos
                        break;
                    case TextAnchor.LowerLeft:
                        //No need to change xPos;
                        yPos = -yPos + this.gameObject.GetComponent<RectTransform>().sizeDelta.y - cellSize.y; //Flip yPos to go Right to Left
                        break;
                    case TextAnchor.LowerCenter:
                        xPos += (0.5f * (this.gameObject.GetComponent<RectTransform>().sizeDelta.x + (spacing.x + padding.left + padding.left) - (columns * (cellSize.x + spacing.x + padding.left)))); //Center xPos
                        yPos = -yPos + this.gameObject.GetComponent<RectTransform>().sizeDelta.y - cellSize.y; //Flip yPos to go Right to Left
                        break;
                    case TextAnchor.LowerRight:
                        xPos = -xPos + this.gameObject.GetComponent<RectTransform>().sizeDelta.x - cellSize.x; //Flip xPos to go bottom-up
                        yPos = -yPos + this.gameObject.GetComponent<RectTransform>().sizeDelta.y - cellSize.y; //Flip yPos to go Right to Left
                        break;
                }

                SetChildAlongAxis(item, 0, xPos, cellSize.x);
                SetChildAlongAxis(item, 1, yPos, cellSize.y);
            }


        }

        public override void SetLayoutHorizontal()
        {

        }

        public override void SetLayoutVertical()
        {

        }


        #endregion
    }
}