using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class MainForm : Form
    {
        private const float FF = 0.3f;
        private const float UPPER = 1.0f;
        private const float LOWER = 0.01f;

        private bool _isMouseDown = false;
        private bool _isFirstTouchPoint = false;

        private int _counter;
        private Point[] _points = new Point[5];
        private LineSegment _lastSegmentOfPrev;

        public MainForm()
        {
            InitializeComponent();

            Graphics g = panel1.CreateGraphics();
            g.FillRectangle(Brushes.White, panel1.ClientRectangle);
        }

        private float len_sq(Point p1, Point p2) 
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            return dx * dx + dy * dy;
        }

        private float clamp(float val, float lower, float higher) 
        {
            if (val < lower) return lower;
            if (val > higher) return higher;
            return val;
        }

        
        private LineSegment lineSegmentPerpendicularTo(LineSegment seg, float relativeLengthFraction) 
        {
          float x0 = seg.P1.X,
                y0 = seg.P1.Y,
                x1 = seg.P2.X,
                y1 = seg.P2.Y;

          float dx = x1 - x0,
                dy = y1 - y0;

          float xa = x1 + relativeLengthFraction / 2 * dy,
               ya = y1 - relativeLengthFraction / 2 * dx,
               xb = x1 - relativeLengthFraction / 2 * dy,
               yb = y1 + relativeLengthFraction / 2 * dx;

          return new LineSegment()
          {
              P1 = new PointF(xa, ya),
              P2 = new PointF(xb, yb)
          };
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            _isMouseDown = true;
            _isFirstTouchPoint = true;

            Graphics g = panel1.CreateGraphics();
            g.FillRectangle(Brushes.White, panel1.ClientRectangle);

            _counter = 0;
            _points[0] = e.Location;
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isMouseDown) return;

            Graphics g = panel1.CreateGraphics();
            g.SmoothingMode = SmoothingMode.HighQuality;
            
            _counter += 1;
            _points[_counter] = e.Location;
            
            LineSegment[] ls = new LineSegment[4];
            

            if (_counter == 4) {
                
                GraphicsPath path = new GraphicsPath();

                _points[3] = new Point((_points[2].X + _points[4].X) / 2,
                                       (_points[2].Y + _points[4].Y) / 2);

                if (_isFirstTouchPoint) {
                    ls[0] = new LineSegment(){
                        P1 = _points[0],
                        P2 = _points[0]
                    };
                    _isFirstTouchPoint = false;
                } 
                else {
                    ls[0] = _lastSegmentOfPrev;
                }
                
                float frac1 = FF / clamp(len_sq(_points[0], _points[1]), LOWER, UPPER);
                float frac2 = FF / clamp(len_sq(_points[1], _points[2]), LOWER, UPPER);
                float frac3 = FF / clamp(len_sq(_points[2], _points[3]), LOWER, UPPER);

                ls[1] = lineSegmentPerpendicularTo(new LineSegment()
                {
                    P1 = _points[0],
                    P2 = _points[1]
                }, frac1);

                ls[2] = lineSegmentPerpendicularTo(new LineSegment()
                {
                    P1 = _points[1],
                    P2 = _points[2]
                }, frac2);

                ls[3] = lineSegmentPerpendicularTo(new LineSegment()
                {
                    P1 = _points[2],
                    P2 = _points[3]
                }, frac3);

                path.AddBezier(ls[0].P1, ls[1].P1, ls[2].P1, ls[3].P1);
                path.AddLine(ls[3].P1, ls[3].P2);
                path.AddBezier(ls[3].P2, ls[2].P2, ls[1].P2, ls[0].P2);
                path.CloseFigure();

                g.DrawPath(Pens.Black, path);
                g.FillPath(Brushes.Black, path);

                _lastSegmentOfPrev = ls[3];

                _points[0] = _points[3];
                _points[1] = _points[4];
                _counter = 1;
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            _isMouseDown = false;
            Point lastPoint = e.Location;

            Graphics g = panel1.CreateGraphics();
            GraphicsPath path = new GraphicsPath();

            path.AddLine(_lastSegmentOfPrev.P1, lastPoint);
            path.AddLine(lastPoint, _lastSegmentOfPrev.P2);
            path.CloseFigure();

            g.DrawPath(Pens.Black, path);
            g.FillPath(Brushes.Black, path);

            _points = new Point[5];
        }
    }
}
