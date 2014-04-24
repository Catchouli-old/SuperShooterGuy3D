using UnityEngine;
using System.Collections;

public class GUICompass : MonoBehaviour
{
	private float MAX_DISTANCE_COMPASS = 60.0f;

	private float MIN_SCALE_COMPASS = 0.3f;
	private float MAX_SCALE_COMPASS = 2.0f;

	public Texture2D compassTexture;

	protected void OnGUI()
	{
		Camera mainCamera = Camera.main;

		// Get character position in viewport space
		Vector3 screenPos = mainCamera.WorldToViewportPoint(transform.position);
		bool onScreen = screenPos.x > 0 && screenPos.x < 1 &&
			screenPos.y > 0 && screenPos.y < 1;

		if (onScreen)
		{
			return;
		}

		// Get camera position
		Vector3 camPos = mainCamera.transform.position;

		// Calculate distance from camera to current position
		Vector3 diff = transform.position - camPos;
		float distanceSqr = diff.sqrMagnitude;

		if (distanceSqr > MAX_DISTANCE_COMPASS * MAX_DISTANCE_COMPASS)
		{
			return;
		}

		// Calculate distance from camera to current position
		float distance = Mathf.Sqrt(distanceSqr);

		// Calculate direction to camera position
		Vector3 dir = diff / distance;

		// Calculate scale based on distance
		float iconScale = Mathf.Lerp(MAX_SCALE_COMPASS, MIN_SCALE_COMPASS, distance / MAX_DISTANCE_COMPASS);

		// Save GUI matrix for later restoration
		Matrix4x4 oldMatrix = GUI.matrix;

		// Reverse angle of quaternion
		Quaternion rotation = Quaternion.LookRotation(dir, Vector3.back);
		rotation.w = -rotation.w;
		rotation = Quaternion.Euler(0, 0, rotation.eulerAngles.z);

		// Get smallest screen dimension
		float smallestDimension = Screen.width < Screen.height ? Screen.width : Screen.height;
		float halfSmallestDimension = 0.5f * smallestDimension;
		float compassIconDistanceFromCentre = halfSmallestDimension - compassTexture.width;

		// Apply translation to matrices
		GUI.matrix = Matrix4x4.identity;
		GUI.matrix *= translate(new Vector3(0.5f * Screen.width, 0.5f * Screen.height, 0));
		GUI.matrix *= rotate(rotation);
		GUI.matrix *= translate(new Vector3(0, -compassIconDistanceFromCentre, 0));
		GUI.matrix *= scale(new Vector3(iconScale, iconScale, 1));

		// Draw icon
		float halfCompassWidth = compassTexture.width * 0.5f;
		float halfCompassHeight = compassTexture.height * 0.5f;
		Rect compassIconRect = new Rect(-halfCompassWidth, -halfCompassHeight, compassTexture.width, compassTexture.height);

		GUI.DrawTexture(compassIconRect, compassTexture);

		// Restore gui matrix
		GUI.matrix = oldMatrix;
	}
	
	private Matrix4x4 translate(Vector3 translation)
	{
		return Matrix4x4.TRS(translation, Quaternion.identity, Vector3.one);
	}
	
	private Matrix4x4 rotate(Quaternion rotation)
	{
		return Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
	}
	
	private Matrix4x4 scale(Vector3 scale)
	{
		return Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
	}
}
