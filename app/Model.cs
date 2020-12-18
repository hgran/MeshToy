using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;
using Assimp;

namespace common
{
    // A helper class, much like Shader, meant to simplify loading textures.
    public class Model
    {
        List<Mesh> meshes = new();
        List<Texture> texturesLoaded = new();
        string directory;
        public Model(string modelpath)
        {
            LoadModel(modelpath);
        }
        void LoadModel(string path)
        {
            AssimpContext importer = new AssimpContext();
            var scene = importer.ImportFile(path, PostProcessSteps.Triangulate | PostProcessSteps.GenerateSmoothNormals| PostProcessSteps.FlipUVs | PostProcessSteps.CalculateTangentSpace);
            
            if (scene is null ||  scene.RootNode is null)
            {
                Console.Error.WriteLine("Error loading {0}", path);
                return;
            }

            directory = System.IO.Path.GetDirectoryName(path);

            ProcessNode(scene.RootNode, scene);

        }

        void ProcessNode(Node node, Scene scene)
        {
            foreach(var meshindex in node.MeshIndices)
            {
                var mesh = scene.Meshes[meshindex];
                Console.WriteLine(mesh.Name);
                meshes.Add(ProcessMesh(mesh, scene));
            }

            foreach(var child in node.Children)
            {
                ProcessNode(child, scene);
            }
        }
        Mesh ProcessMesh(Assimp.Mesh mesh, Scene scene)
        {
            var vlist = new List<float>();
                for(int vCnt = 0; vCnt < mesh.VertexCount; vCnt++)
                {
                    var v = mesh.Vertices[vCnt];
                    var n = mesh.Normals[vCnt];

                    vlist.Add(v.X);
                    vlist.Add(v.Y);
                    vlist.Add(v.Z);
                    
                    vlist.Add(n.X);
                    vlist.Add(n.Y);
                    vlist.Add(n.Z);
                    

                    if (mesh.HasTextureCoords(0))
                    {
                        var t = mesh.TextureCoordinateChannels[0][vCnt];
                        vlist.Add(t.X);
                        vlist.Add(t.Y);
                    }
                    else
                    {
                        vlist.Add(0f);
                        vlist.Add(0f);
                    }

                }

                var ilist = new List<uint>();
                foreach(var face in mesh.Faces)
                {
                    foreach(var ind in face.Indices)
                    {
                        ilist.Add((uint)ind);
                    }
                }

                List<Texture> meshTextures = new();
                if (mesh.MaterialIndex >= 0)
                {
                    var material = scene.Materials[mesh.MaterialIndex];
                    List<Texture> diffuseMaps = LoadMaterialTextures(material, TextureType.Diffuse, "texture_diffuse");
                    List<Texture> specularMaps = LoadMaterialTextures(material, TextureType.Specular, "texture_specular");
                    
                    meshTextures.AddRange(diffuseMaps);
                    meshTextures.AddRange(specularMaps);
                }

               return new common.Mesh(vlist.ToArray(), ilist.ToArray(), meshTextures);
        }

        List<Texture> LoadMaterialTextures(Material material, TextureType type, string typeName)
        {
            List<Texture> textures = new();
            for(int i = 0; i < material.GetMaterialTextureCount(type); i++)
            {
                TextureSlot textureslot;
                material.GetMaterialTexture(type, i, out textureslot);
                string path = directory + "\\" + textureslot.FilePath;
                var existingTexture = texturesLoaded.Find(t => t.path == path);
                if (existingTexture is null)
                {
                    Texture texture = new(path);
                    texture.typeName = typeName;
                    textures.Add(texture);
                    texturesLoaded.Add(texture);
                }
                else
                {
                    textures.Add(existingTexture);
                }
            }
            return textures;
        }

        public void Draw(Shader shader)
        {
            foreach (var mesh in meshes)
            {
                mesh.Draw(shader);
            }
        }
    }
}