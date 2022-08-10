using Cysharp.Threading.Tasks;
using UnityEngine;
using GeminiStateMachine;

/// <summary>  
/// Ingame scene controller
/// Scene Controller → State → ViewModel → View passes the data
/// In each view, the data is not updated, but only it is received as Props.
/// </summary>
public class InGameSceneController : MonoBehaviour
{
    #region Variables

    public enum StateEvents
    {
        OnInitialized,
        OnDemoStarted,
        OnDemoFinished,
        OnNextDemoStarted,
    }

    /// <summary>
    /// In -game state machine
    /// </summary>
    private GeminiStateMachine<InGameSceneController> stateMachine;

    #endregion

    #region Life Cycle
    private void Awake() => InitializeStateMachine();

    #endregion

    #region Unity Methods

    void Start() => stateMachine.Update();

    void Update() => stateMachine.Update();

    #endregion

    #region Class

    private void InitializeStateMachine()
    {
        /// <summary>
        /// First state is InitializationState
        /// Second state is ...StartState
        /// Third state is ...ResultState
        /// on each state, the state machine will go to the next state with event
        /// OnInitialized → OnQuestionStarted → OnQuestionFinished → OnNextQuestionStarted
        /// </summary>
        stateMachine = new GeminiStateMachine<InGameSceneController>(this);

        stateMachine.AddTransition<InitializationState, DemoInitializationState>((int)StateEvents.OnInitialized);
        stateMachine.AddTransition<DemoInitializationState, DemoStartState>((int)StateEvents.OnDemoStarted);
        stateMachine.AddTransition<DemoStartState, DemoResultState>((int)StateEvents.OnDemoFinished);
        stateMachine.AddTransition<DemoResultState, DemoInitializationState>((int)StateEvents.OnNextDemoStarted);

        stateMachine.SetStartState<InitializationState>();

    }

    #endregion

}