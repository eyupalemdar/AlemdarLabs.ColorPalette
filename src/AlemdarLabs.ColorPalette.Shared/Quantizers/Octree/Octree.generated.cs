using AlemdarLabs.ColorPalette.MachineLearning.Helpers;
using System.Collections.Generic;

namespace AlemdarLabs.ColorPalette.MachineLearning
{
    public partial class Octree
    {
        private class Node
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="baseLengthVal">Length of this node, not taking looseness into account.</param>
            /// <param name="minSizeVal">Minimum size of nodes in this octree.</param>
            /// <param name="centerVal">Center position of this node.</param>
            public Node(float baseLengthVal, float minSizeVal, Point centerVal)
            {
                SetValues(baseLengthVal, minSizeVal, centerVal);
            }

            /// <summary>
            /// Center of this node
            /// </summary>
            public Point Center { get; private set; }

            /// <summary>
            /// Length of the sides of this node
            /// </summary>
            public float SideLength { get; private set; }

            /// <summary>
            /// Minimum size for a node in this octree
            /// </summary>
            private float _minSize;

            /// <summary>
            /// Objects in this node
            /// </summary>
            private readonly List<OctreeObject> _objects = new List<OctreeObject>();

            /// <summary>
            /// Child nodes, if any
            /// </summary>
            private Node[] _children = null;

            /// <summary>
            /// If there are already NumObjectsAllowed in a node, we split it into children
            /// </summary>
            /// <remarks>
            /// A generally good number seems to be something around 8-15
            /// </remarks>
            private const int NumObjectsAllowed = 8;

            /// <summary>
            /// Gets a value indicating whether this node has children
            /// </summary>
            private bool HasChildren
            {
                get { return _children != null; }
            }


        }

        /// <summary>
        /// An object in the octree
        /// </summary>
        private class OctreeObject
        {
            /// <summary>
            /// Object content
            /// </summary>
            public T Obj;

            /// <summary>
            /// Object position
            /// </summary>
            public Point Pos;
        }
    }
}
