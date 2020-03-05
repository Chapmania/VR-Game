using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.ComponentModel;

public class Arrow : MonoBehaviour
{
    private OVRGrabbable _grabbable;
    private Rigidbody _rigidbody;
    private bool _isFired;
    private Vector3 _lastPosition;
    private bool _isAttached;

    public float speed;
    public Transform tip;

    public ParticleSystem flightParticles;
    private ParticleSystem.EmissionModule _flightEmission;

    // Start is called before the first frame update
    void Start()
    {
        _grabbable = GetComponent<OVRGrabbable>();
        _rigidbody = GetComponent<Rigidbody>();
        _grabbable.GrabChanged += GrabbedChangedHandler;
        _isAttached = false;
        _flightEmission = flightParticles.emission;
        _flightEmission.enabled = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_isFired)
        {
            _rigidbody.MoveRotation(Quaternion.LookRotation(_rigidbody.velocity, transform.up));

            if (Physics.Linecast(_lastPosition, tip.position, out RaycastHit hit, LayerMask.NameToLayer("Grabbable"), QueryTriggerInteraction.Ignore))
            {
                Stop(hit.transform);
            }

            _lastPosition = tip.position;
        }
    }

    private void Stop(Transform hit)
    {
        _isFired = false;
        //turn arrow kinematic
        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;
        //turn on grabbable
        _grabbable.enabled = true;
        //set parent to the hit obj, so that if it moves we move with it
        transform.SetParent(hit);
        _flightEmission.enabled = false;

        NPC npc = hit.GetComponentInParent<NPC>();
        if (npc != null)
        {
            npc.TakeDamage();
            GetComponent<BoxCollider>().enabled = false;
            StartCoroutine(DelayedForce(hit.GetComponent<Rigidbody>()));
        }
    }


    private IEnumerator DelayedForce(Rigidbody rigidbody)
    {
        yield return null; //wait till next frame when rigidbodies are active
        rigidbody.AddForce(transform.forward * 100.0f);
    }

    /// <summary>
    /// Called to attach the arrow to a bow
    /// </summary>
    /// <param name="notch"></param>
    public void Attach(Transform notch)
    {
        //position arrow
        transform.SetParent(notch.transform);
        transform.localPosition = new Vector3(0, 0, 0.425f);
        transform.localRotation = Quaternion.Euler(0, 0, 0);
        //turn off grabbable
        _grabbable.enabled = false;
        //turn arrow kinematic
        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;

        _isAttached = true;
    }

    public void Fire(float pullValue)
    {
        transform.SetParent(null);
        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = true;
        _rigidbody.AddForce(transform.forward * pullValue * speed, ForceMode.VelocityChange);
        _isFired = true;
        _isAttached = false;
        _lastPosition = tip.position;
        flightParticles.Clear();
        StartCoroutine(EnableFlightEmission(0.15f, pullValue));
    }

    private IEnumerator EnableFlightEmission(float waitTime, float distanceRate)
    {
        yield return new WaitForSeconds(waitTime);
        _flightEmission.enabled = true;
        _flightEmission.rateOverDistance = distanceRate;
    }

    private void GrabbedChangedHandler(object sender, PropertyChangedEventArgs e)
    {
        if (!_grabbable.isGrabbed && !_isAttached)
        {
            _rigidbody.isKinematic = false;
            _rigidbody.useGravity = true;
            GetComponent<BoxCollider>().enabled = true;
        }
        else if (_grabbable.isGrabbed && transform.parent != null)
        {
            //detach from obj if we were attached to one
            transform.SetParent(null);
        }
    }
}
