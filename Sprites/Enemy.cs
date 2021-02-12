using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Echo.Sprites
{
    public class Enemy
    {
        public Vector2 Position;
        Vector2 Velocity;
        List<Vector2> _destinations;
        int currentDestination;
        Texture2D _echo;
        SoundEffect _echoSound;
        List<Echo> echos;
        float _speed = 100f;

        double timer = 0;
        public double _echoDelay = 1.5f;
        public Enemy(List<Vector2> destinations, Texture2D echo, float speed, double echoDelay, SoundEffect echoSound)
        {
            _destinations = destinations;
            foreach (var dest in destinations)
            {
                if (dest != Vector2.Zero)
                    continue;
                foreach(var d in destinations)
                    Console.WriteLine(d);
                throw new Exception("Suspicious value for enemy inital position, ensure 0 is first in tmx for each enemy");
            }
            _speed = speed;
            _echo = echo;
            _echoSound = echoSound;
            _echoDelay = echoDelay;
            Reset();
        }

        public void Reset()
        {
            Position = _destinations[0];
            currentDestination = 1;
            echos = new List<Echo>();
        }


        public void Update(GameTime gameTime, Player player)
        {
            timer += gameTime.ElapsedGameTime.TotalSeconds;
            if (timer > _echoDelay)
            {
                timer = 0;
                echos.Add(new Echo(Position, _echo));
                var dist = Vector2.Distance(Position, player.Position);
                if (Vector2.Distance(Position, player.Position) < (Game1.SCREEN_HEIGHT / 2) + 400)
                {
                    float percent = dist / ((Game1.SCREEN_HEIGHT / 2) + 400);

                    if (!Game1.DebugMode)
                    {
                        var effectInstance = _echoSound.CreateInstance();

                        float volume = (1 - percent) + 0.2f;
                        if (volume > 1)
                            volume = 1;

                        effectInstance.Volume = volume;
                        effectInstance.Play();
                    }
                }
            }
            if(_destinations.Count > 1)
                destinationMovement(gameTime);

            for (int i = 0; i < echos.Count; i++)
            {
                echos[i].Update(gameTime);
                if (echos[i].isRemoved)
                    echos.RemoveAt(i--);
            }
        }

        private void destinationMovement(GameTime gameTime)
        {
            Velocity = Vector2.Zero;
            if (Vector2.Distance(Position, _destinations[currentDestination]) < 5)
            {
                if (++currentDestination >= _destinations.Count)
                    currentDestination = 0;
            }
            else
            {
                float magnitude = Math.Abs(_destinations[currentDestination].X - Position.X) + Math.Abs(_destinations[currentDestination].Y - Position.Y);
                magnitude *= _speed;
                Velocity = new Vector2((_destinations[currentDestination].X - Position.X) / magnitude, (_destinations[currentDestination].Y - Position.Y) / magnitude);
                if (_destinations[currentDestination].X > Position.X && _destinations[currentDestination].X < Position.X + (Velocity.X * 2))
                    Position.X = _destinations[currentDestination].X;
                if (_destinations[currentDestination].Y > Position.Y && _destinations[currentDestination].Y < Position.Y + (Velocity.Y * 2))
                    Position.Y = _destinations[currentDestination].Y;
            }

            Position.X += (float)gameTime.ElapsedGameTime.TotalMilliseconds * Velocity.X;
            Position.Y += (float)gameTime.ElapsedGameTime.TotalMilliseconds * Velocity.Y;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if(Game1.DebugMode)
                spriteBatch.Draw(_echo, new Rectangle((int)Position.X - 10, (int)Position.Y - 10, 20, 20), null, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.99f); 
            foreach(var echo in echos)
            {
                echo.Draw(spriteBatch);
            }
        }
    }
}
