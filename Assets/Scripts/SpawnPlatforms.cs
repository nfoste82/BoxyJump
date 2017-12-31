using UnityEngine;
using System.Collections.Generic;

namespace BoxyJump
{
	public class SpawnPlatforms : MonoBehaviour 
	{
		public GameObject m_character;
		public GameObject m_platform;
		public float maxHorizontalRange = 20.0f;

		public float m_oddsOfPit = 0.2f;

		private void Start()
		{
			m_randSeed = System.DateTime.UtcNow.Ticks / System.TimeSpan.TicksPerSecond;

			Vector3 charPosition = m_character.transform.position;
			Vector3 backPosition = charPosition;
			backPosition.x -= 10.0f;

			SpawnGroundPlatforms(backPosition);

			SpawnGroundPlatforms(charPosition);
		}

		private void Update()
		{
			if (Mathf.Abs(m_character.transform.position.x - m_lastPosition.x) > 10.0f)
			{
				m_lastPosition = m_character.transform.position;

				UpdatePlatforms();
			}
		}

		private void UpdatePlatforms()
		{
			Vector3 charPosition = m_character.transform.position;

			List<int> keysToRemove = new List<int>();
			foreach (var platformKVP in m_spawnedPlatforms)
			{
				int platformX = (int)(platformKVP.Value.transform.position.x);

				if (Mathf.Abs(charPosition.x - platformX) > maxHorizontalRange)
				{
					Destroy(platformKVP.Value);

					keysToRemove.Add(platformX);
				}
			}

			foreach (int key in keysToRemove)
			{
				m_spawnedPlatforms.Remove(key);
			}

			SpawnGroundPlatforms(charPosition);
		}

		private void SpawnGroundPlatforms(Vector3 charPosition)
		{
			// Spawn ground platforms

			// 1x2 platforms every 2 units

			int nearestEvenX = Mathf.RoundToInt(charPosition.x);
			if (nearestEvenX % 2 != 0)
			{
				++nearestEvenX;
			}

			// No pits near the spawn
			bool pitsAllowed = !(nearestEvenX < maxHorizontalRange && nearestEvenX > -maxHorizontalRange);

			for (int i = nearestEvenX - 20; i < nearestEvenX + 20; i += 2)
			{
				if (m_spawnedPlatforms.ContainsKey(i))
				{
					continue;
				}

				if (pitsAllowed)
				{
					int seedForX = System.Math.Abs(m_randSeed ^ i);
					var randGen = new System.Random(seedForX);

					if (randGen.NextDouble() < m_oddsOfPit)
					{
						// Pit here, so spawn nothing.
						continue;
					}
				}

				Vector2 spawnPosition = new Vector2(i, 0.0f);
				var platform = Instantiate(m_platform, spawnPosition, Quaternion.identity);

				m_spawnedPlatforms.Add(i, platform);
			}
		}

		private Dictionary<int, GameObject> m_spawnedPlatforms = new Dictionary<int, GameObject>();
		private Vector3 m_lastPosition;
		private int m_randSeed;
	}
}