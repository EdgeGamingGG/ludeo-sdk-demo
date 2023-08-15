using cohtml;
using cohtml.InputSystem;
using UnityEngine;
using LudeoSDK.UI;
using UnityEngine.Rendering;

namespace LudeoSDK
{
    public class CoherentLudeoUrlScreen : MonoBehaviour, ILudeoCoherentViewWrapper
    {
        [SerializeField]
        private Shader      m_clearShader;

        private Camera      m_ludeoViewCam;
        private bool        m_isFirstCall;
        private CohtmlView  m_cohtmlView;

        private Callback                    m_onConnected;
        private CallbackWithRenderTexture   m_onPageLoaded;
        private CallbackWithString          m_onInfo;
        private CallbackWithString          m_onWarning;
        private CallbackWithString          m_onError;

        private CameraEvent cameraEvent = CameraEvent.BeforeDepthTexture;

        private Material m_cameraMaterial;
        private CommandBuffer m_commandBuffer;
        private RenderTexture m_renderTexture;
        private bool m_isCameraSet;

        private void Awake()
        {
            gameObject.AddComponent<CohtmlInitializer>();
            gameObject.AddComponent<UnityMainThreadDispatcher>();
            if (m_clearShader == null)
                m_clearShader = Shader.Find("LudeoSDK/ClearTransparent");
        }

        public void SetLogger(CallbackWithString onInfo, CallbackWithString onWarning, CallbackWithString onError)
        {
            m_onInfo = onInfo;
            m_onWarning = onWarning;
            m_onError = onError;
        }

        public void OnShow(string url, CallbackWithRenderTexture onPageLoaded, Callback onConnected)
        {
            m_onPageLoaded = onPageLoaded;
            m_onConnected = onConnected;
            m_isFirstCall = true;
            m_cohtmlView.Page = url;
        }

        public void OnCreateView(GameObject go, Camera camera, out CallbackWithString onCoherentScript, out CallbackWithString onTriggerEvent)
        {
            m_ludeoViewCam = camera;
            m_cohtmlView = go.GetComponent<CohtmlView>();
            onCoherentScript = ExecuteScriptWithView;
            onTriggerEvent = TriggerEventWithView;
            
            CohtmlInputHandler.OnFocusView -= OnViewFocues;
            CohtmlInputHandler.OnFocusView += OnViewFocues;

            ViewListener viewListener = m_cohtmlView.Listener;

            viewListener.OnWebSocketOpened -= OnConnected;
            viewListener.OnWebSocketOpened += OnConnected;
            
            viewListener.FinishLoad -= OnCohtmlLoaded;
            viewListener.FinishLoad += OnCohtmlLoaded;
            
            viewListener.LoadFailed -= OnCohtmlFailedToLoad;
            viewListener.LoadFailed += OnCohtmlFailedToLoad;

            viewListener.DOMBuilt -= OnDOMBuilt;
            viewListener.DOMBuilt += OnDOMBuilt;

            viewListener.ReadyForBindings -= OnReadyForBindings;
            viewListener.ReadyForBindings += OnReadyForBindings;

            viewListener.ScriptContextCreated -= OnScriptContextCreated;
            viewListener.ScriptContextCreated += OnScriptContextCreated;

        }

        private void OnDOMBuilt()
        {
            m_onInfo("Cohtml, OnDOMBuilt");
        }

        private void OnReadyForBindings()
        {
            m_onInfo("Cohtml, OnReadyForBindings");      
        }

        private void OnScriptContextCreated()
        {
            m_onInfo("Cohtml, OnScriptContextCreated");
        }

        public void OnHide()
        {
            ViewListener viewListener = m_cohtmlView.Listener;

            CohtmlInputHandler.OnFocusView -= OnViewFocues;
            viewListener.OnWebSocketOpened      -= OnConnected;
            viewListener.FinishLoad             -= OnCohtmlLoaded;
            viewListener.LoadFailed             -= OnCohtmlFailedToLoad;
            viewListener.DOMBuilt               -= OnDOMBuilt;
            viewListener.ReadyForBindings       -= OnReadyForBindings;
            viewListener.ScriptContextCreated   -= OnScriptContextCreated;

            m_cohtmlView.enabled = false;
        }

        public void OnConnected()
        {
            UnityMainThreadDispatcher.Enqueue(NotifyOnConnect);
        }

        private void NotifyOnConnect()
        {
            m_onConnected();
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        private void OnCohtmlLoaded(string data)
        {
            if (m_isFirstCall)
            {
                m_isFirstCall = false;

                m_onPageLoaded(m_cohtmlView.ViewTexture);

                SetupCameraCommandBuffer(m_ludeoViewCam.targetTexture);

                CohtmlInputHandler.RaycastCamera = m_ludeoViewCam;

                m_cohtmlView.View.SetResolutionForRendering((uint)Screen.width, (uint)Screen.height);
            }
            else         

            m_onInfo($"Cohtml, OnCohtmlLoaded with {data}");
        }

        private void OnViewFocues(CohtmlView cohtmlView)
        {            
            m_onInfo($"Cohtml, CohtmlInputHandler.OnFocusView with");
        }

        private void OnCohtmlFailedToLoad(string data1, string data2)
        {
            m_onInfo($"Cohtml, CohtmlView.Listener.LoadFailed with {data1} and {data2}");
        }

        private void ExecuteScriptWithView(string data)
        {
            if ((m_cohtmlView != null) && (m_cohtmlView.View != null))
                m_cohtmlView.View.ExecuteScript(data);
            else
                m_onError($"Cohtml, not ready to run script");
        }

        private void TriggerEventWithView(string data)
        {
            if ((m_cohtmlView != null) && (m_cohtmlView.View != null))
                m_cohtmlView.View.TriggerEvent(data);
            else
                m_onError($"Cohtml, not ready to trigger event");
        }

        private void SetupCameraCommandBuffer(RenderTexture renderTexture)
        {
            m_cameraMaterial = new Material(m_clearShader);
            m_renderTexture = renderTexture;

            m_commandBuffer = new CommandBuffer();

            m_ludeoViewCam.AddCommandBuffer(cameraEvent, m_commandBuffer);

            m_isCameraSet = true;
        }

        private void Update()
        {
            if (m_isCameraSet)
            {               
                m_commandBuffer.Clear();

                m_commandBuffer.SetRenderTarget(m_renderTexture);

                RenderTexture previousActive = RenderTexture.active;
                RenderTexture.active = m_renderTexture;
                GL.Clear(true, true, Color.clear);
                RenderTexture.active = previousActive;

                // Draw a full-screen triangle
                m_commandBuffer.DrawProcedural(Matrix4x4.identity, m_cameraMaterial, 0, MeshTopology.Triangles, 3);
            }
        }
    }
}