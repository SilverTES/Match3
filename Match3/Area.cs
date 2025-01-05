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
    public class Area : Node
    {
        //List<PlayGrid> PlayGridTeamL = new List<PlayGrid>();
        //List<PlayGrid> PlayGridTeamT = new List<PlayGrid>();

        public const int CellW = 40;
        public const int CellH = 40;

        public int MapW { get; private set; }
        public int MapH { get; private set; }
        
        // Collision Grid
        Collision2DGrid _collision2DGrid;

        Dictionary<int, Vector2> LeftGem  = new Dictionary<int, Vector2>();
        Dictionary<int, Vector2> RightGem = new Dictionary<int, Vector2>();

        public Area(int mapW, int mapH)
        {
            MapW = mapW;
            MapH = mapH;

            SetSize(mapW * CellW, mapH * CellH);
            SetPivot(0, 0);

            // Create CollisionGrid
            _collision2DGrid = new Collision2DGrid((int)_rect.Width / 128, (int)_rect.Height / 128, 128); // --- VERY IMPORTANT FOR PRECISE COLLISION !!

            // Set Gem Target Position
            int i = -1;
            foreach (var gem in Gem.Colors)
            {
                LeftGem[gem.Key] =  new Vector2(CellW/2, i*CellH + CellH /2 );
                RightGem[gem.Key] = new Vector2(_rect.Width - CellW/2, i*CellH + CellH /2);

                i++;
            }
        }


        public override Node Init()
        {

            return base.Init();
        }

        public Vector2 AbsLeftGem(int color)
        {
            return LeftGem[color] + AbsXY;
        }
        public Vector2 AbsRightGem(int color)
        {
            return RightGem[color] + AbsXY;
        }

        public Unit AddUnit(PlayGrid playGrid, int gemColor, int caseY, float speed = .5f, int maxHP = 32)
        {

            //int caseY = Misc.Rng.Next(0, Area.MapH);

            float x = CellW / 2;
            float y = caseY * CellH + CellH / 2;

            Vector2 target = new Vector2(_rect.Width, y);

            if (playGrid.Team == Teams.R)
            {
                x = _rect.Width - CellW / 2;

                target.X = 0;
            }

            Unit unit = (Unit)new Unit(playGrid, gemColor, new Vector2(x, y), target, speed, maxHP)
                .AppendTo(this);

            new Smoke(unit._x, unit._y, 16, true, false, 16, 16)
                .AppendTo(this);

            return unit;
        }


        public override Node Update(GameTime gameTime)
        {

            // Collision2D System Run !
            //_collision2DGrid.SetPosition((int)_x, (int)_y);
            _collision2DGrid.SetPosition(0, 0);
            Collision2D.ResetAllZone(this);
            Collision2D.GridSystemZone(this, _collision2DGrid);


            UpdateRect();

            UpdateChilds(gameTime);

            return base.Update(gameTime);
        }

        public override Node Render(SpriteBatch batch)
        {
            Draw.FillRectangle(batch, AbsRect, Color.DarkSlateGray);
            Draw.Grid(batch, AbsRect.X, AbsRect.Y, AbsRect.Width, AbsRect.Height, CellW, CellH, Color.Black * .2f);
            //Draw.Grid(batch, AbsRect.X, AbsRect.Y, AbsRect.Width, AbsRect.Height, AbsRect.Width, CellH, Color.Black);

            Draw.Rectangle(batch,  AbsRect, Color.Black, 1);

            int i = 0;
            foreach (var gem in Gem.Colors)
            {
                if (gem.Key != Gem.NULL)
                {
                    Rectangle rectL = (Rectangle)Gfx.TranslateRect(new Rectangle(0, i * CellH, CellW, CellH), AbsXY);
                    Rectangle rectR = (Rectangle)Gfx.TranslateRect(new Rectangle(0, i * CellH, CellW, CellH), AbsXY + new Vector2(AbsRect.Width - CellW,0));


                    Draw.FillRectangle(batch, Gfx.AddRect(rectL, 8,8,-16,-16), gem.Value * .8f);
                    Draw.FillRectangle(batch, Gfx.AddRect(rectR, 8,8,-16,-16), gem.Value * .8f);

                    Draw.Rectangle(batch, Gfx.AddRect(rectL, 8, 8, -16, -16), gem.Value);
                    Draw.Rectangle(batch, Gfx.AddRect(rectR, 8, 8, -16, -16), gem.Value);

                    //batch.DrawLine(
                    //    CellW / 2 + AbsX, i * CellH + CellH / 2 + AbsY,
                    //    AbsRect.Width - CellW / 2 + AbsX, i * CellH + CellH / 2 + AbsY,
                    //    gem.Value * .8f,
                    //    1
                    //    );

                    i++;
                }
            }


            SortZD();
            RenderChilds(batch);

            Draw.TopCenterString(batch, Game1._font_Main, "TEAM L", AbsRect.Width / 2 - AbsRect.Width / 4, AbsRect.Y - 48, Color.Orange);
            Draw.TopCenterString(batch, Game1._font_Main, "TEAM R", AbsRect.Width / 2 + AbsRect.Width / 4, AbsRect.Y - 48, Color.RoyalBlue);


            return base.Render(batch);
        }
    }
}
