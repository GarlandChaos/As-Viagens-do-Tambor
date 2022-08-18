using UnityEngine;

namespace TamborGame.UI
{
    [DisallowMultipleComponent]
    public class UIManager : MonoBehaviour
    {
        //Singleton
        public static UIManager instance { get; set; }

        //Inspector reference fields
        [SerializeField]
        UISettings settings = null;
        [SerializeField]
        PanelLayer panelLayer = null;
        [SerializeField]
        DialogLayer dialogLayer = null;
        [SerializeField]
        SpecialPanelLayer specialPanelLayer = null;
        [SerializeField]
        Canvas mainCanvas = null;
        [SerializeField]
        Camera UICamera = null;

        //Properties
        public Canvas _MainCanvas { get => mainCanvas; }
        public Camera _UICamera { get => UICamera; }

        private void Awake()
        {
            if (instance != null)
                Destroy(this);
            else
            {
                instance = GetComponent<UIManager>();
                DontDestroyOnLoad(this);
            }

            Initialize();
        }

        void Initialize()
        {
            foreach (var screen in settings.screensPrefabs)
            {
                var screenInstance = Instantiate(screen);
                screenInstance.SetActive(false);
                var screenController = screenInstance.GetComponent<IScreenController>();

                if (screenController != null)
                {
                    IDialogController dialog = screenController as IDialogController;
                    if (dialog != null)
                    {
                        dialogLayer.RegisterScreen(screen.name, dialog, screenInstance.transform);
                        continue;
                    }

                    IPanelController panel = screenController as IPanelController;
                    if (panel != null)
                    {
                        panelLayer.RegisterScreen(screen.name, panel, screenInstance.transform);
                        continue;
                    }

                    ISpecialPanelController specialPanel = screenController as ISpecialPanelController;
                    if (specialPanel != null)
                    {
                        specialPanelLayer.RegisterScreen(screen.name, specialPanel, screenInstance.transform);
                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// Requests a screen with the given ID and open or close action
        /// </summary>
        /// <param name="screenID">The requested screen ID.</param>
        /// <param name="open">If true open the screen, otherwise close it.</param>
        public void RequestScreen(string screenID, bool open)
        {
            if (panelLayer.HasScreen(screenID))
            {
                if (open)
                    panelLayer.ShowScreen(screenID);
                else
                    panelLayer.HideScreen(screenID);
            }
            else if (dialogLayer.HasScreen(screenID))
            {
                if (open)
                    dialogLayer.ShowScreen(screenID);
                else
                    dialogLayer.HideScreen(screenID);
            }
            else if (specialPanelLayer.HasScreen(screenID))
            {
                if (open)
                    specialPanelLayer.ShowScreen(screenID);
                else
                    specialPanelLayer.HideScreen(screenID);
            }
#if UNITY_EDITOR
            else
                Debug.LogError(screenID + " not found on any layer.");
#endif
        }

        /// <summary>
        /// Requests a screen with the given ID only. If screen is visible, it will be disabled, otherwise, enabled;
        /// </summary>
        /// <param name="screenID">The requested screen ID.</param>
        public void RequestScreen(string screenID)
        {
            if (panelLayer.HasScreen(screenID))
            {
                if (!panelLayer.IsScreenVisible(screenID))
                    panelLayer.ShowScreen(screenID);
                else
                    panelLayer.HideScreen(screenID);
            }
            else if (dialogLayer.HasScreen(screenID))
            {
                if (!dialogLayer.IsScreenVisible(screenID))
                    dialogLayer.ShowScreen(screenID);
                else
                    dialogLayer.HideScreen(screenID);
            }
            else if (specialPanelLayer.HasScreen(screenID))
            {
                if (!specialPanelLayer.IsScreenVisible(screenID))
                    specialPanelLayer.ShowScreen(screenID);
                else
                    specialPanelLayer.HideScreen(screenID);
            }
#if UNITY_EDITOR
            else
                Debug.LogError(screenID + " not found on any layer.");
#endif
        }

        public bool IsScreenOnStack(string screenID)
        {
            if (dialogLayer.HasScreen(screenID))
                return dialogLayer.IsScreenOnStack(screenID);

            throw new System.Exception("dialogLayer doesn't have " + screenID);
        }

        public void ClearAllScreens()
        {
            panelLayer.ClearScreens();
            dialogLayer.ClearScreens();
            specialPanelLayer.ClearScreens();
        }

        public bool IsScreenVisible(string screenID)
        {
            if (panelLayer.HasScreen(screenID))
                return panelLayer.IsScreenVisible(screenID);
            else if (dialogLayer.HasScreen(screenID))
                return dialogLayer.IsScreenVisible(screenID);
            else if (specialPanelLayer.HasScreen(screenID))
                return specialPanelLayer.IsScreenVisible(screenID);

            return false;
        }
    }
}
