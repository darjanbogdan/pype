namespace Pype
{
    public readonly struct Unit
    {
        public static readonly Unit Value = new Unit();

        public override bool Equals(object obj) => obj is Unit;

        public override int GetHashCode() => default;

        public static bool operator ==(Unit left, Unit right) => left.Equals(right);

        public static bool operator !=(Unit left, Unit right) => left.Equals(right) == false;
    }
}
