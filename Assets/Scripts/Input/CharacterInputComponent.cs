using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoxyJump
{
	public class CharacterInputComponent : MonoBehaviour 
	{
		public PhysicsComponent m_physics;
		public AIComponent m_aiComp;
		public Camera m_camera;

		private void Update() 
		{
			Vector3 cameraPosition = m_camera.transform.position;
			cameraPosition.x = transform.position.x;
			m_camera.transform.position = cameraPosition;

			// Move forward
			if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
			{
				AddThrust(5.0f);
			}

			if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
			{
				AddThrust(-5.0f);
			}

			// Jump
			if (Input.GetKeyDown(KeyCode.Space))
			{
  				Jump(Vector2.up * 5.0f);
			}

			ApplyInputs();
		}

		public void ApplyInputs()
		{
			Vector2 newVelocity = m_physics.Velocity;
			bool change = false;

			if (m_thrust.HasValue)
			{
				newVelocity.x += m_thrust.Value;

				newVelocity.x = Mathf.Clamp(newVelocity.x, -m_thrust.Value, m_thrust.Value);

				change = true;
				m_thrust = null;
			}

			if (m_jump.HasValue)
			{
				newVelocity += m_jump.Value;

				change = true;
				m_jump = null;
			}

			if (change)
			{
				m_physics.Velocity = newVelocity;
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

		public void Jump(Vector2 velocity)
		{
			if (!m_physics.Grounded || m_jump != null)
			{
				return;
			}

			if (m_jump == null)
			{
				m_jump = velocity;
			}
			else
			{
				m_jump += velocity;

				if (m_jump.Value == Vector2.zero)
				{
					m_jump = null;
				}
			}
		}
	
		public float? m_thrust;
		public Vector2? m_jump;
	}
}