using ContinentPro.Resources.Classes;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace ContinentPro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Continent[] continents;

        private double originalWidth, originalHeight, originalLeft, originalTop;
        private bool isExpanded = false;

        public MainWindow()
        {
            InitializeComponent();
            InitialContinents();
        }

        public void InitialContinents()
        {
            if (continents == null)
            {
                continents = new Continent[1];
                continents[0] = new Continent
                {
                    Name = "Europe",
                    Description = "Europe is a continent located entirely in the Northern Hemisphere and mostly in the Eastern Hemisphere. It comprises the westernmost part of Eurasia and is bordered by the Arctic Ocean to the north, the Atlantic Ocean to the west, the Mediterranean Sea to the south, and Asia to the east.",
                    ContinentImageLocation = "/Resources/Images/europe.png",
                    Size = new System.Drawing.Point(200, 140),
                    Location = new System.Drawing.Point(100, 100),
                };
            }

            foreach (Continent continent in continents)
            {
                Label button = new Label
                {
                    Tag = continent.Name,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = continent.Size.X,
                    Height = continent.Size.Y,
                    Margin = new Thickness(continent.Location.X, continent.Location.Y, 0, 0),
                };

                button.Content = new Image
                {
                    Source = new BitmapImage(new Uri(continent.ContinentImageLocation, UriKind.RelativeOrAbsolute)),
                    Stretch = Stretch.Fill
                };

                button.MouseDown += Continent_Click;

                // Add the button to the grid
                MainGrid.Children.Add(button);
            }
        }

        private void Continent_Click(object sender, RoutedEventArgs e)
        {
            Label b = (Label)sender;
            Debug.WriteLine(b.Tag);

            double newWidth, newHeight, newTop, newLeft;

            if (!isExpanded)
            {
                originalWidth = b.Width;
                originalHeight = b.Height;
                originalLeft = b.Margin.Left - b.Margin.Right;
                originalTop = b.Margin.Top - b.Margin.Bottom;

                newWidth = this.Width / 2;
                newHeight = originalHeight * (newWidth / originalWidth);
                newLeft = 0;
                newTop = 0;
            }
            else
            {
                newWidth = originalWidth;
                newHeight = originalHeight;
                newLeft = originalLeft;
                newTop = originalTop;

                InfoViewer.Visibility = Visibility.Hidden;
            }

            isExpanded = !isExpanded;
            AnimateButton(b, newWidth, newHeight, newLeft, newTop);

            foreach (Continent continent in continents)
            {
                if (continent.Name == b.Tag.ToString())
                {
                    ShowInfo(continent);
                    break;
                }
            }
        }

        private void ShowInfo(Continent information)
        {
            ContinentName.Content = information.Name;
            ContinentDescription.Text = information.Description;
        }

        private void AnimateButton(Label ResizeButton, double newWidth, double newHeight, double newLeft, double newTop)
        {
            double animationLength = .5;
            ResizeButton.IsEnabled = false;

            DoubleAnimation widthAnimation = new DoubleAnimation(newWidth, TimeSpan.FromSeconds(animationLength));
            DoubleAnimation heightAnimation = new DoubleAnimation(newHeight, TimeSpan.FromSeconds(animationLength));
            ThicknessAnimation positionAnimation = new ThicknessAnimation(
                new Thickness(newLeft, newTop, 0, 0),
                TimeSpan.FromSeconds(animationLength));

            widthAnimation.Completed += (s, e) =>
            {
                ResizeButton.IsEnabled = true;
                if(originalWidth != newWidth)
                {
                    InfoViewer.Visibility = Visibility.Visible;
                }
            };

            ResizeButton.BeginAnimation(Button.WidthProperty, widthAnimation);
            ResizeButton.BeginAnimation(Button.HeightProperty, heightAnimation);
            ResizeButton.BeginAnimation(Button.MarginProperty, positionAnimation);
        }
    }
}