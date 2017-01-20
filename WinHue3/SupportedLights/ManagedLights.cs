﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using Newtonsoft.Json;

namespace WinHue3
{
    public static class ManagedLights
    {
        public static Dictionary<string, Supportedlight> listAvailableLights;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly Supportedlight UnsupportedLight = new Supportedlight()
        {
            Img = new Dictionary<string, ImageSource>()
            {
                {"on", GDIManager.CreateImageSourceFromImage(Properties.Resources.DefaultLight_on)},
                {"off", GDIManager.CreateImageSourceFromImage(Properties.Resources.DefaultLight_off)},
                {"unr", GDIManager.CreateImageSourceFromImage(Properties.Resources.DefaultLight_unr)},
            },
            Bri = new MaxMin<byte>(0, 0),
            Sat = new MaxMin<byte>(0, 0),
            Ct = new MaxMin<ushort>(153, 153),
            Hue = new MaxMin<ushort>(0, 0),             
        };

        public static void LoadSupportedLights()
        {
            try
            {
                listAvailableLights = new Dictionary<string, Supportedlight>()
                {
                    {"Unsupported", UnsupportedLight},
                };

                string[] listlightsfiles = Directory.GetFiles("lights","*.lt");

                foreach (string file in listlightsfiles)
                {
                    string json = string.Empty;
                    try
                    {
                        TextReader reader = File.OpenText(file);
                        json = reader.ReadToEnd();
                        reader.Close();
                    }
                    catch (Exception ex)
                    { 
                        log.Error($"Error reading file : {file}.",ex);  
                    }

                    try
                    {
                        Supportedlight sl = JsonConvert.DeserializeObject<Supportedlight>(json,new SupportedLightConverter(typeof (Supportedlight)));
                        log.Info($"Loaded supported light : {sl.Name}, modelid : {sl.Modelid}, type : {sl.Type}");
                        listAvailableLights.Add(sl.Modelid, sl);
                    }
                    catch (OverflowException ex)
                    {
                        log.Error($"Invalid max or min in file : {file}. Make sure the max and min are within allowed values.",ex);
                    }
                    catch (ArgumentNullException ex)
                    {
                        log.Error($"Missing tag in file : {file}. Make sure all the xml tags are present in the file.",ex);
                    }
                    catch (Exception ex)
                    {
                        log.Error($"Error reading file : {file}. Make sure the json is properly formatted.",ex);
                    }             
                }

            }
            catch (Exception ex)
            {
                log.Error("Error reading supported lights.",ex);
            }

        }

    }
}
