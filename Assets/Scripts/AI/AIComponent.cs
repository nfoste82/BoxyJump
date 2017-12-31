using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoxyJump
{
	public struct GeneticData
	{
		public float horizontalThrust;
		public float thrustOddsPerSecond;

		public float jumpStrength;
		public float jumpOddsPerSecond;
	}

	[RequireComponent(typeof(CharacterInputComponent))]
	public class AIComponent : MonoBehaviour 
	{
		public CharacterInputComponent m_charInputComponent;

		private GeneticData m_geneticData;

		private void Start()
		{
			// Horz thrust
			m_geneticData.horizontalThrust = 5.0f;
			m_geneticData.thrustOddsPerSecond = 0.3f;

			// Jump spring
			m_geneticData.jumpStrength = 5.0f;
			m_geneticData.jumpOddsPerSecond = 0.2f;
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