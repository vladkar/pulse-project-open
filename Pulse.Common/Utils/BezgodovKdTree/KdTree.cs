using System;
using System.Collections.Generic;
using System.Linq;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Utils.BezgodovKdTree 
{
	public class KdTree<T> 
    {
	    private class Node
	    {
	        public PulseVector2 Point;
	        public T Value;
	        public Node[] KdBranch = new Node[2] {null, null};
	    }


	    Node root	=	null;

		public int MaxDepth { get; protected set; }
        
		public KdTree () { }

        public void Add( PulseVector2 point, T value )
		{
			AddNodeToKdTree( ref root, new Node() { Point = point, Value = value }, 0 );
		}

        public void Nearest (PulseVector2 target, ref T result, ref double distance)
		{
			Nearest( root, target, ref result, ref distance, 0 );
		}

        
		public void Nearest ( PulseVector2 target, ref T result )
		{
			var dummy = 0d;
			Nearest( root, target, ref result, ref dummy, 0 );
		}

		public void NearestRadius ( PulseVector2 target, double radius, out List<T> result )
		{
			var nodes = new List<Node>();
			var distances = new List<double>();
			NearestRadius ( root, target, nodes, distances, radius, 0 );

			result	=	nodes.Select( n => n.Value ).ToList();
		}
        
		void Nearest ( Node root, PulseVector2 target, ref T result, ref double distance, int depth = 0 )
		{
			if (root==null) {
				return;
			}

			#if false
			if (root==ignoreWayPoint) {
				KdNearest( root.KdBranch[ 0 ], target, ref result, ref distance, depth+1 /*, ignoreWayPoint*/ );
				KdNearest( root.KdBranch[ 1 ], target, ref result, ref distance, depth+1 /*, ignoreWayPoint*/ );
				return;
			}
			#endif

		    var dist = root.Point.DistanceTo(target);
			var delta		=	KdTreeDelta ( root.Point, target, depth );
			var   branch	=	KdTreeBranch( root.Point, target, depth );

			if ( result == null || dist < distance ) {
				result = root.Value;
				distance = dist;
			}

			if ( dist<0.0001d ) {
				//Console.WriteLine("fuck");
			}

			if ( root.Point==target ) {
				return;
			}

			Nearest( root.KdBranch[ branch ], target, ref result, ref distance, depth+1 /*, ignoreWayPoint*/ );

			if ( Math.Abs(delta) >= distance ) return;

			Nearest( root.KdBranch[ 1-branch ], target, ref result, ref distance, depth+1 /*, ignoreWayPoint*/ );
		}


		void NearestRadius ( Node root, PulseVector2 target, List<Node> waypoints, List<double> distances, double radius, int depth=0 )
		{
			if (root==null) return;

		    var dist = root.Point.DistanceTo(target);
			var delta		=	KdTreeDelta( root.Point, target, depth );
			var   branch	=	KdTreeBranch( root.Point, target, depth );

			if ( dist < radius ) {
				waypoints.Add( root );
				distances.Add( dist );
			}

			NearestRadius( root.KdBranch[ branch ], target, waypoints, distances, radius, depth+1 );

			if ( Math.Abs(delta) >= radius ) return;

			NearestRadius( root.KdBranch[ 1-branch ], target, waypoints, distances, radius, depth+1 );
		}


		double KdTreeDelta ( PulseVector2 pivot, PulseVector2 point, int depth )
		{
			double pivotX = ((depth%2)==0) ? pivot.X : pivot.Y;
			double pointX = ((depth%2)==0) ? point.X : point.Y;
			return pivotX - pointX;
		}

        
		int KdTreeBranch ( PulseVector2 pivot, PulseVector2 point, int depth )
		{
			double delta = KdTreeDelta( pivot, point, depth );
			return (delta > 0) ? 1 : 0;
		}
        
		void AddNodeToKdTree ( ref Node root, Node node, int depth )
		{
			node.KdBranch[0] = null;
			node.KdBranch[1] = null;

			if ( root==null ) {
				root = node;
				MaxDepth = Math.Max( MaxDepth, depth );
				return;
			}

			int branch = KdTreeBranch( root.Point, node.Point, depth );

			if ( root.KdBranch[ branch ]==null ) {
				root.KdBranch[ branch ] = node;
				MaxDepth = Math.Max( MaxDepth, depth );
				return;
			}

			AddNodeToKdTree( ref root.KdBranch[ branch ], node, depth + 1 );
		}
	}
}
