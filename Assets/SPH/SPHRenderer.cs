using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SPH))]
public class SPHRenderer : MonoBehaviour {
	public bool drawRadius = true;
	public bool drawSmoothingRadius = true;
	public bool drawForce = false;


	Particle selectedDebugParticle = null;

	SPH sph;

	// Use this for initialization
	void Start () {
		sph = GetComponent<SPH>();
	}
	
	// Update is called once per frame
	void LateUpdate () {
		RenderParticles(sph.GetParticles());
	}

	void RenderParticles(List<Particle> particles) 
	{
		foreach (Particle particle in particles)
		{
			RenderParticle(particle);
		}
	}

	void RenderParticle(Particle particle) 
	{
		float smoothingRadius = sph.smoothingRadius;
		float radius = sph.radius;
		Particle selectedParticle = sph.selectedDebugParticle;

		Vector3 pos = particle.position;
		Vector3 dir = particle.velocity.normalized;
		DebugExtension.DebugArrow (pos, dir * radius, Color.white, 0, false);

		if (particle == selectedParticle) {
			DebugExtension.DebugCircle (pos, Vector3.forward, Color.red, radius, 0, false);
			foreach (Particle neighbor in sph.grid.GetNearby(particle))
			{
				DebugExtension.DebugCircle (neighbor.position, Vector3.forward, Color.magenta * 0.7f, radius * 1.5f, 0, false);
			}
		}
		else
			DebugExtension.DebugCircle (pos, Vector3.forward, radius, 0, false);
			


		Color radiusColor = new Color (0.5f, 0.5f, 0.5f, 0.4f);
		if (drawSmoothingRadius) {
			DebugExtension.DebugCircle (pos, Vector3.forward, radiusColor, smoothingRadius);
		}


		if (drawForce) {
			Color forceColor = new Color (1.0f, 0, 0, 0.4f);
			Vector3 force3 = new Vector3 (particle.force.x, particle.force.y, 0);
			DebugExtension.DebugArrow (pos, force3 * radius * 1f, forceColor);
		}


		// Vector3 ul = new Vector3 (offset.x, size.y + offset.y);
		// Vector3 ur = new Vector3 (size.x + offset.x, size.y + offset.y);
		// Vector3 dl = new Vector3 (offset.x, offset.y);
		// Vector3 dr = new Vector3 (size.x + offset.x, offset.y);

		// Debug.DrawLine (ul, ur, Color.grey);
		// Debug.DrawLine (ur, dr, Color.grey);
		// Debug.DrawLine (dr, dl, Color.grey);
		// Debug.DrawLine (dl, ul, Color.grey);
	}

}
