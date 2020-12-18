using System;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Collections.Generic;
using OpenTK.Mathematics;
using Assimp;

namespace app
{
     // This project will explore how to use uniform variable type which allows you to assign values
    // To Shaders at any point during the project
    public class Window : GameWindow
    {

        private readonly float[] _vertices =
        {
            -0.5f, -0.5f, 0.0f, // Bottom-left vertex
             0.5f, -0.5f, 0.0f, // Bottom-right vertex
             0.0f,  0.5f, 0.0f  // Top vertex
        };

        // private readonly float[] _vertices2 =
        // {
        //      0.5f,  0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, // top right
        //      0.5f, -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,// bottom right
        //     -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,// bottom left
        //     -0.5f,  0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,// top left
        // };

        private readonly float[] _vertices2 =
        {
            // Position         Texture coordinates
             0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // top right
             0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
            -0.5f,  0.5f, 0.0f, 0.0f, 1.0f  // top left
        };

        // Then, we create a new array: indices.
        // This array controls how the EBO will use those vertices to create triangles
        private readonly uint[] _indices2 =
        {
            // Note that indices start at 0!
            0, 1, 3, // The first triangle will be the bottom-right half of the triangle
            1, 2, 3  // Then the second will be the top-right half of the triangle
        };

        // So we're going make the triangle pulsate between a color range
        // And in order to do that we'll need a constantly changing value
        // The stopwatch is perfect for this as it's a constantly going up
        private Stopwatch _timer;

        private Shader _shader;

        private common.Camera _camera;
        private bool _firstMove = true;

        private Vector2 _lastPos;
        private List<common.Mesh> meshes = new();
        private List<common.Model> models = new();

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }


        protected override void OnLoad()
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
            // var model = new Model("resources/backpack/backpack.obj");
            // models.Add(model);

            // var mario = new Model("resources/mario/mario.fbx");
            // models.Add(mario);

            var kingkrool = new Model("resources/King K Rool/KingKRool.FBX");
            models.Add(kingkrool);

            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            _shader.Use();


            int nrAttributes = 0;
            GL.GetInteger(GetPName.MaxVertexAttribs, out nrAttributes);
            Console.WriteLine("Maximum number of vertex attributes supported: " + nrAttributes);

            // We start the stop watch here as this method is only called once 
            _timer = new Stopwatch();
            _timer.Start();

            // _view = Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f);
            // _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), Size.X / (float) Size.Y, 0.1f, 100.0f);

            _camera = new common.Camera(Vector3.UnitZ * 30, Size.X / (float)Size.Y);
            CursorGrabbed = true;

            base.OnLoad();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit| ClearBufferMask.DepthBufferBit);

            _shader.Use();

            // So here we get the total seconds that have elapsed since the last time this method has reset
            // And we assign it to the timeValue variable so it can be used for the pulsating color 
            double timeValue = _timer.Elapsed.TotalSeconds;

            // We're increasing / decreasing the green value we're passing into 
            // The shader based off of timeValue we created in the previous line
            // As well as using some built in math functions to help the change be smoother
            float greenValue = (float)Math.Sin(timeValue) / (2.0f + 0.5f);

            // This gets the uniform variable location from the frag shader so that we can 
            // assign the new green value to it
            int vertexColorLocation = GL.GetUniformLocation(_shader.Handle, "ourColor");

            // Here we're assigning the ourColor variable in the frag shader 
            // Via the OpenGL Uniform method which takes in the value as the individual vec values (which total 4 in this instance)
            GL.Uniform4(vertexColorLocation, 0.0f, greenValue, 0.0f, 1.0f);


            _shader.SetMatrix4("view",  _camera.GetViewMatrix());
            // _shader.SetMatrix4("view",  Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f));
            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());

            foreach(var m in models)
            {
                m.Draw(_shader);
            }

            SwapBuffers();

            base.OnRenderFrame(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (!IsFocused) // check to see if the window is focused
            {
                return;
            }

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }
            
            const float cameraSpeed = 15f;
            const float sensitivity = 0.2f;

            if (input.IsKeyDown(Keys.W))
            {
                _camera.Position += _camera.Front * cameraSpeed * (float)e.Time; // Forward
            }

           if (input.IsKeyDown(Keys.S))
            {
                _camera.Position -= _camera.Front * cameraSpeed * (float)e.Time; // Backwards
            }
            if (input.IsKeyDown(Keys.A))
            {
                _camera.Position -= _camera.Right * cameraSpeed * (float)e.Time; // Left
            }
            if (input.IsKeyDown(Keys.D))
            {
                _camera.Position += _camera.Right * cameraSpeed * (float)e.Time; // Right
            }
            if (input.IsKeyDown(Keys.Space))
            {
                _camera.Position += _camera.Up * cameraSpeed * (float)e.Time; // Up
            }
            if (input.IsKeyDown(Keys.LeftShift))
            {
                _camera.Position -= _camera.Up * cameraSpeed * (float)e.Time; // Down
            }

            if (input.IsKeyDown(Keys.L))
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            }
            else
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }

           // Get the mouse state
            var mouse = MouseState;

            if (_firstMove) // this bool variable is initially set to true
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                // Calculate the offset of the mouse position
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
                _camera.Yaw += deltaX * sensitivity;
                _camera.Pitch -= deltaY * sensitivity; // reversed since y-coordinates range from bottom to top
            }
            base.OnUpdateFrame(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            _camera.Fov -= e.OffsetY;
            base.OnMouseWheel(e);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, Size.X, Size.Y);
            _camera.AspectRatio = Size.X / (float)Size.Y;
            base.OnResize(e);
        }

        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            // GL.DeleteBuffer(_vertexBufferObject);
            // GL.DeleteVertexArray(_vertexArrayObject);

            GL.DeleteProgram(_shader.Handle);
            base.OnUnload();
        }
    }
}