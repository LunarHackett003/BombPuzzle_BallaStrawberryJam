using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlacementSystem : MonoBehaviour
{

    Vector2 mousePos;
    bool positionChecked;
    RaycastHit lastMouseHit;
    bool didHit;

    bool rotatingCam;
    public float mouseDragSpeed, camDistance, ctrlRotateSpeed;

    public float pitch, yaw;
    public Transform cam;
    public Transform camTarget;

    public Vector3Int gridBoxHit;


    public float adjustRate;

    public List<BuildingSO> placeables;
    public RawImage currIconDisplay;
    public TMP_Text currNameDisplay;
    int placeableIndex;

    public LayerMask mask;

    public bool deleteMode;
    public bool rotateMode;


    public GameObject rotateGizmo;
    public CanvasGroup rotateUIGroup;
    public CanvasGroup deleteUIGroup;

    public CoreObject targetedObject;
    Rigidbody lastHitRB;

    public GameObject highlighter;
    public SpriteRenderer highlightSprite;

    public Color selectColour, deleteColour;
    

    public void OnMouseMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            mousePos = context.ReadValue<Vector2>();
            positionChecked = false;
        }
    }

    public void OnMouseMovedCamera(InputAction.CallbackContext context)
    {
        if (rotatingCam)
        {
            AddCamInput(0.001f * mouseDragSpeed * context.ReadValue<Vector2>());
        }
    }
    private void AddCamInput(Vector2 input)
    {
        pitch = Mathf.Clamp(pitch - input.y, -5, 65);
        yaw = (yaw + input.x) % 360;

        camTarget.rotation = Quaternion.Euler(pitch, yaw, 0);
        cam.position = camTarget.TransformPoint(Vector3.back * camDistance);
        cam.LookAt(camTarget, camTarget.up);
    }

    public void OnDragMouse(InputAction.CallbackContext context)
    {
        rotatingCam = context.ReadValueAsButton();
        Cursor.lockState = rotatingCam ? CursorLockMode.Locked : CursorLockMode.None;
    }
    public void OnCamControl(InputAction.CallbackContext context)
    {
        if (context.performed)
            AddCamInput(context.ReadValue<Vector2>() * ctrlRotateSpeed);
    }

    public void Adjust(InputAction.CallbackContext context)
    {
        if (context.performed && targetedObject != null)
        {
            targetedObject.Adjust(context.ReadValue<float>() * adjustRate);
        }
    }
    
    
    public void ToggleRotateMode(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            rotateMode = !rotateMode;
            deleteMode = false;

            UpdateUI();
        }
    }
    public void ToggleDeleteMode(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            deleteMode = !deleteMode;
            rotateMode = false;
            UpdateUI();
        }
    }
    void UpdateUI()
    {
        ToggleRotateUI();
        ToggleDeleteUI();
    }
    void ToggleRotateUI()
    {
        rotateGizmo.SetActive(rotateMode);
        rotateUIGroup.alpha = rotateMode ? 1 : 0;
    }
    void ToggleDeleteUI()
    {
        highlightSprite.color = deleteMode ? deleteColour : selectColour;
        deleteUIGroup.alpha = deleteMode ? 1 : 0;
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
    public void AltConfirmAction(InputAction.CallbackContext context)
    {
        if (!rotatingCam && context.performed && targetedObject != null)
        {
            if (rotateMode)
            {
                TryRotate(true);
            }
        }
    }

    public void OnConfirmAction(InputAction.CallbackContext context)
    {
        if (rotatingCam)
        {
            return;
        }

        if (context.performed)
        {
            if (didHit)
            {
                if(targetedObject == null)
                {
                    if ( !deleteMode && !rotateMode)
                    {
                        PlaceBuilding();
                    }
                }
                else
                {
                    if (deleteMode)
                    {
                        Destroy(targetedObject.gameObject);
                        targetedObject = null;
                        lastHitRB = null;
                    }
                    else if (rotateMode)
                    {
                        TryRotate();
                    }
                    else
                    {
                        targetedObject.Interacted();
                    }
                }
            }
            positionChecked = false;
        }
    }
    void TryRotate(bool alt = false)
    {
        Vector3 angle = Vector3.zero;
        if(targetedObject.rotationLocked)
        {
            angle = targetedObject.lockedAxis;
        }
        else
        {
            angle = lastMouseHit.normal;
        }
        targetedObject.Rotate(Vector3Int.RoundToInt(angle) * (alt ? -1 : 1));
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
        //we have a valid index
        if(placeables.Count > 0 && placeableIndex > -1 && placeableIndex < placeables.Count)
        {
            if (placeables[placeableIndex].prefab != null)
            {
                var go = Instantiate(placeables[placeableIndex].prefab, (Vector3)gridBoxHit + (Vector3.up * 0.5f), Quaternion.identity);
                positionChecked = false;
            }
        }
    }

    private void OnValidate()
    {

    }

    private void Start()
    {
        AddCamInput(Vector2.zero);
        UpdatePlaceable();
    }

    private void Update()
    {
        if (!positionChecked && !rotatingCam)
        {
            CheckMousePosition();
            if (didHit)
            {
                if (lastMouseHit.rigidbody != null)
                {
                    CheckHitRigidbody();
                }
                else
                {
                    if (targetedObject != null)
                        targetedObject = null;
                    DisableGizmo();
                }
            }
            else
            {
                targetedObject = null;
            }
        }
        GridHitDebug();
    }
    void DisableGizmo()
    {
        if (rotateGizmo.activeInHierarchy)
            rotateGizmo.SetActive(false);

    }
    void GridHitDebug()
    {
        Debug.DrawRay(lastMouseHit.point, Vector3.up, Color.green, Time.deltaTime);
        Debug.DrawRay(gridBoxHit + new Vector3(-0.5f, 0, -0.5f), Vector3.up, Color.yellow, Time.deltaTime);
        Debug.DrawRay(gridBoxHit + new Vector3(.5f, 0, .5f), Vector3.up, Color.yellow, Time.deltaTime);
        Debug.DrawRay(gridBoxHit + new Vector3(.5f, 0, -.5f), Vector3.up, Color.yellow, Time.deltaTime);
        Debug.DrawRay(gridBoxHit + new Vector3(-.5f, 0, .5f), Vector3.up, Color.yellow, Time.deltaTime);

    }
    void CheckMousePosition()
    {
        var ray = Camera.main.ScreenPointToRay(mousePos);
        didHit = Physics.Raycast(ray, out lastMouseHit, 50, mask, QueryTriggerInteraction.Collide);
        if (didHit)
        {
            gridBoxHit = Vector3Int.RoundToInt(lastMouseHit.point);
            highlighter.transform.position = gridBoxHit;
        }
        Debug.Log($"checked: {didHit}");
        positionChecked = true;
    }
    void CheckHitRigidbody()
    {
        if ((lastHitRB == null || lastHitRB != lastMouseHit.rigidbody) && lastMouseHit.rigidbody.TryGetComponent(out targetedObject))
        {
            highlighter.transform.position = targetedObject.transform.position - (targetedObject.transform.up * 0.4f);
        }
        if (targetedObject != null && rotateMode)
        {
            rotateGizmo.transform.position = targetedObject.transform.position + (lastMouseHit.normal * 0.5f);
            rotateGizmo.transform.up = lastMouseHit.normal;
            if (!rotateGizmo.activeInHierarchy)
                rotateGizmo.SetActive(true);
        }
        if(targetedObject == null)
        {
            DisableGizmo();
        }
    }
}
