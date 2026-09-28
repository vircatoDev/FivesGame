using System;

namespace Fives.Domain
{
    /// <summary>An exchange of two cells; the lower cell index is always First.</summary>
    public readonly struct Swap : IEquatable<Swap>
    {
        public int First { get; }
        public int Second { get; }

        public Swap(int a, int b)
        {
            First = Math.Min(a, b);
            Second = Math.Max(a, b);
        }

        public bool Equals(Swap other) => First == other.First && Second == other.Second;
        public override bool Equals(object obj) => obj is Swap other && Equals(other);
        public override int GetHashCode() => First * 397 ^ Second;
        public override string ToString() => $"{First}<->{Second}";
    }
}
