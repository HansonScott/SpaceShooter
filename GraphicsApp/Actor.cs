#region using...
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System;

#endregion

namespace GraphicsApp
{
    public partial class Actor
    {
#region declarations

        // constants - only get updated on difficulty change.
        public static int startLives = 2;
        public static int startSpeed = 4;
        public static int maxSpeed = 12;
        public static int startLaser = 1;
        public static int maxLaser = 5;
        public static int startLaserSpeed = 15;
        public static int maxLaserSpeed = 30;
        public static int startCoolDown = 500;
        public static int minCoolDown = 100;
        public static int startHealth = 5;
        public static int dmgDelay = 250;

        // member vars
        public Bitmap img;
        public int ObjWidth;
        public int ObjHeight;

        public double vX;
        public double vY;
        public double posX;
        public double posY;
        public int numLives;
        public int health;
        public int currentMaxHealth;
        public int speed;
        public int laser;
        public int laserSpeed;
        public int coolDown;
        public DateTime lastShot;
        public DateTime lastDmg;
        public bool deleteMe;

        public int score;

        public Panel ActionPanel;

#endregion

#region constructors
        public Actor(Panel ActionPanel)
        {
            this.ActionPanel = ActionPanel;

            //setup new vars
            posX = (ActionPanel.Width - ObjWidth) / 2;
            posY = ActionPanel.Height - (2 * ObjHeight);
            vX = 0;
            vY = 0;
            numLives = startLives;
            speed = startSpeed;
            laser = 1;
            health = startHealth;
            currentMaxHealth = health;
            laserSpeed = startLaserSpeed;
            coolDown = startCoolDown;
            lastShot = System.DateTime.Now;
        }
        public Actor(Panel ActionPanel, string imgFileName):this(ActionPanel)
        {
            SetImage(imgFileName);

            posX = (ActionPanel.Width - this.ObjWidth) / 2;
            posY = ActionPanel.Height - (2 * this.ObjHeight);

        } // end constructor

#endregion

        public void SetImage(String imgFileName)
        {
            if (imgFileName == null) { return; }
            // get visuals
            try
            {   
                img = Res.getPic(imgFileName);
                SetSize(img);
            }
            catch (Exception e)
            {
                e.ToString();
            }
        } // end setImage
        public void SetSize(Image img)
        {
            ObjWidth = img.Width;
            ObjHeight = img.Height;
        }
        public virtual void update()
        {
            CheckWallCollisions();
            posX = vX + posX;
            posY = vY + posY;
        } // end update
        public virtual void CheckWallCollisions()
        {
            if (posY >= ActionPanel.Height - 1.5 * ObjHeight)
                {
                    vY = Math.Min(vY, 0);
                } // if hit bottom

            if (posY < 2 * ObjHeight)
                {
                    vY = Math.Max(vY, 0);
                } // end if top

            if (posX > ActionPanel.Width - ObjWidth)
                {
                    vX = Math.Min(vX, 0);
                } // if hit right

            if (posX < 0)
                {
                    vX = Math.Max(vX, 0);
                } // end if hit left

        } // end checkWallCollisions
        public virtual void CheckObjectCollision(Actor[] objects)
        {
            for (int i = 1; i < objects.Length; i++)
            {
                if (objects[i] != null){
                    if (objects[i] != this && !objects[i].deleteMe)
                    {
                        // check collision, if so, respond.
                        if (DoesCollide(this, objects[i]))
                        {
                            if (objects[i].GetType() == typeof(Target))
                            {
                                // then damage/destroy both target and player ship.
                                if (System.DateTime.Now.CompareTo(lastDmg.AddMilliseconds(dmgDelay)) > 0)
                                {
                                    this.damageMe(objects[i].laser);
                                    objects[i].damageMe(this.laser);
                                    lastDmg = System.DateTime.Now;
                                } // end if long enough
                            } // if collision with target
                            else if (objects[i].GetType() == typeof(Boss))
                            {
                                objects[i].damageMe(this.currentMaxHealth); // damage boss by player's max health. (ramming power)
                                this.damageMe(objects[i].laser); // damage me by boss's laser power
                                lastDmg = System.DateTime.Now;
                            }
                        } // end if target collided
                    } // end if alive target
                } // end if
            } // end for loop
        } // end checkObjectCollision
        public virtual void drawSelf(Graphics g)
        {
            g.DrawImage(img, (int)posX, (int)posY);
        }
        public virtual void damageMe(int dmg)
        {
            health -= dmg;

            if (health <= 0)
            {
                // explode - 
                Explosion e = new Explosion(ActionPanel, this, Explosion.ExplosionCategory.PlayerDetroyed);
                GameWindow.ThisGameWindow.actors[
                GameWindow.GetFirstOpenIndex(GameWindow.ThisGameWindow.actors)] = e;
                
                numLives--;

                if (numLives >= 0)
                {
                    // leaving a few upgrades, if appropriate.
                    DropBonusesOnDeath();

                    // reset to a new ship.
                    posX = (ActionPanel.Width - ObjWidth) / 2;
                    posY = ActionPanel.Height - (2 * ObjHeight);
                    vX = 0;
                    vY = 0;
                    speed = startSpeed;
                    laser = 1;
                    laserSpeed = startLaserSpeed;
                    coolDown = startCoolDown;
                    currentMaxHealth = startHealth;
                    health = currentMaxHealth;

                    lastShot = System.DateTime.Now;

                    //FUTURE: set some short time of invincible.

                } // if more lives
                else
                {
                    GameWindow.ThisGameWindow.gameOver = true;
                } // no more lives
            } // end if health below zero (died)
        } // end damageMe

        public void ApplyBonus(Bonus bonus)
        {
            switch (bonus.Category)
            {
                case (Bonus.BonusCategory.laserPowerBonus):
                    {
                        this.laser = Math.Min(this.laser + 1, maxLaser);
                        break;
                    }
                case (Bonus.BonusCategory.healthBonus):
                    {
                        this.currentMaxHealth += 1;
                        this.health = Math.Min(this.health + 3, currentMaxHealth);
                        break;
                    }
                case(Bonus.BonusCategory.speedBonus):
                    {
                        this.speed = Math.Min(this.speed + 1, maxSpeed);
                        break;
                    }
                case(Bonus.BonusCategory.laserSpeedBonus):
                    {
                        this.laserSpeed = Math.Min(this.laserSpeed + 1, maxLaserSpeed);
                        this.coolDown = Math.Max((int)(this.coolDown * .75), minCoolDown); // reduce cooldown by 25%
                        break;
                    }
                default:
                    {
                        MessageBox.Show("Bonus category not handled.");
                        break;
                    }
            } // end switch

            // play bonus sound
            Res.playSound("bonus.wav");
        }
        public static bool DoesCollide(Actor ob1, Actor ob2)
        {
            // if any object position corners overlap...
            if (ob1.posX < ob2.posX + ob2.ObjWidth &&
               ob1.posX + ob1.ObjWidth > ob2.posX &&
               ob1.posY < ob2.posY + ob2.ObjHeight &&
               ob1.posY + ob1.ObjHeight > ob2.posY)
            {
                return true;
            }
            return false;

        } // end DoesCollide
        private void DropBonusesOnDeath()
        {
            // theory - drop 1/2 the bonuses we have gathered so far.

            int laserBonuses = (int)(laser / 2);
            int healthBonuses = (int)((this.currentMaxHealth - Actor.startHealth) / 2);
            int speedBonuses = (int)((this.speed - Actor.startSpeed) / 2);
            int laserSpeedBonuses = (int)((this.laserSpeed - Actor.startLaserSpeed) / 2);
            int totalBonuses = laserBonuses + healthBonuses + speedBonuses + laserSpeedBonuses;
            int currentBonusIndex = 0;

            for (int i = 0; i < laserBonuses; i++)
            {
                Bonus b = new Bonus(ActionPanel, this, Bonus.BonusCategory.laserPowerBonus);
                // change b's direction

                PositionBonus(b, currentBonusIndex++);

                GameWindow.ThisGameWindow.actors[
                    GameWindow.GetFirstOpenIndex(GameWindow.ThisGameWindow.actors)] = b;
            }

            for (int i = 0; i < healthBonuses; i++)
            {
                Bonus b = new Bonus(ActionPanel, this, Bonus.BonusCategory.healthBonus);
                // change b's direction

                PositionBonus(b, currentBonusIndex++);

                GameWindow.ThisGameWindow.actors[
                    GameWindow.GetFirstOpenIndex(GameWindow.ThisGameWindow.actors)] = b;
            }

            for (int i = 0; i < speedBonuses; i++)
            {
                Bonus b = new Bonus(ActionPanel, this, Bonus.BonusCategory.speedBonus);
                // change b's direction

                PositionBonus(b, currentBonusIndex++);

                GameWindow.ThisGameWindow.actors[
                    GameWindow.GetFirstOpenIndex(GameWindow.ThisGameWindow.actors)] = b;
            }

            for (int i = 0; i < laserSpeedBonuses; i++)
            {
                Bonus b = new Bonus(ActionPanel, this, Bonus.BonusCategory.laserSpeedBonus);
                // change b's direction

                PositionBonus(b, currentBonusIndex++);

                GameWindow.ThisGameWindow.actors[
                    GameWindow.GetFirstOpenIndex(GameWindow.ThisGameWindow.actors)] = b;
            }
        }
        private void PositionBonus(Bonus b, int currentBonusIndex)
        {
            // set bonus speed according to the current bonus index.
            int speed = 1 * ((int)(currentBonusIndex / 8) + 1); // multiples cause higher speed explosion.;

            switch (currentBonusIndex % 8)
            {
                case 0:
                    {
                        // N
                        b.vY = - (int)(speed * 1.25);
                        b.vX = 0;
                        break;
                    }
                case 1:
                    {
                        //NE
                        b.vY = - speed;
                        b.vX = speed;
                        break;
                    }
                case 2:
                    {
                        //NW
                        b.vY = - speed;
                        b.vX = - speed;
                        break;
                    }
                case 3:
                    {
                        //E
                        b.vY = 0;
                        b.vX = (int)(speed * 1.25);
                        break;
                    }
                case 4:
                    {
                        //W
                        b.vY = 0;
                        b.vX = -(int)(speed * 1.25);
                        break;
                    }
                case 5:
                    {
                        //SE
                        b.vY = speed;
                        b.vX = speed;
                        break;
                    }
                case 6:
                    {
                        //SW
                        b.vY = speed;
                        b.vX = - speed;
                        break;
                    }
                case 7:
                    {
                        //S
                        b.vY = (int)(speed * 1.25);
                        b.vX = 0;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
    } // end class Actor
} // end namespace