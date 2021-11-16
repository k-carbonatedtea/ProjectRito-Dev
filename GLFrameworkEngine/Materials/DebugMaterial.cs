﻿using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core;

namespace GLFrameworkEngine
{
    public class DebugMaterial
    {
        public void Render(GLContext context)
        {
            context.CurrentShader = GlobalShaders.GetShader("DEBUG");

            var debugShader = context.CurrentShader;
            debugShader.SetInt("debugShading", (int)DebugShaderRender.DebugRendering);
            debugShader.SetInt("weightRampType", 2);
            debugShader.SetInt("selectedBoneIndex", Runtime.SelectedBoneIndex);

            int slot = 1;
            debugShader.SetTexture(RenderTools.uvTestPattern, "UVTestPattern", slot++);
            debugShader.SetTexture(RenderTools.boneWeightGradient, "weightRamp1", slot++);
            debugShader.SetTexture(RenderTools.boneWeightGradient2, "weightRamp2", slot++);

            var viewProjMatInv = context.Camera.ViewProjectionMatrix.Inverted();
            var lightVP = context.Scene.ShadowRenderer.GetLightSpaceViewProjMatrix();
            var shadowMap = context.Scene.ShadowRenderer.GetProjectedShadow();
            var lightDir = context.Scene.ShadowRenderer.GetLightDirection();

            debugShader.SetVector3("lightDir", lightDir);
            debugShader.SetMatrix4x4("mtxViewProjInv", ref viewProjMatInv);
            debugShader.SetMatrix4x4("mtxLightVP", ref lightVP);
            debugShader.SetTexture(shadowMap, "shadowMap", slot++);
        }
    }
}
