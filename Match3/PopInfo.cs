using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Retro2D;

namespace Match3
{
    public class PopInfo : Node
    {
        string _label;
        Color _color;

        public PopInfo(string label, Color color, float start = 0, float end = 16, float duration = 32)
        {
            _label = label;
            _color = color;

            _animate.Add("popup", Easing.BackEaseInOut, new Tweening(start, end, duration));
            _animate.Start("popup");

            _z = -10000; // Over all Node Childs
        }

        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            if (_animate.OnEnd("popup"))
            {
                KillMe();
            }

            _animate.NextFrame();


            return base.Update(gameTime);
        }

        public override Node Render(SpriteBatch batch)
        {

            //Draw.CenterStringX(batch, Game1._font_Main, _label, AbsX()-1, AbsY() - _animate.Value(), Color.Black);
            //Draw.CenterStringX(batch, Game1._font_Main, _label, AbsX()+1, AbsY() - _animate.Value(), Color.Black);
            //Draw.CenterStringX(batch, Game1._font_Main, _label, AbsX(), AbsY()-1 - _animate.Value(), Color.Black);
            //Draw.CenterStringX(batch, Game1._font_Main, _label, AbsX(), AbsY()+1 - _animate.Value(), Color.Black);

            Draw.TopCenterBorderedString(batch, Game1._font_Big, _label, AbsX, AbsY - _animate.Value(), _color, Color.Black);

            return base.Render(batch);
        }

        public override Node RenderAdditive(SpriteBatch batch)
        {
            int width = (int)Game1._font_Big.MeasureString(_label).X * 2;
            int height = (int)Game1._font_Big.MeasureString(_label).Y * 2;

            batch.Draw(Game1._tex_glow1, new Rectangle(AbsX - width / 2, AbsY - height/4 - (int)_animate.Value(), width, height), _color * 1f);

            return base.RenderAdditive(batch);
        }
    }
}
