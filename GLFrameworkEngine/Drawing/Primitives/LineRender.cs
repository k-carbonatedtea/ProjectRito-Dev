﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public class LineRender : RenderMesh<LineRender.LineVertex>
    {
        public struct LineVertex
        {
            [RenderAttribute(GLConstants.VPosition, VertexAttribPointerType.Float, 0)]
            public Vector3 Position;

            [RenderAttribute(GLConstants.VColor, VertexAttribPointerType.Float, 12)]
            public Vector4 Color;

            public LineVertex(Vector3 position, Vector4 color)
            {
                Position = position;
                Color = color;
            }
        }

        private int length = 0;

        public LineRender(PrimitiveType type = PrimitiveType.Lines) : base(new LineVertex[0], type)
        {
        }

        public void Draw(Vector3 start, Vector3 end, Vector4 color, bool forceUpdate = false)
        {
            if (length == 0 || forceUpdate)
                UpdateVertexData(start, end, color);

            this.Draw(GLContext.ActiveContext);
        }

        public void Draw(Vector2 start, Vector2 end, Vector4 color, bool forceUpdate = false)
        {
            if (length == 0 || forceUpdate)
                UpdateVertexData(start, end, color);

            this.Draw(GLContext.ActiveContext);
        }

        public void Draw(List<Vector3> points, bool forceUpdate = false) {
            Draw(points, new List<Vector4>() { Vector4.One }, forceUpdate);
        }

        public void Draw(List<Vector3> points, List<Vector4> colors, bool forceUpdate = false)
        {
            if (length == 0 || forceUpdate)
                UpdateVertexData(points, colors);

            this.Draw(GLContext.ActiveContext);
        }


        void UpdateVertexData(Vector2 start, Vector2 end, Vector4 color) {
            UpdateVertexData(new Vector3(start.X, start.Y, 0), new Vector3(end.X, end.Y, 0), color);
        }

        void UpdateVertexData(Vector3 start, Vector3 end, Vector4 color) {
            UpdateVertexData(new List<Vector3>() { start, end }, new List<Vector4>() { color, color });
        }

        void UpdateVertexData(List<Vector3> points, List<Vector4> colors)
        {
            List<LineVertex> list = new List<LineVertex>();
            for (int i = 0; i < points.Count; i++)
            {
                Vector4 color = new Vector4(1);
                if (colors.Count > i)
                    color = colors[i];
                list.Add(new LineVertex(points[i], color));
            }
            LineVertex[] data = list.ToArray();
            this.UpdateVertexData(data);

            length = data.Length;
        }
    }
}
