using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoxyJump
{
	public class DeathPlaneDetection : MonoBehaviour 
	{
		public EntityController m_entityController;
		public GameObject m_entity;

		private void Update()
		{
			CheckEntityForCrossingDeathPlane(m_entity);
		}

		private void CheckEntityForCrossingDeathPlane(GameObject entity)
		{
			if (entity.transform.position.y < -1.0f)
			{
				// Fell below death plane
				m_entityController.EntityDied(entity);
			}
		}
	}
}