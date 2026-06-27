using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlacementSystem : MonoBehaviour
{

    Vector2 mousePos;
    bool positionChecked;
    RaycastHit lastMouseHit;
    bool didHit;

    public Vector3 gridBoxHit;

    public List<BuildingSO> placeables;
    public RawImage currIconDisplay;
    public TMP_Text currNameDisplay;
    int placeableIndex;

    public LayerMask mask;

    public void OnMouseMove(InputAction.CallbackContext context)
    {
        mousePos = context.ReadValue<Vector2>();
        positionChecked = false;
    }

    public void SwitchPlaceable(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            placeableIndex += Mathf.RoundToInt(context.ReadValue<float>());
            placeableIndex = (int)Mathf.Repeat(placeableIndex, placeables.Count);
            UpdatePlaceable();
        }
    }

    public void OnConfirmAction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            PlaceBuilding();
        }
    }

    void UpdatePlaceable()
    {
        if (placeableIndex < placeables.Count && placeableIndex > -1)
        {
            currIconDisplay.texture = placeables[placeableIndex].icon;
            currNameDisplay.text = placeables[placeableIndex].DisplayName;
        }
    }

    void PlaceBuilding()
    {
        Debug.Log($"placed building @ {gridBoxHit}");
    }



    private void Start()
    {
        UpdatePlaceable();
    }

    private void Update()
    {
        if (!positionChecked)
        {
            var ray =  Camera.main.ScreenPointToRay(mousePos);
            didHit = Physics.Raycast(ray, out lastMouseHit, 50, mask);
            if (didHit)
            {
                gridBoxHit = new()
                {
                    x = Mathf.Round(lastMouseHit.point.x),
                    y = Mathf.Round(lastMouseHit.point.y),
                    z = Mathf.Round(lastMouseHit.point.z)
                };
            }
            Debug.Log($"checked: {didHit}");
            positionChecked = true;
        }
        if( didHit)
        {
            Debug.DrawRay(lastMouseHit.point, Vector3.up, Color.green, Time.deltaTime);
            Debug.DrawRay(gridBoxHit + new Vector3(-0.5f, 0, -0.5f), Vector3.up, Color.yellow, Time.deltaTime);
            Debug.DrawRay(gridBoxHit + new Vector3(.5f, 0, .5f), Vector3.up, Color.yellow, Time.deltaTime);
            Debug.DrawRay(gridBoxHit + new Vector3(.5f, 0, -.5f), Vector3.up, Color.yellow, Time.deltaTime);
            Debug.DrawRay(gridBoxHit + new Vector3(-.5f, 0, .5f), Vector3.up, Color.yellow, Time.deltaTime);

        }
    }


}
