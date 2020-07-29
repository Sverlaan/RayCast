using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;

namespace RayCast
{
    class Sketch : Form
    {
        readonly Source source;
        List<Boundary> walls;
        bool dragged;       // Signals mouse-drag
        double max_dist;    // Maximum viewing distance
        int wall_height = 200;
        Point start, end;

        readonly Canvas canvas1, canvas2;
        readonly RadioButton add, remove;
        readonly Button clear, random, reset;
        readonly TrackBar slider1, slider3, slider2;
        readonly Label label1, label2, label3;

        public Sketch()
        {
            this.Size = new Size(1225, 640);
            this.BackColor = Color.SlateGray;
            this.Text = "RayCast";
            this.KeyPreview = true;

            // Initialize the source (camera)
            source = new Source(new Point(130, 240), 0, 60);

            int space = 10;

            // Initialize controls with Size, Location, etc.
            canvas1 = new Canvas
            {
                Location = new Point(0, 0),
                Size = new Size(600, 600),
                BackColor = Color.DarkSlateGray
            };

            walls = SetWalls(canvas1.Width, canvas1.Height);    // Set bounds and random boundaries
            max_dist = 850;     // Value more than diagonal of canvas1, so everything is visible

            canvas2 = new Canvas
            {
                Location = new Point(canvas1.Right + space, canvas1.Top),
                Size = new Size(602, 400),
                BackColor = Color.Black
            };

            slider1 = new TrackBar
            {
                Location = new Point(900, canvas2.Bottom + 2 * space),
                Size = new Size(150, 30),
                TickFrequency = 5,
                Minimum = 10,
                Maximum = 120,
                Value = source.angle
            };

            slider2 = new TrackBar
            {
                Location = new Point(slider1.Left, slider1.Bottom),
                Size = new Size(150, 30),
                TickFrequency = 50,
                Minimum = 50,
                Maximum = 400,
                Value = wall_height
            };

            slider3 = new TrackBar
            {
                Location = new Point(slider2.Left, slider2.Bottom),
                Size = new Size(150, 30),
                TickFrequency = 100,
                Minimum = 100,
                Maximum = 1500,
                Value = 850
            };

            label1 = new Label
            {
                Location = new Point(slider1.Right, slider1.Top),
                Size = new Size(60, slider1.Height),
                TextAlign = System.Drawing.ContentAlignment.TopLeft,
                Font = new Font("Arial", 16, FontStyle.Regular),
                Text = source.angle.ToString() + "°"
            };

            label2 = new Label
            {
                Location = new Point(slider3.Right, slider3.Top),
                Size = new Size(100, slider3.Height),
                TextAlign = System.Drawing.ContentAlignment.TopLeft,
                Font = new Font("Arial", 16, FontStyle.Regular),
                Text = (max_dist / 100.0).ToString() + " m"
            };

            label3 = new Label
            {
                Location = new Point(slider2.Right, slider2.Top),
                Size = new Size(100, slider3.Height),
                TextAlign = System.Drawing.ContentAlignment.TopLeft,
                Font = new Font("Arial", 16, FontStyle.Regular),
                Text = (wall_height / 100.0).ToString() + " m"
            };

            reset = new Button
            {
                Location = new Point(slider3.Right - (slider3.Width + 100) / 2, slider3.Bottom),
                Size = new Size(100, 30),
                Text = "Reset"
            };

            add = new RadioButton
            {
                Location = new Point(canvas2.Left + 70, canvas2.Bottom + 40),
                Size = new Size(155, 30),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Font = new Font("Arial", 14, FontStyle.Regular),
                Text = "Add Walls"
            };

            remove = new RadioButton
            {
                Location = new Point(add.Left, add.Bottom),
                Size = new Size(155, 30),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Font = new Font("Arial", 14, FontStyle.Regular),
                Text = "Remove Walls"
            };

            clear = new Button
            {
                Location = new Point(remove.Left, remove.Bottom + space),
                Size = new Size(70, 30),
                Text = "Clear"
            };

            random = new Button
            {
                Location = new Point(clear.Right, remove.Bottom + space),
                Size = new Size(70, 30),
                Text = "Random"
            };

            // Events
            this.KeyDown += this.MoveHead;
            canvas1.Paint += this.DrawCaster;
            canvas2.Paint += this.DrawViewer;
            canvas2.MouseWheel += this.Rotate;
            slider1.ValueChanged += this.SetAngle;
            slider3.ValueChanged += this.SetDist;
            slider2.ValueChanged += this.SetWallHeight;
            reset.Click += this.ResetSliders;
            clear.Click += this.ClearWalls;
            random.Click += this.RandomWalls;
            canvas1.MouseDown += this.Mouse_Down;
            canvas1.MouseMove += this.Mouse_Drag;
            canvas1.MouseUp += this.Mouse_Up;

            // Add controls to the window
            Controls.AddRange(new Control[] { canvas1, canvas2, slider1, slider3, slider2,
                label1, label2, label3, reset, add, remove, clear, random
            });
        }

        private void DrawCaster(object ob, PaintEventArgs pea)
        {
            Graphics gr = pea.Graphics;
            Pen pen = new Pen(Color.FromArgb(50, 255, 255, 255), 1);

            if (dragged)
                gr.DrawLine(new Pen(Color.Gray, 2), start, end);    // Draw preview of adding new boundary

            source.UpdateRays();    // Set rays for current position and angle
            foreach (Ray ray in source.rays)
            {
                // Get endpoint of ray (first intersection)
                Point closest = ray.Intersect(walls, source.dir, max_dist);

                if (!closest.IsEmpty)
                    gr.DrawLine(pen, source.pos.X, source.pos.Y, closest.X, closest.Y);   // Draw ray
            }

            // Draw all boundaries
            foreach (Boundary wall in walls)
                wall.Show(gr);

            source.Show(gr);    // Draw source/camera-point

            if (!dragged)
                canvas2.Refresh();  // Draw 3D-viewer
        }


        private void DrawViewer(object ob, PaintEventArgs pea)
        {
            Graphics gr = pea.Graphics;

            // Draw floor
            gr.FillRectangle(Brushes.DarkSlateGray, 0, canvas2.Height / 2, canvas2.Width, canvas2.Height / 2);

            // Calculate focal length
            double radian = Math.PI / 180 * source.angle;
            double focal_length = (canvas2.Width / 2) * Math.Tan(radian / 2);

            float width = (float)canvas2.Width / source.rays.Count();  // width of rectangle corresponding to a single ray
            float yCenter = canvas2.Height / 2;

            int counter = 0;
            foreach (Ray ray in source.rays)
            {
                // Transparency (white-value) and height of rectangle depends on distance
                int alpha = (int)(255 - (ray.dist * 255 / max_dist));
                float height = (float)(wall_height * focal_length / ray.dist);

                float xCenter = counter * width - (width / 2);  // From left to right

                // Draw rectangle slice
                SolidBrush brush = new SolidBrush(Color.FromArgb(alpha, alpha, alpha));
                FillCenterRect(gr, brush, xCenter, yCenter, width, height);

                counter++;
            }

            // Draw red '+' sign in the center
            gr.FillRectangle(Brushes.Red, (canvas2.Width - 6) / 2, (canvas2.Height - 20) / 2, 3, 17);
            gr.FillRectangle(Brushes.Red, (canvas2.Width - 20) / 2, (canvas2.Height - 6) / 2, 17, 3);
        }
        private void MoveHead(object ob, KeyEventArgs kea)
        {
            double dir_radian;

            // Perform corresponding action depending on which key was clicked
            switch (kea.KeyData)
            {
                case (Keys.D):
                    source.dir -= 5;    // Turn view to right
                    break;
                case (Keys.A):
                    source.dir += 5;    // Turn view to left
                    break;
                case (Keys.W):
                    // Step forwards
                    dir_radian = Math.PI / 180 * source.dir;
                    source.pos.X += (float)(5 * Math.Cos(dir_radian));
                    source.pos.Y += (float)(5 * -Math.Sin(dir_radian));
                    break;
                case (Keys.S):
                    // Step backwards
                    dir_radian = Math.PI / 180 * source.dir;
                    source.pos.X -= (float)(5 * Math.Cos(dir_radian));
                    source.pos.Y -= (float)(5 * -Math.Sin(dir_radian));
                    break;
            }
            canvas1.Refresh();
        }

        private void Mouse_Down(object ob, MouseEventArgs mea)
        {
            if (add.Checked)
            {
                // First time click sets the start-position of the new wall
                if (!dragged)
                    start = mea.Location;

                dragged = true;
                end = mea.Location;
                canvas1.Refresh();
            }
            else if (remove.Checked)
            {
                Point pos = mea.Location;

                // Delete corresponding boundary if mouseclick was on top of one
                foreach (Boundary wall in walls)
                    if (Collides(wall, pos))
                    {
                        walls.Remove(wall);
                        break;
                    }
                canvas1.Refresh();
            }
        }

        private void Mouse_Up(object ob, MouseEventArgs mea)
        {
            // New definitive wall gets added
            if (add.Checked)
            {
                dragged = false;
                walls.Add(new Boundary(start, end));
                canvas1.Refresh();
            }
        }
        private void Mouse_Drag(object ob, MouseEventArgs mea)
        {
            if (dragged && add.Checked)
            {
                // Change end-position for intermediate/preview drawing of new boundary
                end = mea.Location;
                canvas1.Refresh();
            }
        }


        // For turning the view, using the mousewheel
        private void Rotate(object ob, MouseEventArgs mea)
        {
            if (mea.Delta > 0)
                source.dir += 5;
            else
                source.dir -= 5;

            canvas1.Refresh();
        }

        // For changing the FOV-angle, using a slider
        private void SetAngle(object ob, EventArgs ea)
        {
            source.angle = slider1.Value;
            label1.Text = source.angle.ToString() + "°";
            canvas1.Refresh();
        }

        // For changing the viewing distance, using a slider
        private void SetDist(object ob, EventArgs ea)
        {
            max_dist = slider3.Value;
            label2.Text = (max_dist / 100.0).ToString() + " m";
            canvas1.Refresh();
        }

        // For changing the height of the boundaries, using a slider
        private void SetWallHeight(object ob, EventArgs ea)
        {
            wall_height = slider2.Value;
            label3.Text = (wall_height / 100.0).ToString() + " m";
            canvas2.Refresh();
        }

        private void ResetSliders(object ob, EventArgs ea)
        {
            // Set all sliders to default values
            slider1.Value = 60;
            slider2.Value = 200;
            slider3.Value = 850;
        }

        private void ClearWalls(object ob, EventArgs ea)
        {
            // Clear all existing walls
            walls.Clear();

            // Add outer walls
            walls.Add(new Boundary(new Point(0, 0),
                new Point(0, canvas1.Height)));
            walls.Add(new Boundary(new Point(0, 0),
                new Point(canvas1.Width, 0)));
            walls.Add(new Boundary(new Point(0, canvas1.Height),
                new Point(canvas1.Width, canvas1.Height)));
            walls.Add(new Boundary(new Point(canvas1.Width, 0),
                new Point(canvas1.Width, canvas1.Height)));

            canvas1.Refresh();
        }

        private void RandomWalls(object ob, EventArgs ea)
        {
            walls = SetWalls(canvas1.Width, canvas1.Height);
            canvas1.Refresh();
        }

        // Helper-function for drawing rectangles given a center-point
        private void FillCenterRect(Graphics gr, Brush brush, float xCenter, float yCenter, float width, float height)
        {
            float x = xCenter - width / 2;
            float y = yCenter - height / 2;

            RectangleF wall = new RectangleF(x, y, width, height);
            gr.FillRectangle(brush, wall);
        }

        private static List<Boundary> SetWalls(int width, int height)
        {
            List<Boundary> boundaries = new List<Boundary>();

            // Add 5 random boundaries
            Random random = new Random();
            for (int i = 0; i < 5; i++)
            {
                boundaries.Add(new Boundary(new Point(random.Next(0, width), random.Next(0, height)),
                    new Point(random.Next(0, width), random.Next(0, height))));
            }

            // Add outer walls
            boundaries.Add(new Boundary(new Point(0, 0),
                new Point(0, height)));
            boundaries.Add(new Boundary(new Point(0, 0),
                new Point(width, 0)));
            boundaries.Add(new Boundary(new Point(0, height),
                new Point(width, height)));
            boundaries.Add(new Boundary(new Point(width, 0),
                new Point(width, height)));

            return boundaries;
        }

        // The following adjusted code snippet is from:
        // https://stackoverflow.com/questions/907390/how-can-i-tell-if-a-point-belongs-to-a-certain-line
        public bool Collides(Boundary wall, Point pos)
        {
            Point leftPoint;
            Point rightPoint;
            double fuzziness = 10;

            // Normalize start/end to left right to make the offset calc simpler.
            if (wall.a.X <= wall.b.X)
            {
                leftPoint = wall.a;
                rightPoint = wall.b;
            }
            else
            {
                leftPoint = wall.b;
                rightPoint = wall.a;
            }

            // If point is out of bounds, no need to do further checks.                  
            if (pos.X + fuzziness < leftPoint.X || rightPoint.X < pos.X - fuzziness)
                return false;
            else if (pos.Y + fuzziness < Math.Min(leftPoint.Y, rightPoint.Y)
                || Math.Max(leftPoint.Y, rightPoint.Y) < pos.Y - fuzziness)
                return false;

            double deltaX = rightPoint.X - leftPoint.X;
            double deltaY = rightPoint.Y - leftPoint.Y;

            // If the line is straight, the earlier boundary check is enough to determine that the point is on the line.
            // Also prevents division by zero exceptions.
            if (deltaX == 0 || deltaY == 0)
                return true;

            double slope = deltaY / deltaX;
            double offset = leftPoint.Y - leftPoint.X * slope;
            double calculatedY = pos.X * slope + offset;

            // Check calculated Y matches the points Y coord with some easing.
            bool lineContains = pos.Y - fuzziness <= calculatedY && calculatedY
                <= pos.Y + fuzziness;

            return lineContains;
        }
    }
}
