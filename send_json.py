import socket
import threading
import sys
import time
import logging
import json	

if __name__ == '__main__':

    s = socket.socket()
    #s.bind(('0.0.0.0', 5000))
    s.bind(('127.0.0.1', 5000))
    s.listen()
    c, addr = s.accept()
    #c.send(b'test')
    #print("well")

    #formatBoxes = [{'X1':0.47328507900238037,'Y1':0.0011004656553268433,'X2':0.7134413719177246,'Y2':0.25107067823410034,'Unknown':0.6047452688217163,'Conf':0.6047452688217163,'Name':'person'},{'X1':0.7134413719177246,'Y1':0.0011004656553268433,'X2':0.3456234567891098,'Y2':0.74836485763869015,'Unknown':0.8563732736648573,'Conf':0.8563732736648573,'Name':'cup'}]
    formatBoxes = [{'X1':0.47328507900238037,'Y1':0.0011004656553268433,'X2':0.7134413719177246,'Y2':0.25107067823410034,'Unknown':0.6047452688217163,'Conf':0.6047452688217163,'Name':'person'}]
    #print(type(formatBoxes))
    #print(formatBoxes)
    formatBoxes = json.dumps(formatBoxes)
    #print(type(formatBoxes))
    #print(formatBoxes)
    c.send(formatBoxes.encode('utf8'))
    print("ok")