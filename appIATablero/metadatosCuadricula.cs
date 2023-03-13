using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace appIATablero
{
	public class metadatosCuadricula
	{
		public int posX { get; set; }
		public int posY { get; set; }
		public bool camino { get; set; }
		
		public int getCamino()
		{
			if (camino)
				return 0;
			else
				return 1;
		}

		public int getX()
		{
			return posX;
		}

		public int getY()
		{
			return posY;
		}
	}

}
