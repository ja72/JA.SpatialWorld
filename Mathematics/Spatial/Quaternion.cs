using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

using static System.Math;

namespace JA.Mathematics.Spatial
{
    /// <summary>
    /// Specify the layout of a quaternion.
    /// </summary>
    public enum QuaternionLayout
    {
        /// <summary>
        /// The four values are (scalar,vector)
        /// </summary>
        ScalarVector,
        /// <summary>
        /// The four values are (vector,scalar)
        /// </summary>
        VectorScalar,
    }

    /// <summary>
    /// Defines a quaternion object, used to describe a rotation axis and angle.
    /// </summary>
    /// <remarks></remarks>
    /// <seealso cref="IDblVector" />
    public struct Quaternion :
        ICollection<double>,
        IEquatable<Quaternion>,
        IFormattable,
        System.Collections.ICollection
    {
        /// <summary>
        /// Specifies the preferred layout to use when one is not specified explicitly.
        /// </summary>
        public const QuaternionLayout DefaultLayout = QuaternionLayout.VectorScalar;

        #region Factory
        public Quaternion(Vector3 vector, double scalar, QuaternionLayout layout = QuaternionLayout.VectorScalar)
        {
            this.Vector=vector;
            this.Scalar=scalar;
            this.Layout = layout;
        }
        public Quaternion(double scalar, Vector3 vector, QuaternionLayout layout = QuaternionLayout.ScalarVector)
        {
            this.Vector=vector;
            this.Scalar=scalar;
            this.Layout = layout;            
        }
        public Quaternion(QuaternionLayout layout, params double[] elements)
            : this(elements[0], elements[1], elements[2], elements[3], layout)
        {
            Contract.Requires(elements!=null);
            Contract.Requires(elements.Length==4);
        }
        public Quaternion(double[] elements, QuaternionLayout layout = DefaultLayout)
           : this(elements[0], elements[1], elements[2], elements[3], layout)
        {
            Contract.Requires(elements!=null);
            Contract.Requires(elements.Length==4);
        }
        public Quaternion(double a, double b, double c, double d, QuaternionLayout layout = DefaultLayout)
        {
            switch(layout)
            {
                case QuaternionLayout.ScalarVector:
                    this.Scalar = a;
                    this.Vector = new Vector3(b, c, d);
                    break;
                case QuaternionLayout.VectorScalar:
                    this.Vector =new Vector3(a, b, c);
                    this.Scalar = d;
                    break;
                default:
                    throw new NotSupportedException();
            }
            this.Layout = layout;            
        }
        public Quaternion(Quaternion other, QuaternionLayout new_layout = DefaultLayout)
        {
            this.Vector = other.Vector;
            this.Scalar = other.Scalar;
            this.Layout = new_layout;            
        }
        static readonly Quaternion Zero = new Quaternion(Vector3.Zero, 0);
        //public static readonly Quaternion UnitX = new Quaternion(Vector3.UnitX, 0);
        //public static readonly Quaternion UnitY = new Quaternion(Vector3.UnitY, 0);
        //public static readonly Quaternion UnitZ = new Quaternion(Vector3.UnitZ, 0);
        public static readonly Quaternion Identity = new Quaternion(Vector3.Zero, 1);
        public static Quaternion FromVector(Vector3 vector, QuaternionLayout layout = DefaultLayout) 
            //tex:Vector $$\hat{q} = \pmatrix{\vec{v} \\ 0}$$
            => new Quaternion(vector, 0, layout);
        public static Quaternion FromAxisAngle(AxisName axis, double angle, QuaternionLayout layout = DefaultLayout)
            => FromAxisAngle(Vector3.FromAxis(axis), angle, layout);
        public static Quaternion FromAxisAngle(Vector3 axis, double angle, QuaternionLayout layout = DefaultLayout)
            //tex:Unit rotation $$\hat{q} = \pmatrix{ \vec{z}\, \sin \tfrac{\theta}{2} \\ \cos \tfrac{\theta}{2} }$$
            => new Quaternion(Sin(angle/2)*axis.Normalized(), Cos(angle/2), layout);
        public static Quaternion FromRotVelocityAndTime(Vector3 omega, double timeStep, QuaternionLayout layout = DefaultLayout)
        {
            var ω = omega.Magnitude;
            var θ = ω*timeStep;
            if (Abs(θ)> 2.98023223876953E-08)
            {
                var k = omega/ω;
                return Quaternion.FromAxisAngle(k, θ, layout);
            }
            return Quaternion.Identity;
        }
        public static Quaternion RotateX(double angle) => FromAxisAngle(AxisName.X, angle);
        public static Quaternion RotateY(double angle) => FromAxisAngle(AxisName.Y, angle);
        public static Quaternion RotateZ(double angle) => FromAxisAngle(AxisName.Z, angle);
        public static implicit operator Quaternion(double scalar) => new Quaternion(Vector3.Zero, scalar);
        public static implicit operator Quaternion(Vector3 vector) => new Quaternion(vector, 0);
        public Quaternion(string description)
        {
            this=Parse(description);
        }
        public static Quaternion FromRotationMatrix(Matrix3 R)
        {
            var a23 = R[2, 1]-R[1, 2];
            var a13 = R[0, 2]-R[2, 0];
            var a12 = R[1, 0]-R[0, 1];
            var a11 = R[0, 0];
            var a22 = R[1, 1];
            var a33 = R[2, 2];
            var trace = a11+a22+a33;
            if (trace<=3)
            {
                var s = 0.5*Sqrt((a23*a23 + a13*a13 + a12*a12)/(3-a11-a22-a33));
                if (s>0)
                {
                    var v = Vector3.Cartesian(a23, a13, a12)/(4*s);
                    return new Quaternion(s, v);
                }
                return Identity;
            }
            return Zero;
        }
        public void Deconstruct(out Vector3 vector, out double scalar)
        {
            vector = this.Vector;
            scalar = this.Scalar;
        }
        #endregion

        #region Properties & Methods
        public int Count => 4;
        public QuaternionLayout Layout { get; }
        public double this[int index]
        {
            get
            {
                switch(Layout)
                {
                    case QuaternionLayout.ScalarVector:
                        if(index>=0 && index<=2)
                        {
                            return Vector[index];
                        }
                        else if(index==3)
                        {
                            return Scalar;
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException(nameof(index));
                        }
                    case QuaternionLayout.VectorScalar:
                        if(index==0)
                        {
                            return Scalar;
                        }
                        else if(index>=1 && index<=3)
                        {
                            return Vector[index];
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException(nameof(index));
                        }
                    default:
                        throw new NotSupportedException();
                }
            }
        }
        /// <summary>
        /// Quaternion multiplication as an operator. It assumes the result is going to be in the
        /// VectorScalar layout since the result is always VectorScalar 3×3 matrix.
        /// </summary>
        /// <returns></returns>
        public Matrix31 ProductOperator()
        {
            //tex:Quaternion product as an operator $$\pmatrix{\vec{v}_1\\s_1} \otimes \pmatrix{\vec{v}_2\\s_2} 
            // = \left| \matrix{s_1 + \vec{v}_1\times & \vec{v}_1 \\ -\vec{v}_1^\intercal & s_1} \right| \pmatrix{\vec{v}_2\\s_2}
            // = \pmatrix{s_1 \vec{v}_2 + s_2 \vec{v}_1 + \vec{v}_1 \times \vec{v}_2 \\ s_1 s_2 - \vec{v}_1 \cdot \vec{v}_2}$$
            return new Matrix31(
                Scalar+Vector.Cross(), Vector,
                -Vector, Scalar);
        }


        /// <summary>
        /// Derivate of a quaternion. dq=1/2*[Ω]*q
        /// </summary>
        /// <param name="ω">The angular velocity of the body</param>
        /// <returns>A quaternion</returns>
        public Quaternion Derivative(Vector3 ω)
        {
            //tex:Quaternion derivative is defined as
            // $$\dot{\hat{q}} = \frac{1}{2} \pmatrix{ \vec{\omega} \\ 0} \otimes \pmatrix{\vec{v}\\s}
            //            = \frac{1}{2} \pmatrix{\vec{\omega} \times \vec{v} + s\, \vec{\omega} \\ -\vec{\omega} \cdot \vec{v}}$$
            return new Quaternion(
                0.5*((ω ^ Vector) + Scalar*ω),
                -0.5*(ω * Vector), Layout);
        }
        
        /// <summary>
        /// Converts quaternion to 3×3 rotation matrix.
        /// </summary>
        /// <param name="inverse">if set to <c>true</c> return the inverse rotation matrix.</param>
        /// <returns>A rotation matrix of type <see cref="Matrix3"/></returns>
        public Matrix3 ToRotationMatrix(bool inverse = false)
        {
            //tex:Rodrigues rotation formula
            //            $$\mathrm{R}=\mathbf{1}+2\,s\,\vec{v}\times+ 2\, \vec{v}\times \vec{v}\times$$
            var s = Scalar;
            var vx = Vector.Cross();
            var sign = inverse ? -1 : 1;
            return 1 + 2*sign*s*vx + 2*(vx*vx);
        }

        /// <summary>
        /// Congruent transformation of a matrix. It is required that the matrix is symmetric.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="inverse">if set to <c>true</c> use the inverse rotation.</param>
        /// <remarks>Equivalent to <c>R*M*tr(R)</c></remarks>
        public Matrix3 Rotate(Matrix3 matrix, bool inverse = false)
        {
            //tex:Congruent rotation formula for symmetric matrix 
            //$$ = \mathrm{R}\, \mathrm{M}\, \mathrm{R}^\intercal $$
            var R = ToRotationMatrix(inverse);
            var Rt = ToRotationMatrix(!inverse);
            return R * matrix * Rt;
        }
        /// <summary>
        /// Rotate a vector
        /// </summary>
        /// <param name="target">The vector.</param>
        public Vector3 Rotate(Vector3 target)
        {
            //tex:Vector rotation formula
            //            $$=\vec{p}+2\,s\,(\vec{v}\times\vec{p})+ 2\, \vec{v}\times( \vec{v}\times\vec{p})$$
            var vxp = Vector^target;
            return target + 2*Scalar*vxp + 2*(Vector ^ vxp);
        }
        /// <summary>
        /// Inverse rotate a vector
        /// </summary>
        /// <param name="target">The vector.</param>
        public Vector3 InvRotate(Vector3 target)
        {
            //tex:Vector inverse rotation formula
            //            $$=\vec{p}-2\,s\,(\vec{v}\times\vec{p})+ 2\, \vec{v}\times( \vec{v}\times\vec{p})$$
            var vxp = Vector^target;
            return target - 2*Scalar*vxp + 2*(Vector ^ vxp);
        }
        #endregion

        #region Formatting
        /// <summary>
        /// Display the quaternion with them form <![CDATA[((x,y,z),w)]]> or <![CDATA[(w,(x,y,z))]]>
        /// </summary>
        /// <returns>A formatted string</returns>
        public override string ToString() => ToString(null);
        /// <summary>
        /// Display the quaternion with them form <![CDATA[((x,y,z),w)]]> or <![CDATA[(w,(x,y,z))]]>, but the specified formatting.
        /// </summary>
        /// <param name="format">The formatting specification.</param>
        public string ToString(string format) => ToString(format, null);
        /// <summary>
        /// Display the quaternion with them form <![CDATA[((x,y,z),w)]]> or <![CDATA[(w,(x,y,z))]]>, but the specified formatting.
        /// </summary>
        /// <param name="format">The formatting specification.</param>
        /// <param name="formatProvider">The format provider, or null for the current culture format provider.</param>
        public string ToString(string format, IFormatProvider formatProvider) 
        {
            var v = Vector.ToString(format, formatProvider);
            var s = Scalar.ToString(format, formatProvider);
            switch(Layout)
            {
                case QuaternionLayout.ScalarVector:
                    return string.Format($"({s},{v})");
                case QuaternionLayout.VectorScalar:
                    return string.Format($"({v},{s})");
                default:
                    return string.Empty;
            }
        }
        /// <summary>
        /// Parses a quaternion from a string.
        /// </summary>
        /// <param name="description">The string description.</param>
        /// <returns>A new quaternion</returns>
        /// <remarks>It tries to find the layout automatically, either VectorScalar or ScalarVector</remarks>
        public static Quaternion Parse(string description)
        {
            Contract.Requires(description!=null);
            description = description.Trim();
            // <(1,2,3)|4>
            if(description.StartsWith("(") && description.EndsWith(")"))
            {
                description = description.Substring(1, description.Length-2);
                // distinguish between ScalarVector and VectorScalar
                //                     q(1,(2,3,4))      q((1,2,3),4)
                if(description.StartsWith("("))
                {
                    int i = description.LastIndexOf(',');
                    if(i>=0)
                    {
                        var vector = Vector3.Parse(description.Substring(0, i));
                        double.TryParse(description.Substring(i+1), out double scalar);
                        return new Quaternion(vector, scalar);
                    }
                }
                else if(description.EndsWith(")"))
                {
                    int i = description.IndexOf(',');
                    if(i>=0)
                    {
                        double.TryParse(description.Substring(0, i), out double scalar);
                        var vector = Vector3.Parse(description.Substring(i+1));
                        return new Quaternion(scalar, vector);
                    }
                }
            }
            return Quaternion.Zero;
        }

        #endregion

        #region Properties        
        /// <summary>
        /// The vector components of the quaternion
        /// </summary>
        public Vector3 Vector { get; }
        /// <summary>
        /// The scalar component of the quaternion
        /// </summary>
        public double Scalar { get; }

        /// <summary>
        /// Get the unit rotation axis.
        /// </summary>
        public Vector3 Axis
        {
            get
            {
                var vm = Vector.Magnitude;
                if (vm>0 && vm!=1)
                {
                    return Vector/vm;
                }
                else if (vm==1)
                {
                    return Vector;
                }
                return Vector3.Zero;
            }
        }
        /// <summary>
        /// Get the rotation angle in radians
        /// </summary>
        public double Angle
        {
            get => 2*Phase;
        }

        /// <summary>
        /// The angle between the scalar and vector parts. The should be equal to half the rotation angle.
        /// </summary>
        public double Phase
        {
            get
            {
                var vm = Vector.Magnitude;
                if (vm>0)
                {
                    var s = Scalar;
                    return Atan2(vm, s);
                }
                return 0;
            }
        }

        #endregion

        #region Algebra        
        /// <summary>
        /// Adds two quaternions element by element.
        /// </summary>
        /// <param name="other">The other quaternion to add.</param>
        /// <returns>The resulting quaternion (with the same layout as this)</returns>
        public Quaternion Add(Quaternion other)
            //tex:$$\pmatrix{\vec{v}_1\\s_1} + \pmatrix{\vec{v}_2\\s_2} = \pmatrix{\vec{v}_1+\vec{v}_2\\s_1+s_2} $$
            => new Quaternion(Vector+other.Vector, Scalar+other.Scalar, Layout);

        /// <summary>
        /// Scales this quaternion by a scalar value.
        /// </summary>
        /// <param name="factor">The scalar factor.</param>
        /// <returns>The resulting quaternion (with the same layout as this)</returns>
        public Quaternion Scale(double factor) 
            //tex:$$\lambda \pmatrix{\vec{v} \\ s} = \pmatrix{\lambda \vec{v} \\ \lambda s}$$
            => new Quaternion(factor*Vector, factor*Scalar, Layout);

        /// <summary>
        /// Quaternion division
        /// </summary>
        /// <param name="numerator">The numerator, default=1.</param>
        /// <returns>Quaternion inverse times the numerator.</returns>
        public Quaternion Reciprocal(double numerator=1) 
            //tex:$$=\lambda \pmatrix{\vec{v} \\s}^{-1}$$
            => numerator*Inverse();


        public bool IsPure => Scalar == 0;
        public bool IsZero => Scalar == 0 && Vector.IsZero;
        public bool IsIdentity => Scalar ==1 && Vector.IsZero;
        public bool IsUnit => Scalar * Scalar + Vector.SumSquares==1;


        public Quaternion Product(Quaternion other)
        {
            //tex:Quaternion product$$\pmatrix{\vec{v}_1\\s_1} \otimes \pmatrix{\vec{v}_2\\s_2} 
            // = \left| \matrix{s_1 + \vec{v}_1\times & \vec{v}_1 \\ -\vec{v}_1^\intercal & s_1} \right| \pmatrix{\vec{v}_2\\s_2}
            // = \pmatrix{s_1 \vec{v}_2 + s_2 \vec{v}_1 + \vec{v}_1 \times \vec{v}_2 \\ s_1 s_2 - \vec{v}_1 \cdot \vec{v}_2}$$

            return new Quaternion(
                (Vector ^ other.Vector) + Scalar*other.Vector+other.Scalar*Vector,
                Scalar*other.Scalar - (Vector * other.Vector), Layout);
        }
        /// <summary>
        /// Quaternion inner product (also known as the dot product).
        /// </summary>
        /// <param name="other">The other quaternion.</param>
        /// <returns>The dot product of the vectors plus the product of the scalar values.</returns>
        public double Inner(Quaternion other) 
            //tex:$=\vec{v}_1 \cdot \vec{v}_2 + s_1 s_2$
            => (Vector* other.Vector)+Scalar*other.Scalar;

        /// <summary>
        /// Quaternion magnitude is the square root of the sum of squares.
        /// </summary>
        public double Magnitude
            //tex:$=\sqrt{|\vec{v}|^2 + s^2}$
            => Math.Sqrt(SumSquares);

        /// <summary>
        /// Quaternion sum of squares is the inner product with itself.
        /// </summary>
        /// <remarks>This is also known as the quadrature.</remarks>
        public double SumSquares 
            //tex:$=|\vec{v}|^2 + s^2$
            => Vector.SumSquares+Scalar*Scalar;

        /// <summary>
        /// Convert this quaternion to a unit quaternion.
        /// </summary>
        public Quaternion Unit()
        {
            //tex:$$=\frac{1}{\sqrt{s^2+|\vec{v}|^2}} \pmatrix{\vec{v}\\s}$$
            double m2 = SumSquares;
            if(m2>0)
            {
                return this/Math.Sqrt(m2);
            }
            return this;
        }

        /// <summary>
        /// Creates the conjugate of this quaternion
        /// </summary>
        /// <returns>The quaternion <c>(Scalar,-Vector)</c></returns>
        public Quaternion Conjugate() 
            //tex:$$\pmatrix{\vec{v}\\s}^\star = \pmatrix{-\vec{v}\\s}$$
            => new Quaternion(-Vector, Scalar, Layout);

        /// <summary>
        /// Computes the inverse quaternion.
        /// </summary>
        /// <returns>The quaternion <c>(Scalar,-Vector)/(SumSquares)</c></returns>
        public Quaternion Inverse()
            //tex:Quaternion Inverse$$\pmatrix{\vec{v}\\s}^{-1} = \frac{1}{s^2+|\vec{v}|^2} \pmatrix{-\vec{v}\\s}$$
            => Conjugate()/SumSquares;

        /// <summary>
        /// Interpolates a quartenrion towards a target, using the SLERP method.
        /// </summary>
        /// <param name="target">The target quaternion.</param>
        /// <param name="ratio">The interpolation ratio. 
        /// When 0 it returns this object,
        /// Wehn 1 it returns the target object.</param>
        /// <remarks>
        /// More accurare version of q-> (1-ratio)*q_1 + ratio*q_2 since it maintains
        /// follows along a sphere instead of a line.
        /// </remarks>
        public Quaternion Interpolate(Quaternion target, double ratio) 
            => this*(Conjugate()*target).Pow(ratio);

        /// <summary>
        /// Interpolates between two quaternions using a spline
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="ω_1">The ω 1.</param>
        /// <param name="ω_2">The ω 2.</param>
        /// <param name="h">The h.</param>
        /// <param name="t">The t.</param>
        /// <param name="normalize"><c>true</c> to always return a unit vector.</param>
        public Quaternion Interpolate(Quaternion target, Vector3 ω_1, Vector3 ω_2, double h, double t, bool normalize = true)
        {
            var qp_1 = Derivative(ω_1);
            var qp_2 = target.Derivative(ω_2);
            double t2 = t*t, t3 = t2*t;
            var q = (2*t3-2*t2+1)*this + h*t*(t2-2*t+1)*qp_1 +
                t2*(3-2*t)*target + h*t2*(t-1)*qp_2;
            if (normalize)
            {
                q = q.Unit();
            }
            return q;
        }

        /// <summary>
        /// Takes a step using constant angular velocity <paramref name="ω"/>
        /// </summary>
        /// <param name="h">The time step.</param>
        /// <param name="ω">The angular velocity vector.</param>
        /// <returns>A rotated quaterion</returns>
        public Quaternion FixedStep(double h, Vector3 ω)
        {
            var m = ω.Magnitude;
            if (m>0 && h!=0)
            {
                var q = Quaternion.FromAxisAngle(ω/m, h*m);
                return q.Product(this);
            }
            else
            {
                return this;
            }
        }

        #endregion

        #region Operators        
        public static Quaternion operator +(Quaternion lhs, Quaternion rhs) => lhs.Add(rhs);
        public static Quaternion operator -(Quaternion rhs) => rhs.Scale(-1);
        public static Quaternion operator -(Quaternion lhs, Quaternion rhs) => lhs.Add(-rhs);
        public static Quaternion operator *(double lhs, Quaternion rhs) => rhs.Scale(lhs);
        public static Quaternion operator *(Quaternion lhs, double rhs) => lhs.Scale(rhs);
        public static Quaternion operator /(Quaternion lhs, double rhs) => lhs.Scale(1/rhs);
        public static Quaternion operator *(Quaternion lhs, Quaternion rhs) => lhs.Product(rhs);
        public static Quaternion operator *(Matrix31 lhs, Quaternion rhs) => lhs.Product(rhs);
        public static Quaternion operator ~(Quaternion lhs) => lhs.Conjugate();
        public static Quaternion operator !(Quaternion lhs) => lhs.Inverse();
        public static Quaternion operator ^(Quaternion @base, double exponent) => @base.Pow(exponent);
        #endregion

        #region IEquatable Members

        /// <summary>
        /// Equality overrides from <see cref="System.Object"/>
        /// </summary>
        /// <param name="obj">The object to compare this with</param>
        /// <returns>False if object is a different type, otherwise it calls <code>Equals(Quaternion)</code></returns>
        public override bool Equals(object obj)
        {
            if(obj is Quaternion q)
            {
                return Equals(q);
            }
            return false;
        }
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.
        /// </returns>
        public bool Equals(Quaternion other)
        {
            return Scalar==other.Scalar
                && Vector.Equals(other.Vector);
        }
        public bool Equals(Quaternion other, double tol)
        {
            return Abs(Scalar - other.Scalar) <= tol
                && Vector.Equals(other.Vector, tol);
        }
        /// <summary>
        /// Calculates the hash code for the <see cref="Quaternion"/>
        /// </summary>
        /// <returns>The int hash value</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (17*23+Vector.GetHashCode())*23+Scalar.GetHashCode();
            }
        }
        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="other">The other.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Quaternion target, Quaternion other) => target.Equals(other);
        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="other">The other.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Quaternion target, Quaternion other) => !target.Equals(other);

        #endregion

        #region IEnumerable<double> Members        
        /// <summary>
        /// The number of elements in the quaternion.
        /// </summary>
        /// <value>
        /// Always the value 4.
        /// </value>
        public int Size => 4;
        /// <summary>
        /// Gets a value indicating whether this quaternion is read only.
        /// </summary>
        /// <value>
        ///   Always <c>true</c>.
        /// </value>
        public bool IsReadOnly => true;
        /// <summary>
        /// Get a value indicating whether this quaternion is fixed size.
        /// </summary>
        /// <value>
        ///   Always <c>true</c>.
        /// </value>
        public bool IsFixedSize => true;

        /// <summary>
        /// Extract an array of elements based on the <see cref="Layout"/> specified.
        /// </summary>
        /// <returns>Either <c>[x,y,z,w]</c> or <c>[w,x,y,z]</c></returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public double[] ToArray()
        {
            switch(Layout)
            {
                case QuaternionLayout.ScalarVector:
                    return new[] { Scalar, Vector.X, Vector.Y, Vector.Z };
                case QuaternionLayout.VectorScalar:
                    return new[] { Vector.X, Vector.Y, Vector.Z, Scalar };
                default:
                    throw new NotSupportedException();
            }
        }
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<double> GetEnumerator()
        {
            foreach(var item in ToArray())
            {
                yield return item;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion

        #region ICollection<double> Members
        void ICollection<double>.Add(double item) => throw new NotSupportedException();
        void ICollection<double>.Clear() => throw new NotSupportedException();
        bool ICollection<double>.Remove(double item) => throw new NotSupportedException();
        public bool Contains(double item) => Vector.Contains(item) || Scalar==item;
        public void CopyTo(double[] array, int arrayIndex) => Array.Copy(ToArray(), 0, array, arrayIndex, Count);
        void System.Collections.ICollection.CopyTo(Array array, int arrayIndex) => CopyTo(array as double[], arrayIndex);
        bool System.Collections.ICollection.IsSynchronized => false;
        object System.Collections.ICollection.SyncRoot => throw new InvalidOperationException();
        #endregion

    }

    /// <summary>
    /// Static extensions and helpers for quaternions
    /// </summary>
    public static class QuaterionAlgebra
    {
        /// <summary>
        /// Matrix-Quaternion product. <c>p=Matrix*Quaternion</c>
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="q">The quaternion.</param>
        /// <returns>A quaternion.</returns>
        public static Quaternion Product(this Matrix31 matrix, Quaternion q)
        {
            //tex:Matrix quaternion product
            //$$\left| \matrix{M & \vec{g} \\ \vec{h}^\intercal & c} \right| \pmatrix{\vec{v}\\s} 
            //            = \pmatrix{ M \vec{v} + s \vec{g} \\ \vec{h} \cdot \vec{v} + c\, s }$$
            return new Quaternion(
                matrix.Matrix*q.Vector + matrix.Vector1*q.Scalar,
                Vector3.Dot(matrix.Vector1, q.Vector) + matrix.Scalar*q.Scalar, q.Layout);
        }

        /// <summary>
        /// The Omega operator converts a rotation vector ω into a 4×4 quaternion
        /// product operator, such that <c>ω*q = Ω(ω)q</c>.
        /// </summary>
        /// <param name="ω">The rotation vector ω.</param>
        public static Matrix31 Omega(Vector3 ω)
            //tex:Omega operator
            //$$\Omega(\vec{\omega}) = \left| \matrix{ \vec{\omega}\times & \vec{\omega} \\ -\vec{\omega} & 0} \right| $$
            //tex:Such that
            //$$ \Omega(\vec{\omega})q = \pmatrix{0 \\ \vec{\omega}} \otimes q  $$
            => new Matrix31(ω.Cross(), ω, -ω, 0);

        public static Quaternion Log(this Quaternion q)
        {
            const double exact_mode = 0.001;

            var qm = q.Magnitude;
            var v = q.Vector;
            var vm = v.Magnitude;
            var t = q.Phase;

            if (vm>exact_mode)
            {
                return new Quaternion(Math.Log(qm), t*v/vm, q.Layout);
            }
            else
            {
                var t2 = t*t;
                var t4 = t2*t2;
                return new Quaternion(Math.Log(qm), v*(1+t2/6+7*t4/360)/qm, q.Layout);
            }
        }

        public static Quaternion Exp(this Quaternion q)
        {
            const double exact_mode = 0.001;

            var v = q.Vector;
            var vm = v.Magnitude;
            var s = q.Scalar;

            if (vm>exact_mode)
            {
                return new Quaternion(Math.Exp(s)*Math.Cos(vm), Math.Exp(s)*Math.Sin(vm)/vm*v,q.Layout);
            }
            else
            {
                var vm2 = vm*vm;
                var vm4 = vm2*vm2;
                return new Quaternion(Math.Exp(s)*Math.Cos(vm), Math.Exp(s)*(1-vm2/6+vm4/120)*v,q.Layout);
            }
        }

        public static Quaternion Pow(this Quaternion q, Quaternion p) => Exp(Log(q)*p);
        public static Quaternion Pow(this Quaternion q, double p) => Exp(Log(q)*p);


    }
}
