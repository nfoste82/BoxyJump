using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace BoxyJump
{
	// When a RECEPTOR senses something, a RESPONSE for that sense may exist.
	// If a REACTION exists then it may trigger a RESPONSE.

	public enum ReceptorType
	{
		Thought,	// This receptor fires whenever the AI thinks (which is always happening, once per frame)
		//Eyes,	// Further in front
		Feet,	// Bottom
		//Hands	// Front
	}

	public enum ResponseType
	{
		Thrust,
		Jump,
	}

	public static class ResponseFactory
	{
		public static Response Generate(ResponseType responseType)
		{
			switch (responseType)
			{
			case ResponseType.Thrust: 
				return new ThrustResponse(true);
			case ResponseType.Jump: 
				return new JumpResponse(true);
			default: throw new System.ArgumentOutOfRangeException("responseType");
			}
		}

		public static Response Clone(Response original)
		{
			Type originalType = original.GetType();

			Response response = null;

			if (originalType == typeof(ThrustResponse))
			{
				response = new ThrustResponse(false);
			}
			else if (originalType == typeof(JumpResponse))
			{
				response = new JumpResponse(false);	
			}
			else
			{
				throw new System.ApplicationException("Type of response not handled by ResponseFactory.Clone()");
			}

			response.data = original.data;
			return response;
		}

		public static Response GenerateRandomResponse()
		{
			Array values = Enum.GetValues(typeof(ResponseType));
			ResponseType randomType = (ResponseType)values.GetValue(UnityEngine.Random.Range(0, values.Length));

			return Generate(randomType);
		}
	}

	public struct ResponseData
	{
		public float odds;
		public float amount;
		public float secondaryValue;
	}

	public static class ResponseExtensions
	{
		public static T Clone<T>(this T self) where T : Response
		{
			return (T)ResponseFactory.Clone(self);
		}
	}

	public abstract class Response
	{
		public ResponseData data;

		public virtual bool RequiresGroundContact { get { return false; } }

		public abstract void Fire(CharacterInputComponent charComponent);

		public abstract void ClampAmount();

		public abstract void ClampOdds();

		public virtual void ClampSecondaryValue() {}

		public void Mutate(float mutationRate)
		{
			MutateValue(ref data.odds, mutationRate);
			ClampOdds();

			MutateValue(ref data.amount, mutationRate);
			ClampAmount();

			MutateValue(ref data.secondaryValue, mutationRate);
			ClampSecondaryValue();
		}

		private void MutateValue(ref float val, float mutationRate)
		{
			float finalMutationRate = GetNextMutationRate(mutationRate);

			val += (val * finalMutationRate);
		}

		private float GetNextMutationRate(float mutationRate)
		{
			bool positive = (UnityEngine.Random.Range(0.0f, 1.0f) > 0.5f);

			return (positive) ? mutationRate : -mutationRate;
		}
	}

	public class ThrustResponse : Response
	{
		public ThrustResponse(bool generatedRandomly)
		{
			if (generatedRandomly)
			{
				data.amount = UnityEngine.Random.Range(4.0f, 7.0f);
				data.odds = UnityEngine.Random.Range(1.3f, 2.0f);
			}
		}

		public override void Fire(CharacterInputComponent charComponent)
		{
			charComponent.AddThrust(data.amount);
		}

		public override void ClampAmount()
		{
			data.amount = Mathf.Clamp(data.amount, 0.0f, 20.0f);
		}

		public override void ClampOdds()
		{
			data.odds = Mathf.Clamp(data.odds, 0.0f, 60.0f);
		}
	}

	public class JumpResponse : Response
	{
		public override bool RequiresGroundContact { get { return true; } }

		public float JumpAngle { get { return data.secondaryValue; } }

		public JumpResponse(bool generatedRandomly)
		{
			if (generatedRandomly)
			{
				data.amount = UnityEngine.Random.Range(3.0f, 6.0f);
				data.odds = UnityEngine.Random.Range(0.6f, 1.2f);
				data.secondaryValue = UnityEngine.Random.Range(45.0f, 90.0f);
			}
		}

		public override void Fire(CharacterInputComponent charComponent)
		{
			Vector2 jumpVector = VectorUtils.Vector2FromAngleDegrees(JumpAngle);

			charComponent.Jump(jumpVector * data.amount);
		}

		public override void ClampAmount()
		{
			data.amount = Mathf.Clamp(data.amount, 1.5f, 100.0f);
		}

		public override void ClampOdds()
		{
			data.odds = Mathf.Clamp(data.odds, 0.0f, 60.0f);
		}

		public override void ClampSecondaryValue()
		{
			data.secondaryValue = Mathf.Clamp(data.secondaryValue, 0.0f, 90.0f);
		}
	}

	public struct GeneticData
	{
		public int generation;

		public HashSet<ReceptorType> m_receptors;
		public Dictionary<ReceptorType, Response> m_responses;

		public void Initialize()
		{
			m_receptors = new HashSet<ReceptorType>();
			m_responses = new Dictionary<ReceptorType, Response>();
		}

		public List<Response> Update(float deltaTime, PhysicsComponent physics)
		{
			List<Response> responses = null;

			foreach (var receptor in m_receptors)
			{
				Response response;
				if (!m_responses.TryGetValue(receptor, out response))
				{
					continue;
				}

				// Response didn't fire because odds weren't good enough
				if (UnityEngine.Random.Range(0.0f, 1.0f) >= (response.data.odds * deltaTime))
				{
					continue;
				}

				bool receptorFired = false;
				switch (receptor)
				{
				//case ReceptorType.Eyes: break;
				case ReceptorType.Feet:
					// Entity being on the ground fires the feet receptor
					receptorFired = physics.Grounded;
					break;
				//case ReceptorType.Hands: break;
				case ReceptorType.Thought: 
					receptorFired = true;
					break;
				}

				if (receptorFired)
				{
					if (response.RequiresGroundContact && !physics.Grounded)
					{
						continue;
					}

					if (responses == null)
					{
						responses = new List<Response>();
					}
					responses.Add(response);
				}
			}

			return responses;
		}

		public GeneticData(int generation)
		{
			this.generation = generation;

			m_receptors = new HashSet<ReceptorType>();
			m_responses = new Dictionary<ReceptorType, Response>();
		}

		public GeneticData Mate(GeneticData other)
		{
			GeneticData result = new GeneticData(this.generation);

			// If both parents have a receptor, child gets it.
			// If one parent has receptor, child has a 50% chance.
			// If no parents have receptor then child can't get it from parent (but can through mutation)
			foreach (var receptorType in (ReceptorType[])Enum.GetValues(typeof(ReceptorType)))
			{
				float oddsToGainReceptor = 0.0f;

				if (m_receptors.Contains(receptorType))
				{
					oddsToGainReceptor += 0.5f;
				}

				if (other.m_receptors.Contains(receptorType))
				{
					oddsToGainReceptor += 0.5f;
				}

				if (oddsToGainReceptor == 0.0f)
				{
					continue;
				}

				if (oddsToGainReceptor == 1.0f || UnityEngine.Random.Range(0.0f, 1.0f) < oddsToGainReceptor)
				{
					result.m_receptors.Add(receptorType);
				}
			}

			// Child has chance to get response to receptor from either parent that has one.
			foreach (var receptorType in (ReceptorType[])Enum.GetValues(typeof(ReceptorType)))
			{
				bool parentAHasResponse = m_responses.ContainsKey(receptorType);
				bool parentBHasResponse = other.m_responses.ContainsKey(receptorType);

				if (!parentAHasResponse && !parentBHasResponse)
				{
					continue;
				}

				// TODO: Cleanup this nested branching below
				if (parentAHasResponse)
				{
					if (parentBHasResponse)
					{
						// Roll to determine which parent contributes, less than 0.5 takes ParentA
						if (UnityEngine.Random.Range(0.0f, 1.0f) < 0.5f)
						{
							result.m_responses.Add(receptorType, m_responses[receptorType].Clone());
						}
						else
						{
							result.m_responses.Add(receptorType, other.m_responses[receptorType].Clone());
						}
					}
					else
					{
						result.m_responses.Add(receptorType, m_responses[receptorType].Clone());
					}
				}
				else if (parentBHasResponse)
				{
					result.m_responses.Add(receptorType, other.m_responses[receptorType].Clone());
				}
			}

			return result;
		}

		public void Mutate(float mutationChance, float mutationRate)
		{
			MutateRandomReceptor(mutationChance, mutationRate);

			MutateResponses(mutationChance, mutationRate);
		}

		private void MutateRandomReceptor(float mutationChance, float mutationRate)
		{
			// If the entity has no receptors then there's a 100% to get one.
			if (m_receptors.Any())
			{
				if (UnityEngine.Random.Range(0.0f, 1.0f) > mutationChance)
				{
					return;
				}	
			}

			ReceptorType[] receptorTypes = (ReceptorType[])Enum.GetValues(typeof(ReceptorType));
			int index = UnityEngine.Random.Range(0, receptorTypes.Length);
			SwapHasReceptor(receptorTypes[index]);
		}

		private void SwapHasReceptor(ReceptorType receptorType)
		{
			// If we had this receptor, lose it. Otherwise, gain this receptor.
			if (m_receptors.Contains(receptorType))
			{
				m_receptors.Remove(receptorType);
			}
			else
			{
				m_receptors.Add(receptorType);
			}
		}

		private void MutateResponses(float mutationChance, float mutationRate)
		{
			// Chance to lose a response, gain a response, or mutate a response

			float chanceToGainOrAlterResponse = 0.666f;
			if (m_responses.Any())
			{
				if (UnityEngine.Random.Range(0.0f, 1.0f) > mutationChance)
				{
					return;
				}
			}
			else
			{
				chanceToGainOrAlterResponse = 1.0f;
			}

			ReceptorType[] receptorTypes = m_receptors.ToArray();
			int index = UnityEngine.Random.Range(0, receptorTypes.Length);
			ReceptorType receptorType = receptorTypes[index];

			// Gain or alter a response
			if (UnityEngine.Random.Range(0.0f, 1.0f) < chanceToGainOrAlterResponse)
			{
				Response response;
				if (m_responses.TryGetValue(receptorType, out response))
				{
					// Alter response to receptor
					m_responses[receptorType].Mutate(mutationRate);
				}
				else
				{
					// Gain response for receptor
					response = ResponseFactory.GenerateRandomResponse();

					// Add response
					m_responses.Add(receptorType, response);
				}
			}
			else
			{
				m_responses.Remove(receptorType);
			}
		}

		private void AlterReaction(ReceptorType receptorType, float mutationRate)
		{
			Response response;
			if (!m_responses.TryGetValue(receptorType, out response))
			{
				response = ResponseFactory.GenerateRandomResponse();

				// Add response
				m_responses.Add(receptorType, response);
			}
			else
			{
				// Either lose response or alter response

				// Lose response
				if (UnityEngine.Random.Range(0.0f, 1.0f) < 0.5f)
				{
					m_responses.Remove(receptorType);
				}
				else
				{
					// Mutate existing response
					m_responses[receptorType].Mutate(mutationRate);
				}
			}
		}
	}

	[RequireComponent(typeof(CharacterInputComponent))]
	public class AIComponent : MonoBehaviour 
	{
		public CharacterInputComponent m_charInputComponent;
		public PhysicsComponent m_physicsComponent;

		public GeneticData GeneticData { get { return m_geneticData; } }
		public GeneticData m_geneticData;

		public void Initialize(GeneticData data)
		{
			m_geneticData = data;
		}

		private void Update()
		{
			float deltaTime = UnityEngine.Time.deltaTime;

			List<Response> responses = m_geneticData.Update(deltaTime, m_physicsComponent);

			if (responses == null)
			{
				return;
			}

			foreach (Response response in responses)
			{
				response.Fire(m_charInputComponent);	
			}
		}
	}
}