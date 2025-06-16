using UnityEngine;

public class LilGuardSpawnManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _copEnemy;

	private void Awake()
	{
		this._copEnemy = GameObject.Find("CopEnemy").transform; 
	}

	private void Start()
	{
		this.transform.position = this._copEnemy.position;
	}

	private void Update()
	{
		if (this._copEnemy != null)
		{
			RedirectSpawnPoints();
		}
	}

	private void RedirectSpawnPoints()
	{
		this.transform.position = new Vector3(this._copEnemy.position.x, this.transform.position.y, this.transform.position.z);
		this.transform.rotation = this._copEnemy.rotation;
	}
}
