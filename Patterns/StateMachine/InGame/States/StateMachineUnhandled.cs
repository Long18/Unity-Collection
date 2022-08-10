using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
#if ENABLE_IL2CPP
using UnityEngine.Scripting;
#endif

namespace GeminiStateMachine
{

    /// <summary>  
    /// It is a enumerated type that expresses how to behave in unprocessed exceptions that occur during the update of the state machine.
    /// </summary>
    public enum StateMachineUnhandledExceptionMode
    {
        /// <summary>
        /// Exceptions generated in the Update function are generated as an exception.
        /// </summary>
        ThrowException,

        /// <summary>
        /// It will be transferred to the OnUnhandledException handlage.
        /// </summary>
        CatchException,

        /// <summary>
        /// Exceptions will be transferred to Error (), which is currently running.
        /// However, if there is no state in operation, it will be the same behavior as ThrowException.
        /// </summary>
        CatchStateException,
    }

    #region Standard state machine basal implementation
    /// <summary>
    /// It is a state machine class that can have a context
    /// </summary>
    /// <typeparam name="TContext">The type of context of this state machine</typeparam>
    /// <typeparam name="TEvent">Event type to send to a state machine</typeparam>
    public class StateMachine<TContext, TEvent>
    {
        #region Definition of the stat

        /// <summary>
        /// It is a state class that expresses the state where the state machine is processed.
        /// </summary>
        public abstract class State
        {
            // Menba variable definition
            internal Dictionary<TEvent, State> transitionTable;
            internal StateMachine<TContext, TEvent> stateMachine;

            /// <summary>
            /// State machine to which this state belongs
            /// </summary>
            protected StateMachine<TContext, TEvent> StateMachine => stateMachine;


            /// <summary>
            /// Context of the state machine to which this state belongs
            /// </summary>
            protected TContext Context => stateMachine.Context;



            /// <summary>
            /// Perform the process when entering the state
            /// </summary>
            protected internal virtual void Enter()
            {
            }


            /// <summary>
            /// Perform the process of updating the state
            /// </summary>
            protected internal virtual void Update()
            {
            }


            /// <summary>
            /// Process when escaping from the state
            /// </summary>
            protected internal virtual void Exit()
            {
            }


            /// <summary>
            /// Processing when an unprocessed exception of a state machine occurs.
            /// However, UnhandledExceptionMode must be CatchstateException.
            /// </summary>
            /// <remarks>
            /// If this function returns False, the exception is eventually determined to be unprocessed and the state machine
            /// The Update () function will send exceptions.
            /// </remarks>
            /// <param name="exception">Exception of unprocessed that occurred</param>
            /// <returns>If the exception is processed, return True, and if it is not processed, return False.</returns>
            protected internal virtual bool Error(Exception exception)
            {
                //Usually return as unprocessed
                return false;
            }


            /// <summary>
            /// This state guards the event when the state machine receives an event
            /// </summary>
            /// <param name="eventId">Event ID passed</param>
            /// <returns>Returns True when guarding the event reception, and if you accept the event without guarding, returns.</returns>
            protected internal virtual bool GuardEvent(TEvent eventId)
            {
                // Normally not guard
                return false;
            }


            /// <summary>
            /// Before the state machine pops the stacked state, this state guards the pop
            /// </summary>
            /// <returns>Returns True to guard the pop operation, and returns the pop operation without guarding.</returns>
            protected internal virtual bool GuardPop()
            {
                // Normally not guard
                return false;
            }
        }

        /// <summary>
        /// It is a special state class that expresses "optional" in a state machine
        /// </summary>
#if ENABLE_IL2CPP
        [Preserve]
#endif
        public sealed class AnyState : State { }
        #endregion

        #region Enumerable definition
        /// <summary>
        /// Express the update state of the state machine
        /// </summary>
        private enum UpdateState
        {
            /// <summary>
            /// I am idling.In other words, I'm not doing anything
            /// </summary>
            Idle,

            /// <summary>
            /// The state is in the process of rushing in the state
            /// </summary>
            Enter,

            /// <summary>
            /// State update processing
            /// </summary>
            Update,

            /// <summary>
            /// The state is being escaped
            /// </summary>
            Exit,
        }
        #endregion

        // Menba variable definition
        private UpdateState updateState;
        private List<State> stateList;
        private State currentState;
        private State nextState;
        private Stack<State> stateStack;
        private HashSet<Func<Type, State>> stateFactorySet;

        /// <summary>
        /// This is an event handler when an unprocessed exception occurs in the update () of the state machine.
        /// However, the CatchException must be set in the UnhandledExceptionMode property.
        /// When False is returned, the exception is determined to be unprocessed, and the update () function sends an exception.
        /// </summary>
        public event Func<Exception, bool> UnhandledException;



        /// <summary>
        /// Context held by the state machine
        /// </summary>
        public TContext Context { get; private set; }


        /// <summary>
        /// Whether the state machine is running
        /// </summary>
        public bool Running => currentState != null;


        /// <summary>
        /// Whether the state machine is under updating.
        /// Even if this property indicates True even if you think you have escaped from the update function
        /// It may be possible to end an illegal end with an exception during update.
        /// </summary>
        public bool Updating => (Running && updateState != UpdateState.Idle);


        /// <summary>
        /// Number of stacks that are currently stacked
        /// </summary>
        public int StackCount => stateStack.Count;


        /// <summary>
        /// Get the name of the current state.
        /// If the state machine is not running yet, it will be an empty string.
        /// </summary>
        public string CurrentStateName => (Running ? currentState.GetType().Name : string.Empty);


        /// <summary>
        /// Whether to re -transition by SendEvent () after the transition () function once becomes a transition state.
        /// </summary>
        public bool AllowRetransition { get; set; }


        /// <summary>
        /// Get the setting of behavior when an unprocessed exception occurs
        /// </summary>
        public StateMachineUnhandledExceptionMode UnhandledExceptionMode { get; set; }


        /// <summary>
        /// The thread ID that finally updated this state machine
        /// </summary>
        public int LastUpdateThreadId { get; private set; }


        /// <summary>
        /// The last event ID accepted by this state machine
        /// </summary>
        public TEvent LastAcceptedEventID { get; private set; }

        /// <summary>
        /// Initialize the Statemachine instance instance
        /// </summary>
        /// <param name="context">Context of this state machine</param>
        /// <exception cref="ArgumentNullException">Context is null</exception>
        /// <exception cref="InvalidOperationException">Failed to generate the instance of the state class</exception>
        public StateMachine(TContext context)
        {
            // 渡されたコンテキストがnullなら
            if (context == null)
            {
                // nullは許されない
                throw new ArgumentNullException(nameof(context));
            }


            // Initialize the member
            Context = context;
            stateList = new List<State>();
            stateStack = new Stack<State>();
            updateState = UpdateState.Idle;
            AllowRetransition = false;
            UnhandledExceptionMode = StateMachineUnhandledExceptionMode.ThrowException;
            stateFactorySet = new HashSet<Func<Type, State>>();
        }

        #region General -purpose logic system
        /// <summary>
        /// Register a factory function that generates state instance from the type
        /// </summary>
        /// <param name="stateFactory">Registered factory function</param>
        /// <exception cref="ArgumentNullException">StateFactory is NULL</exception>
        public void RegisterStateFactory(Func<Type, State> stateFactory)
        {
            //Register in the hash set
            stateFactorySet.Add(stateFactory ?? throw new ArgumentNullException(nameof(stateFactory)));
        }


        /// <summary>
        ///Release the registered factory function
        /// </summary>
        /// <param name="stateFactory">Factory function to be released</param>
        /// <exception cref="ArgumentNullException">StateFactory is NULL</exception>
        public void UnregisterStateFactory(Func<Type, State> stateFactory)
        {
            // Cancel registration from the hash set
            stateFactorySet.Remove(stateFactory ?? throw new ArgumentNullException(nameof(stateFactory)));
        }
        #endregion


        #region State transition table construction system
        /// <summary>
        /// Add a voluntary transition structure of the state.
        /// </summary>
        /// <remarks>
        /// Use this function if the transition source wants a transition from an arbitrary state.
        /// Please note that any transition is less prioritized than normal transitions (transitions other than any of Any).
        /// In addition, the transition table setting of the state must be completed before the state machine starts.
        /// </remarks>
        /// <typeparam name="TNextState">State type that transitions from arbitrary state</typeparam>
        /// <param name="eventId">Event ID that is the condition to transition</param>
        /// <exception cref="ArgumentException"> There is a transition destination state where the same Eventid is already set</exception>
        /// <exception cref="InvalidOperationException">The state machine is already running</exception>
        public void AddAnyTransition<TNextState>(TEvent eventId) where TNextState : State, new()
        {
            // Calls a simple transition function that simply transitions are Anystate
            AddTransition<AnyState, TNextState>(eventId);
        }


        /// <summary>
        /// Add a state transition structure.
        /// Also, the transition table setting of the state must be completed before the state machine starts.
        /// </summary>
        /// <typeparam name="TPrevState">The type of the state that becomes the source of the transition</typeparam>
        /// <typeparam name="TNextState">State type of state to transition</typeparam>
        /// <param name="eventId">Event ID that is the condition to transition</param>
        /// <exception cref="ArgumentException">There is already a transition destination state with the same Eventid.</exception>
        /// <exception cref="InvalidOperationException">The state machine is already running</exception>
        /// <exception cref="InvalidOperationException">Failed to generate the instance of the state class</exception>
        public void AddTransition<TPrevState, TNextState>(TEvent eventId) where TPrevState : State, new() where TNextState : State, new()
        {
            // If the state machine is running
            if (Running)
            {
                // I can't set it anymore so I spit out the exception
                throw new InvalidOperationException("ステートマシンは、既に起動中です");
            }


            // Get the state instance of the transition source and the transition destination
            var prevState = GetOrCreateState<TPrevState>();
            var nextState = GetOrCreateState<TNextState>();


            // If the same event ID already exists on the transition table of the transitional state
            if (prevState.transitionTable.ContainsKey(eventId))
            {
                // I do not allow overwrite registration, so I exhale exception
                UnityEngine.Debug.LogError(($"State '{prevState.GetType().Name}In the event ID '{eventId}'The transition has already been set"));
                throw new ArgumentException($"State '{prevState.GetType().Name}In the event ID '{eventId}'The transition has already been set");
            }


            // Set the transition to the transition table
            prevState.transitionTable[eventId] = nextState;
        }


        /// <summary>
        /// Set the first state to start when the state machine starts.
        /// </summary>
        /// <typeparam name="TStartState">State machine starts when starting up</typeparam>
        /// <exception cref="InvalidOperationException">The state machine is already running</exception>
        /// <exception cref="InvalidOperationException">Failed to generate the instance of the state class</exception>
        public void SetStartState<TStartState>() where TStartState : State, new()
        {
            // If the state machine has already started
            if (Running)
            {
                // If it starts, the operation of this function is not allowed
                throw new InvalidOperationException("The state machine is already running");
            }


            // Set the next state to process
            nextState = GetOrCreateState<TStartState>();
        }
        #endregion


        #region State Stack operation system
        /// <summary>
        /// Push the state that is currently running to the state stack
        /// </summary>
        /// <exception cref="InvalidOperationException">The state machine has not started yet</exception>
        public void PushState()
        {
            // If there is no state currently running in the first place, throw an exception
            IfNotRunningThrowException();


            // Stack the current state on the stack
            stateStack.Push(currentState);
        }


        /// <summary>
        /// Take out the state of the state stack and prepare for the transition.
        /// </summary>
        /// <remarks>
        /// The behavior of this function is very similar to the SendEvent function, except for the event ID.
        /// If SendEvents are already ready for the next transition, the stack will not be popped from the stack.
        /// </remarks>
        /// <returns>If the stack is popped from the stack and the next transition is ready, True will be returned, and if the pop is guarded by the state, False is returned.</returns>
        /// <exception cref="InvalidOperationException">The state machine has not started yet</exception>
        public virtual bool PopState()
        {
            // If there is no state currently running in the first place, throw an exception
            IfNotRunningThrowException();


            // In the first place, the stack is empty, the next transitional state exists, the re -transition is not permission, or if it is guarded to the current state before popping
            if (stateStack.Count == 0 || (nextState != null && !AllowRetransition) || currentState.GuardPop())
            {
                // I can't pop it, so return False
                return false;
            }


            // Take out the stack from the stack and return to the next state to return the success
            nextState = stateStack.Pop();
            return true;
        }


        /// <summary>
        /// Take out the state of the state stack and set it directly as a current state.
        /// </summary>
        /// <remarks>
        /// The behavior of this function, unlike the Popstate () function, is set immediately as the pop -up state is immediately set as a state currently being processed.
        /// Transition processing in the state is not performed, and the pop -up state () is not called, and Update () will be called next time.
        /// </remarks>
        /// <returns>If the stack is popped from the stack and it can be set as the current state, TRUE is available, and if the pop is guarded, False is returned.</returns>
        /// <exception cref="InvalidOperationException">The state machine has not started yet</exception>
        public virtual bool PopAndDirectSetState()
        {
            // If there is no state currently running in the first place, throw an exception
            IfNotRunningThrowException();


            // If the stack is empty in the first place, or if it was guarded to the current state before popping
            if (stateStack.Count == 0 || currentState.GuardPop())
            {
                // I can't pop it, so return False
                return false;
            }


            // Take out the state from the stack and set it as the current state to return the success
            currentState = stateStack.Pop();
            return true;
        }


        /// <summary>
        /// Take out one state on the state stack and throw it away.
        /// </summary>
        /// <remarks>
        /// Use it when you want to throw away the stacked state at the top of the state stack.
        /// </remarks>
        public void PopAndDropState()
        {
            // If the stack is empty
            if (stateStack.Count == 0)
            {
                // End without doing anything
                return;
            }


            // Take out the state from the stack and throw it away without doing anything
            stateStack.Pop();
        }


        /// <summary>
        /// Throw off all the stacks on the state stack.
        /// </summary>
        public void ClearStack()
        {
            // Empty the stack
            stateStack.Clear();
        }
        #endregion


        #region ステートマシン制御系
        /// <summary>
        /// Investigate whether the currently running state is a specified state.
        /// </summary>
        /// <typeparam name="TState">Confirmation type type</typeparam>
        /// <returns>If the specified state is in the state of the specified state, return true, if different, False is returned.</returns>
        /// <exception cref="InvalidOperationException">The state machine has not started yet</exception>
        public bool IsCurrentState<TState>() where TState : State
        {
            // If there is no state currently running in the first place, throw an exception
            IfNotRunningThrowException();


            // Return the result of the conditional formula for whether the current state and the type match
            return currentState.GetType() == typeof(TState);
        }


        /// <summary>
        /// Send an event to the state machine and prepare for the state transition.
        /// </summary>
        /// <remarks>
        /// The transition of the state is not performed immediately, and the transition processing is performed when the next update is executed.
        /// In addition, the event reception priority by this function is only the first event that accepts the transition, and all subsequent events will fail until the transition is transitioned by Update.
        /// However, if True is set in the AllowRetransition property, the re -transition is allowed.
        /// Furthermore, the event can be accepted during the state of the state or update, and during the update of the state machine
        /// It is possible to transition many times, but if you send an event during EXIT, the exception will be sent because it will be on the transition.
        /// </remarks>
        /// <param name="eventId">Event ID to be sent to the state machine</param>
        /// <returns>If the state machine is accepted, rejects the event, or returns False if the event cannot be accepted.</returns>
        /// <exception cref="InvalidOperationException">The state machine has not started yet</exception>
        /// <exception cref="InvalidOperationException">Events cannot be accepted because the state is under EXIT processing</exception>
        public virtual bool SendEvent(TEvent eventId)
        {
            //If there is no state currently running in the first place, throw an exception
            IfNotRunningThrowException();


            // If EXIT processing
            if (updateState == UpdateState.Exit)
            {
                // Sendevent in EXIT is not allowed
                throw new InvalidOperationException("Events cannot be accepted because the state is under EXIT processing");
            }


            // If you are already preparing for the transition and is not allowed to transition
            if (nextState != null && !AllowRetransition)
            {
                // Returns that the event could not be accepted
                return false;
            }


            // If you call an event guard to the current state and get guarded
            if (currentState.GuardEvent(eventId))
            {
                //Returns that you have failed and failed
                return false;
            }


            // If you take out the next transitioned state from the current state, if you can't find it
            if (!currentState.transitionTable.TryGetValue(eventId, out nextState))
            {
                // If you couldn't even transition from any state
                if (!GetOrCreateState<AnyState>().transitionTable.TryGetValue(eventId, out nextState))
                {
                    // I couldn't accept the event
                    return false;
                }
            }


            // Return the event ID that you accepted at the end and return the event reception
            LastAcceptedEventID = eventId;
            return true;
        }


        /// <summary>
        /// Update the state of the state machine.
        /// </summary>
        /// <remarks>
        /// Update the state that is currently processing the state machine, but if it is not yet, the set set by the setstartState function will start.
        /// In addition, if the state machine is started for the first time, the update of the state is not called and the next update process is executed.
        /// </remarks>
        /// <exception cref="InvalidOperationException">The current state machine is performing renewal processing by another thread.[UpdaterThread={LastUpdateThreadId}, CurrentThread={currentThreadId}]</exception>
        /// <exception cref="InvalidOperationException">The current state machine has already been updated</exception>
        /// <exception cref="InvalidOperationException">Since the start state is not set, the state machine cannot be started.</exception>
        public virtual void Update()
        {
            // If the state of the state machine is not idling
            if (updateState != UpdateState.Idle)
            {
                // If a multiple Update by Update from another thread
                int currentThreadId = Thread.CurrentThread.ManagedThreadId;
                if (LastUpdateThreadId != currentThreadId)
                {
                    // Exporting in exception that it is a multiple update from another thread
                    throw new InvalidOperationException($"The current state machine is performing renewal processing by another thread.[UpdaterThread={LastUpdateThreadId}, CurrentThread={currentThreadId}]");
                }


                // Multivorized and can't call an exception
                throw new InvalidOperationException("The current state machine has already been updated");
            }


            // Learn the update start thread ID
            LastUpdateThreadId = Thread.CurrentThread.ManagedThreadId;


            // If it has not yet moved
            if (!Running)
            {
                // If the next state to be processed (that is, the startup start state) has not been set
                if (nextState == null)
                {
                    //Exhale exceptions that cannot be started
                    throw new InvalidOperationException("Since the start state is not set, the state machine cannot be started.");
                }


                // Set as a state under processing
                currentState = nextState;
                nextState = null;


                try
                {
                    //Set Enter and call Enter
                    updateState = UpdateState.Enter;
                    currentState.Enter();
                }
                catch (Exception exception)
                {
                    // Return at startup is bad if NULL is not included in the current state, so return it to the state before the transition.
                    nextState = currentState;
                    currentState = null;

                    // Update is idled, and error handling in the event of an exception is completed.
                    updateState = UpdateState.Idle;
                    DoHandleException(exception);
                    return;
                }


                // If there is no next state to transition
                if (nextState == null)
                {
                    // 起動処理は終わったので一旦終わる
                    updateState = UpdateState.Idle;
                    return;
                }
            }


            try
            {
                // If there is no next state to transition
                if (nextState == null)
                {
                    // Set up the Update process and call Update
                    updateState = UpdateState.Update;
                    currentState.Update();
                }


                // Loop while there is a state to transition next
                while (nextState != null)
                {
                    // Set up that it is under EXIT and call EXIT processing
                    updateState = UpdateState.Exit;
                    currentState.Exit();


                    // Switch to the next state
                    currentState = nextState;
                    nextState = null;


                    // Set Enter and call Enter
                    updateState = UpdateState.Enter;
                    currentState.Enter();
                }


                // Return to idling after update processing
                updateState = UpdateState.Idle;
            }
            catch (Exception exception)
            {
                // Update state is idled, and error handling in the event of an exception is completed.
                updateState = UpdateState.Idle;
                DoHandleException(exception);
                return;
            }
        }
        #endregion


        #region Internal logic
        /// <summary>
        /// Handling the unprocessed exception
        /// </summary>
        /// <param name="exception">Exception of unprocessed that occurred</param>
        /// <exception cref="ArgumentNullException">exception is null</exception>
        private void DoHandleException(Exception exception)
        {
            //If NULL is given
            if (exception == null)
            {
                // 何をハンドリングすればよいのか
                throw new ArgumentNullException(nameof(exception));
            }


            //If you have a mode that picks up exceptions and a handler is set
            if (UnhandledExceptionMode == StateMachineUnhandledExceptionMode.CatchException && UnhandledException != null)
            {
                //If you call the event and handle it correctly
                if (UnhandledException(exception))
                {
                    // As it is
                    return;
                }
            }


            // If you pick up the exception and leave it to the state, and if the current execution state is set
            if (UnhandledExceptionMode == StateMachineUnhandledExceptionMode.CatchStateException && currentState != null)
            {
                // If you throw an exception on the state and handle it correctly
                if (currentState.Error(exception))
                {
                    //Finish as it is
                    return;
                }
            }


            // Other than the above mode (that is, ThrowException), or if the exception is not handled (returned), the exception is captured and generated.
            ExceptionDispatchInfo.Capture(exception).Throw();
        }


        /// <summary>
        /// Exceptions will be sent if the state machine is not launched
        /// </summary>
        /// <exception cref="InvalidOperationException">The state machine has not started yet</exception>
        protected void IfNotRunningThrowException()
        {
            // If there is no state currently running in the first place
            if (!Running)
            {
                // I haven't even started it yet so I spit out the exception
                throw new InvalidOperationException("The state machine has not started yet");
            }
        }


        /// <summary>
        ///Acquires an instance of the specified state type, but if it does not exist, it will be generated and obtained.
        /// The generated instance will be obtained from the next time.
        /// </summary>
        /// <typeparam name="TState">The type of the state to acquire or generate</typeparam>
        /// <returns>Returns the instance of the acquired or generated state</returns>
        /// <exception cref="InvalidOperationException">Failed to generate the instance of the state class</exception>
        private TState GetOrCreateState<TState>() where TState : State, new()
        {
            //A few minutes of the state
            var stateType = typeof(TState);
            foreach (var state in stateList)
            {
                // If you have an instance that matches the type of the relevant state
                if (state.GetType() == stateType)
                {
                    // Return that instance
                    return (TState)state;
                }
            }


            // If you get out of the loop, there is no matching instance, so generate and cache an instance.
            var newState = CreateStateInstanceCore<TState>() ?? throw new InvalidOperationException("Failed to generate the instance of the state class");
            stateList.Add(newState);


            // Return to the new state, the initialization of your own reference and the instance of the transition table
            newState.stateMachine = this;
            newState.transitionTable = new Dictionary<TEvent, State>();
            return newState;
        }


        /// <summary>
        /// Generate instances of the specified state type.
        /// </summary>
        /// <typeparam name="TState">Type of status to be generated</typeparam>
        /// <returns>Returns the generated instance</returns>
        private TState CreateStateInstanceCore<TState>() where TState : State, new()
        {
            //Declare variables to receive results
            TState result;


            //Turn around the registered factory function
            var stateType = typeof(TState);
            foreach (var factory in stateFactorySet)
            {
                //If the instance was generated by trying to generate
                result = (TState)factory(stateType);
                if (result != null)
                {
                    // Return this instance
                    return result;
                }
            }


            // If you can't even use a factory function, rely on the implementation side generation function
            return CreateStateInstance<TState>();
        }


        /// <summary>
        /// Generate instances of the specified state type.
        /// </summary>
        /// <typeparam name="TState">Type of status to be generated</typeparam>
        /// <returns>Returns the generated instance</returns>
        protected virtual TState CreateStateInstance<TState>() where TState : State, new()
        {
            //Returned the default operation only by doing generic New
            return new TState();
        }
        #endregion
    }
    #endregion



    #region Old int event -type state machine implementation
    /// <summary>
    /// It is a state machine class that can have a context
    /// </summary>
    /// <typeparam name="TContext">The type of context of this state machine</typeparam>
    public class GeminiStateMachine<TContext> : StateMachine<TContext, int>
    {
        /// <summary>
        /// Initialize the Statemachine instance
        /// </summary>
        /// <param name="context">Context of this state machine</param>
        /// <exception cref="ArgumentNullException">context is null</exception>
        public GeminiStateMachine(TContext context) : base(context)
        {
        }
    }
    #endregion
}