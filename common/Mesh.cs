using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace common
{
    public class Mesh
    {
        private readonly float[] vertices;
        private readonly uint[] indices;

        private List<Texture> textures = new();

        private int VAO, VBO, EBO;
        private float rotation = 0.0f;

        public Mesh(float[] vertices, uint[] indices, List<Texture> textures)
        {
            this.vertices = vertices;
            this.indices = indices;
            this.textures = textures;

            setupMesh();
        }

        void setupMesh()
        {
            GL.GenBuffers(1, out VBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // We create/bind the EBO the same way as the VBO, just with a different BufferTarget.
            GL.GenBuffers(1, out EBO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);

            // We also buffer data to the EBO the same way.
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            GL.GenVertexArrays(1, out VAO);
            GL.BindVertexArray(VAO);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

            // We bind the EBO here too, just like with the VBO in the previous tutorial.
            // Now, the EBO will be bound when we bind the VAO.
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);

            // The EBO has now been properly setup. Go to the Render function to see how we draw our rectangle now!

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));

            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));


            GL.EnableVertexAttribArray(0);

            GL.BindVertexArray(0);
        }

        // In the mouse wheel function we manage all the zooming of the camera
        // this is simply done by changing the FOV of the camera


        public void Draw(Shader shader)
        {
            var transform = Matrix4.Identity;
            transform *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(20f));
            transform *= Matrix4.CreateScale(1.1f);
            transform *= Matrix4.CreateTranslation(0.1f, 0.1f, 0.0f);
            rotation = rotation + 1;
            // var model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(rotation));
            var model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(20f));

            shader.Use();
            shader.SetMatrix4("model", model);
            var texture = textures.Find(t => t.typeName == "texture_diffuse");
            if (texture is not null)
                texture.Use();
            GL.BindVertexArray(VAO);

            GL.DrawElements(PrimitiveType.Triangles, indices.Length,  DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }
    }
}