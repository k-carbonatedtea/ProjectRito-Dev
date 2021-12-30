﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace GLFrameworkEngine
{
    public static class GLMath
    {
        public static float Deg2Rad = (float)(System.Math.PI * 2) / 360;
        public static float Rad2Deg = (float)(360 / (System.Math.PI * 2));

        //From https://github.com/Ploaj/SSBHLib/blob/e37b0d83cd088090f7802be19b1d05ec998f2b6a/CrossMod/Tools/CrossMath.cs#L42
        //Seems to give good results
        public static Vector3 ToEulerAngles(double X, double Y, double Z, double W)
        {
            return ToEulerAngles(new Quaternion((float)X, (float)Y, (float)Z, (float)W));
        }

        public static Vector3 ToEulerAngles(float X, float Y, float Z, float W)
        {
            return ToEulerAngles(new Quaternion(X, Y, Z, W));
        }

        public static Vector3 ToEulerAngles(Quaternion q)
        {
            Matrix4 mat = Matrix4.CreateFromQuaternion(q);
            float x, y, z;
            y = (float)Math.Asin(Clamp(mat.M13, -1, 1));

            if (Math.Abs(mat.M13) < 0.99999)
            {
                x = (float)Math.Atan2(-mat.M23, mat.M33);
                z = (float)Math.Atan2(-mat.M12, mat.M11);
            }
            else
            {
                x = (float)Math.Atan2(mat.M32, mat.M22);
                z = 0;
            }
            return new Vector3(x, y, z) * -1;
        }

        public static Quaternion FromEulerAngles(Vector3 rotation)
        {
            Quaternion xRotation = Quaternion.FromAxisAngle(Vector3.UnitX, rotation.X);
            Quaternion yRotation = Quaternion.FromAxisAngle(Vector3.UnitY, rotation.Y);
            Quaternion zRotation = Quaternion.FromAxisAngle(Vector3.UnitZ, rotation.Z);
            Quaternion q = (zRotation * yRotation * xRotation);

            if (q.W < 0)
                q *= -1;

            return q;
        }

        public static float Clamp(float v, float min, float max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }

        public static int Clamp(int v, int min, int max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }

        public static bool RayIntersectsLineSphere(CameraRay ray, Vector3 center, float radius, out Vector3 result)
        {
            Vector3d resultDouble = new Vector3d();
            bool intersects = RayIntersectsLineSphere(ray, (Vector3d)center, radius, out resultDouble);

            result = (Vector3)resultDouble;
            return intersects;
        }
        
        public static bool RayIntersectsLineSphere(CameraRay ray, Vector3d center, float radius, out Vector3d result)
        {
            var p1 = (Vector3d)ray.Origin.Xyz;
            var p2 = (Vector3d)ray.Far.Xyz;

            var d = p2 - p1;
            double a = Vector3d.Dot(d, d);

            if (a > 0.0f)
            {
                double b = 2 * Vector3d.Dot(d, p1 - center);
                double c = (Vector3d.Dot(center, center) + Vector3d.Dot(p1, p1)) - (2 * Vector3d.Dot(center, p1)) - (radius * radius);

                double magnitude = (b * b) - (4 * a * c);

                if (magnitude >= 0.0f)
                {
                    magnitude = (float)Math.Sqrt(magnitude);
                    a *= 2;

                    double scale = (-b + magnitude) / a;
                    double dist2 = (-b - magnitude) / a;

                    if (dist2 < scale)
                        scale = dist2;

                    result = (p1 + (d * scale));
                    return true;
                }
            }

            result = new Vector3d();
            return false;
        }

        //https://github.com/Sage-of-Mirrors/WindEditor/blob/0da4cc95cb5a013593c6c8f25baf150f46704c66/WCommon/Math/Math.cs#L89
        public static bool RayIntersectsAABB(CameraRay ray, Vector3 aabbMin, Vector3 aabbMax, out float intersectionDistance)
        {
            Vector3 t_1 = new Vector3(), t_2 = new Vector3();

            float tNear = float.MinValue;
            float tFar = float.MaxValue;

            // Test infinite planes in each directin.
            for (int i = 0; i < 3; i++)
            {
                // Ray is parallel to planes in this direction.
                if (ray.Direction[i] == 0)
                {
                    if ((ray.Origin[i] < aabbMin[i]) || (ray.Origin[i] > aabbMax[i]))
                    {
                        // Parallel and outside of the box, thus no intersection is possible.
                        intersectionDistance = float.MinValue;
                        return false;
                    }
                }
                else
                {
                    t_1[i] = (aabbMin[i] - ray.Origin[i]) / ray.Direction[i];
                    t_2[i] = (aabbMax[i] - ray.Origin[i]) / ray.Direction[i];

                    // Ensure T_1 holds values for intersection with near plane.
                    if (t_1[i] > t_2[i])
                    {
                        Vector3 temp = t_2;
                        t_2 = t_1;
                        t_1 = temp;
                    }

                    if (t_1[i] > tNear)
                        tNear = t_1[i];

                    if (t_2[i] < tFar)
                        tFar = t_2[i];

                    if ((tNear > tFar) || (tFar < 0))
                    {
                        intersectionDistance = float.MinValue;
                        return false;
                    }
                }
            }

            intersectionDistance = tNear;
            return true;
        }
    }
}
