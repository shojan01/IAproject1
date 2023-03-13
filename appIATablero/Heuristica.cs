using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace appIATablero
{
    class Heuristica
    {
        private List<Nodo> openSet;
        private List<Nodo> closedSet;
        private Nodo startNode;
        private Nodo endNode;
        private int mapWidth;
        private int mapHeight;
        private Nodo[,] nodos;
        private List<metadatosCuadricula> meta;

        //------- Variables publicas para muestreo de datos -------

        public List<float> distancias = new List<float>();
        public List<Nodo> vecinos = new List<Nodo>();
        public List<float> costos = new List<float>();
        public List<Nodo> nodoActual = new List<Nodo>();


        // EL CONSTRUCTOR REQUIERE res = numero de cuadricula,
        // metadatos = Lista de metadatos de posicion que indica si el camino es abierto o cerrado
        // X y Y de inicio, X y Y de objetivo
        public Heuristica(int res, List<metadatosCuadricula> metadatos,int startX, int startY, int endX, int endY)
        {
            mapWidth = res;
            mapHeight = res;
            nodos = new Nodo[mapWidth, mapHeight];
            meta = metadatos;

            //guarda los datos
            int data = 0;
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    nodos[x, y] = new Nodo(x, y,  meta.ElementAt(data).getCamino() == 1);
                    data++;
                }
            }

            // crea la lista abierta y una cerrada
            startNode = nodos[startX, startY];
            endNode = nodos[endX, endY];

            openSet = new List<Nodo>();
            closedSet = new List<Nodo>();

            openSet.Add(startNode);

        }

        // GetNeighbours Lanza una lista de nodos que indica los vecinos en tal posicion de nodo
        public List<Nodo> GetNeighbours(Nodo node)
        {
            List<Nodo> neighbours = new List<Nodo>();

            int[][] directions = {
            new int[] {0, -1}, // Arriba
            new int[] {0, 1},  // Abajo
            new int[] {-1, 0}, // Izquierda
            new int[] {1, 0}   // Derecha
        };

            foreach (int[] direction in directions)
            {
                int checkX = node.x + direction[0];
                int checkY = node.y + direction[1];

                if (checkX >= 0 && checkX < mapWidth && checkY >= 0 && checkY < mapHeight)
                {
                    neighbours.Add(nodos[checkX, checkY]);
                }
            }
            return neighbours;
        }

        // GetDistance retorna el valor que tiene un nodo y otro
        public int GetDistance(Nodo nodeA, Nodo nodeB)
        {
            int dstX = Math.Abs(nodeA.x - nodeB.x);
            int dstY = Math.Abs(nodeA.y - nodeB.y);

            if (dstX > dstY)
            {
                return 14 * dstY + 10 * (dstX - dstY);
            }
            return 14 * dstX + 10 * (dstY - dstX);
        }

        // GetLowestFNode se encarga de regresar el nodo que cueste menos
        public Nodo GetLowestFNode()
        {
            Nodo lowestFNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].f < lowestFNode.f)
                {
                    lowestFNode = openSet[i];
                }
            }
            return lowestFNode;
        }

        // FindPath utiliza el algoritmo a* para trazar un camino optimo de nodo inicial a meta
        public List<Nodo> FindPath()
        {
            while (openSet.Count > 0)
            {

                // ------ INDICA UN NUEVO MOVIMIENTO EN LAS LISTAS ---------
                nodoActual.Add(new Nodo(0,0,false));
                costos.Add(0);
                distancias.Add(0);
                vecinos.Add(new Nodo(0, 0, false));
                // ---------------------------------------------------------


                Nodo currentNode = GetLowestFNode(); //nodo actual = nodo que cueste menos
                nodoActual.Add(currentNode);

                // si nodo actual es igual al de la meta, termina y regresa la lista generada
                if (currentNode == endNode)
                {
                    List<Nodo> path = new List<Nodo>();
                    Nodo node = currentNode;

                    while (node != startNode)
                    {
                        path.Add(node);
                        node = node.padre;
                    }

                    path.Reverse();
                    return path;
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                // Realiza la busqueda en base Nodos vecinos del nodo actual
                foreach (Nodo neighbourNode in GetNeighbours(currentNode))
                {
                    // si el nodo vecino es un obstaculo, omite todo el recorrido
                    if (neighbourNode.obstaculo || closedSet.Contains(neighbourNode))
                    {
                        continue;
                    }

                    // calcula el nuevo costo
                    int newMovementCostToNeighbour = currentNode.g + GetDistance(currentNode, neighbourNode);

                    //Realiza el calculo de costo y distancia optima en movimiento
                    if (newMovementCostToNeighbour < neighbourNode.g || !openSet.Contains(neighbourNode))
                    {
                        neighbourNode.g = newMovementCostToNeighbour;
                        neighbourNode.h = GetDistance(neighbourNode, endNode);
                        neighbourNode.padre = currentNode;

                        costos.Add(newMovementCostToNeighbour);
                        distancias.Add(GetDistance(neighbourNode, endNode));

                        if (!openSet.Contains(neighbourNode))
                        {
                            openSet.Add(neighbourNode);
                            vecinos.Add(neighbourNode);
                        }
                    }
                }
            }

            return null;
        }
    }

}

