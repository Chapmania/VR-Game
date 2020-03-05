using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    private Rigidbody[] _allRigidbodies;
    private Animator _anim;

    private Vector3 _defaultPos;

    void Awake()
    {
        _allRigidbodies = GetComponentsInChildren<Rigidbody>(true);
        _anim = GetComponent<Animator>();

        _defaultPos = transform.position;

        //initially, make sure it's not in ragdoll mode
        SetRagdoll(false);
    }

    /// <summary>
    /// Turns on ragdoll effect and turns off animator
    /// </summary>
    private void SetRagdoll(bool isRagdoll)
    {
        _anim.enabled = !isRagdoll;
        foreach(Rigidbody r in _allRigidbodies)
        {
            r.isKinematic = !isRagdoll;
        }
    }

    /// <summary>
    /// Take damage from attack
    /// </summary>
    public void TakeDamage()
    {
        //currently 1 hit kills
        SetRagdoll(true);

    }

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            transform.position = _defaultPos;
            transform.rotation = Quaternion.identity;
            SetRagdoll(false);
        }
    }
}
