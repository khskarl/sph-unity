using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SPH))]
public class SPHRenderer : MonoBehaviour {

	SPH sph;

	// Use this for initialization
	void Start () {
		sph = GetComponent<SPH>();
	}
	
	// Update is called once per frame
	void Update () {
		// RenderParticles(sph.pa)
	}

	void RenderParticles(Particle particles) 
	{

	}

	void RenderParticle(Particle particle) 
	{

	}
}
