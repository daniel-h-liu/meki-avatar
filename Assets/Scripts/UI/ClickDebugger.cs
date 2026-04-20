using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ClickDebugger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    void Update() {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Mouse.current.position.ReadValue();
            
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);
            
            for (int i = 0; i < results.Count; i++)
            {
                Debug.Log("You just clicked on: " + results[i].gameObject.name);
            }

            if (results.Count == 0)
            {
                Debug.Log("Nothing clicked.");
            }
        }
}
}
