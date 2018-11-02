using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace GraphicsApp
{
    class SeekingLaser: Laser
    {
        private static int changeDirectionSeverity = 1;
        private static int changeDirectionCooldown = 200;
        private static int maxVX = 8;

        private DateTime lastDirectionChange = DateTime.Now;

        public SeekingLaser(Panel ActionPanel, Actor owner)
            : base(ActionPanel, owner)
        {
            // set different image?
            SetImage("SeekingLaser.gif");
        }

        public override void update()
        {
            // decide to change vX based on Actor[0] location

            // if we haven't changed direction lately...
            if (DateTime.Now > lastDirectionChange.AddMilliseconds(changeDirectionCooldown))
            {
                // update last change.
                lastDirectionChange = DateTime.Now;

                // capture X locations
                int hisX = (int)GameWindow.ThisGameWindow.actors[0].posX + (GameWindow.ThisGameWindow.actors[0].ObjWidth / 2);
                int myX = (int)this.posX + (this.ObjWidth / 2);

                // check location difference to see if we should turn
                if (hisX > myX + changeDirectionSeverity)
                {
                    this.vX = Math.Min(this.vX + changeDirectionSeverity, maxVX);
                }
                else if (hisX < myX - changeDirectionSeverity)
                {
                    this.vX = Math.Max(this.vX - changeDirectionSeverity, -maxVX);
                }
                else // we're right on top of it, try to slow to a halt.
                {
                    if (this.vX > 0)
                    {
                        this.vX--;
                    }
                    else if (this.vX < 0)
                    {
                        this.vX++;
                    }
                    else 
                    {
                        // we're already going straight down (vX == 0), don't turn.
                    }
                }
            }

            // change position just like always;
            base.update();
        }
    }
}
