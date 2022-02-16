namespace JA.Mathematics.Spatial
{
    /// <summary>
    /// A triangle in space
    /// </summary>
    public struct Triangle
    {
        public Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            this.A=a;
            this.B=b;
            this.C=c;
        }
        public Vector3 A { get; }
        public Vector3 B { get; }
        public Vector3 C { get; }

        public Vector3 ParallepidedVector
            => (A^B) + (B^C) + (C^A);

        public double TripleProduct => A*(B^C);

        public Vector3 Normal
            => ParallepidedVector.UnitVector();

        public double Area => ParallepidedVector.Magnitude/2;

        public Vector3 Centoid
            => (A+B+C)/3;
    }
}
