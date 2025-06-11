using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEngine;

public class AsteriodSpawnManager : MonoBehaviour
{
    [Header("References")]
	[SerializeField] private GameObject _asteroidPrefab;
	[SerializeField] private Rigidbody2D _rb2d;
	[SerializeField] Transform _player;

	[Header("Spawn Points")]
	[SerializeField] private Transform _spawnPointsParent;
	[SerializeField] private Transform[] _spawnPoints = new Transform[3];

    [Header("Spawn Settings")]
	[SerializeField] private float _spawnInterval = 5f; // Time in seconds between spawns

	private GameObject _asteroidInstance;
	
	private Transform _randomSpawnPoint;

	private float _nextSpawnTime = 0f;

	private float _playerStartPositionX;
	private float _playerPositionXDiffFromStart;
	private float _spawnPointStartPositionX;

	private void Awake()
	{
		//this._spawnPoints = this._spawnPointsParent.GetComponentsInChildren<Transform>();
	}

	private void Start()
	{
		this._playerStartPositionX = this._player.position.x;
		this._spawnPointStartPositionX = this._spawnPointsParent.position.x;
	}

	private void Update()
	{
		if (this._player != null)
		{
			SpawnAsteroid();
			if (InputManager.instance._isMoving) RedirectSpawnPoints();
		}
	}
	
	private void SpawnAsteroid()
	{
		int randomSpawnIndex = Random.Range(0, this._spawnPoints.Length);
		if (Time.time >= this._nextSpawnTime)
		{
			this._randomSpawnPoint = this._spawnPoints[randomSpawnIndex];

			this._asteroidInstance = Instantiate(this._asteroidPrefab, this._randomSpawnPoint.position, Quaternion.identity);
			this._nextSpawnTime = Time.time + this._spawnInterval;
		}	
	}

	private void RedirectSpawnPoints()
	{
		this._playerPositionXDiffFromStart = this._player.position.x - this._playerStartPositionX;
		
		this._spawnPointsParent.position = new Vector2
			(this._spawnPointStartPositionX + this._playerPositionXDiffFromStart, this._spawnPointsParent.position.y);
	}

}
