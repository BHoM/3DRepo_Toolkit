using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeDRepo
{
    public class Point {
        public double x;
        public double y;
        public double z;

        public Point(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    public class Face {
        public int[] indices;

        public Face(int[] indices) {
            this.indices = indices;
        }
    }

    public class Mesh
    {
        public string name;
        public Point[] vertices;
        public Face[] faces;

        public Mesh(string name, Point[] vertices, Face[] faces) {
            this.name = name;
            this.vertices = vertices;
            this.faces = faces;
        }
    }
}
