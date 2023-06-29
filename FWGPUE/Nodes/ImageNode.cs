namespace FWGPUE.Nodes;

class ImageNode : Node2D
{
    public string Image;

    public override void Draw()
    {
        base.Draw();

        Renderer.PushSprite(Offset.X, Offset.Y, Z, Image, Scale.X, Scale.Y, 0, 0, Rotation);
    }
}
