using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HashGrid2D
{
	int numCols;
	int numRows;
	float cellSize;

	float radius;

	Vector2 offset;
	Vector2 size;

	Dictionary<int, List<Particle>> cells;


	/* Debug */
	List<Vector2> positions = new List<Vector2>();

	public HashGrid2D(Vector2 min, Vector2 max, float argCellSize, float argRadius) {
		cellSize = argCellSize;
		radius = argRadius;

		offset = min;
		size = max;

		Setup(size, cellSize);
	}

	void Setup(Vector2 size, float argCellSize) {
		size += new Vector2(4, 4);
		numCols = Mathf.CeilToInt(size.x / argCellSize);
		numRows = Mathf.CeilToInt(size.y / argCellSize);
		cellSize = argCellSize;
		
		cells = new Dictionary<int, List<Particle>>(numCols * numRows);
		for(int i = 0; i < GetNumCells(); i++)
		{
			cells.Add(i, new List<Particle>());
		}
	}

	public void ClearCells() {
		cells.Clear();
		for(int i = 0; i < GetNumCells(); i++)
		{
			cells.Add(i, new List<Particle>());
		}

		positions.Clear();
	}

	public void RegisterParticle(Particle particle)
	{
		List<int> cellIds = GetCellsForParticle(particle);

		foreach (int id in cellIds)
		{
			cells[id].Add(particle);
		}

	}

	List<int> GetCellsForParticle(Particle particle)
	{
		List<int> cellIds = new List<int>();  
		Vector2 min = new Vector2(particle.position.x - radius,
								  particle.position.y - radius);   
		Vector2 max = new Vector2(particle.position.x + radius,
								  particle.position.y + radius);

		float width = numCols;   
		int cellId;
		
		cellId = GetHashValue(min);
		if (cellIds.Contains(cellId) == false)
			cellIds.Add(cellId);
		
		cellId = GetHashValue(new Vector2(max.x, min.y));
		if (cellIds.Contains(cellId) == false)
			cellIds.Add(cellId);
		
		cellId = GetHashValue(max);
		if (cellIds.Contains(cellId) == false)
			cellIds.Add(cellId);
		
		cellId = GetHashValue(new Vector2(min.x, max.y));
		if (cellIds.Contains(cellId) == false)
			cellIds.Add(cellId);

		return cellIds;
	}

	private int GetHashValue(Vector2 pos)
	{
		int x = Mathf.FloorToInt(pos.x / cellSize);
		int y = Mathf.FloorToInt(pos.y / cellSize);
		
		return x + y * numRows;
	}

	Vector2 GetHashKey(Vector2 position)
	{
		int x = Mathf.FloorToInt(position.x / cellSize);
		int y = Mathf.FloorToInt(position.y / cellSize);
		return new Vector2(x, y);
	}

	// SAME1
	public List<Particle> GetNearby(Particle particle)
	{
		List<Particle> nearby = new List<Particle>();
		List<int> cellIds =  GetCellsForParticle(particle);
		foreach(int cellId in cellIds)
		{
			nearby.AddRange(cells[cellId]);
		}
		return nearby;
	}
	// SAME1
	public List<Particle> GetPossibleNeighbours(Vector2 position)
	{
		List<Particle> neighbours;

		cells.TryGetValue(GetHashValue(position), out neighbours);

		return neighbours;
	}


	public int GetNumCells ()
	{
		return numCols * numRows;
	}


	/* Debug */

	public void DrawGrid () 
	{
		Vector3 offset3D = new Vector3(offset.x, offset.y, 0);
		Color gridColor = new Color(0.4f, 0.2f, 0.3f, 1.0f);
		for (int i = 0; i <= numRows; i++)
		{
			Vector3 start = new Vector3(0, i * cellSize, 0) + offset3D;
			Vector3 end   = new Vector3(numCols * cellSize, i * cellSize, 0) + offset3D;
			Debug.DrawLine(start, end, gridColor);
		}
		for (int j = 0; j <= numCols; j++)
		{			
			Vector3 start = new Vector3(j * cellSize, 0, 0) + offset3D;
			Vector3 end   = new Vector3(j * cellSize, numRows * cellSize, 0) + offset3D;
			Debug.DrawLine(start, end, gridColor);
		}

		// foreach (var pos in positions)
		// {
		// 	Vector3 pos3D = new Vector3(pos.x, pos.y, 0);
		// 	DebugExtension.DebugPoint(pos3D, Color.magenta, 0.4f);
		// }
	}
}
