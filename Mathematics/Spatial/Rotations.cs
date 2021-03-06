using System;

using static System.Math;

namespace JA.Mathematics.Spatial
{
    using num = Double;
    using mat3 = Matrix3;


    /// <summary>
    /// Key to three axes
    /// </summary> 
    /// <remarks>Used in the constructor for <see cref="Rotation3"/></remarks>
    public enum AxisName
    {
        /// <summary>X-axis</summary>
        X,
        /// <summary>Y-axis</summary>
        Y,
        /// <summary>Z-axis</summary>
        Z
    }

    public enum RotationSequence
    {
        XYX,
        XYZ,
        XZX,
        XZY,
        YXY,
        YXZ,
        YZX,
        YZY,
        ZXY,
        ZXZ,
        ZYX,
        ZYZ
    }

    public static class Rotations
    {
        /*  Trigonometric Half Angles
         * 
         *  if  t = TAN(θ/2) then
         *      SIN(θ) = 2t/(1+t²)
         *      COS(θ) = (1-t²)/(1+t²)
         *      TAN(θ) = 2t/(1-t²)
         *      EXP(θ) = (1+t)/(1-t)
         *  Also
         *      t = SIN(θ)/(1+COS(θ))
         *      t = (1-COS(θ))/SIN(θ)
         */

        public static Matrix3 RotateX(double angle)
        {
            double s = Sin(angle);
            double c = Cos(angle);
            return new mat3(
                1, 0, 0,
                0, c, -s,
                0, s, c);
        }
        public static Matrix3 RotateY(double angle)
        {
            double s = Sin(angle);
            double c = Cos(angle);
            return new mat3(
                c, 0, s,
                0, 1, 0,
                -s, 0, c);
        }
        public static Matrix3 RotateZ(double angle)
        {
            double s = Sin(angle);
            double c = Cos(angle);
            return new mat3(
                c, -s, 0,
                s, c, 0,
                0, 0, 1);
        }

        public static Matrix3 FromAxisRotation(AxisName axis, double angle)
        {
            switch( axis )
            {
                case AxisName.X:
                    return RotateX(angle);
                case AxisName.Y:
                    return RotateY(angle);
                case AxisName.Z:
                    return RotateZ(angle);
                default:
                    return Matrix3.Identity;
            }
        }

        public static Matrix3 FromAxisRotation(Vector3 axis, double angle)
        {

            Vector3 e = axis.Normalized();
            Matrix3 ee = Vector3.Outer(e, e);

            double c = Cos(angle);
            double s = Sin(angle);
            
            return c*Matrix3.Identity - s*e.Cross() + (1f - c) * ee;
        }

        public static Matrix3 BodySequence(RotationSequence seq, params num[] angles)
        {
            int N = angles.Length;
            num theta = N > 0 ? angles[0] : 0;
            num phi = N > 1 ? angles[1] : 0;
            num psi = N > 2 ? angles[2] : 0;
            switch( seq )
            {
                case RotationSequence.XYX:
                    return RotateX(theta) * RotateY(phi) * RotateX(psi);
                case RotationSequence.XYZ:
                    return RotateZ(theta) * RotateY(phi) * RotateX(psi);
                case RotationSequence.XZX:
                    return RotateX(theta) * RotateZ(phi) * RotateX(psi);
                case RotationSequence.XZY:
                    return RotateY(theta) * RotateZ(phi) * RotateX(psi);
                case RotationSequence.YXY:
                    return RotateY(theta) * RotateX(phi) * RotateY(psi);
                case RotationSequence.YXZ:
                    return RotateZ(theta) * RotateX(phi) * RotateY(psi);
                case RotationSequence.YZX:
                    return RotateX(theta) * RotateZ(phi) * RotateY(psi);
                case RotationSequence.YZY:
                    return RotateY(theta) * RotateZ(phi) * RotateY(psi);
                case RotationSequence.ZXY:
                    return RotateY(theta) * RotateX(phi) * RotateZ(psi);
                case RotationSequence.ZXZ:
                    return RotateZ(theta) * RotateX(phi) * RotateZ(psi);
                case RotationSequence.ZYX:
                    return RotateX(theta) * RotateY(phi) * RotateZ(psi);
                case RotationSequence.ZYZ:
                    return RotateZ(theta) * RotateY(phi) * RotateZ(psi);
                default:
                    return Matrix3.Identity;
            }
        }

        public static Matrix3 WorldSequence(RotationSequence seq, params num[] angles)
        {
            int N = angles.Length;
            num theta = N>0 ? angles[0] : 0;
            num phi = N>1 ? angles[1] : 0;
            num psi = N>2 ? angles[2] : 0;
            switch( seq )
            {
                case RotationSequence.XYX:
                    return RotateX(theta) * RotateY(phi) * RotateX(psi);
                case RotationSequence.XYZ:
                    return RotateX(theta) * RotateY(phi) * RotateZ(psi);
                case RotationSequence.XZX:
                    return RotateX(theta) * RotateZ(phi) * RotateX(psi);
                case RotationSequence.XZY:
                    return RotateX(theta) * RotateZ(phi) * RotateY(psi);
                case RotationSequence.YXY:
                    return RotateY(theta) * RotateX(phi) * RotateY(psi);
                case RotationSequence.YXZ:
                    return RotateY(theta) * RotateX(phi) * RotateZ(psi);
                case RotationSequence.YZX:
                    return RotateY(theta) * RotateZ(phi) * RotateX(psi);
                case RotationSequence.YZY:
                    return RotateY(theta) * RotateZ(phi) * RotateY(psi);
                case RotationSequence.ZXY:
                    return RotateZ(theta) * RotateX(phi) * RotateY(psi);
                case RotationSequence.ZXZ:
                    return RotateZ(theta) * RotateX(phi) * RotateZ(psi);
                case RotationSequence.ZYX:
                    return RotateZ(theta) * RotateY(phi) * RotateX(psi);
                case RotationSequence.ZYZ:
                    return RotateZ(theta) * RotateY(phi) * RotateZ(psi);
                default:
                    return Matrix3.Identity;
            }
        }

        public static Vector3 GetAxis(Matrix3 rot)
        {
            // θ = 0        ->  zeta = 1.0
            // 0 < θ < π    ->  zeta = cos(θ)
            // θ = π        ->  zeta = -1.0
            double zeta = (rot.A11 + rot.A22 + rot.A33 - 1f) / 2f;
            if (zeta == 1.0f)
            {
                return Vector3.Zero;
            }
            else if (zeta == -1.0f)
            {
                var list = new[] { rot.A11, rot.A22, rot.A33 };
                var max = System.Linq.Enumerable.Max(list);
                int i = Array.IndexOf(list, max);
                if (i == 0)
                {
                    double w1 = Sqrt(rot.A11 - rot.A22 - rot.A33 + 1f) / 2f;
                    double w2 = rot.A12 / (2 * w1);
                    double w3 = rot.A13 / (2 * w1);
                    return Vector3.Cartesian(w1, w2, w3);
                }
                else if (i == 1)
                {
                    double w2 = Sqrt(rot.A22 - rot.A11 - rot.A33 + 1f) / 2f;
                    double w1 = rot.A12 / (2 * w2);
                    double w3 = rot.A23 / (2 * w2);
                    return Vector3.Cartesian(w1, w2, w3);
                }
                else
                {
                    double w3 = Sqrt(rot.A33 - rot.A22 - rot.A11 + 1f) / 2f;
                    double w1 = rot.A13 / (2 * w3);
                    double w2 = rot.A23 / (2 * w3);
                    return Vector3.Cartesian(w1, w2, w3);
                }
            }
            else
            {
                return Vector3.Cartesian(rot.A32 - rot.A23, rot.A13 - rot.A31, rot.A21 - rot.A12).Normalized();
            }
        }

        public static double GetAngle(Matrix3 rot, Vector3 axis)
        {
            // θ = 0        ->  zeta = 1.0
            // 0 < θ < π    ->  zeta = cos(θ)
            // θ = π        ->  zeta = -1.0
            axis = axis.Normalized();
            double zeta = Vector3.Dot(axis, rot * axis);
            if (zeta == 1.0f)
            {
                return 0f;
            }
            else if (zeta == -1.0f)
            {
                return -(float)PI;
            }
            else
            {
                return Acos(zeta);
            }
        }

        public static double GetAngle(Matrix3 rot)
        {
            // θ = 0        ->  zeta = 1.0
            // 0 < θ < π    ->  zeta = cos(θ)
            // θ = π        ->  zeta = -1.0
            double zeta = (rot.A11 + rot.A22 + rot.A33 - 1f) / 2f;
            if (zeta == 1.0f)
            {
                return 0f;
            }
            else if (zeta == -1.0f)
            {
                return -(float)PI;
            }
            else
            {
                return Acos(zeta);
            }
        }

        public static double[] GetEulerZYZ(Matrix3 rot)
        {
            double phi = Atan2(rot.A31, rot.A32);
            double theta = Acos(rot.A33);
            double psi = -Atan2(rot.A13, rot.A23);
            return new double[] { phi, theta, psi };
        }

        public static Quaternion ToQuaternion(Matrix3 rot)
        {
            return Quaternion.FromRotationMatrix(rot);
        }

        public static Matrix3 AlignZX(Vector3 unit_z, Vector3 unit_x)
        {
            unit_x = unit_x.Normalized();
            unit_z = unit_z.Normalized();
            Vector3 unit_y = Vector3.Cross(unit_z, unit_x);
            unit_x = Vector3.Cross(unit_y, unit_z);
            return Matrix3.Combine(unit_x, unit_y, unit_z);
        }
        public static Matrix3 AlignXY(Vector3 unit_x, Vector3 unit_y)
        {
            unit_x = unit_x.Normalized();
            unit_y = unit_y.Normalized();
            Vector3 unit_z = Vector3.Cross(unit_x, unit_y);
            unit_y = Vector3.Cross(unit_z, unit_x);
            return Matrix3.Combine(unit_x, unit_y, unit_z);
        }
        public static Matrix3 AlignYZ(Vector3 unit_y, Vector3 unit_z)
        {
            unit_y = unit_y.Normalized();
            unit_z = unit_z.Normalized();
            Vector3 unit_x = Vector3.Cross(unit_y, unit_z);
            unit_z = Vector3.Cross(unit_x, unit_y);
            return Matrix3.Combine(unit_x, unit_y, unit_z);
        }
    }
}
