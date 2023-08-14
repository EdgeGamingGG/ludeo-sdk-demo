using LudeoSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class LudeoFacade
{
    public bool IsInitialized { get; private set; } = false;
    public LudeoGameplayType GameplayType => LudeoManager.GetLudeoStateInfo().gameplayType;
    internal LudeoFlowState CurrentFlowState => LudeoManager.GetLudeoStateInfo().flowState;
    public CreatorFlow Creator { get; private set; }
    public PlayerFlow Player { get; private set; }

    public LudeoFacade()
    {
        LudeoHelpers.SetLogLevel(LudeoLogLevel.Error);
    }

    public async Task InitializeWithSteam(string userId, string apiKey)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentException("The steam user id cannot be null or empty");
        if (string.IsNullOrEmpty(apiKey))
            throw new ArgumentException("The api key cannot be null or empty");
        if (userId.Length != 17)
            throw new ArgumentException("The steam user id must be exactly 17 characters");

        LudeoManager.Init(userId, LudeoLauncher.Steam, apiKey, OnLudeoFlowState);

        await AwaitState(LudeoFlowState.WaitingForLoadGameplayData);

        IsInitialized = true;
        LudeoHelpers.SetLogLevel(LudeoLogLevel.Error);

        Creator = new CreatorFlow(this);
        Player = new PlayerFlow(this);
    }

    public void SetGameplayVariable<T>(string key, T value)
    {
        switch (value)
        {
            case bool b:
                LudeoManager.SetGameplayState(key, b);
                break;
            case int i:
                LudeoManager.SetGameplayState(key, i);
                break;
            case float f:
                LudeoManager.SetGameplayState(key, f);
                break;
            case Quaternion q:
                LudeoManager.SetGameplayState(key, new Quatern(q.x, q.y, q.z, q.w));
                break;
            case Vector3 v:
                LudeoManager.SetGameplayState(key, new Vec3(v.x, v.y, v.z));
                break;
            default:
                throw new ArgumentException($"The type {typeof(T).Name} is not supported by Ludeo.");
        }
    }

    private void OnLudeoFlowState(LudeoFlowState ludeoFlowState, object data)
    {
        Debug.Log("<color=red>" + ludeoFlowState.ToString() + "</color>");
        LudeoMessage? msg = null;

        switch (ludeoFlowState)
        {
            case LudeoFlowState.Initialization:
                break;
            case LudeoFlowState.NewLudeoSelected:
                if (!IsInitialized)
                {
                    // TODO: Play ludeo automatically
                }
                break;
            case LudeoFlowState.GameplayAborted:
                Player.IsRunningLudeo = false;
                break;
        }
    }

    private async Task AwaitState(LudeoFlowState state)
    {
        while (CurrentFlowState != state)
            await Task.Yield();
    }

    private void DebugMessage(LudeoMessage msg)
    {
        if (msg != LudeoMessage.Success)
        {
            Debug.LogError(msg);
        }
    }

    public class CreatorFlow
    {
        public bool IsRunningGameplay { get; private set; } = false;
        private LudeoFacade _ludeoFacade;

        public CreatorFlow(LudeoFacade ludeo)
        {
            _ludeoFacade = ludeo;
        }

        public async Task BeginGameplay()
        {
            await BeginGameplay(new GameplayDefinitions());
        }

        public async Task BeginGameplay(GameplayDefinitions definitions)
        {
            if (!_ludeoFacade.IsInitialized)
                throw new InvalidOperationException("Ludeo must be initialized before beginning gameplay.");
            if (definitions == null)
                throw new ArgumentNullException(nameof(definitions), "definitions cannot be null. If you want no definitions pass an empty object instead.");
            if (_ludeoFacade.CurrentFlowState != LudeoFlowState.WaitingForLoadGameplayData)
                throw new InvalidOperationException($"Ludeo is currently in a state not permitting begin gameplay. Ludeo State: {_ludeoFacade.CurrentFlowState}");
            if (IsRunningGameplay)
            {
                Debug.Log("Cannot begin another gameplay until the current one is finished.");
                return;
            }

            var msg = LudeoManager.LoadGameplayData();
            _ludeoFacade.DebugMessage(msg);

            await _ludeoFacade.AwaitState(LudeoFlowState.WaitingForSetGameplayDefinitions);

            msg = LudeoManager.SetGameplayDefinitions(definitions);
            _ludeoFacade.DebugMessage(msg);

            await _ludeoFacade.AwaitState(LudeoFlowState.WaitingForReadyForGameplay);

            msg = LudeoManager.ReadyForGameplay();
            _ludeoFacade.DebugMessage(msg);

            await _ludeoFacade.AwaitState(LudeoFlowState.WaitingForGameplayBegin);

            msg = LudeoManager.BeginGameplay();
            _ludeoFacade.DebugMessage(msg);

            await _ludeoFacade.AwaitState(LudeoFlowState.GameplayOn);

            IsRunningGameplay = true;
        }

        public void MarkHIghlight() => TryRunGameplayFunctionality(LudeoManager.MarkHighlight);

        public void PauseGameplay() => TryRunGameplayFunctionality(LudeoManager.PauseGameplay);

        public void ResumeGameplay() => TryRunGameplayFunctionality(LudeoManager.ResumeGameplay);

        public async Task EndGameplay()
        {
            TryRunGameplayFunctionality(LudeoManager.EndGameplay);
            await _ludeoFacade.AwaitState(LudeoFlowState.WaitingForLoadGameplayData);
            IsRunningGameplay = false;
        }

        private bool TryRunGameplayFunctionality(Func<LudeoMessage> functionality)
        {
            if (!IsRunningGameplay)
            {
                Debug.LogError($"Ludeo: Cannot {functionality.Method.Name} when not in gameplay.");
                return false;
            }

            var msg = functionality();
            if (msg == LudeoMessage.Success)
            {
                return true;
            }
            else
            {
                Debug.Log($"Ludeo: {msg}");
                return false;
            }
        }
    }

    // TODO: Replay Ludeo
    public class PlayerFlow
    {
        public bool IsRunningLudeo { get; internal set; } = false;

        public event Action RestartRequested;

        public IReadOnlyDictionary<string, object> GameState;

        Guid _guid;

        private LudeoFacade _ludeoFacade;
        public PlayerFlow(LudeoFacade ludeo)
        {
            _ludeoFacade = ludeo;
        }

        public async Task BeginGameplay(string ludeoGUID)
        {
            if (!_ludeoFacade.IsInitialized)
                throw new InvalidOperationException("Ludeo must be initialized before beginning Ludeo.");
            if (_ludeoFacade.CurrentFlowState != LudeoFlowState.WaitingForLoadGameplayData)
                throw new InvalidOperationException($"Ludeo is currently in a state not permitting begin gameplay. Ludeo State: {_ludeoFacade.CurrentFlowState}");
            if (IsRunningLudeo)
            {
                Debug.Log("Cannot begin another Ludeo until the current one is aborted.");
                return;
            }

            if (TestGuidValidity(ludeoGUID))
            {
                _guid = new Guid(ludeoGUID);
            }
            else
            {
                Debug.LogError("Ludeo: GUID provided is not valid, please provide a valid GUID.");
                return;
            }

            var msg = LudeoManager.LoadGameplayData(_guid);
            _ludeoFacade.DebugMessage(msg);

            await _ludeoFacade.AwaitState(LudeoFlowState.WaitingForGetGameplayDefinitions);

            var defs = GetDefinitions();
            var vars = GetGameState();

            GameState = vars.Concat(defs).ToDictionary(x => x.Key, x => x.Value);

            await RunLudeo();
        }

        private async Task RunLudeo()
        {
            var msg = LudeoManager.ReadyForGameplay();

            _ludeoFacade.DebugMessage(msg);

            await _ludeoFacade.AwaitState(LudeoFlowState.WaitingForGameplayBegin);

            msg = LudeoManager.BeginGameplay();
            _ludeoFacade.DebugMessage(msg);

            IsRunningLudeo = true;

            CheckRestart();
        }

        private async Task CheckRestart()
        {
            while (IsRunningLudeo && _ludeoFacade.CurrentFlowState != LudeoFlowState.WaitingForGetGameplayDefinitions)
                await Task.Yield();

            OnRestart();
        }

        private void OnRestart()
        {
            RunLudeo();
            RestartRequested?.Invoke();
        }

        public void EndGameplay()
        {
            LudeoManager.AbortGameplay();
            IsRunningLudeo = false;
        }

        private bool TestGuidValidity(string ludeoGUID)
        {
            // xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
            var split = ludeoGUID.Split('-');

            if (string.IsNullOrEmpty(ludeoGUID))
                return false;
            if (split.Length != 5)
                return false;
            if (ludeoGUID.Length != 36)
                return false;
            if (split[0].Length != 8)
                return false;
            if (split[1].Length != 4)
                return false;
            if (split[2].Length != 4)
                return false;
            if (split[3].Length != 4)
                return false;
            if (split[4].Length != 12)
                return false;

            return true;
        }

        private Dictionary<string, object> GetGameState()
        {
            var ludeoGameState = GetLudeoGameState(
                new GetLudeoKeysDelegate(LudeoManager.GetGameplayStateKeys),
                new GetLudeoKeyValueDelegate<bool>(LudeoManager.GetGameplayState),
                new GetLudeoKeyValueDelegate<float>(LudeoManager.GetGameplayState),
                new GetLudeoKeyValueDelegate<int>(LudeoManager.GetGameplayState),
                new GetLudeoKeyValueDelegate<Quatern>(LudeoManager.GetGameplayState),
                new GetLudeoKeyValueDelegate<Vec3>(LudeoManager.GetGameplayState)
                );

            foreach (var kvp in ludeoGameState)
            {
                switch (kvp.Value)
                {
                    case bool b:
                        _ludeoFacade.SetGameplayVariable(kvp.Key, b);
                        break;
                    case float f:
                        _ludeoFacade.SetGameplayVariable(kvp.Key, f);
                        break;
                    case int i:
                        _ludeoFacade.SetGameplayVariable(kvp.Key, i);
                        break;
                    case Quatern q:
                        _ludeoFacade.SetGameplayVariable(kvp.Key, q);
                        break;
                    case Vec3 v:
                        _ludeoFacade.SetGameplayVariable(kvp.Key, v);
                        break;
                }
            }

            return ludeoGameState;
        }

        private Dictionary<string, object> GetDefinitions()
        {
            return GetLudeoGameState(
                new GetLudeoKeysDelegate(LudeoManager.GetGameplayDefinitionsKeys),
                new GetLudeoKeyValueDelegate<bool>(LudeoManager.GetGameplayDefinition),
                new GetLudeoKeyValueDelegate<float>(LudeoManager.GetGameplayDefinition),
                new GetLudeoKeyValueDelegate<int>(LudeoManager.GetGameplayDefinition),
                new GetLudeoKeyValueDelegate<Quatern>(LudeoManager.GetGameplayDefinition),
                new GetLudeoKeyValueDelegate<Vec3>(LudeoManager.GetGameplayDefinition)
                );
        }

        delegate LudeoMessage GetLudeoKeysDelegate(LudeoParam ludeoParam, out string[] def);
        delegate LudeoMessage GetLudeoKeyValueDelegate<T>(string key, out T value);
        private Dictionary<string, object> GetLudeoGameState
            (GetLudeoKeysDelegate getKeys,
            GetLudeoKeyValueDelegate<bool> getBoolState,
            GetLudeoKeyValueDelegate<float> getfloatState,
            GetLudeoKeyValueDelegate<int> getIntState,
            GetLudeoKeyValueDelegate<Quatern> getQState,
            GetLudeoKeyValueDelegate<Vec3> getVState)
        {
            var ret = new Dictionary<string, object>(128);

            var msg = getKeys(LudeoParam.Bool, out string[] def1);
            _ludeoFacade.DebugMessage(msg);
            msg = getKeys(LudeoParam.Float, out string[] def2);
            _ludeoFacade.DebugMessage(msg);
            msg = getKeys(LudeoParam.Int, out string[] def3);
            _ludeoFacade.DebugMessage(msg);
            msg = getKeys(LudeoParam.Quatern, out string[] def4);
            _ludeoFacade.DebugMessage(msg);
            msg = getKeys(LudeoParam.Vec3, out string[] def5);
            _ludeoFacade.DebugMessage(msg);

            string[][] definitions = { def1, def2, def3, def4, def5 };

            for (int i = 0; i < definitions.Length; i++)
            {
                string[] keys = definitions[i];
                if (keys == null || keys.Length == 0)
                    continue;

                foreach (var key in keys)
                {
                    switch (i)
                    {
                        case 0:
                            getBoolState(key, out bool value1);
                            ret.Add(key, value1);
                            break;
                        case 1:
                            getfloatState(key, out float value2);
                            ret.Add(key, value2);
                            break;
                        case 2:
                            getIntState(key, out int value3);
                            ret.Add(key, value3);
                            break;
                        case 3:
                            getQState(key, out Quatern value4);
                            ret.Add(key, value4.ToQuaternion());
                            break;
                        case 4:
                            getVState(key, out Vec3 value5);
                            ret.Add(key, value5.ToVector3());
                            break;
                    }
                }
            }

            return ret;
        }
    }
}