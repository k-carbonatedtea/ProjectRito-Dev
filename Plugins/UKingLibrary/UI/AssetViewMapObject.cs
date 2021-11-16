﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using MapStudio.UI;
using ImGuiNET;
using Toolbox.Core;
using CafeLibrary.Rendering;
using OpenTK.Graphics;
using OpenTK;

namespace UKingLibrary.UI
{
    /// <summary>
    /// Represemts an asset view of map objects to preview, drag and drop objects into the scene.
    /// </summary>
    public class AssetViewMapObject : IAssetCategory
    {
        public string Name => "Map Objects";

        public bool IsFilterMode => filterSkybox || filterObjPath;

        public static bool filterSkybox = false;
        public static bool filterObjPath = false;

        public List<AssetItem> Reload()
        {
            List<AssetItem> assets = new List<AssetItem>();
            Dictionary<string, AssetFolder> folders = new Dictionary<string, AssetFolder>();

            var objectList = GlobalData.Actors.Values.ToList();

            assets.Clear();
            foreach (IDictionary<string, dynamic> obj in objectList.OrderBy(x => x["name"]))
            {
                if (!obj.ContainsKey("bfres") || !obj.ContainsKey("name"))
                    continue;

                string name = obj["name"];
                string bfres = obj["bfres"];
                string profile = obj["profile"];

                if (!profile.Contains("Enemy"))
                    continue;

                if (!folders.ContainsKey(profile)) {
                    folders.Add(profile, new AssetFolder(profile));
                }

                if (folders[profile].Items.Any(x => ((MapObjectAsset)x).BfresPath == bfres))
                    continue;

                folders[profile].Items.Add(new MapObjectAsset()
                {
                    BfresPath = bfres,
                    Name = $"{name}",
                    Tag = obj,
                });

                assets.Add(folders[profile].Items.Last());
            }

            var Thread3 = new Thread((ThreadStart)(() =>
            {
                foreach (var asset in assets)
                    asset.Background_LoadThumbnail();
            }));
            Thread3.Start();

            return assets;
        }

        public bool UpdateFilterList()
        {
            bool filterUpdate = false;
            return filterUpdate;
        }
    }

    public class AssetFolder : AssetItem
    {
        public List<AssetItem> Items = new List<AssetItem>();

        public AssetFolder(string folder)
        {
            Name = folder;
            Icon = IconManager.GetTextureIcon("FOLDER");
        }
    }

    public class MapObjectAsset : AssetItem
    {
        public string BfresPath { get; set; }

        private int icon = -1;
        public override int Icon 
        {
            get {
                if (icon == -1 && ImageSource != null)
                    icon = GLFrameworkEngine.GLTexture2D.FromBitmap(ImageSource).ID;

                return icon; 
            }
            set { icon = value; }
        }

        System.Drawing.Bitmap ImageSource;

        public override void Background_LoadThumbnail()
        {
            string modelpath = PluginConfig.GetContentPath($"Model\\{BfresPath}.sbfres");
            string texpath = PluginConfig.GetContentPath($"Model\\{BfresPath}.Tex.sbfres");

            if (File.Exists($"{modelpath}"))
            {
                int size = 128;

                //Seperate GL context
                GraphicsMode mode = new GraphicsMode(new ColorFormat(32), 24, 8, 4, new ColorFormat(32), 2, false);
                var window = new GameWindow(size, size, mode);
                var context = new GraphicsContext(mode, window.WindowInfo);
                context.MakeCurrent(window.WindowInfo);

                var render = new BfresRender(modelpath);
                ((BfresRender)render).OnRenderInitialized += delegate
                {
                    if (File.Exists(texpath) && ((BfresRender)render).Textures.Count == 0)
                        ((BfresRender)render).Textures = BfresLoader.GetTextures(texpath);
                };

                ImageSource = IconModelRenderer.CreateRender(context, new List<BfresRender>() { render }, size, size)[0];
                render.Dispose();
            }
        }
    }
}
