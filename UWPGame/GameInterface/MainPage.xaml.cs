using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using GameLibrary;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Core;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

/* UWP Game Template
 * Created By: Melissa VanderLely
 * Modified By:  
 */

//Loc Pham 4389690

namespace GameInterface
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static GamePiece player;
        private static GamePiece dot;
        private static GamePiece redDot;
        private static GamePiece enemy;
        private DispatcherTimer animationTimer;
        private DispatcherTimer collisionTimer;
        private DispatcherTimer enemySpawnTimer;
        private DispatcherTimer Timeout;
        private bool isPacManVisible = true;
        private static GamePiece pacManImage;
        private static int Score = 0;
        private static int ScoreToWin = 4000;
        private static Random random = new Random();
        private bool isScored = false;
        private bool isScoredRed = false;
        private bool isTouched = false;
        private static int Inmunity = 0;
        private static int Jump = 0;
        private static int WhiteDotCount = 0;
        private bool spawnRedDot = false;
        private static int currentEnemy = 1;
        private List<GamePiece> enemies = new List<GamePiece>();
        private static int time = 120;
        private string PlayerName;
        private List<PlayerRecord> playerRecords = new List<PlayerRecord>();
        private string Difficulty;
        private int MaxEnemies;
        private int SpawnInterval;
        private int PlayerMovementSpeed;
        private static int playerHighestScore = 0;
        //Create and open the text file
        StorageFolder storageFolder;
        StorageFile storageFile;
        private bool FreeRoamMode = false;


        // Create and configure the TextBlock for the score
        TextBlock scoreText = new TextBlock
        {
            Text = $"Score: {Score}",
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 50, 80),
            Foreground = new SolidColorBrush(Windows.UI.Colors.White),
            FontWeight = Windows.UI.Text.FontWeights.Bold,
            FontSize = 24,
            Opacity = 0.7
        };

        // Create and configure the TextBlock for the player highest score
        TextBlock highestText = new TextBlock
        {
            Text = $"Highest Score: {playerHighestScore}",
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 30, 50, 0),
            Foreground = new SolidColorBrush(Windows.UI.Colors.White),
            FontWeight = Windows.UI.Text.FontWeights.Bold,
            FontSize = 24,
            Opacity = 0.7
        };

        // Create and configure the TextBlock for the winning score
        TextBlock winningScore = new TextBlock
        {
            Text = $"Score to win: {ScoreToWin - Score}",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(50, 0, 0, 80),
            Foreground = new SolidColorBrush(Windows.UI.Colors.White),
            FontWeight = Windows.UI.Text.FontWeights.Bold,
            FontSize = 24,
            Opacity = 0.7
        };

        // Create a StackPanel to hold the TextBlocks
        StackPanel textBlockPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal, // Arrange TextBlocks horizontally
            HorizontalAlignment = HorizontalAlignment.Center, // Center the StackPanel
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 0, 80),
        };

        // Create and configure the TextBlock for Inmunity
        TextBlock inmunityText = new TextBlock
        {
            Text = $"Inmunity: {Inmunity}",
            Foreground = new SolidColorBrush(Windows.UI.Colors.White),
            FontWeight = Windows.UI.Text.FontWeights.Bold,
            FontSize = 24,
            Opacity = 0.7
        };

        // Create and configure the TextBlock for the teleport skill
        TextBlock jumpText = new TextBlock
        {
            Text = $"Teleport: {Jump}",
            Margin = new Thickness(20, 0, 0, 0),
            Foreground = new SolidColorBrush(Windows.UI.Colors.White),
            FontWeight = Windows.UI.Text.FontWeights.Bold,
            FontSize = 24,
            Opacity = 0.7
        };

        // Create and configure the TextBlock for the time out
        TextBlock timeoutText = new TextBlock
        {
            Text = $"Time: {time}",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 30, 0, 0),
            Foreground = new SolidColorBrush(Windows.UI.Colors.White),
            FontWeight = Windows.UI.Text.FontWeights.Bold,
            FontSize = 24,
            Opacity = 0.7
        };


        public MainPage()
        {
            this.InitializeComponent();
            //Store new player name and inititialize the highscore with 0 if player record has not been created

            ShowPlayerNameDialog();

        }

        private async void ShowPlayerNameDialog()
        {
            PlayerNameDialogPopup.IsOpen = true;
            //Create and open the text file
            storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            storageFile = await storageFolder.CreateFileAsync("HighScore.txt", CreationCollisionOption.OpenIfExists);

            //Reset the file
            //await FileIO.WriteTextAsync(storageFile, "");
        }

        //Handle Dialogs click events
        private void PlayerNameDialogOK_Click(object sender, RoutedEventArgs e)
        {
            PlayerName = PlayerNameTextBox.Text.Trim();

            if (!string.IsNullOrWhiteSpace(PlayerName))
            {
                // Player entered a valid name, start the game with playerName
                PlayerNameDialogPopup.IsOpen = false;
                List<string> difficultyLevels = new List<string> { "Easy", "Normal", "MyLife" };
                DifficultyListView.ItemsSource = difficultyLevels;
                DifficultyPopup.IsOpen = true;
            }
            else
            {
                new MessageDialog($"Player name cannot be empty").ShowAsync();
            }

        }

        private void HighestScoreDialogClose_Click(object sender, RoutedEventArgs e)
        {
            HighestScoreDialogPopup.IsOpen = false;
        }

        private void HighestScoreDialogKeepPlaying_Click(object sender, RoutedEventArgs e)
        {
            HighestScoreDialogPopup.IsOpen = false;
            FreeRoamMode = true;
            Inmunity++;
            Jump++;
            inmunityText.Text = $"Inmunity: {Inmunity}";
            jumpText.Text = $"Teleport: {player.Jump}";
            enemySpawnTimer.Start();
            animationTimer.Start();
            collisionTimer.Start();
            foreach (var enemy in enemies.ToList())
            {
                enemy.StartEnemyMovement();
            }
            player.StartPlayerMovement();
            pacManImage.StartPlayerMovement();
        }

        private void HighestScoreDialogPlayAgain_Click(object sender, RoutedEventArgs e)
        {
            // Handle the Play Again button click event
            HighestScoreDialogPopup.IsOpen = false;
            Reset();
            PlayerNameDialogPopup.IsOpen = true;
        }

        private void HighestScoreDialogQuit_Click(object sender, RoutedEventArgs e)
        {
            // Handle the Quit button click event
            HighestScoreDialogPopup.IsOpen = false;
            CoreApplication.Exit();
        }

        private void DifficultyListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handle the selection
            if (DifficultyListView.SelectedItem != null)
            {
                Difficulty = DifficultyListView.SelectedItem.ToString();
                // Do something with the selected difficulty, such as setting game difficulty.
                // For now, just close the popup.
                DifficultyPopup.IsOpen = false;
                StartGame();
            }
        }

        private void Reset()
        {
            FreeRoamMode = false;
            gridMain.Children.Remove(player.onScreen);
            gridMain.Children.Remove(pacManImage.onScreen);
            Timeout.Stop();
            enemySpawnTimer.Stop();
            animationTimer.Stop();
            collisionTimer.Stop();
            foreach (var enemy in enemies.ToList())
            {
                gridMain.Children.Remove(enemy.onScreen);
                enemies.Remove(enemy);
            }
            isPacManVisible = true;
            Score = 0;
            ScoreToWin = 4000;
            isScored = false;
            isScoredRed = false;
            isTouched = false;
            Inmunity = 0;
            Jump = 0;
            WhiteDotCount = 0;
            spawnRedDot = false;
            currentEnemy = 1;
            playerHighestScore = 0;
            scoreText.Text = $"Score: {Score}";
            winningScore.Text = $"Score to win: {ScoreToWin - Score}";
            timeoutText.Text = $"Time: {time}";
            inmunityText.Text = $"Inmunity: {Inmunity}";
            jumpText.Text = $"Teleport: {Jump}";
            highestText.Text = $"Highest Score: {playerHighestScore}";

            gridMain.Children.Remove(scoreText);
            gridMain.Children.Remove(winningScore);
            gridMain.Children.Remove(timeoutText);
            gridMain.Children.Remove(highestText);

            // Remove the TextBlocks from the StackPanel
            textBlockPanel.Children.Remove(inmunityText);
            textBlockPanel.Children.Remove(jumpText);
            gridMain.Children.Remove(textBlockPanel);
            gridMain.Children.Remove(dot.onScreen);
            if (redDot != null && gridMain.Children.Contains(redDot.onScreen))
            {
                gridMain.Children.Remove(redDot.onScreen);
            }
        }

        //Start game method
        private async void StartGame()
        {
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            //Randomizing the margins for the dot object
            int newDotTopMargin = random.Next(50, 900);
            int newDotLeftMargin = random.Next(20, 1750);

            //Add the initial dot object
            dot = CreatePiece("dot", 30, newDotLeftMargin, newDotTopMargin);

            //Randomizing the speed for the initialized enemy
            double newEnemySpeed = random.Next(15, 26);


            //Set Up difficulties
            switch (Difficulty)
            {
                case "Easy":
                    ScoreToWin = 2500;
                    winningScore.Text = $"Score to win: {ScoreToWin - Score}";
                    time = 180;
                    timeoutText.Text = $"Time: {time}";
                    Inmunity = 1;
                    inmunityText.Text = $"Inmunity: {Inmunity}";
                    MaxEnemies = 9;
                    SpawnInterval = 11;
                    PlayerMovementSpeed = 38;
                    break;
                case "Normal":
                    ScoreToWin = 4000;
                    winningScore.Text = $"Score to win: {ScoreToWin - Score}";
                    time = 150;
                    timeoutText.Text = $"Time: {time}";
                    Inmunity = 1;
                    inmunityText.Text = $"Inmunity: {Inmunity}";
                    MaxEnemies = 12;
                    SpawnInterval = 9;
                    PlayerMovementSpeed = 32;
                    break;
                case "MyLife":
                    ScoreToWin = 6000;
                    winningScore.Text = $"Score to win: {ScoreToWin - Score}";
                    time = 120;
                    timeoutText.Text = $"Time: {time}";
                    MaxEnemies = 16;
                    SpawnInterval = 7;
                    PlayerMovementSpeed = 28;
                    break;

            }

            //Read file and add to the list
            IList<string> lines = await FileIO.ReadLinesAsync(storageFile);

            if (lines.Count > 0)
            {
                //Retrieve the player records from the text file based on formatted regex
                foreach (var line in lines)
                {
                    var match = Regex.Match(line, @"{playername: ""(.+)"", highscore: (\d+)}");

                    if (match.Success)
                    {
                        string playerName = match.Groups[1].Value;
                        int highScore = int.Parse(match.Groups[2].Value);

                        // Create a new Player instance
                        PlayerRecord playerrecord = new PlayerRecord(playerName, highScore);
                        playerRecords.Add(playerrecord);
                    }
                }
            }

            //Check the list for player prev highest score
            if (playerRecords.Count > 0)
            {
                var existedRecord = playerRecords.Find(p => p.Name == PlayerName);
                if (existedRecord != null)
                {
                    playerHighestScore = existedRecord.HighScore;
                    highestText.Text = $"Highest Score: {playerHighestScore}";
                }
                else
                {
                    playerHighestScore = 0;
                }
            }

            enemy = CreatePieceWithSpeed("enemy", 90, 300, 500, newEnemySpeed);             //create a GamePiece object associated with the enemy image
            enemies.Add(enemy);
            player = CreatePieceWithJump("player", 70, 50, 50, Jump, PlayerMovementSpeed);                      //create a GamePiece object associated with the pac-man image
            pacManImage = CreatePieceWithJump("pacman-close-mouth", 70, 50, 50, Jump, PlayerMovementSpeed);    //create a GamePiece object associated with the 2nd pac-man image for animation

            //Initialize all timers
            InitializeAnimation();
            InitializeCollisionCheck();
            InitializeEnemySpawn();
            InitializeTimeOut();

            //Add the score and the winning score text block to the grid
            gridMain.Children.Add(scoreText);
            gridMain.Children.Add(winningScore);
            gridMain.Children.Add(timeoutText);
            gridMain.Children.Add(highestText);

            // Add the TextBlocks to the StackPanel
            textBlockPanel.Children.Add(inmunityText);
            textBlockPanel.Children.Add(jumpText);

            //Add the stack pannel for the skill and inmunity text block
            gridMain.Children.Add(textBlockPanel);

            //Start the animation movements and the enemies movements
            player.StartPlayerMovement();
            pacManImage.StartPlayerMovement();
            enemy.StartEnemyMovement();
        }

        private GamePiece CreatePiece(string imgSrc, int size, int left, int top)
        {
            Image img = CreateImage(imgSrc, size, left, top);
            gridMain.Children.Add(img);
            return new GamePiece(img, gridMain);
        }

        private GamePiece CreatePieceWithSpeed(string imgSrc, int size, int left, int top, double speed)
        {
            Image img = CreateImage(imgSrc, size, left, top);
            gridMain.Children.Add(img);
            return new GamePiece(img, speed, gridMain);
        }

        private GamePiece CreatePieceWithJump(string imgSrc, int size, int left, int top, int jump, int ms)
        {
            Image img = CreateImage(imgSrc, size, left, top);
            gridMain.Children.Add(img);
            return new GamePiece(img, jump, ms, gridMain);
        }

        private Image CreateImage(string imgSrc, int size, int left, int top)
        {
            Image img = new Image
            {
                Source = new BitmapImage(new Uri($"ms-appx:///Assets/{imgSrc}.png")),
                Width = size,
                Height = size,
                Name = $"img{imgSrc}",
                Margin = new Thickness(left, top, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            return img;
        }

        //Animation timer
        private void InitializeAnimation()
        {
            //DispatcherTimer for animation
            animationTimer = new DispatcherTimer();
            animationTimer.Interval = TimeSpan.FromSeconds(0.3); // animation interval
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();
        }

        //Animation action
        private void AnimationTimer_Tick(object sender, object e)
        {
            // Toggle visibility of Pac-Man and ghost images
            isPacManVisible = !isPacManVisible;

            if (isPacManVisible)
            {
                player.SetVisibility(Visibility.Visible);
                pacManImage.SetVisibility(Visibility.Collapsed);
            }
            else
            {
                player.SetVisibility(Visibility.Collapsed);
                pacManImage.SetVisibility(Visibility.Visible);
            }
        }

        //Animation timer
        private void InitializeTimeOut()
        {
            //DispatcherTimer for animation
            Timeout = new DispatcherTimer();
            Timeout.Interval = TimeSpan.FromSeconds(1.05); // animation interval
            Timeout.Tick += TimeOut_Tick;
            Timeout.Start();
        }

        //Animation action
        private void TimeOut_Tick(object sender, object e)
        {
            time--;
            if(time < 0)
            {
                time = 0;
            }
            timeoutText.Text = $"Time: {time}";

            if (time < 1)
            {
                gridMain.Children.Remove(player.onScreen);
                gridMain.Children.Remove(pacManImage.onScreen);
                collisionTimer.Stop();
                Timeout.Stop();
                new MessageDialog($"Time out!!! Try again").ShowAsync();

                string youtubeUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
                var uri = new Uri(youtubeUrl);
                Windows.System.Launcher.LaunchUriAsync(uri);
            }
        }

        //Enemy spawn handler timer
        private void InitializeEnemySpawn()
        {
            //DispatcherTimer for animation
            enemySpawnTimer = new DispatcherTimer();
            enemySpawnTimer.Interval = TimeSpan.FromSeconds(SpawnInterval); // spawn interval
            enemySpawnTimer.Tick += EnemySpawnTimer_Tick;
            enemySpawnTimer.Start();
        }


        //Enemy spawn action
        private void EnemySpawnTimer_Tick(object sender, object e)
        {
            EnemySpawnHandler();
        }

        //Enemy spawn method
        private void EnemySpawnHandler()
        {
            if (currentEnemy < MaxEnemies)
            {
                if (Score > 1600)
                {
                    if (currentEnemy % 5 == 0)
                    {
                        double newBigEnemySpeed = random.Next(35, 51);
                        int newBigEnemyTopMargin = random.Next(150, 900);
                        int newBigEnemyLeftMargin = random.Next(150, 1750);
                        GamePiece newBigEnemy = CreatePieceWithSpeed("enemy", 150, newBigEnemyLeftMargin, newBigEnemyTopMargin, newBigEnemySpeed);
                        newBigEnemy.StartEnemyMovement();
                        enemies.Add(newBigEnemy);
                        currentEnemy++;
                    }
                    else
                    {
                        double newEnemySpeed = random.Next(35, 51);
                        int newEnemyTopMargin = random.Next(150, 900);
                        int newEnemyLeftMargin = random.Next(150, 1750);
                        GamePiece newEnemy = CreatePieceWithSpeed("enemy", 90, newEnemyLeftMargin, newEnemyTopMargin, newEnemySpeed);
                        newEnemy.StartEnemyMovement();
                        enemies.Add(newEnemy);
                        currentEnemy++;
                    }
                }
                else
                {
                    if (currentEnemy % 5 == 0)
                    {
                        double newBigEnemySpeed = random.Next(20, 31);
                        int newBigEnemyTopMargin = random.Next(150, 900);
                        int newBigEnemyLeftMargin = random.Next(150, 1750);
                        GamePiece newBigEnemy = CreatePieceWithSpeed("enemy", 150, newBigEnemyLeftMargin, newBigEnemyTopMargin, newBigEnemySpeed);
                        newBigEnemy.StartEnemyMovement();
                        enemies.Add(newBigEnemy);
                        currentEnemy++;
                    }
                    else
                    {
                        double newEnemySpeed = random.Next(35, 51);
                        int newEnemyTopMargin = random.Next(150, 900);
                        int newEnemyLeftMargin = random.Next(150, 1750);
                        GamePiece newEnemy = CreatePieceWithSpeed("enemy", 90, newEnemyLeftMargin, newEnemyTopMargin, newEnemySpeed);
                        newEnemy.StartEnemyMovement();
                        enemies.Add(newEnemy);
                        currentEnemy++;
                    }
                }
            }
        }

        //Keydown method
        private void CoreWindow_KeyDown(object sender, Windows.UI.Core.KeyEventArgs e)
        {
            // Calculate new location for the player character
            player.PlayerMove(e.VirtualKey);
            pacManImage?.PlayerMove(e.VirtualKey);
            // Update the teleport TextBlock
            jumpText.Text = $"Teleport: {player.Jump}";
        }

        //Collision action
        private void CollisionCheck_Tick(object sender, object e)
        {
            // Check for collisions between player and collectible
            // Note: this looks for identical locations of the two objects. To be more precise, you can write a method to measure distances
            if (IsCollision(player.Location, dot.Location) || IsCollision(pacManImage.Location, dot.Location)) // If true, then collide action
            {
                if (!isScored) //This is to make sure even when both of my player images collide with the dot, only 1 action is done, not both
                {
                    collisionTimer.Stop();
                    Score += 50;
                    var existedPlayerRecord = playerRecords.Find(x => x.Name == PlayerName);
                    if (existedPlayerRecord != null)
                    {
                        if (Score > existedPlayerRecord.HighScore)
                        {
                            playerHighestScore = Score;
                        }
                    }
                    else
                    {
                        playerHighestScore = Score;
                    }
                    ScoreToWin -= 50;
                    if (ScoreToWin < 0)
                    {
                        ScoreToWin = 0;
                    }
                    WhiteDotCount++;
                    if (WhiteDotCount > 5) // Spawn one red dot after 5 white dots, increase player size, decrease ms
                    {
                        Inmunity++;
                        WhiteDotCount = 0;
                        if (Difficulty != "Easy")
                        {
                            player.playerMovingSpeed--;
                            player.onScreen.Width += 15;
                            player.onScreen.Height += 15;
                            pacManImage.playerMovingSpeed--;
                            pacManImage.onScreen.Width += 15;
                            pacManImage.onScreen.Height += 15;
                        }
                        if (spawnRedDot == false)
                        {
                            spawnRedDot = true;
                        }
                        EnemySpawnHandler();
                        if (spawnRedDot == true) // This is to avoid multiple red dots on the screen
                        {
                            int newRedDotTopMargin = random.Next(80, 900);
                            int newRedDotLeftMargin = random.Next(80, 1750);
                            redDot = CreatePiece("redDot", 60, newRedDotLeftMargin, newRedDotTopMargin);
                        }
                    }
                    gridMain.Children.Remove(dot.onScreen); // Remove the existing dot
                    isScored = true;

                    // Add a new dot
                    int newDotTopMargin = random.Next(80, 900);
                    int newDotLeftMargin = random.Next(80, 1750);
                    dot = CreatePiece("dot", 30, newDotLeftMargin, newDotTopMargin);
                    collisionTimer.Start();

                    // Update the score TextBlock
                    scoreText.Text = $"Score: {Score}";

                    // Update the winning score TextBlock
                    winningScore.Text = $"Score to win: {ScoreToWin}";

                    // Update the inmunity TextBlock
                    inmunityText.Text = $"Inmunity: {Inmunity}";

                    // Update the teleport TextBlock
                    jumpText.Text = $"Teleport: {player.Jump}";

                    //Update the highestscore textblock
                    highestText.Text = $"Highest Score: {playerHighestScore}";

                    if (ScoreToWin <= 0 && FreeRoamMode == false)
                    {
                        Timeout.Stop();
                        enemySpawnTimer.Stop();
                        animationTimer.Stop();
                        foreach (var em in enemies.ToList())
                        {
                            em.StopEnemyMovement();
                        }
                        player.StopPlayerMovement();
                        pacManImage.StopPlayerMovement();
                        btnKeepPlaying.Visibility = Visibility.Visible;
                        collisionTimer.Stop();
                        new MessageDialog($"Congratulations!!! You won").ShowAsync();
                        CalculateHighScore();
                    }
                }
            }
            else
            {
                isScored = false;
            }
            if (spawnRedDot == true)
            {
                if (IsRedDotCollision(player.Location, redDot.Location) || IsRedDotCollision(pacManImage.Location, redDot.Location)) // If true, then collide action
                {
                    if (!isScoredRed) //This is to make sure even when both of my player images collide with the red dot, only 1 action is done, not both
                    {
                        collisionTimer.Stop();
                        Score += 125;
                        ScoreToWin -= 125;
                        var existedPlayerRecord = playerRecords.Find(x => x.Name == PlayerName);
                        if (existedPlayerRecord != null)
                        {
                            if (Score > existedPlayerRecord.HighScore)
                            {
                                playerHighestScore = Score;
                            }
                        }
                        else
                        {
                            playerHighestScore = Score;
                        }
                        if (ScoreToWin < 0)
                        {
                            ScoreToWin = 0;
                        }
                        player.Jump++;
                        pacManImage.Jump++;
                        gridMain.Children.Remove(redDot.onScreen); // Remove the existing dot
                        spawnRedDot = false;
                        isScoredRed = true;
                        collisionTimer.Start();

                        // Update the score TextBlock
                        scoreText.Text = $"Score: {Score}";

                        // Update the winning score TextBlock
                        winningScore.Text = $"Score to win: {ScoreToWin}";

                        // Update the inmunity TextBlock
                        inmunityText.Text = $"Inmunity: {Inmunity}";

                        // Update the teleport TextBlock
                        jumpText.Text = $"Teleport: {player.Jump}";

                        //Update the highestscore textblock
                        highestText.Text = $"Highest Score: {playerHighestScore}";

                        if (ScoreToWin <= 0 && FreeRoamMode == false)
                        {
                            Timeout.Stop();
                            enemySpawnTimer.Stop();
                            animationTimer.Stop();
                            foreach (var em in enemies.ToList())
                            {
                                em.StopEnemyMovement();
                            }
                            player.StopPlayerMovement();
                            pacManImage.StopPlayerMovement();
                            btnKeepPlaying.Visibility = Visibility.Visible;
                            collisionTimer.Stop();
                            new MessageDialog($"Congratulations!!! You won").ShowAsync();
                            CalculateHighScore();
                        }
                    }
                }
                else
                {
                    isScoredRed = false;
                }
            }
            foreach (var enemy in enemies.ToList()) //Check every new enemies added to the list for the separated collision actions handle
            {
                bool isBigEnemy = enemy.onScreen.Width == 150;
                if (isBigEnemy)
                {
                    if (IsBigEnemyCollision(player.Location, enemy.Location) || IsBigEnemyCollision(pacManImage.Location, enemy.Location)) // If true, then collide action
                    {
                        if (!isTouched) //This is to make sure even when both of my player images collide with the enemy, only 1 action is done, not both
                        {
                            collisionTimer.Stop();
                            if (Inmunity > 0)
                            {
                                Inmunity--;
                                Score += 200;
                                ScoreToWin -= 200;
                                var existedPlayerRecord = playerRecords.Find(x => x.Name == PlayerName);
                                if (existedPlayerRecord != null)
                                {
                                    if (Score > existedPlayerRecord.HighScore)
                                    {
                                        playerHighestScore = Score;
                                    }
                                }
                                else
                                {
                                    playerHighestScore = Score;
                                }
                                if (ScoreToWin < 0)
                                {
                                    ScoreToWin = 0;
                                }
                                enemy.StopEnemyMovement();
                                gridMain.Children.Remove(enemy.onScreen);
                                enemies.Remove(enemy);
                                currentEnemy--;

                                // Update the score TextBlock
                                scoreText.Text = $"Score: {Score}";

                                // Update the winning score TextBlock
                                winningScore.Text = $"Score to win: {ScoreToWin}";

                                // Update the inmunity TextBlock
                                inmunityText.Text = $"Inmunity: {Inmunity}";

                                // Update the teleport TextBlock
                                jumpText.Text = $"Teleport: {player.Jump}";

                                //Update the highestscore textblock
                                highestText.Text = $"Highest Score: {playerHighestScore}";

                                isTouched = true;

                                if (ScoreToWin <= 0 && FreeRoamMode == false)
                                {
                                    Timeout.Stop();
                                    enemySpawnTimer.Stop();
                                    animationTimer.Stop();
                                    foreach (var em in enemies.ToList())
                                    {
                                        em.StopEnemyMovement();
                                    }
                                    player.StopPlayerMovement();
                                    pacManImage.StopPlayerMovement();
                                    btnKeepPlaying.Visibility = Visibility.Visible;
                                    collisionTimer.Stop();
                                    new MessageDialog($"Congratulations!!! You won").ShowAsync();
                                    CalculateHighScore();
                                    break;
                                }
                            }
                            else
                            {
                                inmunityText.Text = $"Inmunity: 0";
                                Timeout.Stop();
                                btnKeepPlaying.Visibility = Visibility.Collapsed;
                                gridMain.Children.Remove(player.onScreen);
                                gridMain.Children.Remove(pacManImage.onScreen);
                                collisionTimer.Stop();
                                isTouched = true;
                                new MessageDialog($"Too Bad!!! Try harder").ShowAsync();
                                CalculateHighScore();
                                string youtubeUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
                                var uri = new Uri(youtubeUrl);
                                Windows.System.Launcher.LaunchUriAsync(uri);
                                break;
                            }
                            collisionTimer.Start();
                        }
                        else
                        {
                            isTouched = false;
                        }
                    }
                }
                else
                {
                    if (IsEnemyCollision(player.Location, enemy.Location) || IsEnemyCollision(pacManImage.Location, enemy.Location)) // If true, then collide action
                    {
                        if (!isTouched) //This is to make sure even when both of my player images collide with the enemy, only 1 action is done, not both
                        {
                            collisionTimer.Stop();
                            if (Inmunity > 0)
                            {
                                Inmunity--;
                                Score += 200;
                                ScoreToWin -= 200;
                                var existedPlayerRecord = playerRecords.Find(x => x.Name == PlayerName);
                                if (existedPlayerRecord != null)
                                {
                                    if (Score > existedPlayerRecord.HighScore)
                                    {
                                        playerHighestScore = Score;
                                    }
                                }
                                else
                                {
                                    playerHighestScore = Score;
                                }
                                if (ScoreToWin < 0)
                                {
                                    ScoreToWin = 0;
                                }
                                enemy.StopEnemyMovement();
                                gridMain.Children.Remove(enemy.onScreen);
                                enemies.Remove(enemy);
                                currentEnemy--;

                                // Update the score TextBlock
                                scoreText.Text = $"Score: {Score}";

                                // Update the winning score TextBlock
                                winningScore.Text = $"Score to win: {ScoreToWin}";

                                // Update the inmunity TextBlock
                                inmunityText.Text = $"Inmunity: {Inmunity}";

                                // Update the teleport TextBlock
                                jumpText.Text = $"Teleport: {player.Jump}";

                                //Update the highestscore textblock
                                highestText.Text = $"Highest Score: {playerHighestScore}";

                                isTouched = true;

                                if (ScoreToWin <= 0 && FreeRoamMode == false)
                                {
                                    Timeout.Stop();
                                    enemySpawnTimer.Stop();
                                    animationTimer.Stop();
                                    foreach (var em in enemies.ToList())
                                    {
                                        em.StopEnemyMovement();
                                    }
                                    player.StopPlayerMovement();
                                    pacManImage.StopPlayerMovement();
                                    btnKeepPlaying.Visibility = Visibility.Visible;
                                    collisionTimer.Stop();
                                    new MessageDialog($"Congratulations!!! You won").ShowAsync();
                                    CalculateHighScore();
                                    break;
                                }
                            }
                            else
                            {
                                inmunityText.Text = $"Inmunity: 0";
                                Timeout.Stop();
                                btnKeepPlaying.Visibility = Visibility.Collapsed;
                                gridMain.Children.Remove(player.onScreen);
                                gridMain.Children.Remove(pacManImage.onScreen);
                                collisionTimer.Stop();
                                isTouched = true;
                                new MessageDialog($"Too Bad!!! Try harder").ShowAsync();
                                CalculateHighScore();
                                string youtubeUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
                                var uri = new Uri(youtubeUrl);
                                Windows.System.Launcher.LaunchUriAsync(uri);
                                break;
                            }
                            collisionTimer.Start();
                        }
                        else
                        {
                            isTouched = false;
                        }
                    }
                }
            }
        }

        //Rewrite, Update Highscore and display on the screen
        private async void CalculateHighScore()
        {
            IList<string> lines = await FileIO.ReadLinesAsync(storageFile);
            playerRecords = new List<PlayerRecord>();

            if (lines.Count > 0)
            {
                //Retrieve the player records from the text file based on formatted regex
                foreach (var line in lines)
                {
                    var match = Regex.Match(line, @"{playername: ""(.+)"", highscore: (\d+)}");

                    if (match.Success)
                    {
                        string playerName = match.Groups[1].Value;
                        int highScore = int.Parse(match.Groups[2].Value);

                        // Create a new Player instance
                        PlayerRecord playerrecord = new PlayerRecord(playerName, highScore);
                        if (!playerRecords.Contains(playerrecord))
                        {
                            playerRecords.Add(playerrecord);
                        }
                    }
                }
            }
            else
            {
                PlayerRecord newplayer = new PlayerRecord(PlayerName, Score);
                await FileIO.WriteTextAsync(storageFile, newplayer.ToString());
                if (!playerRecords.Contains(newplayer))
                {
                    playerRecords.Add(newplayer);
                }
            }

            List<string> updatedLines = new List<string>();

            if (playerRecords.Count > 0)
            {
                var existedRecord = playerRecords.Find(p => p.Name == PlayerName);
                if (existedRecord != null)
                {
                    if (Score > existedRecord.HighScore)
                    {
                        existedRecord.HighScore = Score;
                    }
                }
                else
                {
                    PlayerRecord newplayer = new PlayerRecord(PlayerName, Score);
                    playerRecords.Add(newplayer);
                }
            }

            var top10HighestScore = playerRecords.OrderByDescending(p => p.HighScore).Take(10).ToList();

            foreach (var record in top10HighestScore)
            {
                updatedLines.Add(record.ToString());
            }


            await FileIO.WriteLinesAsync(storageFile, updatedLines);

            lines = await FileIO.ReadLinesAsync(storageFile);
            listView.ItemsSource = lines;

            HighestScoreDialogPopup.IsOpen = true;
        }

        private void InitializeCollisionCheck() //Add collision timer
        {
            //DispatcherTimer for collision
            collisionTimer = new DispatcherTimer();
            collisionTimer.Interval = TimeSpan.FromSeconds(0.05);
            collisionTimer.Tick += CollisionCheck_Tick;
            collisionTimer.Start();
        }

        //Real collision logic handler
        private bool IsCollision(Thickness object1Margins, Thickness object2Margins) //Collide action takes 2 img margins, took so much time on this
        {
            // Calculate the average of magin bot and top and the radius of the circular objects
            double AVGX1 = object1Margins.Left + player.onScreen.Width / 2;
            double AVGY1 = object1Margins.Top + player.onScreen.Height / 2;
            double radius1 = player.onScreen.Width / 2; //Get the radius

            double AVGX2 = object2Margins.Left + dot.onScreen.Width / 2;
            double AVGY2 = object2Margins.Top + dot.onScreen.Height / 2;
            double radius2 = dot.onScreen.Width / 2;

            // Calculate the distance of 2 circular objs, got some helps from the internet
            double distance = Math.Sqrt(Math.Pow(AVGX2 - AVGX1, 2) + Math.Pow(AVGY2 - AVGY1, 2));

            // Check if the distance is less than the sum of the radii for a collision
            if (distance < radius1 + radius2)
            {
                return true; // Collision detected
            }

            return false; // No collision
        }

        //Real collision logic handler
        private bool IsRedDotCollision(Thickness object1Margins, Thickness object2Margins) //Collide action takes 2 img margins, took so much time on this
        {
            // Calculate the average of magin bot and top and the radius of the circular objects
            double AVGX1 = object1Margins.Left + player.onScreen.Width / 2;
            double AVGY1 = object1Margins.Top + player.onScreen.Height / 2;
            double radius1 = player.onScreen.Width / 2; //Get the radius

            double AVGX2 = object2Margins.Left + redDot.onScreen.Width / 2;
            double AVGY2 = object2Margins.Top + redDot.onScreen.Height / 2;
            double radius2 = redDot.onScreen.Width / 2;

            // Calculate the distance of 2 circular objs, got some helps from the internet
            double distance = Math.Sqrt(Math.Pow(AVGX2 - AVGX1, 2) + Math.Pow(AVGY2 - AVGY1, 2));

            // Check if the distance is less than the sum of the radii for a collision
            if (distance < radius1 + radius2)
            {
                return true; // Collision detected
            }

            return false; // No collision
        }

        private bool IsEnemyCollision(Thickness object1Margins, Thickness object2Margins)
        {
            // Calculate the average and radius for the player
            double AVGX1 = object1Margins.Left + player.onScreen.Width / 2;
            double AVGY1 = object1Margins.Top + player.onScreen.Height / 2;
            double radius1 = Math.Min(player.onScreen.Width, player.onScreen.Height) / 2; // Use the minimum dimension as radius

            // Calculate the average and radius for the enemy
            double AVGX2 = object2Margins.Left + enemy.onScreen.Width / 2;
            double AVGY2 = object2Margins.Top + enemy.onScreen.Height / 2;
            double radius2 = Math.Min(enemy.onScreen.Width, enemy.onScreen.Height) / 2; // Use the minimum dimension as radius

            // Calculate the distance between the player and the enemy
            double distance = Math.Sqrt(Math.Pow(AVGX2 - AVGX1, 2) + Math.Pow(AVGY2 - AVGY1, 2));

            // Check if the distance is less than the sum of the radii for a collision
            if (distance < radius1 + radius2)
            {
                return true; // Collision detected
            }

            return false; // No collision
        }

        private bool IsBigEnemyCollision(Thickness object1Margins, Thickness object2Margins)
        {
            // Calculate the average and radius for the player
            double AVGX1 = object1Margins.Left + player.onScreen.Width / 2;
            double AVGY1 = object1Margins.Top + player.onScreen.Height / 2;
            double radius1 = Math.Min(player.onScreen.Width, player.onScreen.Height) / 2; // Use the minimum dimension as radius

            // Calculate the average and radius for the enemy
            double AVGX2 = object2Margins.Left + enemy.onScreen.Width / 2;
            double AVGY2 = object2Margins.Top + enemy.onScreen.Height / 2;
            double radius2 = 75; // Use the minimum dimension as radius

            // Calculate the distance between the player and the enemy
            double distance = Math.Sqrt(Math.Pow(AVGX2 - AVGX1, 2) + Math.Pow(AVGY2 - AVGY1, 2));

            // Check if the distance is less than the sum of the radii for a collision
            if (distance < radius1 + radius2)
            {
                return true; // Collision detected
            }

            return false; // No collision
        }

    }
}
