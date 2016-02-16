using System;
using Microsoft.Xna.Framework;
using WinShooterGame.Core;
using System.Data.SQLite;
using Microsoft.Xna.Framework.Graphics;

namespace WinShooterGame.Subsystems
{
    class ClampScreenSubsystem : Subsystem
    {
        private GraphicsDevice graphicsDevice;

        SQLiteCommand getComponentDataCmd;
        SQLiteCommand movableUpdateCmd;
        SQLiteCommand getEntitiesCmd;

        public ClampScreenSubsystem(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;

            getComponentDataCmd = EntityManager.PrepareGetComponentDataCommand("render_component", "position_component", "movable_component");
            movableUpdateCmd = EntityManager.PrepareUpdateComponentCommand("movable_component", "dx, dy");
            getEntitiesCmd = EntityManager.PrepareGetAllEntitiesWithComponents("clamp_screen_component", "render_component", "position_component", "movable_component");
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

                            // Make sure that the player does not go out of bounds
                            Vector2 newPosition = new Vector2();
                            newPosition.X = MathHelper.Clamp(position.X, /*width / 2*/ origin.X * scale.X, graphicsDevice.Viewport.Width - /*width / 2*/(width - origin.X * scale.X));
                            newPosition.Y = MathHelper.Clamp(position.Y, /*height / 2*/ origin.Y * scale.Y, graphicsDevice.Viewport.Height - /*height / 2*/(height - origin.Y * scale.Y));

                            movableUpdateCmd.Parameters.Add(new SQLiteParameter("@entity_id", entityId));
                            movableUpdateCmd.Parameters.Add(new SQLiteParameter("@param1", change.X + newPosition.X - position.X));
                            movableUpdateCmd.Parameters.Add(new SQLiteParameter("@param2", change.Y + newPosition.Y - position.Y));
                            movableUpdateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
