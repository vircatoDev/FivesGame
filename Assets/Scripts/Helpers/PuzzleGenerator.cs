using System.Collections.Generic;
using Fives.Domain;
using UnityEngine;

namespace Scripts.Helpers
{
    public static class PuzzleGenerator
    {
        public static List<Vector2> GenerateStartField(out int emptyIndexResult)
        {
            return GenerateStartField(3, out emptyIndexResult);
        }

        public static List<Vector2> GenerateStartField(int boardSize, out int emptyIndexResult)
        {
            var cellCount = BoardMath.CellCount(boardSize);
            var field = new List<Vector2>(cellCount);

            for (int y = 0; y < boardSize; y++)
            {
                for (int x = 0; x < boardSize; x++)
                {
                    field.Add(new Vector2(x, y));
                }
            }

            var emptyIndex = Random.Range(0, field.Count);
            Vector2 emptyCell = field[emptyIndex];
            emptyIndexResult = emptyIndex;

            HashSet<string> previousStates = new HashSet<string>();
            previousStates.Add(FieldToString(field));

            int shuffleCount = Random.Range(cellCount * 2, cellCount * 4 + 1);
            for (int i = 0; i < shuffleCount; i++)
            {
                List<Vector2> neighbors = GetUniqueNeighbors(field, emptyCell, previousStates, boardSize);

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
            HashSet<string> previousStates, int boardSize)
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

                if (neighbor.x >= 0 && neighbor.x < boardSize && neighbor.y >= 0 && neighbor.y < boardSize)
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
