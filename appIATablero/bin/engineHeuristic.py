import json
import cmath

#region CLASE NODO
#       CLASE PARA LOS NODOS

class Nodo:
    def __init__(self, x, y):
        self.x = x
        self.y = y

# ------------------------------
#endregion

#region CLASE HEURISTIC
class heuristic:
    actual = []
    puntaje = 0
    mejorPos = 0
    posicion = 0

    def __init__(self, actual, puntaje):
        self.actual = actual
        self.puntaje = puntaje

        if len(actual) > 0:
            self.mejorPos = puntaje + actual[0]

            for valor in actual:
                if(self.actual[self.posicion] + puntaje < self.mejorPos):
                    self.mejorPos = valor
                self.posicion = self.posicion + 1

        
    def analisis(self):
        return self.mejorPos
    
    def getPosicionNodoOptimo(self):
        return self.posicion

    def borraMemoria(self):
        self.actual = []
        self.puntaje = 0
        self.mejorPos = 0
        self.posicion = 0



    
#endregion
                
#region DIRECCIONES Y CARGA DE FICHEROS DE C#
#                           DIRECCIONES Y CARGA DE FICHEROS DE C#

data = open('C://Users/mjoha/source/repos/appIATablero/appIATablero/bin/dataArray.json')
person = open('C://Users/mjoha/source/repos/appIATablero/appIATablero/bin/dataPerson.json')
lista1 = json.load(data)
lista2 = json.load(person)

AuxX = []
AuxY = []
camino = []

# posicion 0 = objetivo / posicion 1 = inteligencia
AuxXp = []
AuxYp = []

# LECTURA DE DATOS TRAIDOS DE C#
for i in lista1:
    x1 = str(i).split(':')
    y1 = x1[1].split(',')
    z1 = y1[0]
    AuxX.append(z1)
    y2 = x1[2].split(',')
    z2 = y2[0]
    AuxY.append(z2)
    ca = x1[3]
    camino.append(ca[1])

for i in lista2:
    x1 = str(i).split(':')
    y1 = x1[1].split(',')
    z1 = y1[0]
    AuxXp.append(z1)
    y2 = x1[2].split(',')
    z2 = y2[0]
    AuxYp.append(z2)

inicio = Nodo(int(AuxXp[1]),int(AuxYp[1]))
objetivo = Nodo(int(AuxXp[0]),int(AuxYp[0]))

# ----------------------------------------------------------------------------------------
#endregion

#region SALIDA DE PRUEBA
print('--------- ANALISIS DE PUNTOS -----------')
print('NODO INICIAL: ',inicio.x,' ',inicio.y)
print('NODO OBJETIVO: ',objetivo.x,' ',objetivo.y,end='\n\n')
#endregion

#region ALGORITMO A*
# PROCESO DE BUSQUEDA ALGORITMO A*

#LISTA DE MOVIMIENTOS VALIDOS EN POSICION para calificar
movValidx = [1,0,-1,0] 
movValidy = [0,-1,0,1]

nodoActual = inicio
puntaje = []
nodosResult = []
camino = []

g = 0;
encontrado = False

while encontrado == False:

    #POSICIONES
    if(nodoActual == objetivo):
        encontrado = True
        print('Lo encontre')

    g = g + 1
    for intento in [0,1,2,3]:
        puntajeInit = cmath.sqrt(pow((nodoActual.x+movValidx[intento])-objetivo.x,2)+pow((nodoActual.y+movValidy[intento])-objetivo.y,2))
        nodosResult.append(puntajeInit.real)
        print(puntajeInit.real)

    nuevoNodo = heuristic(nodosResult, g)
    for nodo in nodosResult:
        if nuevoNodo.analisis() == nodo:
            print('SE ELIGIO',nuevoNodo.analisis())
            nodoActual.x = nodoActual.x+movValidx[nuevoNodo.getPosicionNodoOptimo()-1]
            nodoActual.y = nodoActual.y+movValidy[nuevoNodo.getPosicionNodoOptimo()-1]
            print('SE ELIGIO',nodoActual.x,' ',nodoActual.y)
            camino.append(nodoActual)
            #nuevoNodo.borraMemoria()
            nodosResult.clear()

    if(g == 10):
        encontrado = True
    print('----------------')

        
     

#endregion
 

    