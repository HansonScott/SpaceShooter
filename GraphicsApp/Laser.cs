#region using...
using System;
using System.Windows.Forms;
using System.Drawing;
#endregion

namespace GraphicsApp
{
    class Laser : Actor
    {
#region declarations
        Actor owner;
        private string m_shootSound;
#endregion

#region constructor
        public Laser(Panel ActionPanel, Actor owner):
            base(ActionPanel, "laser0" + owner.laser + ".gif")

        {
            // set actor variables for the laser:
            this.owner = owner;
            posX = owner.posX + (int)(owner.ObjWidth / 2); // center (left-right)
            posY = owner.posY + (int)(owner.ObjHeight / 2); // center (top-bottom)
            vX = 0;
            vY = owner.laserSpeed; // should be positive.
            numLives = 1;
            speed = owner.laserSpeed;
            laserSpeed = owner.laserSpeed;
            laser = owner.laser;
            coolDown = 0;

            SetSound();
            Res.playSound(this.m_shootSound);

            // if image does not exist, then set to max.
            if (img == null)
            {
                SetImage("laser04.GIF");
            }

        } // end constructor
        private void SetSound()
        {
            if (owner.laser > 0 && owner.laser < 5)
            {
                m_shootSound = "shoot0" + owner.laser + ".wav";
            }
            else
            {
                m_shootSound = "shoot04.wav";
            }
        }
#endregion

        #region public methods
        public override void CheckWallCollisions()
        {
            if (posY <= -ObjHeight ||
                posY >= ActionPanel.Height)
            {
                deleteMe = true;
            } // end if too high or low

            else if(posX <= -ObjWidth ||
                posX >= ActionPanel.Width)
            {
                deleteMe = true;
            } // end if too wide.

        } // end checkWallCollisions
        public override void CheckObjectCollision(Actor[] objects)
        {
            // if laser traveling downward, then it is an enemy laser
            if (vY > 0)
            {
                // if laser hit player AND laser does not belong to player
                if(Actor.DoesCollide(this, objects[0]) 
                    && deleteMe == false
                    && owner != objects[0])
                {
                    // damage player
                    objects[0].damageMe(this.laser);

                    // create small explosion...
                    Explosion e = new Explosion(ActionPanel, this, Explosion.ExplosionCategory.laserHit);
                    GameWindow.ThisGameWindow.actors[
                    GameWindow.GetFirstOpenIndex(GameWindow.ThisGameWindow.actors)] = e;

                    // destroy self
                    this.deleteMe = true;

                } // end if hit player
            } // end if going down (enemy laser)

            else { // it's the players laser
                for (int i = 1; i < objects.Length; i++)
                {
                    if (objects[i] != null &&
                        objects[i] != this &&
                        objects[i].GetType() != typeof(Laser) && // lasers don't counter each other
                        objects[i].GetType() != typeof(SeekingLaser) && // lasers don't counter each other
                        objects[i].GetType() != typeof(Bonus) && // lasers can't kill a bonus
                        objects[i].GetType() != typeof(Explosion) && // lasers go through explosions.
                        objects[i].deleteMe == false)
                    {
                        // check collision, if so, respond.
                        if (Actor.DoesCollide(this, objects[i]))
                        {
                            // player laser damaged target, destroy/hurt target
                            objects[i].damageMe(this.laser);
                            if (objects[i].deleteMe == true)
                            {
                                GameWindow.DropBonus(ActionPanel, objects[i]);
                                objects[0].score += objects[i].laser;
                            }
                            else // if target not destroyed
                            {
                                // create small laser explosion...
                                Explosion e = new Explosion(ActionPanel, this, Explosion.ExplosionCategory.laserHit);
                                
                                // set explosion to match movement of target.
                                e.vX = objects[i].vX;
                                e.vY = objects[i].vY;

                                GameWindow.ThisGameWindow.actors[
                                GameWindow.GetFirstOpenIndex(GameWindow.ThisGameWindow.actors)] = e;
                            }

                            // destory self
                            this.deleteMe = true;
                        } // if collides.
                    } // end if
                } // end for loop
            } // end if going up (player laser)

        } // end checkObjectCollision
        public override void drawSelf(Graphics g)
        {
            try
            {
                g.DrawImage(img, (int)posX, (int)posY);
            }
            catch (Exception e) 
            {
                e.ToString();
            }

        } // end drawSelf
        #endregion

    } // end class
} // end namespace