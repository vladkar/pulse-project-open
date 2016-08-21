# -*- coding: cp1251 -*-
# Author: Sergey Ivanov (ITMO University)

"""
 - Моделирование процесса распространения инфекции на основе системы ДУ
 """
# ________ Tiff Model ________
# S - Suceptible (восприимчивые к болезни)
# I - Infected (инфицированные с признаками болезни)
# R - Recovered (выздоровевшие)
# D - Death - умершие
# ---- ФОРМА УРАВНЕНИЙ ----
# dS/dt  = -k1*S*I                <--  текущее здоровых
# dI/dt =  +k1*S*I - k2*I - k3*I  <--  текущее инфицированных
# dR/dt  = +k2*I                  <--- ВЫЗДОРОВЕЛО ВСЕГО (ТОЛЬКО РОСТ)
# dD/dt  = +k3*I                  <--- УМЕРЛО ВСЕГО (ТОЛЬКО РОСТ)

from numpy import *
import pylab as p
from itertools import accumulate
from matplotlib.font_manager import FontProperties
from scipy.stats import binom
from msvcrt import getch
from matplotlib import rc
from itertools import cycle
import sys,os,itertools,shutil,fnmatch,time

lines = ["-","--","-.",":"]
linecycler = cycle(lines)
font = {'family': 'Verdana','weight': 'normal'}
rc('font', **font)

# "inf_rate": "0.3","recov_rate": "0.1","death_rate": "0.01"
k1,k2,k3 = [0.3, 0.1, 0.01 ]
N = 120

# k1 = 0.5    # S -> I
# k2 = 0.25   # I -> R
# k3 = 0.3    # I -> D

# ---- данные ----
def ShowFluStat(I,R,D):    
    p.figure()       
    p.plot(range(len(I)), I, "-",label='Infected',linewidth=3.0)        
    p.plot(range(len(R)), R, "--",label='Recovered',linewidth=2.0)
    p.plot(range(len(D)), D, "--",label='Dead',linewidth=2.0)    

    p.legend(loc='best',fancybox=True, shadow=True)
    p.ylabel('infection incidence')
    p.xlabel('days')    
    p.grid()
    p.savefig("infection.png")
    p.show()
    p.close()

    #DefaultSize = p.get_size_inches()
    #p.set_size_inches( (DefaultSize[0]*2, DefaultSize[1]*2) )

# ________ общая процедура моделирования ________
def MakeSimulation(K,N,SCALE=1.0):
    # ________ ДУ ________
    inInf,k1,k2,k3 = K    
    t =  linspace(0, N-1, N)
    def dX_dt(X, t=0): # Return parameters of the infected populations.
        S,I,R,D = X 
        return array([-k1*S*I,
                      +k1*S*I - k2*I - k3*I,
                      +k2*I,
                      +k3*I])
    # ________ интегрируем ДУ ________
    from scipy import integrate    
    # переделать на zeros
    X0 = array([1.-inInf, inInf, 0.0,0.0])  # initials conditions: S,I,R,D
    X,infodict = integrate.odeint(dX_dt, X0, t, full_output=True)
    S,I,R,D = X.T
    return (I*SCALE,R*SCALE,D*SCALE) # инфицированные + умершие

# --- отрезать лишние значения и произвести перенормировку---
K1=[0.1,k1,k2,k3]
#INF = MakeSimulation(K1,N)
#ShowFluStat(INF[0],INF[1],INF[2])