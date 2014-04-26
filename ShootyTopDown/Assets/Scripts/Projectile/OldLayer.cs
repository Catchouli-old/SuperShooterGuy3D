using UnityEngine;

public class OldLayer
	: MonoBehaviour
{
	public LayerMask oldlayer;
	
	public void ChangeObjectLayer(int layer)
	{
		foreach (Transform trans in gameObject.GetComponentsInChildren<Transform>(true))
		{
			trans.gameObject.layer = layer;
		}
	}
	
	public void ChangeObjectLayermask(LayerMask newMask)
	{
		foreach (Transform trans in gameObject.GetComponentsInChildren<Transform>(true))
		{
			trans.gameObject.layer = (int)Mathf.Log(newMask.value, 2);
		}
	}
	
	public void RestoreObjectLayer()
	{
		foreach (Transform trans in gameObject.GetComponentsInChildren<Transform>(true))
		{
			trans.gameObject.layer = oldlayer;
		}
	}
};