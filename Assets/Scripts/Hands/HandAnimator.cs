using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets
{
    // Largely repurposing the pre-existing controller animator script as a base
    public class HandAnimator : MonoBehaviour
    {
        [Header("Thumbstick")]

        [SerializeField]
        XRInputValueReader<Vector2> m_StickInput = new XRInputValueReader<Vector2>("Thumbstick");
        
        [Header("Index")]


        [SerializeField]
        XRInputValueReader<float> m_TriggerInput = new XRInputValueReader<float>("Trigger");

        [Header("Grip")]

        [SerializeField]
        XRInputValueReader<float> m_GripInput = new XRInputValueReader<float>("Grip");

        private Animator animator;
        private float thumbVal;
        private float indexVal;
        private float gripVal;


        void Start()
        {
            animator = GetComponent<Animator>();
            thumbVal = 0.0f;
            indexVal = 0.0f;
            gripVal = 0.0f;
        }
        void OnEnable()
        {
            if (thumbVal == null || indexVal == null || gripVal == null)
            {
                enabled = false;
                Debug.LogWarning($"Controller Animator component missing references on {gameObject.name}", this);
                return;
            }
            
            m_StickInput?.EnableDirectActionIfModeUsed();
            m_TriggerInput?.EnableDirectActionIfModeUsed();
            m_GripInput?.EnableDirectActionIfModeUsed();
        }

        void OnDisable()
        {
            m_StickInput?.DisableDirectActionIfModeUsed();
            m_TriggerInput?.DisableDirectActionIfModeUsed();
            m_GripInput?.DisableDirectActionIfModeUsed();
        }

        void Update()
        {
            if (m_StickInput != null)
            {
                //var thumbVal = m_StickInput.ReadValue();
                // Probably not going to read anything from the thumbstick for this!

            }

            if (m_TriggerInput != null)
            {
                indexVal = m_TriggerInput.ReadValue();
                animator.SetFloat("index", indexVal);
            }
            
            if (m_GripInput != null)
            {
                gripVal = m_GripInput.ReadValue();
                animator.SetFloat("3fingers", gripVal);
                animator.SetFloat("thumb", gripVal);

            }
        }
    }
}