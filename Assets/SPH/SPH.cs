using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// https://imdoingitwrong.wordpress.com/2010/12/14/why-my-fluids-dont-flow/
// https://nccastaff.bournemouth.ac.uk/jmacey/MastersProjects/MSc11/Rajiv/MasterThesis.pdf
// https://www8.cs.umu.se/kurser/TDBD24/VT06/lectures/sphsurvivalkit.pdf
// http://www.essentialmath.com/GDC2010/VanVerth_Fluids10.pdf
// http://www.cs.umd.edu/class/fall2009/cmsc828v/presentations/Presentation1_Sep15.pdf
// http://rlguy.com/sphfluidsim/

class Particle
{
	public Particle (Vector2 pos, Vector2 vel = default(Vector2))
	{
		position = pos;
		velocity = vel;
	}

	public Vector2 position = Vector2.zero;
	public Vector2 oldPosition = Vector2.zero;
	public Vector2 velocity = Vector2.zero;
	public Vector2 oldAcceleration = Vector2.zero;
	public Vector2 force = Vector2.zero;
	public float pressure = 0;
	public float density = 0;
}

public class SPH : MonoBehaviour
{
	public int numParticles = 100;
	[Range (20f, 100f)]
	public float kStiffness = 60f;


	public float mass = 28.0f;
	public float radius = 0.2f;
	public float smoothingRadius = 0.6f;
	public float viscosity = 0.7f;
	public float restDensity = 82.0f;


	public Vector2 bounds = new Vector2 (10, 8);


	List<Particle> particles = new List<Particle> ();
	Vector2 gravity = Physics.gravity;

	public bool usePressureForce = true;
	public bool useViscosityForce = true;
	public bool useGravityForce = false;

	/*-------*/
	/* Debug */
	/*-------*/
	Text txtDebug;
	Particle selectedDebugParticle;
	public bool bDrawForce = true;
	public bool bDrawSmoothingRadius = true;

	public Camera camera;

	void Start ()
	{
		txtDebug = GameObject.Find ("DebugText").GetComponent<Text> ();

		smoothingRadius = radius * 6;
		RestartSimulation ();
	}

	public void RestartSimulation ()
	{
		if (particles.Count != 0) {
			particles.Clear ();
		}

		int side = (int)Mathf.Sqrt (numParticles);

		float dx = smoothingRadius / 1.5f;
		for (int i = 0; i < side; i++) {
			for (int j = 0; j < side; j++) {
				Vector2 pos = new Vector2 (i * dx, j * dx) - new Vector2 (side * dx, side * dx) / 2;
				Vector2 vel = new Vector2 (Random.Range (-1f, 1f), Random.Range (-1f, 1f)) * 0.00f;

				Particle particle = new Particle (pos, vel);
				particles.Add (particle);
			}
		}

	}

	void FixedUpdate ()
	{
		Simulate (Time.fixedDeltaTime);
	}

	void Update ()
	{
		if (Input.GetMouseButton (1) && camera) {
			Vector2 mousePos = camera.ScreenToWorldPoint (Input.mousePosition);


			foreach (Particle p in particles) {
				Vector2 toMouseDir = mousePos - p.position;
				
				if (toMouseDir.sqrMagnitude < smoothingRadius * smoothingRadius) {
					selectedDebugParticle = p;
				}
			}

		}

		DrawDebug ();

		if (selectedDebugParticle != null) {			
			DrawDebugParticleText (selectedDebugParticle);
		}
	}

	int GetParticleCount ()
	{
		return particles.Count;
	}

	void Simulate (float dt)
	{
		/* Compute Density and pressure */		
		foreach (Particle p0 in particles) {

			p0.density = 0.0f;

			foreach (Particle p1 in particles) {		
				if (p0 == p1)
					continue;

				float distSqr = (p0.position - p1.position).sqrMagnitude;

				if (distSqr <= smoothingRadius * smoothingRadius) {
					p0.density += mass * Kernels.Poly6 (distSqr, smoothingRadius);
				}
			}

			p0.density = Mathf.Max (p0.density, restDensity);

			ComputeParticlePressure (p0);
		}


		/* Compute forces */
		foreach (Particle particle in particles) {
			particle.force = Vector2.zero;

			if (usePressureForce == true)
				PressureForce (particle);

			if (useViscosityForce == true)
				ViscosityForce (particle);

			ExternalForces (particle);

		}

		/* Integrate motion */

		// p(t+∆t) = p(t) + v(t)∆t + F(t)∆t2 / 2m
		// v(t) = ( p(t) – p(t–∆t) ) / ∆t
		foreach (Particle particle in particles) {
			ForceBounds (particle);

			if (true) {
				Vector2 acceleration = particle.force;
				particle.velocity += 0.5f * (particle.oldAcceleration + acceleration) * dt;

				Vector2 deltaPos = particle.velocity * dt + 0.5f * acceleration * dt * dt;
//				if (deltaPos.sqrMagnitude > smoothingRadius * smoothingRadius) {
//					deltaPos = Vector2.zero;
//				}
				
				particle.position += deltaPos;
				particle.oldAcceleration = acceleration;
			} else {
				particle.velocity = (particle.position - particle.oldPosition) / dt;
				particle.oldPosition = particle.position;
				particle.position += particle.velocity * dt + (particle.force * dt * dt) / (0.05f * mass);
			}

		}
	}

	void ComputeParticlePressure (Particle particle)
	{
		/* Ideal gas state equation (Muller2003) */
		particle.pressure = kStiffness * (particle.density - restDensity);
	}

	void PressureForce (Particle particle)
	{
		/* Pressure force caused by the gradient */
		Vector2 pressureGradient = Vector2.zero;

		foreach (Particle p in particles) {
			if (particle == p || particle.density == 0 || p.density == 0)
				continue;

			float dividend = particle.pressure + p.pressure;
			float divisor = 2 * particle.density * p.density;

			Vector2 r = particle.position - p.position;
//			if (r.sqrMagnitude > radius * radius) 
			{
				pressureGradient += -mass * (dividend / divisor) * Kernels.GradientSpiky (r, smoothingRadius);
			}
		}

		particle.force += pressureGradient;		
	}

	void ViscosityForce (Particle particle)
	{
		/* Pressure Gradient */
		Vector2 viscosityForce = Vector2.zero;

		foreach (Particle p in particles) {
			if (particle == p || p.density == 0f || particle.density == 0f)
				continue;

			Vector2 r = particle.position - p.position;
			Vector2 v = p.velocity - particle.velocity;
			viscosityForce += viscosity * v * (mass / p.density) * Kernels.ViscosityLaplacian (r.magnitude, smoothingRadius);
		}

		particle.force += viscosityForce;		
	}

	void ExternalForces (Particle particle)
	{
		if (useGravityForce)
			particle.force += gravity;

		if (Input.GetMouseButton (0) && camera) {
			Vector2 mousePos = camera.ScreenToWorldPoint (Input.mousePosition);

			Vector2 toMouseDir = mousePos - particle.position;

			if (toMouseDir.sqrMagnitude < 4 * 4) {
				particle.force += toMouseDir * 10 - gravity;
			}
		}
	}

	/* ************** */
	/* Debug & others */
	/* ************** */

	void ForceBounds (Particle particle)
	{
		float velocityDamping = 0.5f;
		Vector2 pos = particle.position;

		if (pos.x < -bounds.x) {
			particle.position.x = -bounds.x;
			particle.velocity.x = -particle.velocity.x * velocityDamping;
			particle.force.x = 0;
		} else if (pos.x > bounds.x) {
			particle.position.x = bounds.x;
			particle.velocity.x = -particle.velocity.x * velocityDamping;
			particle.force.x = 0;
		}

		if (pos.y < -bounds.y) {
			particle.position.y = -bounds.y;
			particle.velocity.y = -particle.velocity.y * velocityDamping;
			particle.force.y = 0;
		} else if (pos.y > bounds.y) {
			particle.position.y = bounds.y;
			particle.velocity.y = -particle.velocity.y * velocityDamping;
			particle.force.y = 0;
		}

	}

	void DrawDebug ()
	{

		foreach (Particle particle in particles) {
			Vector3 pos = particle.position;
			Vector3 dir = particle.velocity.normalized;
			DebugExtension.DebugArrow (pos, dir * radius);
			DebugExtension.DebugCircle (pos, Vector3.forward, radius);

			Color radiusColor = new Color (0.5f, 0.5f, 0.5f, 0.1f);
			if (bDrawSmoothingRadius) {
				if (particle == selectedDebugParticle)
					DebugExtension.DebugCircle (pos, Vector3.forward, radiusColor * 2, smoothingRadius);
				else
					DebugExtension.DebugCircle (pos, Vector3.forward, radiusColor, smoothingRadius);
			}


			if (bDrawForce) {
				Color forceColor = new Color (1.0f, 0, 0, 0.4f);
				Vector3 force3 = new Vector3 (particle.force.x, particle.force.y, 0);
				DebugExtension.DebugArrow (pos, force3 * radius * 1f, forceColor);
			}


			Vector3 ul = new Vector3 (-bounds.x, bounds.y);
			Vector3 ur = new Vector3 (bounds.x, bounds.y);
			Vector3 dl = new Vector3 (-bounds.x, -bounds.y);
			Vector3 dr = new Vector3 (bounds.x, -bounds.y);

			Debug.DrawLine (ul, ur, Color.grey);
			Debug.DrawLine (ur, dr, Color.grey);
			Debug.DrawLine (dr, dl, Color.grey);
			Debug.DrawLine (dl, ul, Color.grey);
		}
	}

	void DrawDebugParticleText (Particle particle)
	{
		if (txtDebug == null)
			return;

		string text = "";
		text += "Density: " + particle.density + '\n';
		text += "Pressure: " + particle.pressure + '\n';
		text += "Velocity: " + particle.velocity + '\n';
		text += "Force: " + particle.force.ToString () + '\n';
		text += "Old Force: " + particle.oldAcceleration + '\n';

		txtDebug.text = text;

	}
}
