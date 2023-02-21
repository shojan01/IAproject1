import json
import os

directory = os.path.dirname(__file__)

pos = str(directory)


# FALTA HACER DINAMICAS LAS DIRECCIONES
data = open('C://Users/mjoha/source/repos/appIATablero/appIATablero/bin/dataArray.json')
person = open('C://Users/mjoha/source/repos/appIATablero/appIATablero/bin/dataPerson.json')

lista1 = json.load(data)
lista2 = json.load(person)

AuxX = []
AuxY = []
camino = []

# posicion 0 = objetivo / posicion 1 = actor
AuxXp = []
AuxYp = []

# LECTURA DE DATOS
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

#-------- ANALISIS PRUEBA ---------

#POSICION INICIAL DEL ACTOR
posTempx=AuxXp[1]
posTempy=AuxYp[1]

#LISTA DE MOVIMIENTOS VALIDOS EN POSICION
movValidx = [1,1,1,0,-1,-1,-1,0]
movValidy = [1,0,-1,-1,-1,0,1,1]
intento = 0

#ALGORITMO
while (posTempx != AuxXp[0] & posTempy != AuxYp[0]):

    ## FALTA VALIDACION
    posTempx = posTempx + movValidx[intento]
    posTempy = posTempy + movValidy[intento]

    #POSICIONES 





print("ANALISIS COMPLETADO")