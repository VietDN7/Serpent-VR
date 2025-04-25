using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets
{
    // Largely repurposing the pre-existing controller animator script as a base
    public class CamoMenu : MonoBehaviour
    {
        [Header("Face Button")]

        [SerializeField]
        XRInputValueReader<float> m_ButtonInput = new XRInputValueReader<float>("X");

        private GameObject camoMenuCanvas;
        private float buttonVal;



        void Start()
        {
            camoMenuCanvas = gameObject.transform.GetChild(0).gameObject;
            buttonVal = 0.0f;
        }
        void OnEnable()
        {
            if (buttonVal == null)
            {
                enabled = false;
                Debug.LogWarning($"Controller Animator component missing references on {gameObject.name}", this);
                return;
            }
            
            m_ButtonInput?.EnableDirectActionIfModeUsed();
        }

        void OnDisable()
        {

            m_ButtonInput?.DisableDirectActionIfModeUsed();
        }

        void Update()
        {

            if (m_ButtonInput != null)
            {
                buttonVal = m_ButtonInput.ReadValue();
                camoMenuCanvas.SetActive(true);

            }
            else
            {
                camoMenuCanvas.SetActive(false);
            }
        }
    }
}