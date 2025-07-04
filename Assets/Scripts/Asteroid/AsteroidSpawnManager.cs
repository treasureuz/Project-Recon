using UnityEngine;

public class AsteroidSpawnManager : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private GameObject _cosmicAsteroidPrefab;
	[SerializeField] private GameObject _dwarfAsteroidPrefab;
	[SerializeField] private Rigidbody2D _rb2d;
	[SerializeField] Transform _player;

	[Header("Spawn Points")]
	[SerializeField] private Transform _spawnPointsParent;
	[SerializeField] private Transform[] _spawnPoints = new Transform[3];

    [Header("Cosmic Spawn Settings")]
	[SerializeField] private float _cosmicSpawnInterval = 5f;

	[Header("Dwarf Spawn Settings")]
	[SerializeField] private float _dwarfSpawnInterval = 2f;

	private GameObject _asteroidPrefab;
	private GameObject _asteroidInstance;
	private Transform _randomSpawnPoint;

	private float _spawnInterval;
	private float _nextSpawnTime = 0f;

	private float _playerStartPositionX;
	private float _playerPositionXDiffFromStart;
	private float _spawnPointStartPositionX;

	private void Awake()
	{
		this._player = GameObject.FindWithTag("Player").transform;
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
			if (InputManager.instance.GetIsMoving()) RedirectSpawnPoints();
		}
	}

	#region Asteroid Type Methods
	private void CosmicAsteroid()
	{
		this._asteroidPrefab = this._cosmicAsteroidPrefab;
		this._spawnInterval = this._cosmicSpawnInterval;
	}

	private void DwarfAsteroid()
	{
		this._asteroidPrefab = this._dwarfAsteroidPrefab;
		this._spawnInterval = this._dwarfSpawnInterval;
	}
	#endregion

	private void SpawnAsteroid()
	{
		DetermineAsteroidTypeToSpawn(); //First determine the type of asteroid to spawn

		int randomSpawnIndex = Random.Range(0, this._spawnPoints.Length);
		if (Time.time >= this._nextSpawnTime)
		{
			this._randomSpawnPoint = this._spawnPoints[randomSpawnIndex];

			this._asteroidInstance = Instantiate(this._asteroidPrefab, this._randomSpawnPoint.position, Quaternion.identity);
			this._nextSpawnTime = Time.time + this._spawnInterval;
		}	
	}

	private void DetermineAsteroidTypeToSpawn()
	{
		int randomAsteroidIndex = Random.Range(0, 4);

		switch (randomAsteroidIndex)
		{
			// 0 and 1 are cosmic asteroids, 
			case 0: 
			case 1: CosmicAsteroid(); break;

			// 2 and 3 are dwarf asteroids
			case 2: 
			case 3: DwarfAsteroid(); break;

		}
	}
	private void RedirectSpawnPoints()
	{
		this._playerPositionXDiffFromStart = this._player.position.x - this._playerStartPositionX;
		
		this._spawnPointsParent.position = new Vector2
			(this._spawnPointStartPositionX + this._playerPositionXDiffFromStart, this._spawnPointsParent.position.y);
	}

}
