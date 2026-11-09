using Rive;
using Rive.Components;
using Rive.EditorTools;
using Rive.Utils;
using UnityEngine;

public class RiveTest : MonoBehaviour
{
    [SerializeField] private RiveWidget widget;
    [SerializeField] private int NewValue;
    private ViewModelInstanceNumberProperty property;

    private void OnEnable()
    {
        widget.OnWidgetStatusChanged += HandleWidgetStatusChanged;
    }

    private void OnDestroy()
    {
        widget.OnWidgetStatusChanged -= HandleWidgetStatusChanged;
    }

    private void HandleWidgetStatusChanged()
    {
        if (widget.Status == WidgetStatus.Loaded)
        {
            property = widget.StateMachine.ViewModelInstance.GetNumberProperty("healthAmount");            
        }
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        property.Value = NewValue;
    }
}
