using ContinentPro.Resources.Classes;
using System.Diagnostics;
using System.IO;
using System.Speech.Synthesis;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
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
        private Label[] continentButtons;

        private double originalWidth, originalHeight, originalLeft, originalTop;
        private bool isExpanded = false;
        private readonly SpeechSynthesizer BuiltInSpeech= new();

        string? RootPath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.FullName;

        public MainWindow()
        {

            InitializeComponent();
            InitialContinents();

            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);

            BuiltInSpeech.StateChanged += ChangeSpeechUI;
            ChangeSpeechUI(null, null);
        }

        private void ReloadContent()
        {
            foreach(var c in continentButtons)
            {
                MainGrid.Children.Remove(c);
            }
            InitialContinents();
        }

        public void InitialContinents()
        {
            using (StreamReader r = new StreamReader(RootPath + "/Resources/Save/ContinentsSave.json"))
            {
                string jsonString = r.ReadToEnd();

                var jsonObject = JsonSerializer.Deserialize<Continent[]>(jsonString);

                if (jsonObject == null)
                {
                    Debug.WriteLine("Error loading continents");
                    return;
                }

                continents = jsonObject;
            }
            
            continentButtons = new Label[continents.Length];
            var i = 0;
            foreach (Continent continent in continents)
            {
                Label button = new Label
                {
                    Tag = continent.Name,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = continent.Width,
                    Height = continent.Height,
                    Margin = new Thickness(continent.Longitude, continent.Latitude, 0, 0),
                };

                button.Content = new Image
                {
                    Source = new BitmapImage(new Uri(RootPath + continent.ContinentImageLocation, UriKind.Absolute)),
                    Stretch = Stretch.Fill
                };

                button.MouseDown += Continent_Click;

                // Add the button to the grid
                MainGrid.Children.Insert(1,button);
                continentButtons[i++] = button;
            }
        }

        #region UserInput
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    if (isExpanded && !isAnimating)
                    {
                        isExpanded = false;

                        foreach (Label continent in continentButtons)
                        {
                            if (continent.Tag == ContinentName.Content)
                            {
                                InfoViewer.Visibility = Visibility.Hidden;
                                AnimateButton(continent, originalWidth, originalHeight, originalLeft, originalTop);
                                break;
                            }
                        }
                    }
                    break;
                case Key.Space:
                    ReloadContent();
                    break;
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

        #endregion

        #region UI
        private void ShowInfo(Continent information)
        {
            ContinentName.Content = information.Name;
            ContinentDescription.Text = information.Description;
        }

        bool isAnimating;
        private void AnimateButton(Label ResizeButton, double newWidth, double newHeight, double newLeft, double newTop)
        {
            if (isAnimating) return;
            isAnimating = true;

            double animationLength = .5;
            ResizeButton.IsEnabled = false;

            Debug.WriteLine($"Target: {ResizeButton.Tag.ToString()}");
            Debug.WriteLine($"Width {originalWidth}, Height {originalHeight}, Left {originalLeft}, Top {originalTop}");
            Debug.WriteLine($"Width {newWidth}, Height {newHeight}, Left {newLeft}, Top {newTop}");

            DoubleAnimation widthAnimation = new DoubleAnimation(newWidth, TimeSpan.FromSeconds(animationLength));
            DoubleAnimation heightAnimation = new DoubleAnimation(newHeight, TimeSpan.FromSeconds(animationLength));
            ThicknessAnimation positionAnimation = new ThicknessAnimation(
                new Thickness(newLeft, newTop, 0, 0),
                TimeSpan.FromSeconds(animationLength));

            widthAnimation.Completed += (s, e) =>
            {
                isAnimating = false;
                ResizeButton.IsEnabled = true;
                if(isExpanded)
                {
                    InfoViewer.Visibility = Visibility.Visible;
                }
            };

            ResizeButton.BeginAnimation(Label.WidthProperty, widthAnimation);
            ResizeButton.BeginAnimation(Label.HeightProperty, heightAnimation);
            ResizeButton.BeginAnimation(Label.MarginProperty, positionAnimation);
        }

        #endregion

        #region TTS

        private void ReadText(string text)
        {
            string voice = BuiltInSpeech.GetInstalledVoices()[0].VoiceInfo.Name;
            BuiltInSpeech.SelectVoice(voice);
            BuiltInSpeech.SpeakAsync(text);
        }

        private void ContinentRead(object sender, EventArgs e)
        {
            if (BuiltInSpeech.State == SynthesizerState.Speaking)
            {
                BuiltInSpeech.SpeakAsyncCancelAll();
                return;
            }

            ReadText(ContinentDescription.Text);
        }

        private void PlaceRead(object sender, EventArgs e)
        {
            if (BuiltInSpeech.State == SynthesizerState.Speaking)
            {
                BuiltInSpeech.SpeakAsyncCancelAll();
                return;
            }

            ReadText(PlaceDescription.Text);
        }

        private void ChangeSpeechUI(object? sender, StateChangedEventArgs? e)
        {
            var muteImage1 = new Image
            {
                Source = new BitmapImage(new Uri(RootPath + "/Resources/Images/mute.png", UriKind.RelativeOrAbsolute))
            };

            var muteImage2 = new Image
            {
                Source = new BitmapImage(new Uri(RootPath + "/Resources/Images/mute.png", UriKind.RelativeOrAbsolute))
            };

            // Create separate mute containers for each button
            Grid muteContainer1 = new Grid();
            muteContainer1.Children.Add(new Border { Background = new SolidColorBrush(Colors.LightSlateGray) }); // Gray background
            muteContainer1.Children.Add(muteImage1); // Mute icon

            Grid muteContainer2 = new Grid();
            muteContainer2.Children.Add(new Border { Background = new SolidColorBrush(Colors.LightSlateGray) });
            muteContainer2.Children.Add(muteImage2); // Separate mute icon

            // Set default state to mute for both
            ContinentSpeech.Content = muteContainer1;
            PlaceSpeech.Content = muteContainer2;

            if (e != null && e.State == SynthesizerState.Speaking)
            {
                var volumeImage1 = new Image
                {
                    Source = new BitmapImage(new Uri(RootPath + "/Resources/Images/volume.png", UriKind.RelativeOrAbsolute))
                };


                Grid volumeContainer1 = new Grid();
                volumeContainer1.Children.Add(new Border { Background = new SolidColorBrush(Colors.LightGreen) }); // Green background
                volumeContainer1.Children.Add(volumeImage1); // Volume icon

                // Update both buttons when speaking
                ContinentSpeech.Content = volumeContainer1;
            }

        }
        #endregion
    }
}