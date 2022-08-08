using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>  
/// This class is used to scale the UI camera.
/// </summary>
[ExecuteInEditMode, RequireComponent(typeof(Camera))]
public class UICameraScaler : MonoBehaviour
{

    #region Variables

    public Camera targetCamera;
    public Vector2Int screenReferenceSize = new Vector2Int(1920, 1080);
    public float baseCameraSize = 8;


    #endregion

    #region Unity Methods

    private void Reset()
    {
        this.targetCamera = this.GetComponent<Camera>();
    }

    private IEnumerator Start()
    {
        yield return true;
        Update();
    }

    void Update()
    {
        if (this.screenReferenceSize.x <= 0 || this.screenReferenceSize.y <= 0)
            return;

        var screen_ratio = Screen.width * 1.0f / Screen.height;
        var ratio = this.screenReferenceSize.x * 1.0f / this.screenReferenceSize.y;

        if (ratio < screen_ratio)
            this.targetCamera.orthographicSize = this.baseCameraSize;
        else
            this.targetCamera.orthographicSize = this.baseCameraSize * ratio / screen_ratio;
    }
    #endregion

}
