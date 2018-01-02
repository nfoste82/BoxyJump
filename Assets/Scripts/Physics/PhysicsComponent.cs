using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoxyJump
{
	public class PhysicsComponent : MonoBehaviour 
	{
		public Rigidbody2D m_rigidBody;
		public BoxCollider2D m_collider;

		public bool Grounded { get; private set; }

		public Vector2 Position 
		{ 
			get { return m_rigidBody.position; } 
			set { m_rigidBody.position = value; }
		}

		public Vector2 Velocity 
		{
			get { return m_rigidBody.velocity; }
			set { m_rigidBody.velocity = value; }
		}

		private void Update() 
		{
			Vector2 startPos = m_collider.bounds.center;

			Vector2 endPos = startPos;
			endPos.y = endPos.y - m_collider.bounds.extents.y - 0.1f;

			Grounded = (Mathf.Abs(Velocity.y) <= 0.1f) && Physics2D.Linecast(startPos, endPos, 1 << LayerMask.NameToLayer("Ground"));
		}
	}
}