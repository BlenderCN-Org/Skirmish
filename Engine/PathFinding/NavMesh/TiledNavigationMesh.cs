﻿using SharpDX;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Engine.PathFinding.NavMesh
{
    using Engine.Common;

    /// <summary>
    /// A TiledNavMesh is a continuous region, which is used for pathfinding. 
    /// </summary>
    public class TiledNavigationMesh
    {
        private Dictionary<Point, List<MeshTile>> tileSet;
        private Dictionary<MeshTile, int> tileRefs;
        private List<MeshTile> tileList;

        /// <summary>
        /// 
        /// </summary>
        public Vector3 Origin { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public float TileWidth { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public float TileHeight { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public int MaxTiles { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public int MaxPolys { get; private set; }
        /// <summary>
        /// Gets the maximum number of tiles that can be stored
        /// </summary>
        public int TileCount
        {
            get
            {
                return tileList.Count;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public PolyIdManager IdManager { get; private set; }
        /// <summary>
        /// Gets the mesh tile at a specified index.
        /// </summary>
        /// <param name="index">The index referencing a tile.</param>
        /// <returns>The tile at the index.</returns>
        public ReadOnlyCollection<MeshTile> this[Point location]
        {
            get
            {
                return new ReadOnlyCollection<MeshTile>(tileSet[location]);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public ReadOnlyCollection<MeshTile> this[int x, int y]
        {
            get
            {
                return this[new Point(x, y)];
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MeshTile this[int id]
        {
            get
            {
                int reference = this.IdManager.DecodeTileIndex(ref id);
                return this.GetTileByReference(reference);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TiledNavigationMesh"/> class.
        /// </summary>
        /// <param name="data">The Navigation Mesh data</param>
        public TiledNavigationMesh(NavigationMeshBuilder data)
        {
            this.Origin = data.Header.Bounds.Minimum;
            this.TileWidth = data.Header.Bounds.Maximum.X - data.Header.Bounds.Minimum.X;
            this.TileHeight = data.Header.Bounds.Maximum.Z - data.Header.Bounds.Minimum.Z;
            this.MaxTiles = 1;
            this.MaxPolys = data.Header.PolyCount;

            //init tiles
            this.tileSet = new Dictionary<Point, List<MeshTile>>();
            this.tileRefs = new Dictionary<MeshTile, int>();
            this.tileList = new List<MeshTile>();

            //init ID generator values
            int tileBits = GeometryUtil.Log2(GeometryUtil.NextPowerOfTwo(this.MaxTiles));
            int polyBits = GeometryUtil.Log2(GeometryUtil.NextPowerOfTwo(this.MaxPolys));

            //only allow 31 salt bits, since salt mask is calculated using 32-bit int and it will overflow
            int saltBits = Math.Min(31, 32 - tileBits - polyBits);

            //TODO handle this in a sane way/do we need this?
            if (saltBits < 10)
            {
                return;
            }

            this.IdManager = new PolyIdManager(polyBits, tileBits, saltBits);

            this.AddTile(data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="id"></param>
        public void AddTileAt(MeshTile tile, int id)
        {
            //TODO more error checking, what if tile already exists?

            Point loc = tile.Location;
            List<MeshTile> locList;
            if (!tileSet.TryGetValue(loc, out locList))
            {
                locList = new List<MeshTile>();
                locList.Add(tile);
                tileSet.Add(loc, locList);
            }
            else
            {
                locList.Add(tile);
            }

            tileRefs.Add(tile, id);

            int index = this.IdManager.DecodeTileIndex(ref id);

            //HACK this is pretty bad but only way to insert at index
            //TODO tileIndex should have a level of indirection from the list?
            while (index >= tileList.Count)
            {
                tileList.Add(null);
            }

            tileList[index] = tile;
        }
        /// <summary>
        /// Build a tile and link all the polygons togther, both internally and externally.
        /// Make sure to link off-mesh connections as well.
        /// </summary>
        /// <param name="data">Navigation Mesh data</param>
        /// <param name="lastRef">Last polygon reference</param>
        /// <param name="result">Last tile reference</param>
        public int AddTile(NavigationMeshBuilder data)
        {
            //make sure data is in right format
            var header = data.Header;

            //make sure location is free
            if (GetTileAt(header.X, header.Y, header.Layer) != null)
            {
                return 0;
            }

            int newTileId = GetNextTileRef();
            MeshTile tile = new MeshTile(new Point(header.X, header.Y), header.Layer, this.IdManager, newTileId);
            tile.Salt = this.IdManager.DecodeSalt(ref newTileId);

            if (header.BvNodeCount == 0)
            {
                tile.BVTree = null;
            }

            //patch header
            tile.Verts = data.NavVerts;
            tile.Polys = data.NavPolys;
            tile.PolyCount = header.PolyCount;
            tile.DetailMeshes = data.NavDMeshes;
            tile.DetailVerts = data.NavDVerts;
            tile.DetailTris = data.NavDTris;
            tile.BVTree = data.NavBvTree;
            tile.OffMeshConnections = data.OffMeshCons;
            tile.OffMeshConnectionCount = header.OffMeshConCount;
            tile.BvQuantFactor = header.BvQuantFactor;
            tile.BvNodeCount = header.BvNodeCount;
            tile.Bounds = header.Bounds;
            tile.WalkableClimb = header.WalkableClimb;

            //create connections within tile

            tile.ConnectIntLinks();
            tile.BaseOffMeshLinks();

            //create connections with neighbor tiles

            //connect with layers in current tile
            foreach (MeshTile layerTile in GetTilesAt(header.X, header.Y))
            {
                if (layerTile != tile)
                {
                    tile.ConnectExtLinks(layerTile, BoundarySide.Internal);
                    layerTile.ConnectExtLinks(tile, BoundarySide.Internal);
                }

                tile.ConnectExtOffMeshLinks(layerTile, BoundarySide.Internal);
                layerTile.ConnectExtOffMeshLinks(tile, BoundarySide.Internal);
            }

            //connect with neighbor tiles
            for (int i = 0; i < 8; i++)
            {
                BoundarySide b = (BoundarySide)i;
                BoundarySide bo = b.GetOpposite();
                foreach (MeshTile neighborTile in GetNeighborTilesAt(header.X, header.Y, b))
                {
                    tile.ConnectExtLinks(neighborTile, b);
                    neighborTile.ConnectExtLinks(tile, bo);
                    tile.ConnectExtOffMeshLinks(neighborTile, b);
                    neighborTile.ConnectExtOffMeshLinks(tile, bo);
                }
            }

            AddTileAt(tile, GetNextTileRef());

            return newTileId;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetNextTileRef()
        {
            //Salt is 1 for first version. As tiles get edited, change salt.
            //Salt can't be 0, otherwise the first poly of tile 0 is incorrectly seen as PolyId.Null.
            return this.IdManager.Encode(1, tileList.Count, 0);
        }
        /// <summary>
        /// Retrieve the endpoints of the offmesh connection at the specified polygon
        /// </summary>
        /// <param name="prevRef">The previous polygon reference</param>
        /// <param name="polyRef">The current polygon reference</param>
        /// <param name="startPos">The starting position</param>
        /// <param name="endPos">The ending position</param>
        /// <returns>True if endpoints found, false if not</returns>
        public bool GetOffMeshConnectionPolyEndPoints(int prevRef, int polyRef, ref Vector3 startPos, ref Vector3 endPos)
        {
            int salt = 0, indexTile = 0, indexPoly = 0;

            if (polyRef == 0)
            {
                return false;
            }

            //get current polygon
            this.IdManager.Decode(ref polyRef, out indexPoly, out indexTile, out salt);
            if (indexTile >= this.MaxTiles)
            {
                return false;
            }

            if (tileList[indexTile].Salt != salt)
            {
                return false;
            }

            MeshTile tile = tileList[indexTile];
            if (indexPoly >= tile.PolyCount)
            {
                return false;
            }

            Poly poly = tile.Polys[indexPoly];
            if (poly.PolyType != PolyType.OffMeshConnection)
            {
                return false;
            }

            int idx0 = 0, idx1 = 1;

            //find the link that points to the first vertex
            foreach (Link link in poly.Links)
            {
                if (link.Edge == 0)
                {
                    if (link.Reference != prevRef)
                    {
                        idx0 = 1;
                        idx1 = 0;
                    }

                    break;
                }
            }

            startPos = tile.Verts[poly.Vertices[idx0]];
            endPos = tile.Verts[poly.Vertices[idx1]];

            return true;
        }
        /// <summary>
        /// Get the tile reference
        /// </summary>
        /// <param name="tile">Tile to look for</param>
        /// <returns>Tile reference</returns>
        public int GetTileRef(MeshTile tile)
        {
            if (tile == null)
            {
                return 0;
            }

            int id;
            if (!tileRefs.TryGetValue(tile, out id))
            {
                id = 0;
            }

            return id;
        }
        /// <summary>
        /// Find the tile at a specific location.
        /// </summary>
        /// <param name="x">The X coordinate of the tile.</param>
        /// <param name="y">The Y coordinate of the tile.</param>
        /// <param name="layer">The layer of the tile.</param>
        /// <returns>The MeshTile at the specified location.</returns>
        public MeshTile GetTileAt(int x, int y, int layer)
        {
            return GetTileAt(new Point(x, y), layer);
        }
        /// <summary>
        /// Find the tile at a specific location.
        /// </summary>
        /// <param name="location">The (X, Y) coordinate of the tile.</param>
        /// <param name="layer">The layer of the tile.</param>
        /// <returns>The MeshTile at the specified location.</returns>
        public MeshTile GetTileAt(Point location, int layer)
        {
            //Find tile based off hash
            List<MeshTile> list;
            if (!tileSet.TryGetValue(location, out list))
            {
                return null;
            }

            return list.Find(t => t.Layer == layer);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reference"></param>
        /// <returns></returns>
        public MeshTile GetTileByReference(int reference)
        {
            return tileList[reference];
        }
        /// <summary>
        /// Find and add a tile if it is found
        /// </summary>
        /// <param name="x">The x-coordinate</param>
        /// <param name="y">The y-coordinate</param>
        /// <returns>A read-only collection of tiles at the specified coordinate</returns>
        public IEnumerable<MeshTile> GetTilesAt(int x, int y)
        {
            return GetTilesAt(new Point(x, y));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public IEnumerable<MeshTile> GetTilesAt(Point location)
        {
            //Find tile based off hash
            List<MeshTile> list;
            if (!tileSet.TryGetValue(location, out list))
            {
                return Enumerable.Empty<MeshTile>();
            }

            return new ReadOnlyCollection<MeshTile>(list);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        /// <param name="side"></param>
        /// <returns></returns>
        public IEnumerable<MeshTile> GetNeighborTilesAt(Point location, BoundarySide side)
        {
            return GetNeighborTilesAt(location.X, location.Y, side);
        }
        /// <summary>
        /// Gets the neighboring tile at that position
        /// </summary>
        /// <param name="x">The x-coordinate</param>
        /// <param name="y">The y-coordinate</param>
        /// <param name="side">The side value</param>
        /// <param name="tiles">An array of MeshTiles</param>
        /// <returns>The number of tiles satisfying the condition</returns>
        public IEnumerable<MeshTile> GetNeighborTilesAt(int x, int y, BoundarySide side)
        {
            int nx = x, ny = y;
            switch (side)
            {
                case BoundarySide.PlusX:
                    nx++;
                    break;

                case BoundarySide.PlusXPlusZ:
                    nx++;
                    ny++;
                    break;

                case BoundarySide.PlusZ:
                    ny++;
                    break;

                case BoundarySide.MinusXPlusZ:
                    nx--;
                    ny++;
                    break;

                case BoundarySide.MinusX:
                    nx--;
                    break;

                case BoundarySide.MinusXMinusZ:
                    nx--;
                    ny--;
                    break;

                case BoundarySide.MinusZ:
                    ny--;
                    break;

                case BoundarySide.PlusXMinusZ:
                    nx++;
                    ny--;
                    break;
            }

            return GetTilesAt(nx, ny);
        }
        /// <summary>
        /// Retrieve the tile and poly based off of a polygon reference
        /// </summary>
        /// <param name="reference">Polygon reference</param>
        /// <param name="tile">Resulting tile</param>
        /// <param name="poly">Resulting poly</param>
        /// <returns>True if tile and poly successfully retrieved</returns>
        public bool TryGetTileAndPolyByRef(int reference, out MeshTile tile, out Poly poly)
        {
            tile = null;
            poly = null;

            if (reference == 0)
            {
                return false;
            }

            //Get tile and poly indices
            int salt, polyIndex, tileIndex;
            this.IdManager.Decode(ref reference, out polyIndex, out tileIndex, out salt);

            //Make sure indices are valid
            if (tileIndex >= this.MaxTiles)
            {
                return false;
            }

            if (tileList[tileIndex].Salt != salt)
            {
                return false;
            }

            if (polyIndex >= tileList[tileIndex].PolyCount)
            {
                return false;
            }

            //Retrieve tile and poly
            tile = tileList[tileIndex];
            poly = tileList[tileIndex].Polys[polyIndex];

            return true;
        }
        /// <summary>
        /// Only use this function if it is known that the provided polygon reference is valid.
        /// </summary>
        /// <param name="reference">Polygon reference</param>
        /// <param name="tile">Resulting tile</param>
        /// <param name="poly">Resulting poly</param>
        public void TryGetTileAndPolyByRefUnsafe(int reference, out MeshTile tile, out Poly poly)
        {
            int salt, polyIndex, tileIndex;
            this.IdManager.Decode(ref reference, out polyIndex, out tileIndex, out salt);
            tile = tileList[tileIndex];
            poly = tileList[tileIndex].Polys[polyIndex];
        }
        /// <summary>
        /// Check if polygon reference is valid.
        /// </summary>
        /// <param name="reference">Polygon reference</param>
        /// <returns>True if valid</returns>
        public bool IsValidPolyRef(int reference)
        {
            if (reference == 0)
            {
                return false;
            }

            int salt, polyIndex, tileIndex;
            this.IdManager.Decode(ref reference, out polyIndex, out tileIndex, out salt);

            if (tileIndex >= this.MaxTiles)
            {
                return false;
            }

            if (tileList[tileIndex].Salt != salt)
            {
                return false;
            }

            if (polyIndex >= tileList[tileIndex].PolyCount)
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Calculates the tile location.
        /// </summary>
        /// <param name="pos">The position</param>
        /// <param name="tx">The tile's x-coordinate</param>
        /// <param name="ty">The tile's y-coordinate</param>
        public void CalcTileLoc(ref Vector3 pos, out int tx, out int ty)
        {
            tx = (int)Math.Floor((pos.X - this.Origin.X) / this.TileWidth);
            ty = (int)Math.Floor((pos.Z - this.Origin.Z) / this.TileHeight);
        }
    }
}
