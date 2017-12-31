using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoxyJump
{
	public class SlowProgressDetection : MonoBehaviour 
	{
		public EntityController m_entityController;

		public float averageDistancePerSecond = 5.0f;
		public float secondsBetweenChecks = 5.0f;

		public void NewEntityMade(GameObject entity)
		{
			m_entity = entity;
			m_totalTime = 0.0f;
			m_timeUntilNextCheck = secondsBetweenChecks;
		}

		private void Start()
		{
			m_distancePerSecond = averageDistancePerSecond;
		}

		private void Update()
		{
			if (m_entity == null)
			{
				return;
			}

			m_timeUntilNextCheck -= Time.deltaTime;
			m_totalTime += Time.deltaTime;

			if (m_timeUntilNextCheck < 0.0f)
			{
				m_timeUntilNextCheck += secondsBetweenChecks;

				CheckEntityForSlowProgress(m_entity);
			}
		}

		private void CheckEntityForSlowProgress(GameObject entity)
		{
			float entityXPos = m_entity.transform.position.x;
			if (entityXPos < (m_distancePerSecond * m_totalTime))
			{
				// Kill entity because of slow progress
				m_entityController.EntityDied(entity);
			}
		}

		private GameObject m_entity;

		private float m_timeUntilNextCheck;
		private float m_totalTime;
		private float m_distancePerSecond;
	}
}