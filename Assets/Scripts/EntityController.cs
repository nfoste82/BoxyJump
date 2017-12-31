using System.Collections;
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
		public EntityInfoDisplay m_entityDisplay;

		// Amount that a mutation will change a trait
		public float m_mutationRate = 0.1f;

		// Percentage chance that a trait mutates a trait
		public float m_mutationChance = 0.25f;

		public List<KeyValuePair<float, GeneticData>> ScoresToGeneticData { get { return m_scoreToDataMap; } }

		public void EntityDied(GameObject entity)
		{
			// HACK: For now we only support one entity at a time, so m_entity should always be equal for now
			if (m_entity != entity)
			{
				throw new System.ApplicationException("Entity mismatch!");
			}

			float score = entity.transform.position.x;

			m_scoreToDataMap.Add(new KeyValuePair<float, GeneticData>(score, m_generations[m_generations.Count - 1]));
			m_sortedScoreToGenerationMap.Add(new KeyValuePair<float, int>(score, m_generations.Count - 1));

			m_sortedScoreToGenerationMap.Sort((a, b) => b.Key.CompareTo(a.Key));

			Destroy(entity);
			m_entity = null;

			SpawnEntity();
		}

		private void Start()
		{
			// TODO: Do something other than this, this is gross.
			m_entityDisplay = GetComponent<EntityInfoDisplay>();
			m_entityDisplay.m_entityController = this;

			SpawnEntity();
		}

		// TODO: Break up this entire function and break up dependencies. Way too much going on in this function
		private void SpawnEntity()
		{
			if (m_entity != null)
			{
				throw new System.ApplicationException("An entity has already been spawned. There is currently only support for one at a time.");
			}

			m_entity = Instantiate(m_entityPrefab, new Vector3(0.0f, 5.0f), Quaternion.identity);

			// Platform spawner
			m_platformSpawner.m_character = m_entity;

			// Death plane detector
			m_deathDetector.m_entity = m_entity;

			// Generate new genetic data
			GeneticData data = GenerateNextGeneration();

			// Setup the entity
			CharacterInputComponent charInputComp = m_entity.GetComponent<CharacterInputComponent>();
			charInputComp.m_camera = m_mainCamera;

			AIComponent aiComp = m_entity.GetComponent<AIComponent>();
			aiComp.Initialize(data);

			// Entity Display
			m_entityDisplay.NewEntityMade(m_entity, data);
		}

		private GeneticData GenerateNextGeneration()
		{
			GeneticData data;

			if (m_generations.Count == 0)
			{
				data = new GeneticData();
				data.horizontalThrust = 5.0f;
				data.thrustOddsPerSecond = 0.3f;
				data.jumpStrength = 5.0f;
				data.jumpOddsPerSecond = 0.2f;
			}
			else if (m_generations.Count == 1)
			{
				data = m_generations[m_generations.Count - 1];

				data.Mutate(m_mutationChance, m_mutationRate);
			}
			else
			{
				// Mate the two best scores
				GeneticData bestData = m_generations[m_sortedScoreToGenerationMap[0].Value];
				GeneticData nextBestData = m_generations[m_sortedScoreToGenerationMap[1].Value];

				data = bestData.Mate(nextBestData);

				data.Mutate(m_mutationChance, m_mutationRate);
			}

			data.generation = m_generations.Count;
			m_generations.Add(data);

			return data;
		}

		private GameObject m_entity;
		private List<GeneticData> m_generations = new List<GeneticData>();
		private List<KeyValuePair<float, GeneticData>> m_scoreToDataMap = new List<KeyValuePair<float, GeneticData>>();
		private List<KeyValuePair<float, int>> m_sortedScoreToGenerationMap = new List<KeyValuePair<float, int>>();
	}
}