using System;
using System.Collections.Generic;
using WinShooterGame.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Data.SQLite;

namespace WinShooterGame.Subsystems
{
    class RenderSubsystem : Subsystem
    {
        private GraphicsDevice graphicsDevice;
        private SpriteBatch spriteBatch;

        SQLiteCommand getComponentDataCmd;
        SQLiteCommand getEntitiesCmd;

        public RenderSubsystem(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(graphicsDevice);

            getComponentDataCmd = EntityManager.PrepareGetComponentDataCommand("render_component");
            getEntitiesCmd = EntityManager.PrepareGetAllEntitiesWithComponents("render_component");
        }

        public override void Update(GameTime gameTime)
        {
            graphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

            using (SQLiteDataReader entitiesReader = getEntitiesCmd.ExecuteReader())
            {
                while (entitiesReader.Read())
                {
                    Int64 entityId = (Int64)entitiesReader[0];

                    getComponentDataCmd.Parameters.Add(new SQLiteParameter("@entity_id", entityId));
                    using (SQLiteDataReader getDataReader = getComponentDataCmd.ExecuteReader())
                    {
                        if (getDataReader.Read())
                        {
                            Texture2D texture = (Texture2D)Game1.ResourceMap[(string)getDataReader["texture"]];

                            Vector2 position = new Vector2(Convert.ToSingle(getDataReader["position_X"]),
                                                           Convert.ToSingle(getDataReader["position_Y"]));

                            Nullable<Rectangle> sourceRectangle = null;
                            if (getDataReader["source_rect_X"] != DBNull.Value)
                            {
                                sourceRectangle = new Rectangle(Convert.ToInt32(getDataReader["source_rect_X"]),
                                                                Convert.ToInt32(getDataReader["source_rect_Y"]),
                                                                Convert.ToInt32(getDataReader["source_rect_Width"]),
                                                                Convert.ToInt32(getDataReader["source_rect_Height"]));
                            }
                            Color color = new Color(Convert.ToInt32(getDataReader["color_R"]),
                                                    Convert.ToInt32(getDataReader["color_G"]),
                                                    Convert.ToInt32(getDataReader["color_B"]),
                                                    Convert.ToInt32(getDataReader["color_A"]));
                            float rotation = Convert.ToSingle(getDataReader["rotation"]);
                            Vector2 origin = new Vector2(Convert.ToSingle(getDataReader["origin_X"]),
                                                         Convert.ToSingle(getDataReader["origin_Y"]));
                            Vector2 scale = new Vector2(Convert.ToSingle(getDataReader["scale_X"]),
                                                         Convert.ToSingle(getDataReader["scale_Y"]));
                            SpriteEffects effects = SpriteEffects.None;
                            float layerDepth = Convert.ToSingle(getDataReader["layer_depth"]);

                            spriteBatch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
                        }
                    }
                }
            }

            spriteBatch.End();
        }
    }
}
