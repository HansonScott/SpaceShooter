#region using...
using System;
using System.Windows.Forms;
using System.Drawing;
#endregion

namespace GraphicsApp
{
    public class Bonus : Actor
    {
        #region declarations
        public enum BonusCategory
        {
            laserPowerBonus = 1,
            healthBonus = 2,
            laserSpeedBonus = 3,
            speedBonus = 4
        }

        private BonusCategory m_category;
        Actor source;

        #endregion

        #region constructor
        public Bonus(Panel ActionPanel, Actor sourceActor, BonusCategory category): base(ActionPanel)
        {
            // set actor variables for the laser:
            source = sourceActor;
            this.speed = source.speed;
            m_category = category;
            SetImage("Bonus_" + (int)m_category + ".GIF");
            posX = source.posX;
            posY = source.posY;
            vX = source.vX / 2;
            vY = source.vY;

        }
        public Bonus(Panel ActionPanel, Actor sourceActor)
            :
            base(ActionPanel)
        {
            // set actor variables for the laser:
            source = sourceActor;

            this.speed = source.speed;

            // weight the bonuses - less ammo bonuses
            int rnd = MyRnd.next(100);
            if (rnd < 20) // 20% chance of bonus being  
            {
                m_category = BonusCategory.laserPowerBonus;
            }
            else if (rnd < 45) // 25% chance of bonus being 
            {
                m_category = BonusCategory.healthBonus;
            }
            else if (rnd < 75) // 30% chance of bonus being 
            {
                m_category = BonusCategory.speedBonus;
            }
            else  // 25% chance of bonus being 
            {
                m_category = BonusCategory.laserSpeedBonus;
            }

            SetImage("Bonus_" + (int)m_category + ".GIF");
          
            posX = source.posX;
            posY = source.posY;
            vX = source.vX / 2;
            vY = source.vY;

        } // end constructor
        #endregion

        #region Property methods
        public BonusCategory Category
        {
            get { return m_category; }
        }
        #endregion

        #region public methods
        public override void update()
        {
            base.update();
            if (posY > ActionPanel.Height)
            {
                deleteMe = true;
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
               if (objects[0] != null)
                {
                    // check collision, if so, respond.
                   if(Actor.DoesCollide(this, objects[0]))
                   {
                       if (this.deleteMe == false)
                       {
                           objects[0].ApplyBonus(this);
                           this.deleteMe = true;
                       } // end if not deleteMe
                   }
                } // end if
        } // end checkObjectCollision
        public override void drawSelf(Graphics g)
        {
            g.DrawImage(img, (int)posX, (int)posY);
        } // end drawSelf
        #endregion
    } // end class
} // end namespace