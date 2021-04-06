import socket
import struct
import abc
import threading
from datetime import datetime, timedelta
from collections import namedtuple, deque
from enum import Enum
import numpy as np
import cv2
import sys
import time
from app.yolov4 import *
import logging
import json

np.warnings.filterwarnings('ignore')

VIDEO_STREAM_HEADER_FORMAT = "@qIIII18f"

VIDEO_FRAME_STREAM_HEADER = namedtuple(
    'SensorFrameStreamHeader',
    'Timestamp ImageWidth ImageHeight PixelStride RowStride fx fy '
    'PVtoWorldtransformM11 PVtoWorldtransformM12 PVtoWorldtransformM13 PVtoWorldtransformM14 '
    'PVtoWorldtransformM21 PVtoWorldtransformM22 PVtoWorldtransformM23 PVtoWorldtransformM24 '
    'PVtoWorldtransformM31 PVtoWorldtransformM32 PVtoWorldtransformM33 PVtoWorldtransformM34 '
    'PVtoWorldtransformM41 PVtoWorldtransformM42 PVtoWorldtransformM43 PVtoWorldtransformM44 '
)

VIDEO_STREAM_PORT = 23940

HOST = '192.168.43.21'

class FrameReceiverThread(threading.Thread):
    def __init__(self, host, port, header_format, header_data):
        super(FrameReceiverThread, self).__init__()
        self.header_size = struct.calcsize(header_format)
        self.header_format = header_format
        self.header_data = header_data
        self.host = host
        self.port = port
        self.latest_frame = None
        self.latest_header = None
        self.socket = None

    def get_data_from_socket(self):
        reply = self.recvall(self.header_size)

        if not reply:
            print('ERROR: Failed to receive data from stream.')
            return

        data = struct.unpack(self.header_format, reply)
        header = self.header_data(*data)

        image_size_bytes = header.ImageHeight * header.RowStride
        image_data = self.recvall(image_size_bytes)

        return header, image_data

    def recvall(self, size):
        msg = bytes()
        while len(msg) < size:
            part = self.socket.recv(size - len(msg))
            if part == '':
                break  # the connection is closed
            msg += part
        return msg

    def start_socket(self):
        self.socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.socket.connect((self.host, self.port))
        # send_message(self.socket, b'socket connected at ')
        print('INFO: Socket connected to ' + self.host + ' on port ' + str(self.port))

    def start_listen(self):
        t = threading.Thread(target=self.listen)
        t.start()

    @abc.abstractmethod
    def listen(self):
        return

    @abc.abstractmethod
    def get_mat_from_header(self, header):
        return


class VideoReceiverThread(FrameReceiverThread):
    def __init__(self, host):
        super().__init__(host, VIDEO_STREAM_PORT, VIDEO_STREAM_HEADER_FORMAT,
                         VIDEO_FRAME_STREAM_HEADER)

    def listen(self):
        while True:
            self.latest_header, image_data = self.get_data_from_socket()
            self.latest_frame = np.frombuffer(image_data, dtype=np.uint8).reshape((self.latest_header.ImageHeight,
                                                                                   self.latest_header.ImageWidth,
                                                                                   self.latest_header.PixelStride))

    def get_mat_from_header(self, header):
        pv_to_world_transform = np.array(header[7:24]).reshape((4, 4)).T
        return pv_to_world_transform
	
	
def detection(frame):
    sized = cv2.resize(frame,(darknet.width,darknet.height))
    sized = cv2.cvtColor(sized, cv2.COLOR_BGR2RGB)
	
    boxes = do_detect(darknet, sized, 0.5, 0.4, USE_CUDA)
    boxes = np.array(boxes[0]).tolist()
	
    result_img = plot_boxes_cv2(video_receiver.latest_frame, boxes, class_names = class_names)
	
    for i in range(len(boxes)):
        boxes[i][6] = class_names[int(boxes[i][6])]
	
    formatBoxes = []
    for i in range(len(boxes)):
        formatBoxes.append({
            'X1': boxes[i][0],
            'Y1': boxes[i][1],
            'X2': boxes[i][2],
            'Y2': boxes[i][3],
            'Unknown': boxes[i][4],
            'Conf': boxes[i][5],
            'Name': boxes[i][6]
            })
    print(formatBoxes) 
    formatBoxes = json.dumps(formatBoxes)
		
    return result_img, formatBoxes

    

if __name__ == '__main__':
	
    #video_receiver.socket.send(b'test')

    video_receiver = VideoReceiverThread(HOST)
    video_receiver.start_socket()
    video_receiver.start_listen()

    namesfile = 'data/coco.names'
    class_names = load_class_names(namesfile)

    s = socket.socket()
    s.bind(('0.0.0.0', 5000))
    #s.bind(('127.0.0.1', 5000))
    s.listen()
    c, addr = s.accept()
    #c.send(b'test')
    #print("well")


    while True:
        if np.any(video_receiver.latest_frame) :
            result_img, formatBoxes = detection(video_receiver.latest_frame)
            c.send(formatBoxes.encode('utf8'))
            #c.send(b'test')
            #print("fine")
            cv2.imshow('Result', result_img)

        if cv2.waitKey(1) & 0xFF == ord('q'):
            break




