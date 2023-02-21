
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace appIATablero
{
	public partial class Form1 : Form
	{

		/*--------------- DETALLES TECNICOS ----------------*/
		/*
		 *dataArray.json provee las posiciones generadas en forma
		  de tabla y si es camino o un obstaculo (para python).

		 *dataPerson.json provee las posiciones de actor y objetivo (para python).
		 
		 *dataHeuristic.json provee los intentos y asi va guardando las mejores decisiones, 
		  este lo genera python y lo interpreta c#.
		*/

		// VARIABLES GLOBALES
		List<RectangleF> coordenadas = new List<RectangleF>(); //GUARDARA INFORMACION DE LAS POSICIONES 
															   //DE CUBOS GENERADOS.

		List<RectangleF> pasosAbiertos = new List<RectangleF>(); //GUARDA POSICIONES DE PASOS ABIERTOS PARA
																 //GENERAR LOS PUNTOS.

		List<metadatosCuadricula> posicionesCuad = new List<metadatosCuadricula>(); //GUARDA LOS DATOS PARA JSON.

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

			int ren = 0;
			int col = 0;
			bool caminodata = false;

			for (int i = 0; i < coordenadas.Count; i++)
			{
				des = random.Next(0, 10);
				if (des > 1) // MAS ALTO = MAS OBSTACULOS
				{
					caminodata = true;
					pasosAbiertos.Add(coordenadas.ElementAt(i));
					e.Graphics.FillRectangle(paso, coordenadas.ElementAt(i));
				}
				else
				{
					caminodata = false;
					e.Graphics.FillRectangle(obstaculo, coordenadas.ElementAt(i));
				}

		//------------ GENERACION DE CUADRICULA SIMPLE A PYTHON ----------------

				if (col < 10)
				{
					posicionesCuad.Add(new metadatosCuadricula
					{
						posRenglon = ren,
						posColumna = col,
						camino = caminodata
					}) ;
				}
				else
				{
					col = 0;
					ren++;

					posicionesCuad.Add(new metadatosCuadricula
					{
						posRenglon = ren,
						posColumna = col,
						camino = caminodata
					});
				}
				col++;
			}

			string fileName = Path.GetDirectoryName(Application.StartupPath) + "/dataArray.json";
			var jsonData = JsonConvert.SerializeObject(posicionesCuad);
			File.WriteAllText(fileName, jsonData); // ESCRITURA dataArray

			generaPuntos(e);

		}


		// generaPuntos SE ENCARGA DE GENERAR LOS PUNTOS DE ACTOR Y OBJETIVO 
		// MEDIANTE EL LIST DE "PASOS ABIERTOS" QUE ES EL QUE TIENE COORDENADAS DE PASO
		private void generaPuntos(PaintEventArgs e)
		{
			Random random = new Random();

			int pos1 = random.Next(0, pasosAbiertos.Count / 2);
			int pos2 = random.Next(pasosAbiertos.Count / 2, pasosAbiertos.Count);

			SolidBrush actor = new SolidBrush(Color.FromArgb(150, 200, 100, 150));
			SolidBrush objetivo = new SolidBrush(Color.FromArgb(150, 20, 20, 100));

			e.Graphics.FillEllipse(objetivo, 
				new RectangleF(new PointF(pasosAbiertos.ElementAt(pos1).X+30, pasosAbiertos.ElementAt(pos1).Y+5),
				new SizeF(pasosAbiertos.ElementAt(pos1).Width-30, pasosAbiertos.ElementAt(pos1).Height-10)));

			e.Graphics.FillEllipse(actor,
				new RectangleF(new PointF(pasosAbiertos.ElementAt(pos2).X + 30, pasosAbiertos.ElementAt(pos2).Y + 5),
				new SizeF(pasosAbiertos.ElementAt(pos2).Width-30, pasosAbiertos.ElementAt(pos2).Height-10)));


			// posicion 0 = objetivo / posicion 1 = actor

			List<metadatosCuadricula> posiciones = new List<metadatosCuadricula>();

			posiciones.Add(encuentraPos(pos1));
			posiciones.Add(encuentraPos(pos2));

			string fileName = Path.GetDirectoryName(Application.StartupPath) + "/dataPerson.json";
			var jsonData = JsonConvert.SerializeObject(posiciones);
			File.WriteAllText(fileName, jsonData); // ESCRITURA dataPerson



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
				posRenglon = posicionesCuad.ElementAt(posicionEnPlano).posRenglon,
				posColumna = posicionesCuad.ElementAt(posicionEnPlano).posColumna,
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
			this.Refresh();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			//
		}

		private void button3_Click(object sender, EventArgs e)
		{

		}
	}

	public class metadatosCuadricula
	{
		public int posRenglon  { get; set; }
		public int posColumna { get; set; }
		public bool camino { get; set; }
	}

}
