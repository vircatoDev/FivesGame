using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Helpers
{
    public static class PuzzleGenerator
    {
        public static List<Vector2> GenerateStartField(out int emptyIndexResult)
        {
            // Create virtual game field from [0, 0] to [2, 2]
            List<Vector2> field = new List<Vector2>();

            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    field.Add(new Vector2(x, y));
                }
            }

            var emptyIndex = Random.Range(0, field.Count);
            Vector2 emptyCell = field[emptyIndex];
            emptyIndexResult = emptyIndex;

            HashSet<string> previousStates = new HashSet<string>();
            previousStates.Add(FieldToString(field));

            // can control the difficulty by changing the number of shuffles
            int shuffleCount = Random.Range(5, 10);
            for (int i = 0; i < shuffleCount; i++)
            {
                List<Vector2> neighbors = GetUniqueNeighbors(field, emptyCell, previousStates);

                if (neighbors.Count == 0)
                {
                    break;
                }

                Vector2 nextEmptyCell = neighbors[Random.Range(0, neighbors.Count)];

                Swap(field, emptyCell, nextEmptyCell);

                previousStates.Add(FieldToString(field));

                emptyCell = nextEmptyCell;

                emptyIndexResult = field.IndexOf(emptyCell);
            }

            return field;
        }

        private static List<Vector2> GetUniqueNeighbors(List<Vector2> field, Vector2 emptyCell,
            HashSet<string> previousStates)
        {
            List<Vector2> neighbors = new List<Vector2>();

            // Move set
            Vector2[] directions =
            {
                new Vector2(-1, 0),
                new Vector2(1, 0),
                new Vector2(0, -1),
                new Vector2(0, 1)
            };

            foreach (Vector2 direction in directions)
            {
                Vector2 neighbor = emptyCell + direction;

                if (neighbor.x >= 0 && neighbor.x < 3 && neighbor.y >= 0 && neighbor.y < 3)
                {
                    //Check next state ,we need unique moves
                    var tempField = new List<Vector2>(field);
                    Swap(tempField, emptyCell, neighbor);
                    string tempState = FieldToString(tempField);

                    if (!previousStates.Contains(tempState))
                    {
                        neighbors.Add(neighbor);
                    }
                }
            }

            return neighbors;
        }

        private static void Swap(List<Vector2> field, Vector2 emptyCell, Vector2 nextEmptyCell)
        {
            int emptyIndex = field.IndexOf(emptyCell);
            int neighborIndex = field.IndexOf(nextEmptyCell);

            Vector2 temp = field[emptyIndex];
            field[emptyIndex] = field[neighborIndex];
            field[neighborIndex] = temp;
        }

        private static string FieldToString(List<Vector2> field)
        {
            return string.Join(",", field);
        }
    }
}