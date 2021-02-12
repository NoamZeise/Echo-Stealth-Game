using Camera2D;
using Echo.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System.IO;
using Tiled;
using System;

namespace Echo
{
    /// <summary>
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        
        public static bool DebugMode = false;
        public static int SCREEN_WIDTH = 800;
        public static int SCREEN_HEIGHT = 800;

        KeyboardState previousState;
        Camera _camera;
        Player player;
        Texture2D dangerScreen;
        List<Map> maps;
        int currentMap = 0;
        double warningFlash = 0;
        bool flashIn = true;
        SoundEffectInstance background;
        Texture2D cursor;


        #region Animation parameters

        bool startScreen = true;
        double startFadeTimer = 0;
        Texture2D startScreenText;
        double startTextFadeDelay = 5;


        bool discoveredScreen = false;
        double discoveredFadeTimer = 0;
        double discoveredFadeDelay = 3;
        Texture2D deathScreenText;
        Texture2D enterText;
        double deathTextFadeDelay = 5;
        double enterFlash = 0;
        bool enterflashIn = true;

        double blankTimer = 0;
        double blankDelay = 100;
        bool recentlyTeleported = false;

        bool paused = false;
        bool unpausing = false;
        Texture2D pausedTex;
        double pausedFade = 0;
        double pausedFadeDelay = 0.5;

        bool endScreen = false;
        double endFade = 0;
        double endFadeDelay = 5;

        double endGameTimer = 0;
        double endGameDelay = 1;



        #endregion


        #region stat tracking parameters

        double runTime = 0;
        int timesAlerted = 0;


        #endregion

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }


        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 800;
            graphics.ApplyChanges();
            _camera = new Camera(GraphicsDevice, graphics, new RenderTarget2D(GraphicsDevice, SCREEN_WIDTH, SCREEN_HEIGHT));
            base.Initialize();
            IsMouseVisible = false;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            maps = new List<Map>();
            int i = 0;
            while(File.Exists("Maps/" + i.ToString() + ".tmx"))
            {
                 maps.Add(new Map("Maps/" + i++.ToString() + ".tmx", Content, GraphicsDevice));
            }
            maps[maps.Count - 1].lastMap = true;

            currentMap = 0;
            

            maps[currentMap].justMoved = true;

            player = new Player(maps[currentMap].Entrance.Center.ToVector2(), Content.Load<Texture2D>("Sprites/Player/torch"), Content.Load<Texture2D>("Sprites/Player/torchTight"), Content.Load<Texture2D>("Maps/warfog"));
            recentlyTeleported = true;
            dangerScreen = Content.Load<Texture2D>("Sprites/Player/dangerScreen");
            deathScreenText = Content.Load<Texture2D>("deathScreenText");
            enterText = Content.Load<Texture2D>("PressEnter");
            pausedTex = Content.Load<Texture2D>("pauseScreen");
            startScreenText = Content.Load<Texture2D>("startText");
            background = Content.Load<SoundEffect>("echoDeepBackground").CreateInstance() ;
            background.IsLooped = true;

            cursor = Content.Load<Texture2D>("Sprites/cursor");

            //load save
            if (File.Exists("save"))
            {
                try
                {
                    StreamReader sr = new StreamReader("save");
                    currentMap = Convert.ToInt32(sr.ReadLine());
                    runTime = Convert.ToDouble(sr.ReadLine());
                    timesAlerted = Convert.ToInt32(sr.ReadLine());
                    startScreen = false;
                    sr.Close();
                }
                catch
                {
                    currentMap = 0;
                    player.Position = maps[currentMap].Entrance.Center.ToVector2();
                    timesAlerted = 0;
                    runTime = 0;
                }
                player.Position = maps[currentMap].Entrance.Center.ToVector2();
                maps[currentMap].justMoved = true;
                background.Play();
            }

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            StreamWriter sw = new StreamWriter("save");
            sw.WriteLine(currentMap);
            sw.WriteLine(runTime);
            sw.WriteLine(timesAlerted);
            sw.Close();

            if (maps[currentMap].lastMap)
                File.Delete("save");
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (!startScreen)
            {
                if (!discoveredScreen && !paused)
                    normalUpdate(gameTime);
                else if (discoveredScreen)
                    discoveredScreenUpdate(gameTime);
                else if (paused)
                    PauseUpdate(gameTime);

                if (endScreen)
                    EndScreenUpdate(gameTime);

                if (recentlyTeleported)
                {
                    blankTimer += gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (blankTimer > blankDelay)
                    {
                        recentlyTeleported = false;
                        blankTimer = 0;
                    }
                }
            }
            else //start screen
            {
                startFadeTimer += gameTime.ElapsedGameTime.TotalSeconds;
                if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                {
                    startScreen = false;
                    enterFlash = 0;
                    if (!DebugMode)
                        background.Play();
                }
            }
            previousState = Keyboard.GetState();
            base.Update(gameTime);
        }

        private void EndScreenUpdate(GameTime gameTime)
        {
            endFade += gameTime.ElapsedGameTime.TotalSeconds;
            if (endFade > endFadeDelay)
            {
                endFade = endFadeDelay;
                endGameTimer += gameTime.ElapsedGameTime.TotalSeconds;
                if (endGameTimer > endGameDelay)
                {
                    //write txt of player stats
                    StreamWriter sw = new StreamWriter("finish stats " + DateTime.Now.ToString().Replace(":", ".").Replace("/", "-") + ".txt");
                    sw.WriteLine("Run Time: " + runTime.ToString() + " seconds\nTimes alerted: " + timesAlerted.ToString() + "\n\nThanks for playing! :)");

                    sw.Close();

                    Exit();
                }
            }
        }

        private void normalUpdate(GameTime gameTime)
        {
            runTime += gameTime.ElapsedGameTime.TotalSeconds;
            if (Keyboard.GetState().IsKeyDown(Keys.Escape) && previousState.IsKeyUp(Keys.Escape))
            {
                paused = true;
            }
            player.Update(gameTime, maps[currentMap].SolidColliders, _camera);
            maps[currentMap].Update(gameTime, player);
            _camera.Follow(player.Position);

            if (maps[currentMap].playerDetected)
            {
                discoveredScreen = true;
                timesAlerted++;
            }
            if (maps[currentMap].nextMap)
            {
                maps[currentMap].nextMap = false;
                if (maps.Count > currentMap + 1)
                {
                    currentMap++;
                    maps[currentMap].justMoved = true;
                    player.Position = maps[currentMap].Entrance.Center.ToVector2();
                    recentlyTeleported = true;
                }
                else
                {
                    endScreen = true;
                }
                
            }
            if (maps[currentMap].previousMap)
            {
                maps[currentMap].previousMap = false;
                if (currentMap != 0)
                {
                    currentMap--;
                    maps[currentMap].justMoved = true;
                    player.Position = maps[currentMap].Exit.Center.ToVector2();
                    recentlyTeleported = true;
                }
            }
        }

        private void discoveredScreenUpdate(GameTime gameTime)
        {
            discoveredFadeTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                ResetCurrentMap();
            }
        }

        private void ResetCurrentMap()
        {
            player.Position = maps[currentMap].Checkpoints[maps[currentMap].currentCheckpoint].Center.ToVector2();
            maps[currentMap].playerDetected = false;
            maps[currentMap].justMoved = true;
            maps[currentMap].Reset();
            warningFlash = 0;
            discoveredFadeTimer = 0;
            recentlyTeleported = true;
            discoveredScreen = false;

            paused = false;
            unpausing = false;
            pausedFade = 0;
        }

        private void PauseUpdate(GameTime gameTime)
        {
            if (background.State == SoundState.Playing && !unpausing)
                background.Pause();
            if (Keyboard.GetState().IsKeyDown(Keys.Escape) && previousState.IsKeyUp(Keys.Escape) && !unpausing)
            {
                unpausing = true;
                if (pausedFade > pausedFadeDelay)
                    pausedFade = 0;
                else
                {
                    pausedFade = pausedFadeDelay - pausedFade;
                }
            }
            pausedFade += gameTime.ElapsedGameTime.TotalSeconds;

            if (unpausing)
            {
                if (background.State == SoundState.Paused)
                    background.Resume();
                if (pausedFade > pausedFadeDelay)
                {
                    unpausing = false;
                    paused = false;
                    pausedFade = 0;
                }

            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_camera.RenderTarget);
            GraphicsDevice.Clear(Color.LightGray);

            spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _camera.Translation, sortMode: SpriteSortMode.FrontToBack);
            maps[currentMap].DrawBase(gameTime, spriteBatch);

            player.Draw(spriteBatch);
          
            maps[currentMap].drawWalls(gameTime, spriteBatch);
            spriteBatch.End();

            spriteBatch.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.FrontToBack);

            if (startScreen)
            {
                StartScreenDraw(gameTime);
            }
            else
            {
                if (endScreen)
                {
                    float percent = (float)endFade / (float)endFadeDelay;
                    spriteBatch.Draw(maps[currentMap].wallTex, new Rectangle(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT), null, Color.White * (percent), 0f, Vector2.Zero, SpriteEffects.None, 1f);
                }

                if ((maps[currentMap].playerWarning || warningFlash > 0) && !discoveredScreen && !paused)
                {
                    WarningDraw(gameTime);
                }
                else if (discoveredScreen)
                {
                    DiscoveredScreenDraw(gameTime);
                }
                else if (paused)
                {
                    float percent = (float)pausedFade / (float)pausedFadeDelay;

                    if (percent > 1)
                        percent = 1;

                    if (unpausing)
                    {
                        spriteBatch.Draw(maps[currentMap].wallTex, new Rectangle(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT), null, Color.White * (1 - percent), 0f, Vector2.Zero, SpriteEffects.None, 0.98f);
                        spriteBatch.Draw(pausedTex, Vector2.Zero, null, Color.White * (1 - percent), 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                    }
                    else
                    {
                        spriteBatch.Draw(maps[currentMap].wallTex, new Rectangle(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT), null, Color.White * (percent), 0f, Vector2.Zero, SpriteEffects.None, 0.98f);
                        spriteBatch.Draw(pausedTex, Vector2.Zero, null, Color.White * percent, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                    }
                }
                else
                {
                    background.Volume = 0.5f;
                    background.Pitch = 0f;
                    warningFlash = 0;
                    flashIn = true;
                    enterFlash = 0;
                    enterflashIn = true;
                }

                if (recentlyTeleported)
                {
                    spriteBatch.Draw(maps[currentMap].wallTex, new Rectangle(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1f);
                }

                if(!discoveredScreen && !paused)
                {
                    spriteBatch.Draw(cursor, new Vector2(Mouse.GetState().Position.X - (cursor.Width / 2), Mouse.GetState().Position.Y - (cursor.Height / 2)), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                }

            }
            spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            spriteBatch.Draw(_camera.RenderTarget, _camera.ScreenRectangle, Color.White);
            spriteBatch.End();
        }

        private void StartScreenDraw(GameTime gameTime)
        {
            spriteBatch.Draw(maps[currentMap].wallTex, new Rectangle(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.95f);
            float txtFadeR = (float)(startFadeTimer) /
                (float)(startTextFadeDelay);
            if (txtFadeR > 1)
                txtFadeR = 1;

            spriteBatch.Draw(startScreenText, Vector2.Zero, null, Color.White * txtFadeR, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);

            if (startFadeTimer > startTextFadeDelay)
            {
                if (enterFlash > 1)
                    enterflashIn = false;
                if (enterFlash < 0)
                    enterflashIn = true;

                if (enterflashIn)
                    enterFlash += gameTime.ElapsedGameTime.TotalSeconds;
                else
                    enterFlash -= gameTime.ElapsedGameTime.TotalSeconds;

                spriteBatch.Draw(enterText, Vector2.Zero, null, Color.White * (float)(enterFlash), 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            }
        }


        private void DiscoveredScreenDraw(GameTime gameTime)
        {
            spriteBatch.Draw(dangerScreen, Vector2.Zero, null, Color.White * (float)warningFlash * 2, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.96f);

            float disFadeR = (float)discoveredFadeTimer / (float)discoveredFadeDelay;
            if (disFadeR > 1)
                disFadeR = 1;
            spriteBatch.Draw(maps[currentMap].wallTex, new Rectangle(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT), null, Color.White * disFadeR, 0f, Vector2.Zero, SpriteEffects.None, 1f);

            float txtFadeR = (float)(discoveredFadeTimer - discoveredFadeDelay) /
                (float)(deathTextFadeDelay - discoveredFadeDelay);
            if (txtFadeR > 1)
                txtFadeR = 1;

            spriteBatch.Draw(deathScreenText, Vector2.Zero, null, Color.White * txtFadeR, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);

            if (discoveredFadeTimer > deathTextFadeDelay)
            {
                if (enterFlash > 1)
                    enterflashIn = false;
                if (enterFlash < 0)
                    enterflashIn = true;

                if (enterflashIn)
                    enterFlash += gameTime.ElapsedGameTime.TotalSeconds;
                else
                    enterFlash -= gameTime.ElapsedGameTime.TotalSeconds;

                spriteBatch.Draw(enterText, Vector2.Zero, null, Color.White * (float)(enterFlash), 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            }
        }

        private void WarningDraw(GameTime gameTime)
        {
            if (!maps[currentMap].playerWarning)
                flashIn = false;

                background.Volume = 0.7f;
            background.Pitch = 0.3f;

            if (flashIn)
                warningFlash += gameTime.ElapsedGameTime.TotalSeconds;
            else
                warningFlash -= gameTime.ElapsedGameTime.TotalSeconds;

            if (warningFlash > 0.5)
            {
                warningFlash = 0.5;
                flashIn = false;
            }
            if (warningFlash < 0)
            {
                warningFlash = 0;
                flashIn = true;
            }
            spriteBatch.Draw(dangerScreen, Vector2.Zero, null, Color.White * (float)warningFlash * 2, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.96f);
        }
    }
}
