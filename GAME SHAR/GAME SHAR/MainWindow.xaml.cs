using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

//The Color Ball Game(14V)

namespace GAME_SHAR
{
    public partial class MainWindow : Window
    {
        // Основные элементы игры
        private Ellipse ball;          // Шарик, которым управляет игрок
        private double ballSize = 40;  // Размер шарика
        private Geometry starGeometry;  // Геометрия звезды для маски
        private int fillCounter;       // Счетчик заполнения звезды
        private const int WinThreshold = 2000; // Порог для победы
        private readonly Brush starColor = Brushes.LightGoldenrodYellow; // Цвет звезды

        public MainWindow()
        {
            InitializeComponent();
            InitializeBall(); // Инициализация шарика
            this.Loaded += MainWindow_Loaded; // Обработчик загрузки окна
        }

        // Инициализация при загрузке окна
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CreateStarMask(); // Создание маски и звезды
        }

        // Создание и настройка шарика
        private void InitializeBall()
        {
            ball = new Ellipse
            {
                Width = ballSize,
                Height = ballSize,
                Fill = Brushes.Red // Красный цвет шарика
            };
            CanvasBall.Children.Add(ball); // Добавление шарика на верхний слой
        }

        // Создание маски и визуального представления звезды
        private void CreateStarMask()
        {
            // Получение размеров игрового поля
            double canvasWidth = CanvasTrail.ActualWidth;
            double canvasHeight = CanvasTrail.ActualHeight;

            // Генерация геометрии звезды
            starGeometry = CreateStarGeometry(
                new Point(canvasWidth / 2, canvasHeight / 2), // Центр звезды
                outerRadius: 100,    // Внешний радиус лучей
                innerRadius: 50,     // Внутренний радиус впадин
                numPoints: 5         // Количество лучей
            );

            // Настройка маски прозрачности
            var maskBrush = new VisualBrush(new Path { Data = starGeometry, Fill = Brushes.Black })
            {
                Stretch = Stretch.None,
                AlignmentX = AlignmentX.Center,
                AlignmentY = AlignmentY.Center
            };

            // Установка фона звезды и маски
            CanvasTrail.Background = new VisualBrush(new Path { Data = starGeometry, Fill = starColor });
            CanvasTrail.OpacityMask = maskBrush; // Маска показывает только область звезды
        }

        // Генерация геометрии звезды
        private Geometry CreateStarGeometry(Point center, double outerRadius, double innerRadius, int numPoints)
        {
            var geo = new StreamGeometry();
            double angleStep = Math.PI / numPoints; // Шаг угла между лучами

            using (var ctx = geo.Open())
            {
                bool isFirst = true;
                // Создание 2*numPoints+1 точек для формирования звезды
                for (int i = 0; i <= 2 * numPoints; i++)
                {
                    // Чередование внешнего и внутреннего радиусов
                    double radius = (i % 2 == 0) ? outerRadius : innerRadius;
                    double angle = i * angleStep - Math.PI / 2; // Смещение на 90 градусов

                    // Расчет координат точки
                    Point p = new Point(
                        center.X + radius * Math.Cos(angle),
                        center.Y + radius * Math.Sin(angle)
                    );

                    // Начало фигуры
                    if (isFirst)
                    {
                        ctx.BeginFigure(p, true, true);
                        isFirst = false;
                    }
                    else
                    {
                        ctx.LineTo(p, true, false);
                    }
                }
            }

            geo.Freeze(); // Оптимизация производительности
            return geo;
        }

        // Обработчик движения мыши
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(CanvasTrail);

            // Обновление позиции шарика (центрирование относительно курсора)
            Canvas.SetLeft(ball, mousePos.X - ballSize / 2);
            Canvas.SetTop(ball, mousePos.Y - ballSize / 2);

            // Добавление следа
            var trail = new Ellipse
            {
                Width = ballSize,
                Height = ballSize,
                Fill = Brushes.LightBlue,
                Opacity = 0.4,
                IsHitTestVisible = false // Игнорирование событий мыши
            };

            CanvasTrail.Children.Add(trail);
            Canvas.SetLeft(trail, mousePos.X - ballSize / 2);
            Canvas.SetTop(trail, mousePos.Y - ballSize / 2);

            // Проверка заполнения звезды
            if (starGeometry.FillContains(mousePos))
            {
                fillCounter++;
                if (fillCounter >= WinThreshold)
                {
                    // Показать сообщение о победе
                    var result = MessageBox.Show(
                        "Поздравляем! Вы полностью закрасили звезду!\nНачать заново?",
                        "Победа!",
                        MessageBoxButton.OK);

                    if (result == MessageBoxResult.OK)
                    {
                        // Сброс игры
                        fillCounter = 0;
                        CanvasTrail.Children.Clear(); // Очистка следов
                    }
                }
            }
        }
    }
}