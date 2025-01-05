using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Retro2D;
using System.Collections.Generic;

namespace Match3
{
    //public enum BallColor
    //{
    //    NULL = Const.NoIndex,
    //}

    public class Gem : Node // , IClone<Gem>
    {
        public enum Items
        {
            NONE,
            TREASURE,
            CRYSTAL,
            MAGIC,
        }

        public enum Mode
        {
            NULL,
            ATTACK,
            DEFENSE,
            HEAL,
            MANA,
            CHIMERA,

        }

        // Static VOID Gem :
        public static Gem VOID = new Gem(Const.NoIndex, Const.NoIndex, NULL);

        #region Colors
        public const int NULL = Const.NoIndex;
        public const int RED = 0;
        public const int GREEN = 1;
        public const int BLUE = 2;
        public const int YELLOW = 3;
        public const int VIOLET = 4;
        public const int GRAY = 5;
        public const int ORANGE = 6;
        public const int TURQUOISE = 7;
        public const int WHITE = 8;
        
        //public const int PINK = 9;
        //public const int BROWN = 10;

        public const int MAX_COLOR = 9;

        public static Dictionary<int, Color> Colors = new Dictionary<int, Color>()
        {
            {NULL, Color.Black},
            {RED, Color.Red},
            {GREEN, Color.LimeGreen},
            {BLUE, Color.RoyalBlue},
            {YELLOW, Color.Yellow},
            {VIOLET, Color.BlueViolet},
            {GRAY, Color.Gray},
            {ORANGE, Color.Orange},
            {TURQUOISE, Color.Turquoise},
            {WHITE, Color.White},
            //{PINK, Color.Pink},
            //{BROWN, Color.SaddleBrown},
        };
        #endregion

        #region Attributes

        Shake _shake = new Shake();

        Addon.Loop _loop = new Addon.Loop();

        public bool IsWarning; // Warning when they are only one or two Gem as same color !
        public bool IsSameColorAsCurrent; // True if the current color in areaGame is same !
        public Items Item = Items.NONE; // If true it contain item for player !

        public bool IsMove; // Status : Gem is move
        public bool IsGoal; // Status : Gem reach goal
        public bool OnGoal; // Trigger : Gem reah goal
        // Gem map Position
        public int _mapX;
        public int _mapY;

        public int _ticMove; 
        public int _durationMove;
        public int _toMapX;
        public int _toMapY;
        public float _fromX;
        public float _fromY;
        public float _toX;
        public float _toY;

        public int _mode; // Mode of gems Attack / Defense / Heal / Mana / Chimera !
        public int _color;
        public bool _alreadyScan; // if case is already scanned !
        public bool _isSameColor; // if case has same color !
        public int _nbSameColor; // Number of neighbour has same color !

        #endregion

        public Gem Copy()
        {
            Gem copy = (Gem)MemberwiseClone();
            return copy;
        }

        public Gem(int mapX , int mapY, int color = NULL, bool isItem = true)
        {
            _type = UID.Get<Gem>();

            _mapX = mapX;
            _mapY = mapY;

            SetSize(Game1.CELL_SIZEW, Game1.CELL_SIZEH);
            SetPivot(Position.CENTER);

            _color = color;
            _alreadyScan = false;
            _nbSameColor = 0;

            _loop.SetLoop(0, 0, 4, .5f, Loops.PINGPONG);
            _loop.Start();

            //if (isItem)
            //{
            //    int rngItem = Misc.Rng.Next(0, 100);

            //    if (rngItem > 90) Item = Items.TREASURE;
            //}

        }

        public override Node Init()
        {
            _color = NULL;
            _alreadyScan = false;
            _isSameColor = false;
            _nbSameColor = 0;

            return base.Init();
        }

        public override Node Update(GameTime gameTime)
        {
            _loop.Update();

            OnGoal = false;

            //if (_color != NULL)
            //    _frame = Game1._animation_Ball.Get(_color);

            if (IsMove)
            {
                _x = Easing.GetValue(Easing.BounceEaseInOut, _ticMove, _fromX, _toX, _durationMove);
                _y = Easing.GetValue(Easing.BounceEaseInOut, _ticMove, _fromY, _toY, _durationMove);

                _ticMove++;
                if (_ticMove >= _durationMove)
                {
                    _x = _toX;
                    _y = _toY;

                    OnGoal = true;
                    IsGoal = true;
                    IsMove = false;

                    _mapX = _toMapX;
                    _mapY = _toMapY;
                }

            }

            return base.Update(gameTime);
        }

        public static void Draw(SpriteBatch batch, float x, float y, Gem gem)
        {
            //gem._frame = Game1._animation_Ball.Get(gem._color);
            //Game1._animation_Ball.Draw(batch, gem._frame, gem.AbsX + gem._shake.GetVector2().X, gem.AbsY + gem._shake.GetVector2().Y, Color.White);

            if (gem.Item == Items.NONE)
                batch.Draw(
                    Game1._tex_Atlas,
                    new Vector2(x - gem._oX + gem._shake.GetVector2().X, y - gem._oY + gem._shake.GetVector2().Y),
                    new Rectangle(0, 0, 64, 64), Colors[gem._color]);

            if (gem.Item == Items.TREASURE)
            {
                Retro2D.Draw.FillRectangle(batch, gem.AbsRect, Colors[gem._color] * .8f);

                batch.Draw(
                    Game1._tex_Atlas,
                    new Vector2(x - gem._oX + gem._shake.GetVector2().X, y - gem._oY + gem._shake.GetVector2().Y),
                    new Rectangle(192, 128, 64, 64), Colors[gem._color]);

                batch.Draw(
                    Game1._tex_Atlas,
                    new Vector2(x - gem._oX + gem._shake.GetVector2().X, y - gem._oY + gem._shake.GetVector2().Y),
                    new Rectangle(128, 0, 64, 64), Color.White);
            }

            //string sameColor = _isSameColor ? "X" : "-";
            //Retro2D.Draw.CenterBorderedStringXY(batch, Game1._font_Main, gem._nbSameColor.ToString(), gem.AbsX, gem.AbsY, Color.White, Color.Black);
            //Draw.CenterBorderedStringXY(batch, Game1._font_Main, IsWarning?"X":".", AbsX, AbsY, Color.White, Color.Black);

            //Retro2D.Draw.CenterBorderedStringXY(batch, Game1._font_Main, gem._mode.ToString(), gem.AbsX, gem.AbsY, Color.White, Color.Black);
        }
        
        public override Node Render(SpriteBatch batch)
        {

            if (_color != NULL)
            {
                Rectangle rect = Gfx.AddRect(AbsRect, new Rectangle(-(int)_loop._current, -(int)_loop._current, (int)_loop._current * 2, (int)_loop._current * 2));


                Draw(batch, AbsX, AbsY, this);

                if (IsSameColorAsCurrent)
                {
                    batch.Draw(Game1._tex_Atlas,
                        rect,
                    //new Rectangle(128, 128, 64, 64), Colors[_color]);
                    //new Rectangle(64, 128, 64, 64), Colors[_color]);
                    new Rectangle(64, 128, 64, 64), Color.White * .8f);

                    //Retro2D.Draw.RectFill(batch, AbsRect, Colors[_color] * .4f);
                    //batch.DrawRectangle(AbsRect, Colors[_color] * .8f);

                }

                if (IsWarning)
                {
                    batch.Draw(Game1._tex_Atlas,
                        rect,
                        new Rectangle(0, 128, 64, 64), Colors[_color]);
                    //new Rectangle(0, 128, 64, 64), Color.White);
                }
            }

            return base.Render(batch);
        }

        //public override Node RenderAdditive(SpriteBatch batch)
        //{
        //    if (_color != NULL && _shake.IsShake)
        //        batch.Draw(Game1._tex_glow1, new Rectangle(AbsX - 48, AbsY - 48, 96, 96), Colors[_color] * .6f);

        //    return base.RenderAdditive(batch);
        //}


        public Gem Shake(float intensity = 1, float step = .05f, bool shakeX = true, bool shakeY = true)
        {
            _shake.SetIntensity(intensity, step, shakeX, shakeY);
            return this;
        }

        public Gem MoveTo(int toMapX, int toMapY, int durationMove = 8)
        {
            // Start Move : Init values
            IsMove = true;
            _ticMove = 0;

            _durationMove = durationMove;

            _fromX = _x;
            _fromY = _y;

            _toMapX = toMapX;
            _toMapY = toMapY;

            _toX = _toMapX * Game1.CELL_SIZEW + Game1.CELL_SIZEW / 2;
            _toY = _toMapY * Game1.CELL_SIZEH + Game1.CELL_SIZEH / 2;

            return this;
        }

        public Gem SetMapPosition(int mapX, int mapY)
        {
            _mapX = mapX;
            _mapY = mapY;

            _x = _mapX * Game1.CELL_SIZEW + Game1.CELL_SIZEW / 2;
            _y = _mapY * Game1.CELL_SIZEH + Game1.CELL_SIZEH / 2;

            return this;
        }

 
    }
}
