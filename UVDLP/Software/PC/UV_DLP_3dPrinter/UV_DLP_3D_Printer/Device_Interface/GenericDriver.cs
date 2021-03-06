﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UV_DLP_3D_Printer.Drivers
{
    public class GenericDriver : DeviceDriver
    {
        public GenericDriver() 
        {
            m_drivertype = eDriverType.eGENERIC;
        }
        public override bool Connect() 
        {
            try
            {
                m_serialport.Open();
                if (m_serialport.IsOpen)
                {
                    m_connected = true;
                    RaiseDeviceStatus(this, eDeviceStatus.eConnect);
                    return true;
                }
            }catch(Exception ex)
            {
                DebugLogger.Instance().LogRecord(ex.Message);
            }
            return false;
        }
        public override bool Disconnect() 
        {
            try
            {
                m_serialport.Close();
                m_connected = false;
                RaiseDeviceStatus(this, eDeviceStatus.eDisconnect);
                return true;
            }
            catch (Exception ex) 
            {
                DebugLogger.Instance().LogRecord(ex.Message);
                return false;
            }
            
        }
        private readonly object _locker = new object();
        
        public override int Write(byte[] data, int len) 
        {
            lock (_locker)
            {
                m_serialport.Write(data, 0, len);
                return len;
            }
        }

        private string RemoveComment(string line) 
        {
           
            string newln = "";
            // this function removes the comments from the line
            line = line.Trim(); // trim off any whitespace
            if (line.StartsWith("("))
            {
                return "";
            }
            if (line.StartsWith(";"))
            {
                return "";
            }
            // now replace the ';' with '('
            line = line.Replace(';','(');
            if (line.Contains('('))
            {
                // this line does not start with a comment, but contains a comment,
                // split the line and give only the first portion
                String[] Lines = line.Split('(');
                if (Lines.Length > 0)
                {
                    newln = Lines[0];
                    newln += "\r\n"; // make sure to cap it off               
                }
                else
                {
                    //wtf here?
                    DebugLogger.Instance().LogError("Should be a line here....");
                }
            }
            else 
            {
                //line contains no comments
                newln = line + "\r\n";
            }
            return newln;
        }

        public override int Write(String line) 
        {
            lock (_locker) // ensure synchronization
            {
                line = RemoveComment(line);
                if (line.Trim().Length > 0)
                {
                    m_serialport.Write(line);
                    
                }
                return line.Trim().Length;
            }
        }
    }
}
