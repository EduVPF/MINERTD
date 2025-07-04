using System;
using UnityEditor.U2D.Common;
using UnityEngine.UIElements;

namespace UnityEditor.U2D.Animation
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    internal partial class PastePanel : VisualElement
    {
#if ENABLE_UXML_TRAITS
        public class PastePanelFactory : UxmlFactory<PastePanel, PastePanelUxmlTraits> { }

        public class PastePanelUxmlTraits : UxmlTraits { }
#endif

        public event Action<bool, bool, bool, bool> onPasteActivated = (bones, mesh, flipX, flipY) => { };

        private Toggle m_BonesToggle;
        private Toggle m_MeshToggle;
        private Toggle m_FlipXToggle;
        private Toggle m_FlipYToggle;

        public bool bones
        {
            get { return m_BonesToggle.value && m_BonesToggle.enabledSelf; }
            set { m_BonesToggle.value = value; }
        }

        public bool mesh
        {
            get { return m_MeshToggle.value; }
            set { m_MeshToggle.value = value; }
        }

        public bool flipX
        {
            get { return m_FlipXToggle.value; }
            set { m_FlipXToggle.value = value; }
        }

        public bool flipY
        {
            get { return m_FlipYToggle.value; }
            set { m_FlipYToggle.value = value; }
        }

        public PastePanel()
        {
            styleSheets.Add(ResourceLoader.Load<StyleSheet>("SkinningModule/PastePanelStyle.uss"));
            if (EditorGUIUtility.isProSkin)
                AddToClassList("Dark");

            RegisterCallback<MouseDownEvent>((e) => { e.StopPropagation(); });
            RegisterCallback<MouseUpEvent>((e) => { e.StopPropagation(); });
        }

        public void BindElements()
        {
            m_BonesToggle = this.Q<Toggle>("BonesField");
            m_MeshToggle = this.Q<Toggle>("MeshField");
            m_FlipXToggle = this.Q<Toggle>("FlipXField");
            m_FlipYToggle = this.Q<Toggle>("FlipYField");

            Button pasteButton = this.Q<Button>("PasteButton");
            pasteButton.clickable.clicked += OnPasteActivated;
        }

        public void OnPasteActivated()
        {
            onPasteActivated(bones, mesh, flipX, flipY);
        }

        public void BonePasteEnable(bool value)
        {
            m_BonesToggle.SetEnabled(value);
        }

        public static PastePanel GenerateFromUXML()
        {
            VisualTreeAsset visualTree = ResourceLoader.Load<VisualTreeAsset>("SkinningModule/PastePanel.uxml");
            PastePanel clone = visualTree.CloneTree().Q<PastePanel>("PastePanel");
            clone.LocalizeTextInChildren();
            clone.BindElements();
            return clone;
        }
    }
}
