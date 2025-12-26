namespace MiniPhotoshop.Services.ImageEditor
{
    internal sealed partial class ImageEditor
    {
        public void ResetWorkspace()
        {
            ClearArithmeticSnapshot();
            State.Reset();
        }
    }
}
