using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoxyJump
{
	public class CharacterInputComponent : MonoBehaviour 
	{
		public Rigidbody2D m_rigidBody;
		public Camera m_camera;

		public float m_speed;

		private void Update() 
		{
			Vector3 cameraPosition = m_camera.transform.position;
			cameraPosition.x = transform.position.x;
			m_camera.transform.position = cameraPosition;

			Vector2 startPos = m_rigidBody.transform.position;

			Vector2 endPos = startPos;
			endPos.y -= 0.75f;

			grounded = m_rigidBody.velocity.y <= 0.1f && Physics2D.Linecast(startPos, endPos, 1 << LayerMask.NameToLayer("Ground"));

			Vector2 newVelocity = m_rigidBody.velocity;

			bool changed = false;

			// Move forward
			if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
			{
				changed = true;
				newVelocity.x += m_speed;
			}

			if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
			{
				changed = true;
				newVelocity.x -= m_speed;
			}

			// Jump
 			if (grounded && Input.GetKeyDown(KeyCode.Space))
			{
				changed = true;
				newVelocity.y += 5.0f;
			}

			if (changed)
			{
				m_rigidBody.velocity = newVelocity;
			}
		}

		private bool grounded = false;
	}
}