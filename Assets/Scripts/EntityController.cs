using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;

namespace BoxyJump
{
	public class EntityController : MonoBehaviour 
	{
		public Camera m_mainCamera;
		public GameObject m_entityPrefab;
		public PlatformSpawner m_platformSpawner;
		public DeathPlaneDetection m_deathDetector;
		public SlowProgressDetection m_slowProgressDetector;
		public EntityInfoDisplay m_entityDisplay;

		// Amount that a mutation will change a trait
		public float m_mutationRate = 0.1f;

		// Percentage chance that a trait mutates a trait
		public float m_mutationChance = 0.25f;

		public List<KeyValuePair<float, GeneticData>> ScoresToGeneticData { get { return m_scoreToDataMap; } }

		public List<KeyValuePair<float, int>> SortedScoreToGenerationMap { get { return m_sortedScoreToGenerationMap; } }

		public float ElapsedTimeForCurrentRun { get { return m_timeForRun; } }

		public void EntityDied(GameObject entity)
		{
			// HACK: For now we only support one entity at a time, so m_entity should always be equal for now
			if (m_entity != entity)
			{
				throw new System.ApplicationException("Entity mismatch!");
			}

			float score = GetScore(entity.transform.position.x);

			m_generations[m_generations.Count - 1] = new KeyValuePair<float, GeneticData>(score, m_generations[m_generations.Count - 1].Value);
			m_scoreToDataMap.Add(new KeyValuePair<float, GeneticData>(score, m_generations[m_generations.Count - 1].Value));
			m_sortedScoreToGenerationMap.Add(new KeyValuePair<float, int>(score, m_generations.Count - 1));

			m_sortedScoreToGenerationMap.Sort((a, b) => b.Key.CompareTo(a.Key));

			Destroy(entity);
			m_entity = null;

			SpawnEntity();
		}

		public List<KeyValuePair<float, GeneticData>> GetTopScores(int numScores)
		{
			numScores = System.Math.Min(numScores, m_sortedScoreToGenerationMap.Count);

			List<KeyValuePair<float, GeneticData>> topScores = new List<KeyValuePair<float, GeneticData>>();

			for (int i = 0; i < numScores; ++i)
			{
				KeyValuePair<float, int> scoreToGen = m_sortedScoreToGenerationMap[i];

				topScores.Add(new KeyValuePair<float, GeneticData>(scoreToGen.Key, m_generations[scoreToGen.Value].Value));
			}

			return topScores;
		}

		public float GetScore(float distance)
		{
			float timeScoreModifier = Mathf.Log10(Mathf.Max(m_timeForRun, 10));

			return distance / timeScoreModifier;
		}

		private void Start()
		{
			SpawnEntity();
		}

		// TODO: Break up this entire function and break up dependencies. Way too much going on in this function
		private void SpawnEntity()
		{
			if (m_entity != null)
			{
				throw new System.ApplicationException("An entity has already been spawned. There is currently only support for one at a time.");
			}

			m_entity = Instantiate(m_entityPrefab, new Vector3(0.0f, 1.0f), Quaternion.identity);

			// Platform spawner
			m_platformSpawner.m_character = m_entity;

			// Death plane detector
			m_deathDetector.NewEntityMade(m_entity);

			// Slow progress detector
			m_slowProgressDetector.NewEntityMade(m_entity);

			// Setup the entity
			CharacterInputComponent charInputComp = m_entity.GetComponent<CharacterInputComponent>();
			charInputComp.m_camera = m_mainCamera;

			AIComponent aiComp = m_entity.GetComponent<AIComponent>();

			// Generate new genetic data
			GeneticData data = GenerateNextGeneration();

			aiComp.Initialize(data);

			// Entity Display
			m_entityDisplay.NewEntityMade(m_entity, data);

			m_timeForRun = 0.0f;
		}

		private GeneticData GenerateNextGeneration()
		{
			GeneticData data;

			if (m_generations.Count == 0)
			{
				data = new GeneticData();
				data.Initialize();
			}
			else if (m_generations.Count == 1)
			{
				data = m_generations[m_generations.Count - 1].Value;
			}
			else
			{
				// Mate the best score with the most recent generation that made some progress
				GeneticData bestData = m_generations[m_sortedScoreToGenerationMap[0].Value].Value;

				GeneticData recentGenData = m_generations[m_generations.Count - 1].Value;

				for (int i = m_generations.Count - 1; i > 0; --i)
				{
					if (m_generations[i].Key > 5.0f)
					{
						recentGenData = m_generations[i].Value;
						break;
					}
				}

				data = bestData.Mate(recentGenData);
			}

			data.Mutate(m_mutationChance, m_mutationRate);
			data.generation = m_generations.Count;
			m_generations.Add(new KeyValuePair<float, GeneticData>(0.0f, data));

			return data;
		}

		private void Update()
		{
			m_timeForRun += Time.deltaTime;
		}

		private GameObject m_entity;
		private float m_timeForRun;

		private List<KeyValuePair<float, GeneticData>> m_generations = new List<KeyValuePair<float, GeneticData>>();
		private List<KeyValuePair<float, GeneticData>> m_scoreToDataMap = new List<KeyValuePair<float, GeneticData>>();
		private List<KeyValuePair<float, int>> m_sortedScoreToGenerationMap = new List<KeyValuePair<float, int>>();
	}
}