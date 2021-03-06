﻿using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Contains triangle meshes that represent detailed height data associated with the polygons in its associated polygon mesh object.
    /// </summary>
    class PolyMeshDetail
    {
        /// <summary>
        /// Builds a new polygon mesh detail
        /// </summary>
        /// <param name="mesh">Polygon mesh</param>
        /// <param name="chf">Compact heightfield</param>
        /// <param name="sampleDist">Sample distance</param>
        /// <param name="sampleMaxError">Sample maximum error</param>
        /// <returns>Returns the new polygon mesh detail</returns>
        public static PolyMeshDetail Build(PolyMesh mesh, CompactHeightfield chf, float sampleDist, float sampleMaxError)
        {
            PolyMeshDetail dmesh;

            if (mesh.NVerts == 0 || mesh.NPolys == 0)
            {
                return null;
            }

            int nvp = mesh.NVP;
            float cs = mesh.CS;
            float ch = mesh.CH;
            Vector3 orig = mesh.BMin;
            int borderSize = mesh.BorderSize;
            int heightSearchRadius = Math.Max(1, (int)Math.Ceiling(mesh.MaxEdgeError));

            HeightPatch hp = new HeightPatch();
            int nPolyVerts = 0;
            int maxhw = 0, maxhh = 0;

            Int4[] bounds = new Int4[mesh.NPolys];

            // Find max size for a polygon area.
            for (int i = 0; i < mesh.NPolys; ++i)
            {
                var p = mesh.Polys[i];
                int xmin = chf.Width;
                int xmax = 0;
                int ymin = chf.Height;
                int ymax = 0;
                for (int j = 0; j < nvp; ++j)
                {
                    if (p[j] == IndexedPolygon.RC_MESH_NULL_IDX) break;
                    var v = mesh.Verts[p[j]];
                    xmin = Math.Min(xmin, v.X);
                    xmax = Math.Max(xmax, v.X);
                    ymin = Math.Min(ymin, v.Z);
                    ymax = Math.Max(ymax, v.Z);
                    nPolyVerts++;
                }
                xmin = Math.Max(0, xmin - 1);
                xmax = Math.Min(chf.Width, xmax + 1);
                ymin = Math.Max(0, ymin - 1);
                ymax = Math.Min(chf.Height, ymax + 1);
                bounds[i] = new Int4(xmin, xmax, ymin, ymax);
                if (xmin >= xmax || ymin >= ymax) continue;
                maxhw = Math.Max(maxhw, xmax - xmin);
                maxhh = Math.Max(maxhh, ymax - ymin);
            }

            hp.Data = new int[maxhw * maxhh];

            dmesh = new PolyMeshDetail();

            List<Vector3> poly = new List<Vector3>(nvp);

            for (int i = 0; i < mesh.NPolys; ++i)
            {
                var p = mesh.Polys[i];

                // Store polygon vertices for processing.
                poly.Clear();
                for (int j = 0; j < nvp; ++j)
                {
                    if (p[j] == IndexedPolygon.RC_MESH_NULL_IDX) break;
                    var v = mesh.Verts[p[j]];
                    var pv = new Vector3(v.X * cs, v.Y * ch, v.Z * cs);
                    poly.Add(pv);
                }

                // Get the height data from the area of the polygon.
                hp.Bounds = new Rectangle(bounds[i].X, bounds[i].Z, bounds[i].Y - bounds[i].X, bounds[i].W - bounds[i].Z);

                chf.GetHeightData(p, mesh.Verts, borderSize, hp, mesh.Regs[i]);

                // Build detail mesh.
                BuildPolyDetailParams param = new BuildPolyDetailParams
                {
                    SampleDist = sampleDist,
                    SampleMaxError = sampleMaxError,
                    HeightSearchRadius = heightSearchRadius,
                };
                chf.BuildPolyDetail(poly.ToArray(), param, hp, out var verts, out var tris);

                // Move detail verts to world space.
                for (int j = 0; j < verts.Length; ++j)
                {
                    verts[j].X += orig.X;
                    verts[j].Y += orig.Y + chf.CellHeight; // Is this offset necessary?
                    verts[j].Z += orig.Z;
                }
                // Offset poly too, will be used to flag checking.
                for (int j = 0; j < poly.Count; ++j)
                {
                    poly[j] += orig;
                }

                // Store detail submesh.
                PolyMeshDetailIndices tmp = new PolyMeshDetailIndices
                {
                    VertBase = dmesh.Vertices.Count,
                    VertCount = verts.Length,
                    TriBase = dmesh.Triangles.Count,
                    TriCount = tris.Length,
                };
                dmesh.Meshes.Add(tmp);

                dmesh.Vertices.AddRange(verts);

                // Store triangles
                foreach (var t in tris)
                {
                    dmesh.Triangles.Add(new PolyMeshTriangleIndices
                    {
                        Point1 = t.X,
                        Point2 = t.Y,
                        Point3 = t.Z,
                        Flags = GetTriFlags(verts[t.X], verts[t.Y], verts[t.Z], poly),
                    });
                }
            }

            return dmesh;
        }
        /// <summary>
        /// Merges a list of polygon mesh details
        /// </summary>
        /// <param name="meshes">Mesh list</param>
        /// <returns>Returns the merged polygon mesh detail</returns>
        public static PolyMeshDetail Merge(IEnumerable<PolyMeshDetail> meshes)
        {
            PolyMeshDetail res = new PolyMeshDetail();

            int maxVerts = 0;
            int maxTris = 0;
            int maxMeshes = 0;

            foreach (var mesh in meshes)
            {
                if (mesh == null)
                {
                    continue;
                }
                maxVerts += mesh.Vertices.Count;
                maxTris += mesh.Triangles.Count;
                maxMeshes += mesh.Meshes.Count;
            }

            // Merge datas.
            foreach (var dm in meshes)
            {
                if (dm == null)
                {
                    continue;
                }

                foreach (var src in dm.Meshes)
                {
                    var dst = new PolyMeshDetailIndices
                    {
                        VertBase = res.Vertices.Count + src.VertBase,
                        VertCount = src.VertCount,
                        TriBase = res.Triangles.Count + src.TriBase,
                        TriCount = src.TriCount,
                    };

                    res.Meshes.Add(dst);
                }

                res.Vertices.AddRange(dm.Vertices);

                res.Triangles.AddRange(dm.Triangles);
            }

            return res;
        }
        private static int GetEdgeFlags(Vector3 va, Vector3 vb, IEnumerable<Vector3> vpoly)
        {
            int npoly = vpoly.Count();

            // Return true if edge (va,vb) is part of the polygon.
            float thrSqr = 0.001f * 0.001f;
            for (int i = 0, j = npoly - 1; i < npoly; j = i++)
            {
                var vi = vpoly.ElementAt(i);
                var vj = vpoly.ElementAt(j);
                if (RecastUtils.DistancePtSeg2D(va, vj, vi) < thrSqr &&
                    RecastUtils.DistancePtSeg2D(vb, vj, vi) < thrSqr)
                {
                    return 1;
                }
            }
            return 0;
        }
        private static int GetTriFlags(Vector3 va, Vector3 vb, Vector3 vc, IEnumerable<Vector3> vpoly)
        {
            int flags = 0;
            flags |= GetEdgeFlags(va, vb, vpoly) << 0;
            flags |= GetEdgeFlags(vb, vc, vpoly) << 2;
            flags |= GetEdgeFlags(vc, va, vpoly) << 4;
            return flags;
        }

        /// <summary>
        /// The sub-mesh data.
        /// </summary>
        public List<PolyMeshDetailIndices> Meshes { get; set; } = new List<PolyMeshDetailIndices>();
        /// <summary>
        /// The mesh vertices.
        /// </summary>
        public List<Vector3> Vertices { get; set; } = new List<Vector3>();
        /// <summary>
        /// The mesh triangles.
        /// </summary>
        public List<PolyMeshTriangleIndices> Triangles { get; set; } = new List<PolyMeshTriangleIndices>();
    }
}
