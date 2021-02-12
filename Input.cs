using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controllers
{
    public class Input
    {
        public static bool Left
        {
            get
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Left) || Keyboard.GetState().IsKeyDown(Keys.A))
                    return true;
                return false;
            }
        }
        public static bool Right
        {
            get
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Right) || Keyboard.GetState().IsKeyDown(Keys.D))
                    return true;
                return false;
            }
        }
        public static bool Up
        {
            get
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Up) || Keyboard.GetState().IsKeyDown(Keys.W))
                    return true;
                return false;
            }
        }
        public static bool Down
        {
            get
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Down) || Keyboard.GetState().IsKeyDown(Keys.S))
                    return true;
                return false;
            }
        }
        public static bool Shoot
        {
            get
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Z) || Keyboard.GetState().IsKeyDown(Keys.OemComma))
                    return true;
                return false;
            }
        }
        public static bool Secondary
        {
            get
            {
                if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift))
                    return true;
                return false;
            }
        }
        public static InputState GetState()
        {
            return new InputState(Left, Right, Up, Down, Secondary);
        }
    }
    public class InputState
    {
        public bool Left;
        public bool Right;
        public bool Up;
        public bool Down;
        public bool Secondary;
        public InputState(bool left, bool right, bool up, bool down, bool secondary)
        {
            Left = left;
            Right = right;
            Up = up;
            Down = down;
            Secondary = secondary;
        }
    }
}
