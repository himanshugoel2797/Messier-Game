using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.DataStructures
{
    public class NTree<T>
    {
        public NTree<T> Parent { get; set; }
        public NTree<T>[] Children { get; set; }
        public T Value { get; set; }

        public NTree(NTree<T> p, T value, int childCount)
        {
            this.Value = value;
            this.Parent = p;
            this.Children = new NTree<T>[childCount];
        }

        public void Add(NTree<T> e)
        {

        }
    }
}
