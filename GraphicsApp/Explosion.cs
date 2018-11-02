using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace GraphicsApp
{
    class Explosion: Actor
    {
        #region declarations
        private int m_frameDuration;
        private int m_currentFrame;
        private DateTime lastFrame;
        private GifImage animatedImage;
        private string m_SoundFileName;
        ExplosionCategory m_category;
        public enum ExplosionCategory
        {
            laserHit = 0,
            targetDestroyed = 1,
            BossDestroyed = 2,
            PlayerDetroyed = 3
        }
        #endregion

        #region constructors
        public Explosion(Panel ActionPanel, Actor source, ExplosionCategory cat): base(ActionPanel)
        {
            this.posX = source.posX + (source.ObjWidth / 2);
            this.posY = source.posY + (source.ObjHeight / 2);

            this.m_category = cat;
            SetImagesByCategory();
            SetSoundByCategory();
            lastFrame = DateTime.Now;
            m_currentFrame = 0;
            m_frameDuration = 8;

            Res.playSound(m_SoundFileName);
        }
        #endregion

        #region method overrides
        public override void update()
        {
            posX = posX + vX;
            posY = posY + vY;
        } // end update
        public override void CheckWallCollisions()
        {
        } // end checkWallCollisions
        public override void CheckObjectCollision(Actor[] objects)
        {
        } // end checkObjectCollision
        public override void drawSelf(Graphics g)
        {
            // draw current frame
            g.DrawImage(animatedImage.GetFrame(m_currentFrame), 
                (int)posX - this.ObjWidth / 2, 
                (int)posY - this.ObjHeight / 2);

            // figure out if we need to change frames for next draw
            if (lastFrame.AddMilliseconds(m_frameDuration).CompareTo(DateTime.Now) < 0)
            {
                // then change frames.
                if (m_currentFrame >= animatedImage.MaxFrames - 1)
                {
                    deleteMe = true;
                }
                else
                {
                    m_currentFrame++;
                }

                lastFrame = DateTime.Now;
            }
        }
        public override void damageMe(int dmg)
        {
        } // end damageMe
        #endregion

        #region private methods
        private void SetImagesByCategory()
        {
            switch (m_category)
            {
                case ExplosionCategory.BossDestroyed:
                    {
                        animatedImage = Res.getAnimation("explosion.gif");
                        break;
                    }
                case ExplosionCategory.laserHit:
                    {
                        animatedImage = Res.getAnimation("explosionSmall.gif");
                        break;
                    }
                case ExplosionCategory.PlayerDetroyed:
                    {
                        animatedImage = Res.getAnimation("explosion.gif");
                        break;
                    }
                case ExplosionCategory.targetDestroyed:
                    {
                        animatedImage = Res.getAnimation("explosion.gif");
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            SetSize(animatedImage.Image);
        }
        private void SetSoundByCategory()
        {
            switch (m_category)
            {
                case ExplosionCategory.BossDestroyed:
                    {
                        m_SoundFileName = "explode01.wav";
                        break;
                    }
                case ExplosionCategory.laserHit:
                    {
                        m_SoundFileName = "laserHit01.wav";
                        break;
                    }
                case ExplosionCategory.PlayerDetroyed:
                    {
                        m_SoundFileName = "explode02.wav";
                        break;
                    }
                case ExplosionCategory.targetDestroyed:
                    {
                        m_SoundFileName = "explode03.wav";
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
        #endregion
    }
}
