#region using...
using System;
using System.Windows.Forms;
using System.Drawing;
#endregion

namespace GraphicsApp
{
    class Target : Actor
    {
#region declarations
        int lvl;
#endregion

#region constructor
        public Target(Panel ActionPanel, int lvl)
            :
            base(ActionPanel, "target0" + lvl + ".gif")
        {

            // set actor variables for the laser:
            this.lvl = lvl;

            speed = lvl + MyRnd.next(5);
            laser = lvl;
            health = 2 * lvl;
            coolDown = startCoolDown;

            posX = MyRnd.next(ActionPanel.Width - this.ObjWidth);
            posY = -this.ObjHeight;
            vX = speed - (2 * MyRnd.next(speed));
            vY = speed;
            numLives = 0;
        } // end constructor
#endregion

#region public methods
        public override void update()
        {
            base.update();
            try
            {
                if (posY > ActionPanel.Height)
                {
                    deleteMe = true;
                }
                else
                {
                    // decide if we want to shoot at the player
                    if (MyRnd.next(1000) >= (998 - this.lvl)) // 0.2% - 0.5% based on lvl.
                    {
                        // shoot
                        Laser shot;
                        this.lvl += 1;
                        if (lvl > 3)
                        {
                            shot = new SeekingLaser(ActionPanel, this);
                        }
                        else
                        {
                            shot = new Laser(ActionPanel, this);
                        }
                        this.lvl -= 1;

                        shot.vY = 10 + this.lvl; // set speed
                        GameWindow.ThisGameWindow.actors[
                            GameWindow.GetFirstOpenIndex(GameWindow.ThisGameWindow.actors)] = shot;

                    }
                } // end not off screen
            }
            catch (ArgumentNullException ane)
            {
                ane.ToString();
            }
            catch (NullReferenceException ne)
            {
                ne.ToString();
            }

        }
        public override void CheckWallCollisions()
        {
            if (posX >= ActionPanel.Width - ObjWidth)
            {
                vX = Math.Min(-vX, 0);
            } // if hit right

            if (posX <= 0)
            {
                vX = Math.Max(-vX, 0);
            } // end if hit left
        } // end checkWallCollisions
        public override void CheckObjectCollision(Actor[] objects)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                {
                    // check collision, if so, respond.
                } // end if
            } // end for loop
        } // end checkObjectCollision
        public override void drawSelf(Graphics g)
        {
            try
            {
                g.DrawImage(img, (int)posX, (int)posY);
            }
            catch (ArgumentNullException ane)
            {
                ane.ToString();
            }
            catch (NullReferenceException ne)
            {
                ne.ToString();
            }
        } // end drawSelf
        public override void damageMe(int dmg)
        {
            health -= dmg;
            if (health <= 0)
            {
                this.deleteMe = true;

                // explode - 
                Explosion e = new Explosion(ActionPanel, this, Explosion.ExplosionCategory.targetDestroyed);
                GameWindow.ThisGameWindow.actors[
                GameWindow.GetFirstOpenIndex(GameWindow.ThisGameWindow.actors)] = e;
            }
        } // end damageMe
#endregion

    } // end class
} // end namespace