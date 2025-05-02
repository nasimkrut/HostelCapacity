internal class run2
{
    private static List<List<char>> GetInput()
    {
        var data = new List<List<char>>();
        string line;
        while ((line = Console.ReadLine()) != null && line != "")
            data.Add(line.ToCharArray().ToList());
        return data;
    }

    private static int Solve(List<List<char>> data)
    {
        ParseMap(data, out var walls, out var robots, out var keyMap, out var doorMap, out var allKeys);
        var initialRobots = InitializeRobots(robots);

        var reachableCache = new Dictionary<((int x, int y) pos, int keys), Dictionary<(int x, int y), int>>();
        var heap = new PriorityQueue<(int steps, (int, int), (int, int), (int, int), (int, int), int), int>();
        heap.Enqueue((0, initialRobots.Item1, initialRobots.Item2, initialRobots.Item3, initialRobots.Item4, 0), 0);

        var visited = new Dictionary<((int, int), (int, int), (int, int), (int, int), int), int>();

        while (heap.Count > 0)
        {
            var (steps, r1, r2, r3, r4, keys) = heap.Dequeue();

            if (keys == allKeys)
                return steps;

            if (visited.TryGetValue((r1, r2, r3, r4, keys), out var prevSteps) && prevSteps < steps)
                continue;

            var positions = new[] { r1, r2, r3, r4 };
            for (var i = 0; i < 4; i++)
            {
                var reachable = GetReachable(positions[i], keys, walls, keyMap, doorMap, reachableCache);
                foreach (var (kPos, dist) in reachable)
                {
                    var newKeys = keys | keyMap[kPos];
                    var newPositions = positions.ToArray();
                    newPositions[i] = kPos;
                    Array.Sort(newPositions,
                        (a, b) => a.Item1 == b.Item1 ? a.Item2.CompareTo(b.Item2) : a.Item1.CompareTo(b.Item1));


                    var newState = (newPositions[0], newPositions[1], newPositions[2], newPositions[3], newKeys);
                    var newSteps = steps + dist;

                    if (!visited.TryGetValue(newState, out var oldSteps) || newSteps < oldSteps)
                    {
                        visited[newState] = newSteps;
                        heap.Enqueue(
                            (newSteps, newState.Item1, newState.Item2, newState.Item3, newState.Item4, newKeys),
                            newSteps);
                    }
                }
            }
        }

        return -1;
    }

    private static void ParseMap(
        List<List<char>> data,
        out HashSet<(int x, int y)> walls,
        out List<(int x, int y)> robots,
        out Dictionary<(int x, int y), int> keyMap,
        out Dictionary<(int x, int y), char> doorMap,
        out int allKeys)
    {
        walls = new HashSet<(int x, int y)>();
        robots = new List<(int x, int y)>();
        keyMap = new Dictionary<(int x, int y), int>();
        doorMap = new Dictionary<(int x, int y), char>();
        allKeys = 0;

        for (var y = 0; y < data.Count; y++)
        {
            var row = data[y];
            for (var x = 0; x < row.Count; x++)
            {
                var c = row[x];
                var pos = (x, y);
                switch (c)
                {
                    case '@':
                        robots.Add(pos);
                        break;
                    case '#':
                        walls.Add(pos);
                        break;
                    default:
                        if (char.IsLower(c))
                        {
                            var keyId = c - 'a';
                            var keyBit = 1 << keyId;
                            keyMap[pos] = keyBit;
                            allKeys |= keyBit;
                        }
                        else if (char.IsUpper(c))
                        {
                            doorMap[pos] = char.ToLower(c);
                        }

                        break;
                }
            }
        }
    }

    private static ((int, int), (int, int), (int, int), (int, int)) InitializeRobots(List<(int x, int y)> robots)
    {
        var sorted = robots.OrderBy(p => p.x).ThenBy(p => p.y).ToList();
        return (sorted[0], sorted[1], sorted[2], sorted[3]);
    }

    private static Dictionary<(int x, int y), int> GetReachable(
        (int x, int y) pos,
        int ownedKeys,
        HashSet<(int x, int y)> walls,
        Dictionary<(int x, int y), int> keyMap,
        Dictionary<(int x, int y), char> doorMap,
        Dictionary<((int x, int y), int), Dictionary<(int x, int y), int>> cache)
    {
        var key = (pos, ownedKeys);
        if (cache.TryGetValue(key, out var cached))
            return cached;

        var visited = new HashSet<(int x, int y)>();
        var queue = new Queue<((int x, int y), int)>();
        var reachable = new Dictionary<(int x, int y), int>();

        queue.Enqueue((pos, 0));

        while (queue.Count > 0)
        {
            var (p, steps) = queue.Dequeue();
            if (!visited.Add(p))
                continue;

            if (keyMap.TryGetValue(p, out var keyBit) && (ownedKeys & keyBit) == 0)
            {
                reachable[p] = steps;
                continue;
            }

            foreach (var (dx, dy) in new[] { (-1, 0), (1, 0), (0, 1), (0, -1) })
            {
                var np = (p.x + dx, p.y + dy);
                if (walls.Contains(np) || visited.Contains(np))
                    continue;

                if (doorMap.TryGetValue(np, out var reqChar))
                {
                    var reqKey = 1 << (reqChar - 'a');
                    if ((ownedKeys & reqKey) == 0)
                        continue;
                }

                queue.Enqueue((np, steps + 1));
            }
        }

        cache[key] = reachable;
        return reachable;
    }

    private static void Main()
    {
        var data = GetInput();
        var result = Solve(data);
        if (result == -1)
            Console.WriteLine("No solution found");
        else
            Console.WriteLine(result);
    }
}