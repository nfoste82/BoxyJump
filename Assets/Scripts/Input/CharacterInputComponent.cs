using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoxyJump
{
	public class CharacterInputComponent : MonoBehaviour 
	{
		public bool Grounded { get; private set; }

		public Rigidbody2D m_rigidBody;
		public Camera m_camera;

		private void Update() 
		{
			Vector3 cameraPosition = m_camera.transform.position;
			cameraPosition.x = transform.position.x;
			m_camera.transform.position = cameraPosition;

			Vector2 startPos = m_rigidBody.transform.position;

			Vector2 endPos = startPos;
			endPos.y -= 0.75f;

			Grounded = m_rigidBody.velocity.y <= 0.1f && Physics2D.Linecast(startPos, endPos, 1 << LayerMask.NameToLayer("Ground"));

			AIComponent aiComp = GetComponent<AIComponent>();

			// Move forward
			if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
			{
				AddThrust(aiComp.GeneticData.horizontalThrust);
			}

			if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
			{
				AddThrust(-aiComp.GeneticData.horizontalThrust);
			}

			// Jump
			if (Grounded && Input.GetKeyDown(KeyCode.Space))
			{
				Jump();
			}

			ApplyInputs();
		}

		public void ApplyInputs()
		{
			Vector2 newVelocity = m_rigidBody.velocity;
			bool change = false;

			AIComponent aiComp = GetComponent<AIComponent>();

			if (m_thrust.HasValue)
			{
				newVelocity.x += m_thrust.Value;

				newVelocity.x = Mathf.Clamp(newVelocity.x, -aiComp.GeneticData.horizontalThrust, aiComp.GeneticData.horizontalThrust);

				change = true;
				m_thrust = null;
			}

			if (m_jump)
			{
				newVelocity.y += aiComp.GeneticData.jumpStrength;

				change = true;
				m_jump = false;
			}

			if (change)
			{
				m_rigidBody.velocity = newVelocity;
			}
		}

		public void AddThrust(float thrust)
		{
			if (m_thrust == null)
			{
				m_thrust = thrust;
			}
			else
			{
				m_thrust += thrust;

				if (m_thrust.Value == 0.0f)
				{
					m_thrust = null;
				}
			}
		}

		public void Jump()
		{
			if (m_jump)
			{
				return;
			}

			if (Grounded)
			{
				m_jump = true;
			}
		}
	
		public float? m_thrust;
		public bool m_jump;
	}
}