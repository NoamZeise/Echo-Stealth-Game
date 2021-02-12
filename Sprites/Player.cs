using Camera2D;
using Controllers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Echo.Sprites
{
    public class Player
    {
        public Vector2 Position;
        Vector2 _velocity = Vector2.Zero;
        float _angle = 0;
        Texture2D _torch;
        Texture2D _torchTight;
        Texture2D _currentTex;
        Texture2D _warfog;
        float speed = 90f;
        public float detectionRadius
        {
            get
            {
                return 100f;
                /*
                if (_currentTex == _torch)
                    return 150f;
                else
                    return 300f;
                */
            }
        }
        public Vector2 torchEnd
        {
            get //length tight=360 spread=130
            {
                int length = 130;
                if (_currentTex == _torchTight)
                    length = 360;
                float xDiff = (float)(length * Math.Sin(_angle));
                float yDiff = (float)(length * Math.Cos(_angle));
                return new Vector2(Position.X + xDiff, Position.Y - yDiff);
            }
        }

        public Player(Vector2 positon, Texture2D torch, Texture2D torchTight, Texture2D warfog)
        {
            Position = positon;
            _torch = torch;
            _torchTight = torchTight;
            _currentTex = _torch;
            _warfog = warfog;
            if (Game1.DebugMode)
                speed = 500f;
        }

        public void Update(GameTime gameTime, List<Rectangle> colliders, Camera camera)
        {
            Movement(gameTime, colliders);
            CalculateTorchAngle(camera);
        }

        private void CalculateTorchAngle(Camera camera)
        {
            var mPos = Mouse.GetState().Position.ToVector2();

            mPos.X /= ((float)camera.ScreenDimentions.Width / (float)Game1.SCREEN_WIDTH);
            mPos.Y /= ((float)camera.ScreenDimentions.Height / (float)Game1.SCREEN_HEIGHT);

            mPos.X -= camera.Translation.Translation.X;
            mPos.Y -= camera.Translation.Translation.Y;

            var difference = new Vector2(mPos.X - Position.X, mPos.Y - Position.Y);
            if (difference.Y < 0)
            {
                float tempAngle = (float)(Math.Atan(Math.Abs(difference.X) / Math.Abs(difference.Y)));
                if (difference.X < 0)
                {
                    _angle = (float)(Math.PI * 2) - tempAngle;
                }
                if (difference.X > 0)
                {
                    _angle = tempAngle;
                }
            }
            if (difference.Y > 0)
            {
                float tempAngle = (float)(Math.Atan(Math.Abs(difference.Y) / Math.Abs(difference.X)));
                if (difference.X < 0)
                {
                    _angle = (float)(Math.PI * 3) / 2 - tempAngle;
                }
                if (difference.X > 0)
                {
                    _angle = (float)(Math.PI / 2) + tempAngle;
                }
            }
        }

        private void Movement(GameTime gameTime, List<Rectangle> colliders)
        {
            if (Input.Up)
                _velocity.Y -= speed;
            if (Input.Down)
                _velocity.Y += speed;
            if (Input.Left)
                _velocity.X -= speed;
            if (Input.Right)
                _velocity.X += speed;
            if (Input.Secondary)
                _currentTex = _torchTight;
            else
                _currentTex = _torch;

            var xMove = (float)(gameTime.ElapsedGameTime.TotalSeconds) * _velocity.X;
            Position.X += xMove;
            if(xMove != 0)
            foreach(var collider in colliders)
            {
                while(collider.Contains(Position))
                {
                    Position.X -= xMove / 10;
                }
            }
            var yMove = (float)(gameTime.ElapsedGameTime.TotalSeconds) * _velocity.Y;
            Position.Y += yMove;
            if(yMove != 0)
            foreach (var collider in colliders)
            {
                while (collider.Contains(Position))
                {
                    Position.Y -= yMove / 10;
                }
            }

            _velocity = Vector2.Zero;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            
            spriteBatch.Draw(_currentTex, Position, null, Color.White, _angle, new Vector2(_currentTex.Width / 2, _currentTex.Height / 2), 1f, SpriteEffects.None, 0.85f);

            if (_currentTex != _torchTight)
            {
                spriteBatch.Draw(_warfog, new Rectangle((int)(Position.X - Game1.SCREEN_WIDTH / 2), (int)(Position.Y - Game1.SCREEN_HEIGHT / 2), Game1.SCREEN_WIDTH, (int)((Game1.SCREEN_HEIGHT / 2) - (_currentTex.Height / 4))), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.8f);
                spriteBatch.Draw(_warfog, new Rectangle((int)(Position.X - (Game1.SCREEN_WIDTH / 2)), (int)(Position.Y + _currentTex.Width / 4), Game1.SCREEN_WIDTH, (int)(Game1.SCREEN_HEIGHT / 2)), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.8f);

                spriteBatch.Draw(_warfog, new Rectangle((int)(Position.X - Game1.SCREEN_WIDTH), (int)(Position.Y - _currentTex.Height), (Game1.SCREEN_WIDTH) - (_currentTex.Width / 4), Game1.SCREEN_HEIGHT), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.8f);
                spriteBatch.Draw(_warfog, new Rectangle((int)(Position.X + (_currentTex.Width / 4)), (int)(Position.Y - _currentTex.Height), (Game1.SCREEN_WIDTH), Game1.SCREEN_HEIGHT), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.8f);
            }
        }

    }
}
