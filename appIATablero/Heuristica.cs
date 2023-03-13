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


        public Heuristica(int res, List<metadatosCuadricula> metadatos,int startX, int startY, int endX, int endY)
        {
            mapWidth = res;
            mapHeight = res;
            nodos = new Nodo[mapWidth, mapHeight];
            meta = metadatos;

            int data = 0;
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    nodos[x, y] = new Nodo(x, y,  meta.ElementAt(data).getCamino() == 1);
                    data++;
                }
            }

            startNode = nodos[startX, startY];
            endNode = nodos[endX, endY];

            openSet = new List<Nodo>();
            closedSet = new List<Nodo>();

            openSet.Add(startNode);

        }

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


                Nodo currentNode = GetLowestFNode();
                nodoActual.Add(currentNode);

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

                foreach (Nodo neighbourNode in GetNeighbours(currentNode))
                {
                    if (neighbourNode.obstaculo || closedSet.Contains(neighbourNode))
                    {
                        continue;
                    }

                    int newMovementCostToNeighbour = currentNode.g + GetDistance(currentNode, neighbourNode);

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

