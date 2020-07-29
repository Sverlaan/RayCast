using System;
using System.Collections.Generic;
using System.Drawing;

namespace RayCast
{
    class Source
    {
        public List<Ray> rays;
        public PointF pos;
        public int angle;   // Angle for field of vision
        public int dir;     // Angle from direction vector

        public Source(PointF source, int direction, int angle)
        {
            this.pos = source;
            this.angle = angle;
            this.dir = direction;
            this.rays = new List<Ray>();
        }

        public void UpdateRays()
        {
            // Rays get updated for current position, direction and angle

            rays.Clear();

            double half = angle / 2;
            double right = (dir - half);
            double left = (dir + half);

            // Rays get added from left (+half angle) to right (-half angle)
            // with direction ray in the middle
            for (double i = left; i >= right; i -= 0.2)
                rays.Add(new Ray(this.pos, i));
        }

        public void Show(Graphics gr)
        {
            // Draw source as a small, red circle
            Brush brush = Brushes.Red;
            gr.FillEllipse(brush, this.pos.X - 5, this.pos.Y - 5, 10, 10);
        }
    }

    class Ray
    {
        PointF pos;     // Start point
        PointF point;
        public double angle;    // specifies direction corresponding to source-direction
        public double dist;     // Length to first intersection

        public Ray(PointF source, double angle)
        {
            this.pos = source;
            this.angle = angle;

            // Calculate other point on the ray-line
            // No end-point exists, but it is needed for line equation
            double radian = angle * Math.PI / 180;
            this.point = new PointF((float)Math.Cos(radian), (float)-Math.Sin(radian));
        }

        public Point Intersect(List<Boundary> walls, int source_direction, double distance)
        {
            Point closest = Point.Empty;

            foreach (Boundary wall in walls)
            {
                Point intersect = this.Cast(wall);      // Get intersection of ray and boundary

                if (!intersect.IsEmpty)
                {
                    // Calculate Euclidean distance from source to intersection
                    double euclidean_dist = Distance(this.pos, intersect);

                    // Adjustment for distance perpendicular to source-direction
                    double delta_angle = (this.angle - source_direction) * Math.PI / 180;
                    double d = Math.Cos(delta_angle) * euclidean_dist;

                    // Only save the closest intersection with minimum distance
                    if (d < distance)
                    {
                        closest = intersect;
                        distance = d;
                    }
                }
            }
            this.dist = Math.Abs(distance);
            return closest;
        }

        // Calculate the intersection between ray and boundary (two line-segment intersection)
        // See: https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection
        private Point Cast(Boundary wall)
        {
            float x1 = wall.a.X;
            float y1 = wall.a.Y;
            float x2 = wall.b.X;
            float y2 = wall.b.Y;

            float x3 = this.pos.X;
            float y3 = this.pos.Y;
            float x4 = this.pos.X + this.point.X;
            float y4 = this.pos.Y + this.point.Y;

            float den = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);  // Denominator

            if (den == 0)       // Parallel lines
                return Point.Empty;

            float t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / den;
            float u = -((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / den;

            if (t > 0 && t < 1 && u > 0)    // Ray has no end-point, while boundary is a line-segment
            {
                int x = (int)(x1 + t * (x2 - x1));
                int y = (int)(y1 + t * (y2 - y1));
                Point pt = new Point(x, y);
                return pt;
            }
            else
                return Point.Empty;
        }

        private static double Distance(PointF a, PointF b)
        {
            // Calculate distance bewteen two points, using Pythagorean Theorem
            float deltaX = a.X - b.X;
            float deltaY = a.Y - b.Y;
            float distance = (deltaX * deltaX) + (deltaY * deltaY);
            return Math.Sqrt(distance);
        }
    }

    public class Boundary
    {
        public Point a, b;
        public Boundary(Point start, Point end)
        {
            this.a = start;
            this.b = end;
        }

        public void Show(Graphics gr)
        {
            // Draw boundary
            Pen pen = new Pen(Brushes.White, 2);
            gr.DrawLine(pen, this.a, this.b);
        }
    }
}
