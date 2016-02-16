using System;
using Microsoft.Xna.Framework;
using WinShooterGame.Core;
using System.Data.SQLite;
using Microsoft.Xna.Framework.Graphics;

namespace WinShooterGame.Subsystems
{
    class WrapScreenSubsystem : Subsystem
    {
        private GraphicsDevice graphicsDevice;

        SQLiteCommand getComponentDataCmd;
        SQLiteCommand movableUpdateCmd;
        SQLiteCommand getEntitiesCmd;

        public WrapScreenSubsystem(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            
            getComponentDataCmd = EntityManager.PrepareGetComponentDataCommand("render_component", "position_component", "movable_component");
            movableUpdateCmd = EntityManager.PrepareUpdateComponentCommand("movable_component", "dx, dy");
            getEntitiesCmd = EntityManager.PrepareGetAllEntitiesWithComponents("wrap_screen_component", "render_component", "position_component", "movable_component");
        }

        public override void Update(GameTime gameTime)
        {
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
                            Vector2 position = new Vector2(Convert.ToSingle(getDataReader["X"]),
                                                          Convert.ToSingle(getDataReader["Y"]));
                            Vector2 scale = new Vector2(Convert.ToSingle(getDataReader["scale_X"]),
                                                        Convert.ToSingle(getDataReader["scale_Y"]));
                            Vector2 origin = new Vector2(Convert.ToSingle(getDataReader["origin_X"]),
                                                        Convert.ToSingle(getDataReader["origin_Y"]));
                            Vector2 change = new Vector2(Convert.ToSingle(getDataReader["dx"]),
                                                         Convert.ToSingle(getDataReader["dy"]));

                            // Take current frame's change into account
                            position += change;

                            float width = 0, height = 0;
                            if (getDataReader["source_rect_Width"] != DBNull.Value && getDataReader["source_rect_Height"] != DBNull.Value)
                            {
                                width = Convert.ToInt32(getDataReader["source_rect_Width"]) * scale.X;
                                height = Convert.ToInt32(getDataReader["source_rect_Height"]) * scale.Y;
                            }
                            else
                            {
                                Texture2D texture = (Texture2D)Game1.ResourceMap[(string)getDataReader["texture"]];
                                width = texture.Width * scale.X;
                                height = texture.Height * scale.Y;
                            }

                            // if it is out of screen on the left
                            if (position.X <= -(width - origin.X * scale.X))
                            {
                                change.X += graphicsDevice.Viewport.Width + (width - origin.X * scale.X);
                            }
                            // if it is out of screen on the right
                            else if (position.X >= graphicsDevice.Viewport.Width + origin.X * scale.X)
                            {
                                change.X += -(width - origin.X * scale.X) - graphicsDevice.Viewport.Width;
                            }

                            // if it is out of screen on the top
                            if (position.Y <= -height)
                            {
                                change.Y += graphicsDevice.Viewport.Height + (height - origin.Y * scale.Y);
                            }
                            // if it is out of screen on the bottom
                            else if (position.Y >= graphicsDevice.Viewport.Height + origin.Y * scale.Y)
                            {
                                change.Y += -(height - origin.Y * scale.Y) - graphicsDevice.Viewport.Height;
                            }

                            movableUpdateCmd.Parameters.Add(new SQLiteParameter("@entity_id", entityId));
                            movableUpdateCmd.Parameters.Add(new SQLiteParameter("@param1", change.X));
                            movableUpdateCmd.Parameters.Add(new SQLiteParameter("@param2", change.Y));
                            movableUpdateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
