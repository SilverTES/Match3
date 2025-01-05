using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Retro2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Match3
{
    public class Blood : Node
    {
        Texture2D _image;
        //Point _position;
        float _transparency;

        public Blood(float x, float y)
        {
            _image = Game1._tex_blood;
            SetPosition(x, y);
            _transparency = 1f;
        }

        public override Node Update(GameTime gameTime)
        {
            if (_transparency <= 0f)
                KillMe();

            _transparency -= 0.001f;

            return base.Update(gameTime);
        }

        public override Node Render(SpriteBatch batch)
        {
            int px = AbsX - (int)(_image.Width * .5f);
            int py = AbsY - (int)(_image.Height * .5f);

            batch.Draw(_image, new Rectangle(px, py + 8, _image.Width, _image.Height - 16), Color.White * _transparency);
            return base.Render(batch);
        }
    }
}
