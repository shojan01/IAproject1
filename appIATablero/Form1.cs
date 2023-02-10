using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace appIATablero
{
	public partial class Form1 : Form
	{

		// VARIABLES GLOBALES
		List<RectangleF> coordenadas = new List<RectangleF>(); //GUARDARA INFORMACION DE LAS POSICIONES 
															   //DE CUBOS GENERADOS

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
			new PointF (350.0F, 10.0F),
			new PointF (350.0F, 350.0F),
			new PointF (10.0F, 350.0F),
			new PointF(10.0F, 10.0F)
			};

			// TABLERO DIMESIONES A GENERAR OBSTACULOS
			// x10, y10					x350, y10
			//    ----------------------------
			//	  .							 .
			//	  .							 .
			//	  .							 .
			//	  .							 .
			//	  .							 .
			//    ----------------------------
			// x10, y350				x350, y350

			// GENERA CUADRICULA DE 8 x 8 

			float espaciado = (350 - 10) / 8; //PIXELES ENTRE DIVISION DE AREAS 350px
											  //-10 por el espacio que comienza a generar linea en el panel
			generaCuadricula(10, 350, espaciado);
			e.Graphics.DrawLines(pincel, tablero);
			generaObstaculos(e);

		}

		private void generaObstaculos(PaintEventArgs e)
		{
			Random random = new Random();
			int des;
			SolidBrush paso = new SolidBrush(Color.FromArgb(50, 0, 0, 0));
			SolidBrush obstaculo = new SolidBrush(Color.FromArgb(150, 0, 0, 0));

			for (int i = 0; i < coordenadas.Count; i++)
			{
				des = random.Next(0, 10) + 1;
				if (des > 3) // MAS ALTO = MAS OBSTACULOS
				{
					e.Graphics.FillRectangle(paso, coordenadas.ElementAt(i));
				}
				else
				{
					e.Graphics.FillRectangle(obstaculo, coordenadas.ElementAt(i));
					//falta metadatos para obstaculos (iran para python)
				}
			}

		}

		//GENERA CUADRICULA SE ENCARGA DE RECOPILAR POSICIONES DEL DIBUJO DIVIDIENDO EL TAMAÑO DE PIXELES
		//A PARTES PARA DIBUJAR
		private void generaCuadricula(float valIni, float valFin, float valEspa)
		{
			float valPointx = valIni;
			float valPointy = valIni;

			float valSizeH = valEspa; // HEIGHT
			float valSizeW = valEspa; // WIDTH

			for (int i = 0; i < 8; i++) 
			{
				for (int j = 0; j < 8; j++)
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
			this.Refresh();  
		}

		private void button2_Click(object sender, EventArgs e)
		{

		}
	}
}
