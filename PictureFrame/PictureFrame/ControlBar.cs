using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PictureFrame
{
    public class ControlBar: System.Windows.Forms.Control
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public ControlBar() 
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); //控件忽略窗口消息 WM_ERASEBKGND 以减少闪烁
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true); //首先绘制到缓冲区而不是直接绘制到屏幕，这可以减少闪烁
            CreateControl();
        }

        #region Properties

        private Color _barColor = Color.FromArgb(200, 200, 200);
        /// <summary>
        /// 背景条颜色，默认浅灰色
        /// </summary>
        public Color _BarColor
        {
            get { return _barColor; }
            set
            {
                _barColor = value;
                Invalidate();
            }
        }

        private Color _sliderColor = Color.FromArgb(0, 0, 0);
        /// <summary>
        /// 滑块颜色，默认黑色
        /// </summary>
        public Color _SliderColor
        {
            get { return _sliderColor; }
            set
            {
                _sliderColor = value;
                Invalidate();
            }
        }

        private int _size = 2;
        /// <summary>
        /// 进度条高度
        /// </summary>
        public int _Size
        {
            get { return _size; }
            set
            {
                _size = value;
                if (_size < 1) { _size = 1; }
                Size = new Size(Width, _size); 
            }
        }

        private float _dotSize = 2.0f;
        /// <summary>
        /// 进度条原点直径/进度条高度
        /// </summary>
        private float _DotSize
        {
            get { return _dotSize; }
            set
            {
                _dotSize= value;
                if (_dotSize < 1) { _dotSize = 1; }
                
            }
        }

        private bool _isRound = true; //风格是否圆角
        /// <summary>
        /// 是否是圆角
        /// </summary>
        public bool IsRound
        {
            get { return _isRound; }
            set
            {
                _isRound = value;
                Invalidate();
            }
        }

        private int _radius = 2;
        /// <summary>
        /// 滑块半径
        /// </summary>
        public int _Radius
        {
            get { return _radius; }
            set
            {
                _radius = value;
            }
        }


        private int _min = 0;
        /// <summary>
        /// 进度条最小值
        /// </summary>
        public int _Min
        {
            get { return _min; }
            set
            {
                _min = value;
                if (_min >= _max) { _min = _max - 1; }
                if (_min < 0) { _min = 0; }
                Invalidate();
            }
        }

        private int _max = 10;
        /// <summary>
        /// 进度条最大值
        /// </summary>
        public int _Max
        {
            get { return _max; }
            set
            {
                _max = value;
                if (_max <= _min) { _max = _min + 1; }
                Invalidate();
            }
        }

        private int _value = 3;
        /// <summary>
        /// 进度条当前值
        /// </summary>
        public int _Value
        {
            get { return _value; }
            set
            {
                _value = value;
                if (_value < _min) { _value = _min; }
                if (_value > _max) { _value = _max; }
                Invalidate(); //重绘控件
                //触发事件ValueChanged
                ValueChanged?.Invoke(this, new ControlBarEventArgs(_value));
            }
        }
        #endregion

        #region Events
        //注册一个委托
        public delegate void ValueChangedEventHandler(object sender, ControlBarEventArgs e);
        //为本控件注册一个事件，该事件在_value值被改动时发送
        public event ValueChangedEventHandler ValueChanged;
        #endregion

        #region Entity
        private MouseStatus mouseStatus = MouseStatus.Leave; //鼠标状态
        private PointF mousePoint = Point.Empty; //坐标(在ValueUpdate函数中代表光标坐标，在PointUpdate函数中代表滑条右侧坐标)
        #endregion


        /// <summary>
        /// 控件重绘
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            PointUpdate(); //当滑条不处于拖动情况下，根据_value值获取滑条右端应该在的坐标值
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

            //创建画笔
            Pen penBack = new Pen(_barColor, Height);
            Pen penFore = new Pen(_sliderColor, Height);
            Pen penDot = new Pen(_sliderColor, Height * _dotSize);

            float CapWidth = 0; float CapHalfWidth = 0;
            //如果圆角风格，获取半圆的半径和直径
            if (_isRound)
            {
                CapWidth = Size.Height;
                CapHalfWidth = Size.Height / 2.0f;
                //画笔样式更改
                penBack.StartCap = LineCap.Round;
                penFore.StartCap = LineCap.Round;
                penBack.EndCap = LineCap.Round;
                penFore.EndCap = LineCap.Round;
            }

            float PointValue = 0;
            //绘制背景条
            e.Graphics.DrawLine(penBack, CapHalfWidth, Height / 2f, Width - CapHalfWidth, Height / 2f);
            /*
             *如果不处于滑动条拖动状态，PointValue代表滑动条右端应该在的坐标
             *如果处于滑动条拖动状态，更新滑动条
             */
            PointValue = mousePoint.X;
            //绘制滑动条
            e.Graphics.DrawLine(penFore, CapHalfWidth, Height / 2f, PointValue, Height / 2f);
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            mouseStatus = MouseStatus.Down;
            mousePoint = e.Location;
            ValueUpdate(); //更新进度条当前值
            Invalidate();
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            mouseStatus = MouseStatus.Up;
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (mouseStatus == MouseStatus.Down)
            {
                mousePoint = e.Location; //跟踪光标位置，此时e.Location相对于本控件，而非整个屏幕
                ValueUpdate(); //更新进度条当前值
                Invalidate();
            }
        }
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            mouseStatus = MouseStatus.Enter;
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            mouseStatus = MouseStatus.Leave;
        }

        /// <summary>
        /// 根据光标位置更新进度条当前值
        /// </summary>
        private void ValueUpdate()
        {
            float CapWidth = 0; float CapHalfWidth = 0;
            //如果圆角风格，获取半圆的半径和直径
            if (_isRound)
            {
                CapWidth = Size.Height;
                CapHalfWidth = Size.Height / 2.0f;
            }
            float ratio = Convert.ToSingle(mousePoint.X - CapHalfWidth) / (Width - CapHalfWidth);
            _value = Convert.ToInt32(_min + ratio * (_max - _min));
            if (_value < _min)
            {
                _value = _min;
            }
            if (_value > _max)
            {
                _value = _max;
            }
            //触发事件ValueChanged
            ValueChanged?.Invoke(this, new ControlBarEventArgs(_value));
        }

        /// <summary>
        /// 根据进度条当前值获取滑条右端应该到达的坐标
        /// </summary>
        public void PointUpdate()
        {
            float CapWidth = 0; float CapHalfWidth = 0;
            //如果圆角风格，获取半圆的半径和直径
            if (_isRound)
            {
                CapWidth = Size.Height;
                CapHalfWidth = Size.Height / 2.0f;
            }
            float ratio = Convert.ToSingle(_value - _min) / (_max - _min);
            float PointMove = CapHalfWidth + (Width - CapWidth) * ratio;
            mousePoint = new PointF(PointMove, CapHalfWidth);
        }
    }
}
