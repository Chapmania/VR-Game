using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : MonoBehaviour
{
    private OVRGrabbable _bowGrabbable;
    private GameObject _bowController = null;
    private GameObject _pullController = null;
    private OVRInput.Controller _pullKey;
    private Arrow _attachedArrow = null;

    private GameObject _leftController;
    private GameObject _rightController;

    private Animator _anim;

    private bool _pulling = false;

    public Transform pullStart;
    public Transform pullEnd;
    public Transform notch;


    // Start is called before the first frame update
    void Start()
    {
        _bowGrabbable = GetComponent<OVRGrabbable>();

        _leftController = GameObject.FindGameObjectWithTag("Left Controller");
        _rightController = GameObject.FindGameObjectWithTag("Right Controller");

        _anim = GetComponent<Animator>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_bowGrabbable.isGrabbed)
        {
            if (_bowController != _bowGrabbable.grabbedBy.gameObject) //make sure controllers are setup
            {
                _bowController = _bowGrabbable.grabbedBy.gameObject;
                if (_bowController == _leftController)
                {
                    _pullController = _rightController;
                    _pullKey = OVRInput.Controller.RTouch;
                }
                else
                {
                    _pullController = _leftController;
                    _pullKey = OVRInput.Controller.LTouch;
                }
            }


            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, _pullKey))
                Pull();
            if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, _pullKey))
                Release();

            if (_pulling)
            {
                _anim.SetFloat("Blend", CalculatePull());
            }
            else
            {
                _anim.SetFloat("Blend", Mathf.Lerp(_anim.GetFloat("Blend"), 0.0f, Time.deltaTime * 25.0f)); //if not being pulled, reset back to unpulled state
            }
        }
        else if (_bowController != null)  //reset controllers bow must have been dropped
        {
            Release();
            _bowController = null;
            _pullController = null;
            _pullKey = OVRInput.Controller.None;
        }
    }

    private float CalculatePull()
    {
        if(_pullController == null) return 0.0f;

        Vector3 direction = pullEnd.position - pullStart.position;
        float magnitude = direction.magnitude;

        direction.Normalize();
        Vector3 difference = _pullController.transform.position - pullStart.position;

        return Mathf.Clamp(Vector3.Dot(difference, direction) / magnitude, 0.0f, 1.0f);
    }

    private void Pull()
    {
        float distance = Vector3.Distance(_pullController.transform.position, pullStart.position); //check distance from pull controller to socket

        if(distance < 0.5f)  //if close enough initiate pulling
        {
            _pulling = true;

            //if controller holding arrow, attach it to bow and remove it from the controller
            OVRGrabber pullGrabber = _pullController.GetComponent<OVRGrabber>();
            if (pullGrabber.grabbedObject != null)
            {
                _attachedArrow = pullGrabber.grabbedObject.GetComponent<Arrow>();
                pullGrabber.ForceRelease(_attachedArrow.GetComponent<OVRGrabbable>());
                _attachedArrow.Attach(notch);
            }

            //disable the grabber so they can't grab things while pulling bow string
            pullGrabber.enabled = false;
        }

    }

    private void Release()
    {
        if (_pulling)
        {
            _pulling = false;

            //if bow had an arrow attached, fire it
            if(_attachedArrow != null)
            {
                _attachedArrow.Fire(CalculatePull());
                _attachedArrow = null;
            }

            //re-enable grabber
            OVRGrabber pullGrabber = _pullController.GetComponent<OVRGrabber>();
            pullGrabber.enabled = true;
        }
    }
}
