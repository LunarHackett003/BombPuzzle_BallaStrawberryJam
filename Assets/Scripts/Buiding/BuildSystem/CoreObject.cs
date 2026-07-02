using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
/// <summary>
/// The base class for everything that we will be allowed to place or interact with in this game.<br></br>
/// Contains methods and events that we may need all over the place.
/// </summary>
public abstract class CoreObject : MonoBehaviour
{
    public static Action OnGamePlay;
    public static Action OnGameReset;
    [SerializeField] protected bool actOnGamePlay;

    public GameObject visualRoot;
    public Vector3 originalPosition;
    public Vector3 angle;

    public bool rotationLocked;
    public Vector3Int lockedAxis = Vector3Int.up;

    public Rigidbody thisRB;
    public BoxCollider interactTrigger;

    private void Awake()
    {
        if (actOnGamePlay)
        {
            OnGamePlay += ObjectAction;
        }
        OnGameReset += ResetState;
        ResetState();
    }

    private void OnValidate()
    {
        if (interactTrigger == null)
        {
            TryGetComponent(out interactTrigger);
        }
        if (thisRB == null)
        {
            TryGetComponent(out thisRB);
        }
    }

    void ResetState()
    {
        originalPosition = transform.position;
        transform.eulerAngles = angle;
        visualRoot.SetActive(true);
    }

    /// <summary>
    /// When an incoming interaction hit this object, such as a bomb hitting this object, we'll do something.
    /// </summary>
    public virtual void Interacted()
    {
        //In most cases, we'd probably want to do ObjectAction here, but I've no idea of the scope of this yet
    }

    /// <summary>
    /// What happens when this object wants to do something, for example, triggers a bomb exploding
    /// </summary>
    protected virtual void ObjectAction()
    {
        
    }

    public virtual void Adjust(float delta)
    {

    }

    public void Rotate(Vector3Int axis)
    {
        angle += axis * 90;
        transform.eulerAngles = angle;
    }
}
