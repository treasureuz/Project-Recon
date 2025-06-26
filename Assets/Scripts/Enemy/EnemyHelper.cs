using System.Collections.Generic;
using UnityEngine;

public class EnemyHelper : MonoBehaviour
{
	private List<GameObject> _enemyList = new List<GameObject>();

	private void Update()
	{
		Debug.Log("Enemies in list: " + GetEnemyList().Count);
	}

	public void AddEnemyToList(GameObject newEnemy)
	{
		if (!this._enemyList.Contains(newEnemy))
		{
			this._enemyList.Add(newEnemy);
		}
	}

	public void RemoveEnemyFromList(GameObject enemyToRemove)
	{
		if (this._enemyList.Contains(enemyToRemove))
		{
			this._enemyList.Remove(enemyToRemove);
		}
	}

	public List<GameObject> GetEnemyList()
	{
		return this._enemyList;
	}
}
