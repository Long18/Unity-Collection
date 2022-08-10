using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using GeminiStateMachine;
using UniRx;
using UniRx.Triggers;
using UnityEngine;


public class DemoStartState : GeminiStateMachine<InGameSceneController>.State
{

    #region Variables

    /// <summary>
    /// STREAM deletion control game object
    /// </summary>
    private GameObject streamGameObject;

    /// <summary>
    /// Processing performed at the time of state transition
    /// </summary>
    protected internal override void Enter()
    {
        InitializeStreams();
        InitializeAsync().Forget();
    }

    /// <summary>
    /// Processing performed at the time of state transition
    /// </summary>
    protected internal override void Exit() => GameObject.Destroy(streamGameObject);

    #endregion

    #region Class
    /// <summary>
    /// Initialization of Stream
    /// </summary>
    private void InitializeStreams() => streamGameObject = new GameObject("QuestionStartState");

    private async UniTask InitializeAsync() => await UniTask.Delay(TimeSpan.FromSeconds(5f));

    #endregion
}
