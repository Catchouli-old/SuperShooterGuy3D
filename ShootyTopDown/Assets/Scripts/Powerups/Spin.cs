using UnityEngine;
using System.Collections;

public class Spin : MonoBehaviour
{
	private const float SPIN_RATE = 500.0f;

	private Quaternion originalRotation;

	protected void Start()
	{
		originalRotation = transform.rotation;
	}

	protected void Update()
	{
		transform.rotation = Quaternion.AngleAxis(SPIN_RATE * Time.time, Vector3.forward);
	}
}
