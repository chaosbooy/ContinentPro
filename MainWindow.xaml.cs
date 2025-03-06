using ContinentPro.Resources.Classes;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Speech.Synthesis;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shell;

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
        private readonly MediaPlayer mediaPlayer = new();

        string? RootPath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.FullName;

        public MainWindow()
        {

            InitializeComponent();
            InitialContinents();

            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);

            BuiltInSpeech.StateChanged += ChangeSpeechUI;
            mediaPlayer.MediaEnded += (s, e) =>
            {
                PlaceSound.Background = Brushes.Gray;
            };
            ChangeSpeechUI(null, null);
        }

        private void ReloadContent()
        {
            isExpanded = false;
            InfoViewer.Visibility = Visibility.Hidden;
            foreach (var c in continentButtons)
            {
                MainGrid.Children.Remove(c);
            }
            InitialContinents();
        }

        public void InitialContinents()
        {
            using (StreamReader r = new StreamReader(RootPath + "/Resources/Save/ContinentSave.json"))
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
        }

        #region UserInput
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    if (isExpanded && !isAnimating)
                    {
                        BuiltInSpeech.SpeakAsyncCancelAll();
                        isExpanded = false;
                        QuizPanel.Visibility = Visibility.Collapsed;

                        foreach (Label continent in continentButtons)
                        {
                            if (continent.Tag == ContinentName.Content)
                            {
                                InfoViewer.Visibility = Visibility.Hidden;
                                continent.Visibility = Visibility.Visible;
                                AnimateButton(continent, originalWidth, originalHeight, originalLeft, originalTop);
                                break;
                            }
                        }
                    }
                    break;
                case Key.Space:
                    if (!isAnimating)
                        ReloadContent();
                    break;
            }
        }

        private void Continent_Click(object sender, MouseEventArgs e)
        {
            Label b = (Label)sender;
            Debug.WriteLine(b.Tag);

            Point clickPosition = e.GetPosition(b);
            if (b.Content is Image image && image.Source is BitmapImage bitmap)
            {
                int x = (int)(clickPosition.X * bitmap.PixelWidth / b.ActualWidth);
                int y = (int)(clickPosition.Y * bitmap.PixelHeight / b.ActualHeight);

                CroppedBitmap croppedBitmap = new CroppedBitmap(bitmap, new Int32Rect(x, y, 1, 1));
                byte[] pixel = new byte[4]; // RGBA (4 bytes)
                croppedBitmap.CopyPixels(pixel, 4, 0);

                // Check if the pixel is transparent
                if (pixel[3] == 0) // Alpha channel (fully transparent)
                    return;
            }

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

                foreach (Continent continent in continents)
                {
                    if (continent.Name == b.Tag.ToString())
                    {
                        ShowInfo(continent, newHeight);
                        break;
                    }
                }
            }
            else
            {
                newWidth = originalWidth;
                newHeight = originalHeight;
                newLeft = originalLeft;
                newTop = originalTop;

                InfoViewer.Visibility = Visibility.Hidden;
                b.Visibility = Visibility.Visible;
            }

            isExpanded = !isExpanded;
            AnimateButton(b, newWidth, newHeight, newLeft, newTop);
        }

        private void Place_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            Places place = (Places)b.Tag;
            PlaceName.Content = place.Name;
            PlaceName.Tag = place;
            PlaceDescription.Text = place.Description;
            if (place.ImageLocation != null)
                PlaceImage.Source = new BitmapImage(new Uri(RootPath + place.ImageLocation, UriKind.Absolute));
            else
                PlaceImage.Height = 0;

            if (place.SoundLocation != null)
            {
                PlaceSound.Tag = place.SoundLocation;
                PlaceSound.Visibility = Visibility.Visible;
            }
            else
                PlaceSound.Visibility = Visibility.Hidden;

            PlaceInfo.Visibility = Visibility.Visible;
            PlacesList.Visibility = Visibility.Collapsed;
        }

        private void BackToList(object sender, RoutedEventArgs e)
        {
            BuiltInSpeech.SpeakAsyncCancelAll();
            PlacesList.Visibility = Visibility.Visible;
            PlaceInfo.Visibility = Visibility.Collapsed;
            QuizPanel.Visibility = Visibility.Collapsed;
        }

        private void GoToQuiz(object sender, RoutedEventArgs e)
        {
            QuizPanel.Visibility = Visibility.Visible;

            QuizPanel.Tag = 0;
            SetupQuestion(0);
        }

        private void LeaveQuiz(object sender, RoutedEventArgs e)
        {
            QuizPanel.Visibility = Visibility.Collapsed; 
            foreach (Button b in Answers.Children)
            {
                b.Background = Brushes.LightGray;
            }
        }


        private void PlaySound(object sender, RoutedEventArgs e)
        {
            if (BuiltInSpeech.State == SynthesizerState.Speaking)
            {
                BuiltInSpeech.SpeakAsyncCancelAll();
                return;
            }
            Button b = (Button)sender;
            mediaPlayer.Open(new Uri(RootPath + b.Tag, UriKind.RelativeOrAbsolute));
            mediaPlayer.Play();


            PlaceSound.Background = Brushes.LightGreen;
        }

        #endregion

        #region Quiz

        private void AnswerClick(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            if (b.Tag != null && b.Tag.ToString() == "true")
            {
                b.Background = Brushes.LightGreen;
            }
            else
            {
                b.Background = Brushes.LightCoral;

                foreach (Button answers in Answers.Children)
                {
                    if (answers.Tag != null && answers.Tag.ToString() == "true")
                    {
                        answers.Background = Brushes.LightGreen;
                    }
                }
            }
        }

        private void NextQuestion(object sender, RoutedEventArgs e)
        {
            int currentQuestion = (int)QuizPanel.Tag;

            if (currentQuestion + 1 >= ((Places)PlaceName.Tag).Quizzes.Length)
            {
                MessageBox.Show("No more questions");
                return;
            }

            foreach(Button b in Answers.Children)
            {
                b.Background = Brushes.LightGray;
            }

            QuizPanel.Tag = currentQuestion + 1;
            SetupQuestion((int)QuizPanel.Tag);
        }

        private void SetupQuestion(int questionIndex)
        {
            Places place = (Places)PlaceName.Tag;
            Quiz[] quizzes = place.Quizzes;

            if (quizzes == null || quizzes.Length == 0)
            {
                Debug.WriteLine("No quizzes found");
                return;
            }

            Quiz quiz = quizzes[questionIndex];
            Question.Text = quiz.Question;

            Answer1.Content = quiz.Options[0];
            Answer2.Content = quiz.Options[1];
            Answer3.Content = quiz.Options[2];

            Answer1.Tag = null;
            Answer2.Tag = null;
            Answer3.Tag = null;

            switch (quiz.Answer)
            {
                case 0:
                    Answer1.Tag = "true";
                    break;
                case 1:
                    Answer2.Tag = "true";
                    break;
                case 2:
                    Answer3.Tag = "true";
                    break;
            }
        }
        #endregion

        #region UI
        private void ShowInfo(Continent information, double newHeight)
        {
            ContinentName.Content = information.Name;
            ContinentDescription.Text = information.Description;

            ContinentImage.Height = newHeight;
            ContinentImage.Source = new BitmapImage(new Uri(RootPath + information.ContinentImageLocation, UriKind.RelativeOrAbsolute));


            PlacesList.Visibility = Visibility.Visible;
            PlaceInfo.Visibility = Visibility.Collapsed;
            PlacesList.Children.Clear();

            if (information.PlacesInfo == null)
                return;

            foreach (Places place in information.PlacesInfo)
            {
                Button placeButton = new Button
                {
                    Content = place.Name,
                    Tag = place,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = 200,
                    Height = 50,
                    Margin = new Thickness(0, 10, 0, 0)
                };
                placeButton.Click += Place_Click;
                PlacesList.Children.Add(placeButton);
            }
        }

        bool isAnimating;
        private void AnimateButton(Label ResizeButton, double newWidth, double newHeight, double newLeft, double newTop)
        {
            if (isAnimating) return;
            isAnimating = true;

            double animationLength = .5;
            MainGrid.IsEnabled = false;

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
                MainGrid.IsEnabled = true;
                if(isExpanded)
                {
                    ResizeButton.Visibility = Visibility.Hidden;
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

        private void PlaceRead(object sender, EventArgs e)
        {
            if (BuiltInSpeech.State == SynthesizerState.Speaking)
            {
                BuiltInSpeech.SpeakAsyncCancelAll();
                return;
            }

            ReadText(PlaceDescription.Text);

            var volumeImage1 = new Image
            {
                Source = new BitmapImage(new Uri(RootPath + "/Resources/Images/volume.png", UriKind.RelativeOrAbsolute))
            };


            Grid volumeContainer1 = new Grid();
            volumeContainer1.Children.Add(new Border { Background = new SolidColorBrush(Colors.LightGreen) }); // Green background
            volumeContainer1.Children.Add(volumeImage1); // Volume icon

            // Update both buttons when speaking
            PlaceSpeech.Content = volumeContainer1;
        }

        private void ChangeSpeechUI(object? sender, StateChangedEventArgs? e)
        {
            if (BuiltInSpeech.State == SynthesizerState.Speaking) return;

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

        }
        #endregion
    }
}