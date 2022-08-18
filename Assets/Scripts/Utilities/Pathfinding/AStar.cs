using System.Collections.Generic;
using UnityEngine;
using TamborGame.Gameplay;

namespace TamborGame.Utilities
{
    public static class AStar
    {
        public static float Heuristic(Vector3 a, Vector3 b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z);
        }

        public static Queue<BoardSpace> CalculatePathToPlace(BoardSpace start, BoardSpace goal)
        {
            PriorityQueue<BoardSpace> frontier = new PriorityQueue<BoardSpace>();
            frontier.Insert(new PriorityNode<BoardSpace>(start, 0));
            Dictionary<BoardSpace, BoardSpace> cameFrom = new Dictionary<BoardSpace, BoardSpace>();
            Dictionary<BoardSpace, float> costSoFar = new Dictionary<BoardSpace, float>();
            cameFrom.Add(start, null);
            costSoFar.Add(start, 0);

            BoardSpace current = null;
            while (frontier.Count() > 0)
            {
                current = frontier.Pull().value;
                if (current == goal)
                    break;

                foreach (BoardSpace bs in current._Adjacent)
                {
                    float newCost = costSoFar[current] + Vector3.SqrMagnitude(bs.gameObject.transform.position - current.gameObject.transform.position);
                    if (!costSoFar.ContainsKey(bs))
                    {
                        costSoFar.Add(bs, newCost);
                        float priority = newCost + Heuristic(goal.transform.position, bs.transform.position);
                        frontier.Insert(new PriorityNode<BoardSpace>(bs, priority));
                        cameFrom.Add(bs, current);
                    }
                    else if (newCost < costSoFar[bs])
                    {
                        costSoFar[bs] = newCost;
                        float priority = newCost + Heuristic(goal.transform.position, bs.transform.position);
                        frontier.Insert(new PriorityNode<BoardSpace>(bs, priority));
                        cameFrom[bs] = current;
                    }
                }
            }

            current = goal;
            Stack<BoardSpace> pathTemp = new Stack<BoardSpace>();
            while (current != start)
            {
                pathTemp.Push(current);
                current = cameFrom[current];
            }
            pathTemp.Push(start);

            Queue<BoardSpace> path = new Queue<BoardSpace>();
            while (pathTemp.Count > 0)
                path.Enqueue(pathTemp.Pop());

            return path;
        }
    }
}