using VirtualSociety.Model.Tree;

namespace VirtualSociety.Model.Path
{
    public interface IPathFinder
    {
//        IList FindPath(Coords start, Coords end, IPathSpace space);
    }

    public interface IPathSpace
    {
        
    }

    

    public class QuadTreeAstarPathSolver : IPathFinder
    {
        private QuadTree _tree;

        public QuadTreeAstarPathSolver(QuadTree tree)
        {
            _tree = tree;
        }
    }
    
    
}
