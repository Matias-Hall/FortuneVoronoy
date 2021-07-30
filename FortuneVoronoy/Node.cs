using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FortuneVoronoy
{
    public class Node
    {
        public bool IsParabola { get; set; }
        public Ray Ray { get; set; }
        public Parabola Parabola { get; set; }
        public Node LeftChildren { get; set; } //Smaller / to the left.
        public Node RightChildren { get; set; } //Larger / to the right.
        public void AssignLeftChildren(Node child)
        {
            LeftChildren = child;
            child.Parent = this;
        }
        public void AssignRightChildren(Node child)
        {
            RightChildren = child;
            child.Parent = this;
        }
        public Node Parent { get; set; }
        public Node(Node parent)
        {
            Parent = parent;
            Exists = true;
        }
        public bool IsLeftChildren { get => Parent?.LeftChildren == this; }
        public bool IsRoot { get; set; }
        public bool Exists { get; set; }
    }
}
