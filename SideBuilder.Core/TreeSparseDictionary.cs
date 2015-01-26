// Copyright (c) 2014 Francesco Pretto
// This file is subject to the MIT license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;

namespace Collections.Specialized
{
    /// <summary>
    /// Represents a hierarchical, tree based, dictionary in which key-value pairs
    /// in a node are the sum of pairs of current node and ancestors
    /// </summary>
    public class TreeSparseDictionary<T>
    {
        #region Fields

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int _Index;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TreeSparseDictionary<T> _Root;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TreeSparseDictionary<T> _Parent;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<TreeSparseDictionary<T>> _Children;

        private BitArray _TreeMask;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Data _Data;

        #endregion // Fields

        #region Constructors

        public TreeSparseDictionary()
        {
            _Root = this;
            _Parent = null;
            _Index = 0;
            _Children = new List<TreeSparseDictionary<T>>();
            _TreeMask = null;
            _Data = new Data();
        }

        private TreeSparseDictionary(TreeSparseDictionary<T> parent)
        {
            _Root = parent.Root;
            _Parent = parent;
            _Index = _Root.Descendants.Count + 1;
            _Children = new List<TreeSparseDictionary<T>>();
            _TreeMask = null;
            _Data = null;
        }

        #endregion // Constructors

        #region Inquiry

        public bool ContainsKey(String key)
        {
            T value;
            // TODO: Dont' lookup value, just tell if the dictionary contains it
            // (easy, add a flag to TryLookupValue)
            return _Root.TryLookupValue(key, _TreeMask, _Index, out value);
        }

        public bool TryGetValue(String key, out T value)
        {
            return _Root.TryLookupValue(key, _TreeMask, _Index, out value);
        }

        public TreeSparseDictionary<T> Open()
        {
            TreeSparseDictionary<T> child = new TreeSparseDictionary<T>(this);
            string childID = child.Parent.ID + (child.Parent.ID == "" ? "" : ".") + (child.Parent.Children.Count + 1);
            _Children.Add(child);
            _Root.AddDescendant(childID);
            child.CreateTreeBitMask();
            return child;
        }

        #endregion // Inquiry

        #region Private methods

        /// <param name="treeMask">Provided treeMask of the children for comparison</param>
        /// <param name="idx">Index of the children</param>
        /// <returns></returns>
        private bool TryLookupValue(string key, BitArray treeMask, int idx, out T value)
        {
            BitArray valueMask;
            bool success = InnerDict.TryGetValue(key, out valueMask);
            if (!success)
            {
                value = default(T);
                return false;
            }

            if (treeMask == null)
            {
                if (valueMask[0])
                {
                    value = SparseValues[key];
                    return true;
                }
                else
                {
                    value = default(T);
                    return false;
                }
            }

            BitArray left = valueMask;
            BitArray right = new BitArray(left.Count);
            for (int it = 0; it < valueMask.Count && it < treeMask.Count; it++)
            {
                right[it] = treeMask[it];
            }

            BitArray ancestorValues = left.And(right);
            int rightMost = FindRightmostBitSet(ancestorValues, Math.Min(treeMask.Count, valueMask.Count));
            if (rightMost == -1)
            {
                value = default(T);
                return false;
            }
            else
            {
                string id = GetID(rightMost);
                value = SparseValues[id + key];
                return true;
            }
        }

        private void SetValue(string key, T value, int idx)
        {
            BitArray mask;
            bool found = InnerDict.TryGetValue(key, out mask);
            if (found)
            {
                if (idx > mask.Count - 1)
                {
                    bool[] values = new bool[idx + 1];
                    mask.CopyTo(values, 0);
                    mask = new BitArray(values);
                    InnerDict[key] = mask;
                }
            }
            else
            {
                mask = new BitArray(idx + 1);
                InnerDict[key] = mask;
            }

            mask[idx] = true;

            string id = GetID(idx);
            SparseValues[id + key] = value;
        }

        private int FindRightmostBitSet(BitArray array, int limitindex)
        {
            for (int it = limitindex -1 ; it >= 0; it--)
            {
                if (array[it])
                    return it;
            }

            return -1;
        }

        private void CreateTreeBitMask()
        {
            HashSet<string> lineage = new HashSet<string>();
            FillLineage(lineage);
            BitArray treeMask = new BitArray(_Root.Descendants.Count + 1);
            
            // Root is always an ancestor
            treeMask[0] = true;

            int it = 1;
            foreach (string descendantID in _Root.Descendants)
            {
                if (lineage.Contains(descendantID))
                    treeMask[it] = true;

                it++;
            }

            _TreeMask = treeMask;
        }

        private void FillLineage(HashSet<string> ids)
        {
            if (_Parent != null)
            {
                _Parent.FillLineage(ids);
                ids.Add(ID);
            }
        }

        private void AddDescendant(string id)
        {
            Descendants.Add(id);
        }

        private string GetID(int idx)
        {
            if (idx == 0)
                return "";

            return _Root.Descendants[idx - 1];
        }

        private string FormatBitmask(BitArray array)
        {
            StringBuilder builder = new StringBuilder();
            for (int it = 0; it < array.Count; it++)
            {
                builder.Append("| ");
                builder.Append(array[it] ? 1 : 0);
            }
            builder.Append(" |");

            FormatAddFrames(builder);
            string ret = builder.ToString();
            return ret;
        }

        private string FormatTreeMask(BitArray array)
        {
            StringBuilder builder = new StringBuilder();
            for (int it = 0; it < array.Count; it++)
            {
                bool ancestor = array[it];
                builder.Append("| ");
                if (ancestor)
                    builder.Append(it);
                else
                    builder.Append("#");

                builder.Append(" ");
            }
            builder.Append("|");

            FormatAddFrames(builder);
            string ret = builder.ToString();
            return ret;
        }

        private void FormatAddFrames(StringBuilder builder)
        {
            string mask = builder.ToString();

            builder.Clear();

            builder.Append('-', mask.Length);
            builder.AppendLine();
            builder.Append(mask);
            builder.AppendLine();
            builder.Append('-', mask.Length);
        }

        #endregion // Private methods

        #region Properties

        public T this[string key]
        {
            get
            {
                T ret;
                bool found = _Root.TryLookupValue(key, _TreeMask, _Index, out ret);
                if (!found)
                    throw new Exception("Key not found");

                return ret;
            }
            set { _Root.SetValue(key, value, _Index); }
        }

        public string ID
        {
            get { return GetID(_Index); }
        }

        public TreeSparseDictionary<T> Root
        {
            get { return _Root; }
        }

        public TreeSparseDictionary<T> Parent
        {
            get { return _Parent; }
        }

        #endregion // Properties

        #region Private Properties

        private List<TreeSparseDictionary<T>> Children
        {
            get { return _Children; }
        }

        public Dictionary<string, BitArray> InnerDict
        {
            get { return _Data.InnerDict; }
        }

        public Dictionary<string, T> SparseValues
        {
            get { return _Data.SparseValues; }
        }

        private List<string> Descendants
        {
            get { return _Data.Descendants; }
        }

        #endregion // Private Properties

        #region Support

        private class Data
        {
            private Dictionary<string, BitArray> _InnerDict;
            private Dictionary<string, T> _SparseValues;
            private List<string> _Descendants;

            public Data()
            {
                _InnerDict = new Dictionary<string, BitArray>();
                _SparseValues = new Dictionary<string, T>();
                _Descendants = new List<string>();
            }

            public Dictionary<string, BitArray> InnerDict
            {
                get { return _InnerDict; }
            }

            public Dictionary<string, T> SparseValues
            {
                get { return _SparseValues; }
            }

            public List<string> Descendants
            {
                get { return _Descendants; }
            }
        }

        #endregion // Support
    }
}
