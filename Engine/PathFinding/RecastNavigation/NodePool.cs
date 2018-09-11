﻿using System;
using System.Runtime.InteropServices;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Node pool
    /// </summary>
    public class NodePool : IDisposable
    {
        public int m_maxNodes;
        public int m_nodeCount;

        public Node[] m_nodes;
        public int[] m_first;
        public int[] m_next;
        public int m_hashSize;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxNodes">Maximum nodes in pool</param>
        /// <param name="hashSize">Hash size</param>
        public NodePool(int maxNodes, int hashSize)
        {
            m_maxNodes = maxNodes;
            m_hashSize = hashSize;

            m_nodes = new Node[m_maxNodes];
            m_next = Helper.CreateArray(m_maxNodes, Detour.DT_NULL_IDX);
            m_first = Helper.CreateArray(m_hashSize, Detour.DT_NULL_IDX);
            m_nodeCount = 0;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~NodePool()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.m_nodes = null;
                this.m_next = null;
                this.m_first = null;
            }
        }

        public void Clear()
        {
            m_first = Helper.CreateArray(m_hashSize, Detour.DT_NULL_IDX);
            m_nodeCount = 0;
        }
        public Node GetNode(int id, int state)
        {
            int bucket = Detour.HashRef(id) & (m_hashSize - 1);
            int i = m_first[bucket];
            while (i != Detour.DT_NULL_IDX)
            {
                if (m_nodes[i] != null && m_nodes[i].id == id && m_nodes[i].state == state)
                {
                    return m_nodes[i];
                }
                i = m_next[i];
            }

            if (m_nodeCount >= m_maxNodes)
            {
                return null;
            }

            i = m_nodeCount;
            m_nodeCount++;

            // Init node
            m_nodes[i] = new Node
            {
                pidx = 0,
                cost = 0,
                total = 0,
                id = id,
                state = state,
                flags = 0
            };

            m_next[i] = m_first[bucket];
            m_first[bucket] = i;

            return m_nodes[i];
        }
        public Node FindNode(int id, int state)
        {
            int bucket = Detour.HashRef(id) & (m_hashSize - 1);
            int i = m_first[bucket];
            while (i != Detour.DT_NULL_IDX)
            {
                if (m_nodes[i].id == id && m_nodes[i].state == state)
                {
                    return m_nodes[i];
                }
                i = m_next[i];
            }
            return null;
        }
        public int FindNodes(int id, out Node[] nodes, int maxNodes)
        {
            nodes = new Node[maxNodes];

            int n = 0;
            int bucket = Detour.HashRef(id) & (m_hashSize - 1);
            int i = m_first[bucket];
            while (i != Detour.DT_NULL_IDX)
            {
                if (m_nodes[i].id == id)
                {
                    if (n >= maxNodes)
                    {
                        return n;
                    }
                    nodes[n++] = m_nodes[i];
                }
                i = m_next[i];
            }

            return n;
        }
        public int GetNodeIdx(Node node)
        {
            if (node == null) return 0;

            return Array.IndexOf(m_nodes, node) + 1;
        }
        public Node GetNodeAtIdx(int idx)
        {
            if (idx == 0) return null;
            return m_nodes[idx - 1];
        }

        public int GetMemUsed()
        {
            return
                Marshal.SizeOf(this) +
                Marshal.SizeOf(typeof(Node)) * m_maxNodes +
                Marshal.SizeOf(typeof(int)) * m_maxNodes +
                Marshal.SizeOf(typeof(int)) * m_hashSize;
        }

        public int GetMaxNodes()
        {
            return m_maxNodes;
        }
        public int GetHashSize()
        {
            return m_hashSize;
        }
        public int GetFirst(int bucket)
        {
            return m_first[bucket];
        }
        public int GetNext(int i)
        {
            return m_next[i];
        }
        public int GetNodeCount()
        {
            return m_nodeCount;
        }
    }
}
