using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Echo;
using Echo.Sprites;
using Microsoft.Xna.Framework.Audio;

namespace Tiled
{
    public class Map
    {
        public bool playerDetected = false;
        public bool playerWarning = false;
        public bool nextMap = false;
        public bool previousMap = false;
        public bool justMoved = false;
        public bool lastMap = false;

        public int Width;
        public int Height;
        public int TileWidth;
        public int TileHeight;


        public List<Rectangle> SolidColliders;
        public Vector2 playerSpawn = default;
        public Rectangle Exit = default;
        public Rectangle Entrance = default;

        Rectangle Top;
        Rectangle Bottom;
        Rectangle Left;
        Rectangle Right;



        Texture2D background;
        Texture2D walls;
        public Texture2D wallTex;
        SoundEffect echoSound;

        public List<Enemy> Enemies;
        public List<Rectangle> Checkpoints;
        public int currentCheckpoint = 0;
        public Map(string fileName, ContentManager content, GraphicsDevice graphicsDevice)
        {
            StreamReader sr = new StreamReader(fileName);
            XmlReader xmlR = XmlReader.Create(sr);

            SolidColliders = new List<Rectangle>();
            Checkpoints = new List<Rectangle>();
            Checkpoints.Add(new Rectangle(0, 0, 0, 0));
            Enemies = new List<Enemy>();

            wallTex = content.Load<Texture2D>("Maps/wallTex");
            echoSound = content.Load<SoundEffect>("EchoSound");
            bool inMap = false;
            while (xmlR.Read())
            {
                if (xmlR.Name == "map" && !inMap)
                {
                    //set map properties
                    Width = Convert.ToInt32(xmlR.GetAttribute("width"));
                    TileWidth = Convert.ToInt32(xmlR.GetAttribute("tilewidth"));
                    Height = Convert.ToInt32(xmlR.GetAttribute("height"));
                    TileHeight = Convert.ToInt32(xmlR.GetAttribute("tileheight"));
                    inMap = true;
                }
                Top = new Rectangle(-Game1.SCREEN_WIDTH, -Game1.SCREEN_HEIGHT, (Game1.SCREEN_WIDTH * 2) + (TileWidth * Width), Game1.SCREEN_HEIGHT);
                Bottom = new Rectangle(-Game1.SCREEN_WIDTH, (TileHeight * Height), (Game1.SCREEN_WIDTH * 2) + (TileWidth * Width), Game1.SCREEN_HEIGHT);
                Left = new Rectangle(-Game1.SCREEN_WIDTH, 0, Game1.SCREEN_WIDTH, TileHeight * Height);
                Right = new Rectangle(TileWidth * Width, 0, Game1.SCREEN_WIDTH, TileHeight * Height);
                if (xmlR.Name == "imagelayer")
                {
                    xmlR.Read();
                    xmlR.Read();
                    background = content.Load<Texture2D>(xmlR.GetAttribute("source").Replace(".png", ""));
                    walls = content.Load<Texture2D>(xmlR.GetAttribute("source").Replace(".png", "").Replace("base", "walls"));
                    xmlR.Read();
                    xmlR.Read();
                }
                if (xmlR.Name == "objectgroup")
                    createObjects(xmlR, content);

            }
            Checkpoints[0] = Entrance;
            SolidColliders.Add(Top);
            SolidColliders.Add(Bottom);
            SolidColliders.Add(Left);
            SolidColliders.Add(Right);

        }


        private void createObjects(XmlReader xmlR, ContentManager content)
        {
            bool collidable = false;
            bool mapEvent = false;
            bool enemy = false;
            bool checkpoint = false;
            double speed = 0;
            float echoDelay = 1.5f;
            xmlR.Read();
            xmlR.Read();
            if (xmlR.Name == "properties")
            {
                xmlR.Read();
                while (xmlR.Name != "properties")
                {
                    if (xmlR.GetAttribute("name") == "Collidable" && xmlR.GetAttribute("value") == "true")
                        collidable = true;
                    if (xmlR.GetAttribute("name") == "event" && xmlR.GetAttribute("value") == "true")
                        mapEvent = true;
                    if (xmlR.GetAttribute("name") == "enemy" && xmlR.GetAttribute("value") == "true")
                        enemy = true;
                    if (xmlR.GetAttribute("name") == "checkpoint" && xmlR.GetAttribute("value") == "true")
                        checkpoint = true;
                    if (xmlR.GetAttribute("name") == "speed")
                        speed = Convert.ToDouble(xmlR.GetAttribute("value"));
                    if (xmlR.GetAttribute("name") == "echoDelay")
                        echoDelay = (float)Convert.ToDouble(xmlR.GetAttribute("value"));
                    xmlR.Read();
                }
            }
            xmlR.Read();
            int count = 0;
            if (enemy)
                count++;
            if (collidable)
                count++;
            if (mapEvent)
                count++;
            if (count > 1)
                throw new Exception("object layer is more than one type?");

            if (collidable)
                while (xmlR.Name != "objectgroup")
                {
                    SolidColliders.Add(new Rectangle((int)Convert.ToDouble(xmlR.GetAttribute("x")),
                        (int)Convert.ToDouble(xmlR.GetAttribute("y")),
                        (int)Convert.ToDouble(xmlR.GetAttribute("width")),
                        (int)Convert.ToDouble(xmlR.GetAttribute("height"))));

                    xmlR.Read();
                }
            else if (mapEvent)
                while (xmlR.Name != "objectgroup")
                {
                    if (xmlR.Name == "object")
                    {
                        if (xmlR.GetAttribute("name") == "player")
                        {
                            playerSpawn = new Vector2((float)Convert.ToDouble(xmlR.GetAttribute("x")), (float)Convert.ToDouble(xmlR.GetAttribute("y")));
                        }
                        if (xmlR.GetAttribute("name") == "exit")
                        {
                            Exit = new Rectangle((int)Convert.ToDouble(xmlR.GetAttribute("x")),
                        (int)Convert.ToDouble(xmlR.GetAttribute("y")),
                        (int)Convert.ToDouble(xmlR.GetAttribute("width")),
                        (int)Convert.ToDouble(xmlR.GetAttribute("height")));
                        }
                        if (xmlR.GetAttribute("name") == "entrance")
                        {
                            Entrance = new Rectangle((int)Convert.ToDouble(xmlR.GetAttribute("x")),
                        (int)Convert.ToDouble(xmlR.GetAttribute("y")),
                        (int)Convert.ToDouble(xmlR.GetAttribute("width")),
                        (int)Convert.ToDouble(xmlR.GetAttribute("height")));
                        }
                    }
                    xmlR.Read();
                }
            else if (enemy)
            {
                var destinationChart = new Dictionary<int, Vector2>();
                while (xmlR.Name != "objectgroup")
                {
                    if (xmlR.Name == "object")
                    {
                        if (!destinationChart.ContainsKey(Convert.ToInt32(xmlR.GetAttribute("name"))))
                        {
                            destinationChart.Add(Convert.ToInt32(xmlR.GetAttribute("name")),
                                new Vector2((float)Convert.ToDouble(xmlR.GetAttribute("x")),
                                (float)Convert.ToDouble(xmlR.GetAttribute("y"))));
                        }
                    }
                    xmlR.Read();
                }
                List<Vector2> tempDestinations = new List<Vector2>();
                for (int i = 0; i < destinationChart.Count; i++)
                {
                    tempDestinations.Add(destinationChart[i]);
                }

                Enemies.Add(new Enemy(tempDestinations, content.Load<Texture2D>("Sprites/Enemy/echo"), (float)speed, echoDelay, echoSound));
            }
            else if (checkpoint)
                while (xmlR.Name != "objectgroup")
                {
                    Checkpoints.Add(new Rectangle((int)Convert.ToDouble(xmlR.GetAttribute("x")),
                        (int)Convert.ToDouble(xmlR.GetAttribute("y")),
                        (int)Convert.ToDouble(xmlR.GetAttribute("width")),
                        (int)Convert.ToDouble(xmlR.GetAttribute("height"))));

                    xmlR.Read();
                }
            else
                while (xmlR.Name != "objectgroup")
                    xmlR.Read();



        }

        public void Reset()
        {
            foreach (var enemy in Enemies)
                enemy.Reset();
        }

        public void Update(GameTime gameTime, Player player)
        {

            bool noPlayerWarning = true;
            foreach(var enemy in Enemies)
            {
                if(lastMap)
                {
                    enemy._echoDelay -= (gameTime.ElapsedGameTime.TotalSeconds / 3);
                    if (enemy._echoDelay < 0.1)
                        enemy._echoDelay = 0.1;
                   
                }
                enemy.Update(gameTime, player);
                if (!lastMap)
                {
                    if ((Vector2.Distance(enemy.Position, player.Position) < player.detectionRadius) ||
                        (Vector2.Distance(enemy.Position, player.torchEnd) < player.detectionRadius))
                    {
                        bool colliderBetween = false;
                        float dist = Vector2.Distance(player.Position, enemy.Position); //get distanace between points
                        for (int i = 0; i < dist; i++) //check through line connecting points for collider
                            foreach (var rectangle in SolidColliders) // \/ iterate through by unit vector multiplied by each point between
                                if (rectangle.Contains(player.Position.X - ((i / dist) * (player.Position.X - enemy.Position.X)), player.Position.Y - ((i / dist) * (player.Position.Y - enemy.Position.Y))))
                                {
                                    colliderBetween = true;
                                }
                        if(!colliderBetween)
                            playerDetected = true;

                    }
                }
                if ((Vector2.Distance(enemy.Position, player.Position) < player.detectionRadius * 2.5) ||
                    (Vector2.Distance(enemy.Position, player.torchEnd) < player.detectionRadius * 2.5))
                {
                    playerWarning = true;
                    noPlayerWarning = false;
                }
            }
            foreach(var chP in Checkpoints)
            {
                if (chP.Contains(player.Position))
                {
                    currentCheckpoint = Checkpoints.IndexOf(chP);
                }
            }
            if (noPlayerWarning)
                playerWarning = false ;

            if (lastMap)
            {
                if (Enemies[0]._echoDelay < 0.2)
                    nextMap = true;
            }

            if (Exit.Contains(player.Position))
            {
                if(!justMoved && !lastMap)
                {
                    nextMap = true;
                }
            }
            else if (lastMap && Enemies[0]._echoDelay < 1 )
            {
                if(player.Position.Y < Exit.Center.ToVector2().Y)
                    player.Position.Y += (float)gameTime.ElapsedGameTime.TotalSeconds * 700f;
                else
                    player.Position.Y -= (float)gameTime.ElapsedGameTime.TotalSeconds * 700f;
            }
            else if(Entrance.Contains(player.Position) && !lastMap)
            {
                if (!justMoved)
                {
                    previousMap = true;
                }
            }
            else
            {
                justMoved = false;
            }
        }


        public void DrawBase(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(background, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.1f);

            foreach (var enemy in Enemies)
                enemy.Draw(spriteBatch);
        }
        public void drawWalls(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(walls, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.95f);

            spriteBatch.Draw(wallTex, Top, null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.95f);
            spriteBatch.Draw(wallTex, Bottom, null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.95f);
            spriteBatch.Draw(wallTex, Left, null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.95f);
            spriteBatch.Draw(wallTex, Right, null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.95f);

        }


    }
}
