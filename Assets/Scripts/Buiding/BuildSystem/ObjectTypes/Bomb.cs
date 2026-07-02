using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public class Bomb : CoreObject
{
    public int rangeLeft, rangeRight, rangeUp, rangeDown, rangeForward, rangeBack;
    bool exploded;
    [Tooltip("How much force is applied to an object when it is hit by a bomb. Does not apply to kinematic bodies.")]
    public float blastForce;
    public float fuseTime;

    public bool canAdjustFuse = true;

    public float fuseMaxTime = 5, fuseMinTime = 0.1f;

    public Transform timerRoot;
    public TMP_Text timerText;

    public ParticleSystem fuse, explosion;

    public bool hideOnExplode;

    public LayerMask mask;

    public CinemachineImpulseSource impulseSource;

    public float blastShake = 0.5f;

    public int triggerCount = 1;
    int triggersLeft;

    private void Start()
    {
        triggersLeft = triggerCount;
        Adjust(0);
    }

    private void Update()
    {
        timerRoot.forward = Camera.main.transform.forward;
    }

    public override void Adjust(float delta)
    {
        fuseTime += delta;
        fuseTime = Mathf.Clamp(fuseTime, fuseMinTime, fuseMaxTime);

        timerText.text = $"{fuseTime:0.0}";
    }

    public override void Interacted()
    {
        ObjectAction();   
    }

    protected override void ObjectAction()
    {
        if (triggersLeft > 0 || triggerCount == -1)
        {
            triggersLeft--;
            if (fuseTime <= 0)
            {
                Explode();
            }
            else
            {
                fuse.Play();
                StartCoroutine(BlastDelay());
            }
        }
    }


    IEnumerator BlastDelay()
    {
        float t = fuseTime;
        while (t >= 0)
        {
            timerText.text = $"{t:0.0}";
            t -= Time.fixedDeltaTime;
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
        Explode();
    }

    void Explode()
    {
        impulseSource.GenerateImpulse(Random.onUnitSphere * blastShake);

        //forward/backward cast
        CastExplodeRay(transform.forward, rangeForward);
        CastExplodeRay(-transform.forward, rangeBack);
        //side casts
        CastExplodeRay(transform.right, rangeRight);
        CastExplodeRay(-transform.right, rangeLeft);
        //up/down casts
        CastExplodeRay(transform.up, rangeUp);
        CastExplodeRay(-transform.up, rangeDown);

        if (hideOnExplode)
        {
            visualRoot.SetActive(false);
        }
        explosion.Play();
        fuse.Stop();
    }
    void CastExplodeRay(Vector3 direction, float range)
    {
        range += Mathf.Abs(Vector3.Dot(interactTrigger.size, direction) * 0.5f);

        Debug.DrawRay(transform.position, direction * range, Color.red, 2f, true);

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, range, mask, QueryTriggerInteraction.Ignore))
        {
            if(hit.rigidbody != null && hit.rigidbody.TryGetComponent(out CoreObject obj))
            {
                obj.Interacted();
                if (!hit.rigidbody.isKinematic)
                {
                    hit.rigidbody.AddForce(direction * blastForce);
                }
            }
        }
    }
}
