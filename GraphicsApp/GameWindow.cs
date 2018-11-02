#region using...
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
#endregion

namespace GraphicsApp
{
    public partial class GameWindow : Form
    {

#region declarations

        public bool inDebug = false;

        public static GameWindow ThisGameWindow;
        public static int fontSize = 12;
        public static Font lblFont;

        const int maxArraySize = 150; // volume of objects on the screen at one time.
        const int desiredFPS   = 40; // speed of the game
        int slidingSpeed = 3; // speed of the background scrolling
        int slidingHeight = 0;

        Bitmap backImg;
        Bitmap scrollingBackgroundImage;
        Graphics backGraphics;
        Graphics actionGraphics;

        Thread playThread;
        public int startingLevelTargetFrequency;
        public int levelTargetFrequency;
        public int minTargetFrequency;
        DateTime lastTargetSpawn;
        DateTime lastFreqencyChange;
        DateTime lastLevelChange;
        public long lvlDuration; 
        DateTime now;
        DateTime lastFrame;
        public int currentLvl;
        public int plot;

        public Actor[] actors;

        bool isUpPressed;
        bool isUpLeftPressed;
        bool isUpRightPressed;
        bool isLeftPressed;
        bool isRightPressed;
        bool isDownPressed;
        bool isDownLeftPressed;
        bool isDownRightPressed;
        bool isSpacePressed;

        public bool inSession;
        public bool gameOver;
        public bool isPaused;
        public bool isBossPresent;
        public bool gameWon;
#endregion

#region constructor
        public GameWindow()
        {
            InitializeComponent();
            Res.setupRes();

            backImg = new Bitmap(ActionPanel.Width, ActionPanel.Height);
            backGraphics = Graphics.FromImage(backImg);
            actionGraphics = ActionPanel.CreateGraphics();
            lblFont = new Font(ActionPanel.Font.FontFamily, fontSize, FontStyle.Bold);
            scrollingBackgroundImage = Res.getPic("back_water.gif");
            slidingHeight = -scrollingBackgroundImage.Height;
            ThisGameWindow = this;
            actors = new Actor[maxArraySize];
        }
        private void GameWindow_Shown(object sender, EventArgs e)
        {
            PopMenu();
        }
#endregion

#region main game methods
        private void CloseCurrentSession()
        {
            if (inSession == true)
            {
                inSession = false;

                levelTargetFrequency = startingLevelTargetFrequency;
                lastFreqencyChange   = System.DateTime.Now;
                lastLevelChange = System.DateTime.Now;
                now = System.DateTime.Now;
                currentLvl = 0;
                plot = 0;
                UpdateLvlInfo();

                playThread.Join();
                for (int i = 0; i < actors.Length; i++)
                {
                    if (actors[i] != null)
                    {
                        actors[i].deleteMe = true;
                        actors[i] = null; // this might not work...
                    } // end if
                } // end for
            } // end if inSession
        }
        private void newSession()
        {
            if (inSession)
            {
                CloseCurrentSession();
            }

            inSession = true;
            isPaused= false;
            gameOver = false;
            gameWon = false;
            isBossPresent = false;
            lastTargetSpawn = System.DateTime.Now;
            lastFreqencyChange   = System.DateTime.Now;
            lastLevelChange = System.DateTime.Now;
            now = System.DateTime.Now;
            lastFrame = System.DateTime.Now;

            startingLevelTargetFrequency = 700;
            levelTargetFrequency = 700; // ms between target spawn.
            minTargetFrequency = 200;
            lvlDuration = 60 * 1000; // 1 min

            currentLvl = 0;
            plot = 0;

            resetLevel();

            // setup session variables (lvls, player, etc.)
            actors = new Actor[maxArraySize];

            // setup player
            actors[0] = new Actor(ActionPanel, "ship.gif");

            // temporary for testing.
            //actors[0].health = 50;
            //actors[0].laser = 5;

            Random rnd = new Random();

            // begin session
            playThread = new Thread(new ThreadStart(play));
            playThread.Start();

        } // end new Session
        private void play()
        {
            while (inSession)
            {
                if (isPaused == false && gameOver == false)
                {
                    // update game info
                    UpdateLvlInfo();

                    // update info of all actors
                    for (int i = 0; i < actors.Length; i++)
                    {
                        if (actors[i] != null)
                        {
                            if (actors[i].deleteMe)
                            {
                                actors[i] = null;
                            }
                            else
                            {
                                actors[i].update();
                                actors[i].CheckObjectCollision(actors);
                            }
                        }
                    } // end foreach
                } // end if not paused

                // redraw game field
                redrawAction();

                // sleep until next update
                int ms = System.DateTime.Now.Millisecond;
                if (ms - lastFrame.Millisecond < 0)
                {
                    ms += 1000;
                }
                System.Threading.Thread.Sleep(Math.Max(0, 
                    (int)(1000/desiredFPS) - (ms - lastFrame.Millisecond)));
                lastFrame = System.DateTime.Now;
            } // end while in session
        }
#endregion

#region Game processing methods
        private void UpdateLvlInfo() 
        {
            if (plot == 1 ||
                plot == 3 ||
                plot == 5 ||
                plot == 7)
            {
                // check for duration of level,
                if (lastLevelChange.AddMilliseconds(lvlDuration).
                              CompareTo(DateTime.Now) > 0)
                {
                    // during this level...

                    // check for duration of frequency spawn
                    if (lastFreqencyChange.AddMilliseconds(lvlDuration / 10).
                              CompareTo(DateTime.Now) < 0)
                    {
                        levelTargetFrequency = Math.Max(minTargetFrequency, levelTargetFrequency - 50);
                        lastFreqencyChange = DateTime.Now;
                    } // end if speed up
                    else
                    {
                        // then don't change frequency, just spawn
                        // if within time, check for spawn enemy
                        checkToSpawnLeveledTarget();
                    } // end don't speed up
                } // end if same level
                else
                {
                    // else, increment plot.
                     plot += 1;
                }// end if level ended
            } // end if plot is mid-level

            else if (plot == 2 ||
                     plot == 4 ||
                     plot == 6 ||
                     plot == 8)
             {
                // if boss exists, just keep going,
                if (isBossPresent) { return; }

                // if no boss yet but on this plot number, then spawn boss.
                else
                {
                    actors[GetFirstOpenIndex(actors)] = 
                        new Boss(ActionPanel, currentLvl);
                    isBossPresent = true;
                } // end if no boss
            } // end if plot is end-level

            else if (plot >= 9) 
            {
                // we have reached the end of the game, just reset everything for now. 
                if (gameWon == false)
                {
                    gameWon = true;
                }
            }
        }
        private void checkToSpawnLeveledTarget()
        {
            // only spawn target every so many milliseconds
            if (lastTargetSpawn.AddMilliseconds(levelTargetFrequency).CompareTo(DateTime.Now) < 0)
            {
                actors[GetFirstOpenIndex(actors)] = new Target(ActionPanel, currentLvl);
                lastTargetSpawn = DateTime.Now;
            }
        }
        private void redrawAction()
        {
            if (this.inSession)
            {
                // draw background
                drawScrollingBackground(backGraphics);

                #region Draw Messages
                if (isPaused)
                {
                    backGraphics.DrawString("Paused", new Font(ActionPanel.Font.FontFamily, 50),
                        Brushes.Red, (ActionPanel.Width / 2) - 130, (ActionPanel.Height / 2) - 100);
                }
                else if (gameOver)
                {
                    backGraphics.DrawString("Game Over", new Font(ActionPanel.Font.FontFamily, 50),
                        Brushes.Black, (ActionPanel.Width / 2) - 200, (ActionPanel.Height / 2) - 100);
                }
                else if (gameWon)
                {
                    backGraphics.DrawString("Victory!", new Font(ActionPanel.Font.FontFamily, 50),
                        Brushes.DarkGreen, (ActionPanel.Width / 2) - 120, (ActionPanel.Height / 2) - 150);

                    int s = actors[0].score;
                    if (s < 100)
                    {
                        backGraphics.DrawString("Total: " + s, new Font(ActionPanel.Font.FontFamily, 30),
                            Brushes.DarkGreen, (ActionPanel.Width / 2) - 20, (ActionPanel.Height / 2));
                    }
                    else if (s < 1000)
                    {
                        backGraphics.DrawString("Total: " + s, new Font(ActionPanel.Font.FontFamily, 30),
                            Brushes.DarkGreen, (ActionPanel.Width / 2) - 80, (ActionPanel.Height / 2));
                    }
                    else
                    {
                        backGraphics.DrawString("Total: " + s, new Font(ActionPanel.Font.FontFamily, 30),
                            Brushes.DarkGreen, (ActionPanel.Width / 2) - 130, (ActionPanel.Height / 2));
                    }

                }
                #endregion
                else
                {
                    // draw all actors
                    foreach (Actor actor in actors)
                    {
                        if (actor != null && actor.deleteMe == false)
                        {
                            actor.drawSelf(backGraphics);
                        }
                    }
                }
                #region draw score block
                // score
                backGraphics.DrawString("Score:  " + actors[0].score,
                    GameWindow.lblFont,
                    Brushes.White,
                    ActionPanel.Width - 115, 10);

                // draw lives
                if (actors[0].numLives > -1)
                {
                    backGraphics.DrawString("lives:  " + actors[0].numLives,
                        GameWindow.lblFont,
                        Brushes.White,
                        ActionPanel.Width - 115, 30);
                }
                else
                {
                    backGraphics.DrawString("lives:     -",
                        GameWindow.lblFont,
                        Brushes.White,
                        ActionPanel.Width - 115, 30);
                }

                // draw health
                backGraphics.DrawRectangle(Pens.White, ActionPanel.Width - 111, 49, 91, 11);
                if (actors[0].health > actors[0].currentMaxHealth)
                {
                    backGraphics.FillRectangle(Brushes.Green, ActionPanel.Width - 110, 50, 90, 10);
                }
                else if (actors[0].health == actors[0].currentMaxHealth)
                {
                    backGraphics.FillRectangle(Brushes.Red, ActionPanel.Width - 110, 50, 90, 10);
                }
                else if (actors[0].health != actors[0].currentMaxHealth)
                {
                    backGraphics.FillRectangle(Brushes.Red, ActionPanel.Width - 110, 50,
                        (90 - ((actors[0].currentMaxHealth - actors[0].health) * 90) / actors[0].currentMaxHealth), 10);
                }
                #endregion

                // transfer the back image to the actionGraphics
                try
                {
                    actionGraphics.DrawImage(backImg, 0, 0);
                }
                catch (Exception e) { e.ToString(); }
            } // end if inSession
        } // end redrawAction
        private void drawScrollingBackground(Graphics backGraphics)
        {
            // slide the screen down.
            slidingHeight += slidingSpeed;
            if (slidingHeight > 0) {
                slidingHeight -= scrollingBackgroundImage.Height;
            }

            // loop through the screen area and draw.
            for (int i = 0; i <= this.Width; i += scrollingBackgroundImage.Width)
            {
                for (int j = slidingHeight; j <= this.Height; j += scrollingBackgroundImage.Height)
                {
                    backGraphics.DrawImage(scrollingBackgroundImage, i, j);
                } // end j for loop
            } // end i for loop
        }
        private void HandleShoot()
        {
            // user has chosen to shoot primary weapon, fire laser.
            if (inSession == false){ return;}
            
            if (actors[0].lastShot.AddMilliseconds(actors[0].coolDown).CompareTo(System.DateTime.Now) < 0)
            {
                actors[0].lastShot = System.DateTime.Now;

                if (actors[0].laser == 1)
                {
                    Actor laser = new Laser(ActionPanel, actors[0]);
                    laser.vY = -actors[0].laserSpeed;
                    actors[GetFirstOpenIndex(actors)] = laser;
                }
                else if (actors[0].laser == 2)
                {
                    Actor laser = new Laser(ActionPanel, actors[0]);
                    Actor laser02 = new Laser(ActionPanel, actors[0]);
                    laser.posX -= actors[0].ObjWidth/3;
                    laser.vY = -actors[0].laserSpeed;
                    laser02.posX += actors[0].ObjWidth / 3;
                    laser02.vY = -actors[0].laserSpeed;
                    actors[GetFirstOpenIndex(actors)] = laser;
                    actors[GetFirstOpenIndex(actors)] = laser02;
                }
                else if (actors[0].laser == 3)
                {
                    Actor laser01 = new Laser(ActionPanel, actors[0]);
                    Actor laser02 = new Laser(ActionPanel, actors[0]);
                    Actor laser03 = new Laser(ActionPanel, actors[0]);

                    laser01.posX = actors[0].posX;
                    laser02.posX = actors[0].posX + actors[0].ObjWidth / 2;
                    laser03.posX = actors[0].posX + actors[0].ObjWidth;

                    laser01.vX = -3;
                    laser03.vX = 3;

                    laser01.vY = -actors[0].laserSpeed;
                    laser02.vY = -actors[0].laserSpeed;
                    laser03.vY = -actors[0].laserSpeed;

                    actors[GetFirstOpenIndex(actors)] = laser01;
                    actors[GetFirstOpenIndex(actors)] = laser02;
                    actors[GetFirstOpenIndex(actors)] = laser03;
                }
                else if (actors[0].laser == 4)
                {
                    Actor laser01 = new Laser(ActionPanel, actors[0]);
                    Actor laser02 = new Laser(ActionPanel, actors[0]);
                    Actor laser03 = new Laser(ActionPanel, actors[0]);
                    Actor laser04 = new Laser(ActionPanel, actors[0]);

                    laser01.posX = actors[0].posX;
                    laser02.posX = actors[0].posX + actors[0].ObjWidth / 4;
                    laser03.posX = actors[0].posX + actors[0].ObjWidth * 3 / 4;
                    laser04.posX = actors[0].posX + actors[0].ObjWidth;

                    laser01.vX = -4;
                    laser02.vX = -1;
                    laser03.vX = 1;
                    laser04.vX = 4;

                    laser01.vY = -actors[0].laserSpeed;
                    laser02.vY = -actors[0].laserSpeed;
                    laser03.vY = -actors[0].laserSpeed;
                    laser04.vY = -actors[0].laserSpeed;

                    actors[GetFirstOpenIndex(actors)] = laser01;
                    actors[GetFirstOpenIndex(actors)] = laser02;
                    actors[GetFirstOpenIndex(actors)] = laser03;
                    actors[GetFirstOpenIndex(actors)] = laser04;
                }
                else if (actors[0].laser >= 5)
                {
                    Actor laser01 = new Laser(ActionPanel, actors[0]);
                    Actor laser02 = new Laser(ActionPanel, actors[0]);
                    Actor laser03 = new Laser(ActionPanel, actors[0]);
                    Actor laser04 = new Laser(ActionPanel, actors[0]);
                    Actor laser05 = new Laser(ActionPanel, actors[0]);

                    laser01.posX = actors[0].posX;
                    laser02.posX = actors[0].posX + actors[0].ObjWidth / 4;
                    laser05.posX = actors[0].posX + actors[0].ObjWidth / 2;
                    laser03.posX = actors[0].posX + actors[0].ObjWidth * 3 / 4;
                    laser04.posX = actors[0].posX + actors[0].ObjWidth;

                    laser01.vX = -4;
                    laser02.vX = -1;
                    laser03.vX = 1;
                    laser04.vX = 4;
                    laser05.vX = 0;

                    laser01.vY = -actors[0].laserSpeed;
                    laser02.vY = -actors[0].laserSpeed;
                    laser03.vY = -actors[0].laserSpeed;
                    laser04.vY = -actors[0].laserSpeed;
                    laser05.vY = -actors[0].laserSpeed;

                    actors[GetFirstOpenIndex(actors)] = laser01;
                    actors[GetFirstOpenIndex(actors)] = laser02;
                    actors[GetFirstOpenIndex(actors)] = laser03;
                    actors[GetFirstOpenIndex(actors)] = laser04;
                    actors[GetFirstOpenIndex(actors)] = laser05;
                }
            }

        }
        public  void resetLevel() 
        {
            currentLvl += 1;
            plot += 1;
            lastLevelChange = System.DateTime.Now;
            slidingHeight = -scrollingBackgroundImage.Height;
            levelTargetFrequency = startingLevelTargetFrequency;

            if (currentLvl == 1)
            {
                scrollingBackgroundImage = Res.getPic("back_water.gif");
            }
            else if (currentLvl == 2)
            {
                scrollingBackgroundImage = Res.getPic("back_forest.gif");
            }
            else if (currentLvl == 3)
            {
                scrollingBackgroundImage = Res.getPic("back_fire.gif");
            }
            else if (currentLvl == 4)
            {
                scrollingBackgroundImage = Res.getPic("back_space.gif");
            }
            else
            {
                scrollingBackgroundImage = Res.getPic("back_waterfall.gif");
            }

            // play level up sound
            Res.playSound("level.wav");

        }
        public void ClearScreen()
        {
            actionGraphics.FillRectangle(Brushes.DarkGreen, 0, 0, this.Width, this.Height);
        }
#endregion

#region key handlers
        private void HandleKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                PopMenu();
            }

            if (e.KeyCode == Keys.Q) 
            { isUpLeftPressed = true;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.W)
            { isUpPressed = true;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.E)
            { isUpRightPressed = true;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.A)
            { isLeftPressed = true;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.S)
            { isDownPressed = true;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.D)
            { isRightPressed = true;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Z)
            { isDownLeftPressed = true;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.X)
            { isDownPressed = true;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.C)
            { isDownRightPressed = true;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Left)
            { isLeftPressed = true;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Right)
            { isRightPressed = true;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up)
            { isUpPressed = true;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Down)
            { isDownPressed = true; 
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.NumPad7)
            { isUpLeftPressed = true;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.NumPad8)
            { isUpPressed = true;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.NumPad9)
            { isUpRightPressed = true;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.NumPad4)
            { isLeftPressed = true;         
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.NumPad6)
            { isRightPressed = true;       
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.NumPad1)
            { isDownLeftPressed = true;       
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.NumPad2)
            { isDownPressed = true;         
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.NumPad3)
            { isDownRightPressed = true;    
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Space)
            {
                isSpacePressed = true;
                e.Handled = true;
            }

            changeSpeed();
            if (isSpacePressed)
            {
                HandleShoot();
            }
            if (e.Handled == false)
            {
                if (inDebug)
                {
                    Tell("Keycode: '" + e.KeyCode + "' not handled.");
                }
            }            
        } // end HandleKeyDown
        private void HandleKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Q)
            {
                isUpLeftPressed = false;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape){}

            else if (e.KeyCode == Keys.W)
            {
                isUpPressed = false;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.E)
            {
                isUpRightPressed = false;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.A)
            {
                isLeftPressed = false;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.S)
            {
                isDownPressed = false;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.D)
            {
                isRightPressed = false;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Z)
            {
                isDownLeftPressed = false;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.X)
            {
                isDownPressed = false;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.C)
            {
                isDownRightPressed = false;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Left)
            {
                isLeftPressed = false;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Right)
            {
                isRightPressed = false;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                isUpPressed = false;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Down)
            {
                isDownPressed = false;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.NumPad7)
            {
                isUpLeftPressed = false;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.NumPad8)
            {
                isUpPressed = false;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.NumPad9)
            {
                isUpRightPressed = false;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.NumPad4)
            {
                isLeftPressed = false;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.NumPad6)
            {
                isRightPressed = false;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.NumPad1)
            {
                isDownLeftPressed = false;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.NumPad2)
            {
                isDownPressed = false;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.NumPad3)
            {
                isDownRightPressed = false;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Space)
            {
                isSpacePressed = false;
                e.Handled = true;
            }

            changeSpeed();
            if (isSpacePressed)
            {
                HandleShoot();
            }

            if (e.KeyCode == Keys.Space) { e.Handled = true; }

            if (e.Handled == false)
            {
                if (inDebug)
                {
                    Tell("Keycode: '" + e.KeyCode + "' not handled.");
                }
            }
        } // end HandleKeyUp
        private void changeSpeed() 
        {
            if (actors[0] == null){return;}

            if (isLeftPressed || isUpLeftPressed || isDownLeftPressed)
            {
                actors[0].vX = -actors[0].speed;
            } // end if left pressed
            else if(!isLeftPressed && !isUpLeftPressed &&!isDownLeftPressed)
            {                
                actors[0].vX = Math.Max(actors[0].vX, 0);
            }

            if (isRightPressed || isUpRightPressed || isDownRightPressed)
            {
                actors[0].vX = actors[0].speed;
            } // end if right
            else if (!isRightPressed && !isUpRightPressed && !isDownRightPressed)
            {
                actors[0].vX = Math.Min(actors[0].vX, 0);
            }

            if (isUpPressed || isUpLeftPressed || isUpRightPressed)
            {
                actors[0].vY = -actors[0].speed;
            } // end if up
            else if(!isUpPressed && !isUpLeftPressed && !isUpRightPressed)
            {
                actors[0].vY = Math.Max(actors[0].vY, 0);
            }

            if (isDownPressed || isDownLeftPressed || isDownRightPressed)
            {
                actors[0].vY = actors[0].speed;
            } // end if down
            else if(!isDownPressed && !isDownLeftPressed & !isDownRightPressed)
            {
                actors[0].vY = Math.Min(actors[0].vY, 0);
            }

        } // end changeSpeed

        private void HandleKeyPressed(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'n')
            {
                newSession();
            }
            else if (e.KeyChar == 'i')
            {
                Tell("To move your ship, use 'w,a,s,d', the arrow keys, or the number pad.\nTo fire, press space.");
            }
            else if (e.KeyChar == 'p') { isPaused = !isPaused; }
            else if (e.KeyChar == 'm') { Res.USE_SOUNDS = !Res.USE_SOUNDS;}
            else if (e.KeyChar == 'x')
            {
                inSession = false;
                this.CloseCurrentSession();
                Application.Exit();
            }

            e.Handled = true;
        }
        private void GameWindow_KeyPress(object sender, KeyPressEventArgs e)
        {
            HandleKeyPressed(sender, e);
        }
        private void GameWindow_KeyDown(object sender, KeyEventArgs e)
        {
            HandleKeyDown(e);
        }
        private void GameWindow_KeyUp(object sender, KeyEventArgs e)
        {
            HandleKeyUp(e);
        }
#endregion

#region util methods
        public static void PopMenu()
        {
            Tell("ESC MENU\n\nN - New Game\nI - Instructions\nP - Pause\nM - Mute Sound\nX - Exit");
        }
        public static void Tell(String message)
        {   
            MessageBox.Show(message, "Blaster", MessageBoxButtons.OK);
        }
        public static int GetFirstOpenIndex(Object[] objs)
        {
            int result = 0;
            for (result = 0; result < objs.Length; result++)
            {
                if (objs[result] == null)
                {
                    return result;
                }
            }
            return result;
        }
        public static void DropBonus(Panel ActionPanel, Actor sourceActor)
        {
            if (new Random().Next(100) > 80)
            {
                GameWindow.ThisGameWindow.actors[GameWindow.GetFirstOpenIndex(GameWindow.ThisGameWindow.actors)] =
                    new Bonus(ActionPanel, sourceActor);
            }
        }
#endregion

        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GameWindow());
        }
    } // end class
} // end namespace