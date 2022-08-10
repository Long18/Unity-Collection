using System;
using Cysharp.Threading.Tasks;
using GeminiStateMachine;
using UniRx;
using UnityEngine;

public class DemoResultState : GeminiStateMachine<InGameSceneController>.State
{

    #region Variables

    /// <summary>
    /// Stream deletion control game object
    /// </summary>
    private GameObject streamGameObject;

    #endregion

    #region Unity Methods
    protected internal override void Enter() => Initialize();

    protected internal override void Exit() => GameObject.Destroy(streamGameObject);

    #endregion

    #region Class

    private void Initialize() => streamGameObject = new GameObject("DemoInitializationState");

    #endregion
}
