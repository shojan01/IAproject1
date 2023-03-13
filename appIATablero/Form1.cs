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

		List<Nodo> path; //GUARDA EL CAMINO TRAZADO DE LA HEURISTICA

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

		// generaObstaculos se encarga de dibujar en la cuadricula los caminos y obstaculos en base un
		// numero aleatorio, dando asi cada generacion de area, un espacio diferente.
		private void generaObstaculos(PaintEventArgs e)
		{

			Random random = new Random();
			int des;

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
					//Si es mayor a 1 dibujalo como camino
					caminodata = true;
					pasosAbiertos.Add(coordenadas.ElementAt(i));
					e.Graphics.DrawImage(bitPasto,
						new PointF(coordenadas.ElementAt(i).X, coordenadas.ElementAt(i).Y));
				}
				else
				{
					//Si es menor, hazlo obstaculo
					caminodata = false;
					e.Graphics.DrawImage(bitArbusto,
						new PointF(coordenadas.ElementAt(i).X, coordenadas.ElementAt(i).Y));
				}

		//------------ GENERACION DE CUADRICULA SIMPLE ----------------
		// X se refiere a las columnas, Y a los renglones

				if (x < 10)
				{
					posicionesCuad.Add(new metadatosCuadricula
					{
						posX = x,
						posY = y,
						camino = caminodata //camino data indica si en el registro es un camino o un obstaculo
					}) ;
				}
				else
				{
					// si X pasa de 10 vuelve a 0 y Y aumenta uno mas para rellenar las columnas del sig renglon 
					x = 0;
					y++;
					posicionesCuad.Add(new metadatosCuadricula
					{
						posX = x,
						posY = y,
						camino = caminodata 
					});
				}
				x++; // x va sumandose cada vuelta para meter un nuevo dato de cada columna
			}

			generaPuntos(e);
		}


		// generaPuntos SE ENCARGA DE GENERAR LOS PUNTOS DE ACTOR Y OBJETIVO 
		// MEDIANTE EL LIST DE "PASOS ABIERTOS" QUE ES EL QUE TIENE COORDENADAS DE PASO
		private void generaPuntos(PaintEventArgs e)
		{
			Random random = new Random();

			//Toma un numero al azar de la lista pasosAbiertos, el objetivo va de 0 a media lista
			// y el actor o agente de media lista al resto, para evitar que puedan generarse en el mismo punto
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
			// Guardamos las posiciones en un List que solo contiene al objetivo y al agente
			posiciones.Add(encuentraPos(pos1));
			posiciones.Add(encuentraPos(pos2));

		}

		// encuentraPos recibe como parametro un valor entero para ubicar en posicionesCuad
		// si existe en las coordenadas generales, este se rellena en un metadatosCuadricula y se entrega para que sea utilizado,
		// se utiliza para rellenar datos que requiere la heuristica
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
					posicionEnPlano = i; //SI LO ENCUENTRA, GUARDA LA POSICION PARA UTILIZARLA
				}
			}

			// RELLENA DATOS UBICADOS EN posicionesCuad QUE COMPARTEN MISMO NUMERO CON coordenadas
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
			float valPointx = valIni;	// X
			float valPointy = valIni;	// Y

			float valSizeH = valEspa2; // ALTO
			float valSizeW = valEspa; // ANCHO

			for (int i = 0; i < Cuadricula; i++)
			{
				for (int j = 0; j < Cuadricula; j++)
				{
					//Aqui se rellenan las coordenadas generales que son enfocadas en el dibujado
					coordenadas.Add(new RectangleF(new PointF(valPointx, valPointy), new SizeF(valSizeW, valSizeH)));
					valPointx += valSizeW; // numero valor x = ancho de esta misma
				}
				//Cuando termina la primer serie de columnas x retorna a valor inicial y Y aumenta a el alto del mismo
				valPointx = valIni;
				valPointy += valSizeH;
			}

		}

		private void button1_Click(object sender, EventArgs e)
		{
			//CADA GENERACION DE AREA REQUIERE UN FLUSH (BORRA DATOS DE VARIABLES GLOBALES Y LIBERA MEMORIA)
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

		// analisis Se encarga de hacer la llamada principal de la heuristica
		private void analisis()
		{
			Heuristica ia = new Heuristica(10, posicionesCuad,
				posiciones.ElementAt(1).getX(),
				posiciones.ElementAt(1).getY(),
				posiciones.ElementAt(0).getX(),
				posiciones.ElementAt(0).getY());

			path = ia.FindPath();
		}

		// obtenPuntaje retorna un valor de DatosDistancia, realiza un calculo de distancia del nodo actual al nodo objetivo
		// requiere un indice para saber que valor retornar de la lista de vecinos de las cuales calculo esa distancia
		private DatosDistancia obtenPuntaje(Nodo actual, int index)
		{

			List<Nodo> vecinos;

			//SE INVOCA UNA HEURISTICA CON LOS MISMOS VALORES
			Heuristica ia = new Heuristica(10, posicionesCuad,
				posiciones.ElementAt(1).getX(),
				posiciones.ElementAt(1).getY(),
				posiciones.ElementAt(0).getX(),
				posiciones.ElementAt(0).getY());

			//TOMAMOS LA LISTA DE VECINOS QUE SE GENERO
			vecinos = ia.GetNeighbours(actual);
			List<DatosDistancia> dist = new List<DatosDistancia>();

			foreach (Nodo nodo in vecinos)
			{
				//EN CICLO VAMOS AGREGANDO DatosDistancia LLEGADOS DE LOS NODOS VECINOS LOS CUALES SE ENRIQUECEN CON INFORMACION DE POSICION, DISTANCIA Y ESTADO
				dist.Add(new DatosDistancia
				{
					x = nodo.x,
					y = nodo.y,
					distancia = ia.GetDistance(nodo, new Nodo(posiciones.ElementAt(1).getX(), posiciones.ElementAt(1).getY(), true)),
					estado = "buscando"
				});

			}

			//CON BASE AL INDICE REGRESAMOS EL ELEMENTO DESEADO, SI ES MAS ALTO A LA LISTA DE VECINOS, RETORNA NULL
			if (index < dist.Count)
				return dist.ElementAt(index);
			else
				return null;

		}

		// CuadriculaAPuntos se encarga de convertir la cuadricula simple a pixeles para dibujar en pantalla
		// o simplemente retornar de nuevo los valores vecinos (convertidos o no)
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

			vecinos = ia.GetNeighbours(actual); // INICIA UNA HEURISTICA Y OBTEN VECINOS DEL NODO ACTUAL

			//si esta convertiido, rellena los nodos con posiciones en pixel
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
				//si no, solo agrega con valor simple
				vecinos.Add(actual);
				return vecinos;
			}
			
		}

		// Pantalla se encarga de interpretar todos los datos obtenidos de la heuristica principal, nodos e informacion extra
		// al Frame principal
		private void Pantalla(PaintEventArgs e)
		{
			// CARGA IMAGENES PARA DIBUJAR MOVIMIENTOS
			String huellitas = Path.GetDirectoryName(Application.StartupPath) + "/imagenes/pisadas.png";
			Bitmap bitPasos = new Bitmap(huellitas);
			String explorado = Path.GetDirectoryName(Application.StartupPath) + "/imagenes/pastoexp.png";
			Bitmap bitExplorado = new Bitmap(explorado);
			String atrapado = Path.GetDirectoryName(Application.StartupPath) + "/imagenes/meta.png";
			Bitmap bitAtrapado = new Bitmap(atrapado);
			String gato = Path.GetDirectoryName(Application.StartupPath) + "/imagenes/gato.png";
			Bitmap bitGato = new Bitmap(gato);

			int pasos = 0; //VALOR PARA UBICAR VECINOS EN EL PLANO
			int ubicacion = 0; //VALOR QUE AYUDA A UBICAR VALORES PARA PINTAR AREAS VECINAS
			int index = 0; //SE UTILIZA DE INDICE PARA obtenPuntaje

			int acumulado = 0; //ACUMULADO GUARDA PUNTAJE 

			if (path != null)
			{
				foreach (Nodo nodo in path)
				{
					// SE GENERAN DOS LISTAS DE NODOS VECINOS CON MISMOS PARAMETROS, PERO UNA SIMPLE Y UNA A PIXELES
					List<Nodo> nuevosVecinos = CuadriculaAPuntos(nodo, false);
					List<Nodo> pos = CuadriculaAPuntos(nodo, true);

					// DIBUJA DATO EXPLORADO = NODO INICIAL
					e.Graphics.DrawImage(bitExplorado,
									new PointF((float)pos.ElementAt(pos.Count - 1).x + 6.5f, (float)pos.ElementAt(pos.Count - 1).y + 3.5f));
					
					// SE ENCARGA DE INTERPRETAR VECINOS Y MOSTRAR SUS DISTANCIAS, INDICE ES CLAVE PARA DETERMINAR QUE POSICIONES
					// SE MUESTRAN
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
							this.richTextBox1.AppendText("x: " + nodos.x + ", y: " + nodos.y + "/	 h:" + nodos.distancia + "\n");
							this.richTextBox1.Focus();
							acumulado += nodos.distancia;
							index++;
						}
						if(nodos == null)
						{
							index = 0;
						}

						//ESTE CICLO SE ENCARGA DE PINTAR BUSQUEDA MIENTRAS EL AREA A PINTAR NO SEA UN OBSTACULO QUE NO SE PUEDA VISITAR
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
									Thread.Sleep(100);
									
								}

								e.Graphics.DrawImage(bitPasos,
									new PointF((float)pos.ElementAt(pos.Count - 1).x + 6.5f, (float)pos.ElementAt(pos.Count - 1).y + 3.5f));
								Thread.Sleep(100);

							}
							pasos++;
						}
						ubicacion++;
						pasos = 0;
					}
					ubicacion = 0;
					Thread.Sleep(100);
				}

				List<Nodo> dibujofinal = CuadriculaAPuntos(path.ElementAt(path.Count - 1), true);
				this.button2.Enabled = false;

				//HACE UN RESUMEN DE PASOS
				foreach (Nodo nodo in path)
				{
					List<Nodo> pos = CuadriculaAPuntos(nodo, true);

					e.Graphics.DrawImage(bitPasos,
									new PointF((float)pos.ElementAt(pos.Count - 1).x + 6.5f, (float)pos.ElementAt(pos.Count - 1).y + 3.5f));
				}


				//UNA VEZ TERMINADO EL CICLO, ESTE PINTA AL FINAL AL AGENTE ATRAPANDO AL OBJETIVO ;)
				e.Graphics.DrawImage(bitExplorado,
					new PointF((float)dibujofinal.ElementAt(dibujofinal.Count - 1).x + 6.5f, (float)dibujofinal.ElementAt(dibujofinal.Count - 1).y + 3.5f));
				e.Graphics.DrawImage(bitAtrapado,
					new PointF((float)dibujofinal.ElementAt(dibujofinal.Count - 1).x + 6.5f, (float)dibujofinal.ElementAt(dibujofinal.Count - 1).y + 3.5f));

				
				//INDICA QUE LA BUSQUEDA TERMINO
				this.richTextBox1.AppendText("\nCOMPLETADO\n");
			}
		}


	}



}
