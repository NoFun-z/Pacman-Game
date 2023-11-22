using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace GameLibrary
{
    public class GamePiece
    {
        public Thickness objectMargins;
        public Image onScreen;
        private RotateTransform rotateTransform; //Rotate option for img
        public ScaleTransform scaleTransform = new ScaleTransform(); //Flipping option for img
        private Grid gridMain; //The grid from the mainpage, this means if we change the grid w and h from the xaml page, this will reflect everything
        public Thickness Location
        {
            get { return onScreen.Margin; }
        }
        public static Random random = new Random(); //Randomize options
        public int direction;
        public int playerDirection = 4; //The player img will always go from the left to the right
        private double movingDistance_; //This is enemy movement speed
        private DispatcherTimer directionChangeTimer;
        private DispatcherTimer moveTimer;
        public int Jump; //The number of teleporting that the player can do
        public double playerMovingSpeed = 26; //Player mvement speed

        //Different constructors
        public GamePiece(Image img, Grid grid)
        {
            onScreen = img;
            objectMargins = img.Margin;
            rotateTransform = new RotateTransform();
            onScreen.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
            onScreen.RenderTransform = rotateTransform;
            gridMain = grid;
            if (onScreen.RenderTransform is ScaleTransform)
            {
                scaleTransform.ScaleX = 1;
                onScreen.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
                onScreen.RenderTransform = scaleTransform;
            }
        }

        public GamePiece(Image img, double movingDistance, Grid grid)
        {
            onScreen = img;
            objectMargins = img.Margin;
            rotateTransform = new RotateTransform();
            onScreen.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
            onScreen.RenderTransform = rotateTransform;
            gridMain = grid;
            if (onScreen.RenderTransform is ScaleTransform)
            {
                scaleTransform.ScaleX = 1;
                onScreen.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
                onScreen.RenderTransform = scaleTransform;
            }
            movingDistance_ = movingDistance;
        }

        public GamePiece(Image img, int jump, int ms, Grid grid)
        {
            onScreen = img;
            objectMargins = img.Margin;
            rotateTransform = new RotateTransform();
            onScreen.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
            onScreen.RenderTransform = rotateTransform;
            Jump = jump;
            playerMovingSpeed = ms;
            gridMain = grid;
            if (onScreen.RenderTransform is ScaleTransform)
            {
                scaleTransform.ScaleX = 1;
                onScreen.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
                onScreen.RenderTransform = scaleTransform;
            }
        }

        //This to change the visibility of both player img from the main page to reflect the animation
        public void SetVisibility(Visibility visibility)
        {
            onScreen.Visibility = visibility;
        }

        public void StartPlayerMovement()
        {
            InitializePlayerMoveTimer();
        }

        public void StopPlayerMovement()
        {
            moveTimer?.Stop();
            moveTimer = null;
        }

        //Player movement timer
        private void InitializePlayerMoveTimer() //Move Interval for infinite movement of the enemy
        {
            moveTimer = new DispatcherTimer();
            moveTimer.Interval = TimeSpan.FromMilliseconds(70);
            moveTimer.Tick += PlayerMoveTimer_Tick;
            moveTimer.Start();
        }

        //Player movement action
        private void PlayerMoveTimer_Tick(object sender, object e)
        {
            Move();
        }

        //Actual player movement action
        public bool Move()
        {
            switch (playerDirection)
            {
                case 1:
                    objectMargins.Top -= playerMovingSpeed;
                    break;
                case 2:
                    objectMargins.Top += playerMovingSpeed;
                    break;
                case 3:
                    objectMargins.Left -= playerMovingSpeed;
                    break;
                case 4:
                    objectMargins.Left += playerMovingSpeed;
                    break;
                default:
                    return false;
            }

            HandleScreenEdgeCollisionPlayer();

            onScreen.Margin = objectMargins;
            return true;
        }

        //Player direction handler
        public bool PlayerMove(Windows.System.VirtualKey direction)
        {

            switch (direction)
            {
                case Windows.System.VirtualKey.Up:
                    onScreen.RenderTransform = rotateTransform;
                    rotateTransform.Angle = -90;
                    playerDirection = 1;
                    break;
                case Windows.System.VirtualKey.Down:
                    onScreen.RenderTransform = rotateTransform;
                    rotateTransform.Angle = 90;
                    playerDirection = 2;
                    break;
                case Windows.System.VirtualKey.Left:
                    onScreen.RenderTransform = scaleTransform;
                    ((ScaleTransform)onScreen.RenderTransform).ScaleX = -1;
                    playerDirection = 3;
                    break;
                case Windows.System.VirtualKey.Right:
                    onScreen.RenderTransform = scaleTransform;
                    ((ScaleTransform)onScreen.RenderTransform).ScaleX = 1;
                    playerDirection = 4;
                    break;
                case Windows.System.VirtualKey.Space:
                    if(Jump > 0)
                    {
                        if (playerDirection == 1)
                        {
                            objectMargins.Top -= 400;
                        }
                        else if (playerDirection == 2)
                        {
                            objectMargins.Top += 400;
                        }
                        else if (playerDirection == 3)
                        {
                            objectMargins.Left -= 400;
                        }
                        else if (playerDirection == 4)
                        {
                            objectMargins.Left += 400;
                        }
                        Jump--;
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }

        public void StartEnemyMovement()
        {
            InitializeDirectionChangeTimer();
            InitializeEnemyMoveTimer();
        }

        public void StopEnemyMovement()
        {
            directionChangeTimer?.Stop();
            directionChangeTimer = null;

            moveTimer?.Stop();
            moveTimer = null;
        }

        //Auto-change the direction of the enemy every 3 seconds
        private void InitializeDirectionChangeTimer() //Direction change Interval for infinite enemy direction change
        {
            directionChangeTimer = new DispatcherTimer();
            directionChangeTimer.Interval = TimeSpan.FromSeconds(3); //Only start to move after 3s when spawned and change direction every 3s
            directionChangeTimer.Tick += DirectionChangeTimer_Tick;
            directionChangeTimer.Start();
        }

        //Enemy direction change handler
        private void DirectionChangeTimer_Tick(object sender, object e)
        {
            ChangeEnemyDirection();
        }

        //Randomizing the direction of the enemy (1 means north, 2 means south, 3 means west and 4 means east)
        private void ChangeEnemyDirection()
        {
            direction = random.Next(1, 5);
        }

        //Enemy movement handler, start contuniously every 0.05s
        private void InitializeEnemyMoveTimer() //Move Interval for infinite movement of the enemy
        {
            moveTimer = new DispatcherTimer();
            moveTimer.Interval = TimeSpan.FromMilliseconds(60);
            moveTimer.Tick += EnemyMoveTimer_Tick;
            moveTimer.Start();
        }

        //The contunious enemy movement action
        private void EnemyMoveTimer_Tick(object sender, object e)
        {
            MoveEnemy();
        }

        //Contunious enemy movement action handler
        public bool MoveEnemy()
        {
            switch (direction)
            {
                case 1: // Going up
                    objectMargins.Top -= movingDistance_;
                    break;
                case 2: // Going down
                    objectMargins.Top += movingDistance_;
                    break;
                case 3: // Going left
                    onScreen.RenderTransform = scaleTransform;
                    ((ScaleTransform)onScreen.RenderTransform).ScaleX = -1;
                    objectMargins.Left -= movingDistance_;
                    break;
                case 4: // Going right
                    onScreen.RenderTransform = scaleTransform;
                    ((ScaleTransform)onScreen.RenderTransform).ScaleX = 1;
                    objectMargins.Left += movingDistance_;
                    break;
                default:
                    return false;
            }

            HandleScreenEdgeCollision();

            onScreen.Margin = objectMargins;
            return true;
        }

        //So this is the screen collision handler
        private void HandleScreenEdgeCollisionPlayer()
        {
            if (objectMargins.Left < 0)
            {
                objectMargins.Left = 0;
            }
            else if (objectMargins.Left + onScreen.Width > gridMain.Width) 
            {
                objectMargins.Left = gridMain.Width - onScreen.Width;
            }

            if (objectMargins.Top < 60)
            {
                objectMargins.Top = 60;
            }
            else if (objectMargins.Top + onScreen.Height > gridMain.Height - 120)
            {
                objectMargins.Top = gridMain.Height - onScreen.Height - 120;
            }
        }

        //So this is the screen collision handler
        private void HandleScreenEdgeCollision() // For enemy
        {
            if (objectMargins.Left < 0) //If the enemy hit the left edge, then the next direction it goes will be randomized except for going west again
            {
                objectMargins.Left = 0;
                int[] desiredValues = { 1, 2, 4 };
                ChangeDirectionUntilDesiredValues(desiredValues);
            }
            else if (objectMargins.Left + onScreen.Width > gridMain.Width) //If the enemy hit the right edge, then the next direction it goes will not be eastagain
            {
                objectMargins.Left = gridMain.Width - onScreen.Width;
                int[] desiredValues = { 1, 2, 3 };
                ChangeDirectionUntilDesiredValues(desiredValues);
            }

            if (objectMargins.Top < 60) //If the enemy hit the top edge, then the next direction it goes will not be north again
            {
                objectMargins.Top = 60;
                int[] desiredValues = { 2, 3, 4 };
                ChangeDirectionUntilDesiredValues(desiredValues);
            }
            else if (objectMargins.Top + onScreen.Height > gridMain.Height - 120) //If the enemy hit the bottom edge, then the next direction it goes will not be south again
            {
                objectMargins.Top = gridMain.Height - onScreen.Height - 120;
                int[] desiredValues = { 1, 3, 4 };
                ChangeDirectionUntilDesiredValues(desiredValues);
            }
        }

        //Direction change handler for screenedge collision
        private void ChangeDirectionUntilDesiredValues(int[] desiredValues)
        {
            do
            {
                direction = random.Next(1, 5);
            } while (!desiredValues.Contains(direction));
        }
    }
}
