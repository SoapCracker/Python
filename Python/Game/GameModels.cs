using System;

namespace Python.Game
{
    public enum Direction { Up, Down, Left, Right }

    // Makkelijk highlights the correct apple as a hint; Moeilijk doesn't.
    public enum Difficulty { Makkelijk, Moeilijk }

    public enum Topic { Programmeertalen, Wiskunde }

    public enum PowerUpType { Geen, Rood, Blauw, Klok }

    // A single tile, addressed by column and row.
    public struct Cell : IEquatable<Cell>
    {
        public int Col;
        public int Row;

        public Cell(int col, int row) { Col = col; Row = row; }

        public bool Equals(Cell other) { return Col == other.Col && Row == other.Row; }
        public override bool Equals(object obj) { return obj is Cell c && Equals(c); }
        public override int GetHashCode() { return Col * 1000 + Row; }
    }

    public class Apple
    {
        public Cell Position;
        public string Label;
        public bool IsCorrect;

        public Apple(Cell position, string label, bool isCorrect)
        {
            Position = position;
            Label = label;
            IsCorrect = isCorrect;
        }
    }

    // One round: the four apple labels, which one is correct, and the question shown on top.
    public class Round
    {
        public string[] Labels;
        public int CorrectIndex;
        public string Question;
    }
}
