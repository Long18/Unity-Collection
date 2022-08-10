using Random = UnityEngine.Random;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GeminiStateMachine;
using System.Linq;
using UnityEngine;

/// <summary>  
/// Brief summary of what the class does
/// </summary>
public class DemoInitializationState : GeminiStateMachine<InGameSceneController>.State
{

    #region Variables

    /// <summary>
    /// STREAM deletion control game object
    /// </summary>
    private GameObject streamGameObject;

    /// <summary>
    /// Processing performed at the time of state transition
    /// </summary>
    protected internal override void Enter() => InitializeAsync().Forget();

    #endregion

    #region Class

    /// <summary>
    /// Processing performed at the time of state transition
    /// </summary>
    protected internal override void Exit() => GameObject.Destroy(streamGameObject);


    private async UniTaskVoid InitializeAsync()
    {

        // Create an event
        stateMachine.SendEvent((int)InGameSceneController.StateEvents.OnDemoStarted);
    }


    #endregion
}
