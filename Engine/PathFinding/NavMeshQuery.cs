﻿using Engine.Collections;
using Engine.Common;
using Engine.Geometry;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.PathFinding
{
    /// <summary>
    /// Do pathfinding calculations on the TiledNavMesh
    /// </summary>
    public class NavMeshQuery
    {
        private const float H_SCALE = 0.999f;

        private TiledNavMesh nav;
        private float[] areaCost;
        private NodePool tinyNodePool;
        private NodePool nodePool;
        private PriorityQueue<Node> openList;
        private QueryData query;
        private Random rand;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavMeshQuery"/> class.
        /// </summary>
        /// <param name="nav">The navigation mesh to query.</param>
        /// <param name="maxNodes">The maximum number of nodes that can be queued in a query.</param>
        public NavMeshQuery(TiledNavMesh nav, int maxNodes)
            : this(nav, maxNodes, new Random())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NavMeshQuery"/> class.
        /// </summary>
        /// <param name="nav">The navigation mesh to query.</param>
        /// <param name="maxNodes">The maximum number of nodes that can be queued in a query.</param>
        /// <param name="rand">A random number generator for use in methods like <see cref="NavMeshQuery.FindRandomPoint()"/></param>
        public NavMeshQuery(TiledNavMesh nav, int maxNodes, Random rand)
        {
            this.nav = nav;

            areaCost = new float[byte.MaxValue + 1];
            for (int i = 0; i < areaCost.Length; i++)
                areaCost[i] = 1.0f;

            nodePool = new NodePool(maxNodes/*, MathHelper.NextPowerOfTwo(maxNodes / 4)*/);
            tinyNodePool = new NodePool(64/*, 32*/);
            openList = new PriorityQueue<Node>(maxNodes);

            this.rand = rand;
        }

        /// <summary>
        /// The cost between two points may vary depending on the type of polygon.
        /// </summary>
        /// <param name="pa">Point A</param>
        /// <param name="pb">Point B</param>
        /// <param name="curPoly">Current polygon</param>
        /// <returns>Cost</returns>
        public float GetCost(Vector3 pa, Vector3 pb, Poly curPoly)
        {
            return (pa - pb).Length() * areaCost[(int)curPoly.Area.Id];
        }

        public TiledNavMesh NavMesh
        {
            get
            {
                return nav;
            }
        }

        /// <summary>
        /// Finds a random point on a polygon.
        /// </summary>
        /// <param name="tile">The current mesh tile</param>
        /// <param name="poly">The current polygon</param>
        /// <param name="polyRef">Polygon reference</param>
        /// <returns>Resulting random point</returns>
        public Vector3 FindRandomPointOnPoly(MeshTile tile, Poly poly, PolyId polyRef)
        {
            Vector3 result;
            this.FindRandomPointOnPoly(tile, poly, polyRef, out result);
            return result;
        }

        /// <summary>
        /// Finds a random point on a polygon.
        /// </summary>
        /// <param name="tile">The current mesh tile</param>
        /// <param name="poly">The current polygon</param>
        /// <param name="polyRef">Polygon reference</param>
        /// <param name="randomPt">Resulting random point</param>
        public void FindRandomPointOnPoly(MeshTile tile, Poly poly, PolyId polyRef, out Vector3 randomPt)
        {
            Vector3[] verts = new Vector3[PathfindingCommon.VERTS_PER_POLYGON];
            float[] areas = new float[PathfindingCommon.VERTS_PER_POLYGON];
            for (int j = 0; j < poly.VertCount; j++)
                verts[j] = tile.Verts[poly.Verts[j]];

            float s = (float)rand.NextDouble();
            float t = (float)rand.NextDouble();

            PathfindingCommon.RandomPointInConvexPoly(verts, poly.VertCount, areas, s, t, out randomPt);

            //TODO bad state again.
            float h = 0.0f;
            if (!GetPolyHeight(polyRef, randomPt, ref h))
                throw new InvalidOperationException("Outside bounds?");

            randomPt.Y = h;
        }

        /// <summary>
        /// Finds a random point somewhere in the navigation mesh.
        /// </summary>
        /// <returns>Resulting random point.</returns>
        public NavPoint FindRandomPoint()
        {
            NavPoint result;
            this.FindRandomPoint(out result);
            return result;
        }

        /// <summary>
        /// Finds a random point somewhere in the navigation mesh.
        /// </summary>
        /// <param name="randomPoint">Resulting random point.</param>
        public void FindRandomPoint(out NavPoint randomPoint)
        {
            //TODO we're object-oriented, can prevent this state from ever happening.
            if (nav == null)
                throw new InvalidOperationException("TODO prevent this state from ever occuring");

            //randomly pick one tile
            //assume all tiles cover roughly the same area
            MeshTile tile = null;
            float tsum = 0.0f;

            for (int i = 0; i < nav.TileCount; i++)
            {
                MeshTile t = nav[i];

                if (t == null)
                    continue;

                //choose random tile using reservoir sampling
                float area = 1.0f;
                tsum += area;
                float u = (float)rand.NextDouble();
                if (u * tsum <= area)
                    tile = t;
            }

            //TODO why?
            if (tile == null)
                throw new InvalidOperationException("No tiles?");

            //randomly pick one polygon weighted by polygon area
            Poly poly = null;
            PolyId polyRef = PolyId.Null;
            PolyId polyBase = nav.GetTileRef(tile);

            float areaSum = 0.0f;
            for (int i = 0; i < tile.PolyCount; i++)
            {
                Poly p = tile.Polys[i];

                //don't return off-mesh connection polygons
                if (p.PolyType != PolygonType.Ground)
                    continue;

                PolyId reference;
                nav.IdManager.SetPolyIndex(ref polyBase, i, out reference);

                //calculate area of polygon
                float polyArea = 0.0f;
                float area;
                for (int j = 2; j < p.VertCount; j++)
                {
                    Triangle3.Area2D(ref tile.Verts[p.Verts[0]], ref tile.Verts[p.Verts[j - 1]], ref tile.Verts[p.Verts[j]], out area);
                    polyArea += area;
                }

                //choose random polygon weighted by area, usig resevoir sampling
                areaSum += polyArea;
                float u = (float)rand.NextDouble();
                if (u * areaSum <= polyArea)
                {
                    poly = p;
                    polyRef = reference;
                }
            }

            //TODO why?
            if (poly == null)
                throw new InvalidOperationException("No polys?");

            //randomRef = polyRef;
            Vector3 randomPt;
            FindRandomPointOnPoly(tile, poly, polyRef, out randomPt);

            randomPoint = new NavPoint(polyRef, randomPt);
        }

        /// <summary>
        /// Finds a random point in a NavMesh connected to a specified point on the same mesh.
        /// </summary>
        /// <param name="connectedTo">The point that the random point will be connected to.</param>
        /// <param name="randomPoint">A random point connected to <c>connectedTo</c>.</param>
        public void FindRandomConnectedPoint(NavPoint connectedTo, out NavPoint randomPoint)
        {
            FindRandomPointAroundCircle(connectedTo, 0, out randomPoint);
        }

        /// <summary>
        /// Finds a random point in a NavMesh connected to a specified point on the same mesh.
        /// </summary>
        /// <param name="connectedTo">The point that the random point will be connected to.</param>
        /// <returns>A random point connected to <c>connectedTo</c>.</returns>
        public NavPoint FindRandomConnectedPoint(NavPoint connectedTo)
        {
            NavPoint result;
            FindRandomConnectedPoint(connectedTo, out result);
            return result;
        }

        /// <summary>
        /// Finds a random point in a NavMesh within a specified circle.
        /// </summary>
        /// <param name="center">The center point.</param>
        /// <param name="radius">The maximum distance away from the center that the random point can be. If 0, any point on the mesh can be returned.</param>
        /// <returns>A random point within the specified circle.</returns>
        public NavPoint FindRandomPointAroundCircle(NavPoint center, float radius)
        {
            NavPoint result;
            this.FindRandomPointAroundCircle(center, radius, out result);
            return result;
        }

        /// <summary>
        /// Finds a random point in a NavMesh within a specified circle.
        /// </summary>
        /// <param name="center">The center point.</param>
        /// <param name="radius">The maximum distance away from the center that the random point can be. If 0, any point on the mesh can be returned.</param>
        /// <param name="randomPoint">A random point within the specified circle.</param>
        public void FindRandomPointAroundCircle(NavPoint center, float radius, out NavPoint randomPoint)
        {
            //TODO fix state
            if (nav == null || nodePool == null || openList == null)
                throw new InvalidOperationException("Something null");

            //validate input
            if (center.Polygon == PolyId.Null)
                throw new ArgumentOutOfRangeException("startRef", "Null poly reference");

            if (!nav.IsValidPolyRef(center.Polygon))
                throw new ArgumentException("startRef", "Poly reference is not valid for this navmesh");

            MeshTile startTile;
            Poly startPoly;
            nav.TryGetTileAndPolyByRefUnsafe(center.Polygon, out startTile, out startPoly);

            nodePool.Clear();
            openList.Clear();

            Node startNode = nodePool.GetNode(center.Polygon);
            startNode.Pos = center.Position;
            startNode.ParentIdx = 0;
            startNode.cost = 0;
            startNode.total = 0;
            startNode.Id = center.Polygon;
            startNode.Flags = NodeFlags.Open;
            openList.Push(startNode);

            bool doRadiusCheck = radius != 0;
            float radiusSqr = radius * radius;
            float areaSum = 0.0f;

            MeshTile randomTile = null;
            Poly randomPoly = null;
            PolyId randomPolyRef = PolyId.Null;

            while (openList.Count > 0)
            {
                Node bestNode = openList.Pop();
                SetNodeFlagClosed(ref bestNode);

                //get poly and tile
                PolyId bestRef = bestNode.Id;
                MeshTile bestTile;
                Poly bestPoly;
                nav.TryGetTileAndPolyByRefUnsafe(bestRef, out bestTile, out bestPoly);

                //place random locations on ground
                if (bestPoly.PolyType == PolygonType.Ground)
                {
                    //calculate area of polygon
                    float polyArea = 0.0f;
                    float area;
                    for (int j = 2; j < bestPoly.VertCount; j++)
                    {
                        Triangle3.Area2D(ref bestTile.Verts[bestPoly.Verts[0]], ref bestTile.Verts[bestPoly.Verts[j - 1]], ref bestTile.Verts[bestPoly.Verts[j]], out area);
                        polyArea += area;
                    }

                    //choose random polygon weighted by area using resevoir sampling
                    areaSum += polyArea;
                    float u = (float)rand.NextDouble();
                    if (u * areaSum <= polyArea)
                    {
                        randomTile = bestTile;
                        randomPoly = bestPoly;
                        randomPolyRef = bestRef;
                    }
                }

                //get parent poly and tile
                PolyId parentRef = PolyId.Null;
                MeshTile parentTile;
                Poly parentPoly;
                if (bestNode.ParentIdx != 0)
                    parentRef = nodePool.GetNodeAtIdx(bestNode.ParentIdx).Id;
                if (parentRef != PolyId.Null)
                    nav.TryGetTileAndPolyByRefUnsafe(parentRef, out parentTile, out parentPoly);

                foreach (Link link in bestPoly.Links)
                {
                    PolyId neighborRef = link.Reference;

                    //skip invalid neighbors and do not follow back to parent
                    if (neighborRef == PolyId.Null || neighborRef == parentRef)
                        continue;

                    //expand to neighbor
                    MeshTile neighborTile;
                    Poly neighborPoly;
                    nav.TryGetTileAndPolyByRefUnsafe(neighborRef, out neighborTile, out neighborPoly);

                    //find edge and calculate distance to edge
                    Vector3 va = new Vector3();
                    Vector3 vb = new Vector3();
                    if (!GetPortalPoints(bestRef, bestPoly, bestTile, neighborRef, neighborPoly, neighborTile, ref va, ref vb))
                        continue;

                    //if circle isn't touching next polygon, skip it
                    if (doRadiusCheck)
                    {
                        float tseg;
                        float distSqr = Distance.PointToSegment2DSquared(ref center.Position, ref va, ref vb, out tseg);
                        if (distSqr > radiusSqr)
                            continue;
                    }

                    Node neighborNode = nodePool.GetNode(neighborRef);
                    if (neighborNode == null)
                        continue;

                    if (IsInClosedList(neighborNode))
                        continue;

                    //cost
                    if (neighborNode.Flags == 0)
                        neighborNode.Pos = Vector3.Lerp(va, vb, 0.5f);

                    float total = bestNode.total + (bestNode.Pos - neighborNode.Pos).Length();

                    //node is already in open list and new result is worse, so skip
                    if (IsInOpenList(neighborNode) && total >= neighborNode.total)
                        continue;

                    neighborNode.Id = neighborRef;
                    neighborNode.Flags = RemoveNodeFlagClosed(neighborNode);
                    neighborNode.ParentIdx = nodePool.GetNodeIdx(bestNode);
                    neighborNode.total = total;

                    if (IsInOpenList(neighborNode))
                    {
                        openList.Modify(neighborNode);
                    }
                    else
                    {
                        neighborNode.Flags = NodeFlags.Open;
                        openList.Push(neighborNode);
                    }
                }
            }

            //TODO invalid state.
            if (randomPoly == null)
                throw new InvalidOperationException("Poly null?");

            Vector3 randomPt;
            FindRandomPointOnPoly(randomTile, randomPoly, randomPolyRef, out randomPt);

            randomPoint = new NavPoint(randomPolyRef, randomPt);
        }

        /// <summary>
        /// Find a path from the start polygon to the end polygon.
        /// -If the end polygon can't be reached, the last polygon will be nearest the end polygon
        /// -If the path array is too small, it will be filled as far as possible 
        /// -start and end positions are used to calculate traversal costs
        /// </summary>
        /// <param name="startPt">The start point.</param>
        /// <param name="endPt">The end point.</param>
        /// <param name="path">The path of polygon references</param>
        /// <returns>True, if path found. False, if otherwise.</returns>
        public bool FindPath(ref NavPoint startPt, ref NavPoint endPt, List<PolyId> path)
        {
            //reset path of polygons
            path.Clear();

            PolyId startRef = startPt.Polygon;
            Vector3 startPos = startPt.Position;
            PolyId endRef = endPt.Polygon;
            Vector3 endPos = endPt.Position;

            if (startRef == PolyId.Null || endRef == PolyId.Null)
                return false;

            //path can't store any elements
            if (path.Capacity == 0)
                return false;

            //validate input
            if (!nav.IsValidPolyRef(startRef) || !nav.IsValidPolyRef(endRef))
                return false;

            //special case: both start and end are in the same polygon
            if (startRef == endRef)
            {
                path.Add(startRef);
                return true;
            }

            nodePool.Clear();
            openList.Clear();

            //initial node is located at the starting position
            Node startNode = nodePool.GetNode(startRef);
            startNode.Pos = startPos;
            startNode.ParentIdx = 0;
            startNode.cost = 0;
            startNode.total = (startPos - endPos).Length() * H_SCALE;
            startNode.Id = startRef;
            startNode.Flags = NodeFlags.Open;
            openList.Push(startNode);

            Node lastBestNode = startNode;
            float lastBestTotalCost = startNode.total;

            while (openList.Count > 0)
            {
                //remove node from open list and put it in closed list
                Node bestNode = openList.Pop();
                SetNodeFlagClosed(ref bestNode);

                //reached the goal. stop searching
                if (bestNode.Id == endRef)
                {
                    lastBestNode = bestNode;
                    break;
                }

                //get current poly and tile
                PolyId bestRef = bestNode.Id;
                MeshTile bestTile;
                Poly bestPoly;
                nav.TryGetTileAndPolyByRefUnsafe(bestRef, out bestTile, out bestPoly);

                //get parent poly and tile
                PolyId parentRef = PolyId.Null;
                MeshTile parentTile;
                Poly parentPoly;
                if (bestNode.ParentIdx != 0)
                    parentRef = nodePool.GetNodeAtIdx(bestNode.ParentIdx).Id;
                if (parentRef != PolyId.Null)
                    nav.TryGetTileAndPolyByRefUnsafe(parentRef, out parentTile, out parentPoly);

                //examine neighbors
                foreach (Link link in bestPoly.Links)
                {
                    PolyId neighborRef = link.Reference;

                    //skip invalid ids and do not expand back to where we came from
                    if (neighborRef == PolyId.Null || neighborRef == parentRef)
                        continue;

                    //get neighbor poly and tile
                    MeshTile neighborTile;
                    Poly neighborPoly;
                    nav.TryGetTileAndPolyByRefUnsafe(neighborRef, out neighborTile, out neighborPoly);

                    Node neighborNode = nodePool.GetNode(neighborRef);
                    if (neighborNode == null)
                        continue;

                    //if node is visited the first time, calculate node position
                    if (neighborNode.Flags == 0)
                    {
                        GetEdgeMidPoint(bestRef, bestPoly, bestTile, neighborRef, neighborPoly, neighborTile, ref neighborNode.Pos);
                    }

                    //calculate cost and heuristic
                    float cost = 0;
                    float heuristic = 0;

                    //special case for last node
                    if (neighborRef == endRef)
                    {
                        //cost
                        float curCost = GetCost(bestNode.Pos, neighborNode.Pos, bestPoly);
                        float endCost = GetCost(neighborNode.Pos, endPos, neighborPoly);

                        cost = bestNode.cost + curCost + endCost;
                        heuristic = 0;
                    }
                    else
                    {
                        //cost
                        float curCost = GetCost(bestNode.Pos, neighborNode.Pos, bestPoly);

                        cost = bestNode.cost + curCost;
                        heuristic = (neighborNode.Pos - endPos).Length() * H_SCALE;
                    }

                    float total = cost + heuristic;

                    //the node is already in open list and new result is worse, skip
                    if (IsInOpenList(neighborNode) && total >= neighborNode.total)
                        continue;

                    //the node is already visited and processesd, and the new result is worse, skip
                    if (IsInClosedList(neighborNode) && total >= neighborNode.total)
                        continue;

                    //add or update the node
                    neighborNode.ParentIdx = nodePool.GetNodeIdx(bestNode);
                    neighborNode.Id = neighborRef;
                    neighborNode.Flags = RemoveNodeFlagClosed(neighborNode);
                    neighborNode.cost = cost;
                    neighborNode.total = total;

                    if (IsInOpenList(neighborNode))
                    {
                        //already in open, update node location
                        openList.Modify(neighborNode);
                    }
                    else
                    {
                        //put the node in the open list
                        SetNodeFlagOpen(ref neighborNode);
                        openList.Push(neighborNode);
                    }

                    //update nearest node to target so far
                    if (heuristic < lastBestTotalCost)
                    {
                        lastBestTotalCost = heuristic;
                        lastBestNode = neighborNode;
                    }
                }
            }

            //save path
            Node node = lastBestNode;
            do
            {
                path.Add(node.Id);
                if (path.Count >= path.Capacity)
                    break;

                node = nodePool.GetNodeAtIdx(node.ParentIdx);
            }
            while (node != null);

            //reverse the path since it's backwards
            path.Reverse();

            return true;
        }

        /// <summary>
        /// Add vertices and portals to a regular path computed from the method FindPath().
        /// </summary>
        /// <param name="startPos">Starting position</param>
        /// <param name="endPos">Ending position</param>
        /// <param name="path">Path of polygon references</param>
        /// <param name="pathSize">Length of path</param>
        /// <param name="straightPath">An array of points on the straight path</param>
        /// <param name="straightPathFlags">An array of flags</param>
        /// <param name="straightPathRefs">An array of polygon references</param>
        /// <param name="straightPathCount">The number of points on the path</param>
        /// <param name="maxStraightPath">The maximum length allowed for the straight path</param>
        /// <param name="options">Options flag</param>
        /// <returns>True, if path found. False, if otherwise.</returns>
        public bool FindStraightPath(Vector3 startPos, Vector3 endPos, PolyId[] path, int pathSize, Vector3[] straightPath, int[] straightPathFlags, PolyId[] straightPathRefs, ref int straightPathCount, int maxStraightPath, PathBuildFlags options)
        {
            straightPathCount = 0;

            if (path.Length == 0)
                return false;

            bool stat = false;

            Vector3 closestStartPos = new Vector3();
            ClosestPointOnPolyBoundary(path[0], startPos, ref closestStartPos);

            Vector3 closestEndPos = new Vector3();
            ClosestPointOnPolyBoundary(path[pathSize - 1], endPos, ref closestEndPos);

            stat = AppendVertex(closestStartPos, PathfindingCommon.STRAIGHTPATH_START, path[0], straightPath, straightPathFlags, straightPathRefs, ref straightPathCount, maxStraightPath);

            if (!stat)
                return true;

            if (pathSize > 1)
            {
                Vector3 portalApex = closestStartPos;
                Vector3 portalLeft = portalApex;
                Vector3 portalRight = portalApex;
                int apexIndex = 0;
                int leftIndex = 0;
                int rightIndex = 0;

                PolygonType leftPolyType = 0;
                PolygonType rightPolyType = 0;

                PolyId leftPolyRef = path[0];
                PolyId rightPolyRef = path[0];

                for (int i = 0; i < pathSize; i++)
                {
                    Vector3 left = new Vector3();
                    Vector3 right = new Vector3();
                    PolygonType fromType = 0, toType = 0;

                    if (i + 1 < pathSize)
                    {
                        //next portal
                        if (GetPortalPoints(path[i], path[i + 1], ref left, ref right, ref fromType, ref toType) == false)
                        {
                            //failed to get portal points means path[i + 1] is an invalid polygon
                            //clamp end point to path[i] and return path so far
                            if (ClosestPointOnPolyBoundary(path[i], endPos, ref closestEndPos) == false)
                            {
                                //first polygon is invalid
                                return false;
                            }

                            if ((options & (PathBuildFlags.AreaCrossingVertices | PathBuildFlags.AllCrossingVertices)) != 0)
                            {
                                //append portals
                                stat = AppendPortals(apexIndex, i, closestEndPos, path, straightPath, straightPathFlags, straightPathRefs, ref straightPathCount, maxStraightPath, options);
                            }

                            stat = AppendVertex(closestEndPos, 0, path[i], straightPath, straightPathFlags, straightPathRefs, ref straightPathCount, maxStraightPath);

                            return true;
                        }

                        //if starting really close to the portal, advance
                        if (i == 0)
                        {
                            float t;
                            if (Distance.PointToSegment2DSquared(ref portalApex, ref left, ref right, out t) < 0.001 * 0.001)
                                continue;
                        }
                    }
                    else
                    {
                        //end of the path
                        left = closestEndPos;
                        right = closestEndPos;

                        fromType = toType = PolygonType.Ground;
                    }

                    //right vertex
                    float triArea2D;
                    Triangle3.Area2D(ref portalApex, ref portalRight, ref right, out triArea2D);
                    if (triArea2D <= 0.0)
                    {
                        Triangle3.Area2D(ref portalApex, ref portalLeft, ref right, out triArea2D);
                        if (portalApex == portalRight || triArea2D > 0.0)
                        {
                            portalRight = right;
                            rightPolyRef = (i + 1 < pathSize) ? path[i + 1] : PolyId.Null;
                            rightPolyType = toType;
                            rightIndex = i;
                        }
                        else
                        {
                            //append portals along current straight path segment
                            if ((options & (PathBuildFlags.AreaCrossingVertices | PathBuildFlags.AllCrossingVertices)) != 0)
                            {
                                stat = AppendPortals(apexIndex, leftIndex, portalLeft, path, straightPath, straightPathFlags, straightPathRefs, ref straightPathCount, maxStraightPath, options);

                                if (stat != true)
                                    return true;
                            }

                            portalApex = portalLeft;
                            apexIndex = leftIndex;

                            int flags = 0;
                            if (leftPolyRef == PolyId.Null)
                                flags = PathfindingCommon.STRAIGHTPATH_END;
                            else if (leftPolyType == PolygonType.OffMeshConnection)
                                flags = PathfindingCommon.STRAIGHTPATH_OFFMESH_CONNECTION;

                            PolyId reference = leftPolyRef;

                            //append or update vertex
                            stat = AppendVertex(portalApex, flags, reference, straightPath, straightPathFlags, straightPathRefs, ref straightPathCount, maxStraightPath);

                            if (stat != true)
                                return true;

                            portalLeft = portalApex;
                            portalRight = portalApex;
                            leftIndex = apexIndex;
                            rightIndex = apexIndex;

                            //restart
                            i = apexIndex;

                            continue;
                        }
                    }

                    //left vertex
                    Triangle3.Area2D(ref portalApex, ref portalLeft, ref left, out triArea2D);
                    if (triArea2D >= 0.0)
                    {
                        Triangle3.Area2D(ref portalApex, ref portalRight, ref left, out triArea2D);
                        if (portalApex == portalLeft || triArea2D < 0.0f)
                        {
                            portalLeft = left;
                            leftPolyRef = (i + 1 < pathSize) ? path[i + 1] : PolyId.Null;
                            leftPolyType = toType;
                            leftIndex = i;
                        }
                        else
                        {
                            if ((options & (PathBuildFlags.AreaCrossingVertices | PathBuildFlags.AllCrossingVertices)) != 0)
                            {
                                stat = AppendPortals(apexIndex, rightIndex, portalRight, path, straightPath, straightPathFlags, straightPathRefs, ref straightPathCount, maxStraightPath, options);

                                if (stat != true)
                                    return true;
                            }

                            portalApex = portalRight;
                            apexIndex = rightIndex;

                            int flags = 0;
                            if (rightPolyRef == PolyId.Null)
                                flags = PathfindingCommon.STRAIGHTPATH_END;
                            else if (rightPolyType == PolygonType.OffMeshConnection)
                                flags = PathfindingCommon.STRAIGHTPATH_OFFMESH_CONNECTION;

                            PolyId reference = rightPolyRef;

                            //append or update vertex
                            stat = AppendVertex(portalApex, flags, reference, straightPath, straightPathFlags, straightPathRefs, ref straightPathCount, maxStraightPath);

                            if (stat != true)
                                return true;

                            portalLeft = portalApex;
                            portalRight = portalApex;
                            leftIndex = apexIndex;
                            rightIndex = apexIndex;

                            //restart 
                            i = apexIndex;

                            continue;
                        }
                    }
                }

                //append portals along the current straight line segment
                if ((options & (PathBuildFlags.AreaCrossingVertices | PathBuildFlags.AllCrossingVertices)) != 0)
                {
                    stat = AppendPortals(apexIndex, pathSize - 1, closestEndPos, path, straightPath, straightPathFlags, straightPathRefs, ref straightPathCount, maxStraightPath, options);

                    if (stat != true)
                        return true;
                }
            }

            stat = AppendVertex(closestEndPos, PathfindingCommon.STRAIGHTPATH_END, PolyId.Null, straightPath, straightPathFlags, straightPathRefs, ref straightPathCount, maxStraightPath);

            return true;
        }

        /// <summary>
        /// This method is optimized for small delta movement and a small number of polygons.
        /// If movement distance is too large, the result will form an incomplete path.
        /// </summary>
        /// <param name="startPoint">The start point.</param>
        /// <param name="endPos">End position</param>
        /// <param name="resultPos">Intermediate point</param>
        /// <param name="visited">Visited polygon references</param>
        /// <returns>True, if point found. False, if otherwise.</returns>
        public bool MoveAlongSurface(NavPoint startPoint, Vector3 endPos, ref Vector3 resultPos, List<PolyId> visited)
        {
            if (nav == null)
                return false;
            if (tinyNodePool == null)
                return false;

            visited.Clear();

            //validate input
            if (startPoint.Polygon == PolyId.Null)
                return false;
            if (!nav.IsValidPolyRef(startPoint.Polygon))
                return false;

            int MAX_STACK = 48;
            Queue<Node> nodeQueue = new Queue<Node>(MAX_STACK);

            tinyNodePool.Clear();

            Node startNode = tinyNodePool.GetNode(startPoint.Polygon);
            startNode.ParentIdx = 0;
            startNode.cost = 0;
            startNode.total = 0;
            startNode.Id = startPoint.Polygon;
            startNode.Flags = NodeFlags.Closed;
            nodeQueue.Enqueue(startNode);

            Vector3 bestPos = startPoint.Position;
            float bestDist = float.MaxValue;
            Node bestNode = null;

            //search constraints
            Vector3 searchPos = Vector3.Lerp(startPoint.Position, endPos, 0.5f);
            float searchRad = (startPoint.Position - endPos).Length() / 2.0f + 0.001f;
            float searchRadSqr = searchRad * searchRad;

            Vector3[] verts = new Vector3[PathfindingCommon.VERTS_PER_POLYGON];

            while (nodeQueue.Count > 0)
            {
                //pop front
                Node curNode = nodeQueue.Dequeue();

                //get poly and tile
                PolyId curRef = curNode.Id;
                MeshTile curTile;
                Poly curPoly;
                nav.TryGetTileAndPolyByRefUnsafe(curRef, out curTile, out curPoly);

                //collect vertices
                int nverts = curPoly.VertCount;
                for (int i = 0; i < nverts; i++)
                    verts[i] = curTile.Verts[curPoly.Verts[i]];

                //if target is inside poly, stop search
                if (Containment.PointInPoly(endPos, verts, nverts))
                {
                    bestNode = curNode;
                    bestPos = endPos;
                    break;
                }

                //find wall edges and find nearest point inside walls
                for (int i = 0, j = curPoly.VertCount - 1; i < curPoly.VertCount; j = i++)
                {
                    //find links to neighbors
                    List<PolyId> neis = new List<PolyId>(8);

                    if ((curPoly.Neis[j] & Link.External) != 0)
                    {
                        //tile border
                        foreach (Link link in curPoly.Links)
                        {
                            if (link.Edge == j)
                            {
                                if (link.Reference != PolyId.Null)
                                {
                                    MeshTile neiTile;
                                    Poly neiPoly;
                                    nav.TryGetTileAndPolyByRefUnsafe(link.Reference, out neiTile, out neiPoly);

                                    if (neis.Count < neis.Capacity)
                                        neis.Add(link.Reference);
                                }
                            }
                        }
                    }
                    else if (curPoly.Neis[j] != 0)
                    {
                        int idx = curPoly.Neis[j] - 1;
                        PolyId reference = nav.GetTileRef(curTile);
                        nav.IdManager.SetPolyIndex(ref reference, idx, out reference);
                        neis.Add(reference); //internal edge, encode id
                    }

                    if (neis.Count == 0)
                    {
                        //wall edge, calculate distance
                        float tseg = 0;
                        float distSqr = Distance.PointToSegment2DSquared(ref endPos, ref verts[j], ref verts[i], out tseg);
                        if (distSqr < bestDist)
                        {
                            //update nearest distance
                            bestPos = Vector3.Lerp(verts[j], verts[i], tseg);
                            bestDist = distSqr;
                            bestNode = curNode;
                        }
                    }
                    else
                    {
                        for (int k = 0; k < neis.Count; k++)
                        {
                            //skip if no node can be allocated
                            Node neighborNode = tinyNodePool.GetNode(neis[k]);
                            if (neighborNode == null)
                                continue;

                            //skip if already visited
                            if ((neighborNode.Flags & NodeFlags.Closed) != 0)
                                continue;

                            //skip the link if too far from search constraint
                            float distSqr = Distance.PointToSegment2DSquared(ref searchPos, ref verts[j], ref verts[i]);
                            if (distSqr > searchRadSqr)
                                continue;

                            //mark the node as visited and push to queue
                            if (nodeQueue.Count < MAX_STACK)
                            {
                                neighborNode.ParentIdx = tinyNodePool.GetNodeIdx(curNode);
                                neighborNode.Flags |= NodeFlags.Closed;
                                nodeQueue.Enqueue(neighborNode);
                            }
                        }
                    }
                }
            }

            if ((endPos - bestPos).Length() > 1f)
                return false;

            if (bestNode != null)
            {
                //save the path
                Node node = bestNode;
                do
                {
                    visited.Add(node.Id);
                    if (visited.Count >= visited.Capacity)
                        break;

                    node = tinyNodePool.GetNodeAtIdx(node.ParentIdx);
                }
                while (node != null);

                //reverse the path since it's backwards
                visited.Reverse();
            }

            resultPos = bestPos;

            return true;
        }

        /// <summary>
        /// Initialize a sliced path, which is used mostly for crowd pathfinding.
        /// </summary>
        /// <param name="startPoint">The start point.</param>
        /// <param name="endPoint">The end point.</param>
        /// <returns>True if path initialized, false otherwise</returns>
        public bool InitSlicedFindPath(NavPoint startPoint, NavPoint endPoint)
        {
            //validate input
            if (startPoint.Polygon == PolyId.Null || endPoint.Polygon == PolyId.Null)
                return false;

            if (!nav.IsValidPolyRef(startPoint.Polygon) || !nav.IsValidPolyRef(endPoint.Polygon))
                return false;

            if (startPoint.Polygon == endPoint.Polygon)
            {
                query.Status = true;
                return true;
            }

            //init path state
            query = new QueryData();
            query.Status = false;
            query.Start = startPoint;
            query.End = endPoint;

            nodePool.Clear();
            openList.Clear();

            Node startNode = nodePool.GetNode(startPoint.Polygon);
            startNode.Pos = startPoint.Position;
            startNode.ParentIdx = 0;
            startNode.cost = 0;
            startNode.total = (endPoint.Position - startPoint.Position).Length() * H_SCALE;
            startNode.Id = startPoint.Polygon;
            startNode.Flags = NodeFlags.Open;
            openList.Push(startNode);

            query.Status = true;
            query.LastBestNode = startNode;
            query.LastBestNodeCost = startNode.total;

            return query.Status;
        }

        /// <summary>
        /// Update the sliced path as agents move across the path.
        /// </summary>
        /// <param name="maxIter">Maximum iterations</param>
        /// <param name="doneIters">Number of times iterated through</param>
        /// <returns>True if updated, false if not</returns>
        public bool UpdateSlicedFindPath(int maxIter, ref int doneIters)
        {
            if (query.Status != true)
                return query.Status;

            //make sure the request is still valid
            if (!nav.IsValidPolyRef(query.Start.Polygon) || !nav.IsValidPolyRef(query.End.Polygon))
            {
                query.Status = false;
                return false;
            }

            int iter = 0;
            while (iter < maxIter && !openList.Empty())
            {
                iter++;

                //remove node from open list and put it in closed list
                Node bestNode = openList.Pop();
                SetNodeFlagClosed(ref bestNode);

                //reached the goal, stop searching
                if (bestNode.Id == query.End.Polygon)
                {
                    query.LastBestNode = bestNode;
                    query.Status = true;
                    doneIters = iter;
                    return query.Status;
                }

                //get current poly and tile
                PolyId bestRef = bestNode.Id;
                MeshTile bestTile;
                Poly bestPoly;
                if (nav.TryGetTileAndPolyByRef(bestRef, out bestTile, out bestPoly) == false)
                {
                    //the polygon has disappeared during the sliced query, fail
                    query.Status = false;
                    doneIters = iter;
                    return query.Status;
                }

                //get parent poly and tile
                PolyId parentRef = PolyId.Null;
                MeshTile parentTile;
                Poly parentPoly;
                if (bestNode.ParentIdx != 0)
                    parentRef = nodePool.GetNodeAtIdx(bestNode.ParentIdx).Id;
                if (parentRef != PolyId.Null)
                {
                    if (nav.TryGetTileAndPolyByRef(parentRef, out parentTile, out parentPoly) == false)
                    {
                        //the polygon has disappeared during the sliced query, fail
                        query.Status = false;
                        doneIters = iter;
                        return query.Status;
                    }
                }

                foreach (Link link in bestPoly.Links)
                {
                    PolyId neighborRef = link.Reference;

                    //skip invalid ids and do not expand back to where we came from
                    if (neighborRef == PolyId.Null || neighborRef == parentRef)
                        continue;

                    //get neighbor poly and tile
                    MeshTile neighborTile;
                    Poly neighborPoly;
                    nav.TryGetTileAndPolyByRefUnsafe(neighborRef, out neighborTile, out neighborPoly);

                    Node neighborNode = nodePool.GetNode(neighborRef);
                    if (neighborNode == null)
                        continue;

                    if (neighborNode.Flags == 0)
                    {
                        GetEdgeMidPoint(bestRef, bestPoly, bestTile, neighborRef, neighborPoly, neighborTile, ref neighborNode.Pos);
                    }

                    //calculate cost and heuristic
                    float cost = 0;
                    float heuristic = 0;

                    //special case for last node
                    if (neighborRef == query.End.Polygon)
                    {
                        //cost
                        float curCost = GetCost(bestNode.Pos, neighborNode.Pos, bestPoly);
                        float endCost = GetCost(neighborNode.Pos, query.End.Position, neighborPoly);

                        cost = bestNode.cost + curCost + endCost;
                        heuristic = 0;
                    }
                    else
                    {
                        //cost
                        float curCost = GetCost(bestNode.Pos, neighborNode.Pos, bestPoly);

                        cost = bestNode.cost + curCost;
                        heuristic = (neighborNode.Pos - query.End.Position).Length() * H_SCALE;
                    }

                    float total = cost + heuristic;

                    //the node is already in open list and new result is worse, skip
                    if (IsInOpenList(neighborNode) && total >= neighborNode.total)
                        continue;

                    //the node is already visited and processesd, and the new result is worse, skip
                    if (IsInClosedList(neighborNode) && total >= neighborNode.total)
                        continue;

                    //add or update the node
                    neighborNode.ParentIdx = nodePool.GetNodeIdx(bestNode);
                    neighborNode.Id = neighborRef;
                    neighborNode.Flags = RemoveNodeFlagClosed(neighborNode);
                    neighborNode.cost = cost;
                    neighborNode.total = total;

                    if (IsInOpenList(neighborNode))
                    {
                        //already in open, update node location
                        openList.Modify(neighborNode);
                    }
                    else
                    {
                        //put the node in the open list
                        SetNodeFlagOpen(ref neighborNode);
                        openList.Push(neighborNode);
                    }

                    //update nearest node to target so far
                    if (heuristic < query.LastBestNodeCost)
                    {
                        query.LastBestNodeCost = heuristic;
                        query.LastBestNode = neighborNode;
                    }
                }
            }

            //exhausted all nodes, but could not find path
            if (openList.Empty())
            {
                query.Status = true;
            }

            doneIters = iter;

            return query.Status;
        }

        /// <summary>
        /// Save the sliced path 
        /// </summary>
        /// <param name="path">The path in terms of polygon references</param>
        /// <param name="pathCount">The path length</param>
        /// <param name="maxPath">The maximum path length allowed</param>
        /// <returns>True if the path is saved, false if not</returns>
        public bool FinalizeSlicedFindPath(PolyId[] path, ref int pathCount, int maxPath)
        {
            pathCount = 0;

            if (query.Status == false)
            {
                query = new QueryData();
                return false;
            }

            int n = 0;

            if (query.Start.Polygon == query.End.Polygon)
            {
                //special case: the search starts and ends at the same poly
                path[n++] = query.Start.Polygon;
            }
            else
            {
                //reverse the path
                Node prev = null;
                Node node = query.LastBestNode;
                do
                {
                    Node next = nodePool.GetNodeAtIdx(node.ParentIdx);
                    node.ParentIdx = nodePool.GetNodeIdx(prev);
                    prev = node;
                    node = next;
                }
                while (node != null);

                //store path
                node = prev;
                do
                {
                    path[n++] = node.Id;
                    if (n >= maxPath)
                        break;

                    node = nodePool.GetNodeAtIdx(node.ParentIdx);
                }
                while (node != null);
            }

            //reset query
            query = new QueryData();

            //remember to update the path length
            pathCount = n;

            return true;
        }

        /// <summary>
        /// Save a partial path
        /// </summary>
        /// <param name="existing">Existing path</param>
        /// <param name="existingSize">Existing path's length</param>
        /// <param name="path">New path</param>
        /// <param name="pathCount">New path's length</param>
        /// <param name="maxPath">Maximum path length allowed</param>
        /// <returns>True if path saved, false if not</returns>
        public bool FinalizedSlicedPathPartial(PolyId[] existing, int existingSize, PolyId[] path, ref int pathCount, int maxPath)
        {
            pathCount = 0;

            if (existingSize == 0)
            {
                return false;
            }

            if (query.Status == false)
            {
                query = new QueryData();
                return false;
            }

            int n = 0;

            if (query.Start.Polygon == query.End.Polygon)
            {
                //special case: the search starts and ends at the same poly
                path[n++] = query.Start.Polygon;
            }
            else
            {
                //find furthest existing node that was visited
                Node prev = null;
                Node node = null;
                for (int i = existingSize - 1; i >= 0; i--)
                {
                    node = nodePool.FindNode(existing[i]);
                    if (node != null)
                        break;
                }

                if (node == null)
                {
                    node = query.LastBestNode;
                }

                //reverse the path
                do
                {
                    Node next = nodePool.GetNodeAtIdx(node.ParentIdx);
                    node.ParentIdx = nodePool.GetNodeIdx(prev);
                    prev = node;
                    node = next;
                }
                while (node != null);

                //store path
                node = prev;
                do
                {
                    path[n++] = node.Id;
                    if (n >= maxPath)
                    {
                        break;
                    }

                    node = nodePool.GetNodeAtIdx(node.ParentIdx);
                }
                while (node != null);
            }

            //reset query
            query = new QueryData();

            //remember to update the path length
            pathCount = n;

            return true;
        }

        public bool Raycast(NavPoint startPoint, Vector3 endPos, ref float t, ref Vector3 hitNormal, PolyId[] path, ref int pathCount, int maxPath)
        {
            t = 0;
            pathCount = 0;

            //validate input
            if (startPoint.Polygon == PolyId.Null || !nav.IsValidPolyRef(startPoint.Polygon))
                return false;

            PolyId curRef = startPoint.Polygon;
            Vector3[] verts = new Vector3[PathfindingCommon.VERTS_PER_POLYGON];
            int n = 0;

            hitNormal = new Vector3(0, 0, 0);

            while (curRef != PolyId.Null)
            {
                //cast ray against current polygon
                MeshTile tile;
                Poly poly;
                nav.TryGetTileAndPolyByRefUnsafe(curRef, out tile, out poly);

                //collect vertices
                int nv = 0;
                for (int i = 0; i < poly.VertCount; i++)
                {
                    verts[nv] = tile.Verts[poly.Verts[i]];
                    nv++;
                }

                float tmin, tmax;
                int segMin, segMax;
                if (!Intersection.SegmentPoly2D(startPoint.Position, endPos, verts, nv, out tmin, out tmax, out segMin, out segMax))
                {
                    //could not hit the polygon, keep the old t and report hit
                    pathCount = n;
                    return true;
                }

                //keep track of furthest t so far
                if (tmax > t)
                    t = tmax;

                //store visited polygons
                if (n < maxPath)
                    path[n++] = curRef;

                //ray end is completely inside the polygon
                if (segMax == -1)
                {
                    t = float.MaxValue;
                    pathCount = n;
                    return true;
                }

                //follow neighbors
                PolyId nextRef = PolyId.Null;

                foreach (Link link in poly.Links)
                {
                    //find link which contains the edge
                    if (link.Edge != segMax)
                        continue;

                    //get pointer to the next polygon
                    MeshTile nextTile;
                    Poly nextPoly;
                    nav.TryGetTileAndPolyByRefUnsafe(link.Reference, out nextTile, out nextPoly);

                    //skip off-mesh connection
                    if (nextPoly.PolyType == PolygonType.OffMeshConnection)
                        continue;

                    //if the link is internal, just return the ref
                    if (link.Side == BoundarySide.Internal)
                    {
                        nextRef = link.Reference;
                        break;
                    }

                    //if the link is at the tile boundary

                    //check if the link spans the whole edge and accept
                    if (link.BMin == 0 && link.BMax == 255)
                    {
                        nextRef = link.Reference;
                        break;
                    }

                    //check for partial edge links
                    int v0 = poly.Verts[link.Edge];
                    int v1 = poly.Verts[(link.Edge + 1) % poly.VertCount];
                    Vector3 left = tile.Verts[v0];
                    Vector3 right = tile.Verts[v1];

                    //check that the intersection lies inside the link portal
                    if (link.Side == BoundarySide.PlusX || link.Side == BoundarySide.MinusX)
                    {
                        //calculate link size
                        float s = 1.0f / 255.0f;
                        float lmin = left.Z + (right.Z - left.Z) * (link.BMin * s);
                        float lmax = left.Z + (right.Z - left.Z) * (link.BMax * s);
                        if (lmin > lmax)
                        {
                            //swap
                            float temp = lmin;
                            lmin = lmax;
                            lmax = temp;
                        }

                        //find z intersection
                        float z = startPoint.Position.Z + (endPos.Z - startPoint.Position.Z) * tmax;
                        if (z >= lmin && z <= lmax)
                        {
                            nextRef = link.Reference;
                            break;
                        }
                    }
                    else if (link.Side == BoundarySide.PlusZ || link.Side == BoundarySide.MinusZ)
                    {
                        //calculate link size
                        float s = 1.0f / 255.0f;
                        float lmin = left.X + (right.X - left.X) * (link.BMin * s);
                        float lmax = left.X + (right.X - left.X) * (link.BMax * s);
                        if (lmin > lmax)
                        {
                            //swap
                            float temp = lmin;
                            lmin = lmax;
                            lmax = temp;
                        }

                        //find x intersection
                        float x = startPoint.Position.X + (endPos.X - startPoint.Position.X) * tmax;
                        if (x >= lmin && x <= lmax)
                        {
                            nextRef = link.Reference;
                            break;
                        }
                    }
                }

                if (nextRef == PolyId.Null)
                {
                    //no neighbor, we hit a wall

                    //calculate hit normal
                    int a = segMax;
                    int b = (segMax + 1) < nv ? segMax + 1 : 0;
                    Vector3 va = verts[a];
                    Vector3 vb = verts[b];
                    float dx = vb.X - va.X;
                    float dz = vb.Z - va.Z;
                    hitNormal.X = dz;
                    hitNormal.Y = 0;
                    hitNormal.Z = -dx;
                    hitNormal.Normalize();

                    pathCount = n;
                    return true;
                }

                //no hit, advance to neighbor polygon
                curRef = nextRef;
            }

            pathCount = n;

            return true;
        }

        /// <summary>
        /// Store polygons that are within a certain range from the current polygon
        /// </summary>
        /// <param name="centerPoint">Starting position</param>
        /// <param name="radius">Range to search within</param>
        /// <param name="resultRef">All the polygons within range</param>
        /// <param name="resultParent">Polygon's parents</param>
        /// <param name="resultCount">Number of polygons stored</param>
        /// <param name="maxResult">Maximum number of polygons allowed</param>
        /// <returns>True, unless input is invalid</returns>
        public bool FindLocalNeighborhood(NavPoint centerPoint, float radius, PolyId[] resultRef, PolyId[] resultParent, ref int resultCount, int maxResult)
        {
            resultCount = 0;

            //validate input
            if (centerPoint.Polygon == PolyId.Null || !nav.IsValidPolyRef(centerPoint.Polygon))
                return false;

            int MAX_STACK = 48;
            Node[] stack = new Node[MAX_STACK];
            int nstack = 0;

            tinyNodePool.Clear();

            Node startNode = tinyNodePool.GetNode(centerPoint.Polygon);
            startNode.ParentIdx = 0;
            startNode.Id = centerPoint.Polygon;
            startNode.Flags = NodeFlags.Closed;
            stack[nstack++] = startNode;

            float radiusSqr = radius * radius;

            Vector3[] pa = new Vector3[PathfindingCommon.VERTS_PER_POLYGON];
            Vector3[] pb = new Vector3[PathfindingCommon.VERTS_PER_POLYGON];

            int n = 0;
            if (n < maxResult)
            {
                resultRef[n] = startNode.Id;
                resultParent[n] = PolyId.Null;
                ++n;
            }

            while (nstack > 0)
            {
                //pop front
                Node curNode = stack[0];
                for (int i = 0; i < nstack - 1; i++)
                    stack[i] = stack[i + 1];
                nstack--;

                //get poly and tile
                PolyId curRef = curNode.Id;
                MeshTile curTile;
                Poly curPoly;
                nav.TryGetTileAndPolyByRefUnsafe(curRef, out curTile, out curPoly);

                foreach (Link link in curPoly.Links)
                {
                    PolyId neighborRef = link.Reference;

                    //skip invalid neighbors
                    if (neighborRef == PolyId.Null)
                        continue;

                    //skip if cannot allocate more nodes
                    Node neighborNode = tinyNodePool.GetNode(neighborRef);
                    if (neighborNode == null)
                        continue;

                    //skip visited
                    if ((neighborNode.Flags & NodeFlags.Closed) != 0)
                        continue;

                    //expand to neighbor
                    MeshTile neighborTile;
                    Poly neighborPoly;
                    nav.TryGetTileAndPolyByRefUnsafe(neighborRef, out neighborTile, out neighborPoly);

                    //skip off-mesh connections
                    if (neighborPoly.PolyType == PolygonType.OffMeshConnection)
                        continue;

                    //find edge and calculate distance to edge
                    Vector3 va = new Vector3();
                    Vector3 vb = new Vector3();
                    if (!GetPortalPoints(curRef, curPoly, curTile, neighborRef, neighborPoly, neighborTile, ref va, ref vb))
                        continue;

                    //if the circle is not touching the next polygon, skip it
                    float tseg;
                    float distSqr = Distance.PointToSegment2DSquared(ref centerPoint.Position, ref va, ref vb, out tseg);
                    if (distSqr > radiusSqr)
                        continue;

                    //mark node visited
                    neighborNode.Flags |= NodeFlags.Closed;
                    neighborNode.ParentIdx = tinyNodePool.GetNodeIdx(curNode);

                    //check that the polygon doesn't collide with existing polygons

                    //collect vertices of the neighbor poly
                    int npa = neighborPoly.VertCount;
                    for (int k = 0; k < npa; k++)
                        pa[k] = neighborTile.Verts[neighborPoly.Verts[k]];

                    bool overlap = false;
                    for (int j = 0; j < n; j++)
                    {
                        PolyId pastRef = resultRef[j];

                        //connected polys do not overlap
                        bool connected = false;
                        foreach (Link link2 in curPoly.Links)
                        {
                            if (link2.Reference == pastRef)
                            {
                                connected = true;
                                break;
                            }
                        }

                        if (connected)
                            continue;

                        //potentially overlapping
                        MeshTile pastTile;
                        Poly pastPoly;
                        nav.TryGetTileAndPolyByRefUnsafe(pastRef, out pastTile, out pastPoly);

                        //get vertices and test overlap
                        int npb = pastPoly.VertCount;
                        for (int k = 0; k < npb; k++)
                            pb[k] = pastTile.Verts[pastPoly.Verts[k]];

                        if (Intersection.PolyPoly2D(pa, npa, pb, npb))
                        {
                            overlap = true;
                            break;
                        }
                    }

                    if (overlap)
                        continue;

                    //store poly
                    if (n < maxResult)
                    {
                        resultRef[n] = neighborRef;
                        resultParent[n] = curRef;
                        ++n;
                    }

                    if (nstack < MAX_STACK)
                    {
                        stack[nstack++] = neighborNode;
                    }
                }
            }

            resultCount = n;

            return true;
        }

        /// <summary>
        /// Insert a segment into the array
        /// </summary>
        /// <param name="ints">The array of segments</param>
        /// <param name="nints">The number of segments</param>
        /// <param name="maxInts">The maximium number of segments allowed</param>
        /// <param name="tmin">Parameter t minimum</param>
        /// <param name="tmax">Parameter t maximum</param>
        /// <param name="reference">Polygon reference</param>
        public void InsertInterval(SegInterval[] ints, ref int nints, int maxInts, int tmin, int tmax, PolyId reference)
        {
            if (nints + 1 > maxInts)
                return;

            //find insertion point
            int idx = 0;
            while (idx < nints)
            {
                if (tmax <= ints[idx].TMin)
                    break;
                idx++;
            }

            //move current results
            if (nints - idx > 0)
            {
                for (int i = 0; i < nints - idx; i++)
                    ints[idx + 1 + i] = ints[idx + i];
            }

            //store
            ints[idx].Reference = reference;
            ints[idx].TMin = tmin;
            ints[idx].TMax = tmax;
            nints++;
        }

        /// <summary>
        /// Get edge midpoint between two prolygons
        /// </summary>
        /// <param name="from">"From" polygon reference</param>
        /// <param name="fromPoly">"From" polygon data</param>
        /// <param name="fromTile">"From" mesh tile</param>
        /// <param name="to">"To" polygon reference</param>
        /// <param name="toPoly">"To" polygon data</param>
        /// <param name="toTile">"To" mesh tile</param>
        /// <param name="mid">Edge midpoint</param>
        /// <returns>True, if midpoint found. False, if otherwise.</returns>
        public bool GetEdgeMidPoint(PolyId from, Poly fromPoly, MeshTile fromTile, PolyId to, Poly toPoly, MeshTile toTile, ref Vector3 mid)
        {
            Vector3 left = new Vector3();
            Vector3 right = new Vector3();
            if (!GetPortalPoints(from, fromPoly, fromTile, to, toPoly, toTile, ref left, ref right))
                return false;

            mid = (left + right) * 0.5f;

            return true;
        }

        /// <summary>
        /// Find points on the left and right side.
        /// </summary>
        /// <param name="from">"From" polygon reference</param>
        /// <param name="to">"To" polygon reference</param>
        /// <param name="left">Point on the left side</param>
        /// <param name="right">Point on the right side</param>
        /// <param name="fromType">Polygon type of "From" polygon</param>
        /// <param name="toType">Polygon type of "To" polygon</param>
        /// <returns>True, if points found. False, if otherwise.</returns>
        public bool GetPortalPoints(PolyId from, PolyId to, ref Vector3 left, ref Vector3 right, ref PolygonType fromType, ref PolygonType toType)
        {
            MeshTile fromTile;
            Poly fromPoly;
            if (nav.TryGetTileAndPolyByRef(from, out fromTile, out fromPoly) == false)
                return false;
            fromType = fromPoly.PolyType;

            MeshTile toTile;
            Poly toPoly;
            if (nav.TryGetTileAndPolyByRef(to, out toTile, out toPoly) == false)
                return false;
            toType = toPoly.PolyType;

            return GetPortalPoints(from, fromPoly, fromTile, to, toPoly, toTile, ref left, ref right);
        }

        /// <summary>
        /// Find points on the left and right side.
        /// </summary>
        /// <param name="from">"From" polygon reference</param>
        /// <param name="fromPoly">"From" polygon data</param>
        /// <param name="fromTile">"From" mesh tile</param>
        /// <param name="to">"To" polygon reference</param>
        /// <param name="toPoly">"To" polygon data</param>
        /// <param name="toTile">"To" mesh tile</param>
        /// <param name="left">Resulting point on the left side</param>
        /// <param name="right">Resulting point on the right side</param>
        /// <returns>True, if points found. False, if otherwise.</returns>
        public bool GetPortalPoints(PolyId from, Poly fromPoly, MeshTile fromTile, PolyId to, Poly toPoly, MeshTile toTile, ref Vector3 left, ref Vector3 right)
        {
            //find the link that points to the 'to' polygon
            Link link = null;
            foreach (Link fromLink in fromPoly.Links)
            {
                if (fromLink.Reference == to)
                {
                    link = fromLink;
                    break;
                }
            }

            if (link == null)
                return false;

            //handle off-mesh connections
            if (fromPoly.PolyType == PolygonType.OffMeshConnection)
            {
                //find link that points to first vertex
                foreach (Link fromLink in fromPoly.Links)
                {
                    if (fromLink.Reference == to)
                    {
                        int v = fromLink.Edge;
                        left = fromTile.Verts[fromPoly.Verts[v]];
                        right = fromTile.Verts[fromPoly.Verts[v]];
                        return true;
                    }
                }

                return false;
            }

            if (toPoly.PolyType == PolygonType.OffMeshConnection)
            {
                //find link that points to first vertex
                foreach (Link toLink in toPoly.Links)
                {
                    if (toLink.Reference == from)
                    {
                        int v = toLink.Edge;
                        left = toTile.Verts[toPoly.Verts[v]];
                        right = toTile.Verts[toPoly.Verts[v]];
                        return true;
                    }
                }

                return false;
            }

            //find portal vertices
            int v0 = fromPoly.Verts[link.Edge];
            int v1 = fromPoly.Verts[(link.Edge + 1) % fromPoly.VertCount];
            left = fromTile.Verts[v0];
            right = fromTile.Verts[v1];

            //if the link is at the tile boundary, clamp the vertices to tile width
            if (link.Side != BoundarySide.Internal)
            {
                //unpack portal limits
                if (link.BMin != 0 || link.BMax != 255)
                {
                    float s = 1.0f / 255.0f;
                    float tmin = link.BMin * s;
                    float tmax = link.BMax * s;
                    left = Vector3.Lerp(fromTile.Verts[v0], fromTile.Verts[v1], tmin);
                    right = Vector3.Lerp(fromTile.Verts[v0], fromTile.Verts[v1], tmax);
                }
            }

            return true;
        }

        /// <summary>
        /// Given a point on the polygon, find the closest point
        /// </summary>
        /// <param name="reference">Polygon reference</param>
        /// <param name="pos">Given point</param>
        /// <param name="closest">Resulting closest point</param>
        /// <returns>True, if point found. False, if otherwise.</returns>
        public bool ClosestPointOnPoly(PolyId reference, Vector3 pos, ref Vector3 closest)
        {
            if (nav == null)
                return false;

            MeshTile tile;
            Poly poly;

            if (nav.TryGetTileAndPolyByRef(reference, out tile, out poly) == false)
                return false;

            if (tile == null)
                return false;

            tile.ClosestPointOnPoly(poly, pos, ref closest);
            return true;
        }

        /// <summary>
        /// Given a point on the polygon, find the closest point
        /// </summary>
        /// <param name="reference">Polygon reference</param>
        /// <param name="pos">Current position</param>
        /// <param name="closest">Resulting closest position</param>
        /// <param name="posOverPoly">Determines whether the position can be found on the polygon</param>
        /// <returns>True, if the closest point is found. False, if otherwise.</returns>
        public bool ClosestPointOnPoly(PolyId reference, Vector3 pos, out Vector3 closest, out bool posOverPoly)
        {
            posOverPoly = false;
            closest = Vector3.Zero;

            MeshTile tile;
            Poly poly;
            if (!nav.TryGetTileAndPolyByRef(reference, out tile, out poly))
                return false;
            if (tile == null)
                return false;

            if (poly.PolyType == PolygonType.OffMeshConnection)
            {
                Vector3 v0 = tile.Verts[poly.Verts[0]];
                Vector3 v1 = tile.Verts[poly.Verts[1]];
                float d0 = (pos - v0).Length();
                float d1 = (pos - v1).Length();
                float u = d0 / (d0 + d1);
                closest = Vector3.Lerp(v0, v1, u);
                return true;
            }

            int indexPoly = 0;
            for (int i = 0; i < tile.Polys.Length; i++)
            {
                if (tile.Polys[i] == poly)
                {
                    indexPoly = i;
                    break;
                }
            }

            PolyMeshDetail.MeshData pd = tile.DetailMeshes[indexPoly];

            //Clamp point to be inside the polygon
            Vector3[] verts = new Vector3[PathfindingCommon.VERTS_PER_POLYGON];
            float[] edgeDistance = new float[PathfindingCommon.VERTS_PER_POLYGON];
            float[] edgeT = new float[PathfindingCommon.VERTS_PER_POLYGON];
            int numPolyVerts = poly.VertCount;
            for (int i = 0; i < numPolyVerts; i++)
                verts[i] = tile.Verts[poly.Verts[i]];

            closest = pos;
            if (!Distance.PointToPolygonEdgeSquared(pos, verts, numPolyVerts, edgeDistance, edgeT))
            {
                //Point is outside the polygon
                //Clamp to nearest edge
                float minDistance = float.MaxValue;
                int minIndex = -1;
                for (int i = 0; i < numPolyVerts; i++)
                {
                    if (edgeDistance[i] < minDistance)
                    {
                        minDistance = edgeDistance[i];
                        minIndex = i;
                    }
                }

                Vector3 va = verts[minIndex];
                Vector3 vb = verts[(minIndex + 1) % numPolyVerts];
                closest = Vector3.Lerp(va, vb, edgeT[minIndex]);
            }
            else
            {
                posOverPoly = false;
            }

            //find height at the location
            for (int j = 0; j < tile.DetailMeshes[indexPoly].TriangleCount; j++)
            {
                PolyMeshDetail.TriangleData t = tile.DetailTris[pd.TriangleIndex + j];
                Vector3 va, vb, vc;

                if (t.VertexHash0 < poly.VertCount)
                    va = tile.Verts[poly.Verts[t.VertexHash0]];
                else
                    va = tile.DetailVerts[pd.VertexIndex + (t.VertexHash0 - poly.VertCount)];

                if (t.VertexHash1 < poly.VertCount)
                    vb = tile.Verts[poly.Verts[t.VertexHash1]];
                else
                    vb = tile.DetailVerts[pd.VertexIndex + (t.VertexHash1 - poly.VertCount)];

                if (t.VertexHash2 < poly.VertCount)
                    vc = tile.Verts[poly.Verts[t.VertexHash2]];
                else
                    vc = tile.DetailVerts[pd.VertexIndex + (t.VertexHash2 - poly.VertCount)];

                float h;
                if (Distance.PointToTriangle(pos, va, vb, vc, out h))
                {
                    closest.Y = h;
                    break;
                }
            }

            return true;
        }

        /// <summary>
        /// Given a point on a polygon, find the closest point which lies on the polygon boundary.
        /// </summary>
        /// <param name="reference">Polygon reference</param>
        /// <param name="pos">Current position</param>
        /// <param name="closest">Resulting closest point</param>
        /// <returns>True, if the closest point is found. False, if otherwise.</returns>
        public bool ClosestPointOnPolyBoundary(PolyId reference, Vector3 pos, ref Vector3 closest)
        {
            MeshTile tile;
            Poly poly;
            if (nav.TryGetTileAndPolyByRef(reference, out tile, out poly) == false)
                return false;

            tile.ClosestPointOnPolyBoundary(poly, pos, out closest);
            return true;
        }

        /// <summary>
        /// Add a vertex to the straight path.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="flags"></param>
        /// <param name="reference"></param>
        /// <param name="straightPath">An array of points on the straight path</param>
        /// <param name="straightPathFlags">An array of flags</param>
        /// <param name="straightPathRefs">An array of polygon references</param>
        /// <param name="straightPathCount">The number of points on the path</param>
        /// <param name="maxStraightPath">The maximum length allowed for the straight path</param>
        /// <returns>True, if end of path hasn't been reached yet and path isn't full. False, if otherwise.</returns>
        public bool AppendVertex(Vector3 pos, int flags, PolyId reference, Vector3[] straightPath, int[] straightPathFlags, PolyId[] straightPathRefs, ref int straightPathCount, int maxStraightPath)
        {
            if (straightPathCount > 0 && straightPath[straightPathCount - 1] == pos)
            {
                //the vertices are equal
                //update flags and polys
                if (straightPathFlags.Length != 0)
                    straightPathFlags[straightPathCount - 1] = flags;

                if (straightPathRefs.Length != 0)
                    straightPathRefs[straightPathCount - 1] = reference;
            }
            else
            {
                //append new vertex
                straightPath[straightPathCount] = pos;

                if (straightPathFlags.Length != 0)
                    straightPathFlags[straightPathCount] = flags;

                if (straightPathRefs.Length != 0)
                    straightPathRefs[straightPathCount] = reference;

                straightPathCount++;

                if (flags == PathfindingCommon.STRAIGHTPATH_END || straightPathCount >= maxStraightPath)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Update the vertices on the straight path
        /// </summary>
        /// <param name="startIdx">Original path's starting index</param>
        /// <param name="endIdx">Original path's end index</param>
        /// <param name="endPos">The end position</param>
        /// <param name="path">The original path of polygon references</param>
        /// <param name="straightPath">An array of points on the straight path</param>
        /// <param name="straightPathFlags">An array of flags</param>
        /// <param name="straightPathRefs">An array of polygon references</param>
        /// <param name="straightPathCount">The number of points on the path</param>
        /// <param name="maxStraightPath">The maximum length allowed for the straight path</param>
        /// <param name="options">Options flag</param>
        /// <returns></returns>
        public bool AppendPortals(int startIdx, int endIdx, Vector3 endPos, PolyId[] path, Vector3[] straightPath, int[] straightPathFlags, PolyId[] straightPathRefs, ref int straightPathCount, int maxStraightPath, PathBuildFlags options)
        {
            Vector3 startPos = straightPath[straightPathCount - 1];

            //append or update last vertex
            bool stat = false;
            for (int i = startIdx; i < endIdx; i++)
            {
                //calculate portal
                PolyId from = path[i];
                MeshTile fromTile;
                Poly fromPoly;
                if (nav.TryGetTileAndPolyByRef(from, out fromTile, out fromPoly) == false)
                    return false;

                PolyId to = path[i + 1];
                MeshTile toTile;
                Poly toPoly;
                if (nav.TryGetTileAndPolyByRef(to, out toTile, out toPoly) == false)
                    return false;

                Vector3 left = new Vector3();
                Vector3 right = new Vector3();
                if (GetPortalPoints(from, fromPoly, fromTile, to, toPoly, toTile, ref left, ref right) == false)
                    break;

                if ((options & PathBuildFlags.AreaCrossingVertices) != 0)
                {
                    //skip intersection if only area crossings are requested
                    if (fromPoly.Area == toPoly.Area)
                        continue;
                }

                //append intersection
                float s, t;
                if (Intersection.SegmentSegment2D(ref startPos, ref endPos, ref left, ref right, out s, out t))
                {
                    Vector3 pt = Vector3.Lerp(left, right, t);

                    stat = AppendVertex(pt, 0, path[i + 1], straightPath, straightPathFlags, straightPathRefs, ref straightPathCount, maxStraightPath);

                    if (stat != true)
                        return true;
                }
            }

            return true;
        }

        /// <summary>
        /// Return false if the provided position is outside the xz-bounds.
        /// </summary>
        /// <param name="reference">Polygon reference</param>
        /// <param name="pos">Current position</param>
        /// <param name="height">Resulting polygon height</param>
        /// <returns>True, if height found. False, if otherwise.</returns>
        public bool GetPolyHeight(PolyId reference, Vector3 pos, ref float height)
        {
            if (nav == null)
                return false;

            MeshTile tile;
            Poly poly;
            if (!nav.TryGetTileAndPolyByRef(reference, out tile, out poly))
                return false;

            //off-mesh connections don't have detail polygons
            if (poly.PolyType == PolygonType.OffMeshConnection)
            {
                Vector3 closest;
                tile.ClosestPointOnPolyOffMeshConnection(poly, pos, out closest);
                height = closest.Y;
                return true;
            }
            else
            {
                int indexPoly = 0;
                for (int i = 0; i < tile.Polys.Length; i++)
                {
                    if (tile.Polys[i] == poly)
                    {
                        indexPoly = i;
                        break;
                    }
                }

                float h = 0;
                if (tile.ClosestHeight(indexPoly, pos, out h))
                {
                    height = h;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Find the nearest poly within a certain range.
        /// </summary>
        /// <param name="center">Center.</param>
        /// <param name="extents">Extents.</param>
        /// <returns>The neareast point.</returns>
        public NavPoint FindNearestPoly(Vector3 center, Vector3 extents)
        {
            NavPoint result;
            this.FindNearestPoly(ref center, ref extents, out result);
            return result;
        }

        /// <summary>
        /// Find the nearest poly within a certain range.
        /// </summary>
        /// <param name="center">Center.</param>
        /// <param name="extents">Extents.</param>
        /// <param name="nearestPt">The neareast point.</param>
        public void FindNearestPoly(ref Vector3 center, ref Vector3 extents, out NavPoint nearestPt)
        {
            nearestPt = NavPoint.Null;

            //TODO error state?

            // Get nearby polygons from proximity grid.
            List<PolyId> polys = new List<PolyId>(128);
            if (!QueryPolygons(ref center, ref extents, polys))
                throw new InvalidOperationException("no nearby polys?");

            float nearestDistanceSqr = float.MaxValue;
            for (int i = 0; i < polys.Count; i++)
            {
                PolyId reference = polys[i];
                Vector3 closestPtPoly;
                bool posOverPoly;
                ClosestPointOnPoly(reference, center, out closestPtPoly, out posOverPoly);

                // If a point is directly over a polygon and closer than
                // climb height, favor that instead of straight line nearest point.
                Vector3 diff = center - closestPtPoly;
                float d = 0;
                if (posOverPoly)
                {
                    MeshTile tile;
                    Poly poly;
                    nav.TryGetTileAndPolyByRefUnsafe(polys[i], out tile, out poly);
                    d = Math.Abs(diff.Y) - tile.WalkableClimb;
                    d = d > 0 ? d * d : 0;
                }
                else
                {
                    d = diff.LengthSquared();
                }

                if (d < nearestDistanceSqr)
                {
                    nearestDistanceSqr = d;
                    nearestPt = new NavPoint(reference, closestPtPoly);
                }
            }
        }

        /// <summary>
        /// Finds nearby polygons within a certain range.
        /// </summary>
        /// <param name="center">The starting point</param>
        /// <param name="extent">The range to search within</param>
        /// <param name="polys">A list of polygons</param>
        /// <returns>True, if successful. False, if otherwise.</returns>
        public bool QueryPolygons(ref Vector3 center, ref Vector3 extent, List<PolyId> polys)
        {
            Vector3 bmin = center - extent;
            Vector3 bmax = center + extent;

            int minx, miny, maxx, maxy;
            nav.CalcTileLoc(ref bmin, out minx, out miny);
            nav.CalcTileLoc(ref bmax, out maxx, out maxy);

            BBox3 bounds = new BBox3(bmin, bmax);
            int n = 0;
            for (int y = miny; y <= maxy; y++)
            {
                for (int x = minx; x <= maxx; x++)
                {
                    foreach (MeshTile neighborTile in nav.GetTilesAt(x, y))
                    {
                        n += neighborTile.QueryPolygons(bounds, polys);
                        if (n >= polys.Capacity)
                        {
                            return true;
                        }
                    }
                }
            }

            return polys.Count != 0;
        }

        public bool IsValidPolyRef(PolyId reference)
        {
            MeshTile tile;
            Poly poly;
            bool status = nav.TryGetTileAndPolyByRef(reference, out tile, out poly);
            if (status == false)
                return false;
            return true;
        }

        public bool IsInOpenList(Node node)
        {
            return (node.Flags & NodeFlags.Open) != 0;
        }

        public bool IsInClosedList(Node node)
        {
            return (node.Flags & NodeFlags.Closed) != 0;
        }

        public void SetNodeFlagOpen(ref Node node)
        {
            node.Flags |= NodeFlags.Open;
        }

        public void SetNodeFlagClosed(ref Node node)
        {
            node.Flags &= ~NodeFlags.Open;
            node.Flags |= NodeFlags.Closed;
        }

        public NodeFlags RemoveNodeFlagClosed(Node node)
        {
            return node.Flags & ~NodeFlags.Closed;
        }

        private struct QueryData
        {
            public bool Status;
            public Node LastBestNode;
            public float LastBestNodeCost;
            public NavPoint Start, End;
        }

        public struct SegInterval
        {
            public PolyId Reference;
            public int TMin, TMax;
        }
    }
}
