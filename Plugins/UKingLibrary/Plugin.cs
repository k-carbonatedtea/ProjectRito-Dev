﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core;
using GLFrameworkEngine;

namespace UKingLibrary
{
    public class Plugin : IPlugin
    {
        public string Name => "BOTW Map Editor";

        public Plugin()
        {
            //Add global shaders
            GlobalShaders.AddShader("TERRAIN", "Terrain");
            GlobalShaders.AddShader("WATER", "Water");
            GlobalShaders.AddShader("GRASS", "Grass");

            //Load plugin specific data. This is where the game path is stored.
            if (!PluginConfig.init)
                PluginConfig.Load();

            if (PluginConfig.FirstStartup)
            {
                ActorDocs.Update();

                PluginConfig.FirstStartup = false;
                new PluginConfig().Save();
            } // Get our ActorDocs!
                
        }
    }
}