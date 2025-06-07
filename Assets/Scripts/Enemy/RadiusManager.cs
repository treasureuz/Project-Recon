using UnityEngine;

public class RadiusManager : MonoBehaviour
{
	private bool _isWithinRadius;

	public bool IsWithinRadius()
	{
		return this._isWithinRadius;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Player"))
		{
			this._isWithinRadius = true;
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.CompareTag("Player"))
		{
			this._isWithinRadius = false;
		}
	}

}
