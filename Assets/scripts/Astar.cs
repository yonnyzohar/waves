

namespace RTS
{
	using System;
	using System.Collections.Generic;

	class PositionComparer : IComparer<StarNode>
	{
		public int Compare(StarNode x, StarNode y)
		{
			return x.f.CompareTo(y.f);
		}
	}

	public class Astar
	{

		private List<StarNode> path;
		private List<StarNode> closedList;
		private List<StarNode> openList;
		public List<StarNode> visitedList;

		private int numRows;
		private int numCols;


		public Astar()
		{



			/*
			//use case example
			var map:List<StarNode> = [

			[0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
					[0,1,1,0,0,0,0,0,0,0],
					[0,0,1,0,0,0,0,0,0,0],
					[0,0,1,0,0,0,0,0,0,0],
					[0,0,0,0,0,1,1,1,0,0],
					[0,0,0,0,0,0,1,0,0,0],
					[0,0,0,0,0,0,1,0,0,0],
					[0,1,0,0,0,0,1,1,0,0],
					[0,1,0,0,0,0,1,1,0,0],
					[0,1,0,0,0,0,0,1,0,0]
				];

			numRows = map.length;
			numCols = map[0].length;
			List<Node> grid = createNodesBoard(map, 1);
			var startNode:StarNode = grid[9][9];
			var endNode:StarNode = grid[9][0];
			*/

			//List<StarNode> pathList = getPath(grid, startNode, endNode);
			//printPath(map, pathList);

		}

		public List<RowCol> CreatePath(List<List<Node>> map, int walkable, Node start, Node end)
		{
			path = new List<StarNode>();
			visitedList = new List<StarNode>();
			numRows = map.Count;
			numCols = map[0].Count;

			List<List<StarNode>> grid = createNodesBoard(map, TileType.SEA);

			int startRow = start.grid_row;
			int startCol = start.grid_col;

			int endRow = end.grid_row;
			int endCol = end.grid_col;

			StarNode startNode = grid[startRow][startCol];
			StarNode endNode = grid[endRow][endCol];

			List<StarNode> res = getPath(grid, startNode, endNode);
			List<RowCol> nodesList = new List<RowCol>();

			for (int i = 1; i < res.Count; i++)
			{
				StarNode n = res[i];
				nodesList.Add(new RowCol(n.row, n.col));

			}

			return nodesList;
		}

		List<List<StarNode>> createNodesBoard(List<List<Node>> map, TileType walkable)
		{
			List<List<StarNode>> nodesBoard = new List<List<StarNode>>();
			for (int row = 0; row < numRows; row++)
			{
				nodesBoard.Add(new List<StarNode>());
				for (int col = 0; col < numCols; col++)
				{
					StarNode n = new StarNode();
					nodesBoard[row].Add(n);
					n.g = 0;//distance from start node
					n.h = 0;//huristic distnace to end node
					n.f = 0;//combined cost of the two
					n.row = row;
					n.col = col;
					n.walkable = map[row][col].terrainType == walkable;
				}
			}

			return nodesBoard;
		}


		/*



		void printPath(List<List<StarNode>> map, List<StarNode> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				StarNode node = list[i];
				map[node.row][node.col] = 5;
			}

			for (int row = 0; row < numRows; row++)
			{
				Debug.Log(map[row].ToString());
			}

		}
		*/

		public List<StarNode> getPath(List<List<StarNode>> grid, StarNode startNode, StarNode endNode)
		{
			openList = new List<StarNode>();
			closedList = new List<StarNode>();
			startNode.g = 0;//cost from start node
			startNode.h = estimateDistance(startNode, endNode);//cost to end node
			startNode.f = startNode.g + startNode.h;//combination - global cost
			openList.Add(startNode);
			while (openList.Count > 0)
			{
				StarNode curNode = removeSmallest(openList);
				closedList.Add(curNode);
				if (curNode == endNode)
				{
					return createPath(startNode, endNode);
				}
				else
				{
					foreach (StarNode neighbor in getNeighbors(grid, curNode))
					{
						visitedList.Add(neighbor);
						float g = curNode.g + getCost(curNode, neighbor);
						if (closedList.IndexOf(neighbor) >= 0 || openList.IndexOf(neighbor) >= 0)
						{
							if (neighbor.g > g)
							{
								neighbor.g = g;
								neighbor.f = neighbor.h + g;
								neighbor.parent = curNode;
							}
						}
						else
						{
							neighbor.g = g;
							neighbor.h = estimateDistance(neighbor, endNode);
							neighbor.f = neighbor.h + g;
							neighbor.parent = curNode;
							openList.Add(neighbor);
						}
					}
				}
			}
			path = new List<StarNode>();
			return path;
		}

		List<StarNode> createPath(StarNode startNode, StarNode endNode)
		{
			path = new List<StarNode>();
			StarNode curNode = endNode;
			path.Add(endNode);
			while (curNode != startNode)
			{
				curNode = curNode.parent;
				path.Add(curNode);
			}
			path.Reverse();
			return path;
		}

		float getCost(StarNode curNode, StarNode neighbor)
		{
			float cost = 1.0f;
			if ((curNode.row != neighbor.row) && (curNode.col != neighbor.col))
			{
				cost = 1.414f;
			}
			return cost;
		}

		List<StarNode> getNeighbors(List<List<StarNode>> grid, StarNode curNode)
		{
			List<StarNode> neighbors = new List<StarNode>();
			int startRow = Math.Max(0, curNode.row - 1);
			int startCol = Math.Max(0, curNode.col - 1);
			int endRow = Math.Min(numRows - 1, curNode.row + 1);
			int endCol = Math.Min(numCols - 1, curNode.col + 1);
			for (int r = startRow; r <= endRow; r++)
			{
				for (int c = startCol; c <= endCol; c++)
				{
					StarNode neighbor = grid[r][c];
					//&& neighbor.regionNum == curNode.regionNum
					if (neighbor.walkable == true)
					{
						neighbors.Add(neighbor);
					}
				}
			}
			return neighbors;
		}

		float estimateDistance(StarNode curNode, StarNode endNode)
		{
			int distX = Math.Abs(curNode.col - endNode.col);
			int distY = Math.Abs(curNode.row - endNode.row);
			return (float)Math.Sqrt((double)(distX * distX) + (double)(distY * distY));
		}

		StarNode removeSmallest(List<StarNode> array)
		{
			array.Sort(new PositionComparer());
			StarNode first = array[0];
			array.RemoveAt(0);
			return first;
		}



	}
}



