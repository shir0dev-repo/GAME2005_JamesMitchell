using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

#if SADASD
[CustomEditor(typeof(PhysicsBody))]
public class PhysicsBodyEditor : Editor
{
    public VisualTreeAsset m_InspectorXML;

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement inspector = new VisualElement();

        inspector.Add(new Label("Custom Editor"));

        FloatField ff = new FloatField("Mass");
        ff.bindingPath = "m_mass";
        ff.RegisterCallback<BlurEvent>(evt =>
        {
            if (evt.target == ff)
            {
                float valid = Mathf.Clamp(ff.value, 0.001f, Mathf.Infinity);

                evt.StopImmediatePropagation();
                evt.PreventDefault();
                ff.value = valid;
            }
        }, TrickleDown.TrickleDown);
        inspector.Add(ff);
        
        Slider dragSlider = new Slider("Drag", 0, 1.5f, SliderDirection.Horizontal, 1);
        FloatField sliderFF = new FloatField();
        dragSlider.Add(sliderFF);
        dragSlider.bindingPath = "m_dragCoefficient";
        dragSlider.RegisterValueChangedCallback(evt =>
        {
            if (evt.target == dragSlider)
            {
                evt.StopImmediatePropagation();
                evt.PreventDefault();
                sliderFF.SetValueWithoutNotify(dragSlider.value);
            }
        });

        VisualElement dragElem = new VisualElement();
        dragElem.Add(new Label("Drag"));
        dragElem.Add(dragSlider);
        dragElem.Add(sliderFF);

        inspector.Add(dragElem);

        m_InspectorXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/PhysicsBody_UXML.uxml");

        inspector.Add(m_InspectorXML.Instantiate());

        return inspector;
    }
}
#endif