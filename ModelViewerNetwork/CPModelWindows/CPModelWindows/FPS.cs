using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace SnowGlobe
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class FPS : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private double fps = 0, fpsCounter = 0;
        private double intervalTime = 0;
        private const double timeThreshold = 1000; //1 second

        private Game1 m_game;

        SpriteBatch spriteBatch;
        SpriteFont fpsFont;
        public FPS(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
            m_game = (Game1)game;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(this.Game.GraphicsDevice);

            fpsFont = this.Game.Content.Load<SpriteFont>("Fonts\\LabelFont");
            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            intervalTime += gameTime.ElapsedGameTime.Milliseconds;
            if (intervalTime < timeThreshold)
            {
                fpsCounter++;
            }
            else
            {
                fps = fpsCounter;
                intervalTime = 0;
                fpsCounter = 0;
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            //this.drawAllWayPoints();
            
            spriteBatch.Begin();
            spriteBatch.DrawString(fpsFont, "Frames Per Second: " + fps, new Vector2(10, 10), Color.White);
            //Debug for peel
            //spriteBatch.DrawString(fpsFont, "" + this.m_game.peelMode.ToString() + "  " + this.m_game.frameCounter + " " + this.m_game.bPeelValid.ToString(), new Vector2(10, 40), Color.White);
            spriteBatch.End();
            //this.device.RasterizerState = prevRs;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            base.Draw(gameTime);
        }
    }
}
