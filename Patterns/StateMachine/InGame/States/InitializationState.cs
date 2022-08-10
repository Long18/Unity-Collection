using System.Linq;
using GeminiStateMachine;
using UnityEngine;

/// <summary>  
/// Brief summary of what the class does
/// </summary>
public class InitializationState : GeminiStateMachine<InGameSceneController>.State
{

    #region Variables

    /// <summary>
    /// Stream deletion control game object
    /// </summary>
    private GameObject streamGameObject;

    #endregion

    #region Unity Methods

    // Start is called before the first frame update
    protected internal override void Enter()
    {
        streamGameObject = new GameObject("DemoStartState");
        InitializeDemo();
    }

    /// <summary>
    /// Processing performed at the time of state transition
    /// </summary>
    protected internal override void Exit() => GameObject.Destroy(streamGameObject);

    #endregion

    #region Class
    private void InitializeDemo()
    {
        // ...
    }


    #endregion
}
