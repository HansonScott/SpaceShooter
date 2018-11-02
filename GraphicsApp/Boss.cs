#region using...
using System;
using System.Windows.Forms;
using System.Drawing;
#endregion

namespace GraphicsApp
{
    class Boss : Actor
    {
        #region declarations
        int lvl;
        #endregion

        #region constructor
        public Boss(Panel ActionPanel, int lvl)
            :
            base(ActionPanel, "Boss0" + lvl + ".gif")
        {
            // set actor variables for the laser:
            this.lvl = lvl;

            speed = 2 * lvl;
            laser = 2 + (lvl * 3); // 4, 8, 11
            health = 10 + 30 * lvl; // 30, 70, 100, etc.
            coolDown = (10 - lvl) * 100; // 900, 800, 700, etc.

            posX = MyRnd.next(ActionPanel.Width - this.ObjWidth);
            posY = -this.ObjHeight - 300; // - 300 to delay a bit.
            vX = speed - (2 * MyRnd.next(speed));
            vY = speed;
            numLives = 0;
        } // end constructor
        #endregion

        #region public methods
        public override void update()
        {
            base.update();
            if (this.deleteMe)
            {
                GameWindow.ThisGameWindow.isBossPresent = false;
                return;
            }

                #region Shooting
                // first check if we can shoot yet
                if (lastShot.AddMilliseconds(coolDown).CompareTo(System.DateTime.Now) < 0)
                {
                    // if so, then randomly choose.
                    if (MyRnd.next(100) >= (90 - (this.lvl * 2))) // 10% - 15% based on lvl. (per frame)
                    {
                        // standard shot (x3)
                        int temp = this.lvl;
                        this.lvl = 5;
                        Laser shot = new Laser(ActionPanel, this);
                        Laser shot2 = new Laser(ActionPanel, this);
                        Laser shot3 = new Laser(ActionPanel, this);
                        SeekingLaser shot4 = new SeekingLaser(ActionPanel, this);
                        SeekingLaser shot5 = new SeekingLaser(ActionPanel, this);
                        SeekingLaser shot6 = new SeekingLaser(ActionPanel, this);

                        this.lvl = temp;

                        // level specific shooting capabilities
                        // small spread
                        shot.vY = 5 + this.lvl * 2; // set speed
                        shot2.vY = 5 + this.lvl * 2; // set speed
                        shot3.vY = 5 + this.lvl * 2; // set speed
                        shot4.vY = 5 + this.lvl * 2;
                        shot5.vY = 5 + this.lvl * 2;
                        shot6.vY = 5 + this.lvl * 2;

                        shot2.vX = -3; // set angle
                        shot3.vX = 3; // set angle
                        shot5.vX = -5; // set angle
                        shot6.vX = 5; // set angle

                        // if higher boss, shoot seeking laser instead
                        switch (temp)
                        {
                            case 1:
                                GameWindow.ThisGameWindow.actors[
                                    GameWindow.GetFirstOpenIndex(
                                        GameWindow.ThisGameWindow.actors)] = shot;
                                GameWindow.ThisGameWindow.actors[
                                    GameWindow.GetFirstOpenIndex(
                                        GameWindow.ThisGameWindow.actors)] = shot2;
                                GameWindow.ThisGameWindow.actors[
                                    GameWindow.GetFirstOpenIndex(
                                        GameWindow.ThisGameWindow.actors)] = shot3;
                                break;
                            case 2:
                                GameWindow.ThisGameWindow.actors[
                                    GameWindow.GetFirstOpenIndex(
                                        GameWindow.ThisGameWindow.actors)] = shot2;
                                GameWindow.ThisGameWindow.actors[
                                    GameWindow.GetFirstOpenIndex(
                                        GameWindow.ThisGameWindow.actors)] = shot4;
                                GameWindow.ThisGameWindow.actors[
                                    GameWindow.GetFirstOpenIndex(
                                        GameWindow.ThisGameWindow.actors)] = shot3;
                                break;
                            case 3:
                                GameWindow.ThisGameWindow.actors[
                                    GameWindow.GetFirstOpenIndex(
                                        GameWindow.ThisGameWindow.actors)] = shot5;
                                GameWindow.ThisGameWindow.actors[
                                    GameWindow.GetFirstOpenIndex(
                                        GameWindow.ThisGameWindow.actors)] = shot;
                                GameWindow.ThisGameWindow.actors[
                                    GameWindow.GetFirstOpenIndex(
                                        GameWindow.ThisGameWindow.actors)] = shot6;
                                break;
                            case 4:
                                GameWindow.ThisGameWindow.actors[
                                    GameWindow.GetFirstOpenIndex(
                                        GameWindow.ThisGameWindow.actors)] = shot5;
                                GameWindow.ThisGameWindow.actors[
                                    GameWindow.GetFirstOpenIndex(
                                        GameWindow.ThisGameWindow.actors)] = shot4;
                                GameWindow.ThisGameWindow.actors[
                                    GameWindow.GetFirstOpenIndex(
                                        GameWindow.ThisGameWindow.actors)] = shot6;
                                break;
                        }
                        
                        // we have fired, so set the lastShot timer
                        lastShot = DateTime.Now;
                    } // end shoot
                }// end if cooldown
                #endregion

                #region Movement
                // decide if we want to change location/speeds
                if (posY > (2 * img.Height))
                {
                    vY = 0;
                    // if boss is moving slowly vX
                    if (vX < 5 &&
                        vX > -5)
                    {
                        // then speed up
                        vX = 2 * (vX + 1);
                    }
                }
                #endregion
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
            // if boss collides with player, player's code catches it.
        } // end checkObjectCollision
        public override void drawSelf(Graphics g)
        {
            if (img != null && g != null)
            {
                g.DrawImage(img, (int)posX, (int)posY);
            }
        } // end drawSelf
        public override void damageMe(int dmg)
        {
            health -= dmg;
            if (health <= 0)
            {
                // progress level
                GameWindow.ThisGameWindow.isBossPresent = false;
                GameWindow.ThisGameWindow.actors[0].score += lvl * lvl * 10;
                GameWindow.ThisGameWindow.resetLevel();
                this.deleteMe = true;

                // explode - 
                Explosion e = new Explosion(ActionPanel, this, Explosion.ExplosionCategory.PlayerDetroyed);
                GameWindow.ThisGameWindow.actors[
                GameWindow.GetFirstOpenIndex(GameWindow.ThisGameWindow.actors)] = e;

            } // end if dead
        } // end damageMe
        #endregion
    } // end class
} // end namespace