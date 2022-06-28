using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A*: https://www.youtube.com/watch?v=alU04hvz6L4
// frontier = PriorityQueue()
// frontier.put(start, 0)
// came_from = dict()
// cost_so_far = dict()
// came_from[start] = None
// cost_so_far[start] = 0

// while not frontier.empty():
//    current = frontier.get()

//    if current == goal:
//       break

//for next in graph.neighbors(current):
//   new_cost = cost_so_far[current] + graph.cost(current, next)
//   if next not in cost_so_far or new_cost < cost_so_far[next]:
//      cost_so_far[next] = new_cost
//      priority = new_cost + heuristic(goal, next)
//      frontier.put(next, priority)
//      came_from[next] = current

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

            foreach (BoardSpace bs in current.adjacent)
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
