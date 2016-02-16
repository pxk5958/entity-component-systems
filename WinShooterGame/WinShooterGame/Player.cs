using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WinShooterGame;

namespace Shooter
{
    class Player
    {
        //public Texture2D playerTexture;
        public Animation playerAnimation;
        public Vector2 position;
        public bool active;
        public int health;
        public int width
        {
            get { return playerAnimation.frameWidth; }
        }
        public int height
        {
            get { return playerAnimation.frameHeight; }
        }
        
        public void Initialize(Animation playerAnimation, Vector2 position)
        {
            this.playerAnimation = playerAnimation;
            this.position = position;
            active = true;
            health = 100;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //spriteBatch.Draw(playerTexture, position, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            playerAnimation.Draw(spriteBatch);
        }

        public void Update(GameTime gameTime)
        {
            playerAnimation.position = position;
            playerAnimation.Update(gameTime);
        }
    }
}
