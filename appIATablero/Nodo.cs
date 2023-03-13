using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace appIATablero
{
	class Nodo
	{
        public int x;
        public int y;
        public bool obstaculo;
        public Nodo padre;
        public int g;
        public int h;

        public Nodo(int x, int y, bool obstaculo)
        {
            this.x = x;
            this.y = y;
            this.obstaculo = obstaculo;
            padre = null;
            g = 0;
            h = 0;
        }

        public int f
        {
            get
            {
                return g + h;
            }
        }
    }
}
