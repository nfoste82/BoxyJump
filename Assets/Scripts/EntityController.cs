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

		public void Start()
		{
			SpawnEntity();
		}

		private void SpawnEntity()
		{
			m_entity = Instantiate(m_entityPrefab, new Vector3(0.0f, 5.0f), Quaternion.identity);

			CharacterInputComponent charInputComp = m_entity.GetComponent<CharacterInputComponent>();
			charInputComp.m_camera = m_mainCamera;

			m_platformSpawner.m_character = m_entity;
		}

		private GameObject m_entity;
	}
}