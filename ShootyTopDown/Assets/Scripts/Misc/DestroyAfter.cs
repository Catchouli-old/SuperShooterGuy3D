using UnityEngine;
using System.Collections;

public class DestroyAfter : MonoBehaviour
{
	public float time = 0.0f;

	private float startTime;

	void Start()
	{
		startTime = Time.time;
	}

	void Update()
	{
		if (Time.time - startTime >= time)
		{
			GameObject.Destroy(gameObject);
		}
	}
}
