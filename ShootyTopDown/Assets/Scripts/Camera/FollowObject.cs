using UnityEngine;
using System.Collections;

public class FollowObject : MonoBehaviour
{
	public GameObject target;

	private Vector3 offset;

	private Vector3 vel = Vector3.zero;

	protected void Start()
	{
		if (target == null)
			target = ((Component)GameObject.FindObjectOfType(typeof(PlayerCharacter))).gameObject;

		offset = transform.position - target.transform.position;
	}

	protected void FixedUpdate()
	{
		if (target != null)
			transform.position = Vector3.SmoothDamp(transform.position, target.transform.position + offset, ref vel, 0.1f);
	}
}
