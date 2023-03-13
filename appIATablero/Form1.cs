using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace appIATablero
{
	public partial class Form1 : Form
	{

		// VARIABLES GLOBALES
		List<RectangleF> coordenadas = new List<RectangleF>(); //GUARDARA INFORMACION DE LAS POSICIONES 
															   //DE CUBOS GENERADOS
		List<RectangleF> pasosAbiertos = new List<RectangleF>(); //GUARDA POSICIONES DE PASOS ABIERTOS PARA
																 //GENERAR LOS PUNTOS.

		List<metadatosCuadricula> posicionesCuad = new List<metadatosCuadricula>(); //GUARDA LOS DATOS PARA HEURISTIC

		List<metadatosCuadricula> posiciones = new List<metadatosCuadricula>(); //GUARDA POSICIONES DE OBJETIVO Y ACTOR
		List<int> puntajes = new List<int>();
		List<Nodo> path;

		private const int dimensionX = 545;
		private const int dimensionY = 350;
		private const int Cuadricula = 10;

		public Form1()
		{
			InitializeComponent();
		}


		private void panel1_Paint(object sender, PaintEventArgs e)
		{
			Pen pincel = new Pen(Color.Black, 1);

			PointF[] tablero =
			{
			new PointF(10.0F, 10.0F),
			new PointF (dimensionX, 10.0F),
			new PointF (dimensionX, dimensionY),
			new PointF (10.0F, dimensionY),
			new PointF(10.0F, 10.0F)
			};

			// TABLERO DIMESIONES A GENERAR OBSTACULOS
			// x10, y10					x545, y10
			//    ----------------------------
			//	  .							 .
			//	  .							 .
			//	  .							 .
			//	  .							 .
			//	  .							 .
			//    ----------------------------
			// x10, y350				x545, y350

			// GENERA CUADRICULA DE 10 x 10

			float espaciadoX = (dimensionX - 10) / 10.0f;
			float espaciadoY = (dimensionY - 10) / 10.0f; //PIXELES ENTRE DIVISION DE AREAS 350px
														  //-10 por el espacio que comienza a generar linea en el panel
			generaCuadricula(10, dimensionY, espaciadoX, espaciadoY);
			e.Graphics.DrawLines(pincel, tablero);
			generaObstaculos(e);
		}

		private void generaObstaculos(PaintEventArgs e)
		{

			Random random = new Random();
			int des;
			SolidBrush paso = new SolidBrush(Color.FromArgb(50, 100, 100, 0));
			SolidBrush obstaculo = new SolidBrush(Color.FromArgb(150, 0, 0, 0));

			String pasto = Path.GetDirectoryName(Application.StartupPath) + "/imagenes/pasto.png";
			Bitmap bitPasto = new Bitmap(pasto);

			String arbusto = Path.GetDirectoryName(Application.StartupPath) + "/imagenes/arbusto.png";
			Bitmap bitArbusto = new Bitmap(arbusto);

			int x = 0;
			int y = 0;
			bool caminodata = false;

			for (int i = 0; i < coordenadas.Count; i++)
			{
				des = random.Next(0, 10);
				if (des > 1) // MAS ALTO = MAS OBSTACULOS
				{
					caminodata = true;
					pasosAbiertos.Add(coordenadas.ElementAt(i));
					e.Graphics.DrawImage(bitPasto,
						new PointF(coordenadas.ElementAt(i).X, coordenadas.ElementAt(i).Y));
				}
				else
				{
					caminodata = false;
					e.Graphics.DrawImage(bitArbusto,
						new PointF(coordenadas.ElementAt(i).X, coordenadas.ElementAt(i).Y));
				}

		//------------ GENERACION DE CUADRICULA SIMPLE ----------------

				if (x < 10)
				{
					posicionesCuad.Add(new metadatosCuadricula
					{
						posX = x,
						posY = y,
						camino = caminodata
					}) ;
				}
				else
				{
					x = 0;
					y++;
					posicionesCuad.Add(new metadatosCuadricula
					{
						posX = x,
						posY = y,
						camino = caminodata
					});
				}
				x++;
			}

			generaPuntos(e);
		}


		// generaPuntos SE ENCARGA DE GENERAR LOS PUNTOS DE ACTOR Y OBJETIVO 
		// MEDIANTE EL LIST DE "PASOS ABIERTOS" QUE ES EL QUE TIENE COORDENADAS DE PASO
		private void generaPuntos(PaintEventArgs e)
		{
			Random random = new Random();

			int pos1 = random.Next(0, pasosAbiertos.Count / 2);
			int pos2 = random.Next(pasosAbiertos.Count / 2, pasosAbiertos.Count);

			String raton = Path.GetDirectoryName(Application.StartupPath) + "/imagenes/raton.png";
			Bitmap bitRaton = new Bitmap(raton);

			String gato = Path.GetDirectoryName(Application.StartupPath) + "/imagenes/gato.png";
			Bitmap bitGato = new Bitmap(gato);

			e.Graphics.DrawImage(bitRaton,
						new PointF(pasosAbiertos.ElementAt(pos1).X + 15, pasosAbiertos.ElementAt(pos1).Y + 5));

			e.Graphics.DrawImage(bitGato,
						new PointF(pasosAbiertos.ElementAt(pos2).X + 15, pasosAbiertos.ElementAt(pos2).Y + 5));

			// posicion 0 = objetivo / posicion 1 = actor

			posiciones.Add(encuentraPos(pos1));
			posiciones.Add(encuentraPos(pos2));

		}

		private metadatosCuadricula encuentraPos(int valEval)
		{
			int posicionEnPlano = 0;

			for (int i = 0; i < coordenadas.Count; i++)
			{
				if (coordenadas.ElementAt(i).X == pasosAbiertos.ElementAt(valEval).X &&
				   coordenadas.ElementAt(i).Y == pasosAbiertos.ElementAt(valEval).Y &&
				   coordenadas.ElementAt(i).Width == pasosAbiertos.ElementAt(valEval).Width &&
				   coordenadas.ElementAt(i).Height == pasosAbiertos.ElementAt(valEval).Height)
				{
					posicionEnPlano = i;
				}
			}

			return new metadatosCuadricula
			{
				posX = posicionesCuad.ElementAt(posicionEnPlano).posX,
				posY = posicionesCuad.ElementAt(posicionEnPlano).posY,
				camino = posicionesCuad.ElementAt(posicionEnPlano).camino
			};

		}

		//GENERA CUADRICULA SE ENCARGA DE RECOPILAR POSICIONES DEL DIBUJO DIVIDIENDO EL TAMAÑO DE PIXELES
		//A PARTES PARA DIBUJAR
		private void generaCuadricula(float valIni, float valFin, float valEspa, float valEspa2)
		{
			float valPointx = valIni;
			float valPointy = valIni;

			float valSizeH = valEspa2; // HEIGHT
			float valSizeW = valEspa; // WIDTH

			for (int i = 0; i < Cuadricula; i++)
			{
				for (int j = 0; j < Cuadricula; j++)
				{
					coordenadas.Add(new RectangleF(new PointF(valPointx, valPointy), new SizeF(valSizeW, valSizeH)));
					valPointx += valSizeW;
				}
				valPointx = valIni;
				valPointy += valSizeH;
			}

		}

		private void button1_Click(object sender, EventArgs e)
		{
			pasosAbiertos.Clear();
			posicionesCuad.Clear();
			coordenadas.Clear();
			posiciones.Clear();
			this.Refresh();
			this.button2.Enabled = true;
			this.richTextBox1.Text = "";
		}

		private void button2_Click(object sender, EventArgs e)
		{
			analisis();
			Pantalla(new PaintEventArgs(this.panel1.CreateGraphics(), new Rectangle(10, 10,545, 350)));
		}

		private void analisis()
		{
			
			Heuristica ia = new Heuristica(10, posicionesCuad,
				posiciones.ElementAt(1).getX(),
				posiciones.ElementAt(1).getY(),
				posiciones.ElementAt(0).getX(),
				posiciones.ElementAt(0).getY());

			Console.WriteLine(posiciones.ElementAt(1).getX() + ", " + posiciones.ElementAt(1).getY());
			path = ia.FindPath();

		}

		private DatosDistancia obtenPuntaje(Nodo actual, int index)
		{
			List<Nodo> vecinos;

			Heuristica ia = new Heuristica(10, posicionesCuad,
				posiciones.ElementAt(1).getX(),
				posiciones.ElementAt(1).getY(),
				posiciones.ElementAt(0).getX(),
				posiciones.ElementAt(0).getY());

			vecinos = ia.GetNeighbours(actual);
			List<DatosDistancia> dist = new List<DatosDistancia>();

			foreach (Nodo nodo in vecinos)
			{
				dist.Add(new DatosDistancia
				{
					x = nodo.x,
					y = nodo.y,
					distancia = ia.GetDistance(nodo, new Nodo(posiciones.ElementAt(1).getX(), posiciones.ElementAt(1).getY(), true)),
					estado = "buscando"
				});

			}

			if (index < dist.Count)
				return dist.ElementAt(index);
			else
				return null;

		}

		private List<Nodo> CuadriculaAPuntos(Nodo actual, bool convertido)
		{
			List<Nodo> conversion = new List<Nodo>(); //TRUE = Nodo Actual
			List<Nodo> vecinos;
			
			var x = 0;
			var y = 0;

			Heuristica ia = new Heuristica(10, posicionesCuad,
				posiciones.ElementAt(1).getX(),
				posiciones.ElementAt(1).getY(),
				posiciones.ElementAt(0).getX(),
				posiciones.ElementAt(0).getY());

			vecinos = ia.GetNeighbours(actual);

			if (convertido)
			{
				foreach (Nodo nodo in vecinos)
				{
					x = ((dimensionX) / 10) * (nodo.x);
					y = ((dimensionY) / 10) * (nodo.y);
					conversion.Add(new Nodo(x, y, true));
				}

				x = ((dimensionX) / 10) * (actual.x);
				y = ((dimensionY) / 10) * (actual.y);
				conversion.Add(new Nodo(x, y, true));

				return conversion;
			}
			else
			{
				vecinos.Add(actual);
				return vecinos;
			}
			
		}

		private void Pantalla(PaintEventArgs e)
		{

			String huellitas = Path.GetDirectoryName(Application.StartupPath) + "/imagenes/pisadas.png";
			Bitmap bitPasos = new Bitmap(huellitas);
			String explorado = Path.GetDirectoryName(Application.StartupPath) + "/imagenes/pastoexp.png";
			Bitmap bitExplorado = new Bitmap(explorado);
			String atrapado = Path.GetDirectoryName(Application.StartupPath) + "/imagenes/meta.png";
			Bitmap bitAtrapado = new Bitmap(atrapado);
			String gato = Path.GetDirectoryName(Application.StartupPath) + "/imagenes/gato.png";
			Bitmap bitGato = new Bitmap(gato);

			int pasos = 0;
			int ubicacion = 0;
			int index = 0;

			int acumulado = 0;

			if (path != null)
			{
				foreach (Nodo nodo in path)
				{
					List<Nodo> nuevosVecinos = CuadriculaAPuntos(nodo, false);
					List<Nodo> pos = CuadriculaAPuntos(nodo, true);

					e.Graphics.DrawImage(bitExplorado,
									new PointF((float)pos.ElementAt(pos.Count - 1).x + 6.5f, (float)pos.ElementAt(pos.Count - 1).y + 3.5f));
				
					foreach (Nodo vecino in nuevosVecinos)
					{
						var nodos = obtenPuntaje(nodo,index);

						if(index == 0)
						{
							this.richTextBox1.AppendText("AHORA: " + nodo.x + ", " + nodo.y + "\n");
							this.richTextBox1.AppendText("PUNTAJE: " + acumulado + "\n");
							this.richTextBox1.AppendText("------------------------\n");
							this.richTextBox1.AppendText("BUSCANDO...\n");
						}

						if (nodos != null)
						{
							Console.WriteLine("x: "+nodos.x + " "+ nodos.y + " " + nodos.distancia + " " + nodos.estado);
							this.richTextBox1.AppendText("x: " + nodos.x + ", y: " + nodos.y + "/	 h:" + nodos.distancia + "\n");
							this.richTextBox1.Focus();
							acumulado += nodos.distancia;
							Console.WriteLine("ESTOY EN: " + nodo.x + " " + nodo.y);
							

							index++;
						}
						if(nodos == null)
						{
							index = 0;
						}


						while (pasos < posicionesCuad.Count)
						{
							if (vecino.x == posicionesCuad.ElementAt(pasos).getX() &&
								vecino.y == posicionesCuad.ElementAt(pasos).getY() &&
								posicionesCuad.ElementAt(pasos).getCamino() == 0 &&
								vecino.y != nodo.x && vecino.x != nodo.x)
							{

								foreach (Nodo marca in path)
								{
									if (marca.x != vecino.x && marca.y != vecino.y)
										e.Graphics.DrawImage(bitExplorado,
											new PointF((float)pos.ElementAt(ubicacion).x + 6.5f, (float)pos.ElementAt(ubicacion).y + 3.5f));
									Thread.Sleep(50);
									
								}

								e.Graphics.DrawImage(bitPasos,
									new PointF((float)pos.ElementAt(pos.Count - 1).x + 6.5f, (float)pos.ElementAt(pos.Count - 1).y + 3.5f));
								Thread.Sleep(50);

							}
							pasos++;
						}
						ubicacion++;
						pasos = 0;
					}
					ubicacion = 0;
					Thread.Sleep(50);
				}

				List<Nodo> dibujofinal = CuadriculaAPuntos(path.ElementAt(path.Count - 1), true);

				this.button2.Enabled = false;

				foreach (Nodo nodo in path)
				{
					List<Nodo> pos = CuadriculaAPuntos(nodo, true);

					e.Graphics.DrawImage(bitPasos,
									new PointF((float)pos.ElementAt(pos.Count - 1).x + 6.5f, (float)pos.ElementAt(pos.Count - 1).y + 3.5f));
				}

				e.Graphics.DrawImage(bitExplorado,
					new PointF((float)dibujofinal.ElementAt(dibujofinal.Count - 1).x + 6.5f, (float)dibujofinal.ElementAt(dibujofinal.Count - 1).y + 3.5f));
				e.Graphics.DrawImage(bitAtrapado,
					new PointF((float)dibujofinal.ElementAt(dibujofinal.Count - 1).x + 6.5f, (float)dibujofinal.ElementAt(dibujofinal.Count - 1).y + 3.5f));

				
				this.richTextBox1.AppendText("\nCOMPLETADO\n");
			}
		}

		private void label1_Click(object sender, EventArgs e)
		{

		}

		private void richTextBox1_TextChanged(object sender, EventArgs e)
		{

		}

		private void label2_Click(object sender, EventArgs e)
		{

		}

		private void label3_Click(object sender, EventArgs e)
		{

		}
	}



}
