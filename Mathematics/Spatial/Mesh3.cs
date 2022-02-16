using System.Collections.Generic;
using System.IO;
using System.Linq;

using static System.Math;

namespace JA.Mathematics.Spatial
{
    public class Face
    {
        public Face(params int[] vertices)
        {
            this.Vertices = new List<int>(vertices);
        }
        public Face(IEnumerable<int> vertices)
        {
            this.Vertices = new List<int>(vertices);
        }
        public List<int> Vertices { get; }
        public int this[int index] { get => Vertices[index]; }
    }

    public class Mesh3
    {
        public Mesh3()
        {
            this.Nodes = new List<Vector3>();
            this.Faces = new List<Face>();
        }

        public Mesh3(IEnumerable<Vector3> nodes, IEnumerable<Face> faces)
        {
            Nodes=nodes.ToList();
            Faces=faces.ToList();
        }

        public static Mesh3 TriangularPrism(double side, double height)
        {
            // ^     C *  -     
            // |      / \   * D 
            // x     /   \ /    
            //    A *-----* B       y -->

            var A = new Vector3(-side/4, -Sqrt(3)*side/4, -height/4);
            var B = new Vector3(-side/4, +Sqrt(3)*side/4, -height/4);
            var C = new Vector3( side/2, 0, -height/4);
            var D = new Vector3(0, 0, 3*height/4);

            var mesh = new Mesh3();
            mesh.AddFace(A, B, C);
            mesh.AddFace(B, D, C);
            mesh.AddFace(C, D, A);
            mesh.AddFace(A, D, B);

            return mesh;
        }

        /// <summary>
        /// Rectangular prism.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <param name="height">The height.</param>
        /// <param name="width">The width.</param>
        /// <returns></returns>
        public static Mesh3 RectangularPrism(double length, double height, double width)
        {
            //     H        G 
            //      *------*  
            //   D /|   C /|  
            //    *------* |  
            //    | *----|-*  
            //    |/ E   |/ F 
            //    *------*    
            //   A        B   
            var A = new Vector3(-length/2, -height/2, +width/2);
            var B = new Vector3(+length/2, -height/2, +width/2);
            var C = new Vector3(+length/2, +height/2, +width/2);
            var D = new Vector3(-length/2, +height/2, +width/2);
            var E = new Vector3(-length/2, -height/2, -width/2);
            var F = new Vector3(+length/2, -height/2, -width/2);
            var G = new Vector3(+length/2, +height/2, -width/2);
            var H = new Vector3(-length/2, +height/2, -width/2);

            var mesh = new Mesh3();
            mesh.AddFace(A, B, C, D);
            mesh.AddFace(B, F, G, C);
            mesh.AddFace(F, E, H, G);
            mesh.AddFace(E, A, D, H);
            mesh.AddFace(D, C, G, H);
            mesh.AddFace(F, E, A, B);
            return mesh;

        }

        public List<Vector3> Nodes { get; }
        public List<Face> Faces { get; }

        public Vector3 this[int node] { get => Nodes[node]; }
        public Vector3 this[int face, int index] { get => Nodes[Faces[face][index]]; }

        public Vector3[] GetNodes(int face)
            => GetNodes(Faces[face]);

        public Vector3[] GetNodes(Face face) 
            => face.Vertices.Select((index) => Nodes[index]).ToArray();

        public void AddFace(params Vector3[] vertices)
        {
            var list = new int[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                var node = vertices[i];

                var n_index = Nodes.IndexOf(node);
                if (n_index>=0)
                {
                    list[i] = n_index;
                }
                else
                {
                    list[i] = Nodes.Count;
                    Nodes.Add(node);
                }
            }

            Faces.Add(new Face(list));
        }

        public Vector3[][] GetShape()
        {
            var faces = new List<Vector3[]>();
            foreach (var face in Faces)
            {
                faces.Add(GetNodes(face));
            }
            return faces.ToArray();
        }

        public Triangle[][] Tesselate()
        {
            var triangles = new List<Triangle[]>();
            foreach (var face in Faces)
            {
                
                var nodes = GetNodes(face);

                if (nodes.Length==3)
                {
                    triangles.Add(new Triangle[] { new Triangle(nodes[0], nodes[1], nodes[2]) });
                }
                else if (nodes.Length>3)
                {
                    var list = new List<Triangle>();
                    int k = 0;
                    int j = 1;

                    for (int i = 2; i < nodes.Length-2; i++)
                    {
                        list.Add( new Triangle(nodes[k], nodes[j], nodes[i]));
                        j = i;
                    }
                    triangles.Add(list.ToArray());
                }
            }
            return triangles.ToArray();
        }
        public void Translate(double dx, double dy, double dz) => Translate(Vector3.Cartesian(dx, dy, dz));
        public void Translate(Vector3 delta)
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                Nodes[i] += delta;
            }            
        }
        public void Rotate(Quaternion rotation, Vector3 about)
        {
            var R = rotation.ToRotationMatrix();
            for (int i = 0; i < Nodes.Count; i++)
            {
                Nodes[i] = about + R*(Nodes[i]-about);
            }
        }
        public void Scale(double factor, Vector3 about)
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                Nodes[i] = about + factor*(Nodes[i]-about);
            }
        }
        public static Mesh3 ReadFromStl(string filename, float scale = 1)
        {
            // Imports a binary STL file
            // Code Taken From:
            // https://sukhbinder.wordpress.com/2013/12/10/new-fortran-stl-binary-file-reader/
            // Aug 27, 2019
            var fs = File.OpenRead(filename);
            var stl = new BinaryReader(fs);

            // Ignore header
            var header = new string(stl.ReadChars(80));
            var n_elems = stl.ReadInt32();

            var nodes = new List<Vector3>();
            var faces = new List<Face>();

            for (int i = 0; i < n_elems; i++)
            {
                // Ignore surface normals for now
                var normal = new Vector3(
                    stl.ReadSingle(),
                    stl.ReadSingle(),
                    stl.ReadSingle());
                var a = new Vector3(
                    scale*stl.ReadSingle(),
                    scale*stl.ReadSingle(),
                    scale*stl.ReadSingle());
                var b = new Vector3(
                    scale*stl.ReadSingle(),
                    scale*stl.ReadSingle(),
                    scale*stl.ReadSingle());
                var c = new Vector3(
                    scale*stl.ReadSingle(),
                    scale*stl.ReadSingle(),
                    scale*stl.ReadSingle());
                // burn two bytes
                var temp = stl.ReadBytes(2);

                // get index of next point, and add point to list of nodes
                int index_a = nodes.Count;
                nodes.Add(a);
                int index_b = nodes.Count;
                nodes.Add(b);
                int index_c = nodes.Count;
                nodes.Add(c);
                // add face from the three index values
                faces.Add(new Face(index_a, index_b, index_c));
            }

            stl.Close();

            return new Mesh3(nodes.ToArray(), faces.ToArray());
        }
    }
}
