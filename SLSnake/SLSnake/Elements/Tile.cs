﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;

namespace SLSnake.Elements
{
    public enum TileOrientations : int
    {
        Unknown = 0,
        LeftTop, Top, RightTop,
        Left, Right,
        LeftBottom, Bottom, RightBottom
    }

    [StructLayout(LayoutKind.Explicit, Size = 4, CharSet = CharSet.Ansi)]
    public struct Location
    {
        /// <summary>
        /// X + Y (4 bytes)
        /// </summary>
        [FieldOffset(0)]
        public uint XY;

        /// <summary>
        /// X (2 bytes)
        /// </summary>
        [FieldOffset(0)]
        public short X;

        /// <summary>
        /// Y (2 bytes)
        /// </summary>
        [FieldOffset(2)]
        public short Y;

        #region override operator
        public static implicit operator Location(uint i)
        {
            return new Location { XY = i };
        }
        public static bool operator ==(Location a, Location b)
        {
            return a.XY.Equals(b.XY);
        }
        public static bool operator !=(Location a, Location b)
        {
            return !a.XY.Equals(b.XY);
        }
        public override int GetHashCode()
        {
            return (int)this.XY;
        }
        public override bool Equals(object obj)
        {
            return this.XY.Equals(((Location)obj).XY);
        }
        #endregion
    }

    /// <summary>
    /// 1x1 Base Tile
    /// </summary>
    public class Tile : Canvas
    {
        #region FrameAnimation

        protected Storyboard _FrameAnim_StoryBoard = new Storyboard();
        protected DoubleAnimationUsingKeyFrames _FrameAnim_KeyFrames = new DoubleAnimationUsingKeyFrames() { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever };
        protected DiscreteDoubleKeyFrame _FrameAnim_KeyFrame_1 = new DiscreteDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(250d)) };
        protected DiscreteDoubleKeyFrame _FrameAnim_KeyFrame_2 = new DiscreteDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(500d)) };
        protected DiscreteDoubleKeyFrame _FrameAnim_KeyFrame_3 = new DiscreteDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(750d)) };
        protected DiscreteDoubleKeyFrame _FrameAnim_KeyFrame_4 = new DiscreteDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1000d)) };

        protected virtual ImageBrush _FrameAnim_ImageBrush { get; set; }

        protected TranslateTransform _FrameAnim_TranslateTransform = new TranslateTransform() { X = 0, Y = 0 };
        protected RectangleGeometry _FrameAnim_RectangleGeometry = new RectangleGeometry() { Rect = new Rect(0, 0, 24, 24) };

        #endregion

        #region SmoothMove

        protected Storyboard _SmoothMove_StoryBoard = new Storyboard();

        protected DoubleAnimationUsingKeyFrames _SmoothMove_X_KeyFrames = new DoubleAnimationUsingKeyFrames();
        protected SplineDoubleKeyFrame _SmoothMove_X_KeyFrame_1 = new SplineDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0)) };
        protected SplineDoubleKeyFrame _SmoothMove_X_KeyFrame_2 = new SplineDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(500)) };

        protected DoubleAnimationUsingKeyFrames _SmoothMove_Y_KeyFrames = new DoubleAnimationUsingKeyFrames();
        protected SplineDoubleKeyFrame _SmoothMove_Y_KeyFrame_1 = new SplineDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0)) };
        protected SplineDoubleKeyFrame _SmoothMove_Y_KeyFrame_2 = new SplineDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(500)) };

        #endregion

        #region Constructor

        /// <summary>
        /// controls container
        /// </summary>
        protected Canvas _canvas = null;

        public Tile(Canvas container)
            : base()
        {
            // store, locate
            _canvas = container;
            container.Children.Add(this);

            // set width, height, clip area
            this.Clip = _FrameAnim_RectangleGeometry;
            this.Height = 24;
            this.Width = 24;

            // init FrameAnimation StoryBoard

            this.Resources.Add("FrameAnimation", _FrameAnim_StoryBoard);
            _FrameAnim_StoryBoard.Children.Add(_FrameAnim_KeyFrames);

            _FrameAnim_KeyFrames.KeyFrames.Add(_FrameAnim_KeyFrame_1);
            _FrameAnim_KeyFrames.KeyFrames.Add(_FrameAnim_KeyFrame_2);
            _FrameAnim_KeyFrames.KeyFrames.Add(_FrameAnim_KeyFrame_3);
            _FrameAnim_KeyFrames.KeyFrames.Add(_FrameAnim_KeyFrame_4);

            Storyboard.SetTarget(_FrameAnim_KeyFrames, _FrameAnim_TranslateTransform);
            Storyboard.SetTargetProperty(_FrameAnim_KeyFrames, new PropertyPath("X"));

            // init FrameAnimation brush

            this.Background = _FrameAnim_ImageBrush;
            _FrameAnim_ImageBrush.Transform = _FrameAnim_TranslateTransform;

            // init SmoothMove StoryBoard

            this.Resources.Add("SmoothMove", _SmoothMove_StoryBoard);
            _SmoothMove_StoryBoard.Children.Add(_SmoothMove_X_KeyFrames);
            _SmoothMove_StoryBoard.Children.Add(_SmoothMove_Y_KeyFrames);

            _SmoothMove_X_KeyFrames.KeyFrames.Add(_SmoothMove_X_KeyFrame_1);
            _SmoothMove_X_KeyFrames.KeyFrames.Add(_SmoothMove_X_KeyFrame_2);

            _SmoothMove_Y_KeyFrames.KeyFrames.Add(_SmoothMove_Y_KeyFrame_1);
            _SmoothMove_Y_KeyFrames.KeyFrames.Add(_SmoothMove_Y_KeyFrame_2);

            Storyboard.SetTarget(_SmoothMove_X_KeyFrames, this);
            Storyboard.SetTargetProperty(_SmoothMove_X_KeyFrames, new PropertyPath("(Canvas.Left)"));

            Storyboard.SetTarget(_SmoothMove_Y_KeyFrames, this);
            Storyboard.SetTargetProperty(_SmoothMove_Y_KeyFrames, new PropertyPath("(Canvas.Top)"));

            _SmoothMove_StoryBoard.Completed += new EventHandler(_SmoothMove_StoryBoard_Completed);

            this.Orientation = TileOrientations.Bottom;
        }

        public Tile(Canvas container, double speedRatio)
            : this(container)
        {
            _SmoothMove_StoryBoard.SpeedRatio = speedRatio;
            _FrameAnim_StoryBoard.SpeedRatio = speedRatio;
        }

        protected bool _IsSmoothMoving = false;

        void _SmoothMove_StoryBoard_Completed(object sender, EventArgs e)
        {
            _IsSmoothMoving = false;
        }

        #endregion

        #region Orientation

        protected TileOrientations _orientation;

        /// <summary>
        /// tile's orientation (default: bottom)
        /// </summary>
        public virtual TileOrientations Orientation
        {
            get
            {
                return _orientation;
            }
            set
            {
                if (value == _orientation) return;
                _orientation = value;
                _FrameAnim_StoryBoard.Stop();
                switch (value)
                {
                    case TileOrientations.Bottom:
                        _FrameAnim_TranslateTransform.X = 0;
                        _FrameAnim_KeyFrame_1.Value = _FrameAnim_KeyFrame_2.Value = 0 - 24;
                        _FrameAnim_KeyFrame_3.Value = _FrameAnim_KeyFrame_4.Value = 0 - 48;
                        break;
                    case TileOrientations.LeftBottom:
                        _FrameAnim_TranslateTransform.X = -72;
                        _FrameAnim_KeyFrame_1.Value = _FrameAnim_KeyFrame_2.Value = -72 - 24;
                        _FrameAnim_KeyFrame_3.Value = _FrameAnim_KeyFrame_4.Value = -72 - 48;
                        break;
                    case TileOrientations.Left:
                        _FrameAnim_TranslateTransform.X = -144;
                        _FrameAnim_KeyFrame_1.Value = _FrameAnim_KeyFrame_2.Value = -144 - 24;
                        _FrameAnim_KeyFrame_3.Value = _FrameAnim_KeyFrame_4.Value = -144 - 48;
                        break;
                    case TileOrientations.LeftTop:
                        _FrameAnim_TranslateTransform.X = -216;
                        _FrameAnim_KeyFrame_1.Value = _FrameAnim_KeyFrame_2.Value = -216 - 24;
                        _FrameAnim_KeyFrame_3.Value = _FrameAnim_KeyFrame_4.Value = -216 - 48;
                        break;
                    case TileOrientations.Top:
                        _FrameAnim_TranslateTransform.X = -288;
                        _FrameAnim_KeyFrame_1.Value = _FrameAnim_KeyFrame_2.Value = -288 - 24;
                        _FrameAnim_KeyFrame_3.Value = _FrameAnim_KeyFrame_4.Value = -288 - 48;
                        break;
                    case TileOrientations.RightTop:
                        _FrameAnim_TranslateTransform.X = -360;
                        _FrameAnim_KeyFrame_1.Value = _FrameAnim_KeyFrame_2.Value = -360 - 24;
                        _FrameAnim_KeyFrame_3.Value = _FrameAnim_KeyFrame_4.Value = -360 - 48;
                        break;
                    case TileOrientations.Right:
                        _FrameAnim_TranslateTransform.X = -432;
                        _FrameAnim_KeyFrame_1.Value = _FrameAnim_KeyFrame_2.Value = -432 - 24;
                        _FrameAnim_KeyFrame_3.Value = _FrameAnim_KeyFrame_4.Value = -432 - 48;
                        break;
                    case TileOrientations.RightBottom:
                        _FrameAnim_TranslateTransform.X = -504;
                        _FrameAnim_KeyFrame_1.Value = _FrameAnim_KeyFrame_2.Value = -504 - 24;
                        _FrameAnim_KeyFrame_3.Value = _FrameAnim_KeyFrame_4.Value = -504 - 48;
                        break;
                }
                _FrameAnim_StoryBoard.Begin();
            }
        }

        #endregion

        #region Actions

        public short X
        {
            get { return _location.X; }
            set
            {
                _location.X = value;
                Canvas.SetLeft(this, _location.X * 24);
            }
        }
        public short Y
        {
            get { return _location.Y; }
            set
            {
                _location.Y = value;
                Canvas.SetTop(this, _location.Y * 24);
                Canvas.SetZIndex(this, value);
            }
        }

        public virtual int Z { get; set; }

        protected Location _location;
        public Location Location
        {
            get
            {
                return _location;
            }
            set
            {
                _location = value;
                Canvas.SetLeft(this, _location.X * 24);
                Canvas.SetTop(this, _location.Y * 24);
                Canvas.SetZIndex(this, _location.Y);
            }
        }



        /// <summary>
        /// if has been moved to target location, return true; else go(1) and return false
        /// </summary>
        public bool MoveTo(short x, short y)
        {
            if (_IsSmoothMoving) return false;
            if (x == _location.X && y == _location.Y) return true;

            //_SmoothMove_StoryBoard.Stop();
            if (x > _location.X && y > _location.Y)
            {
                _SmoothMove_X_KeyFrame_1.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_1.Value = _location.Y * 24;
                _location.X++; _location.Y++; Canvas.SetZIndex(this, _location.Y + Z);
                _SmoothMove_X_KeyFrame_2.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_2.Value = _location.Y * 24;
                this.Orientation = TileOrientations.RightBottom;
            }
            else if (x < _location.X && y < _location.Y)
            {
                _SmoothMove_X_KeyFrame_1.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_1.Value = _location.Y * 24;
                _location.X--; _location.Y--; Canvas.SetZIndex(this, _location.Y + Z);
                _SmoothMove_X_KeyFrame_2.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_2.Value = _location.Y * 24;
                this.Orientation = TileOrientations.LeftTop;
            }
            else if (x > _location.X && y < _location.Y)
            {
                _SmoothMove_X_KeyFrame_1.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_1.Value = _location.Y * 24;
                _location.X++; _location.Y--; Canvas.SetZIndex(this, _location.Y + Z);
                _SmoothMove_X_KeyFrame_2.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_2.Value = _location.Y * 24;
                this.Orientation = TileOrientations.RightTop;
            }
            else if (x < _location.X && y > _location.Y)
            {
                _SmoothMove_X_KeyFrame_1.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_1.Value = _location.Y * 24;
                _location.X--; _location.Y++; Canvas.SetZIndex(this, _location.Y + Z);
                _SmoothMove_X_KeyFrame_2.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_2.Value = _location.Y * 24;
                this.Orientation = TileOrientations.LeftBottom;
            }
            else if (x < _location.X)
            {
                _SmoothMove_X_KeyFrame_1.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_1.Value = _location.Y * 24;
                _location.X--;
                _SmoothMove_X_KeyFrame_2.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_2.Value = _location.Y * 24;
                this.Orientation = TileOrientations.Left;
            }
            else if (x > _location.X)
            {
                _SmoothMove_X_KeyFrame_1.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_1.Value = _location.Y * 24;
                _location.X++;
                _SmoothMove_X_KeyFrame_2.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_2.Value = _location.Y * 24;
                this.Orientation = TileOrientations.Right;
            }
            else if (y < _location.Y)
            {
                _SmoothMove_X_KeyFrame_1.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_1.Value = _location.Y * 24;
                _location.Y--; Canvas.SetZIndex(this, _location.Y + Z);
                _SmoothMove_X_KeyFrame_2.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_2.Value = _location.Y * 24;
                this.Orientation = TileOrientations.Top;
            }
            else if (y > _location.Y)
            {
                _SmoothMove_X_KeyFrame_1.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_1.Value = _location.Y * 24;
                _location.Y++; Canvas.SetZIndex(this, _location.Y + Z);
                _SmoothMove_X_KeyFrame_2.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_2.Value = _location.Y * 24;
                this.Orientation = TileOrientations.Bottom;
            }

            _IsSmoothMoving = true;
            _SmoothMove_StoryBoard.Begin();
            return false;
        }


        /// <summary>
        /// forward 1 cell
        /// </summary>
        public virtual void Go()
        {
            if (_IsSmoothMoving) return;

            if (_orientation == TileOrientations.RightBottom)
            {
                _SmoothMove_X_KeyFrame_1.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_1.Value = _location.Y * 24;
                _location.X++; _location.Y++; Canvas.SetZIndex(this, _location.Y + Z);
                _SmoothMove_X_KeyFrame_2.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_2.Value = _location.Y * 24;
            }
            else if (this.Orientation == TileOrientations.LeftTop)
            {
                _SmoothMove_X_KeyFrame_1.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_1.Value = _location.Y * 24;
                _location.X--; _location.Y--; Canvas.SetZIndex(this, _location.Y + Z);
                _SmoothMove_X_KeyFrame_2.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_2.Value = _location.Y * 24;
            }
            else if (this.Orientation == TileOrientations.RightTop)
            {
                _SmoothMove_X_KeyFrame_1.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_1.Value = _location.Y * 24;
                _location.X++; _location.Y--; Canvas.SetZIndex(this, _location.Y + Z);
                _SmoothMove_X_KeyFrame_2.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_2.Value = _location.Y * 24;

            }
            else if (this.Orientation == TileOrientations.LeftBottom)
            {
                _SmoothMove_X_KeyFrame_1.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_1.Value = _location.Y * 24;
                _location.X--; _location.Y++; Canvas.SetZIndex(this, _location.Y + Z);
                _SmoothMove_X_KeyFrame_2.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_2.Value = _location.Y * 24;

            }
            else if (this.Orientation == TileOrientations.Left)
            {
                _SmoothMove_X_KeyFrame_1.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_1.Value = _location.Y * 24;
                _location.X--;
                _SmoothMove_X_KeyFrame_2.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_2.Value = _location.Y * 24;

            }
            else if (this.Orientation == TileOrientations.Right)
            {
                _SmoothMove_X_KeyFrame_1.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_1.Value = _location.Y * 24;
                _location.X++;
                _SmoothMove_X_KeyFrame_2.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_2.Value = _location.Y * 24;

            }
            else if (this.Orientation == TileOrientations.Top)
            {
                _SmoothMove_X_KeyFrame_1.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_1.Value = _location.Y * 24;
                _location.Y--; Canvas.SetZIndex(this, _location.Y + Z);
                _SmoothMove_X_KeyFrame_2.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_2.Value = _location.Y * 24;

            }
            else if (this.Orientation == TileOrientations.Bottom)
            {
                _SmoothMove_X_KeyFrame_1.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_1.Value = _location.Y * 24;
                _location.Y++; Canvas.SetZIndex(this, _location.Y + Z);
                _SmoothMove_X_KeyFrame_2.Value = _location.X * 24;
                _SmoothMove_Y_KeyFrame_2.Value = _location.Y * 24;

            }
            _IsSmoothMoving = true;
            _SmoothMove_StoryBoard.Begin();
        }


        public Location GetNextLocation()
        {
            switch (this.Orientation)
            {
                case TileOrientations.Bottom:
                    return new Location { X = (short)(this.Location.X + 0), Y = (short)(this.Location.Y + 1) };
                    break;
                case TileOrientations.Left:
                    return new Location { X = (short)(this.Location.X - 1), Y = (short)(this.Location.Y + 0) };
                    break;
                case TileOrientations.LeftBottom:
                    return new Location { X = (short)(this.Location.X - 1), Y = (short)(this.Location.Y + 1) };
                    break;
                case TileOrientations.LeftTop:
                    return new Location { X = (short)(this.Location.X - 1), Y = (short)(this.Location.Y - 1) };
                    break;
                case TileOrientations.Right:
                    return new Location { X = (short)(this.Location.X + 1), Y = (short)(this.Location.Y + 0) };
                    break;
                case TileOrientations.RightBottom:
                    return new Location { X = (short)(this.Location.X + 1), Y = (short)(this.Location.Y + 1) };
                    break;
                case TileOrientations.RightTop:
                    return new Location { X = (short)(this.Location.X + 1), Y = (short)(this.Location.Y - 1) };
                    break;
                case TileOrientations.Top:
                    return new Location { X = (short)(this.Location.X + 0), Y = (short)(this.Location.Y - 1) };
                    break;
                default:
                    return this.Location;
            }
        }

        #endregion

        #region Process

        public virtual void Process()
        {
            //todo
        }

        #endregion
    }
}
