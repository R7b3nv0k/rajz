using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace rajz_teszt1
{
    public partial class Form1 : Form
    {
        private const int GridSize = 20;
        private Bitmap canvas;
        private bool isDrawing = false;
        private bool isErasing = false;
        private Color currentColor = Color.Black;
        private Button colorButton;
        private Button exitButton;
        private TrackBar thicknessTrackBar;
        private int pencilThickness = 1;
        private FlowLayoutPanel controlPanel;
        private Stack<Bitmap> UndoStack = new Stack<Bitmap>();
        private Button eraserButton;
        private Button fillButton;

        public Form1()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;
            this.BackColor = Color.White;
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;
            this.canvas = new Bitmap(screenWidth, screenHeight);

            this.MouseDown += StartDrawing;
            this.MouseUp += StopDrawing;
            this.MouseMove += Draw;
            this.Paint += RenderCanvas;
            this.KeyDown += Form1_KeyDown;

            AddControlPanel();
            AddExitButton();
            AddColorButton();
            AddThicknessTrackBar();
            AddFillButton();
            AddEraserButton();
            AddUndoButton();
        }

        private void AddControlPanel()
        {
            controlPanel = new FlowLayoutPanel();
            controlPanel.FlowDirection = FlowDirection.TopDown;
            controlPanel.Width = 200;
            controlPanel.Height = this.ClientSize.Height;
            controlPanel.BackColor = Color.FromArgb(50, 50, 50);
            controlPanel.Dock = DockStyle.Right;
            controlPanel.Padding = new Padding(10);
            controlPanel.AutoSize = true;
            this.Controls.Add(controlPanel);
        }

        private void AddUndoButton()
        {
            Button undoButton = new Button();
            undoButton.Text = "↩️ Vissza";
            undoButton.Width = 150;
            undoButton.Height = 50;
            undoButton.FlatStyle = FlatStyle.Flat;
            undoButton.ForeColor = Color.White;
            undoButton.BackColor = Color.FromArgb(70, 70, 70);
            undoButton.Click += UndoButton_Click;
            controlPanel.Controls.Add(undoButton);
        }

        private void UndoButton_Click(object sender, EventArgs e)
        {
            UndoLastAction();
        }

        private void UndoLastAction()
        {
            if (UndoStack.Count > 0)
            {
                canvas = UndoStack.Pop();
                this.Invalidate(); // A képernyő újrarajzolása
            }
        }

        private void SaveCurrentState()
        {
            if (UndoStack.Count >= 10)
            {
                UndoStack.Pop();
            }

            Bitmap backup = new Bitmap(canvas);
            UndoStack.Push(backup);
        }

        private void AddEraserButton()
        {
            eraserButton = new Button();
            eraserButton.Text = "🧽 Radír";
            eraserButton.Width = 150;
            eraserButton.Height = 50;
            eraserButton.FlatStyle = FlatStyle.Flat;
            eraserButton.ForeColor = Color.White;
            eraserButton.BackColor = Color.FromArgb(70, 70, 70);
            eraserButton.Click += EraserButton_Click;
            controlPanel.Controls.Add(eraserButton);
        }

        private void EraserButton_Click(object sender, EventArgs e)
        {
            isErasing = true;
            currentColor = Color.White; // Radírozás színét fehérre állítjuk
        }

        private void AddFillButton()
        {
            fillButton = new Button();
            fillButton.Text = "🔲 Kitöltés";
            fillButton.Width = 150;
            fillButton.Height = 50;
            fillButton.FlatStyle = FlatStyle.Flat;
            fillButton.ForeColor = Color.White;
            fillButton.BackColor = Color.FromArgb(70, 70, 70);
            fillButton.Click += FillButton_Click;
            controlPanel.Controls.Add(fillButton);
        }

        private void FillButton_Click(object sender, EventArgs e)
        {
            if (canvas != null)
            {
                FillArea(100, 100, currentColor);
                this.Invalidate();
            }
        }

        private void FillArea(int startX, int startY, Color fillColor)
        {
            Stack<Point> points = new Stack<Point>();
            Color startColor = canvas.GetPixel(startX, startY);

            if (startColor == fillColor)
                return;

            points.Push(new Point(startX, startY));

            while (points.Count > 0)
            {
                Point p = points.Pop();
                if (p.X < 0 || p.Y < 0 || p.X >= canvas.Width || p.Y >= canvas.Height)
                    continue;

                if (canvas.GetPixel(p.X, p.Y) == startColor)
                {
                    canvas.SetPixel(p.X, p.Y, fillColor);
                    points.Push(new Point(p.X + 1, p.Y));
                    points.Push(new Point(p.X - 1, p.Y));
                    points.Push(new Point(p.X, p.Y + 1));
                    points.Push(new Point(p.X, p.Y - 1));
                }
            }
        }

        private void StartDrawing(object sender, MouseEventArgs e)
        {
            isDrawing = (e.Button == MouseButtons.Left);
            isErasing = (e.Button == MouseButtons.Right);

            SaveCurrentState();
            Draw(sender, e);
        }

        private void StopDrawing(object sender, MouseEventArgs e)
        {
            isDrawing = false;
            isErasing = false;
        }

        private void Draw(object sender, MouseEventArgs e)
        {
            if (isDrawing || isErasing)
            {
                SaveCurrentState();

                using (Graphics g = Graphics.FromImage(canvas))
                {
                    int x = (e.X / GridSize) * GridSize;
                    int y = (e.Y / GridSize) * GridSize;
                    Color color = isErasing ? Color.White : currentColor;
                    using (SolidBrush brush = new SolidBrush(color))
                    {
                        g.FillRectangle(brush, x, y, pencilThickness * GridSize, pencilThickness * GridSize);
                    }
                }
                this.Invalidate();
            }
        }

        private void RenderCanvas(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(canvas, 0, 0);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Application.Exit();
            }
            else if (e.Control && e.KeyCode == Keys.Z)
            {
                UndoLastAction();
            }
        }

        private void AddColorButton()
        {
            colorButton = new Button();
            colorButton.Text = "🎨 Paletta";
            colorButton.Width = 150;
            colorButton.Height = 50;
            colorButton.FlatStyle = FlatStyle.Flat;
            colorButton.ForeColor = Color.White;
            colorButton.BackColor = Color.FromArgb(70, 70, 70);
            colorButton.Click += ColorButton_Click;
            controlPanel.Controls.Add(colorButton);
        }

        private void AddExitButton()
        {
            exitButton = new Button();
            exitButton.Text = "❌ Kilépés";
            exitButton.Width = 150;
            exitButton.Height = 50;
            exitButton.FlatStyle = FlatStyle.Flat;
            exitButton.ForeColor = Color.White;
            exitButton.BackColor = Color.FromArgb(200, 50, 50);
            exitButton.Click += ExitButton_Click;
            controlPanel.Controls.Add(exitButton);
        }

        private void AddThicknessTrackBar()
        {
            thicknessTrackBar = new TrackBar();
            thicknessTrackBar.Minimum = 1;
            thicknessTrackBar.Maximum = 10;
            thicknessTrackBar.Value = pencilThickness;
            thicknessTrackBar.Width = 150;
            thicknessTrackBar.TickFrequency = 1;
            thicknessTrackBar.ValueChanged += ThicknessTrackBar_ValueChanged;
            controlPanel.Controls.Add(thicknessTrackBar);
        }

        private void ThicknessTrackBar_ValueChanged(object sender, EventArgs e)
        {
            pencilThickness = thicknessTrackBar.Value;
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ColorButton_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                currentColor = colorDialog.Color;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            currentColor = Color.Black;
        }
    }
}
