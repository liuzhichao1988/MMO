using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace neil.mmo.net.utility{
    
    public static class MessageUtility
    {
        static bool c_Is_Big_Endian = false;

        public static int ReadIntFromBuffer(byte[] buffer, int offset)
        {
            int value = 0;
            if(c_Is_Big_Endian)
            {
                value = value | (buffer[offset++] << 24);
                value = value | (buffer[offset++] << 16);
                value = value | (buffer[offset++] << 8);
                value = value | (buffer[offset++]);
            }
            else
            {
                value = value | (buffer[offset++]);
                value = value | (buffer[offset++] << 8);
                value = value | (buffer[offset++] << 16);
                value = value | (buffer[offset++] << 24);
            }
            return value;
        }

        public static uint ReadUIntFromBuffer(byte[] buffer, int offset)
        {
            int value = ReadIntFromBuffer(buffer, offset);
            return (uint)value;
        }

        public static byte ReadByteFromBuffer(byte[] buffer, int offset)
        {
            return buffer[offset];
        }

        public static short ReadShortFromBuffer(byte[] buffer, int offset)
        {
            short value = 0;
            if(c_Is_Big_Endian)
            {
                value = (short)((int)value | (buffer[offset++] << 8));
                value = (short)((int)value | (buffer[offset++]));
            }
            else
            {
                value = (short)((int)value | (buffer[offset++]));
                value = (short)((int)value | (buffer[offset++] << 8));
            }
            return value;
        }

        public static char ReadCharFromBuffer(byte[] buffer, int offset)
        {
            return (char)buffer[offset];
        }

        public static int WriteIntToBuffer(byte[] buffer, int value, int offset)
        {
            if(c_Is_Big_Endian){
                buffer[offset++] = (byte)((value >> 24) & 255);
                buffer[offset++] = (byte)((value >> 16) & 255);
                buffer[offset++] = (byte)((value >> 8) & 255);
                buffer[offset++] = (byte)((value) & 255);
            }
            else{
                buffer[offset++] = (byte)((value) & 255);
                buffer[offset++] = (byte)((value >> 8) & 255);
                buffer[offset++] = (byte)((value >> 16) & 255);
                buffer[offset++] = (byte)((value >> 24) & 255);
            }
            return offset;
        }

        public static int WriteUIntToBuffer(byte[] buffer, uint value, int offset)
        {
            if (c_Is_Big_Endian)
            {
                buffer[offset++] = (byte)((value >> 24) & 255);
                buffer[offset++] = (byte)((value >> 16) & 255);
                buffer[offset++] = (byte)((value >> 8) & 255);
                buffer[offset++] = (byte)((value) & 255);
            }
            else
            {
                buffer[offset++] = (byte)((value) & 255);
                buffer[offset++] = (byte)((value >> 8) & 255);
                buffer[offset++] = (byte)((value >> 16) & 255);
                buffer[offset++] = (byte)((value >> 24) & 255);
            }
            return offset;
        }

        public static int WriteShortToBuffer(byte[] buffer, short value, int offset)
        {
            if(c_Is_Big_Endian)
            {
                buffer[offset++] = (byte)((value >> 8) & 255);
                buffer[offset++] = (byte)((value) & 255);
            }
            else
            {
                buffer[offset++] = (byte)((value) & 255);
                buffer[offset++] = (byte)((value >> 8) & 255);
            }
            return offset;
        }

        public static int WriteByteToBuffer(byte[] buffer, byte value, int offset)
        {
            buffer[offset++] = value;
            return offset;
        }

        public static int WriteCharToBuffer(byte[] buffer, char value, int offset)
        {
            buffer[offset++] = (byte)value;
            return offset;
        }
    }

}