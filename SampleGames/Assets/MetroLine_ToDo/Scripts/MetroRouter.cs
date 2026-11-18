using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

namespace MetroLine.Student
{
    public sealed class MetroRouter : IMetroRouter
    {
        public List<int> FindShortestPath(int startId, MetroShape goal,
            IReadOnlyList<MetroStationState> stations,
            IReadOnlyDictionary<int, List<int>> graph)
        {
            // TODO 01: stations, graph, startId가 유효한지 검사하세요.
            if(stations == null || graph == null || !graph.ContainsKey(startId))
            {
                return new List<int>();
            }

            // TODO 02: 방문할 역을 저장할 Queue<int>를 만드세요.
            var queue = new Queue<int>();

            // TODO 03: 각 역의 이전 역을 기록할 Dictionary<int, int>를 만드세요.
            var previouse = new Dictionary<int, int>();

            // TODO 04: 시작 역을 Queue와 방문 기록에 넣으세요.
            queue.Enqueue(startId);
            previouse[startId] = -1;

            // TODO 05: Queue가 빌 때까지 역을 하나씩 꺼내 탐색하세요.
            // TODO 06: 현재 역의 모양이 목적지 모양과 같은지 확인하세요.
            // TODO 07: 목적지를 찾았다면 previous를 따라 경로를 역추적하세요.
            // TODO 08: 방문하지 않은 이웃 역만 Queue에 넣으세요.

            while(queue.Count > 0)
            {
                int current = queue.Dequeue();

                MetroStationState state = stations.FirstOrDefault(station => station.Id == current);

                if(state != null && state.Shape == goal)
                {
                    var path = new List<int>();

                    for(int at = current; at != -1; at = previouse[at])
                    {
                        path.Add(at);
                    }

                    path.Reverse();
                    return path;
                }

                if(!graph.TryGetValue(current, out List<int> neighbors))
                {
                    continue;
                }

                foreach(int next in neighbors)
                {
                    if(previouse.ContainsKey(next))
                    {
                        continue;
                    }

                    previouse[next] = current;
                    queue.Enqueue(next);
                }
            }

            // TODO 09: 도달할 수 없다면 빈 경로를 반환하세요.
            return new List<int>();
        }
    }
}
