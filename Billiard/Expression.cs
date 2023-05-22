using OpenTK.Mathematics;

namespace mechanics;

internal abstract class Expression
{
    protected Vector3 white;

    public Expression(Vector3 white)
    {
        this.white = white;
    }

    // Default color white
    public Expression()
    {
        white = new Vector3(255, 255, 255);
    }

    public abstract bool PointBelongToExpression(float x, float y);

    public bool PointBelongToExpression(Vector2 point)
    {
        return PointBelongToExpression(point.X, point.Y);
    }

    public Vector3? GetPointColor(int x, int y)
    {
        if (PointBelongToExpression(x, y)) return white;

        return null;
    }
}