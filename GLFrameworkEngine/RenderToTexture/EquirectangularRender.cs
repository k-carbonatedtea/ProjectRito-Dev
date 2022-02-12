﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public class EquirectangularRender
    {
        static Dictionary<string, GLTexture2D> cubemapCache = new Dictionary<string, GLTexture2D>();

        public static GLTexture2D CreateTextureRender(GLTexture texture, int arrayLevel, int mipLevel, bool force = false)
        {
            if (!force && cubemapCache.ContainsKey(texture.ID.ToString()))
                return cubemapCache[texture.ID.ToString()];
            else
            {
                if (cubemapCache.ContainsKey(texture.ID.ToString()))
                    cubemapCache[texture.ID.ToString()]?.Dispose();
            }

            int width = 512;
            int height = 256;

            var shader = GlobalShaders.GetShader("EQUIRECTANGULAR");
            var textureOutput = GLTexture2D.CreateUncompressedTexture(width, height, PixelInternalFormat.Rgba32f);
            textureOutput.MipCount = texture.MipCount;

            texture.Bind();

            textureOutput.Bind();
            textureOutput.GenerateMipmaps();
            textureOutput.Unbind();

            Framebuffer frameBuffer = new Framebuffer(FramebufferTarget.Framebuffer, width, height);
            frameBuffer.Bind();

            GLL.Disable(EnableCap.Blend);

            shader.Enable();
            shader.SetBoolToInt("is_array", texture is GLTextureCubeArray);

            if (texture is GLTextureCubeArray)
            {
                GLL.ActiveTexture(TextureUnit.Texture1);
                texture.Bind();
                shader.SetInt("dynamic_texture_array", 1);
            }
            else
            {
                GLL.ActiveTexture(TextureUnit.Texture1);
                texture.Bind();
                shader.SetInt("dynamic_texture", 1);
            }

            for (int i = 0; i < textureOutput.MipCount; i++)
            {
                int mipWidth = (int)(width * Math.Pow(0.5, i));
                int mipHeight = (int)(height * Math.Pow(0.5, i));
                frameBuffer.Resize(mipWidth, mipHeight);

                GLL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                    textureOutput.ID, i);

                shader.SetInt("arrayLevel", arrayLevel);
                shader.SetInt("mipLevel", mipLevel);

                GLL.ClearColor(0, 0, 0, 0);
                GLL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GLL.Viewport(0, 0, mipWidth, mipHeight);

                //Draw the texture onto the framebuffer
                ScreenQuadRender.Draw();

                break;
            }

            if (cubemapCache.ContainsKey(texture.ID.ToString()))
                cubemapCache[texture.ID.ToString()] = textureOutput;
            else
                cubemapCache.Add(texture.ID.ToString(), textureOutput);
            return textureOutput;
        }
    }
}
