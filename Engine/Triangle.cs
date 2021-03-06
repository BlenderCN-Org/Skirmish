﻿using Engine.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    /// <summary>
    /// Triangle
    /// </summary>
    public struct Triangle : IVertexList, IRayIntersectable
    {
        /// <summary>
        /// First point
        /// </summary>
        public Vector3 Point1 { get; set; }
        /// <summary>
        /// Second point
        /// </summary>
        public Vector3 Point2 { get; set; }
        /// <summary>
        /// Third point
        /// </summary>
        public Vector3 Point3 { get; set; }
        /// <summary>
        /// Center
        /// </summary>
        public Vector3 Center { get; set; }
        /// <summary>
        /// First index
        /// </summary>
        public int I1 { get; set; }
        /// <summary>
        /// Second index
        /// </summary>
        public int I2 { get; set; }
        /// <summary>
        /// Plane
        /// </summary>
        public Plane Plane { get; set; }
        /// <summary>
        /// Normal
        /// </summary>
        public Vector3 Normal
        {
            get
            {
                return this.Plane.Normal;
            }
        }
        /// <summary>
        /// Min
        /// </summary>
        public Vector3 Min
        {
            get
            {
                return Vector3.Min(this.Point1, Vector3.Min(this.Point2, this.Point3));
            }
        }
        /// <summary>
        /// Max
        /// </summary>
        public Vector3 Max
        {
            get
            {
                return Vector3.Max(this.Point1, Vector3.Max(this.Point2, this.Point3));
            }
        }
        /// <summary>
        /// Triangle area
        /// </summary>
        /// <remarks>Heron</remarks>
        public float Area
        {
            get
            {
                float a = (this.Point1 - this.Point2).Length();
                float b = (this.Point1 - this.Point3).Length();
                float c = (this.Point2 - this.Point3).Length();

                float p = (a + b + c) * 0.5f;
                float z = p * (p - a) * (p - b) * (p - c);

                return (float)Math.Sqrt(z);
            }
        }
        /// <summary>
        /// Inclination angle
        /// </summary>
        public float Inclination
        {
            get
            {
                return Helper.Angle(this.Normal, Vector3.Down);
            }
        }
        /// <summary>
        /// Returns the triangle vertex by index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Returns a triangle vertex</returns>
        public Vector3 this[int index]
        {
            get
            {
                if (index == 0) return Point1;
                if (index == 1) return Point2;
                if (index == 2) return Point3;

                return Vector3.Zero;
            }
        }

        /// <summary>
        /// Generate a triangle list from vertices
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="vertices">Vertices</param>
        /// <returns>Returns the triangle list</returns>
        public static IEnumerable<Triangle> ComputeTriangleList(Topology topology, IEnumerable<Vector3> vertices)
        {
            List<Triangle> triangleList = new List<Triangle>();

            if (topology == Topology.TriangleList)
            {
                var tmpVerts = vertices.ToArray();

                for (int i = 0; i < tmpVerts.Length; i += 3)
                {
                    Triangle tri = new Triangle(
                        tmpVerts[i + 0],
                        tmpVerts[i + 1],
                        tmpVerts[i + 2]);

                    triangleList.Add(tri);
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            return triangleList.ToArray();
        }
        /// <summary>
        /// Generate a triangle list from vertices and indices
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="vertices">Vertices</param>
        /// <param name="indices">Indices</param>
        /// <returns>Returns the triangle list</returns>
        public static IEnumerable<Triangle> ComputeTriangleList(Topology topology, IEnumerable<Vector3> vertices, IEnumerable<uint> indices)
        {
            List<Triangle> triangleList = new List<Triangle>();

            if (topology == Topology.TriangleList)
            {
                var tmpVerts = vertices.ToArray();
                var tmpIndxs = indices.ToArray();

                for (int i = 0; i < tmpIndxs.Length; i += 3)
                {
                    Triangle tri = new Triangle(
                        tmpVerts[tmpIndxs[i + 0]],
                        tmpVerts[tmpIndxs[i + 1]],
                        tmpVerts[tmpIndxs[i + 2]]);

                    triangleList.Add(tri);
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            return triangleList.ToArray();
        }
        /// <summary>
        /// Generate a triangle list from AABB
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="bbox">AABB</param>
        /// <returns>Returns the triangle list</returns>
        public static IEnumerable<Triangle> ComputeTriangleList(Topology topology, BoundingBox bbox)
        {
            List<Triangle> triangleList = new List<Triangle>();

            if (topology == Topology.TriangleList)
            {
                var v = new Vector3[24];

                float xm = bbox.Minimum.X;
                float ym = bbox.Minimum.Y;
                float zm = bbox.Minimum.Z;

                float xM = bbox.Maximum.X;
                float yM = bbox.Maximum.Y;
                float zM = bbox.Maximum.Z;

                // Fill in the front face vertex data.
                v[0] = new Vector3(xm, ym, zm);
                v[1] = new Vector3(xm, yM, zm);
                v[2] = new Vector3(xM, yM, zm);
                v[3] = new Vector3(xM, ym, zm);

                // Fill in the back face vertex data.
                v[4] = new Vector3(xm, ym, zM);
                v[5] = new Vector3(xM, ym, zM);
                v[6] = new Vector3(xM, yM, zM);
                v[7] = new Vector3(xm, yM, zM);

                // Fill in the top face vertex data.
                v[8] = new Vector3(xm, yM, zm);
                v[9] = new Vector3(xm, yM, zM);
                v[10] = new Vector3(xM, yM, zM);
                v[11] = new Vector3(xM, yM, zm);

                // Fill in the bottom face vertex data.
                v[12] = new Vector3(xm, ym, zm);
                v[13] = new Vector3(xM, ym, zm);
                v[14] = new Vector3(xM, ym, zM);
                v[15] = new Vector3(xm, ym, zM);

                // Fill in the left face vertex data.
                v[16] = new Vector3(xm, ym, zM);
                v[17] = new Vector3(xm, yM, zM);
                v[18] = new Vector3(xm, yM, zm);
                v[19] = new Vector3(xm, ym, zm);

                // Fill in the right face vertex data.
                v[20] = new Vector3(xM, ym, zm);
                v[21] = new Vector3(xM, yM, zm);
                v[22] = new Vector3(xM, yM, zM);
                v[23] = new Vector3(xM, ym, zM);

                // Fill in the front face index data
                triangleList.Add(new Triangle(v[0], v[1], v[2]));
                triangleList.Add(new Triangle(v[0], v[2], v[3]));

                // Fill in the back face index data
                triangleList.Add(new Triangle(v[4], v[5], v[6]));
                triangleList.Add(new Triangle(v[4], v[6], v[7]));

                // Fill in the top face index data
                triangleList.Add(new Triangle(v[8], v[9], v[10]));
                triangleList.Add(new Triangle(v[8], v[10], v[11]));

                // Fill in the bottom face index data
                triangleList.Add(new Triangle(v[12], v[13], v[14]));
                triangleList.Add(new Triangle(v[12], v[14], v[15]));

                // Fill in the left face index data
                triangleList.Add(new Triangle(v[16], v[17], v[18]));
                triangleList.Add(new Triangle(v[16], v[18], v[19]));

                // Fill in the right face index data
                triangleList.Add(new Triangle(v[20], v[21], v[22]));
                triangleList.Add(new Triangle(v[20], v[22], v[23]));
            }
            else
            {
                throw new NotImplementedException();
            }

            return triangleList.ToArray();
        }
        /// <summary>
        /// Generate a triangle list from OBB
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="obb">OBB</param>
        /// <returns>Returns the triangle list</returns>
        public static IEnumerable<Triangle> ComputeTriangleList(Topology topology, OrientedBoundingBox obb)
        {
            List<Triangle> triangleList = new List<Triangle>();

            if (topology == Topology.TriangleList)
            {
                Vector3[] v = obb.GetCorners();

                // Fill in the front face index data
                triangleList.Add(new Triangle(v[0], v[1], v[2]));
                triangleList.Add(new Triangle(v[0], v[2], v[3]));

                // Fill in the back face index data
                triangleList.Add(new Triangle(v[4], v[6], v[5]));
                triangleList.Add(new Triangle(v[4], v[7], v[6]));

                // Fill in the top face index data
                triangleList.Add(new Triangle(v[0], v[3], v[7]));
                triangleList.Add(new Triangle(v[0], v[7], v[4]));

                // Fill in the bottom face index data
                triangleList.Add(new Triangle(v[1], v[6], v[2]));
                triangleList.Add(new Triangle(v[1], v[5], v[6]));

                // Fill in the left face index data
                triangleList.Add(new Triangle(v[3], v[2], v[6]));
                triangleList.Add(new Triangle(v[3], v[6], v[7]));

                // Fill in the right face index data
                triangleList.Add(new Triangle(v[0], v[5], v[1]));
                triangleList.Add(new Triangle(v[0], v[4], v[5]));
            }
            else
            {
                throw new NotImplementedException();
            }

            return triangleList.ToArray();
        }
        /// <summary>
        /// Generate a triangle list from cylinder
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="cylinder">Cylinder</param>
        /// <param name="segments">Number of segments</param>
        /// <returns>Returns the triangle list</returns>
        public static IEnumerable<Triangle> ComputeTriangleList(Topology topology, BoundingCylinder cylinder, int segments)
        {
            List<Triangle> triangleList = new List<Triangle>();

            if (topology == Topology.TriangleList)
            {
                var verts = cylinder.GetVertices(segments);

                for (int i = 0; i < segments - 2; i++)
                {
                    triangleList.Add(new Triangle(verts[0], verts[i + 2], verts[i + 1]));
                    triangleList.Add(new Triangle(verts[0 + segments], verts[i + 1 + segments], verts[i + 2 + segments]));
                }

                for (int i = 0; i < segments; i++)
                {
                    if (i == segments - 1)
                    {
                        triangleList.Add(new Triangle(verts[i], verts[0], verts[i + segments]));
                        triangleList.Add(new Triangle(verts[0], verts[0 + segments], verts[i + segments]));
                    }
                    else
                    {
                        triangleList.Add(new Triangle(verts[i], verts[i + 1], verts[i + segments]));
                        triangleList.Add(new Triangle(verts[i + 1], verts[i + 1 + segments], verts[i + segments]));
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            return triangleList.ToArray();
        }
        /// <summary>
        /// Generate a triangle list from polygon
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="poly">Polygon</param>
        /// <returns>Returns the triangle list</returns>
        public static IEnumerable<Triangle> ComputeTriangleList(Topology topology, Polygon poly)
        {
            if (topology == Topology.TriangleList)
            {
                return poly.Triangulate();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Transform triangle coordinates
        /// </summary>
        /// <param name="triangle">Triangle</param>
        /// <param name="transform">Transformation</param>
        /// <returns>Returns new triangle</returns>
        public static Triangle Transform(Triangle triangle, Matrix transform)
        {
            return new Triangle(
                Vector3.TransformCoordinate(triangle.Point1, transform),
                Vector3.TransformCoordinate(triangle.Point2, transform),
                Vector3.TransformCoordinate(triangle.Point3, transform));
        }
        /// <summary>
        /// Transform triangle list coordinates
        /// </summary>
        /// <param name="triangles">Triangle list</param>
        /// <param name="transform">Transformation</param>
        /// <returns>Returns new triangle list</returns>
        public static IEnumerable<Triangle> Transform(IEnumerable<Triangle> triangles, Matrix transform)
        {
            if (triangles?.Any() != true)
            {
                return triangles;
            }

            List<Triangle> res = new List<Triangle>(triangles.Count());

            foreach (var tri in triangles)
            {
                res.Add(Transform(tri, transform));
            }

            return res;
        }
        /// <summary>
        /// Reverses the normal of all the triangles of the list
        /// </summary>
        /// <param name="triangles">Triangle list</param>
        /// <returns>Returns a new triangle list</returns>
        public static IEnumerable<Triangle> Reverse(IEnumerable<Triangle> triangles)
        {
            if (triangles?.Any() != true)
            {
                return triangles;
            }

            List<Triangle> res = new List<Triangle>(triangles.Count());

            foreach (var tri in triangles)
            {
                res.Add(tri.ReverseNormal());
            }

            return res.ToArray();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="point1">Point 1</param>
        /// <param name="point2">Point 2</param>
        /// <param name="point3">Point 3</param>
        public Triangle(Vector3 point1, Vector3 point2, Vector3 point3)
        {
            this.Point1 = point1;
            this.Point2 = point2;
            this.Point3 = point3;
            this.Center = Vector3.Multiply(point1 + point2 + point3, 1.0f / 3.0f);
            this.Plane = new Plane(this.Point1, this.Point2, this.Point3);

            Vector3 n = this.Plane.Normal;
            float absX = Math.Abs(n.X);
            float absY = Math.Abs(n.Y);
            float absZ = Math.Abs(n.Z);

            Vector3 a = new Vector3(absX, absY, absZ);
            if (a.X > a.Y)
            {
                if (a.X > a.Z)
                {
                    this.I1 = 1;
                    this.I2 = 2;
                }
                else
                {
                    this.I1 = 0;
                    this.I2 = 1;
                }
            }
            else
            {
                if (a.Y > a.Z)
                {
                    this.I1 = 0;
                    this.I2 = 2;
                }
                else
                {
                    this.I1 = 0;
                    this.I2 = 1;
                }
            }
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="points">Point array</param>
        public Triangle(Vector3[] points) : this(points[0], points[1], points[2])
        {

        }

        /// <summary>
        /// Text representation
        /// </summary>
        public override string ToString()
        {
            return string.Format("Vertex 1 {0}; Vertex 2 {1}; Vertex 3 {2};", this.Point1, this.Point2, this.Point3);
        }

        /// <summary>
        /// Intersection test between ray and triangle
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <returns>Returns true if ray intersects with this triangle</returns>
        public bool Intersects(Ray ray)
        {
            return this.Intersects(ray, false, out _, out _);
        }
        /// <summary>
        /// Intersection test between ray and triangle
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="distance">Distance from ray origin and intersection point, if any</param>
        /// <returns>Returns true if ray intersects with this triangle</returns>
        public bool Intersects(Ray ray, out float distance)
        {
            return this.Intersects(ray, false, out _, out distance);
        }
        /// <summary>
        /// Intersection test between ray and triangle
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="point">Intersection point, if any</param>
        /// <param name="distance">Distance from ray origin and intersection point, if any</param>
        /// <returns>Returns true if ray intersects with this triangle</returns>
        public bool Intersects(Ray ray, out Vector3 point, out float distance)
        {
            return this.Intersects(ray, false, out point, out distance);
        }
        /// <summary>
        /// Intersection test between ray and triangle
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Test facing only triangles</param>
        /// <returns>Returns true if ray intersects with this triangle</returns>
        public bool Intersects(Ray ray, bool facingOnly)
        {
            return this.Intersects(ray, facingOnly, out _, out _);
        }
        /// <summary>
        /// Intersection test between ray and triangle
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Test facing only triangles</param>
        /// <param name="distance">Distance from ray origin and intersection point, if any</param>
        /// <returns>Returns true if ray intersects with this triangle</returns>
        public bool Intersects(Ray ray, bool facingOnly, out float distance)
        {
            return this.Intersects(ray, facingOnly, out _, out distance);
        }
        /// <summary>
        /// Intersection test between ray and triangle
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Test facing only triangles</param>
        /// <param name="point">Intersection point, if any</param>
        /// <param name="distance">Distance from ray origin and intersection point, if any</param>
        /// <returns>Returns true if ray intersects with this triangle</returns>
        public bool Intersects(Ray ray, bool facingOnly, out Vector3 point, out float distance)
        {
            point = Vector3.Zero;
            distance = 0;

            bool cull = false;
            if (facingOnly)
            {
                cull = Vector3.Dot(ray.Direction, this.Normal) >= 0f;
            }

            if (!cull)
            {
                var p1 = Point1;
                var p2 = Point2;
                var p3 = Point3;
                return Intersection.RayIntersectsTriangle(ref ray, ref p1, ref p2, ref p3, out point, out distance);
            }

            return false;
        }

        /// <summary>
        /// Retrieves the three vertices of the triangle.
        /// </summary>
        /// <returns>An array of points representing the three vertices of the triangle.</returns>
        public Vector3[] GetVertices()
        {
            return new[]
            {
                this.Point1,
                this.Point2,
                this.Point3,
            };
        }

        /// <summary>
        /// Gets a new triangle with reversed normal vector
        /// </summary>
        /// <returns>Returns a new revered normal triangle</returns>
        public Triangle ReverseNormal()
        {
            return new Triangle(Point1, Point3, Point2);
        }
    }
}
