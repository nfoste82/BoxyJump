using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoxyJump
{
	public class GeneticData
	{
		public int generation;

		public float horizontalThrust;
		public float thrustOddsPerSecond;

		public float jumpStrength;
		public float jumpOddsPerSecond;

		public GeneticData Mate(GeneticData other)
		{
			GeneticData result = this;

			// 50% chance to use 'other'
			if (UnityEngine.Random.Range(0.0f, 1.0f) > 0.5f)
			{
				result.horizontalThrust = other.horizontalThrust;	
			}

			if (UnityEngine.Random.Range(0.0f, 1.0f) > 0.5f)
			{
				result.thrustOddsPerSecond = other.thrustOddsPerSecond;	
			}

			if (UnityEngine.Random.Range(0.0f, 1.0f) > 0.5f)
			{
				result.jumpStrength = other.jumpStrength;	
			}

			if (UnityEngine.Random.Range(0.0f, 1.0f) > 0.5f)
			{
				result.jumpOddsPerSecond = other.jumpOddsPerSecond;	
			}

			return result;
		}

		public void Mutate(float mutationChance, float mutationRate)
		{
			if (MutateTrait(ref horizontalThrust, mutationChance, mutationRate))
			{
				horizontalThrust = Mathf.Clamp(horizontalThrust, 0.0f, 100.0f);
			}

			if (MutateTrait(ref thrustOddsPerSecond, mutationChance, mutationRate))
			{
				horizontalThrust = Mathf.Clamp(thrustOddsPerSecond, 0.0f, 60.0f);
			}

			if (MutateTrait(ref jumpStrength, mutationChance, mutationRate))
			{
				jumpStrength = Mathf.Clamp(jumpStrength, 0.0f, 100.0f);
			}

			if (MutateTrait(ref jumpOddsPerSecond, mutationChance, mutationRate))
			{
				jumpStrength = Mathf.Clamp(jumpOddsPerSecond, 0.0f, 60.0f);
			}
		}

		private bool MutateTrait(ref float trait, float mutationChance, float mutationRate)
		{
			if (UnityEngine.Random.Range(0.0f, 1.0f) > mutationChance)
			{
				return false;
			}

			float finalMutationRate = GetNextMutationRate(mutationRate);

			trait += (trait * finalMutationRate);

			return true;
		}

		private float GetNextMutationRate(float mutationRate)
		{
			bool positive = (UnityEngine.Random.Range(0.0f, 1.0f) > 0.5f);

			return (positive) ? mutationRate : -mutationRate;
		}
	}

	[RequireComponent(typeof(CharacterInputComponent))]
	public class AIComponent : MonoBehaviour 
	{
		public CharacterInputComponent m_charInputComponent;

		public GeneticData GeneticData { get { return m_geneticData; } }
		public GeneticData m_geneticData;

		public void Initialize(GeneticData data)
		{
			m_geneticData = data;
		}

		private void Update()
		{
			float deltaTime = UnityEngine.Time.deltaTime;

			float thrustOdds = m_geneticData.thrustOddsPerSecond * deltaTime;

			if (UnityEngine.Random.Range(0.0f, 1.0f) < thrustOdds)
			{
				m_charInputComponent.AddThrust(m_geneticData.horizontalThrust);
			}

			float jumpOdds = m_geneticData.jumpOddsPerSecond * deltaTime;

			if (UnityEngine.Random.Range(0.0f, 1.0f) < jumpOdds)
			{
				m_charInputComponent.Jump();
			}
		}
	}
}