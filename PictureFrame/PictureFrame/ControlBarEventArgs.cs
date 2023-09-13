namespace PictureFrame
{
    public class ControlBarEventArgs:EventArgs
    {
        public ControlBarEventArgs(object value)
        {
            Value = value;
        }

        public object Value { get; set; }
    }
}
