using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Retro2D;

namespace Match3
{
    public class Cell : Tile
    {
        //public Point _pos = new Point();
        public Gem _gem = Gem.VOID;

        public void Reset()
        {
            _gem.Init();
        }

        public Cell Copy()
        {
            Cell copy = (Cell)MemberwiseClone();
            copy._gem = _gem.Copy();

            return copy;
        }

    }

    public enum Teams
    {
        L = -1,
        R = 1,
    }

    public class PlayGrid : Node
    {
        #region Attributes

        public Area Area { get; private set; }
        public Teams Team { get; private set; }
        PlayGrid _partner; // Coop partner !
        public List<PlayGrid> _opponents = new List<PlayGrid>();


        public Hero _hero { get; private set; }

        Map2D<Cell> _map2D;
        Player _player;
        int _sizeCellW;
        int _sizeCellH;
        public int Level { get; private set; }

        // Manage Shake
        float _prevX;
        float _prevY;

        public int Width { get; private set; }
        public int Height { get; private set; }

        const int MAX_GET_GEM = 6;

        public static Rectangle RectCase;   // Rectangle in pixel of the one Case 
        Point _cursor;                      // Position in case of Cursor
        Point _cursorCase;                  // Position in pixel of Cursor
        int _currentColor = Gem.NULL; // Current color in the Cursor

        Stack<Gem> _gemBank = new Stack<Gem>(); // Gems in bank !

        // Gameplay Status
        bool IsGameOver;
        bool IsLevelUp; // status is level up
        bool IsPossible; // if grid has nb same color < 2
        bool IsAutoInsertRow; // Auto insert when LevelUp;
        bool IsClear;
        bool OnClear;

        bool HasOnlyOneColor; // true if stage contains only one same color !
        int LastGemColor; // Last gem color in stage !

        bool IsDestroy;
        bool IsPushAllUp;
        bool IsPushAtBottom;

        public int NbDestroyGem { get; private set; }

        int _ticCombo;
        int _tempoCombo = 60;
        int ComboPoint;      // current combo point !
        int ComboNeed = 16; // need combo point for active speed bonus !
        bool IsActiveCombo;

        // Gem management
        //Gem LastGemPull; // Last Gem get pull !
        List<Gem> LastGemPushUps = new List<Gem>(); // List of all last gem push up !
        Gem LastGemPushUp; // Last Gem push at bottom : use for determinate the end of all gem push up !!
        Gem LastGemInsert; // Last Gem insert at top : use for determinate the end of all gem row insertion !!
        GemExplose LastGemExplose; // Last Gem insert at top : use for determinate the end of all gem row insertion !!
        GemExplose LastGemExploseSameColor; // Last Gem insert at top : use for determinate the end of all gem row insertion !!

        Dictionary<int, int> _nbGem = new Dictionary<int, int>(); // Get number of Gem as same color !

        // Gameplay Tempo
        int _ticDestroy = 0;
        public static int TempoDestroy = 24;

        int _ticPushAtBottom = 0;
        public static int TempoPushAtBottom = 16;


        Shake _shake = new Shake();

        //Stack<Gem> _stackMapObject = new Stack<Gem>();

        #region ButtonTriggerState

        public bool IS_B_SELECT = false;
        public bool IS_B_START = false;

        public bool IS_B_UP = false;
        public bool IS_B_DOWN = false;
        public bool IS_B_LEFT = false;
        public bool IS_B_RIGHT = false;
        public bool IS_B_L = false;
        public bool IS_B_R = false;
        public bool IS_B_A = false;
        public bool IS_B_B = false;
        public bool IS_B_X = false;
        public bool IS_B_Y = false;

        public bool IS_PUSH_B_SELECT = false;
        public bool IS_PUSH_B_START = false;

        public bool IS_PUSH_B_UP = false;
        public bool IS_PUSH_B_DOWN = false;
        public bool IS_PUSH_B_LEFT = false;
        public bool IS_PUSH_B_RIGHT = false;
        public bool IS_PUSH_B_L = false;
        public bool IS_PUSH_B_R = false;
        public bool IS_PUSH_B_A = false;
        public bool IS_PUSH_B_B = false;
        public bool IS_PUSH_B_X = false;
        public bool IS_PUSH_B_Y = false;

        public bool ON_B_SELECT = false;
        public bool ON_B_START = false;

        public bool ON_B_UP = false;
        public bool ON_B_DOWN = false;
        public bool ON_B_LEFT = false;
        public bool ON_B_RIGHT = false;
        public bool ON_B_L = false;
        public bool ON_B_R = false;
        public bool ON_B_A = false;
        public bool ON_B_B = false;
        public bool ON_B_X = false;
        public bool ON_B_Y = false;

        #endregion

        #endregion

        public PlayGrid Shake(float intensity = 1, float step = .05f)
        {
            _shake.SetIntensity(intensity, step);
            return this;
        }

        public PlayGrid(Area area, Player player, int x, int y, int level = Gem.MAX_COLOR, int Width = 7, int Height = 8, int sizeCellW = Game1.CELL_SIZEW, int sizeCellH = Game1.CELL_SIZEH)
        {
            _hero = new Hero(80, Hero.Avatars.BOY);

            IsGameOver = false;

            Area = area;
            _player = player;

            SetPosition(x, y);
            SetSize(Width * sizeCellW, Height * sizeCellH);
            //SetPivot(Position.TOP_CENTER);

            _prevX = _x;
            _prevY = _y;

            Level = level;
            this.Width = Width;
            this.Height = Height;
            _sizeCellW = sizeCellW;
            _sizeCellH = sizeCellH;

            _map2D = new Map2D<Cell>(Width, Height, sizeCellW, sizeCellH);

            RectCase = new Rectangle(0, 0, sizeCellW, sizeCellH);

            SetCursorPosition(0, this.Height - 1);
        }
        public override Node Init()
        {
            IsGameOver = false;

            // Reset all gameplay variables !
            _hero.Init();
            ComboPoint = 0;
            _currentColor = Gem.NULL;

            _gemBank.Clear();


            _cursor.X = Width / 2;

            ClearStage();

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    //int color = Misc.Rng.Next(0,Gem.MAX_COLOR);
                    //Gem gem = new Gem(i, j, color);
                    //_areaGame[p].Add(gem);
                    AddRandomCell(i, j, new List<int>() { 0, 1});
                }
            }

            return this;
        }
        public PlayGrid ClearStage()
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    Cell cell = GetCell(i, j);

                    if (null != cell)
                    {
                        cell.Reset();
                    }
                }
            }
            return this;
        }
        public PlayGrid SetAvatar(Hero.Avatars avatar)
        {
            _hero.Avatar = avatar;
            return this;
        }
        public PlayGrid SetTeam(Teams team)
        {
            Team = team;
            return this;
        }
        public PlayGrid SetPartner(PlayGrid partner)
        {
            _partner = partner;
            return this;
        }
        public PlayGrid AddOpponents(PlayGrid[] opponents)
        {
            for (int i = 0; i < opponents.Length; i++)
                _opponents.Add(opponents[i]);

            return this;
        }
        public PlayGrid SetLevel(int level) { Level = level;  return this; }
        public PlayGrid LevelUp()
        {
            Level++;
            if (Level > Gem.MAX_COLOR - 1)
            {
                Level = Gem.MAX_COLOR - 1;

                // Debug: Oppenent Lose if reach Max Level 
                //foreach(var opponent in _opponents)
                //{
                //    opponent.GameOver();
                //}
            }

            IsLevelUp = true;

            return this;
        }
        public PlayGrid AddGemInGrid(Gem gem)
        {
            _map2D.Put(gem._mapX, gem._mapY,new Cell() { _gem = gem });

            gem.AppendTo(this)
                .SetPosition(gem._mapX * _sizeCellW + _sizeCellW / 2, gem._mapY * _sizeCellH + _sizeCellH / 2);

            return this;
        }
        //public PlayGrid ResetGemInGrid(Gem gem)
        //{
        //    _map2D.Put(gem._mapX, gem._mapY, new Cell() { _gem = gem });

        //    return this;
        //}
        public Gem AddRandomCell(int mapX, int mapY, List<int> validColors)
        {
            if (validColors.Count < 1) return null;

            int index = 0;

            if (validColors.Count > 1)
                index = Misc.Rng.Next(0, validColors.Count);

            //int color = Misc.Rng.Next(0, Level);
            int color = validColors[index];

            Gem gem = new Gem(mapX, mapY, color);

            AddGemInGrid(gem);

            return gem;
        }
        public PlayGrid SetCursorPosition(int mapX, int mapY)
        {
            _cursor.X = mapX;
            _cursor.Y = mapY;

            return this;
        }
        public PlayGrid CursorMoveX(int direction)
        {
            bool canMove = false;

            if (direction > 0 && _cursor.X < Width - 1)
            {
                _cursor.X += direction;
                canMove = true;
            }
            if (direction < 0 && _cursor.X > 0)
            {
                _cursor.X += direction;
                canMove = true;
            }

            if (canMove)
                Game1._sound_Tic.Play(.1f, .5f, 0);

            return this;
        }

        public PlayGrid AddCombo(int combo)
        {
            ComboPoint += combo;

            if (ComboPoint >= ComboNeed)
            {
                ComboPoint = 0;
                IsActiveCombo = true;
            }

            return this;
        }
        public PlayGrid ResetCombo()
        {
            ComboPoint = 0;
            return this;
        }

        // Manage Cells
        public Cell GetCell(int mapX, int mapY)
        {
            if (null != _map2D.Get(mapX, mapY))
                return _map2D.Get(mapX, mapY);

            return null;
        }
        public void SetCell(int mapX, int mapY, Cell cell)
        {
            if (mapX < 0 || mapX > Width || mapY < 0 || mapY > Height) return;

            _map2D.Put(mapX, mapY, cell);
        }

        // Manage Gems
        Gem PushAtBottom(Gem gemToPush, int mapX = -1, bool scan = true)
        {
            if (mapX == -1) mapX = _cursor.X;

            int row = GetBottomCellRow(mapX);

            Gem gem = null;

            if (gemToPush._color != Gem.NULL)
            {
                gem = gemToPush.Copy();
                gem.SetMapPosition(mapX, row);

                AddGemInGrid(gem);

                gemToPush.Init();
                gemToPush.KillMe();

                gem.Shake(3, .05f);


                // Scan & Destroy neightboor if same color
                if (scan)
                {
                    ResetScanColor();

                    ScanSameColor(mapX, row, gem._color);
                    ScanNbSameColor();
                }

                IsPushAtBottom = true;

                LastGemPushUp = gem;

                return gem;
            }

            return gem;
        }
        bool ScanSameColor(int mapX, int mapY, int color) // Scan the neighboor if same color !
        {
            if (mapX < 0 || mapX > Width || mapY < 0 || mapY > Height) return false;

            // parent
            Cell cell = GetCell(mapX, mapY);

            if (null != cell)
            {
                if (!cell._gem._alreadyScan)
                {
                    cell._gem._alreadyScan = true;

                    if (cell._gem._color == color) // if same color 
                    {
                        cell._gem._isSameColor = true;

                        // check if children have same color
                        bool N = ScanSameColor(mapX, mapY - 1, color);
                        bool S =ScanSameColor(mapX, mapY + 1, color);
                        bool W = ScanSameColor(mapX - 1, mapY, color);
                        bool E = ScanSameColor(mapX + 1, mapY, color);


                        return N || S || W || E;

                    }
                }
            }

            return false; // not same color
        }
        int ScanNbSameColor()
        {


            int nbSameColor = 0;

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    Cell cell = GetCell(i, j);

                    if (null != cell)
                    {
                        if (cell._gem._isSameColor)
                            nbSameColor++;
                    }
                }
            }

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    Cell cell = GetCell(i, j);

                    if (null != cell)
                    {
                        if (cell._gem._isSameColor)
                            cell._gem._nbSameColor = nbSameColor;
                    }
                }
            }

            return nbSameColor;
        }
        void ResetScanColor()
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    Cell cell = GetCell(i, j);

                    if (null != cell)
                    {
                        ResetScanGem(cell._gem);
                    }
                }
            }
        }
        void ResetScanGem(Gem gem)
        {
            gem._alreadyScan = false;
            gem._nbSameColor = 0;
            gem._isSameColor = false;
        }
        void DestroyAllSameColor(int nbColorMini)
        {
            NbDestroyGem = 0; // reset nb nb destroyed gem !

            bool playSound = false;

            for (int i=0; i<Width; i++)
            {
                for (int j=0; j<Height; j++)
                {
                    Cell cell = GetCell(i, j);

                    if (null != cell)
                    {
                        if (cell._gem._nbSameColor > nbColorMini)
                        {
                            if (Team == Teams.L)
                                LastGemExplose = new GemExplose(this, Area.AbsLeftGem(cell._gem._color), cell._gem._mapX, cell._gem._mapY, cell._gem._color);
                            else
                                LastGemExplose = new GemExplose(this, Area.AbsRightGem(cell._gem._color), cell._gem._mapX, cell._gem._mapY, cell._gem._color);


                            LastGemExplose.AppendTo(this);

                            cell.Reset();

                            playSound = true;

                            IsDestroy = true;

                            NbDestroyGem++;
                        }
                    }

                }
            }

            if (playSound) Game1._sound_Pop.Play(.4f, .01f, 0);

        }
        void DestroyAllSameColor(int color, int nbColorMini)
        {
            NbDestroyGem = 0; // reset nb nb destroyed gem !

            bool playSound = false;

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    Cell cell = GetCell(i, j);

                    if (null != cell)
                    {
                        if ((cell._gem._color == color && HasOnlyOneColor) || cell._gem._nbSameColor > nbColorMini) 
                        {
                            if (Team == Teams.L)
                                LastGemExploseSameColor = new GemExplose(this, Area.AbsLeftGem(cell._gem._color), cell._gem._mapX, cell._gem._mapY, cell._gem._color);
                            else
                                LastGemExploseSameColor = new GemExplose(this, Area.AbsRightGem(cell._gem._color), cell._gem._mapX, cell._gem._mapY, cell._gem._color);

                            LastGemExploseSameColor.AppendTo(this);

                            cell.Reset();

                            playSound = true;

                            IsDestroy = true;

                            NbDestroyGem++;
                        }
                    }

                }
            }

            if (playSound) Game1._sound_Pop.Play(.4f, .01f, 0);

        }
        bool PushAllUp()
        {
            bool isPush = false;

            LastGemPushUps.Clear();

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    if (j > GetBottomCellRow(i) - 1)
                    {
                        Cell cell = GetCell(i, j);

                        if (null != cell)
                        {
                            //Gem gemBottom = PushAtBottom(cell._gem._color, i, false);
                            Gem gemBottom = PushAtBottom(cell._gem, i, true);

                            // Get position of the Gem for start Move
                            int fromMapX = cell._gem._mapX;
                            int fromMapY = cell._gem._mapY;

                            if (null != gemBottom)
                            {
                                // get position of the Bottom Gem for goal Move
                                int toMapX = gemBottom._mapX;
                                int toMapY = gemBottom._mapY;

                                isPush = true;
                                gemBottom.SetMapPosition(fromMapX, fromMapY).MoveTo(toMapX, toMapY, 4);

                                //LastGemPushUp = gemBottom;

                                LastGemPushUps.Add(gemBottom);

                                cell.Reset();
                            }

                        }
                    }

                }
            }

            int g = 0;
            foreach (var gem in LastGemPushUps)
            {
                Console.WriteLine("PushAllUp {0} = {1} position : {2}",g, gem._color, gem._mapX);
                g++;
            }

            return isPush;
        }
        void InsertRow(int durationMove = 8)
        {
            for (int i = 0; i < Width; i++)
            {
                //for (int j = 0; j < Height - 1; j++)
                for (int j = Height; j > -1; j--)
                {
                    Cell cell = GetCell(i, j);

                    if (null != cell)
                    {
                        cell._gem.MoveTo(i, j + 1, durationMove);
                    }

                }
            }

            for (int i = 0; i < Width; i++)
            {
                //for (int j = 0; j < Height - 1; j++)
                for (int j = Height; j > -1; j--)
                {
                    Cell cell = GetCell(i, j);
                    //Cell cellDown = GetCell(i, j + 1);

                    if (null != cell)
                    {
                        SetCell(i, j+1, cell);
                    }

                }
            }

            List<int> validColors = new List<int>();

            // Manage Valid color
            for (int i = 0; i < Level + 1; i++ )
            {
                
                if (_nbGem.ContainsKey(i))
                    validColors.Add(i);
                
                // if gem bank is not empty add _currentColor to RNG
                if (_gemBank.Count > 0)
                    validColors.Add(_currentColor);
            }

            if (!IsAutoInsertRow)
            {
                for (int i = 0; i < _nbGem.Count; i++)
                //for (int i = 0; i < Level + 1; i++)
                {
                    if (_nbGem[i] == 0) // Add valid color only if exist one gem as same color
                        validColors.Remove(i);
                }
            }

            Console.WriteLine("InsertRow validColors = "+ validColors.Count);

            for (int i = 0; i < Width; i++)
            {
                //SetCell(i, 0, new Cell());
                Gem gem = AddRandomCell(i, 0, validColors);
                
                if (null != gem)
                {
                    LastGemInsert = gem.SetMapPosition(i,-1).MoveTo(i,0, durationMove);
                }

            }


        }

        public Cell GetBottomCell(int mapX = -1)
        {
            if (mapX == -1) mapX = _cursor.X;

            Cell bottomCell = GetCell(mapX, 0);

            for (int row = 0; row < Height; row++)
            {
                Cell rowCell = GetCell(mapX, row);
                if (rowCell._gem._color == Gem.NULL)
                    break;

                bottomCell = rowCell;
            }

            return bottomCell;
        }
        public int GetBottomCellRow(int mapX = -1) // Return row position where the case is NULL
        {
            if (mapX == -1) mapX = _cursor.X;

            int bottomRow = 0;

            for (int row = 0; row < Height; row++)
            {
                bottomRow = row;

                Cell rowCell = GetCell(mapX, row);

                if (rowCell._gem._color == Gem.NULL)
                    break;

                if (null == rowCell)
                    break;

            }

            return bottomRow;
        }
        public int GetNbFreeCell(int mapX = -1)
        {
            if (mapX == -1) mapX = _cursor.X;

            int nbFreeCell = 0;

            for (int row = GetBottomCellRow(mapX); row < Height; row++)
            {
                Cell rowCell = GetCell(mapX, row);
                if (rowCell._gem._color == Gem.NULL)
                {
                    ++nbFreeCell;
                }
            }

            return nbFreeCell;
        }
        public int GetNbCell(int mapX = -1)
        {
            if (mapX == -1) mapX = _cursor.X;

            int nbCell = 0;

            for (int row = 0; row > Height; row++)
            {
                Cell rowCell = GetCell(mapX, row);
                if (rowCell._gem._color == Gem.NULL)
                {
                    ++nbCell;
                }
            }

            return nbCell;
        }
        public List<Gem> GetAllFocusGems(int mapX = -1)
        {
            if (mapX == -1) mapX = _cursor.X;

            List<Gem> listGem = new List<Gem>();

            for (int row = 0; row < Height; row++)
            {
                Cell rowCell = GetCell(mapX, row);

                if (rowCell._gem._color != Gem.NULL)
                {
                    listGem.Add(rowCell._gem);    
                }
            }
            //Console.WriteLine("listGem = "+ listGem.Count);

            return listGem;
        }
        public bool IsCanInsert()
        {
            bool canInsert = true;
            for (int i = 0; i < Width; i++)
            {
                Cell cell = GetCell(i, Height - 3);

                if (null != cell)
                    if (cell._gem._color != Gem.NULL)
                    {
                        canInsert = false;

                        IsAutoInsertRow = false; // Stop Auto insertion when can't insert more row of gems !!

                        break;
                    }
            }

            return canInsert;
        }

        public PlayGrid GameOver()
        {
            IsGameOver = true;
            return this;
        }

        public override Node Update(GameTime gameTime)
        {
            #region Manage Button Status

            IS_B_SELECT = _player.GetButton((int)SNES.BUTTONS.SELECT) !=0;
            IS_B_START = _player.GetButton((int)SNES.BUTTONS.START) != 0;

            IS_B_UP = _player.GetButton((int)SNES.BUTTONS.UP) != 0;
            IS_B_DOWN = _player.GetButton((int)SNES.BUTTONS.DOWN) != 0;
            IS_B_LEFT = _player.GetButton((int)SNES.BUTTONS.LEFT) != 0;
            IS_B_RIGHT = _player.GetButton((int)SNES.BUTTONS.RIGHT) != 0;

            IS_B_L = _player.GetButton((int)SNES.BUTTONS.L) != 0;
            IS_B_R = _player.GetButton((int)SNES.BUTTONS.R) != 0;

            IS_B_A = _player.GetButton((int)SNES.BUTTONS.A) != 0; // If hero is selected then can't control A !
            IS_B_B = _player.GetButton((int)SNES.BUTTONS.B) != 0;
            IS_B_X = _player.GetButton((int)SNES.BUTTONS.X) != 0;
            IS_B_Y = _player.GetButton((int)SNES.BUTTONS.Y) != 0;

            //Game1.Trigger(ref ON_B_LEFT, ref IS_B_LEFT, ref IS_PUSH_B_LEFT);
            //Game1.Trigger(ref ON_B_RIGHT, ref IS_B_RIGHT, ref IS_PUSH_B_RIGHT);

            ON_B_SELECT = Input.Button.OnePress("SELECT" + _player._name, IS_B_L) && !IsGameOver;
            ON_B_START = Input.Button.OnePress("START" + _player._name, IS_B_L) && !IsGameOver;

            ON_B_L = Input.Button.OnePress("L" + _player._name, IS_B_L) && !IsGameOver;
            ON_B_R = Input.Button.OnePress("R" + _player._name, IS_B_L) && !IsGameOver;

            ON_B_A = Input.Button.OnePress("A" + _player._name, IS_B_A) && !IsGameOver;
            ON_B_B = Input.Button.OnePress("B" + _player._name, IS_B_B) && !IsGameOver;
            ON_B_X = Input.Button.OnePress("X" + _player._name, IS_B_X) && !IsGameOver;
            ON_B_Y = Input.Button.OnePress("Y" + _player._name, IS_B_Y) && !IsGameOver;

            ON_B_LEFT = Input.Button.OnPress("LEFT" + _player._name, IS_B_LEFT, 8) && !IsGameOver;
            ON_B_RIGHT = Input.Button.OnPress("RIGHT" + _player._name, IS_B_RIGHT, 8) && !IsGameOver;

            #endregion

            #region Button Events
            
            // Button : Insert Row
            if (ON_B_L && !IsPushAtBottom)
            {
                if (IsCanInsert())
                {
                    IsAutoInsertRow = false;

                    InsertRow();
                    Console.WriteLine("Insert Row ---");

                    //Game1._sound_Hit.Play(.1f, .001f, 0);

                    ResetCombo();
                }
            }

            // Button : Get the Ball
            if (ON_B_B && !IsAutoInsertRow)
            {
                Cell bottomCell = GetBottomCell();

                // if _cursor is empty
                if (_currentColor == Gem.NULL)
                {
                    _currentColor = bottomCell._gem._color;

                    if (_currentColor != Gem.NULL)
                    {
                        foreach (Gem gem in GetAllFocusGems()) gem.Shake(3, .05f); // Shake all gems in current column !

                        // Get all same color
                        while (bottomCell._gem._color == _currentColor)
                        {
                            //_nbCurrentColor++;

                            //if (_nbCurrentColor > MAX_GET_GEM) break;

                            Gem gemCopy = bottomCell._gem.Copy();
                            _gemBank.Push(gemCopy);

                            bottomCell.Reset();
                            bottomCell = GetBottomCell();

                        }

                        Game1._sound_Bubble.Play(.1f, .1f, 0);
                    }
                }
                // if _cursor contain ball as same color
                else
                {
                    foreach (Gem gem in GetAllFocusGems()) gem.Shake(3, .05f); // Shake all gems in current column !

                    // Get all same color
                    while (bottomCell._gem._color == _currentColor)
                    {
                        //_nbCurrentColor++;

                        //if (_nbCurrentColor > MAX_GET_GEM) break;

                        Gem gemCopy = bottomCell._gem.Copy();
                        _gemBank.Push(gemCopy);

                        bottomCell.Reset();
                        bottomCell = GetBottomCell();
                    }
                    Game1._sound_Bubble.Play(.1f, .1f, 0);

                }

                //int i = 0;
                //foreach(var gem in _gemBank)
                //{
                //    Console.WriteLine("_gemBank[{0}] = {1} : Parent = {2}" , i, gem._color, gem._parent);
                //    i++;
                //}

            }
            
            // Button : Push Current Gems
            if (ON_B_A && !IsAutoInsertRow)
            {
                bool playSound = false;
                while (_gemBank.Count > 0)
                {
                    if (GetNbFreeCell() < 2)
                    {

                        break;
                    }

                    Gem gemInBank = _gemBank.Pop();

                    //if (null != PushAtBottom(_currentColor)) // Succes or not to add ball
                    if (null != PushAtBottom(gemInBank)) // Succes or not to add ball
                    {
                        //_nbCurrentColor--;

                        
                        foreach (Gem gem in GetAllFocusGems()) gem.Shake(16, .5f, false); // Shake all gems in current column !

                        playSound = true;
                    }
                }

                if (playSound) Game1._sound_BlockHit.Play(.2f, .1f, 0);

                if (_gemBank.Count < 1)
                    _currentColor = Gem.NULL;

            }

            // Buton : Send Or Receive Gem to partner "SHARE"
            if (ON_B_Y)
            {
                //Console.WriteLine("Send or Receive Gem to or from partner !! "+ _partner._currentColor);

                // Receive Gem from partner
                if (_gemBank.Count == 0 && _partner._gemBank.Count > 0 && _partner.IS_B_Y)
                {
                    //_nbCurrentColor = _partner._nbCurrentColor;
                    _currentColor = _partner._currentColor;

                    //_partner._nbCurrentColor = 0;
                    _partner._currentColor = Gem.NULL;

                    foreach(var gem in _partner._gemBank)
                    {
                        _gemBank.Push(gem);
                    }
                    _partner._gemBank.Clear();


                    Game1._sound_ShareGem.Play(.2f, .005f, 0);
                }
                else
                // Send gem to partner
                if (_gemBank.Count > 0 && _partner._gemBank.Count == 0 && _partner.IS_B_Y)
                {
                    //_partner._nbCurrentColor = _nbCurrentColor;
                    _partner._currentColor = _currentColor;

                    //_nbCurrentColor = 0;
                    _currentColor = Gem.NULL;

                    foreach (var gem in _gemBank)
                    {
                        _partner._gemBank.Push(gem);
                    }
                    _gemBank.Clear();

                    Game1._sound_ShareGem.Play(.2f, .005f, 0);
                }

            }

            // Button : Move Cursor
            if (ON_B_LEFT)
            {
                CursorMoveX(-1);
                foreach (Gem gem in GetAllFocusGems()) gem.Shake(2, .05f); // Shake all gems in current column !
            }
            if (ON_B_RIGHT)
            {
                CursorMoveX(+1);
                foreach (Gem gem in GetAllFocusGems()) gem.Shake(2, .05f); // Shake all gems in current column !
            }
            #endregion

            // Manage GameOver
            if (_hero.Energy == 0)
            {
                GameOver();
            }

            // Tempo before Destroy
            if (IsPushAtBottom)
            {
                _ticPushAtBottom++;
                if (_ticPushAtBottom > TempoPushAtBottom)
                {
                    _ticPushAtBottom = 0;
                    IsPushAtBottom = false;

                    DestroyAllSameColor(2);

                    AddCombo(NbDestroyGem); // Add to Combo gauge !
                }
            }
            // Tempo before Push All Up
            if (IsDestroy)
            {
                _ticDestroy++;
                if (_ticDestroy > TempoDestroy)
                {
                    _ticDestroy = 0;

                    IsDestroy = false;
                    IsPushAllUp = true;

                    //if (_hero.AddModePoint(_hero.Mode, NbDestroyGem))
                    //{
                    //    Console.WriteLine("Active :" + _hero.Mode);

                    //    foreach(var opponent in _opponents)
                    //    {
                    //        opponent.Shake(16, .2f);
                    //        //opponent._hero.AddEnergy(-Level*10);

                    //        // Add Attack


                    //    }
                    //}

                    //AddUnit(LastGemExplose._color);
                }
            }
            // After destroy Push All Up
            if (IsPushAllUp)
            {
                IsPushAllUp = false;

                if (PushAllUp())
                {
                    //Game1._sound_BlockHit.Play(.4f, 1f, 0);
                }
            }

            // Scan all Gem Push All Up and destroy if nbSameColor is > 2
            if (LastGemPushUps.Count > 0 && !HasOnlyOneColor)
            {
                for (int i=0; i<LastGemPushUps.Count; i++)
                {
                    Gem gem = LastGemPushUps[i];

                    if (gem._color != Gem.NULL)
                    {
                        if (gem.OnGoal)
                        {
                            DestroyAllSameColor(gem._color, 2);

                            AddCombo(NbDestroyGem); // Add to Combo gauge !
                        }
                    }

                }
            }


            // Sound when finish all push up
            if (null != LastGemPushUp)
            {
                if (LastGemPushUp.OnGoal)
                    Game1._sound_BlockHit.Play(.2f, 1f, 0);
            }


            // When finish insert
            if (null != LastGemInsert)
            {
                if (LastGemInsert.OnGoal)
                    Game1._sound_BlockHit.Play(.2f, 1f, 0);

            }

            OnClear = false;

            // Check if Clear Stage
            int nbGem = 0;
            // Reset number of each color gem
            for (int i = 0; i < Gem.MAX_COLOR; i++)
            {
                _nbGem[i] = 0;
            }

            foreach (var Node in GroupOf(UID.Get<Gem>()))
            {
                Gem gem = Node.This<Gem>();

                if (gem._color != Gem.NULL)
                {
                    ++nbGem;
                    ++_nbGem[gem._color];

                    gem.IsWarning = false; // Reset Warning !
                    gem.IsSameColorAsCurrent = false; // Reset IsSameColor ! 
                }
            }

            IsPossible = true;

            // Mark warning if gem number are under < 3
            for (int color = 0; color < Gem.MAX_COLOR; color++)
            {
                if (_nbGem[color] < 3)
                {
                    foreach (var Node in GroupOf(UID.Get<Gem>()))
                    {
                        Gem gem = Node.This<Gem>();

                        if (gem._color == color)
                        {
                            gem.IsWarning = true; // Set Warning !
                            IsPossible = false; // Set False if impossible Game !
                        }

                        if (gem._color == _currentColor)
                        {
                            if (_currentColor != Gem.NULL)
                                gem.IsSameColorAsCurrent = true;
                        }
                    }
                }
            }

            if (IsAutoInsertRow)
            {
                if (IsPossible)
                    IsAutoInsertRow = false;

                if (IsCanInsert() && LastGemInsert.OnGoal)
                {
                    InsertRow(12);
                }

            }

            if (IsLevelUp && !IsDestroy)
            {
                IsLevelUp = false;

                IsAutoInsertRow = true;
                InsertRow();
            }

            if (nbGem < 1 && _gemBank.Count == 0) // Set Is Clear if nbGem = 0 & no Gem in cursor
            {
                if (!IsClear)
                    OnClear = true;

                IsClear = true;
            }
            else
            {
                IsClear = false;
            }

            // When Level is clear
            if (OnClear)
            {
                //Console.WriteLine("Level UP !");
                LevelUp();

                new PopInfo("** NEXT LEVEL **", Color.YellowGreen, 0, 120, 128)
                    .SetPosition(_rect.Width / 2, _rect.Height / 2)
                    .AppendTo(this);

                Game1._sound_LevelUp.Play(.4f, .01f, 0);

                // Add a column Units
                for(int i=0; i < Level + 1; i++)
                {
                    Unit unit = Area.AddUnit(this, i, i, .5f, 32 + NbDestroyGem);
                    new Smoke(unit._x, unit._y, 16, true, false, 16, 16)
                        .AppendTo(Area);
                }


            }


            // Manage ghost GEM when explose
            if (null != LastGemExplose)
            {
                if (LastGemExplose.OnGoal)
                {
                    // Manage GemExplose reach Area Gem
                    Unit unit = Area.AddUnit(this, LastGemExplose._color, LastGemExplose._color, .5f, 32 + NbDestroyGem);
                    new Smoke(unit._x, unit._y, 16, true, false, 16, 16)
                        .AppendTo(Area);
                    Game1._sound_Hit.Play(.04f, .05f, 0);

                    //Console.WriteLine("Add Unit : {0}, {1}", unit._x, unit._y);

                    LastGemExplose = null;

                }
            }

            // Manage ghost GEM when clear last same color
            if (null != LastGemExploseSameColor)
            {
                if (LastGemExploseSameColor.OnGoal)
                {
                    // Manage GemExplose reach Area Gem
                    Unit unit = Area.AddUnit(this, LastGemExploseSameColor._color, LastGemExploseSameColor._color, .5f, 32 + NbDestroyGem);
                    new Smoke(unit._x, unit._y, 16, true, false, 16, 16)
                        .AppendTo(Area);
                    Game1._sound_Hit.Play(.04f, .05f, 0);

                    //Console.WriteLine("Add Unit : {0}, {1}", unit._x, unit._y);

                    LastGemExploseSameColor = null;

                }
            }



            // Scan Last same color Gem : Destroy them all and clear stage
            if (!IsClear && _gemBank.Count == 0)
            {
                LastGemColor = Gem.NULL;

                for (int i = 0; i < _nbGem.Count; i++)
                {
                    if (_nbGem[i] > 0)
                    {
                        if (LastGemColor == Gem.NULL)
                        {
                            LastGemColor = i;

                            HasOnlyOneColor = true;
                        }
                        else
                            HasOnlyOneColor = false;
                    }
                }

                if (HasOnlyOneColor && !IsDestroy)
                {
                    if (LastGemColor != Gem.NULL)
                        DestroyAllSameColor(LastGemColor, 0);
                }
            }

            _cursorCase.X = _cursor.X * _sizeCellW + (int)_x;
            _cursorCase.Y = _cursor.Y * _sizeCellH + (int)_y;

            // Manage Shake !
            if (_shake.IsShake)
            {
                _x = _prevX + _shake.GetVector2().X;
                _y = _prevY + _shake.GetVector2().Y;
            }
            else
            {
                _x = _prevX;
                _y = _prevY;
            }

            // Combo Gauge decrease in time !
            _ticCombo++;
            if (_ticCombo > _tempoCombo)
            {
                _ticCombo = 0;
                ComboPoint += -1;
                if (ComboPoint < 0)
                    ComboPoint = 0;
            }
            if (IsActiveCombo)
            {
                IsActiveCombo = false;
                Console.WriteLine("Active Combo");

                new PopInfo("* SPEED BONUS *", Color.BlueViolet, 0, 120, 64)
                    .SetPosition(_rect.Width / 2, _rect.Height / 2)
                    .AppendTo(this);

                Game1._sound_Speed.Play(.4f, .01f, 0);

                // Add a column Units
                for (int i = 0; i < Level + 1; i++)
                {
                    Unit unit = Area.AddUnit(this, i, i, .5f, 32 + NbDestroyGem);
                    new Smoke(unit._x, unit._y, 16, true, false, 16, 16)
                        .AppendTo(Area);
                }

                // Add Attack
                //AddUnit(.5f); 

            }


            UpdateRect();

            UpdateChilds(gameTime);

            return base.Update(gameTime);
        }

        public override Node Render(SpriteBatch batch)
        {
            // Fill
            Draw.FillRectangle(batch, AbsRect, Color.Black * .4f);

            // Grid Gems
            Draw.Grid(batch, AbsX, AbsY, AbsRect.Width, AbsRect.Height, _sizeCellW, _sizeCellH, Color.Tan * .4f);

            // Grid Heroes
            //Draw.Grid(batch, AbsX, AbsY - _sizeCellH * 6, _sizeCellW * 6, _sizeCellW * 3, _sizeCellW * 2, _sizeCellH, Color.WhiteSmoke * .8f);

            // Ray
            Draw.FillRectangle
            (
                batch, 
                new Rectangle
                (
                    _cursorCase.X + 20, AbsY + (GetBottomCellRow()+1) * _sizeCellH,  
                    _sizeCellW - 20 * 2, (GetNbFreeCell()-1) * _sizeCellH
                ), 
                Gem.Colors[_currentColor] * .6f
            );

            Draw.Line(batch,
                _cursorCase.X + _sizeCellW / 2, AbsY + (GetBottomCellRow() + 1) * _sizeCellH,
                _cursorCase.X + _sizeCellW / 2, AbsY + (GetBottomCellRow() + 1) * _sizeCellH + (GetNbFreeCell() - 1) * _sizeCellH, 
                Gem.Colors[_currentColor] * .8f, 
                8);

            // Cursor
            //batch.DrawRectangle(Gfx.TranslateRect(RectCase, _cursorCase), Color.Red, 1);
            //batch.DrawRectangle(new Rectangle(_cursorCase.X, AbsY + GetBottomCellRow() * _sizeCellH, _sizeCellW, _sizeCellH), Color.LightGoldenrodYellow);

            //if (_gemBank.Count < 1)
            //    batch.Draw(
            //        Game1._tex_Atlas,
            //        new Rectangle(_cursorCase.X, AbsY + GetBottomCellRow() * _sizeCellH, _sizeCellW, _sizeCellH),
            //        new Rectangle(320, 128, 64, 64),
            //        Gem.Colors[_currentColor] * 1f);
            //else
            batch.Draw(
                    Game1._tex_Atlas,
                    new Rectangle(_cursorCase.X, AbsY + GetBottomCellRow() * _sizeCellH, _sizeCellW, _sizeCellH),
                    new Rectangle(256, 128, 64, 64),
                    _gemBank.Count > 0?Gem.Colors[_currentColor]: Color.White);
                    //Gem.Colors[_currentColor] * 1f);

            Draw.CenterBorderedStringXY(batch, Game1._font_Big, _gemBank.Count.ToString(),
                _cursorCase.X + _sizeCellW / 2, 
                //_cursorCase.Y + _sizeCellH / 2,
                AbsY + GetBottomCellRow() * _sizeCellH + _sizeCellH / 2,
                Gem.Colors[_currentColor], Color.Black);


            // Contents
            SortZD();
            RenderChilds(batch);

            // Border
            //batch.DrawRectangle(_rect, Gem.Colors[_currentColor], 4);

            // Combo Bar
            Draw.FillRectangle(batch, new Rectangle(AbsX - 10, AbsY,10, ComboNeed * 4), Color.Black);
            Draw.FillRectangle(batch, new Rectangle(AbsX - 10, AbsY,10, ComboPoint * 4), Color.RoyalBlue);
            Draw.Rectangle(batch, new Rectangle(AbsX - 10, AbsY,10, ComboNeed * 4), Color.Gray, 2);

            // Debug :


            //Frame frame = Game1._animation_Ball.Get(GetBottomCase(_cursor.X)._mapObject._color);
            //Frame frame = Game1._animation_Ball.Get(_currentColor);


            // Draw Grabbed Gem ! 
            //if (null != frame)
            {
                //Game1._animation_Ball.Draw(batch, frame, _cursorCase.X + _sizeCell / 2, _cursorCase.Y + _sizeCell / 2, Color.White);
                Draw.FillRectangle(
                    batch,
                    new Rectangle
                    (
                        AbsX, AbsY + AbsRect.Height,
                        AbsRect.Width, _sizeCellW
                    ),
                    Gem.Colors[_currentColor] * .6f);

                int i = 0;
                foreach (var gem in _gemBank)
                {
                    int centerX = (int)_rect.Width / 2 - (_gemBank.Count * _sizeCellW / 2);

                    //Game1._animation_Ball.Draw(batch, frame, AbsX + _sizeCell/2 + i * 64 + center, AbsY + AbsRect.Height + _sizeCell/2, Color.White);
                    //Gem gem = new Gem(0, 0, _currentColor);

                    gem.SetPosition(_x + i * Game1.CELL_SIZEW + centerX + _sizeCellW / 2, _y + (int)_rect.Height + _sizeCellH / 2);
                    //gem.SetPosition(_x + i * Game1.CELL_SIZEW + centerX + _sizeCellW / 2, _y +  20 + _sizeCellH / 2);

                    //batch.DrawCircle(gem.XY, 32, 8, Color.White, 2);

                    Gem.Draw(batch,gem._x, gem._y, gem);

                    i++;
                }

            }

            //string content = (Gem.NULL != GetCell(_cursor.X, 0)._gem._type) ? GetCell(_cursor.X, 0).ToString() : "null";
            //string content = GetBottomCell()._gem._color.ToString();

            //Draw.CenterStringXY(batch, Game1._font_Main, "[" + GetNbFreeCell() + "]", _cursorCase.X + 16, _cursorCase.Y - 18, Color.Yellow);
            //Draw.CenterStringXY(batch, Game1._font_Main, "NB : " + _nbCurrentColor.ToString(), _cursorCase.X + 16, _cursorCase.Y - 8, Color.Yellow);

            //Draw.CenterStringXY(batch, Game1._font_Main, _cursor.X.ToString(), _cursorCase.X + 16, _cursorCase.Y + 8, Color.Yellow);
            //Draw.CenterStringXY(batch, Game1._font_Main, GetBottomCellRow().ToString(), _cursorCase.X + 16, _cursorCase.Y + 24, Color.Yellow);


            // Debug : Draw all Gem Color
            //for (int i = 0; i < Width; i++)
            //{
            //    for (int j = 0; j < Height; j++)
            //    {
            //        Cell cell = GetCell(i, j);

            //        if (null != cell)
            //        {
            //            if (cell._gem._color != Gem.NULL)
            //                Draw.CenterBorderedStringXY
            //                (
            //                    batch,
            //                    Game1._font_Main,
            //                    "[" + cell._gem._color + "]",
            //                    //cell._gem._mapX * _sizeCell + _sizeCell / 2 + AbsX,
            //                    //cell._gem._mapY * _sizeCell + _sizeCell / 2 + AbsY,
            //                    i * _sizeCell + _sizeCell / 2 + AbsX,
            //                    j * _sizeCell + _sizeCell / 2 + AbsY,
            //                    Color.White,
            //                    Color.Black
            //                );
            //        }

            //    }
            //}

            //if (_nbCurrentColor == 1)
            //    Game1._animation_BallInCursor.Draw(batch, 0, _cursorCase.X + _sizeCell / 2, _cursorCase.Y + _sizeCell / 2, Gem.Colors[_currentColor]);
            //if (_nbCurrentColor == 2)
            //    Game1._animation_BallInCursor.Draw(batch, 1, _cursorCase.X + _sizeCell / 2, _cursorCase.Y + _sizeCell / 2, Gem.Colors[_currentColor]);
            //if (_nbCurrentColor == 3)
            //    Game1._animation_BallInCursor.Draw(batch, 2, _cursorCase.X + _sizeCell / 2, _cursorCase.Y + _sizeCell / 2, Gem.Colors[_currentColor]);
            //if (_nbCurrentColor > 3)
            //Game1._animation_BallInCursor.Draw(batch, 3, _cursorCase.X + _sizeCell / 2, _cursorCase.Y + _sizeCell / 2, Gem.Colors[_currentColor]);

            // Draw Heroes
            //batch.Draw(Game1._tex_Atlas, new Vector2(AbsX+100, AbsY - 500), new Rectangle(0, 960, 192, 192), Color.White);
            //batch.Draw(Game1._tex_Atlas, new Vector2(AbsX + 100, AbsY - 440), new Rectangle(192, 960, 192, 192), Color.White);
            //batch.Draw(Game1._tex_Atlas, new Vector2(AbsX+100, AbsY - 380), new Rectangle(0, 960, 192, 192), Color.White);

            // Draw Avatar
            //if (_hero.Avatar == Hero.Avatars.BOY)
            //    batch.Draw(Game1._tex_Atlas, new Rectangle(AbsX + 200, AbsY - 400, 448 / 2, 704 / 2), new Rectangle(0, 192, 448, 704), Color.White);

            //if (_hero.Avatar == Hero.Avatars.GIRL)
            //    batch.Draw(Game1._tex_Atlas, new Rectangle(AbsX + 200, AbsY - 400, 448 / 2, 704 / 2), new Rectangle(448, 192, 448, 704), Color.White);

            // border on Top hide top Cell
            Draw.FillRectangle(batch, new Rectangle(AbsX, AbsY - _sizeCellH, Width * _sizeCellW, _sizeCellH), Color.Black);

            //Draw.BorderedString(batch, Game1._font_Main, "Is Clear       = " + IsClear, new Vector2(AbsX + 10, AbsY - 24), Color.LightSeaGreen, Color.Black);
            //Draw.BorderedString(batch, Game1._font_Main, "Can Insert Row = " + IsCanInsert(), new Vector2(AbsX + 10, AbsY - 48), Color.Orange, Color.Black);
            //Draw.BorderedString(batch, Game1._font_Main, "Is Destroy     = " + IsDestroy, new Vector2(AbsX + 10, AbsY - 72), Color.Orange, Color.Black);
            //Draw.BorderedString(batch, Game1._font_Main, "Push At bottom = " + IsPushAtBottom, new Vector2(AbsX + 10, AbsY - 96), Color.Orange, Color.Black);

            //Draw.BorderedString(batch, Game1._font_Main, "Combo = " + ComboPoint, new Vector2(AbsX + 10, AbsY - 186), Color.Yellow, Color.Black);

            //Draw.BorderedString(batch, Game1._font_Main, "Nb Destroy Gem = " + NbDestroyGem, new Vector2(AbsX + 10, AbsY - 144), Color.Yellow, Color.Black);

            //Draw.BorderedString(batch, Game1._font_Main, "Nb Same Color = " + ScanNbSameColor(), new Vector2(AbsX + 10, AbsY - 144), Color.Yellow, Color.Black);

            batch.DrawString(Game1._font_Big, "Level " + Level + " Team "+ Team, new Vector2(AbsX + 10, AbsY - 64), Gem.Colors[Level]);

            //batch.DrawString(Game1._font_Main, "Possible = " + IsPossible, new Vector2(AbsX + 10, AbsY - 120), Color.LightBlue);
            //batch.DrawString(Game1._font_Main, "HasOnlyOneColor = " + HasOnlyOneColor, new Vector2(AbsX + 10, AbsY - 120), Color.LightBlue);

            //batch.DrawString(Game1._font_Main, "GemBank.Count = " + _gemBank.Count, new Vector2(AbsX + 10, AbsY - 96), Color.White);

            //if (null != LastGemExplose)
            //    batch.DrawString(Game1._font_Main, "LastGemExplose = " + LastGemExplose.IsDead, new Vector2(AbsX + 10, AbsY - 72), Color.White);

            batch.DrawString(Game1._font_Main, "Hero Energy = " + _hero.Energy, new Vector2(AbsX + 10, AbsY - 26), Color.MonoGameOrange);

            //batch.DrawString(Game1._font_Main, "Hero Mode = " + _hero.Mode + " : " + _hero.GetModePoint(_hero.Mode), new Vector2(AbsX + 10, AbsY - 24), Color.GreenYellow);

            // Draw Nb Gem by colors !
            //for (int i = 0; i < _nbGem.Count; i++)
            //{
            //    batch.DrawString(Game1._font_Main, 
            //        "Color "+ i +" = " + _nbGem[i], 
            //        new Vector2(AbsX + 10, AbsY - 480 + i * 20), 
            //        Gem.Colors[i]);
            //}


            if (IsGameOver)
            {
                Draw.FillRectangle(batch, AbsRect, Color.Black * .8f);
                Draw.CenterStringXY(batch, Game1._font_Big, "YOU LOSE", AbsRect.X + AbsRect.Width / 2, AbsRect.Y + AbsRect.Height / 2, Color.Red);
            }

            return this;
        }

        public override Node RenderAdditive(SpriteBatch batch)
        {

            RenderAdditiveChilds(batch);

            return base.RenderAdditive(batch);
        }
    }
}
