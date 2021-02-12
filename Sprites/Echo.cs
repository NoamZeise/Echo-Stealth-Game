using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Echo.Sprites
{
    class Echo
    {

        Vector2 _position;
        Texture2D _texture;

        double timer = 0;
        double ExpiryTime = 1.2;
        double LingerTime = 3;
        public bool isRemoved = false;

        public Echo(Vector2 position, Texture2D texture)
        {
            _texture = texture;
            _position = position;
        }

        public void Update(GameTime gameTime)
        {
            timer += gameTime.ElapsedGameTime.TotalSeconds;
            if (timer > LingerTime)
                isRemoved = true;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            double timerCompletion = timer / ExpiryTime;
            float fade = 1f;
            if (timerCompletion > 1)
            {
                fade = (float)(1- ((timer - ExpiryTime) / (LingerTime - ExpiryTime)));
            }



            //when close to 0 small area, when close to 1 max area
            //centred on position
            //min 50x50
            //max 100x100

            double width = 50 + (150 * timerCompletion);
            double height = 50 + (150 * timerCompletion);
            double x = _position.X - (width / 2);
            double y = _position.Y - (height / 2);



            spriteBatch.Draw(_texture, new Rectangle((int)x, (int)y, (int)width, (int)height), null, Color.White * fade, 0f, Vector2.Zero, SpriteEffects.None, 0.9f);
        }
    }
}
